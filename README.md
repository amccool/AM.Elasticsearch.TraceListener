# ElasticSearch.Diagnostics

ElasticSearch TraceListener is a System.Diagnostics based TraceListener which submits trace events and data to ElasticSearch making them viewable withKibana

##Getting Started

Install the package from nu-get http://www.nuget.org/packages/ElasticSearch.Diagnostics/

And add the following settings to your application

    <add key="ElasticSearchUri" value="http://127.0.0.1:9200" />
    <add key="ElasticSearchIndex" value="elknet" />
    <add key="ElasticSearchTraceIndex" value="elknet" />

***Please note: Elk.NET automaticly appends the date to the index.***

The kibana format of the index is [elknet-]YYYY-MM-DD

##Usage

###Exceptions

Now that Elk.NET is configured you can simply use it like

    try
    {
        MethodThrowingError();
    }
    catch (Exception ex)
    {
        ElkLog.Instance.Debug(ex);
    }

This will log the error to Kibana.

###Trace

To write a trace message to Kibana add te following to the **configuration** section in your config

    <system.diagnostics>
        <trace autoflush="false" indentsize="4">
          <listeners>
            <add name="ElkNET" type="Elk.NET.ElkTraceListner, Elk.Net" />
            <remove name="Default" />
          </listeners>
        </trace>
      </system.diagnostics>
      
Now when to add a trace message to Kiban do the following

    public void Method()
    {
        Trace.Write("Method Start");
        
        // Do Stuff
        
        Trace.Write(Method End");
    }

##Kibana

Open the your Kibana GUI en use the following config file:

[kibana.json][1]


  [1]: https://raw.githubusercontent.com/pmdevers/Elk.NET/master/Elk.NET.Example/kibana.json
