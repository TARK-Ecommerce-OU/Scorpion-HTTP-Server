using System;

namespace ScorpionHTTPServer
{
    class ScorpionHTTPServer
    {
        //The main HTTP server serving this application
        private static HTTPServer http_server;
        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            Console.WriteLine("Scorpion HTTP Server version {0}", System.Reflection.Assembly.GetExecutingAssembly().ImageRuntimeVersion);
            string command = null;

            while(true)
            {
                command = Console.ReadLine().ToLower();
                if(command == "start")
                {
                    if(args.Length == 5)
                        http_server = new HTTPServer(args[0], args[1], Convert.ToInt32(args[2]), args[3], Convert.ToBoolean(args[4]));
                    else
                        Console.WriteLine("The 'start' command was issued with the wrong set of arguments. Required arguments are: <prefix> <scorpion IEE server IP> <scorpion IEE port> <database name> <debug: true/false>");
                }
            }
        }

        static void Console_CancelKeyPress(object o, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Interrupt Signal. Exiting...");
            Environment.Exit(0);
        }
    }
}
