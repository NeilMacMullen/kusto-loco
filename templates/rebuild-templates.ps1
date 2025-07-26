
function extrude($sbn,$path)
{
write-host "building '$sbn.cs' in '$path'"
$cmd ="textrude.exe render --models Csv!model=Types.csv --template $sbn.sbn --output output=..\libraries\KustoLoco.Core\Evaluation\BuiltIns\$path\$sbn.cs --verbose"
write-host "Executing '$cmd'"
invoke-expression  $cmd
}

extrude IndexFinder Aggregates
extrude ArgMinMaxFunctionImpl Aggregates
extrude TakeAny Aggregates
extrude TypeComparison ScalarFunctions
extrude MinMaxOfFunctionImpl ScalarFunctions
extrude CoalesceFunctionImpl ScalarFunctions
