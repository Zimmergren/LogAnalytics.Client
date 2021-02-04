using LogAnalytics.Client.UnitTests.TestEntities;
using System;
using System.Collections.Generic;
using Xunit;

namespace LogAnalytics.Client.IntegrationTests.UnitTests
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

        [Theory]
        [InlineData("bXlTaGFyZWRLZXk==="), InlineData("bXlTaGFyZWRLZXk"), InlineData("cats1")]
        public void LogAnalyticsClient_InitializationThrowsArgumentNullException_IfSharedKeyIsNotBase64(string sharedKey)
        {
            // Arrange
            // Act
            void act() => new LogAnalyticsClient("workspace id", sharedKey);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void SendLogEntry_ThrowsArgumentNullException_IfEntityIsNull()
        {
            // Arrange.
            LogAnalyticsClient client = new LogAnalyticsClient("id", "bXlTaGFyZWRLZXk=");

            // Act.
            void act() => client.SendLogEntry<ValidTestEntity>(null, "logtype");

            // Assert.
            Assert.Throws<ArgumentNullException>(act);
        }

        [Fact]
        public void SendLogEntries_ThrowsAggregateException_IfEntityIsNull()
        {
            // Arrange.
            LogAnalyticsClient client = new LogAnalyticsClient("id", "bXlTaGFyZWRLZXk=");

            // Act.
            void act() => client.SendLogEntries<ValidTestEntity>(null, "logtype").Wait();

            // Assert.
            Assert.Throws<AggregateException>(act);
        }

        [Fact]
        public void SendLogEntry_ThrowsArgumentOutOfRangeException_IfLogTypeLengthIsExceeded()
        {
            // Arrange.
            LogAnalyticsClient client = new LogAnalyticsClient("id", "bXlTaGFyZWRLZXk=");

            // Act.
            void act() => client.SendLogEntry<ValidTestEntity>(new ValidTestEntity(), "aaaaaaaaaabbbbbbbbbbccccccccccddddddddddeeeeeeeeeeaaaaaaaaaabbbbbbbbbbccccccccccddddddddddeeeeeeeeeeZ");

            // Assert.
            Assert.Throws<ArgumentOutOfRangeException>(act);
        }

        [Fact]
        public void SendLogEntries_ThrowsArgumentOutOfRangeException_IfLogTypeLengthIsExceeded()
        {
            // Arrange.
            LogAnalyticsClient client = new LogAnalyticsClient("id", "bXlTaGFyZWRLZXk=");

            // Act.
            void act() => client.SendLogEntries<ValidTestEntity>(new List<ValidTestEntity>() { new ValidTestEntity() }, "aaaaaaaaaabbbbbbbbbbccccccccccddddddddddeeeeeeeeeeaaaaaaaaaabbbbbbbbbbccccccccccddddddddddeeeeeeeeeeZ").Wait();

            // Assert.
            Assert.Throws<AggregateException>(act);            
        }

        [Theory]
        [
            InlineData("ö"), 
            InlineData("~"), 
            InlineData("!"), 
            InlineData("@"), 
            InlineData("."), 
            InlineData("l-o-g"),
            InlineData("l_o-g"),
            InlineData("l_o-1"),
            InlineData("1_o-."),
            InlineData("1.log"),
            InlineData("log.1")

        ]
        public void SendLogEntry_ThrowsArgumentOutOfRangeException_IfLogTypeContainsAnythingExceptAlphanumericOrUnderscoreCharacters(string logType)
        {
            // Arrange.
            LogAnalyticsClient client = new LogAnalyticsClient("id", "bXlTaGFyZWRLZXk=");

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
            LogAnalyticsClient client = new LogAnalyticsClient("id", "bXlTaGFyZWRLZXk=");

            // Act.
            void act() => client.SendLogEntry<ValidTestEntity>(new ValidTestEntity(), logType);

            // Assert.
            Assert.Throws<ArgumentNullException>(act);
        }

        [Fact]
        public void SendLogEntry_ThrowsArgumentOutOfRangeException_IfInvalidEntity()
        {
            // Arrange.
            LogAnalyticsClient client = new LogAnalyticsClient("id", "bXlTaGFyZWRLZXk=");

            // Act.
            void act() => client.SendLogEntry<InvalidTestEntity>(new InvalidTestEntity(), "logtype");

            // Assert.
            Assert.Throws<ArgumentOutOfRangeException>(act);
        }
    }
}
