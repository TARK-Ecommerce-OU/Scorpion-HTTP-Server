using System;

namespace ScorpionPooling
{
    class ScorpionPooling
    {
        //Future: calculate distance by latency, choose fastest

        private struct available_server {
            string name;
            string ip;
            int port;
            bool alive;
        };

        public ScorpionPooling(){
            find_servers();
        }

        private void find_servers(){
            
        }
    }
}