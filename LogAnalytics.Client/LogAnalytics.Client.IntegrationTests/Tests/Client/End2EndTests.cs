using LogAnalytics.Client.IntegrationTests.Tests.Client.TestEntities;
using LogAnalytics.Client.IntegrationTests.Helpers;
using LogAnalytics.Client.IntegrationTests.TestEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Azure.Monitor.Query;
using Azure.Identity;
using Azure;
using Azure.Monitor.Query.Models;

namespace LogAnalytics.Client.IntegrationTests
{
    [TestClass]
    public class End2EndTests
    {
        private static LogsQueryClient _logsQueryClient;
        private static TestSecrets _secrets;
        private static string testIdentifierEntries;
        private static string testIdentifierEntry;
        private static string testIdentifierEncodingEntry;
        private static string testIdentifierNullableEntry;
        private static string testIdentifierLogTypeEntry;
        private static string diTestId;

        // Init: push some data into the LAW, then wait for a bit, then we'll run all the e2e tests.
        [ClassInitialize]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "TestContext parameter is required for a ClassInitialize method")]
        public static void Init(TestContext context)
        {
            // Wire up test secrets.
            _secrets = TestsBase.InitSecrets();

            // Wire up the LogsQueryClient to retrieve logs from Log Analytics.
            var credential = new ClientSecretCredential(
                _secrets.LawPrincipalCredentials.TenantId, 
                _secrets.LawPrincipalCredentials.ClientId, 
                _secrets.LawPrincipalCredentials.ClientSecret);

            _logsQueryClient = new LogsQueryClient(credential);

            // Set up unique identifiers for the tests. This helps us query the Log Analytics Workspace for our specific messages, and ensure the count and properties are correctly shipped to the logs.
            testIdentifierEntries = $"test-id-{Guid.NewGuid()}";
            testIdentifierEntry = $"test-id-{Guid.NewGuid()}";
            testIdentifierEncodingEntry = $"test-id-{Guid.NewGuid()}-ÅÄÖ@~#$%^&*()123";
            testIdentifierNullableEntry = $"test-id-{Guid.NewGuid()}";
            testIdentifierLogTypeEntry = $"test-id-{Guid.NewGuid()}";
            diTestId = $"test-id-di-{Guid.NewGuid()}";


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

            // 
            // DI LOGGER
            //
            var provider = new ServiceCollection()
                .AddLogAnalyticsClient(c =>
                {
                    c.WorkspaceId = _secrets.LawSecrets.LawId;
                    c.SharedKey = _secrets.LawSecrets.LawKey;
                }).BuildServiceProvider();

            var diLogger = provider.GetRequiredService<LogAnalyticsClient>();

            // Send a log entry to verify it works.
            diLogger.SendLogEntry(new DemoEntity
            {
                Criticality = "e2ecritical",
                Message = diTestId,
                SystemSource = "e2ewithdi",
                Priority = int.MinValue + 1
            }, "endtoendwithdilogs");


            // Unfortunately, from the time we send the logs, until they appear in LAW, takes a few minutes. 
            Thread.Sleep(6 * 1000 * 60);
        }

        [TestMethod]
        public void E2E_VerifySendLogEntries_Test()
        {
            Response<LogsQueryResult> response = _logsQueryClient.QueryWorkspace(
                _secrets.LawSecrets.LawId,
                $"endtoendlogs_CL | where Message == '{testIdentifierEntries}' | order by TimeGenerated desc | limit 20",
                new QueryTimeRange(TimeSpan.FromMinutes(30)));

            LogsTable table = response.Value.Table;

            Assert.AreEqual(12, table.Rows.Count);
            Assert.AreEqual(testIdentifierEntries, table.Rows[0]["Message"]);

            var entry = table.Rows[0];
            Assert.AreEqual(testIdentifierEntries, entry["Message"]);
            Assert.AreEqual("e2etest", entry["SystemSource_s"]);
            Assert.AreEqual("e2ecriticality", entry["Criticality_s"]);
            Assert.AreEqual((double)int.MaxValue-1, entry["Priority_d"]);
        }

        [TestMethod]
        public void E2E_VerifySendLogEntry_Test()
        {
            Response<LogsQueryResult> response = _logsQueryClient.QueryWorkspace(
                _secrets.LawSecrets.LawId,
                $"endtoendlogs_CL | where Message == '{testIdentifierEntry}' | order by TimeGenerated desc | limit 10",
                new QueryTimeRange(TimeSpan.FromMinutes(30)));

            LogsTable table = response.Value.Table;

            Assert.AreEqual(1, table.Rows.Count);

            var entry = table.Rows[0];
            Assert.AreEqual(testIdentifierEntry, entry["Message"]);
            Assert.AreEqual("e2etestsingleentry", entry["SystemSource_s"]);
            Assert.AreEqual("e2ecriticalitysingleentry", entry["Criticality_s"]);
            Assert.AreEqual((double)int.MinValue + 1, entry["Priority_d"]);
        }

        [TestMethod]
        public void E2E_VerifySendLogEntry_VerifyEncoding_Test()
        {
            Response<LogsQueryResult> response = _logsQueryClient.QueryWorkspace(
                _secrets.LawSecrets.LawId,
                $"endtoendlogs_CL | where Message == '{testIdentifierEncodingEntry}' | order by TimeGenerated desc | limit 10",
                new QueryTimeRange(TimeSpan.FromMinutes(30)));

            LogsTable table = response.Value.Table;
            Assert.AreEqual(1, table.Rows.Count);

            var entry = table.Rows[0];
            Assert.AreEqual($"{testIdentifierEncodingEntry}", entry["Message"]);
            Assert.AreEqual("e2etestencoding", entry["SystemSource_s"]);
            Assert.AreEqual("e2ecriticalityencoding", entry["Criticality_s"]);
            Assert.AreEqual((double)int.MaxValue - 10000, entry["Priority_d"]);
        }

        [TestMethod]
        public void E2E_VerifySendLogEntry_Nullable_Test()
        {
            Response<LogsQueryResult> response = _logsQueryClient.QueryWorkspace(
                _secrets.LawSecrets.LawId,
                $"endtoendlogs_CL | where Message == '{testIdentifierNullableEntry}' | order by TimeGenerated desc | limit 10",
                new QueryTimeRange(TimeSpan.FromMinutes(30)));

            LogsTable table = response.Value.Table;
            Assert.AreEqual(1, table.Rows.Count);

            var entry = table.Rows[0];
            Assert.AreEqual($"{testIdentifierNullableEntry}", entry["Message"]);
            //Assert.AreEqual(true, entry.ContainsKey("WithValue_d"));
            Assert.AreEqual((double)int.MaxValue - 20000, entry["WithValue_d"]);
            //Assert.AreEqual(false, entry.ContainsKey("NoValue_d"));
        }

        [TestMethod]
        public void E2E_VerifySendLogEntry_LogTypeName_Test()
        {
            Response<LogsQueryResult> response = _logsQueryClient.QueryWorkspace(
                _secrets.LawSecrets.LawId,
                $"log_name_123_CL | where Message == '{testIdentifierLogTypeEntry}' | order by TimeGenerated desc | limit 10",
                new QueryTimeRange(TimeSpan.FromMinutes(30)));

            LogsTable table = response.Value.Table;
            Assert.AreEqual(1, table.Rows.Count);

            var entry = table.Rows[0];
            Assert.AreEqual($"{testIdentifierLogTypeEntry}", entry["Message"]);
            Assert.AreEqual("logtypetest", entry["SystemSource_s"]);
            Assert.AreEqual("Critical", entry["Criticality_s"]);
            Assert.AreEqual((double)int.MaxValue - 1, entry["Priority_d"]);
        }

        [TestMethod]
        public void E2E_VerifySendLogEntryWithDIClient_Test()
        {
            Response<LogsQueryResult> response = _logsQueryClient.QueryWorkspace(
                _secrets.LawSecrets.LawId,
                $"endtoendwithdilogs_CL | where Message == '{diTestId}' | limit 5",
                new QueryTimeRange(TimeSpan.FromMinutes(30)));

            // Arrange & Act
            LogsTable table = response.Value.Table;
            Assert.AreEqual(1, table.Rows.Count);
            var entry = table.Rows[0];

            // Assert.
            Assert.AreEqual(diTestId, entry["Message"]);
            Assert.AreEqual("e2ecritical", entry["Criticality_s"]);
            Assert.AreEqual("e2ewithdi", entry["SystemSource_s"]);
            Assert.AreEqual((double)int.MinValue + 1, entry["Priority_d"]);
        }
    }
}
