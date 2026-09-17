<#
.SYNOPSIS
  Rebuilds the embedded default geo database (dbip-country-lite.csv.gz) from its two canonical public sources.

.DESCRIPTION
  KustoLoco.Geo embeds a default IP->country database so geo_info_from_ip_address() resolves with one line and
  no data file. That artifact was previously produced by hand, which is how it silently drifted: it shipped only
  DB-IP's IPv4 half, so DbIpGeoProvider.Default returned null for EVERY IPv6 address even though the provider
  fully supports IPv6 (RangeTable<UInt128>). This script exists so the artifact is reproducible and the drift
  cannot recur unnoticed.

  Output layout is DbIpLayout.CountryCentroid: startIp,endIp,countryCode,latitude,longitude
  - ranges come from DB-IP IP-to-Country Lite (CC-BY-4.0) and cover BOTH IPv4 and IPv6
  - per-country centroids come from Google's canonical dspl country list (CC-BY-4.0)
  - a DB-IP row whose country has no canonical centroid is skipped (it cannot be placed); a country whose
    centroid is published as blank (e.g. UM) is KEPT with empty coordinates, which the provider reads as null.

  Both sources are CC-BY-4.0 and require attribution; the emitted header carries it, and NOTICE must keep it.

.PARAMETER Month
  DB-IP release to fetch, as yyyy-MM. Defaults to the current month.

.EXAMPLE
  ./rebuild-geo-database.ps1
  ./rebuild-geo-database.ps1 -Month 2026-09
#>
[CmdletBinding()]
param(
    [string]$Month = (Get-Date -Format 'yyyy-MM')
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$out = Join-Path $here 'dbip-country-lite.csv.gz'

$dbipUrl = "https://download.db-ip.com/free/dbip-country-lite-$Month.csv.gz"
$dsplUrl = 'https://raw.githubusercontent.com/google/dspl/master/samples/google/canonical/countries.csv'

$tmp = Join-Path ([System.IO.Path]::GetTempPath()) "kustoloco-geo-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Force -Path $tmp | Out-Null

try {
    Write-Host "Fetching Google canonical country centroids..."
    $dsplPath = Join-Path $tmp 'countries.csv'
    Invoke-WebRequest -Uri $dsplUrl -OutFile $dsplPath -UseBasicParsing -TimeoutSec 120

    # country,latitude,longitude,name — the name field may be quoted and contain commas, so take the first
    # three fields only. A blank latitude/longitude is legitimate (published that way for some territories).
    $centroid = @{}
    foreach ($line in [System.IO.File]::ReadAllLines($dsplPath) | Select-Object -Skip 1) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $f = $line.Split(',')
        if ($f.Length -lt 3 -or [string]::IsNullOrWhiteSpace($f[0])) { continue }
        $centroid[$f[0]] = @($f[1], $f[2])
    }
    Write-Host "  centroids: $($centroid.Count)"

    Write-Host "Fetching DB-IP IP-to-Country Lite ($Month)..."
    $dbipGz = Join-Path $tmp 'dbip.csv.gz'
    Invoke-WebRequest -Uri $dbipUrl -OutFile $dbipGz -UseBasicParsing -TimeoutSec 300

    # Stream gz -> join -> gz, so neither the ~717k-row input nor the output is held in memory at once.
    $bodyPath = Join-Path $tmp 'body.csv'
    $inGz = [System.IO.Compression.GZipStream]::new(
        [System.IO.File]::OpenRead($dbipGz), [System.IO.Compression.CompressionMode]::Decompress)
    $reader = [System.IO.StreamReader]::new($inGz)
    $writer = [System.IO.StreamWriter]::new($bodyPath, $false, [System.Text.UTF8Encoding]::new($false))
    # LF, not the platform default: the emitted database is a committed binary artifact, so its bytes must not
    # depend on which OS regenerated it (a CRLF rebuild would show as a whole-file diff on every line).
    $writer.NewLine = "`n"
    $v4 = 0; $v6 = 0; $skipped = 0
    try {
        while ($null -ne ($line = $reader.ReadLine())) {
            if ([string]::IsNullOrWhiteSpace($line)) { continue }
            $f = $line.Split(',')
            if ($f.Length -lt 3) { continue }
            $cc = $f[2]
            if (-not $centroid.ContainsKey($cc)) { $skipped++; continue }
            $c = $centroid[$cc]
            $writer.WriteLine("$($f[0]),$($f[1]),$cc,$($c[0]),$($c[1])")
            if ($f[0].Contains(':')) { $v6++ } else { $v4++ }
        }
    }
    finally { $writer.Dispose(); $reader.Dispose(); $inGz.Dispose() }

    # Fail LOUD rather than emit a degenerate database: a source that changed shape or rate-limited would
    # otherwise silently produce a geo file missing a whole address family, which is exactly the defect this
    # script was written to prevent.
    if ($v4 -eq 0) { throw "DB-IP feed produced 0 IPv4 ranges — refusing to write. Existing database left intact." }
    if ($v6 -eq 0) { throw "DB-IP feed produced 0 IPv6 ranges — refusing to write. Existing database left intact." }
    Write-Host "  ranges: ipv4=$v4 ipv6=$v6 (skipped, no canonical centroid: $skipped)"

    $header = @(
        '# KustoLoco.Geo — default IP->geo database for geo_info_from_ip_address().'
        '# Source: DB-IP IP-to-Country Lite (CC-BY-4.0, https://db-ip.com) joined with Google canonical'
        '# country centroids (CC-BY-4.0, https://github.com/google/dspl). Attribution: "IP Geolocation by DB-IP".'
        '# Layout: DbIpLayout.CountryCentroid — startIp,endIp,countryCode,latitude,longitude'
        '# (IPv4 and IPv6 inclusive ranges; coordinates are per-country centroids, blank where none is published).'
        '# Regenerate: libraries/KustoLoco.Geo/rebuild-geo-database.ps1'
        'startIp,endIp,countryCode,latitude,longitude'
    )

    $outStream = [System.IO.File]::Create($out)
    $gz = [System.IO.Compression.GZipStream]::new($outStream, [System.IO.Compression.CompressionLevel]::SmallestSize)
    $sw = [System.IO.StreamWriter]::new($gz, [System.Text.UTF8Encoding]::new($false))
    $sw.NewLine = "`n"   # see above: OS-independent bytes for a committed artifact
    try {
        foreach ($h in $header) { $sw.WriteLine($h) }
        $br = [System.IO.StreamReader]::new($bodyPath)
        try { while ($null -ne ($l = $br.ReadLine())) { $sw.WriteLine($l) } } finally { $br.Dispose() }
    }
    finally { $sw.Dispose(); $gz.Dispose(); $outStream.Dispose() }

    Write-Host "Wrote $out ($([math]::Round((Get-Item $out).Length / 1MB, 2)) MB)"
}
finally {
    Remove-Item -Recurse -Force $tmp -ErrorAction SilentlyContinue
}
