To use the edit-kql cmdlet you must first import the pskql module using:

    import-module $env:LOCALAPPDATA/programs/edit-kql/pskql.dll

After importing the module try a command like:

    get-childitem | edit-kql "project Name,Length | order by Length | take 10 "

If you're running Windows Terminal Preview or a modern terminal that supports
sixels try:

    get-childitem | edit-kql "project Name,Length | order by Length | take 10 | render barchart"



