namespace LogAnalytics.DataCollector.Wrapper.Tests.SentinelDemoEntities
{
    public interface ITZLawEntity
    {
        string Type { get; }
        string Severity { get;set; }
        string Message { get;set; }
        string Source { get;set; }
    }
}
