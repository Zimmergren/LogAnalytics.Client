namespace LogAnalytics.Client.Tests.SentinelDemoEntities
{
    public class CustomAuditEvent : ICustomLawEntity
    {
        public string Type { get { return "CustomAuditEvent"; } }
        public string Severity { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
    }
}
