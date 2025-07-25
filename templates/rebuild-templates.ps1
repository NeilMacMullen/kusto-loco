
textrude.exe render --models Csv!model=types.csv --template IndexFinder.tpl --output output=..\libraries\KustoLoco.Core\Evaluation\BuiltIns\Aggregates\IndexFinder.cs --verbose

textrude.exe render --models Csv!model=types.csv --template argmaxmin.tpl --output output=..\libraries\KustoLoco.Core\Evaluation\BuiltIns\Aggregates\ArgMinMaxFunctionImpl.cs --verbose