namespace LogAnalytics.Client.IntegrationTests.TestEntities
{
    public class TestEntityBadProperties
    {
        public long LongIsNotWorking { get;set;}
        public MyCustomClass MyCustomProperty { get; set; }
    }

    public class MyCustomClass
    {
        public enum MyCustomEnumeration
        {
            Val1,
            Val2
        }

        public string MyString { get; set; }
    }
}
