using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LogAnalytics.Client
{
    public static class DependencyInjectionExtensions
    {
        private static IServiceCollection AddLogAnalyticsClient(this IServiceCollection services)
        {
            services.AddHttpClient<LogAnalyticsClient>();
            return services;
        }

        /// <summary>
        /// Add Log Analytics Client to the service container using HTTP Client Factory.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configureAction">The action used to configure the options.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddLogAnalyticsClient(this IServiceCollection services, Action<LogAnalyticsClientOptions> configureAction)
        {
            services.Configure(configureAction);
            return services.AddLogAnalyticsClient();
        }

        /// <summary>
        /// Add Log Analytics Client to the service container using HTTP Client Factory.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configuration">Configuration or configuration section.</param>
        /// <returns></returns>
        public static IServiceCollection AddLogAnalyticsClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<LogAnalyticsClientOptions>().Bind(configuration);
            return services.AddLogAnalyticsClient();
        }
    }
}
