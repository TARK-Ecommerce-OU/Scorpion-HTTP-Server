//https://stackoverflow.com/questions/61997928/errorcs0579duplicate-globalsystem-runtime-versioning-targetframeworkattribu

using System;

namespace ScorpionHTTPServer
{
    class ScorpionHTTPServer
    {
        //The main HTTP server serving this application
        private static HTTPServer http_server;

        static void Main(string[] args)
        {
            Console.WriteLine("Scorpion HTTP Server version {0}", System.Reflection.Assembly.GetExecutingAssembly().ImageRuntimeVersion);
            string command = null;
            bool stop = false;

            while(!stop)
            {
                command = Console.ReadLine().ToLower();
                if(command == "start" && args.Length == 4)
                    http_server = new HTTPServer(args[0], args[1], Convert.ToInt32(args[2]), args[3]);
                else if(command == "stop" && http_server != null)
                    http_server.stopServer();
                else if(command == "exit")
                {
                    //Check if http_server is instantiated, if yes then stop the server else skip
                    if(http_server != null)
                        http_server.stopServer();
                    stop = true;
                }
                else
                    Console.WriteLine("An error occured, please make sure the command you entered exists or that you entered the correct startup variables:\n\n<???> <???> <???> <???>");
            }
            return;
        }
    }
}
