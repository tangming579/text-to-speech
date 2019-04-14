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
        //百度
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
            String checksum = GetMD5(apiKey + t_s_1970 + base64_str);//准备好一个checksum备用
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

        public static string GetMD5(string source, bool need16 = false, bool toUpper = false)
        {
            var t_toUpper = toUpper ? "X2" : "x2";
            if (string.IsNullOrWhiteSpace(source))
            {
                return string.Empty;
            }
            string t_md5_code = string.Empty;
            try
            {
                MD5 t_md5 = MD5.Create();
                byte[] _t = t_md5.ComputeHash(Encoding.UTF8.GetBytes(source));
                for (int i = 0; i < _t.Length; i++)
                {
                    t_md5_code += _t[i].ToString(t_toUpper);
                }
                if (need16)
                {
                    t_md5_code = t_md5_code.Substring(8, 16);
                }
            }
            catch { }
            return t_md5_code;
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");
            System.Diagnostics.Process.Start(path);
        }
    }
}
