namespace lokqlDx;

/// <summary>
///     A Workspace is the query text and settings for a user's session.
/// </summary>
public class Workspace
{
    public Dictionary<string,string> Settings { get; set; } = new();
    public string Text { get; set; } = string.Empty;
}
