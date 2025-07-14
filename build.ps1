

[CmdletBinding()]
param(

    [Parameter(Mandatory = $true, HelpMessage = "version for package")]
    [string] $version,
    [Parameter(HelpMessage = "api key (if publising nuget packages)")]
    [string] $api,

    [Parameter(HelpMessage = "test only")]
    [switch] $skipBuild,
    [Parameter(HelpMessage = "do not create upload zip")]
    [switch] $skipZip

)
$versionString = $version.replace('.', '-')
$uploadsFolder = "uploads-$versionstring"
$packages = @("KustoLoco.Core", "FileFormats", "Rendering", "ScottPlotRendering", "SixelSupport")

if (-not $skipBuild) {
    #force rebuild
    Get-ChildItem -r bin | Remove-Item -r
    Get-ChildItem -r obj | Remove-Item -r

    dotnet build -c Release
    Get-ChildItem -r *.nupkg | Remove-Item -r
    
    #make nuget packages
    
    foreach ($package in $packages) {
        dotnet pack -p:PackageVersion=$version .\libraries\$package\$package.csproj
    }
   
    dotnet pack   -p:PackageVersion=$version .\sourceGeneration\SourceGenDependencies\SourceGenDependencies.csproj

    #build application exes

    dotnet publish  /p:Version="$version" /p:InformationalVersion="$version"  .\applications\lokql\lokql.csproj -r win-x64 -p:PublishSingleFile=true --self-contained false --output .\publish\lokql -c:Release -p:PackageVersion=$version
    dotnet publish  /p:Version="$version" /p:InformationalVersion="$version"  .\applications\lokql\lokql.csproj -r linux-x64 -p:PublishSingleFile=true --self-contained false --output .\publish\lokql-linux -c:Release -p:PackageVersion=$version

    dotnet publish  /p:Version="$version" /p:InformationalVersion="$version" .\applications\lokqldx\lokqldx.csproj -r win-x64 -p:PublishSingleFile=true --self-contained false --output .\publish\lokqldx  -p:PackageVersion=$version
    dotnet publish  /p:Version="$version" /p:InformationalVersion="$version" .\applications\lokqldx\lokqldx.csproj -r linux-x64 -p:PublishSingleFile=true --self-contained false --output .\publish\lokqldx-linux  -p:PackageVersion=$version
    dotnet publish  /p:Version="$version" /p:InformationalVersion="$version" .\applications\lokqldx\lokqldx.csproj -r osx-x64 -p:PublishSingleFile=true --self-contained false --output .\publish\lokqldx-macos  -p:PackageVersion=$version
  
  
    dotnet publish  /p:Version="$version" /p:InformationalVersion="$version" .\applications\pskql\pskql.csproj -r win-x64 --self-contained false --output .\publish\pskql  -p:PackageVersion=$version
    dotnet publish  /p:Version="$version" /p:InformationalVersion="$version" .\applications\pskql\pskql.csproj -r linux-x64 --self-contained false --output .\publish\pskql-linux  -p:PackageVersion=$version
    
    #remove pdbs
    get-ChildItem -recurse -path .\publish\ -include *.pdb | remove-item

    #clean up pskql....
    get-ChildItem -recurse -path .\publish\pskql -include Microsoft.*.dll | remove-item
    get-ChildItem -recurse -path .\publish\pskql -include System.*.dll | remove-item
    #clean up pskql linux....
    get-ChildItem -recurse -path .\publish\pskql-linux -include Microsoft.*.dll | remove-item
    get-ChildItem -recurse -path .\publish\pskql-linux -include System.*.dll | remove-item

    #copy tutorials to publish folder
    New-Item -ItemType Directory -Path .\publish\tutorials -Force
    copy-item .\docs\tutorials\* .\publish\tutorials   
}

if (-not ($api -like '') ) {
    foreach ($package in $packages) {
        dotnet nuget push libraries\$package\bin\Release\$package.$($version).nupkg --api-key $api --source https://api.nuget.org/v3/index.json
    }
    dotnet nuget push sourceGeneration\SourceGenDependencies\bin\Release\KustoLoco.SourceGeneration.Attributes.$($version).nupkg --api-key $api --source https://api.nuget.org/v3/index.json
}

get-ChildItem -r *.nupkg | % FullName

new-item -ItemType directory -name "$uploadsFolder"


# Build the installers for windows
$installers = @("lokql", "lokqldx", "pskql")
foreach ($installer in $installers) { 
    iscc.exe   /DMyAppVersion="$version" /DSuffix="$versionString" /DOutputDir="$uploadsFolder" .\setup\$installer.iss
}

# copy other platforms as individual archives
if (-not $skipZip) {

    $folders = @("lokql-linux", "lokqldx-linux", "lokqldx-macos", "pskql-linux", "tutorials")  
    foreach ($folder in $folders) {
        $compress = @{
            Path             = ".\publish\$folder"
            CompressionLevel = "Fastest"
            DestinationPath  = "$uploadsFolder\$folder-$versionString.zip"
        }
        Compress-Archive @compress
    }
}