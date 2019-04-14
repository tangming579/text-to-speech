using Microsoft.CognitiveServices.Speech;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            String url = "http://openapi.youdao.com/ttsapi";
            Dictionary<String, String> dic = new Dictionary<String, String>();
            string q = txtMsg.Text;
            string appKey = "2b0423b61a13d0c9";
            string appSecret = "O1Fb0kYPWsxsl0r02CrJyD7mcJHpQH1r";

            /** 目标语言 */
            string langType = "pt";
            /** 音频格式：目前支持pcm和wav(pcm编码) */
            string format = "wav";
            /** 音频采样率：目前支持16000和8000 */
            string rate = "16000";
            /** 音频频道 */
            string channel = "1";

            /** 随机数，自己随机生成，建议时间戳 */
            string salt = DateTime.Now.Millisecond.ToString();

            MD5 md5 = new MD5CryptoServiceProvider();
            string md5Str = appKey + q + salt + appSecret;
            byte[] output = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(md5Str));
            string sign = BitConverter.ToString(output).Replace("-", "");
            dic.Add("q", System.Web.HttpUtility.UrlEncode(q));
            dic.Add("appKey", appKey);
            dic.Add("langType", langType);
            dic.Add("salt", salt);
            dic.Add("sign", sign);
            string fileName = "d:/test.mp3";
            Post(url, dic, fileName);
        }

        public static void Post(string url, Dictionary<String, String> dic, String fileName)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            #region 添加Post 参数
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dic)
            {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            Console.WriteLine(builder.ToString());
            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            #endregion
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            if (resp.ContentType.ToLower().Equals("audio/mp3"))
            {
                SaveBinaryFile(resp, fileName);
            }
            else
            {
                Stream stream = resp.GetResponseStream();
                //获取响应内容
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
                Console.WriteLine(result);
            }
        }

        private static bool SaveBinaryFile(WebResponse response, string FileName)
        {
            bool Value = true;
            byte[] buffer = new byte[1024];

            try
            {
                if (File.Exists(FileName))
                    File.Delete(FileName);
                Stream outStream = System.IO.File.Create(FileName);
                Stream inStream = response.GetResponseStream();

                int l;
                do
                {
                    l = inStream.Read(buffer, 0, buffer.Length);
                    if (l > 0)
                        outStream.Write(buffer, 0, l);
                }
                while (l > 0);

                outStream.Close();
                inStream.Close();
            }
            catch
            {
                Value = false;
            }
            return Value;
        }

    }
}
