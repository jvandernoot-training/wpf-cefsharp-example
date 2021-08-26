using System;
using System.IO;
using CefSharp;

namespace WpfCefSharpExample.Schemes
{
    // This example is taken from: https://thechriskent.com/2014/04/21/use-local-files-in-cefsharp/
    public class LocalSchemeHandler : ResourceHandler
    {
        public override CefReturnValue ProcessRequestAsync(IRequest request, ICallback callback)
        {
            Uri u = new Uri(request.Url);
            var file = u.Authority;

            if (File.Exists(file))
            {
                byte[] bytes = File.ReadAllBytes(file);
                Stream = new MemoryStream(bytes);
                switch (Path.GetExtension(file))
                {
                    case ".html":
                        MimeType = "text/html";
                        break;
                    case ".js":
                        MimeType = "text/javascript";
                        break;
                    case ".png":
                        MimeType = "image/png";
                        break;
                    case ".appcache":
                    case ".manifest":
                        MimeType = "text/cache-manifest";
                        break;
                    default:
                        MimeType = "application/octet-stream";
                        break;
                }
            }

            return base.ProcessRequestAsync(request, callback);
        }
    }

    public class LocalSchemeHandlerFactory : ISchemeHandlerFactory
    {
        public static string SchemeName => "local";

        public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
        {
            var scheme = schemeName ?? "";
            if(scheme.ToLower() == SchemeName.ToLower())
                return new LocalSchemeHandler();

            throw new InvalidOperationException($"Unknown scheme name: {schemeName}");
        }
    }
}