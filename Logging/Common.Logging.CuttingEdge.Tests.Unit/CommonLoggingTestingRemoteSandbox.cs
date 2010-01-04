using System.Collections.Specialized;

using NSandbox;

namespace Common.Logging.CuttingEdge.Tests.Unit
{
    // We use NSandbox: http://thevalerios.net/matt/2008/10/nsandbox-an-introduction/
    public sealed class CommonLoggingTestingRemoteSandbox : RemoteSandboxBase
    {
        public void CallDefaultConstructor()
        {
            var adapter = new CuttingEdgeLoggerFactoryAdapter();
        }

        public void CallConstructorWithPropertiesArgument()
        {
            var adapter = new CuttingEdgeLoggerFactoryAdapter(new NameValueCollection());
        }

        public string GetLoggerName(string name)
        {
            var adapter = new CuttingEdgeLoggerFactoryAdapter();

            var logger = (CuttingEdgeLogger)adapter.GetLogger(name);

            return logger.Name;
        }
    }
}
