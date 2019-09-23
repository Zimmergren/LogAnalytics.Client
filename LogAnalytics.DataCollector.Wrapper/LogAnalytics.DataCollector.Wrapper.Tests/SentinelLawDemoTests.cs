using System;
using System.Collections.Generic;
using LogAnalytics.DataCollector.Wrapper.Tests.SentinelDemoEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogAnalytics.DataCollector.Wrapper.Tests
{
    [TestClass]
    public class SentinelLawDemoTests
    {
        // You should grab these variables from Key Vault, 
        // Credential Store on your machine, 
        // or some other non-code related location that is defined by secure boundraries
        private string workspaceId = "<workspace id>";
        private string sharedKey = "<shared key>";

        [TestMethod]
        public void SendDemoEvents_Test()
        {
            LogAnalyticsWrapper logger = new LogAnalyticsWrapper(
                workspaceId: workspaceId,
                sharedKey: sharedKey);

            // SECURITY EVENTS (Sample/Demo code only)
            List<TZSecurityEvent> securityEntities = new List<TZSecurityEvent>();
            int securityRandom = new Random().Next(100, 12000); // amount of randomized log events to ship.
            for (int ii = 0; ii < securityRandom; ii++)
            {
                securityEntities.Add(new TZSecurityEvent
                {
                    Severity = GetSeverity(DateTime.UtcNow.Millisecond),
                    Source = Environment.MachineName,
                    Message = GetRandomMessageForTest(DateTime.UtcNow.Millisecond)
                });
            }

            logger.SendLogEntries(securityEntities, "tzsecurity");

            // AUDIT EVENTS (Sample/Demo code only)
            List<TZAuditEvent> auditEntities = new List<TZAuditEvent>();
            int auditRandom = new Random().Next(250, 5000); // amount of randomized log events to ship.
            for (int ii = 0; ii < auditRandom; ii++)
            {
                auditEntities.Add(new TZAuditEvent
                {
                    Severity = GetSeverity(DateTime.UtcNow.Millisecond),
                    Source = Environment.MachineName,
                    Message = GetRandomMessageForTest(DateTime.UtcNow.Millisecond)
                });
            }

            logger.SendLogEntries(auditEntities, "tzaudit");
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
