using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace BabyKusto.Server.Service;

internal class KustoQueryV2ResponseWriter
{
    private State _state;

    public KustoQueryV2ResponseWriter(Utf8JsonWriter writer) =>
        JsonWriter = writer ?? throw new ArgumentNullException(nameof(writer));

    internal Utf8JsonWriter JsonWriter { get; }

    public async Task StartAsync()
    {
        if (_state != State.Initial)
        {
            throw new InvalidOperationException($"Invalid state to initiate {nameof(StartAsync)}: {_state}.");
        }

        JsonWriter.WriteStartArray();
        WriteDatasetHeader();
        await JsonWriter.FlushAsync();

        _state = State.ReadyToWriteTable;
    }

    public KustoQueryV2ResponseTableWriter CreateTableWriter()
    {
        if (_state != State.ReadyToWriteTable)
        {
            throw new InvalidOperationException(
                $"Invalid state to initiate {nameof(CreateTableWriter)}: {_state}.");
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
        JsonWriter.WriteEndArray();
        await JsonWriter.FlushAsync();

        _state = State.Done;
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
        JsonWriter.WriteStartObject();
        JsonWriter.WriteString("FrameType", "DataSetHeader");
        JsonWriter.WriteBoolean("IsProgressive", false);
        JsonWriter.WriteString("Version", "v2.0");
        JsonWriter.WriteEndObject();
    }

    private void WriteDatasetCompletion()
    {
        JsonWriter.WriteStartObject();
        JsonWriter.WriteString("FrameType", "DataSetCompletion");
        JsonWriter.WriteBoolean("HasErrors", false);
        JsonWriter.WriteBoolean("Cancelled", false);
        JsonWriter.WriteEndObject();
    }

    private enum State
    {
        Initial,
        ReadyToWriteTable,
        WritingTable,
        Done,
    }
}