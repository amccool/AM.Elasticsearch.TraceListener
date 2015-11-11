using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Elasticsearch.Net;

using Nest;

using Newtonsoft.Json.Linq;
using System.Threading;
using System.Xml.XPath;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Xml.Linq;
using System.Xml;
using System.Dynamic;

namespace ElasticSearch.Diagnostics
{
    public class ElasticSearchTraceListener : TraceListenerBase
    {
        private ElasticsearchClient _client;

        public Uri Uri { get; private set; }
        public string Index { get; private set; }


        private static readonly string[] _supportedAttributes = new string[]
            {
                "ElasticSearchUri", "elasticSearchUri", "elasticsearchuri",
                "ElasticSearchIndex", "elasticSearchIndex", "elasticsearchindex",
                "ElasticSearchTraceIndex", "elasticSearchTraceIndex", "elasticsearchtraceindex",
            };

        /// <summary>
        /// Allowed attributes for this trace listener.
        /// </summary>
        protected override string[] GetSupportedAttributes()
        {
            return _supportedAttributes;
        }


        public string ElasticSearchUri
        {
            get
            {
                if (Attributes.ContainsKey("elasticsearchuri"))
                {
                    return Attributes["elasticsearchuri"];
                }
                else
                {
                    //return _defaultTemplate;
                    throw new ArgumentException("elasticsearchuri attribute is not defined");
                }
            }
            set
            {
                Attributes["elasticsearchuri"] = value;
            }
        }

        public string ElasticSearchIndex
        {
            get
            {
                if (Attributes.ContainsKey("elasticsearchindex"))
                {
                    return Attributes["elasticsearchindex"];
                }
                else
                {
                    //return _defaultTemplate;
                    throw new ArgumentException("elasticsearchindex attribute is not defined");
                }
            }
            set
            {
                Attributes["elasticsearchindex"] = value;
            }
        }


        public string ElasticSearchTraceIndex
        {
            get
            {
                if (Attributes.ContainsKey("elasticsearchtraceindex"))
                {
                    return Attributes["elasticsearchtraceindex"];
                }
                else
                {
                    //return _defaultTemplate;
                    throw new ArgumentException("elasticsearchtraceindex attribute is not defined");
                }
            }
            set
            {
                Attributes["elasticsearchtraceindex"] = value;
            }
        }



        /// <summary>
        /// Gets a value indicating the trace listener is thread safe.
        /// </summary>
        /// <value>true</value>
        public override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }

        public ElasticsearchClient Client
        {
            get
            {
                if (_client != null)
                {
                    return _client;
                }
                else
                {
                    Uri = new Uri(this.ElasticSearchUri);

                    Index = this.ElasticSearchTraceIndex.ToLower() + "-" + DateTime.UtcNow.ToString("yyyy-MM-dd");

                    this._client = new ElasticsearchClient(new ConnectionSettings(Uri));
                    return this._client;

                }
            }
        }

        /// <summary>
        /// We cant grab any of the attributes until the class and more importantly its base class has finsihed initializing
        /// so keep the constructor at a minimum
        /// </summary>
        public ElasticSearchTraceListener() : base()
        { }



        /// <summary>
        /// We cant grab any of the attributes until the class and more importantly its base class has finsihed initializing
        /// so keep the constructor at a minimum
        /// </summary>
        public ElasticSearchTraceListener(string name) : base(name)
        { }






        /// <summary>
        /// Write trace event with data.
        /// </summary>
        protected override void WriteTrace(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id,
            string message,
            Guid? relatedActivityId,
            object data)
        {

            if (eventCache != null && eventCache.Callstack.Contains("Elasticsearch.Net.ElasticsearchClient"))
            {
                return;
            }



            //if ((!string.IsNullOrEmpty(source)) && source.Contains("System.Net")
            //    &&
            //    (eventType <= TraceEventType.Verbose)
            //    )
            //{
            //    return;
            //}






            string updatedMessage = message;
            JObject payload = null;
            if (data != null)
            {
                if (data is Exception)
                {
                    updatedMessage = ((Exception)data).Message;
                    payload = JObject.FromObject(data);
                }
                else if (data is XPathNavigator)
                {
                    var xdata = data as XPathNavigator;
                    //xdata.MoveToRoot();

                    XDocument xmlDoc;
                    try
                    {
                        xmlDoc = XDocument.Parse(xdata.OuterXml);

                    }
                    catch (Exception)
                    {
                        xmlDoc = XDocument.Parse(xdata.ToString());
                        //eat
                        //throw;
                    }

                    // Convert the XML document in to a dynamic C# object.
                    dynamic xmlContent = new ExpandoObject();
                    ExpandoObjectHelper.Parse(xmlContent, xmlDoc.Root);

                    string json = JsonConvert.SerializeObject(xmlContent);
                    payload = JObject.Parse(json);
                }
                else
                {
                    payload = JObject.FromObject(data);
                }
            }

            //Debug.Assert(!string.IsNullOrEmpty(updatedMessage));
            //Debug.Assert(payload != null);

            InternalWrite(eventCache, source, eventType, id, updatedMessage, relatedActivityId, payload);
        }

        private async void InternalWrite(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int? id,
            string message,
            Guid?
            relatedActivityId,
            JObject dataObject)
        {

            var timeStamp = DateTime.UtcNow.ToString("o");
            //var source = Process.GetCurrentProcess().ProcessName;
            var stacktrace = Environment.StackTrace;
            var methodName = (new StackTrace()).GetFrame(StackTrace.METHODS_TO_SKIP + 4).GetMethod().Name;


            DateTime logTime;
            string logicalOperationStack = null;
            if (eventCache != null)
            {
                logTime = eventCache.DateTime.ToUniversalTime();
                if (eventCache.LogicalOperationStack != null && eventCache.LogicalOperationStack.Count > 0)
                {
                    StringBuilder stackBuilder = new StringBuilder();
                    foreach (object o in eventCache.LogicalOperationStack)
                    {
                        if (stackBuilder.Length > 0) stackBuilder.Append(", ");
                        stackBuilder.Append(o);
                    }
                    logicalOperationStack = stackBuilder.ToString();
                }
            }
            else
            {
                logTime = DateTimeOffset.UtcNow.UtcDateTime;
            }

            string threadId = eventCache != null ? eventCache.ThreadId : string.Empty;
            string thread = Thread.CurrentThread.Name ?? threadId;

            try
            {
                await Client.IndexAsync(Index, "Trace", 
                    new JObject
                    {
                        { "Source", source },
                        {"Id", id ?? 0},
                        {"EventType", eventType.ToString()},
                        {"UtcDateTime", logTime},
                        {"timestamp", eventCache != null ? eventCache.Timestamp : 0},
                        {"MachineName", Environment.MachineName},
                        {"AppDomainFriendlyName", AppDomain.CurrentDomain.FriendlyName},
                        {"ProcessId", eventCache != null ? eventCache.ProcessId : 0},
                        {"ThreadName", thread},
                        {"ThreadId", threadId},
                        {"Message", message},
                        {"ActivityId", Trace.CorrelationManager.ActivityId != Guid.Empty ? Trace.CorrelationManager.ActivityId.ToString() : string.Empty},
                        {"RelatedActivityId", relatedActivityId.HasValue ? relatedActivityId.Value.ToString() : string.Empty},
                        {"LogicalOperationStack", logicalOperationStack},
                        {"Data", dataObject},
                    }.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
