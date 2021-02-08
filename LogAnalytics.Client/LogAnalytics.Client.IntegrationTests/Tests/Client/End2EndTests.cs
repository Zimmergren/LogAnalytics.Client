using LogAnalytics.Client.IntegrationTests.Tests.Client.TestEntities;
using LogAnalytics.Client.IntegrationTests.Helpers;
using LogAnalytics.Client.IntegrationTests.TestEntities;
using Microsoft.Azure.OperationalInsights;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LogAnalytics.Client.IntegrationTests
{
    [TestClass]
    public class End2EndTests : TestsBase
    {
        private static OperationalInsightsDataClient _dataClient;
        private static TestSecrets _secrets;
        private static string testIdentifierEntries;
        private static string testIdentifierEntry;
        private static string testIdentifierEncodingEntry;
        private static string testIdentifierNullableEntry;
        private static string testIdentifierLogTypeEntry;
        private static string testIdentifierResourceIdEntry;
        private static string testIdentifierTimeGeneratedFieldEntry;
        private static DateTime testTimeGeneratedDateStamp;

        // Init: push some data into the LAW, then wait for a bit, then we'll run all the e2e tests.
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            // Wire up test secrets.
            _secrets = InitSecrets();

            // Get a data client, helping us actually Read data, too.
            _dataClient = GetLawDataClient(
                _secrets.LawSecrets.LawId, 
                _secrets.LawPrincipalCredentials.ClientId, 
                _secrets.LawPrincipalCredentials.ClientSecret, 
                _secrets.LawPrincipalCredentials.Domain)
                .Result;

            // Set up unique identifiers for the tests. This helps us query the Log Analytics Workspace for our specific messages, and ensure the count and properties are correctly shipped to the logs.
            testIdentifierEntries = $"test-id-{Guid.NewGuid()}";
            testIdentifierEntry = $"test-id-{Guid.NewGuid()}";
            testIdentifierEncodingEntry = $"test-id-{Guid.NewGuid()}-ÅÄÖ@~#$%^&*()123";
            testIdentifierNullableEntry = $"test-id-{Guid.NewGuid()}";
            testIdentifierLogTypeEntry = $"test-id-{Guid.NewGuid()}";
            testIdentifierResourceIdEntry = $"test-id-{Guid.NewGuid()}";
            testIdentifierTimeGeneratedFieldEntry = $"test-id-{Guid.NewGuid()}";
            testTimeGeneratedDateStamp = DateTime.UtcNow.AddDays(-5);


            // Initialize the LAW Client.
            LogAnalyticsClient logger = new LogAnalyticsClient(
                workspaceId: _secrets.LawSecrets.LawId,
                sharedKey: _secrets.LawSecrets.LawKey);

            // Test 1 prep: Push a collection of entities to the logs.
            List<DemoEntity> entities = new List<DemoEntity>();
            for (int ii = 0; ii < 12; ii++)
            {
                entities.Add(new DemoEntity
                {
                    Criticality = "e2ecriticality",
                    Message = testIdentifierEntries,
                    SystemSource = "e2etest",
                    Priority = int.MaxValue-1
                });
            }
            logger.SendLogEntries(entities, "endtoendlogs").Wait();


            // Test 2 prep: Send a single entry to the logs.
            logger.SendLogEntry(new DemoEntity
            {
                Criticality = "e2ecriticalitysingleentry",
                Message = testIdentifierEntry,
                SystemSource = "e2etestsingleentry", 
                Priority = int.MinValue + 1
            }, "endtoendlogs");

            // Since it takes a while before the logs are queryable, we'll sit tight and wait for a few minutes before we launch the retrieval-tests.

            // Test 3 prep: Verify that different encoding types work
            var encodingTestEntity = new DemoEntity
            {
                Criticality = "e2ecriticalityencoding",
                Message = $"{testIdentifierEncodingEntry}", // Special encoding test.
                SystemSource = "e2etestencoding",
                Priority = int.MaxValue - 10000
            };
            logger.SendLogEntry(encodingTestEntity, "endtoendlogs");

            // Test 4 prep: Verify that nullable entries work
            var nullableTestEntity = new NullableDemoEntity
            {
                Message = $"{testIdentifierNullableEntry}",
                NoValue = null,
                WithValue = int.MaxValue - 20000
            };

            logger.SendLogEntry(nullableTestEntity, "endtoendlogs");


            // Test 5 prep: Verify we can use AlphaNum + Underscore for Log-Type.
            var logTypeTestEntity = new DemoEntity
            {
                Criticality = "Critical",
                Message = testIdentifierLogTypeEntry,
                Priority = int.MaxValue - 1,
                SystemSource = "logtypetest"
            };
            logger.SendLogEntry(logTypeTestEntity, "log_name_123");

            // Test 6 prep: Verify the ingestion of ResourceId.
            var resourceIdTestEntity = new DemoEntity
            {
                Criticality = "Critical",
                Message = testIdentifierResourceIdEntry,
                Priority = int.MaxValue - 1,
                SystemSource = "resourceidtest"
            };
            logger.SendLogEntry(resourceIdTestEntity, "endtoendlogs", resourceId: _secrets.LawSecrets.LawResourceId);

            // Test 7 prep: Verify the ingestion of TimeGeneratedField.
            var timeGeneratedFieldTestEntity = new CustomTimeGeneratedTestEntity
            {
                Criticality = "Critical",
                Message = testIdentifierTimeGeneratedFieldEntry,
                Priority = int.MaxValue - 1,
                SystemSource = "timegeneratedfieldtest",
                CustomDateTime = testTimeGeneratedDateStamp
            };
            logger.SendLogEntry(timeGeneratedFieldTestEntity, "endtoendtimelogs", timeGeneratedCustomFieldName: "CustomDateTime");

            // Unfortunately, from the time we send the logs, until they appear in LAW, takes a few minutes. 
            Thread.Sleep(8 * 1000 * 60);
        }

        [TestMethod]
        public void E2E_VerifySendLogEntries_Test()
        {
            var query = _dataClient.Query($"endtoendlogs_CL | where Message == '{testIdentifierEntries}' | order by TimeGenerated desc | limit 20");
            Assert.AreEqual(12, query.Results.Count());
            Assert.AreEqual(testIdentifierEntries, query.Results.First()["Message"]);

            var entry = query.Results.First();
            Assert.AreEqual(testIdentifierEntries, entry["Message"]);
            Assert.AreEqual("e2etest", entry["SystemSource_s"]);
            Assert.AreEqual("e2ecriticality", entry["Criticality_s"]);
            Assert.AreEqual($"{int.MaxValue-1}", entry["Priority_d"]);
        }

        [TestMethod]
        public void E2E_VerifySendLogEntry_Test()
        {
            var query = _dataClient.Query($"endtoendlogs_CL | where Message == '{testIdentifierEntry}' | order by TimeGenerated desc | limit 10");
            Assert.AreEqual(1, query.Results.Count());

            var entry = query.Results.First();
            Assert.AreEqual(testIdentifierEntry, entry["Message"]);
            Assert.AreEqual("e2etestsingleentry", entry["SystemSource_s"]);
            Assert.AreEqual("e2ecriticalitysingleentry", entry["Criticality_s"]);
            Assert.AreEqual($"{int.MinValue + 1}", entry["Priority_d"]);
        }

        [TestMethod]
        public void E2E_VerifySendLogEntry_VerifyEncoding_Test()
        {
            var query = _dataClient.Query($"endtoendlogs_CL | where Message == '{testIdentifierEncodingEntry}' | order by TimeGenerated desc | limit 10");
            Assert.AreEqual(1, query.Results.Count());

            var entry = query.Results.First();
            Assert.AreEqual($"{testIdentifierEncodingEntry}", entry["Message"]);
            Assert.AreEqual("e2etestencoding", entry["SystemSource_s"]);
            Assert.AreEqual("e2ecriticalityencoding", entry["Criticality_s"]);
            Assert.AreEqual($"{int.MaxValue - 10000}", entry["Priority_d"]);
        }

        [TestMethod]
        public void E2E_VerifySendLogEntry_Nullable_Test()
        {
            var query = _dataClient.Query($"endtoendlogs_CL | where Message == '{testIdentifierNullableEntry}' | order by TimeGenerated desc | limit 10");
            Assert.AreEqual(1, query.Results.Count());

            var entry = query.Results.First();
            Assert.AreEqual($"{testIdentifierNullableEntry}", entry["Message"]);
            Assert.AreEqual(true, entry.ContainsKey("WithValue_d"));
            Assert.AreEqual($"{int.MaxValue - 20000}", entry["WithValue_d"]);
            Assert.AreEqual(false, entry.ContainsKey("NoValue_d"));
        }

        [TestMethod]
        public void E2E_VerifySendLogEntry_LogTypeName_Test()
        {
            var query = _dataClient.Query($"log_name_123_CL | where Message == '{testIdentifierLogTypeEntry}' | order by TimeGenerated desc | limit 10");
            Assert.AreEqual(1, query.Results.Count());

            var entry = query.Results.First();
            Assert.AreEqual($"{testIdentifierLogTypeEntry}", entry["Message"]);
            Assert.AreEqual("logtypetest", entry["SystemSource_s"]);
            Assert.AreEqual("Critical", entry["Criticality_s"]);
            Assert.AreEqual($"{int.MaxValue - 1}", entry["Priority_d"]);
        }

        [TestMethod]
        public void E2E_VerifySendLogEntry_ResourceId_Test()
        {
            var query = _dataClient.Query($"endtoendlogs_CL | where Message == '{testIdentifierResourceIdEntry}' | order by TimeGenerated desc | limit 10");
            Assert.AreEqual(1, query.Results.Count());

            var entry = query.Results.First();
            Assert.AreEqual($"{testIdentifierResourceIdEntry}", entry["Message"]);
            Assert.AreEqual("resourceidtest", entry["SystemSource_s"]);
            Assert.AreEqual("Critical", entry["Criticality_s"]);
            Assert.AreEqual($"{int.MaxValue - 1}", entry["Priority_d"]);

            // Assert: that the returned Resource ID exist and matches our specicified resource id.
            Assert.AreEqual(_secrets.LawSecrets.LawResourceId, entry["_ResourceId"]);
        }

        [TestMethod]
        public void E2E_VerifySendLogEntry_TimeGeneratedField_Test()
        {
            var query = _dataClient.Query($"endtoendtimelogs_CL | where Message == '{testIdentifierTimeGeneratedFieldEntry}'");
            Assert.AreEqual(1, query.Results.Count());

            var entry = query.Results.First();
            Assert.AreEqual($"{testIdentifierTimeGeneratedFieldEntry}", entry["Message"]);
            Assert.AreEqual("timegeneratedfieldtest", entry["SystemSource_s"]);
            Assert.AreEqual("Critical", entry["Criticality_s"]);
            Assert.AreEqual($"{int.MaxValue - 1}", entry["Priority_d"]);

            // Assert: that the time stamp is accurate, and reflects our custom datetime field. Time format of the field needs to be based on ISO 8601, which in C# mathes this DateTime.ToString() format: yyyy-MM-ddThh:mm:ssZ
            var testTimestamp = testTimeGeneratedDateStamp.ToString("yyyy-MM-ddThh:mm:ssZ");
            var returnedTimestamp = DateTime.Parse(entry["TimeGenerated"]).ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ");
            Assert.AreEqual(testTimestamp, returnedTimestamp);
        }

        // TODO: Enhance test coverage in the E2E tests
        // - Cover custom types and entities
        // - Cover huge amounts of data

        private static async Task<OperationalInsightsDataClient> GetLawDataClient(string workspaceId, string lawPrincipalClientId, string lawPrincipalClientSecret, string domain)
        {
            // Note 2020-07-26. This is from the Microsoft.Azure.OperationalInsights nuget, which haven't been updated since 2018. 
            // Possibly we'll look for a REST-approach instead, and create the proper client here.

            var authEndpoint = "https://login.microsoftonline.com";
            var tokenAudience = "https://api.loganalytics.io/";

            var adSettings = new ActiveDirectoryServiceSettings
            {
                AuthenticationEndpoint = new Uri(authEndpoint),
                TokenAudience = new Uri(tokenAudience),
                ValidateAuthority = true
            };

            var credentials = await ApplicationTokenProvider.LoginSilentAsync(domain, lawPrincipalClientId, lawPrincipalClientSecret, adSettings);

            var client = new OperationalInsightsDataClient(credentials)
            {
                WorkspaceId = workspaceId
            };

            return client;
        }

    }
}
