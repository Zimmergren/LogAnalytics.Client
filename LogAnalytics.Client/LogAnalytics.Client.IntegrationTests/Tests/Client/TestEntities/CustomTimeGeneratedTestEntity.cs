using System;

namespace LogAnalytics.Client.IntegrationTests.Tests.Client.TestEntities
{
    public class CustomTimeGeneratedTestEntity
    {
        public string Criticality { get; set; }
        public string SystemSource { get; set; }
        public string Message { get; set; }
        public int Priority { get; set; }
        public DateTime CustomDateTime { get; set; }
    }
}
