using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogAnalytics.Client
{
    interface ILogAnalyticsClient
    {
        Task SendLogEntry<T>(T entity, string logType);
        Task SendLogEntries<T>(List<T> entities, string logType);
    }
}
