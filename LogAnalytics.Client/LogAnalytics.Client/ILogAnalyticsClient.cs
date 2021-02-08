using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogAnalytics.Client
{
    public interface ILogAnalyticsClient
    {
        Task SendLogEntry<T>(T entity, string logType, string resourceId, string timeGeneratedCustomFieldName);
        Task SendLogEntries<T>(List<T> entities, string logType, string resourceId, string timeGeneratedCustomFieldName);
    }
}
