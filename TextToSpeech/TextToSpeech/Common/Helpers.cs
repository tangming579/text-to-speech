using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextToSpeech.Common
{
    public static class Helpers
    {
        /// <summary>
        /// 使用 Encoding.UTF8 对 s 加密
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToMD5(this string s)
        {
            return ToMD5(s, Encoding.UTF8);
        }
        public static string ToMD5(this string s, Encoding encoding)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = encoding.GetBytes(s);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                foreach (var hashByte in hashBytes)
                {
                    sb.Append(hashByte.ToString("X2"));
                }

                return sb.ToString();
            }
        }
        public static string Base64Encode(this string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            string str = Convert.ToBase64String(bytes);
            return str;
        }
    }
}
