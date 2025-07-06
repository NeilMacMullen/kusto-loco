# simple script to test edit-kql since it can be a pain to 
# keep typing commands 

$version="9.9.9"

# build from clean and publish
Get-ChildItem -r bin | Remove-Item -r
Get-ChildItem -r obj | Remove-Item -r
dotnet publish  /p:Version="$version" /p:InformationalVersion="$version" .\applications\pskql\pskql.csproj -r win-x64 --self-contained false --output .\publish\pskql  -p:PackageVersion=$version
      
#remove pdbs
get-ChildItem -recurse -path .\publish\ -include *.pdb | remove-item
#clean up pskql....
get-ChildItem -recurse -path .\publish\pskql -include Microsoft.*.dll | remove-item
get-ChildItem -recurse -path .\publish\pskql -include System.*.dll | remove-item


import-module .\publish\pskql\pskql.dll


#run a couple of sample commands
Get-process  | Edit-Kql "summarize count() by bin(Handles,100) | render linechart"
Get-NetTCPConnection | edit-kql "where State == 'Established' | summarize count() by RemoteAddress" 