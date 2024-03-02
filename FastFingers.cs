using System.Text.Json;
using OpenQA.Selenium;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Html;
using System.Globalization;

namespace Paschoalotto_RPA
{
    public class FastFingers
    {
        public static readonly string diretorio = AppDomain.CurrentDomain.BaseDirectory.ToString();
        WebDriverUtils webDriverUtils;
        static readonly JsonElement json = JsonDocument.Parse(File.ReadAllText(diretorio + "/FastFingers.json")).RootElement;

        public FastFingers(WebDriverUtils webDriverUtils)
        {
            this.webDriverUtils = webDriverUtils;
        }

        public void AcceptCookies()
        {
            try
            {
                webDriverUtils.FindElement(TypePath.ID, json.GetProperty("decline_cookies").GetString(), scrollElementTo: "none")?.Click();
            }
            catch (Exception) { }
        }

        public IWebElement GetElementWordsList() => webDriverUtils.FindElement(TypePath.ID, json.GetProperty("listaPalavras_id").GetString(), waitUntilVisible: false, scrollElementTo: "none");

        public IWebElement GetElementInputField() => webDriverUtils.FindElement(TypePath.ID, json.GetProperty("inputField_id").GetString(), scrollElementTo: "none");

        public IWebElement GetResultsPanel() => webDriverUtils.FindElement(TypePath.ID, json.GetProperty("resultPanel_id").GetString(), waitUntilVisible: true, scrollElementTo: "top");

        public string[] GetWordsList(IWebElement elemList) => GetWordsFromHtmlElement(elemList).Trim().Split("|");
        private string GetWordsFromHtmlElement(IWebElement elemList) => webDriverUtils.GetDataByXPathFromStringHtmlNode(
                elemList.GetAttribute("outerHTML"),
                TypePath.ID,
                json.GetProperty("listaPalavras_id").GetString(),
                TypeDataFromHtmlNode.INNER_TEXT
                );

        public void InsertWord(IWebElement inputField, string word)
        {
            webDriverUtils.SendKeys(inputField, word, clearBefore: true);
            webDriverUtils.SendKeys(inputField, Keys.Space);
        }
        public void CloseAlert(int timeWait) => webDriverUtils.CloseAlert(timeWait);
        public JsonElement GetResults() => GetJsonResults(GetResultsPanel().GetAttribute("outerHTML"));

        private JsonElement GetJsonResults(string elem)
        {
            if (string.IsNullOrWhiteSpace(elem))
            {
                Console.WriteLine($"Elemento vazio");
                return new JsonElement();
            }

            string wpm = webDriverUtils.GetDataByXPathFromStringHtmlNode(elem, TypePath.ID, json.GetProperty("resultWpm_id").GetString(), TypeDataFromHtmlNode.INNER_TEXT);
            string keystrokes = webDriverUtils.GetDataByXPathFromStringHtmlNode(elem, TypePath.ID, json.GetProperty("resultKeystrokes_id").GetString(), TypeDataFromHtmlNode.INNER_TEXT);
            string accuracy = webDriverUtils.GetDataByXPathFromStringHtmlNode(elem, TypePath.ID, json.GetProperty("resultAccuracy_id").GetString(), TypeDataFromHtmlNode.INNER_TEXT);
            string correct = webDriverUtils.GetDataByXPathFromStringHtmlNode(elem, TypePath.ID, json.GetProperty("resultCorrect_id").GetString(), TypeDataFromHtmlNode.INNER_TEXT);
            string wrong = webDriverUtils.GetDataByXPathFromStringHtmlNode(elem, TypePath.ID, json.GetProperty("resultWrong_id").GetString(), TypeDataFromHtmlNode.INNER_TEXT);

            wpm = wpm.Split(" ")[0];
            keystrokes = keystrokes.Split(" ")[^1].Split(";")[1];
            accuracy = accuracy.Split(" ")[1].Replace("%", "");
            correct = correct.Split(" ")[^1];
            wrong = wrong.Split(" ")[^1];

            JsonElement jsonResult = JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                wpm = int.Parse(wpm),
                keystrokes = int.Parse(keystrokes),
                accuracy = double.Parse(accuracy, CultureInfo.InvariantCulture),
                correct_word = int.Parse(correct),
                wrong_word = int.Parse(wrong)
            }
            )).RootElement;

            return jsonResult;
        }

        public void GoToHomePage() => webDriverUtils.NavigateTo(json.GetProperty("websiteUrl").GetString());
    }
}
