namespace LogAnalytics.DataCollector.Wrapper.Tests.SentinelDemoEntities
{
    public class TZAuditEvent : ITZLawEntity
    {
        // ITZLawEntity
        public string Type { get { return "TZAuditEvent"; } }
        public string Severity { get; set;}
        public string Message { get; set; }
        public string Source { get; set; }
    }
}
