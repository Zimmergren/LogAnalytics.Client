using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogAnalytics.DataCollector.Wrapper
{
    interface ILogAnalyticsWrapper
    {
        Task SendLogEntry<T>(T entity, string logType);
        Task SendLogEntries<T>(List<T> entities, string logType);
    }
}
