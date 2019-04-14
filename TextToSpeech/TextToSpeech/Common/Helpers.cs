using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TextToSpeech.Common
{
    public static class Helpers
    {
        public static string GetMD5(this string source, bool need16 = false, bool toUpper = false)
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
        public static string Base64Encode(this string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            string str = Convert.ToBase64String(bytes);
            return str;
        }
    }
}
