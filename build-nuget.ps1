

[CmdletBinding()]
param(

    [Parameter(Mandatory = $true, HelpMessage = "version for package")]
    [string] $version,
    [Parameter(HelpMessage = "api key (if publising)")]
    [string] $api,

    [Parameter(HelpMessage = "test only")]
    [switch] $skipBuild



)

if (-not $skipBuild) {
    #force rebuild
    Get-ChildItem -r bin | Remove-Item -r
    Get-ChildItem -r obj | Remove-Item -r

    dotnet build -c Release
    Get-ChildItem -r *.nupkg | Remove-Item -r

    dotnet pack   -p:PackageVersion=$version .\libraries\KustoLoco.Core\KustoLoco.Core.csproj
    dotnet pack   -p:PackageVersion=$version .\libraries\FileFormats\FileFormats.csproj
    dotnet pack   -p:PackageVersion=$version .\libraries\Rendering\Rendering.csproj

}



if (-not ($api -like '') ) {
    dotnet nuget push libraries\KustoLoco.Core\bin\Release\KustoLoco.Core.$($version).nupkg --api-key $api --source https://api.nuget.org/v3/index.json
    dotnet nuget push libraries\FileFormats\bin\Release\KustoLoco.FileFormats.$($version).nupkg --api-key $api --source https://api.nuget.org/v3/index.json
    dotnet nuget push libraries\Rendering\bin\Release\Rendering.$($version).nupkg --api-key $api --source https://api.nuget.org/v3/index.json
}