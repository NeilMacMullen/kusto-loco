

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

if (-not $skipBuild) {
    #force rebuild
    Get-ChildItem -r bin | Remove-Item -r
    Get-ChildItem -r obj | Remove-Item -r

    dotnet build -c Release
    Get-ChildItem -r *.nupkg | Remove-Item -r
    
    #make nuget packages
    dotnet pack   -p:PackageVersion=$version .\libraries\KustoLoco.Core\KustoLoco.Core.csproj
    dotnet pack   -p:PackageVersion=$version .\libraries\FileFormats\FileFormats.csproj
    dotnet pack   -p:PackageVersion=$version .\libraries\Rendering\Rendering.csproj
    dotnet pack   -p:PackageVersion=$version .\sourceGeneration\SourceGenDependencies\SourceGenDependencies.csproj

    #build application exes

    dotnet publish  /p:Version="$version" /p:InformationalVersion="$version"  .\applications\lokql\lokql.csproj -r win-x64 -p:PublishSingleFile=true --self-contained false --output .\publish\lokql -c:Release -p:PackageVersion=$version
    dotnet publish  /p:Version="$version" /p:InformationalVersion="$version"  .\applications\lokql\lokql.csproj -r linux-x64 -p:PublishSingleFile=true --self-contained false --output .\publish\lokql-linux -c:Release -p:PackageVersion=$version

    dotnet publish  /p:Version="$version" /p:InformationalVersion="$version" .\applications\lokqldx\lokqldx.csproj -r win-x64 -p:PublishSingleFile=true --self-contained false --output .\publish\lokqldx  -p:PackageVersion=$version
    dotnet publish  /p:Version="$version" /p:InformationalVersion="$version" .\applications\pskql\pskql.csproj -r win-x64 --self-contained false --output .\publish\pskql  -p:PackageVersion=$version
    dotnet publish  /p:Version="$version" /p:InformationalVersion="$version" .\applications\pskql\pskql.csproj -r linux-x64 --self-contained false --output .\publish\pskql-linux  -p:PackageVersion=$version
    
    #remove pdbs
    get-ChildItem -recurse -path .\publish\ -include *.pdb | remove-item

    #clean up pskql....
     get-ChildItem -recurse -path .\publish\pskql -include Microsoft.*.dll | remove-item
     get-ChildItem -recurse -path .\publish\pskql -include System.*.dll | remove-item
    #clean up pskql....
     get-ChildItem -recurse -path .\publish\pskql-linux -include Microsoft.*.dll | remove-item
     get-ChildItem -recurse -path .\publish\pskql-linux -include System.*.dll | remove-item


}

if (-not ($api -like '') ) {
    dotnet nuget push libraries\KustoLoco.Core\bin\Release\KustoLoco.Core.$($version).nupkg --api-key $api --source https://api.nuget.org/v3/index.json
    dotnet nuget push libraries\FileFormats\bin\Release\KustoLoco.FileFormats.$($version).nupkg --api-key $api --source https://api.nuget.org/v3/index.json
    dotnet nuget push libraries\Rendering\bin\Release\KustoLoco.Rendering.$($version).nupkg --api-key $api --source https://api.nuget.org/v3/index.json
    dotnet nuget push sourceGeneration\SourceGenDependencies\bin\Release\KustoLoco.SourceGeneration.Attributes.$($version).nupkg --api-key $api --source https://api.nuget.org/v3/index.json
}

get-ChildItem -r *.nupkg | % FullName

if (-not $skipZip)
{
    $v=$version.replace('.','-')
    $compress = @{
    Path = ".\publish"
    CompressionLevel = "Fastest"
    DestinationPath = "uploads\kustoloco-$v.zip"
}
Compress-Archive @compress
}