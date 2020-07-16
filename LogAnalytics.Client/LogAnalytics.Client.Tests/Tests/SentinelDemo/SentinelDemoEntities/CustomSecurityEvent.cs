namespace LogAnalytics.Client.Tests.SentinelDemoEntities
{
    public class CustomSecurityEvent : ICustomLawEntity
    {
        public string Type { get { return "CustomSecurityEvent"; } }
        public string Severity { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
    }
}
