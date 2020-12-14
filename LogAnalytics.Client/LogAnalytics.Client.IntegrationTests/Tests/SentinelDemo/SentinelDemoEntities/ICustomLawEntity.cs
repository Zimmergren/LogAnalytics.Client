namespace LogAnalytics.Client.IntegrationTests.SentinelDemoEntities
{
    public interface ICustomLawEntity
    {
        string Type { get; }
        string Severity { get; set; }
        string Message { get; set; }
        string Source { get; set; }
    }
}
