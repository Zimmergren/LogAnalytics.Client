using LogAnalytics.Client.Tests.Helpers;
using LogAnalytics.Client.Tests.SentinelDemoEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace LogAnalytics.Client.Tests
{
    [TestClass]
    public class SentinelLawDemoTests : TestsBase
    {
        private static TestSecrets _secrets;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _secrets = InitSecrets();
        }

        [TestMethod]
        public void SendDemoEvents_Test()
        {
            LogAnalyticsClient logger = new LogAnalyticsClient(
                workspaceId: _secrets.LawSecrets.LawId,
                sharedKey: _secrets.LawSecrets.LawKey);

            // SECURITY EVENTS (Sample/Demo code only)
            List<CustomSecurityEvent> securityEntities = new List<CustomSecurityEvent>();
            int securityRandom = RandomNumberGenerator.GetInt32(100, 12000); // amount of randomized log events to ship.
            for (int ii = 0; ii < securityRandom; ii++)
            {
                securityEntities.Add(new CustomSecurityEvent
                {
                    Severity = GetSeverity(),
                    Source = Environment.MachineName,
                    Message = GetRandomMessageForTest()
                });
            }

            logger.SendLogEntries(securityEntities, "customsecurity").Wait();

            // AUDIT EVENTS (Sample/Demo code only)
            List<CustomAuditEvent> auditEntities = new List<CustomAuditEvent>();
            int auditRandom = RandomNumberGenerator.GetInt32(250, 5000); ; // amount of randomized log events to ship.
            for (int ii = 0; ii < auditRandom; ii++)
            {
                auditEntities.Add(new CustomAuditEvent
                {
                    Severity = GetSeverity(),
                    Source = Environment.MachineName,
                    Message = GetRandomMessageForTest()
                });
            }

            logger.SendLogEntries(auditEntities, "customaudit").Wait();
        }

        private string GetSeverity()
        {
            var severities = new[] { "Critical", "Error", "Warning", "Informational", "Verbose" };

            int rnd = RandomNumberGenerator.GetInt32(0, severities.Length);

            return severities[rnd];
        }
        private string GetRandomMessageForTest()
        {
            var messages = new[]
            {
                "A new event has been escalated in MyApp123",
                "Escalated Privileges Detected during Custom Application Activities",
                "System has detected an anomaly in the magical unicorn gardens",
                "Apples are healthier than potato chips.",
            };
            int rnd = RandomNumberGenerator.GetInt32(0, messages.Length);

            return messages[rnd];
        }
    }
}
