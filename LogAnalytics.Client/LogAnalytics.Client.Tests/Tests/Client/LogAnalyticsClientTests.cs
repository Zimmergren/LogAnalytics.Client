using LogAnalytics.Client.Tests.Helpers;
using LogAnalytics.Client.Tests.TestEntities;
using LogAnalytics.Client.Tests.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace LogAnalytics.Client.Tests
{
    /// <summary>
    /// Basic tests for the LogAnalyticsClient.
    /// </summary>
    [TestClass]
    public class LogAnalyticsClientTests : TestsBase
    {
        private static TestSecrets _secrets;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _secrets = InitSecrets();
        }

        // TODO: Enhance the coverage of base tests. 
        // - Test scenarios and allowed patterns
        // - Test disallowed patterhs, or data, and ensure exceptions are logically handled. 
        // - No verification of data in LAW here, only the usage of the LogAnalyticsClient.

        [TestMethod]
        public void SendLogMessageTest()
        {
            LogAnalyticsClient logger = new LogAnalyticsClient(
                workspaceId: _secrets.LawSecrets.LawId,
                sharedKey: _secrets.LawSecrets.LawKey);

            // after this is sent, wait a couple of minutes and then check your Log Analytics dashboard.
            // todo: if you want a true integration test, wait for it here, then query the logs from code and verify that the entries are there, then assert the test.
            logger.SendLogEntry(new TestEntity
            {
                Category = GetCategory(),
                TestString = $"String Test",
                TestBoolean = true,
                TestDateTime = DateTime.UtcNow,
                TestDouble = 2.1,
                TestGuid = Guid.NewGuid()
            }, "demolog").Wait();
        }

        [TestMethod]
        public void SendDemoEntities_Test()
        {
            LogAnalyticsClient logger = new LogAnalyticsClient(
                workspaceId: _secrets.LawSecrets.LawId,
                sharedKey: _secrets.LawSecrets.LawKey);

            List<DemoEntity> entities = new List<DemoEntity>();
            for (int ii = 0; ii < 5000; ii++)
            {
                entities.Add(new DemoEntity
                {
                    Criticality = GetCriticality(),
                    Message = "lorem ipsum dolor sit amet",
                    SystemSource = GetSystemSource()
                });
            }

            // after this is sent, wait a couple of minutes and then check your Log Analytics dashboard.
            // todo: if you want a true integration test, wait for it here, then query the logs from code and verify that the entries are there, then assert the test.
            logger.SendLogEntries(entities, "demolog").Wait();
        }


        [TestMethod]
        public void SendBadEntity_Test()
        {
            LogAnalyticsClient logger = new LogAnalyticsClient(
                workspaceId: _secrets.LawSecrets.LawId,
                sharedKey: _secrets.LawSecrets.LawKey);

            List<TestEntityBadProperties> entities = new List<TestEntityBadProperties>();
            for (int ii = 0; ii < 1; ii++)
            {
                entities.Add(new TestEntityBadProperties
                {
                    MyCustomProperty = new MyCustomClass
                    {
                        MyString = "hello world",
                    },
                    TestInt = 123
                });
            }

            Assert.ThrowsException<AggregateException>(() => logger.SendLogEntries(entities, "testlog").Wait());
        }

        private string GetCategory()
        {
            var categories = new[] { "DevOps", "Development", "Management", "Administration", "IR", "HR" };
            int rnd = new Random().Next(0, categories.Length);

            return categories[rnd];
        }
        private string GetCriticality()
        {
            var categories = new[] { "Exception", "Warning", "Informational" };
            int rnd = new Random().Next(0, categories.Length);

            return categories[rnd];
        }
        private string GetSystemSource()
        {
            var categories = new[] { "Search Index Runner", "Analysis Runner", "Discovery Engine", "Magical Unicorn Code Box", "Amazing Apples" };
            int rnd = new Random().Next(0, categories.Length);

            return categories[rnd];
        }
    }
}
