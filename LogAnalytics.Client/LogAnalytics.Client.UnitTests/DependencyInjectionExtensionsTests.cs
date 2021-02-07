
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace LogAnalytics.Client.UnitTests
{
    public class DependencyInjectionExtensionsTests
    {
        [Fact]
        public void LogAnalyticsClient_ShouldCreateInstanceWithConfigurationAction()
        {
            // Arrange
            var provider = new ServiceCollection()
                .AddLogAnalyticsClient(c =>
            {
                c.WorkspaceId = "workspaceId";
                c.SharedKey = Convert.ToBase64String(Encoding.UTF8.GetBytes("sharredKey"));
            }).BuildServiceProvider();

            // Act
            var instance = provider.GetRequiredService<LogAnalyticsClient>();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void LogAnalyticsClient_ShouldCreateInstanceWithConfiguratinInstance()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "LogAnalytics:WorkspaceId", "workspaceId" },
                    { "LogAnalytics:SharedKey", Convert.ToBase64String(Encoding.UTF8.GetBytes("sharredKey")) }
                })
                .Build();

            var provider = new ServiceCollection()
                .AddLogAnalyticsClient(configuration.GetSection("LogAnalytics"))
                .BuildServiceProvider();
            // Act
            var instance = provider.GetRequiredService<LogAnalyticsClient>();

            // Assert
            Assert.NotNull(instance);
        }
    }
}
