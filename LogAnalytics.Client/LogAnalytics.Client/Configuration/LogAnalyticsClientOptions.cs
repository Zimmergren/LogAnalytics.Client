namespace LogAnalytics.Client.Configuration
{
    /// <summary>
    /// Log Analytics Client Options.
    /// Defines settings for the Log Analytics Client.
    /// </summary>
    public class LogAnalyticsClientOptions
    {
        /// <summary>
        /// Gets or sets the Workspace ID.
        /// </summary>
        public string WorkspaceId { get; set; }

        /// <summary>
        /// Gets or sets the Shared Key.
        /// </summary>
        public string SharedKey { get; set; }

        /// <summary>
        /// Gets or sets the Azure Sovereign Cloud.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <term>ods.opinsights.azure.com</term>
        /// <description>(Default) The Azure Public Cloud.</description>
        /// </item>
        /// <item>
        /// <term>ods.opinsights.azure.us</term>
        /// <description>Azure Government.</description>
        /// </item>
        /// <item>
        /// <term>ods.opinsights.azure.cn</term>
        /// <description>Azure China.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public string EndPointOverride { get; set; }
    }
}
