using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using BabyKusto.Server.Contract;

namespace BabyKusto.Server.Service
{
    internal class KustoQueryV2ResponseTableWriter : IDisposable
    {
        private readonly KustoQueryV2ResponseWriter _owner;
        private readonly Utf8JsonWriter _jsonWriter;
        private State _state;

        enum State
        {
            Initial,
            ReadyToWriteRow,
            WritingRowValues,
            Done,
            Disposed,
        }

        public KustoQueryV2ResponseTableWriter(KustoQueryV2ResponseWriter owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _jsonWriter = owner.JsonWriter;
        }

        public async Task StartAsync(int tableId, KustoQueryV2ResponseTableKind tableKind, string tableName, IList<KustoApiV2ColumnDescription> columns)
        {
            if (_state != State.Initial)
            {
                throw new InvalidOperationException($"Invalid state for {nameof(StartAsync)}: {_state}.");
            }

            _jsonWriter.WriteStartObject();
            _jsonWriter.WriteString("FrameType", "DataTable");
            _jsonWriter.WriteNumber("TableId", tableId);
            _jsonWriter.WriteString("TableKind", tableKind.ToString());
            _jsonWriter.WriteString("TableName", tableName);

            _jsonWriter.WriteStartArray("Columns");
            foreach (var column in columns)
            {
                _jsonWriter.WriteStartObject();
                _jsonWriter.WriteString("ColumnName", column.ColumnName);
                _jsonWriter.WriteString("ColumnType", column.ColumnType);
                _jsonWriter.WriteEndObject();
            }
            _jsonWriter.WriteEndArray();
            _jsonWriter.WriteStartArray("Rows");

            await _jsonWriter.FlushAsync();
            _state = State.ReadyToWriteRow;
        }

        public async Task FinishAsync()
        {
            if (_state != State.ReadyToWriteRow)
            {
                throw new InvalidOperationException($"Invalid state to initiate {nameof(FinishAsync)}: {_state}.");
            }

            _jsonWriter.WriteEndArray();
            _jsonWriter.WriteEndObject();
            await _jsonWriter.FlushAsync();

            _owner.OnTableWritten();
            _state = State.Done;
        }

        public void StartRow()
        {
            if (_state != State.ReadyToWriteRow)
            {
                throw new InvalidOperationException($"Invalid state to initiate {nameof(StartRow)}: {_state}.");
            }

            _jsonWriter.WriteStartArray();
            _state = State.WritingRowValues;
        }

        public void WriteRowValue(JsonValue? value)
        {
            if (_state != State.WritingRowValues)
            {
                throw new InvalidOperationException($"Invalid state to initiate {nameof(WriteRowValue)}: {_state}.");
            }

            if (value is null)
            {
                _jsonWriter.WriteNullValue();
            }
            else
            {
                // TODO: Figure out why the following attempts to flush the underlying stream synchronously
                ////value.WriteTo(_jsonWriter);
                _jsonWriter.WriteRawValue(value.ToJsonString());
            }
        }

        public void EndRow()
        {
            if (_state != State.WritingRowValues)
            {
                throw new InvalidOperationException($"Invalid state to initiate {nameof(EndRow)}: {_state}.");
            }

            _jsonWriter.WriteEndArray();
            _state = State.ReadyToWriteRow;
        }

        public Task FlushAsync() => _jsonWriter.FlushAsync();

        public void Dispose()
        {
            if (_state == State.Disposed)
            {
                return;
            }
            else if (_state != State.Done)
            {
                throw new InvalidOperationException($"Invalid state for {nameof(Dispose)}: {_state}.");
            }

            _state = State.Disposed;
        }
    }
}
