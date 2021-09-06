using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ScorpionNetworkDriver;

namespace ScorpionHTTPServer
{
    class HTTPServer
    {
        private static HttpListener scorpion_http_listener;
        public static string url = "http://localhost:8000/"; private string current_prefix = null;
        public static int pageViews = 0;
        public static int requestCount = 0;
        public static ScorpionDriver SD;
        public static string pageData = 
            "<!DOCTYPE>" +
            "<html>" +
            "  <head>" +
            "    <title>HttpListener Example</title>" +
            "  </head>" +
            "  <body>" +
            "    <p>Page Views: {0}</p>" +
            "    <form method=\"post\" action=\"shutdown\">" +
            "      <input type=\"submit\" value=\"Shutdown\" {1}>" +
            "    </form>" +
            "  </body>" +
            "</html>";
        public static string errorPageData = 
            "<!DOCTYPE>" +
            "<html>" +
            "  <head>" +
            "    <title>Error</title>" +
            "  </head>" +
            "  <body>" +
            "    <p><h1>500 Internal server error</h1><br><hr><br>Incorrect response given</p>" +
            "  </body>" +
            "</html>";

        public HTTPServer(string prefix, string scorpion_host, int scorpion_port)
        {
            //Start the scorpion driver in order to get data from MicroDB
            SD = new ScorpionDriver(scorpion_host, scorpion_port);

            //Start the HTTP server
            if(!startServer(prefix == "null" ? null : prefix))
                Console.WriteLine("Unable to start the HTTP server as the HTTPListener module is not supported by your system.");
            return;
        }

        public bool startServer(string prefix)
        {
            if(!HttpListener.IsSupported)
                return false;
            scorpion_http_listener = new HttpListener();

            //Adds the default prefix is none is given in the arguments when starting the applicaion
            if(prefix == null)
            {
                scorpion_http_listener.Prefixes.Add(url);
                current_prefix = url;
            }
            else
            {
                scorpion_http_listener.Prefixes.Add(prefix);
                current_prefix = prefix;
            }

            scorpion_http_listener.Start();
            Console.WriteLine("HTTP server started with prefix: {0}", current_prefix);

            //Handle incomming connections
            Task listen_task = handleIncomingConnections(); 
            listen_task.GetAwaiter().GetResult();
            Console.WriteLine("Awaiting connections...");

            //Stop the HTTP listener and close it
            scorpion_http_listener.Stop();
            scorpion_http_listener.Close();
            Console.WriteLine("HTTP server stopped");
            return true;
        }

        public static async Task handleIncomingConnections()
        {
            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await scorpion_http_listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine(req.RawUrl[0]);

                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown"))
                {
                    Console.WriteLine("Shutdown requested");
                    runServer = false;
                }

                // Make sure we don't increment the page views counter if `favicon.ico` is requested
                if (req.Url.AbsolutePath != "/favicon.ico")
                    pageViews += 1;

                // Write the response info
                string disableSubmit = !runServer ? "disabled" : "";
                byte[] data = null;
                string[] URL_elements = getPathElements(req);
                
                if(req.Url.AbsolutePath != "/")
                    data = Encoding.UTF8.GetBytes(String.Format(pageData, pageViews, disableSubmit));
                else
                {
                    string command = await SD.get("http", "feministexperience", "html");
                    if(command == null)
                    {
                        Console.WriteLine("Incorrect response given, ignoring...");
                        data = Encoding.UTF8.GetBytes(errorPageData);
                    }
                    else
                    {
                        data = Encoding.UTF8.GetBytes(command);
                    }
                }
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;
                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }

        public static string[] getPathElements(HttpListenerRequest request)
        {
            //This function parses the URL for the needed specific elements
            //URL format: /page:db/data:tag/data:subtag
            if(request.Url.AbsolutePath == "/")
                return new string[] { "/" };
            else
                return request.Url.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        }

        public void stopServer()
        {
            scorpion_http_listener.Stop();
            scorpion_http_listener.Close();
            Console.WriteLine("HTTP server stopped");
            return;
        }
    }
}