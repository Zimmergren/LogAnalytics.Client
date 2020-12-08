using System;

namespace LogAnalytics.Client.UnitTests.TestEntities
{
    public class ValidTestEntity
    {
        public string Category { get; set; }
        public string TestString { get; set; }
        public bool TestBoolean { get; set; }
        public Double TestDouble { get; set; }
        public int TestInteger { get; set; }
        public DateTime TestDateTime { get; set; }
        public Guid TestGuid { get; set; }
    }
}
