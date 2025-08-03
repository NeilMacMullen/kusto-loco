//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using KustoLoco.Core.Util;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using NLog;

namespace KustoLoco.Core.Evaluation;

internal abstract class DerivedTableSourceBase<TContext> : ITableSource
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private bool _alreadyIterated;
    protected DerivedTableSourceBase(ITableSource source) => Source = source;
    protected ITableSource Source { get; }

    public abstract TableSymbol Type { get; }

    public virtual IEnumerable<ITableChunk> GetData()
    {
        var context = Init();
        foreach (var chunk in Source.GetData())
        {
            var (newContext, newChunk, shouldBreak) = ProcessChunkInternal(context, chunk);
            context = newContext;

            if (newChunk != TableChunk.Empty)
            {
                yield return newChunk;
            }

            if (shouldBreak)
            {
                break;
            }
        }

        var lastChunk = ProcessLastChunkInternal(context);
        if (lastChunk != TableChunk.Empty)
        {
            yield return lastChunk;
        }
    }

    public virtual async IAsyncEnumerable<ITableChunk> GetDataAsync(
        [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var context = Init();
        await foreach (var chunk in Source.GetDataAsync(cancellation))
        {
            var (newContext, newChunk, shouldBreak) = ProcessChunkInternal(context, chunk);
            context = newContext;

            if (newChunk != TableChunk.Empty)
            {
                yield return newChunk;
            }

            if (shouldBreak)
            {
                break;
            }
        }

        var lastChunk = ProcessLastChunkInternal(context);
        if (lastChunk != TableChunk.Empty)
        {
            yield return lastChunk;
        }
    }

    protected virtual TContext Init() => default!;

    protected abstract (TContext NewContext, ITableChunk NewChunk, bool ShouldBreak) ProcessChunk(TContext context,
        ITableChunk chunk);

    protected virtual ITableChunk ProcessLastChunk(TContext context) => TableChunk.Empty;

    private (TContext NewContext, ITableChunk NewChunk, bool ShouldBreak) ProcessChunkInternal(TContext context,
        ITableChunk chunk)
    {
        var (newContext, newChunk, shouldBreak) = ProcessChunk(context, chunk);
        if (newChunk != TableChunk.Empty && newChunk.Table != this)
        {
            throw new InvalidOperationException(
                $"Coding defect, new chunk should set the right table it belongs to ({TypeNameHelper.GetTypeDisplayName(this)})");
        }

        return (newContext, newChunk, shouldBreak);
    }

    private ITableChunk ProcessLastChunkInternal(TContext context)
    {
        if (_alreadyIterated)
            Logger.Warn($"Warning - unnecessary extra iteration of {GetType().Name}");
        _alreadyIterated = true;
        var newChunk = ProcessLastChunk(context);
        if (newChunk != TableChunk.Empty && newChunk.Table != this)
        {
            throw new InvalidOperationException(
                $"Coding defect, new chunk should set the right table it belongs to ({TypeNameHelper.GetTypeDisplayName(this)})");
        }

        return newChunk;
    }
}

internal struct NoContext
{
}