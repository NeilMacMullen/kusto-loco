using KustoLoco.Core;

public readonly record struct QueryResultWithSender(string Sender, KustoQueryResult Result, bool ImmediateDisplay);
