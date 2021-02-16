namespace LogAnalytics.Client.UnitTests.TestEntities
{
    public class InvalidTestEntity
    {
        public long LongIsNotWorking { get; set; }
        public AnotherCustomClass MyCustomProperty { get; set; }
    }

    public class AnotherCustomClass
    {
        public enum MyCustomEnumeration
        {
            Val1,
            Val2
        }

        public string MyString { get; set; }
    }
}
