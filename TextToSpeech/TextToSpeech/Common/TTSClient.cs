using Baidu.Aip.Speech;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextToSpeech.Common
{
    public static class TTSClient
    {
        public static Tts GetClient()
        {
            var apiKey = ConfigurationManager.AppSettings["API_KEY"];
            var appId = ConfigurationManager.AppSettings["APP_ID"];
            var secretKey = ConfigurationManager.AppSettings["SECRET_KEY"];

            var client = new Baidu.Aip.Speech.Tts(apiKey, secretKey);
            client.Timeout = 60000;  // 修改超时时间            

            client.AppId = appId;
            return client;
        }
    }
}
