
Prequisites
===========
pskql requires Powershell 7 or above and dotnet 9.0  

Setup
=====

First import the pskql module using:

    import-module $env:LOCALAPPDATA/programs/pskql/pskql.dll

Alternatively open a shell  using the "pskql Powershell" option from the Start-menu.

Confidence check
================

After importing the module try a command like:

    get-childitem | edit-kql "project Name,Length | order by Length | take 10 "

If you're running Windows Terminal Preview or a modern terminal that supports
sixels try:

    get-childitem | edit-kql "project Name,Length | order by Length | take 10 | render barchart"



