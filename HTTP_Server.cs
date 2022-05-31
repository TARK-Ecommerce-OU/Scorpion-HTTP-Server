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
        public static int pageViews = 0;
        public static int requestCount = 0;
        public static ScorpionDriver SD;
        private static string DB = null;

        public HTTPServer(string prefix, string scorpion_host, int scorpion_port, string scorpion_db, bool debug)
        {
            //Start the scorpion driver in order to get data from MicroDB
            SD = new ScorpionDriver(scorpion_host, scorpion_port, debug);
            scorpion_sessions = new ScorpionHttpSessions.ScorpionHttpSessions(scorpion_host, scorpion_port, debug);
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
            bool is_script = false, isdata = false;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await scorpion_http_listener.GetContextAsync();
                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;
                //If request ends with .js start script mode
                is_script = isJs(req.Url.AbsolutePath);
                //Write information abour request to the console
                showRequestInfo(ref req, is_script);

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
                byte[] data = null; object[] checks = null;

                /*Check for any errors including session errors or no response form XMLDB
                on:
                noerror: set the session variable
                error: respond with the appropriate error page
                */
                if(!(bool)(checks = checkErrors(URL_elements, is_script))[0])
                {
                    await writeResponse((byte[])checks[1], resp, false);
                    continue;
                }
                else
                {
                    //[session:string][isnewsession:bool]
                    session = (string)((object[])checks[2])[0];

                    //If a new session, redirect to correct URL
                    if((bool)((object[])checks[2])[1])
                        resp.Redirect(req.Url.OriginalString + (req.Url.OriginalString.EndsWith("/") ? session : ("/" + session)));
                }

                //Post allows for posting data to the server
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
                    {
                        if(!is_script)
                            data = Encoding.UTF8.GetBytes(string.Format(StaticElements.StaticElements.kdevelopment_format_data,
                            (request_elements == null ? "" : request_elements),
                            StaticElements.StaticElements.default_js,
                            String.Format(StaticElements.StaticElements.js_session, session, req.Url)));
                        else
                            data = Encoding.UTF8.GetBytes((request_elements == null ? "" : request_elements));
                    }
                }

                //Write a successful page or script response
                await writeResponse(data, resp, is_script);

                //Reset is_script
                is_script = false;
            }
        }

        private static object[] checkSession(ref string[] URL_elements, bool is_script)
        {
            //Check if a user token was passed if not apply a new one. If it was passed verify it
            if(URL_elements.Length < 3 && !is_script)
                return new object[2]{scorpion_sessions.newSession(URL_elements[0]), true};
            if(URL_elements.Length >= 3)
                return new object[2]{scorpion_sessions.verifySession(URL_elements[2]) == true ? URL_elements[2] : null, false};
            return null;
        }

        public static async Task writeResponse(byte[] data, HttpListenerResponse resp, bool script)
        {
            if(data != null)
            {
                resp.ContentType = (script == false ? "text/html" : "text/javascript");
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                //Add content security policy information to the response header
                resp.AppendHeader("Content-Security-Policy", "default-src http://localhost:8000");

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

        private static void showRequestInfo(ref HttpListenerRequest req, bool is_script)
        {
            // Print out some info about the request
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Request #: {0}", ++requestCount);
            Console.WriteLine(req.Url.ToString());
            Console.WriteLine(req.HttpMethod);
            Console.WriteLine(req.UserHostName);
            Console.WriteLine(req.UserAgent);
            Console.WriteLine(req.RawUrl[0]);
            Console.WriteLine("Absolute path {0}", req.Url.AbsolutePath);
            Console.WriteLine($"Is javascript script: {is_script}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static bool isJs(string absolute_path)
        {
            return absolute_path.Contains(".js");
        }

        private static object[] checkErrors(string[] URL_elements, bool is_script)
        {
            //returns (bool, data, session)
            byte[] data = null; object[] session = null;
            bool ok = true;

            if(URL_elements.Length < 1)
            {
                data = Encoding.UTF8.GetBytes(@StaticElements.StaticElements.kurl_error_page_data);
                ok = false;
            }

            //If elements ok, check session
            else if((session = checkSession(ref URL_elements, is_script))[0] == null)
            {
                data = Encoding.UTF8.GetBytes(@StaticElements.StaticElements.kerror_session_page_data);
                ok = false;
                Console.WriteLine("Inexistent session {0}", URL_elements[2]);
            }
            return new object[3] { ok, data, session };
        }
    }
}