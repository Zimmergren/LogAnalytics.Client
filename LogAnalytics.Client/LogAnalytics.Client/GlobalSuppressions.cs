// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "SecurityIntelliSenseCS:MS Security rules violation", Justification = "Protocol is always HTTPS. This is a false-positive.", Scope = "member", Target = "~M:LogAnalytics.Client.LogAnalyticsClient.SendLogEntries``1(System.Collections.Generic.List{``0},System.String,System.String,System.String)~System.Threading.Tasks.Task")]
