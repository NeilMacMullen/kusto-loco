﻿using KustoLoco.Core;

public class NullFileLoader : IFileBasedTableAccess
{
    public Task<bool> TryLoad(string path, KustoQueryContext context, string name, IProgress<string> progressReporter)
        => Task.FromResult(false);

    public IReadOnlyCollection<string> SupportedFileExtensions() => [];

    public Task TrySave(string path, KustoQueryResult result) => Task.CompletedTask;
}