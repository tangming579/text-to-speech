using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
using TextToSpeech.Common;

namespace TextToSpeech
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

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");
            System.Diagnostics.Process.Start(path);
        }

        #region 百度
        public void Tts(int speed = 5, int vol = 6, int person = 4)
        {
            var client = TTSClient.GetClient();
            if (person == 1) person = 2;
            else if (person == 2) person = 0;
            // 可选参数
            var option = new Dictionary<string, object>()
             {
                {"spd", speed}, // 语速
                {"vol", vol}, // 音量
                {"per", person}  // 发音人，4：情感度丫丫童声
             };
            var result = client.Synthesis(txtMsg.Text, option);
            object obj = new object();
            if (result.ErrorCode == 0)  // 或 result.Success
            {
                string name = txtMsg.Text;/* + DateTime.Now.ToString("yyyyMMddHHssffff")*/;
                Task.Run(new Action(() =>
                {
                    lock (obj)
                    {
                        var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", $"{name}{DateTime.Now:yyyyMMddHHmmssffff}.mp3");
                        if (File.Exists(path)) File.Delete(path);
                        Thread.Sleep(1000);
                        File.WriteAllBytes(path, result.Data);

                        MessageBox.Show("生成成功");
                    }
                }));

            }
        }

        private void btnTts_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(new Action(() =>
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Tts((int)sldSpeed.Value, (int)sldVol.Value, lstPer.SelectedIndex + 1);
                }));

            }));
        }
        #endregion

        #region 科大讯飞
        //科大讯飞
        private void btnKeda_Click(object sender, RoutedEventArgs e)
        {
            string url = "http://api.xfyun.cn/v1/service/v1/tts";
            string appId = "5b3c9b38";
            string apiKey = "6d3122d48c1b5309eb60eda4fb7f354b";

            Parameter parameter = new Parameter();
            //parameter.speed = ((int)sldSpeed.Value * 10) + "";
            //parameter.volume = ((int)sldVol.Value * 10) + "";

            var json_str = JsonConvert.SerializeObject(parameter);
            var base64_str = Convert.ToBase64String(Encoding.UTF8.GetBytes(json_str));

            HttpWebRequest httpwebrequest = null;
            HttpWebResponse httpwebresponse = null;
            httpwebrequest = (HttpWebRequest)WebRequest.Create(url);
            httpwebrequest.Method = "POST";

            String t_s_1970 = TimestampSince1970;
            String checksum = Helpers.GetMD5(apiKey + t_s_1970 + base64_str);//准备好一个checksum备用
            httpwebrequest.Headers.Clear();
            httpwebrequest.Headers.Add("X-Param", base64_str);
            httpwebrequest.Headers.Add("X-CurTime", t_s_1970);
            httpwebrequest.Headers.Add("X-Appid", appId);
            httpwebrequest.Headers.Add("X-CheckSum", checksum);
            httpwebrequest.Headers.Add("X-Real-Ip", "127.0.0.1");
            httpwebrequest.ContentType = "application/x-www-form-urlencoded";
            httpwebrequest.Headers.Add("charset", "utf-8");

            using (Stream stream = httpwebrequest.GetRequestStream())
            {
                byte[] data = Encoding.UTF8.GetBytes($"text={txtMsg.Text}");//更改生成内容时，text= 要保留
                stream.Write(data, 0, data.Length);
            }
            httpwebresponse = (HttpWebResponse)httpwebrequest.GetResponse();
            Stream res_strem = httpwebresponse.GetResponseStream();
            if (httpwebresponse.ContentType == "text/plain")//ContentType等于"text/plain"即表示生成失败，等于"audio/mpeg"即生成成功
            {
                using (StreamReader s_reader = new StreamReader(res_strem, Encoding.UTF8))
                {
                    String a = s_reader.ReadToEnd();
                    MessageBox.Show($"生成失败:{a}");
                }
            }
            else
            {
                var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", $"{txtMsg.Text}{DateTime.Now:yyyyMMddHHmmssffff}.mp3");
                using (StreamWriter sw = new StreamWriter(path))
                {
                    res_strem.CopyTo(sw.BaseStream);
                    sw.Flush();
                    sw.Close();
                    res_strem.Dispose();
                }

                MessageBox.Show("生成成功");
            }
        }
        public class Parameter
        {
            public string auf { get; set; } = "audio/L16;rate=16000";
            public string aue { get; set; } = "lame";
            public string voice_name { get; set; } = "xiaoyan";
            public string speed { get; set; } = "40";
            public string volume { get; set; } = "70";
            public string pitch { get; set; } = "50";
            public string engine_type { get; set; } = "intp65";
            public string text_type { get; set; } = "text";
        }

        public static string TimestampSince1970
                  => Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds).ToString();

        private void Client_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            var client = sender as WebClient;

            var a = e.Result;
        }        
        #endregion

        #region 有道云
        private void btnYoudao_Click(object sender, RoutedEventArgs e)
        {
            String url = "http://openapi.youdao.com/ttsapi";
            Dictionary<String, String> dic = new Dictionary<String, String>();
            string q = txtMsg.Text;
            string appKey = "2b0423b61a13d0c9";
            string appSecret = "O1Fb0kYPWsxsl0r02CrJyD7mcJHpQH1r";

            /** 目标语言 
             
            语言	    代码	支持发音类型
            中文	    zh-CHS	女声
            日文	    ja	    女声、男声
            英文	    en	    女声、男声
            韩文	    ko	    女声
            法文	    fr	    女声、男声
            葡萄牙文	pt	    女声
            西班牙文	es	    女声、男声
            俄文	    ru	    女声、男声             
             
             */
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
            string fileName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", $"{txtMsg.Text}.mp3");
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
                var rst = SaveBinaryFile(resp, fileName);
                MessageBox.Show(rst ? "生成成功" : "生成失败");
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
            catch (Exception exp)
            {
                Value = false;
            }
            return Value;
        }
        #endregion

    }
}
