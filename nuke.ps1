
Get-ChildItem -r  .ncrunch* | remove-item -recurse -force 
Get-ChildItem -r -directory obj | remove-item -recurse -force 
Get-ChildItem -r -directory bin | remove-item -recurse -force 
get-ChildItem -r *.g.cs | remove-item