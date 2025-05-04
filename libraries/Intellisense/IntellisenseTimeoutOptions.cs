namespace Intellisense;

public class IntellisenseTimeoutOptions
{
    public const string Timeout = "Timeout";
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromMilliseconds(100);
    public TimeSpan IntellisenseTimeout { get; set; } = DefaultTimeout;
}
