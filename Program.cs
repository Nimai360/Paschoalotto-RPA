using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V120.Debugger;
using OpenQA.Selenium.DevTools.V85.Profiler;

namespace Paschoalotto_RPA
{
    class Program
    {
        public static readonly string diretorio = AppDomain.CurrentDomain.BaseDirectory.ToString();

        DbPostgres? dbPostgres;
        static readonly JsonElement json = ReadJsonFile(diretorio + "/FastFingers.json");

        static void Main(string[] args)
        {
            // Foi escolhido as bibliotecas informadas em requeriments.txt visando o melhor desempenho e acesso as informações.
            // buscando deixar o sistema o mais eficiente possível.

            Program program = new();
            WebDriverUtils? webDriver = new WebDriverUtils();

            Console.Title = "10 Fast Fingers";
            Console.SetWindowSize(60, Console.WindowHeight);
            Console.ForegroundColor = ConsoleColor.Green;

            webDriver.AtualizacaoAutomaticaWebDriver();

            if (webDriver != null)
            {
                try
                {
                    program.FastFingersWebScraper(webDriver);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    webDriver.Exit();
                }
            }
            Exit();
        }
        public void FastFingersWebScraper(WebDriverUtils webDriver)
        {
            FastFingers fastFingers = new(webDriver);
            fastFingers.GoToHomePage();
            fastFingers.AcceptCookies();

            Console.WriteLine($"[{DateTime.Now}] Vamos iniciar o RPA");
            string[] list = fastFingers.GetWordsList(fastFingers.GetElementWordsList());
            IWebElement inputField = fastFingers.GetElementInputField();

            DateTime agora = DateTime.Now;
            foreach (string word in list)
            {
                fastFingers.InsertWord(inputField, word);
            }
            Console.WriteLine($"[{DateTime.Now}] Terminado de inserir as palavras");
            fastFingers.CloseAlert((int)(DateTime.Now - agora).TotalSeconds + 1);

            JsonElement results = fastFingers.GetResults();

            dbPostgres ??= new();
            dbPostgres.InsertResults(results);
            Console.WriteLine($"[{DateTime.Now}] RPA Finalizado");
        }

        private static void Exit()
        {
            Environment.Exit(0);
        }
        static JsonElement ReadJsonFile(string filePath)
        {
            try
            {
                string fullPath = Path.Combine(diretorio, filePath);
                return JsonDocument.Parse(File.ReadAllText(fullPath)).RootElement;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na leitura do arquivo JSON: {ex.Message}");
            }
        }
    }
}