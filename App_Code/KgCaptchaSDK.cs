/*
 * NET Framework v4.0/ASP.NET v4.8
 * KgCaptcha v1.0.0
 * http://www.KgCaptcha.com
 *
 * Copyright © 2022 Kyger. All Rights Reserved.
 * http://www.kyger.com.cns
 *
 * Copyright © 2022 by KGCMS.
 * http://www.kgcms.com
 *
 * Date: Thu May 20 15:28:23 2022
*/
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace KgCaptchaSDK{
    public class kgCaptcha{
        public string appCdn = "https://cdn9.kgcaptcha.com"; // 风险防控服务URL
        public string appId; // 公钥
        public string appSecret; // 秘钥

        public int connectTimeout = 5;  // 连接超时断开请求，秒

        public string clientIp = "";  //  客户端IP，安全等级为1和2时必须设置
        public string clientBrowser = "";  // 客户端浏览器信息，安全等级为1和2时必须设置
        public string userId;  // 用户手机号/ID/登录用户名，安全等级为2时必须设置


        public string domain;  // 授权域名，当前应用域名
        public string token;  // 前端验证成功后颁发的 token

        private int time;  // 当时时间戳
        private Dictionary<string, string> data;  // 请求数据包
        private string dataURL = "";  // 提交的数据包

        // 构造函数
        public kgCaptcha(string appId, string appSecret) {
            this.appId = appId;
            this.appSecret = appSecret;
            this.time = (int) (new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() / 1000);

            // 来源页面
            this.domain = HttpContext.Current.Request.Url.ToString();
            // HttpContext.Current.Request.Headers ***************************************
            // HttpContext.Current.Request.Url.AbsolutePath
        }

        // 生成签名 URL
        public string signUrl() {
            string data = "";
            this.data = this.putData();
            foreach (KeyValuePair<string, string> item in this.data) {
                data += item.Key + item.Value;
                this.dataURL += "&" + item.Key + "=" + item.Value;
            }
            return this.appCdn + "/requestBack?appid=" + this.appId + "&sign=" + md5(this.appId + data + this.appSecret);
        }

        public string md5(string input) {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++){
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString().ToLower();  // 转小写
        }

        public ResultClass sendRequest() {
            string PostUrl = this.signUrl();
            string PostData = this.dataURL;

            Encoding encoding = Encoding.GetEncoding("UTF-8");
            Stream outstream = null;
            StreamReader sr = null;
            HttpWebRequest request = null;
            HttpWebResponse response = null;

            string result = ""; // 返回结果

            try {
                request = (HttpWebRequest)WebRequest.Create(PostUrl);

                request.Method = "POST";
                request.Timeout = this.connectTimeout * 1000;
                request.Referer = this.domain;

                // 设置POST的数据类型/超时/来路/长度
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] data = encoding.GetBytes(PostData);
                request.ContentLength = data.Length;

                // 往服务器写入数据
                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Flush();
                outstream.Close();

                // 获取服务端返回数据
                response = (HttpWebResponse)request.GetResponse();
                sr = new StreamReader(response.GetResponseStream(), encoding);
                result = sr.ReadToEnd().Trim();
                sr.Close();
            } catch (Exception e) {
                result = "{ \"code\": 20000, \"msg\": \"" + e.ToString() + "\"}";
            }

            // json 转对象
            JavaScriptSerializer js = new JavaScriptSerializer();
            ResultClass ResulObject = js.Deserialize<ResultClass>(result);
            return ResulObject;
        }

        // post 请求数据转对象
        public class ResultClass {
            public int code { get; set; }
            public string msg { get; set; }
        }

        // 数据包
        public Dictionary<string, string> putData() {
            if (this.clientIp == "") {
                this.clientIp = GetIP();
            }

            if (this.clientBrowser == "") {
                // this.clientBrowser = Request.Browser.Type.ToString();
            }

            return new Dictionary<string, string> {
                {"ip", this.clientIp},
                {"browser", this.clientBrowser},
                {"time", this.time.ToString()},
                {"uid", this.userId},
                {"timeout", this.connectTimeout.ToString()},
                {"token", this.token}
            };
        }

        // 获取客户端IP
        private static string GetIP() {
            string userHostAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? "";
            if (!string.IsNullOrEmpty(userHostAddress)) {
                userHostAddress = userHostAddress.Split(',')[0].Trim();
            }
            if (string.IsNullOrEmpty(userHostAddress)) {
                userHostAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            if (string.IsNullOrEmpty(userHostAddress)) {
                userHostAddress = HttpContext.Current.Request.UserHostAddress;
            }
            if (!string.IsNullOrEmpty(userHostAddress) && IsIP(userHostAddress)) {
                return userHostAddress;
            }
            return "127.0.0.1";
        }

        // 检查IP地址格式
        private static bool IsIP(string ip) {
            return System.Text.RegularExpressions.Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }
    }
}