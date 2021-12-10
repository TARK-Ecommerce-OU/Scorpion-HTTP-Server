namespace StaticElements
{
    public static class StaticElements
    {
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

        public static string developmentFormatData =
            "<!DOCTYPE html><html><head><meta charset='UTF-8'><script src='https://cdn.jsdelivr.net/npm/vue@2/dist/vue.js'></script><script src='https://cdn.jsdelivr.net/npm/axios/dist/axios.min.js'></script></head><body>{0} {1} {2}</body></html>";

        public static string productionFormatData = 
            "<!DOCTYPE html><html><head><meta charset='UTF-8'><script src='https://cdn.jsdelivr.net/npm/vue@2'></script><script src='https://cdn.jsdelivr.net/npm/axios/dist/axios.min.js'></script></head><body>{0} {1} {2}</body></html>";
    }
}