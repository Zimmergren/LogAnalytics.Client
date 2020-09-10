namespace LogAnalytics.Client.Tests.TestEntities
{
    public class TestEntityBadProperties
    {
        public long LongIsNotWorking { get;set;}
        public MyCustomClass MyCustomProperty { get; set; }
    }

    public class MyCustomClass
    {
        public enum MyEnum
        {
            Val1,
            Val2
        }

        public string MyString { get; set; }
    }
}
