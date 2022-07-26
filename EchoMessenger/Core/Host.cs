using System;

namespace EchoMessenger.Core
{
    public static class Host
    {
        public static readonly String Url = "http://localhost:4224";
        // public static readonly String Url = "https://server-echo.herokuapp.com";

        public static String Combine(String source)
        {
            var url1 = Url.TrimEnd('/', '\\');
            var url2 = source.TrimStart('/', '\\');

            return String.Format("{0}/{1}", url1, url2);
        }
    }
}
