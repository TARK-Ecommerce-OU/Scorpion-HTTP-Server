using System;

namespace ScorpionHTTPServer
{
    class ScorpionHTTPServer
    {
        //The main HTTP server serving this application
        private static HTTPServer http_server;

        static void Main(string[] args)
        {
            string command = null;
            bool stop = false;
            while(!stop)
            {
                command = Console.ReadLine().ToLower();
                if(command == "start")
                {
                    Console.WriteLine("Scorpion HTTP Server version {0}", System.Reflection.Assembly.GetExecutingAssembly().ImageRuntimeVersion);
                    http_server = new HTTPServer(args[0], args[1], Convert.ToInt32(args[2]));
                }
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
                    Console.WriteLine("Please start the server if you would wish to stop it");
            }
            return;
        }
    }
}
