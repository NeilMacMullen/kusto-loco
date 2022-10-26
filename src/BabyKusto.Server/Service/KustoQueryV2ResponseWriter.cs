using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace BabyKusto.Server.Service
{
    internal class KustoQueryV2ResponseWriter : IDisposable
    {
        private readonly Utf8JsonWriter _writer;
        private State _state;

        enum State
        {
            Initial,
            ReadyToWriteTable,
            WritingTable,
            Done,
            Disposed,
        }

        public KustoQueryV2ResponseWriter(Utf8JsonWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        internal Utf8JsonWriter JsonWriter => _writer;

        public async Task StartAsync()
        {
            if (_state != State.Initial)
            {
                throw new InvalidOperationException($"Invalid state to initiate {nameof(StartAsync)}: {_state}.");
            }

            _writer.WriteStartArray();
            WriteDatasetHeader();
            await _writer.FlushAsync();

            _state = State.ReadyToWriteTable;
        }

        public KustoQueryV2ResponseTableWriter CreateTableWriter()
        {
            if (_state != State.ReadyToWriteTable)
            {
                throw new InvalidOperationException($"Invalid state to initiate {nameof(CreateTableWriter)}: {_state}.");
            }

            _state = State.WritingTable;
            return new KustoQueryV2ResponseTableWriter(this);
        }

        public async Task FinishAsync()
        {
            if (_state != State.ReadyToWriteTable)
            {
                throw new InvalidOperationException($"Invalid state to initiate {nameof(FinishAsync)}: {_state}.");
            }

            WriteDatasetCompletion();
            _writer.WriteEndArray();
            await _writer.FlushAsync();

            _state = State.Done;
        }

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

        internal void OnTableWritten()
        {
            if (_state != State.WritingTable)
            {
                throw new InvalidOperationException();
            }

            _state = State.ReadyToWriteTable;
        }

        private void WriteDatasetHeader()
        {
            _writer.WriteStartObject();
            _writer.WriteString("FrameType", "DataSetHeader");
            _writer.WriteBoolean("IsProgressive", false);
            _writer.WriteString("Version", "v2.0");
            _writer.WriteEndObject();
        }

        private void WriteDatasetCompletion()
        {
            _writer.WriteStartObject();
            _writer.WriteString("FrameType", "DataSetCompletion");
            _writer.WriteBoolean("HasErrors", false);
            _writer.WriteBoolean("Cancelled", false);
            _writer.WriteEndObject();
        }
    }
}
