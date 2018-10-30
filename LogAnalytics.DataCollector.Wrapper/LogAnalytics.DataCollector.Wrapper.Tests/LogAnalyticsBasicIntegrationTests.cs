using System;
using System.Collections.Generic;
using LogAnalytics.DataCollector.Wrapper.Tests.TestEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogAnalytics.DataCollector.Wrapper.Tests
{
    [TestClass]
    public class LogAnalyticsBasicIntegrationTests
    {
        /*
         Disclaimer and notes: 
            This wrapper is currently not unit tested nor integration tested properly.
            In order to use this in another project, please ensure proper code coverage and unit- and integration testing. 
            Feel free to submit a PR to the github repo too.
         */

        // You should grab these variables from Key Vault, Credential Store on your machine, local config files or some other non-code related location :)
        private string workspaceId = "";
        private string sharedKey = "";

        [TestMethod]
        public void SendLogMessageTest()
        {
            LogAnalyticsWrapper logger = new LogAnalyticsWrapper(
                workspaceId: workspaceId,
                sharedKey: sharedKey);

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
            }, "demolog");
        }

        [TestMethod]
        public void SendDemoEntities_Test()
        {
            LogAnalyticsWrapper logger = new LogAnalyticsWrapper(
                workspaceId: workspaceId,
                sharedKey: sharedKey);

            List<DemoEntity> entities = new List<DemoEntity>();
            for (int ii = 0; ii < 5000; ii++)
            {
                entities.Add(new DemoEntity
                {
                    Criticality = GetCriticality(),
                    Message = "lorem ipsum dolor sit amet",
                    SystemSource = GetSystemSource()
                } );
            }

            // after this is sent, wait a couple of minutes and then check your Log Analytics dashboard.
            // todo: if you want a true integration test, wait for it here, then query the logs from code and verify that the entries are there, then assert the test.
            logger.SendLogEntries(entities, "demolog");
        }

        [TestMethod]
        public void SendBadEntity_Test()
        {
            LogAnalyticsWrapper logger = new LogAnalyticsWrapper(
                workspaceId: workspaceId,
                sharedKey: sharedKey);

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

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => logger.SendLogEntries(entities, "testlog"));
        }

        private string GetCategory()
        {
            var categories = new []{"DevOps", "Development", "Management", "Administration", "IR", "HR"};
            int rnd = new Random().Next(0,categories.Length);
            
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
