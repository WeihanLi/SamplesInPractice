using System;
using System.Net.Http;
using System.Text.Json.Node;
using System.Threading.Tasks;
using WeihanLi.Extensions;

namespace NotificationSample
{
    // 企业微信 API 文档
    // https://work.weixin.qq.com/api/doc/90000/90003/90487
    // https://work.weixin.qq.com/api/doc/90000/90135/90236
    public class WechatCorpNotification
    {
        private const string CorpId = "ww3508de6cad12eebd";
        private const string AppId = "1000003";
        private const string AppSecret = "";
        private const string ToUid = "@all";

        public static async Task MainTest()
        {
            var getTokenUrl =
                $"https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={CorpId.UrlEncode()}&corpsecret={AppSecret.UrlEncode()}";

            using var httpClient = new HttpClient();
            var responseStr = await httpClient.GetStringAsync(getTokenUrl);
            Console.WriteLine(responseStr);
            var accessToken = JsonNode.Parse(responseStr)["access_token"].GetValue<string>();

            var sendMessageUrl =
                $"https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token={accessToken.UrlEncode()}";

            using var response = await httpClient.PostAsJsonAsync(sendMessageUrl,
                new
                {
                    touser = ToUid,
                    msgtype = "text",
                    agentid = AppId,
                    text = new {content = $"Test {DateTime.Now:yyyy-MM-dd HH:mm:ss}"}
                });
            responseStr = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseStr);
        }
    }
}
