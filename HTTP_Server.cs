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
        private static ScorpionHttpSessions.ScorpionHttpSessions scorpion_sessions;
        public static string url = "http://localhost:8000/"; 
        //private string current_prefix = null;
        public static int pageViews = 0;
        public static int requestCount = 0;
        public static ScorpionDriver SD;
        private static string DB = null;

        public HTTPServer(string prefix, string scorpion_host, int scorpion_port, string scorpion_db)
        {
            //Start the scorpion driver in order to get data from MicroDB
            SD = new ScorpionDriver(scorpion_host, scorpion_port);
            scorpion_sessions = new ScorpionHttpSessions.ScorpionHttpSessions();
            DB = scorpion_db;

            //Start the HTTP server
            if(!startServer(prefix == "null" ? null : prefix))
                Console.WriteLine("Unable to start the HTTP server as the HTTPListener module is not supported by your system.You may try configuring your server and try running the HTTP server again\n\nexiting server...");
            return;
        }

        public bool startServer(string prefix)
        {
            if(!HttpListener.IsSupported)
                return false;
            scorpion_http_listener = new HttpListener();

            //Adds the default prefix is none is given in the arguments when starting the application and starts the HTTP server
            scorpion_http_listener.Prefixes.Add(prefix = (prefix == null ? url : prefix));
            scorpion_http_listener.Start();
            Console.WriteLine("HTTP server started with prefix: {0}", prefix);

            //Handle incomming connections
            Task listen_task = Serve(); 
            listen_task.GetAwaiter().GetResult();
            Console.WriteLine("Awaiting connections...");

            //Stop the HTTP listener and close it
            scorpion_http_listener.Stop();
            scorpion_http_listener.Close();
            Console.WriteLine("HTTP server stopped");
            return true;
        }

        public static async Task Serve()
        {
            //URL FORMAT GET: /project/page/hash/

            bool runServer = true;
            string session = "";

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
                Console.WriteLine(req.Url.AbsolutePath);


                // Make sure we don't increment the page views counter if `favicon.ico` is requested
                //Ignore favico for now
                if (req.Url.AbsolutePath != "/favicon.ico")
                    pageViews += 1;
                else
                    continue;

                //Create a workable string out of the url seperating elements withing '/'
                string[] URL_elements = getPathElements(req);

                //Response vars
                string disableSubmit = !runServer ? "disabled" : "";
                byte[] data = null;

                //If the URL does not atleast contain 2 consecutive items: /project/page/
                if(URL_elements.Length < 1)
                {
                    data = Encoding.UTF8.GetBytes(StaticElements.StaticElements.kurl_error_page_data);
                    await writeResponse(data, resp);
                    continue;
                }

                //Checks whether the session passed at URL_elements[3] exists if none is passed it auto creates a new one else it defaults to an error page
                if((session = checkSession(ref URL_elements)) == null)
                {
                    Console.WriteLine("Incorrect session {0}", URL_elements[2]);
                    await writeResponse(Encoding.UTF8.GetBytes(StaticElements.StaticElements.kerror_session_page_data), resp);
                    continue;
                }
                //If a new session is created redirect the page with the new URL including /session/ and continue to serve next request
                if(URL_elements.Length < 2)
                {   //NOTWORKING!
                    await writeResponse(Encoding.UTF8.GetBytes(String.Format(StaticElements.StaticElements.kredirect_new_session, session)), resp);
                    continue;
                }

                /*If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                Handles input*/
                if ((req.HttpMethod == "POST"))
                {
                    Console.WriteLine("--Input request-->");
                    continue;
                }
                
                //Page GET request: /Project/Page/Hash
                if(req.Url.AbsolutePath != "/")
                {
                    Console.WriteLine("--Page/script request-->");
                    string request_elements = await SD.get(DB, URL_elements[0], URL_elements[1], session);

                    if(request_elements == null)
                    {
                        Console.WriteLine("Incorrect Scorpion IEE response given, sending error page");
                        data = Encoding.UTF8.GetBytes(StaticElements.StaticElements.kerror_page_data);
                    }
                    else
                        data = Encoding.UTF8.GetBytes(string.Format(StaticElements.StaticElements.kdevelopment_format_data, (request_elements == null ? "" : request_elements)));
                }
                await writeResponse(data, resp);
            }
        }

        private static string checkSession(ref string[] URL_elements)
        {
            //Check if a user token was passed if not apply a new one. If it was passed verify it
            if(URL_elements.Length < 3)
                return scorpion_sessions.newSession(URL_elements[0]);
            else if(scorpion_sessions.verifySession(URL_elements[2]))
                    return URL_elements[2];
            return null;
        }

        public static async Task writeResponse(byte[] data, HttpListenerResponse resp)
        {
            if(data != null)
            {
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                //Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
            }
            resp.Close();
        }

        public static string[] getPathElements(HttpListenerRequest request)
        {
            //This function parses the URL for the needed specific elements
            //URL format: /page:db/data:tag/data:subtag
            if(request.Url.AbsolutePath == "/")
                return new string[] { "/" };
            else
                return request.Url.AbsolutePath.Replace("../", "").Replace("~", "").Replace("~/", "").Split('/', StringSplitOptions.RemoveEmptyEntries);
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