using AM.Elasticsearch.TraceListener;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ElasticSearch.Diagnostics.Tests
{
    public class esinsideTests : IDisposable
    {
        private readonly ITestOutputHelper output;
        private readonly ElasticsearchInside.Elasticsearch _elasticsearch;

        public esinsideTests(ITestOutputHelper output)
        {
            this.output = output;
            _elasticsearch = new ElasticsearchInside.Elasticsearch();
            }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _elasticsearch?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~esinsideTests() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion


        [Fact]
        public async Task SimpleWrite()
        {
            ////Arrange
            await _elasticsearch.Ready();

            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = _elasticsearch.Url.ToString();
            x.ElasticSearchTraceIndex = "trace";

            x.Write(4);
        }

        [Fact]
        public async Task WriteObjectTest()
        {
            await _elasticsearch.Ready();

            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri =_elasticsearch.Url.ToString();
            x.ElasticSearchTraceIndex = "trace";


            x.Write(new
            {
                thing = "ggg",
                morethings = 11111,
                anotherthing = "yyyy"
            });
        }

        [Fact]
        public async Task WriteExceptionest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri =_elasticsearch.Url.ToString();
            x.ElasticSearchTraceIndex = "trace";

            try
            {
                var n = 0;
                var y = 100000 / n;
            }
            catch (Exception ex)
            {
                x.Write(ex);
                throw;
            }

        }





        [Fact]
        public async Task ALOTofExmsgs()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri =_elasticsearch.Url.ToString();
            x.ElasticSearchTraceIndex = "trace";

            for (int i = 0; i < 100; i++)
            {
                x.Write(new Exception());
            }

            x.Flush();
        }

        [Fact]
        public async Task WriteManySimpleStringsTest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri =_elasticsearch.Url.ToString();
            x.ElasticSearchTraceIndex = "trace";

            for (int i = 0; i < 10; i++)
            {
                x.Write("xxxxx" + i);
            }
            x.Flush();
        }

        [Fact]
        public async Task TraceSourceManySimpleStringsTest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri =_elasticsearch.Url.ToString();
            x.ElasticSearchTraceIndex = "trace";

            var ts = new TraceSource("x", SourceLevels.All);
            ts.Listeners.Add(x);

            for (int i = 0; i < 10; i++)
            {
                ts.TraceInformation("xxxxx" + i);
            }
            x.Flush();

        }

        [Fact]
        public async Task TSTestTimeIds()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri =_elasticsearch.Url.ToString();
            x.ElasticSearchTraceIndex = "trace";

            var ts = new TraceSource("x", SourceLevels.All);
            ts.Listeners.Add(x);

            for (int i = 0; i < 10000; i++)
            {
                ts.TraceEvent(TraceEventType.Error, 1000, DateTime.Now.ToString());
            }
            x.Flush();

        }

        [Fact]
        public async Task TSManyWriteExceptionsTest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri =_elasticsearch.Url.ToString();
            x.ElasticSearchTraceIndex = "trace";

            var ts = new TraceSource("exxxxx", SourceLevels.All);
            ts.Listeners.Add(x);

            try
            {
                var n = 0;
                var y = 100000 / n;
            }
            catch (Exception ex)
            {
                for (int i = 0; i < 10000; i++)
                {
                    ts.TraceData(TraceEventType.Error, 99, ex);
                }
            }
            x.Flush();
        }

        [Fact]
        public async Task TraceDataWithString()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri =_elasticsearch.Url.ToString();
            x.ElasticSearchTraceIndex = "trace";

            var ts = new TraceSource("exxxxx", SourceLevels.All);
            ts.Listeners.Add(x);

            ts.TraceData(TraceEventType.Error, 99, "ggggggggggggggggg");

            x.Flush();
        }




        class Junk
        {
            public IPAddress ipaddr { get; set; }
        }

        [Fact]
        public async Task CauseFailedSerialization()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri =_elasticsearch.Url.ToString();
            x.ElasticSearchTraceIndex = "trace";

            var ts = new TraceSource("exxxxx", SourceLevels.All);
            ts.Listeners.Add(x);

            for (int i = 0; i < 50; i++)
            {

                ts.TraceData(TraceEventType.Information, 99, new ESTLUnitTests.Junk()
                {
                    ipaddr = IPAddress.Parse("1.1.1.1")
                });
            }

            ts.Flush();
        }

        //[Fact]
        //public async Task IdentityTest()
        //{
        //    IPrincipal principal = Thread.CurrentPrincipal;
        //    IIdentity identity = principal == null ? null : principal.Identity;
        //    string name = identity == null ? "" : identity.Name;

        //    Assert.False(string.IsNullOrWhiteSpace(name));
        //}

        [Fact]
        public async Task UserNameTest()
        {
            string name = Environment.UserDomainName + "\\" + Environment.UserName;

            this.output.WriteLine(name);

            Assert.False(string.IsNullOrWhiteSpace(name));
        }


    }
}
