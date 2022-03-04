// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Kusto.Language.Symbols;
using Microsoft.Extensions.Internal;

namespace BabyKusto.Core.Evaluation
{
    internal abstract class DerivedTableSourceBase<TContext> : ITableSource
    {
        private readonly ITableSource _source;

        public DerivedTableSourceBase(ITableSource source)
        {
            _source = source;
        }

        public abstract TableSymbol Type { get; }
        protected ITableSource Source => _source;

        public virtual IEnumerable<ITableChunk> GetData()
        {
            var context = Init();
            foreach (var chunk in _source.GetData())
            {
                var (newContext, newChunk, shouldBreak) = ProcessChunkInternal(context, chunk);
                context = newContext;

                if (newChunk != null)
                {
                    yield return newChunk;
                }

                if (shouldBreak)
                {
                    break;
                }
            }

            var lastChunk = ProcessLastChunkInternal(context);
            if (lastChunk != null)
            {
                yield return lastChunk;
            }
        }

        public virtual async IAsyncEnumerable<ITableChunk> GetDataAsync([EnumeratorCancellation] CancellationToken cancellation = default)
        {
            var context = Init();
            await foreach (var chunk in _source.GetDataAsync(cancellation))
            {
                var (newContext, newChunk, shouldBreak) = ProcessChunkInternal(context, chunk);
                context = newContext;

                if (newChunk != null)
                {
                    yield return newChunk;
                }

                if (shouldBreak)
                {
                    break;
                }
            }

            var lastChunk = ProcessLastChunkInternal(context);
            if (lastChunk != null)
            {
                yield return lastChunk;
            }
        }

        protected virtual TContext Init() => default!;
        protected abstract (TContext NewContext, ITableChunk? NewChunk, bool ShouldBreak) ProcessChunk(TContext context, ITableChunk chunk);
        protected virtual ITableChunk? ProcessLastChunk(TContext context) => null;

        private (TContext NewContext, ITableChunk? NewChunk, bool ShouldBreak) ProcessChunkInternal(TContext context, ITableChunk chunk)
        {
            var (newContext, newChunk, shouldBreak) = ProcessChunk(context, chunk);
            if (newChunk != null && newChunk.Table != this)
            {
                throw new InvalidOperationException($"Coding defect, new chunk should set the right table it belongs to ({TypeNameHelper.GetTypeDisplayName(this)})");
            }

            return (newContext, newChunk, shouldBreak);
        }

        private ITableChunk? ProcessLastChunkInternal(TContext context)
        {
            var newChunk = ProcessLastChunk(context);
            if (newChunk != null && newChunk.Table != this)
            {
                throw new InvalidOperationException($"Coding defect, new chunk should set the right table it belongs to ({TypeNameHelper.GetTypeDisplayName(this)})");
            }

            return newChunk;
        }
    }

    internal struct NoContext
    {
    }
}
