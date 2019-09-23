namespace LogAnalytics.DataCollector.Wrapper.Tests.SentinelDemoEntities
{
    public class TZSecurityEvent : ITZLawEntity
    {
        // ITZLawEntity
        public string Type { get { return "TZSecurityEvent"; } }
        public string Severity { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
    }
}
