using System.Collections.Generic;

namespace LogAnalytics.DataCollector.Wrapper
{
    interface ILogAnalyticsWrapper
    {
        void SendLogEntry<T>(T entity, string logType);
        void SendLogEntries<T>(List<T> entities, string logType);
    }
}
