# ElasticSearch.Diagnostics

ElasticSearch TraceListener is a System.Diagnostics based TraceListener which submits trace events and data to ElasticSearch making them viewable withKibana

##Getting Started

Install the package from nu-get http://www.nuget.org/packages/ElasticSearch.Diagnostics/

Install-Package ElasticSearch.Diagnostics.TraceListener

edit your app.config/web.config

    <system.diagnostics>
        <sharedListeners>
            <add name="estl" type="ElasticSearch.Diagnostics.ElasticSearchTraceListener, ElasticSearch.Diagnostics"
                ElasticSearchUri="http://127.1.1.1:9200"
                ElasticSearchIndex="trace"
                ElasticSearchTraceIndex="trace"
            />
        </sharedListeners>
        <trace autoflush="false" indentsize="4">
          <listeners>
            <!--<remove name="Default" />-->
            <add name="estl" />
          </listeners>
        </trace>
    <sources>
      <source name="MY-SILLY-TRACESOURCE" switchValue="All">
        <listeners>
          <add name="estl" />
        </listeners>
      </source>
      <source name="System.Net" switchValue="All">
        <listeners>
          <add name="estl" />
        </listeners>
      </source>
      <source name="System.ServiceModel" switchValue="All">
        <listeners>
          <add name="estl" />
        </listeners>
      </source>
    </sources>
    </system.diagnostics>

The kibana format of the index is <ElasticSearchTraceIndex>-YYYY-MM-DD

##Usage

    Trace.Write("sdgsgsgsgsgsgsgsgsgsgsg");
    
    class MyBigFatGreekClass
    {
        private static readonly TraceSource _traceSource = new TraceSource("alextrace", SourceLevels.Error);
        public void LoveAndMarriage()
        {
            _traceSource.TraceEvent(TraceEventType.Verbose, 0, "is {0} and all is well", DateTime.UtcNow);
            int x = 0;
            try
            {
                int y = 99999 / x;
            }
            catch(Exception ex)
            {
                _traceSource.TraceData(TraceEventType.Error, 119999911, ex);
            }
        }
        
        public void DeathAndTaxes()
        {
            _traceSource.TraceData(TraceEventType.Warning, 119999911, DateTime.UtcNow);
        }
    }
