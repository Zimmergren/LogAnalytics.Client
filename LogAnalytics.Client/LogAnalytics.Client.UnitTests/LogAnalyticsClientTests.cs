using LogAnalytics.Client.UnitTests.TestEntities;
using System;
using System.Collections.Generic;
using Xunit;

namespace LogAnalytics.Client.Tests.UnitTests
{
    public class LogAnalyticsClientTests
    {
        [Theory]
        [InlineData(null),InlineData("")]
        public void LogAnalyticsClient_InitializationThrowsArgumentNullException_IfWorkspaceIdIsEmpty(string workspaceId)
        {
            // Arrange
            // Act
            void act() => new LogAnalyticsClient(workspaceId, "sharedKey");

            // Assert
            Assert.Throws<ArgumentNullException>(act);
        }

        [Theory]
        [InlineData(null), InlineData("")]
        public void LogAnalyticsClient_InitializationThrowsArgumentNullException_IfSharedKeyIsEmpty(string sharedKey)
        {
            // Arrange
            // Act
            void act() => new LogAnalyticsClient("workspace id", sharedKey);

            // Assert
            Assert.Throws<ArgumentNullException>(act);
        }

        [Fact]
        public void SendLogEntry_ThrowsArgumentNullException_IfEntityIsNull()
        {
            // Arrange.
            LogAnalyticsClient client = new LogAnalyticsClient("id", "key");

            // Act.
            void act() => client.SendLogEntry<ValidTestEntity>(null, "logtype");

            // Assert.
            Assert.Throws<ArgumentNullException>(act);
        }

        [Fact]
        public void SendLogEntries_ThrowsAggregateException_IfEntityIsNull()
        {
            // Arrange.
            LogAnalyticsClient client = new LogAnalyticsClient("id", "key");

            // Act.
            void act() => client.SendLogEntries<ValidTestEntity>(null, "logtype").Wait();

            // Assert.
            Assert.Throws<AggregateException>(act);
        }

        [Fact]
        public void SendLogEntry_ThrowsArgumentOutOfRangeException_IfLogTypeLengthIsExceeded()
        {
            // Arrange.
            LogAnalyticsClient client = new LogAnalyticsClient("id", "key");

            // Act.
            void act() => client.SendLogEntry<ValidTestEntity>(new ValidTestEntity(), "aaaaaaaaaabbbbbbbbbbccccccccccddddddddddeeeeeeeeeeaaaaaaaaaabbbbbbbbbbccccccccccddddddddddeeeeeeeeeeZ");

            // Assert.
            Assert.Throws<ArgumentOutOfRangeException>(act);
        }

        [Fact]
        public void SendLogEntries_ThrowsArgumentOutOfRangeException_IfLogTypeLengthIsExceeded()
        {
            // Arrange.
            LogAnalyticsClient client = new LogAnalyticsClient("id", "key");

            // Act.
            Action act = () => client.SendLogEntries<ValidTestEntity>(new List<ValidTestEntity>() { new ValidTestEntity() }, "aaaaaaaaaabbbbbbbbbbccccccccccddddddddddeeeeeeeeeeaaaaaaaaaabbbbbbbbbbccccccccccddddddddddeeeeeeeeeeZ").Wait();

            // Assert.
            Assert.Throws<AggregateException>(act);            
        }

        [Theory]
        [InlineData("ö"), InlineData("~"), InlineData("!"), InlineData("@")]
        public void SendLogEntry_ThrowsArgumentOutOfRangeException_IfLogTypeContainsNonAlphanumericCharacters(string logType)
        {
            // Arrange.
            LogAnalyticsClient client = new LogAnalyticsClient("id", "key");

            // Act.
            void act() => client.SendLogEntry<ValidTestEntity>(new ValidTestEntity(), logType);

            // Assert.
            Assert.Throws<ArgumentOutOfRangeException>(act);
        }

        [Theory]
        [InlineData(""), InlineData(null)]
        public void SendLogEntry_ThrowsArgumentOutOfRangeException_IfLogTypeIsNullOrEmpty(string logType)
        {
            // Arrange.
            LogAnalyticsClient client = new LogAnalyticsClient("id", "key");

            // Act.
            void act() => client.SendLogEntry<ValidTestEntity>(new ValidTestEntity(), logType);

            // Assert.
            Assert.Throws<ArgumentNullException>(act);
        }

        [Fact]
        public void SendLogEntry_ThrowsArgumentOutOfRangeException_IfInvalidEntity()
        {
            // Arrange.
            LogAnalyticsClient client = new LogAnalyticsClient("id", "key");

            // Act.
            void act() => client.SendLogEntry<InvalidTestEntity>(new InvalidTestEntity(), "logtype");

            // Assert.
            Assert.Throws<ArgumentOutOfRangeException>(act);
        }
    }
}
