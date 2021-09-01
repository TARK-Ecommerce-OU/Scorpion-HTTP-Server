using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ScorpionNetworkDriver
{
    class ScorpionDriver
    {
      private static ScorpionDriverTCP SCDT;
      private static NetworkEngineFunctions nef__;

      public ScorpionDriver(string host, int port)
      {
        SCDT = new ScorpionDriverTCP(host, port);
        nef__ = new NetworkEngineFunctions();
        return;
      }

      public async Task<string> get(string DB, string TAG, string SUBTAG)
      {
        return await SCDT.get(nef__.build_query(DB, TAG, SUBTAG));
      }
    }

    class NetworkEngineFunctions
    {
        private static readonly Dictionary<string, string[]> api = new Dictionary<string, string[]> 
        {
            { "scorpion", new string[]{ "{&scorpion}", "{&/scorpion}" } },
            { "database", new string[]{ "{&database}", "{&/database}" } },
            { "type", new string[] {"{&type}", "{&/type}" } },
            { "tag", new string[] {"{&tag}", "{&/tag}" } },
            { "subtag", new string[] {"{&subtag}", "{&/subtag}" } },
            { "data", new string[] {"{&data}", "{&/data}" } },
            { "status", new string[] {"{&status}", "{&/status}" } },
                };

        public readonly Dictionary<string, string> api_requests = new Dictionary<string, string>
        {
            { "get", "get" },
            { "set", "set" },
            { "response", "response" }
                };

        private readonly Dictionary<string, string> api_result = new Dictionary<string, string>
        {
            { "ok", "ok" },
            { "error", "error" }
        };

        public Dictionary<string, string> replace_api(string Scorp_Line)
        {
            Scorp_Line = Scorp_Line.Remove(0, Scorp_Line.IndexOf(api["scorpion"][0], StringComparison.CurrentCulture));
            if (Scorp_Line.Contains(api["scorpion"][0]) && Scorp_Line.Contains(api["scorpion"][1]))
            {
                //Split other elements
                //Get the app
                string[] db, tag, subtag, type;
                type = Scorp_Line.Split(api["type"], StringSplitOptions.RemoveEmptyEntries);
                db = Scorp_Line.Split(api["database"], StringSplitOptions.RemoveEmptyEntries);
                tag = Scorp_Line.Split(api["tag"], StringSplitOptions.RemoveEmptyEntries);
                subtag = Scorp_Line.Split(api["subtag"], StringSplitOptions.RemoveEmptyEntries);
                return new Dictionary<string, string> { { "type", type[1] }, { "db", db[1] }, { "tag", tag[1] }, { "subtag", subtag[1] } };
            }
            return null;
        }

        public string build_api(string data, bool error)
        {
            if(!error)
                return api["scorpion"][0] + api["type"][0] + api_requests["response"] + api["type"][1] + api["data"][0] + data + api["data"][1] + api["status"][0] + api_result["ok"] + api["status"][1];
            return api["scorpion"][0] + api["type"][0] + api_requests["response"] + api["type"][1] + api["data"][0] + data + api["data"][1] + api["status"][0] + api_result["error"] + api["status"][1];
        }
        public string build_query(string DB, string TAG, string SUBTAG)
        {
          return api["scorpion"][0] + api["type"][0] + api_requests["get"] + api["type"][1] + api["database"][0] + DB + api["database"][1] + api["tag"][0] + TAG + api["tag"][1] + api["subtag"][0] + SUBTAG + api["subtag"][1] + api["scorpion"][1];
        }

        public string replace_telnet(string Scorp_Line)
        {
            return Scorp_Line.Replace("\r\n", "").Replace("959;1R", "");
        }
    }

    class ScorpionDriverTCP
    {
      private static TcpClient scorpion_client;
      private static int PORT = 5002;
      private string HOST;

      public ScorpionDriverTCP(string host, int port)
      {
        HOST = host;
        PORT = port;
        connect(HOST, PORT);
        return;
      }

      public async Task<string> get(string message)
      {
        string responseData = null;
        await Task.Run(() => {
          // Translate the passed message into ASCII and store it as a Byte array.
          Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

          // Get a client stream for reading and writing.
          //  Stream stream = client.GetStream();

          NetworkStream stream = scorpion_client.GetStream();

          // Send the message to the connected TcpServer.
          stream.Write(data, 0, data.Length);
          Console.WriteLine("Sent: {0}", message);

          // Receive the TcpServer.response.

          // Buffer to store the response bytes.
          data = new Byte[256];

          // String to store the response ASCII representation.
          String responseData = String.Empty;

          // Read the first batch of the TcpServer response bytes.
          Int32 bytes = stream.Read(data, 0, data.Length);
          responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
          Console.WriteLine("Received: {0}", responseData);

          // Close everything.
          stream.Close();
        });
        return responseData;
      }

      static void connect(string host, int port)
      {
        try
        {
          // Create a TcpClient.
          // Note, for this client to work you need to have a TcpServer
          // connected to the same address as specified by the server, port
          // combination.
          scorpion_client = new TcpClient(host, port);
        }
        catch (ArgumentNullException e)
        {
          Console.WriteLine("ArgumentNullException: {0}", e);
        }
        catch (SocketException e)
        {
          Console.WriteLine("SocketException: {0}", e);
        }
      }

      static void disconnect()
      {
          scorpion_client.Close();
      }
    }
}