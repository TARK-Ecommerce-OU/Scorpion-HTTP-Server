namespace StaticElements
{
    public static class StaticElements
    {
        public static readonly string errorPageData = 
            "<!DOCTYPE html>" +
            "<html>" +
            "  <head>" +
            "<meta charset='UTF-8'>" +
            "    <title>500 Server error</title>" +
            "  </head>" +
            "  <body>" +
            "    <p><h1>:( 500 Internal server error</h1><br><hr><br>Incorrect response given. The server responded with no recognizable data.</p>" +
            "  </body>" +
            "</html>";
        public static readonly string errorsessionPageData = 
            "<!DOCTYPE html>" +
            "<html>" +
            "  <head>" +
            "<meta charset='UTF-8'>" +
            "    <title>500 Server error</title>" +
            "  </head>" +
            "  <body>" +
            "    <p><h1>:o 500 Internal server error</h1><br><hr><br>Incorrect session given. The server responded with no recognizable session.</p>" +
            "  </body>" +
            "</html>";
        public static readonly string urlerrorPageData = 
            "<!DOCTYPE html>" +
            "<html>" +
            "  <head>" +
            "<meta charset='UTF-8'>" +
            "    <title>500 : Server error</title>" +
            "  </head>" +
            "  <body>" +
            "    <p><h1>:( 500 Internal server error</h1><br><hr><br>URL parameters incorrect. 2 parameters expected, less given.</p>" +
            "  </body>" +
            "</html>";

        public static string developmentFormatData =
            "<!DOCTYPE html><html><head><meta charset='UTF-8'><script src='https://cdn.jsdelivr.net/npm/vue@2/dist/vue.js'></script><script src='https://cdn.jsdelivr.net/npm/axios/dist/axios.min.js'></script></head><body>{0}</body></html>";

        public static string productionFormatData = 
            "<!DOCTYPE html><html><head><meta charset='UTF-8'><script src='https://cdn.jsdelivr.net/npm/vue@2'></script><script src='https://cdn.jsdelivr.net/npm/axios/dist/axios.min.js'></script></head><body>{0}</body></html>";
    }
}