namespace KustoLoco.FileFormats;

public readonly record struct TableSaveResult(string Error)
{
    public static TableSaveResult Success()
    {
        return new TableSaveResult(string.Empty);
    }
}
