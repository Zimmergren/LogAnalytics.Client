using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogAnalytics.Client
{
    /// <summary>
    /// Interface for LogAnalyticsClient.
    /// </summary>
    public interface ILogAnalyticsClient
    {
        /// <summary>
        /// Send a typed object as a log entry.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="entity">The object.</param>
        /// <param name="logType">Log Type. This is the table name in Log Analytics.</param>
        /// <param name="resourceId">Optional. Connects this log entry with a resource in Azure, by the resource id.</param>
        /// <param name="timeGeneratedCustomFieldName">Optional. Defines a custom field that contains the time of the log entry.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task SendLogEntry<T>(T entity, string logType, string resourceId = null, string timeGeneratedCustomFieldName = null);

        /// <summary>
        /// Send a collection of typed objects as log entries.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="entities">The object.</param>
        /// <param name="logType">Log Type. This is the table name in Log Analytics.</param>
        /// <param name="resourceId">Optional. Connects this log entry with a resource in Azure, by the resource id.</param>
        /// <param name="timeGeneratedCustomFieldName">Optional. Defines a custom field that contains the time of the log entry.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task SendLogEntries<T>(List<T> entities, string logType, string resourceId = null, string timeGeneratedCustomFieldName = null);
    }
}
