# ElasticSearch.Diagnostics

### Pull request builds

[![Build status](https://ci.appveyor.com/api/projects/status/1let8gsvksxjv50c?svg=true)](https://ci.appveyor.com/project/amccool/elasticsearch-diagnostics)

### Master build

[![Build status](https://ci.appveyor.com/api/projects/status/1let8gsvksxjv50c/branch/master?svg=true)](https://ci.appveyor.com/project/amccool/elasticsearch-diagnostics/branch/master)


ElasticSearch TraceListener is a System.Diagnostics based TraceListener which submits trace events and data to ElasticSearch making them viewable withKibana

## Getting Started

[![NuGet Status](http://nugetstatus.com/ElasticSearch.Diagnostics.png)](http://nugetstatus.com/packages/ElasticSearch.Diagnostics)

Install the package from nu-get https://www.nuget.org/packages/ElasticSearch.Diagnostics/

Install-Package ElasticSearch.Diagnostics

edit your app.config/web.config

    <system.diagnostics>
        <sharedListeners>
            <add name="estl" type="ElasticSearch.Diagnostics.ElasticSearchTraceListener, ElasticSearch.Diagnostics"
                ElasticSearchUri="http://127.1.1.1:9200"
                ElasticSearchIndex="trace"
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

The kibana format of the index is <ElasticSearchTraceIndex>-yyyy-MM-dd-HH

## Usage

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



## Environmental setup

You need an ElasticSearch host, and likely you want Kibana to view the data

### setup your ElasticSearch host

try using docker

see https://elk-docker.readthedocs.io/

```bash
$ sudo docker run -p 5601:5601 -p 9200:9200 -p 5044:5044 -it --name elk sebp/elk
```

### Run your traces to generate some data

Elasticsearch and Kibana needs some data to get started with

### Configure ElasticSearch and Kibana

Note that `trace` was used for the index prefix

go the kibana managment page
http://192.168.1.1:5601/app/kibana#/management

click index patterns
http://192.168.1.1:5601/app/kibana#/management/kibana/indices

click +Add New
type in the prefix you used (see above) `trace` adding a `dash`

if you have data in ElasticSearch then it will display a Date field, which will be `UtcDateTime`
