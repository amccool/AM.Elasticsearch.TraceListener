using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Elasticsearch.TraceListener
{
    /// <summary>
    /// it would be nice to use this class for serialization but alas that was not to be
    /// </summary>
    class TraceEntry
    {
        public string Source { get; set; }
        public int TraceId { get; set; }
        public string EventType { get; set; }
        public DateTime UtcDateTime { get; set; }
        public long timestamp { get; set; }
        public string MachineName { get; set; }
        public string AppDomainFriendlyName { get; set; }
        public int ProcessId { get; set; }
        public string ThreadName { get; set; }
        public string ThreadId { get; set; }
        public string Message { get; set; }
        public string ActivityId { get; set; }
        public string RelatedActivityId { get; set; }
        public string LogicalOperationStack { get; set; }
        public string Data { get; set; }
    }
}
