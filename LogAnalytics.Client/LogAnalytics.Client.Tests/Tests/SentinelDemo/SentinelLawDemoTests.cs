using LogAnalytics.Client.Tests.SentinelDemoEntities;
using LogAnalyticsClient.Tests.Helpers;
using LogAnalyticsClient.Tests.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace LogAnalytics.Client.Tests
{
    [TestClass]
    public class SentinelLawDemoTests : TestsBase
    {
        private static LawSecrets _secrets;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _secrets = InitSecrets();
        }

        [TestMethod]
        public void SendDemoEvents_Test()
        {
            LogAnalyticsClient logger = new LogAnalyticsClient(
                workspaceId: _secrets.LawId,
                sharedKey: _secrets.LawKey);

            // SECURITY EVENTS (Sample/Demo code only)
            List<CustomSecurityEvent> securityEntities = new List<CustomSecurityEvent>();
            int securityRandom = new Random().Next(100, 12000); // amount of randomized log events to ship.
            for (int ii = 0; ii < securityRandom; ii++)
            {
                securityEntities.Add(new CustomSecurityEvent
                {
                    Severity = GetSeverity(DateTime.UtcNow.Millisecond),
                    Source = Environment.MachineName,
                    Message = GetRandomMessageForTest(DateTime.UtcNow.Millisecond)
                });
            }

            logger.SendLogEntries(securityEntities, "customsecurity").Wait();

            // AUDIT EVENTS (Sample/Demo code only)
            List<CustomAuditEvent> auditEntities = new List<CustomAuditEvent>();
            int auditRandom = new Random().Next(250, 5000); // amount of randomized log events to ship.
            for (int ii = 0; ii < auditRandom; ii++)
            {
                auditEntities.Add(new CustomAuditEvent
                {
                    Severity = GetSeverity(DateTime.UtcNow.Millisecond),
                    Source = Environment.MachineName,
                    Message = GetRandomMessageForTest(DateTime.UtcNow.Millisecond)
                });
            }

            logger.SendLogEntries(auditEntities, "customaudit").Wait();
        }

        private string GetSeverity(int seed)
        {
            var severities = new[] { "Critical", "Error", "Warning", "Informational", "Verbose" };
            int rnd = new Random(DateTime.UtcNow.Millisecond + seed).Next(0, severities.Length);

            return severities[rnd];
        }
        private string GetRandomMessageForTest(int seed)
        {
            var messages = new[]
            {
                "A new event has been escalated in MyApp123",
                "Escalated Privileges Detected during Custom Application Activities",
                "System has detected an anomaly in the magical unicorn gardens",
                "Apples are healthier than potato chips.",
            };
            int rnd = new Random(DateTime.UtcNow.Millisecond + seed).Next(0, messages.Length);

            return messages[rnd];
        }
    }
}
