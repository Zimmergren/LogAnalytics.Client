using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogAnalytics.Client
{
    public interface ILogAnalyticsClient
    {
        Task SendLogEntry<T>(T entity, string logType, string resourceId = null, string timeGeneratedCustomFieldName = null);
        Task SendLogEntries<T>(List<T> entities, string logType, string resourceId = null, string timeGeneratedCustomFieldName = null);
    }
}
