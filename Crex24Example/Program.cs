using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Crex24Example
{
    class Program
    {
        /********************************************
         *                                          *
         * !!!DON'T COMMIT YOUR REAL PRIVATE KEY!!! *
         *                                          *
         *******************************************/
        public const string ApiKey = "Type_your_Api_Key_here";
        public const string PrivateKey = "Type_yor_private_key_here";
        public const string TradingApiUrl = "https://api.crex24.com/CryptoExchangeService/BotTrade/";


        static void Main(string[] args)
        {
            var url = TradingApiUrl + "ReturnBalances";
            var request = WebRequest.CreateHttp(url);
            request.Method = "POST";
            request.UserAgent = "Crex24 API";
            request.Timeout = 10000;
            request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip,deflate";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentType = "application/json";

            var postData = 
                "{" +
                    "\"Names\": [\"BTC\", \"LTC\"]," +
                    "\"NeedNull\": \"true\"," +
                    "\"Nonce\":" + GetCurrentHttpPostNonce().ToString() +
                "}";

            var postBytes = Encoding.ASCII.GetBytes(postData);
            request.ContentLength = postBytes.Length;
            request.Headers["UserKey"] = ApiKey;

            var encryptor = new HMACSHA512(Convert.FromBase64String(PrivateKey));
            request.Headers["Sign"] = Convert.ToBase64String(encryptor.ComputeHash(postBytes));


            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(postBytes, 0, postBytes.Length);
            }

            var result = GetResponseString(request);

            Console.WriteLine(result);

            Console.ReadKey();
        }

        private static readonly DateTime DateTimeUnixStart = new DateTime(2018, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private static ulong currentHttpPostNonce;
        public static ulong GetCurrentHttpPostNonce()
        {
            var newHttpPostNonce = Convert.ToUInt64(Math.Round(DateTime.UtcNow.Subtract(DateTimeUnixStart).TotalMilliseconds * 1000, MidpointRounding.AwayFromZero));
            if (newHttpPostNonce > currentHttpPostNonce)
            {
                currentHttpPostNonce = newHttpPostNonce;
            }
            else
            {
                currentHttpPostNonce += 1;
            }
            return currentHttpPostNonce;
        }

        public static string GetResponseString(HttpWebRequest request)
        {
            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    if (stream == null) return string.Empty;

                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
