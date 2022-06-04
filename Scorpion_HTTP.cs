using System;
using ScorpionConsoleReadWrite;

namespace ScorpionHTTPServer
{
    class ScorpionHTTPServer
    {
        //The main HTTP server serving this application
        private static HTTPServer http_server;
        private const double kversion = 0.1;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            ConsoleWrite.writeSpecial($"Scorpion HTTP Server version {kversion}\n--------------------------------\n");
            ConsoleWrite.writeOutput("Enter the URL for the server (To which requests to your domain name will be forwarded to OR 'null' for default http://loopback:8000):");
            string url = Console.ReadLine();
            ConsoleWrite.writeOutput("Enter the Scorpion IEE tcp server ip address:");
            string scorpion_ip = Console.ReadLine();
            ConsoleWrite.writeOutput("Enter the Scorpion IEE tcp server port:");
            int port = Convert.ToInt32(Console.ReadLine());
            ConsoleWrite.writeOutput("Enter the Scorpion IEE XMLDB database to use for static content:");
            string database = Console.ReadLine();
            http_server = new HTTPServer(url, scorpion_ip, Convert.ToInt32(port), database);         
        }

        static void Console_CancelKeyPress(object o, ConsoleCancelEventArgs e)
        {
            //ADD TCP and HTTP kill()
            ConsoleWrite.writeOutput("Interrupt Signal. Exiting...");
            Environment.Exit(0);
        }
    }
}
