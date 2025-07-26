
textrude.exe render --models Csv!model=Types.csv --template IndexFinder.sbn --output output=..\libraries\KustoLoco.Core\Evaluation\BuiltIns\Aggregates\IndexFinder.cs --verbose

textrude.exe render --models Csv!model=Types.csv --template argmaxmin.sbn --output output=..\libraries\KustoLoco.Core\Evaluation\BuiltIns\Aggregates\ArgMinMaxFunctionImpl.cs --verbose

textrude.exe render --models Csv!model=Types.csv --template takeAny.sbn --output output=..\libraries\KustoLoco.Core\Evaluation\BuiltIns\Aggregates\TakeAny.cs --verbose

textrude.exe render --models Csv!model=Types.csv --template TypeComparison.sbn --output output=..\libraries\KustoLoco.Core\Evaluation\BuiltIns\ScalarFunctions\TypeComparison.cs --verbose

 textrude.exe render --models Csv!model=C:\work\open_source\kusto-loco\templates\Types.csv --template C:\work\open_source\kusto-loco\templates\MaxMinOf.sbn --output output=C:\work\open_source\kusto-loco\libraries\KustoLoco.Core\Evaluation\BuiltIns\ScalarFunctions\MinMaxOfFunctionImpl.cs --verbose

