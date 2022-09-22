using System;
using KgCaptchaSDK;

public partial class _Default : System.Web.UI.Page{
    protected void Page_Load(object sender, EventArgs e) {

        // 后端处理
        string html, appId, appSecret, Token;
        if (Request.Form.ToString().Length > 0){  // 有数据处理

            string cty = Request.QueryString["cty"];
            // 设置 AppId 及 AppSecret，在应用管理中获取
            if (cty == "1"){
                appId = "rA9qRcl6";
                appSecret = "6h75TuboCunnHNQhI5zzxZOZav0Wzf9e";
            } else if (cty == "2") {
                appId = "nC1sCjwg";
                appSecret = "Vq2okDtS8XqtRgCH2sR9SLq0A5eS30Cq";
            } else {
                appId = "4gXIWZzz";
                appSecret = "VqFz4RCxtzYu9IzvhvtiEQDdPrmkA7If";
            }

            // 填写你的 AppId 和 AppSecret，在应用管理中获取
            var request = new kgCaptcha(appId, appSecret);

            // 前端验证成功后颁发的 token，有效期为两分钟
            request.token = Request.Form["kgCaptchaToken"];



            // 填写应用服务域名，在应用管理中获取
            request.appCdn = "https://cdn9.kgcaptcha.com";


            // 当安全策略中的防控等级为3时必须填写，一般情况下可以忽略
            // 可以填写用户输入的登录帐号（如：$_POST["username"]），可拦截同一帐号多次尝试等行为
            request.userId = "kgCaptchaDemo";

            // 请求超时时间，秒
            request.connectTimeout = 5;

            // 发送验证请求
            var requestResult = request.sendRequest();

            if (requestResult.code == 0) {
                // 验签成功逻辑处理 ***

                // 这里做验证通过后的数据处理
                // 如登录/注册场景，这里通常查询数据库、校验密码、进行登录或注册等动作处理
                // 如短信场景，这里可以开始向用户发送短信等动作处理
                // ...

                html = "<script>alert('验证通过');history.back();</script>";
            } else {
                // 验签失败逻辑处理
                html = "<script>alert(\"" + requestResult.msg + " - " + requestResult.code + "\");history.back();</script>";
            }

            // 输出结果
            Response.Write(html);
        } else {
            Response.Redirect("index.html");
        }
    }
}