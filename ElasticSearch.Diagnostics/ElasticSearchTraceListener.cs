using Elasticsearch.Net;
using ElasticSearch.Diagnostics.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

//using Elasticsearch.Net.Connection;

namespace ElasticSearch.Diagnostics
{
    /// <summary>
    /// A TraceListener class used to submit trace data to elasticsearch
    /// </summary>
    public class ElasticSearchTraceListener : TraceListenerBase
    {
        private const string DocumentType = "doc";

        private readonly BlockingCollection<JObject> _queueToBePosted = new BlockingCollection<JObject>();

        private IElasticLowLevelClient _client;

        private string _userDomainName;
        private string _userName;

        /// <summary>
        /// Uri for the ElasticSearch server
        /// </summary>
        private Uri Uri { get; set; }

	    private string ElasticSearchTraceIndex { get; set; }
	    private string ElasticSearchIndexDatePattern { get; set; }



		/// <summary>
		/// prefix for the Index for traces
		/// </summary>
		private string Index => this.ElasticSearchTraceIndex.ToLower() + "-" + DateTime.UtcNow.ToString(this.ElasticSearchIndexDatePattern);


        private static readonly string[] _supportedAttributes = new string[]
        {
            "ElasticSearchUri", "elasticSearchUri", "elasticsearchuri",
            "ElasticSearchTraceIndex", "elasticSearchTraceIndex", "elasticsearchtraceindex",

            //this attribute is to be removed next minor release
            "ElasticSearchIndex", "elasticSearchIndex", "elasticsearchindex",

        };

        /// <summary>
        /// Allowed attributes for this trace listener.
        /// </summary>
        protected override string[] GetSupportedAttributes()
        {
            return _supportedAttributes;
        }


	    public override object InitializeLifetimeService()
	    {
		    return base.InitializeLifetimeService();
	    }


	    /// <summary>
        /// Uri for the ElasticSearch server
        /// </summary>
        public string ElasticSearchUriAttribute
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
                    throw new ArgumentException($"{nameof(ElasticSearchUri)} attribute is not defined");
                }
            }
            set
            {
                Attributes["elasticsearchuri"] = value;
            }
        }

        /// <summary>
        /// Uri for the ElasticSearch server
        /// </summary>
        public string ElasticSearchIndexDatePatternAttribute
		{
            get
            {
                if (Attributes.ContainsKey("elasticsearchindexdatepattern"))
                {
                    return Attributes["elasticsearchindexdatepattern"];
                }
                else
                {
                    return DEFAULT_DATE_PATTERN;
                }
            }
            set
            {
                Attributes["elasticsearchindexdatepattern"] = value;
            }
        }

        /// <summary>
        /// prefix for the Index for traces
        /// </summary>
        public string ElasticSearchTraceIndexAttribute
		{
            get
            {
                if (Attributes.ContainsKey("elasticsearchtraceindex"))
                {
                    return Attributes["elasticsearchtraceindex"];
                }
                else
                {
	                return @"trace";
                }
            }
            set
            {
                Attributes["elasticsearchtraceindex"] = value;
            }
        }

		/// <summary>
		/// BufferSize for number of traces to buffer
		/// </summary>
		public int BufferSizeAttribute
		{
		    get
		    {
			    if (Attributes.ContainsKey("buffersize"))
			    {
				    return int.Parse( Attributes["buffersize"]);
			    }
			    else
			    {
				    return BUFFER_SIZE;
			    }
		    }
		    set
		    {
			    Attributes["buffersize"] = value.ToString();
		    }
	    }

		/// <summary>
		/// BufferSize for number of traces to buffer
		/// </summary>
		public int BufferWaitSecondsAttribute
		{
		    get
		    {
			    if (Attributes.ContainsKey("bufferwaitseconds"))
			    {
				    return int.Parse( Attributes["bufferwaitseconds"]);
			    }
			    else
			    {
				    return BUFFER_WAIT_SECONDS;
			    }
		    }
		    set
		    {
			    Attributes["bufferwaitseconds"] = value.ToString();
		    }
	    }


        /// <summary>
        /// Gets a value indicating the trace listener is thread safe.
        /// </summary>
        /// <value>true</value>
        public override bool IsThreadSafe => true;

        public IElasticLowLevelClient Client
        {
            get
            {
                if (_client != null)
                {
                    return _client;
                }
                else
                {
                    Uri = new Uri(this.ElasticSearchUriAttribute);

					//Index = this.ElasticSearchTraceIndex.ToLower() + "-" + DateTime.UtcNow.ToString("yyyy-MM-dd");
					//var cs = new ConnectionSettings(Uri);
					//cs.ExposeRawResponse();
					//cs.ThrowOnElasticsearchServerExceptions();

					var singleNode = new SingleNodeConnectionPool(Uri);

	                var cc = new ConnectionConfiguration(singleNode,
			                connectionSettings => new ElasticsearchJsonNetSerializer())
		                .EnableHttpPipelining()
		                .ThrowExceptions();

					//the 1.x serializer we needed to use, as the default SimpleJson didnt work right
					//Elasticsearch.Net.JsonNet.ElasticsearchJsonNetSerializer()

	                this._client = new ElasticLowLevelClient(cc);
                    return this._client;
                }
            }
        }

	    /// <summary>
        /// We cant grab any of the attributes until the class and more importantly its base class has finsihed initializing
        /// so keep the constructor at a minimum
        /// </summary>
        public ElasticSearchTraceListener() : base()
	    {
	        _userDomainName = Environment.UserDomainName;
	        _userName = Environment.UserName;
	        _machineName = Environment.MachineName;
            Initialize();
        }

        /// <summary>
        /// We cant grab any of the attributes until the class and more importantly its base class has finsihed initializing
        /// so keep the constructor at a minimum
        /// </summary>
        public ElasticSearchTraceListener(string name) : base(name)
        {
            _userDomainName = Environment.UserDomainName;
            _userName = Environment.UserName;
            _machineName = Environment.MachineName;
            Initialize();
        }

        private void Initialize()
        {
	        var bufferSize = this.BufferSizeAttribute;
	        var bufferTime = TimeSpan.FromSeconds(this.BufferWaitSecondsAttribute);

	        this.ElasticSearchTraceIndex = this.ElasticSearchTraceIndexAttribute;
	        this.ElasticSearchIndexDatePattern = this.ElasticSearchIndexDatePatternAttribute;

			//test the formatter, and blow
	        var test = this.Index;

			//TODO - make sure this is a valid

            //SetupObserver();
            SetupObserverBatchy(bufferTime, bufferSize);
        }

        private Action<JObject> _scribeProcessor;
        private string _machineName;

        private void SetupObserver()
        {
            _scribeProcessor = a => WriteDirectlyToES(a);

            //this._queueToBePosted.GetConsumingEnumerable()
            //.ToObservable(Scheduler.Default)
            //.Subscribe(x => WriteDirectlyToES(x));
        }


        private void SetupObserverBatchy(TimeSpan waittime, int size)
        {
            _scribeProcessor = a => WriteToQueueForprocessing(a);

            this._queueToBePosted.GetConsumingEnumerable()
                .ToObservable(Scheduler.Default)
                .Buffer(waittime, size)
                .Subscribe(async x => await this.WriteDirectlyToESAsBatch(x));
        }



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

            if (eventCache != null && eventCache.Callstack.Contains(nameof(Elasticsearch.Net.ElasticLowLevelClient)))
            {
                return;
            }

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
                else if (data is DateTime)
                {
                    payload = new JObject();
                    payload.Add("System.DateTime", (DateTime)data);
                }
                else if (data is string)
                {
                    payload = new JObject();
                    payload.Add("string", (string)data);
                }
                else if (data.GetType().IsValueType)
                {
                    payload = new JObject { { "data", data.ToString() } };
                }
                else
                {
                    try
                    {
                        payload = JObject.FromObject(data);
                    }
                    catch(JsonSerializationException jEx)
                    {
                        payload = new JObject();
                        payload.Add("FAILURE", jEx.Message);
                        payload.Add("data", data.GetType().ToString());
                    }
                }
            }

            //Debug.Assert(!string.IsNullOrEmpty(updatedMessage));
            //Debug.Assert(payload != null);

            InternalWrite(eventCache, source, eventType, id, updatedMessage, relatedActivityId, payload);
        }

        private void InternalWrite(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int? traceId,
            string message,
            Guid?
            relatedActivityId,
            JObject dataObject)
        {

            //var timeStamp = DateTime.UtcNow.ToString("o");
            //var source = Process.GetCurrentProcess().ProcessName;
            //var stacktrace = Environment.StackTrace;
            //var methodName = (new StackTrace()).GetFrame(StackTrace.METHODS_TO_SKIP + 4).GetMethod().Name;


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




            IPrincipal principal = Thread.CurrentPrincipal;
            IIdentity identity = principal?.Identity;
            string identityname = identity == null ? string.Empty : identity.Name;

            
            string username = $"{_userDomainName}\\{_userName}";

            try
            {
                var jo = new JObject
                    {
                        {"Source", source },
                        {"TraceId", traceId ?? 0},
                        {"EventType", eventType.ToString()},
                        {"UtcDateTime", logTime},
                        {"timestamp", eventCache?.Timestamp ?? 0},
                        {"MachineName", _machineName},
                        {"AppDomainFriendlyName", AppDomain.CurrentDomain.FriendlyName},
                        {"ProcessId", eventCache?.ProcessId ?? 0},
                        {"ThreadName", thread},
                        {"ThreadId", threadId},
                        {"Message", message},
                        {"ActivityId", Trace.CorrelationManager.ActivityId != Guid.Empty ? Trace.CorrelationManager.ActivityId.ToString() : string.Empty},
                        {"RelatedActivityId", relatedActivityId.HasValue ? relatedActivityId.Value.ToString() : string.Empty},
                        {"LogicalOperationStack", logicalOperationStack},
                        {"Data", dataObject},
                        {"Username", username},
                        {"Identityname", identityname},
                    };

                _scribeProcessor(jo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async Task WriteDirectlyToES(JObject jo)
        {
	        try
	        {
                await Client.IndexAsync<VoidResponse>(Index, "Trace", jo.ToString());
	        }
	        catch (Exception ex)
	        {
		        Debug.WriteLine(ex);
	        }
		}

        private async Task WriteDirectlyToESAsBatch(IEnumerable<JObject> jos)
        {
            if (!jos.Any())
                return;

            var indx = new { index = new { _index = Index, _type = "Trace" } };
            var indxC = Enumerable.Repeat(indx, jos.Count());

            var bb = jos.Zip(indxC, (f, s) => new object[] { s, f });
            var bbo = bb.SelectMany(a => a);

            try
            {
	            await Client.BulkPutAsync<VoidResponse>(Index, "Trace", bbo.ToArray(), br => br.Refresh(false));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void WriteToQueueForprocessing(JObject jo)
        {
            this._queueToBePosted.Add(jo);
        }


        /// <summary>
        /// removing the spin flush
        /// </summary>
        public override void Flush()
        {
            //check to make sure the "queue" has been emptied
            //while (this._queueToBePosted.Count() > 0)            { }
            base.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            this._queueToBePosted.Dispose();
            base.Flush();
            base.Dispose(disposing);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
