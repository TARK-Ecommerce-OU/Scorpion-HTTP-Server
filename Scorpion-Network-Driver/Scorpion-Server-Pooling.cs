using System;

namespace ScorpionPooling
{
    class ScorpionPooling
    {
        //Future: calculate distance by latency, choose fastest

        private struct available_server 
        {
            string name;
            string ip;
            int port;
            bool alive;
        };

        private available_server[] pool = new available_server[5]; 

        public ScorpionPooling()
        {
            findServers();
        }

        private void findServers()
        {
            
        }

        /*public available_server getBestServer()
        {

            return null;
        }*/
    }
}