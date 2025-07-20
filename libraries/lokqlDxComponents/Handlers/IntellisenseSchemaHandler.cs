using lokqlDxComponents.Models;
using lokqlDxComponents.Services;
using lokqlDxComponents.Views.Dialogs;

namespace lokqlDxComponents.Handlers;

public class IntellisenseSchemaHandler(SchemaIntellisenseProvider schemaProvider) : IIntellisenseHandler
{
    public Task<CompletionRequest> GetCompletionRequest(HandleKeyDownMessage message)
    {
        var cursor = message.Cursor;

        return Task.FromResult(message.Text switch
        {
            "@" => new CompletionRequest
            {
                Completions = schemaProvider.GetColumns(cursor.GetTextAroundCursor()),
                Rewind = 1
            },
            "[" => new CompletionRequest
            {
                Completions = schemaProvider.GetTables(cursor.GetTextAroundCursor()),
                Rewind = 1
            },
            _ => CompletionRequest.Empty
        });
    }
}
