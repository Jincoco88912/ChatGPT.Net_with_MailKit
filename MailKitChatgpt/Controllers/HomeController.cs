using HtmlAgilityPack;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKitChatgpt.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ChatGPT.Net;
using Newtonsoft.Json;

namespace MailKitChatgpt.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        int? price;
        string? buyer;
        string? product;

        //https://localhost:7050/Home/SearchMail?orderNo=230515QREA78VM
        public async Task<IActionResult> SearchMail(string orderNo)
        {
                using (var client = new ImapClient())
                {
                    // MailKit
                    // 連線並進行身份驗證
                    client.Connect("imap.gmail.com", 993, true);
                    client.Authenticate("dogedogman.god@gmail.com", "fyfryyxehmpdzgio");

                    // 開啟收件夾權限
                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadWrite);

                    // 使用訂單號進行搜尋
                    var query = SearchQuery.SubjectContains(orderNo);
                    var results = inbox.Search(query);
                    var message = inbox.GetMessage(results[0]);

                    //// HtmlAgilityPack
                    //// 解析信件 HTML 內容
                    //var doc = new HtmlDocument();
                    //doc.LoadHtml(message.HtmlBody);

                    //// 取得純文字內容
                    //var emailText = doc.DocumentNode.SelectSingleNode("//body").InnerText;

                    // System.Text.RegularExpressions
                    // 移除多餘的空格
                    //emailText = Regex.Replace(emailText, @"\s+", " ");

                    // 解析純文字信件文字內容
                    var emailText = message.TextBody;

                    // ChatGPT.Net
                    // 透過 ChatGPT 解析郵件內容，並取得回傳的 JSON 字串
                    var gptJson = await ChatGPT(emailText + "商品總額是多少?買家帳號是多少?商品名稱是什麼?。請用{ \"price\": value, \"buyer\": \"value\", \"product\": \"value\" }json格式回覆");

                    // 進一步檢查json資料
                    CheckJson(gptJson);

                    // 中斷伺服器連結
                    client.Disconnect(true);

                    return View("Index");
                }
        }

        public void CheckJson(string gptJson)
        {
            // Newtonsoft.Json;
            // 假設 json 是您從 ChatGPT 取得的 JSON 字串
            var resultJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(gptJson);

            // 解析 JSON 字串，並取得對應的屬性值
            price = Convert.ToInt32(resultJson["price"]);
            buyer = (string)resultJson["buyer"];
            product = (string)resultJson["product"];

            ViewBag.Info = buyer + "" + product + "" + price;
        }

        public async Task<string> ChatGPT(string askSomething)
        {
            var bot = new ChatGpt("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 ");

            var response = await bot.Ask(askSomething);

            return response;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}