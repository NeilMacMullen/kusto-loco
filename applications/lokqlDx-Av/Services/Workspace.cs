namespace LokqlDx.Services;

/// <summary>
///     A Workspace is the query text and settings for a user's session.
/// </summary>
public record struct Workspace()
{
    public string Text { get;  set; } = string.Empty;
    public string StartupScript { get;  set; }  = string.Empty;
}
