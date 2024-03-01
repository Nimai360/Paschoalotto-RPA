using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace Paschoalotto_RPA
{
    public class WebDriverUtils
    {
        private readonly int timeOut = 2;
        private readonly WebDriver driver;

        public WebDriverUtils()
        {
            driver = (WebDriver)ConfigDriver();
        }

        public IWebElement? FindElement(TypePath type, string path, string alt_XPath = "", bool waitUntilVisible = true, string scrollElementTo = "middle", IWebElement? parent = null)
        {
            // Localiza o elemento na página
            try
            {
                if (parent != null)
                {
                    path = "." + path;
                    alt_XPath = "." + alt_XPath;
                }

                By elem = GetByType(type, path, alt_XPath);

                if (parent == null)
                {
                    IWebElement? element = waitUntilVisible ? WaitElementVisible(elem) : WaitElementExist(elem);

                    if (element != null)
                    {
                        ScrollElement(scrollElementTo, element);
                    }

                    return element;
                }

                return parent.FindElement(elem);
            }
            catch
            {
                Console.WriteLine($"Não foi possível localizar o elemento");
                return null;
            }
        }

        public IList<IWebElement>? FindElementsByParent(TypePath type, IWebElement parentElement, string path, string alt_XPath = "")
        {
            // Localiza os elementos no elemento pai
            try
            {
                By elem = GetByType(type, path, alt_XPath);
                return parentElement.FindElements(elem);
            }
            catch
            {
                Console.WriteLine($"Não foi possível localizar o elemento no nó pai");
                return null;
            }
        }

        public IList<IWebElement>? FindElements(TypePath type, string path, string alt_XPath = "")
        {
            // Localiza os elementos na página
            try
            {
                By elem = GetByType(type, path, alt_XPath);
                return driver.FindElements(elem);
            }
            catch
            {
                Console.WriteLine($"Não foi possível localizar os elementos");
                return null;
            }
        }
        private By GetByType(TypePath type, string path, string alt_XPath = "")
        {
            return type switch
            {
                TypePath.ID => By.Id(path),
                TypePath.CLASS_NAME => By.ClassName(path),
                TypePath.NAME => By.Name(path),
                TypePath.CSS_SELECTOR => By.CssSelector(path),
                TypePath.TAG_NAME => By.TagName(path),
                TypePath.XPATH => By.XPath(path),
                _ => By.XPath(alt_XPath)
            };
        }
        public string? GetDataByXPathFromStringHtmlNode(string nodeHtml, TypePath type, string path, TypeDataFromHtmlNode typeReturnDataFromHtmlNode)
        {
            HtmlDocument document = new();
            document.LoadHtml(nodeHtml);
            HtmlNode data = document.DocumentNode.SelectSingleNode($"//*[@{type.ToLowerString()}='{path}']");
            if (data != null)
            {
                return typeReturnDataFromHtmlNode switch
                {
                    TypeDataFromHtmlNode.INNER_HTML => data.InnerHtml,
                    TypeDataFromHtmlNode.OUTER_HTML => data.OuterHtml,
                    TypeDataFromHtmlNode.INNER_TEXT => data.InnerText,
                    _ => null
                };
            }

            Console.WriteLine("Elemento não encontrado.");
            return null;
        }

        public void SendKeys(IWebElement elem, string text, bool clearBefore = false)
        {
            if (clearBefore) elem?.Clear();
            elem?.SendKeys(text);
        }

        public IWebElement? WaitElementVisible(By elementoEsperado)
        {
            try
            {
                WebDriverWait wait = new(driver, TimeSpan.FromSeconds(timeOut));
                return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(elementoEsperado));
            }
            catch
            {
                Console.WriteLine($"Não foi possível localizar o elemento");
                return null;
            }
        }
        public IWebElement? WaitElementExist(By elementoEsperado)
        {

            WebDriverWait wait = new(driver, TimeSpan.FromSeconds(timeOut));

            if (wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementWithText(elementoEsperado, "|")))
                return driver.FindElement(elementoEsperado);

            Console.WriteLine($"Não foi possível localizar o elemento");
            return null;
        }
        private void ScrollElement(string scrollType, IWebElement elemento)
        {
            if (elemento != null)
            {
                switch (scrollType.ToLower())
                {
                    case "top":
                        ScrollElementoToTopOfScreen(elemento);
                        break;
                    case "middle":
                        ScrollElementToMiddleOfScreen(elemento);
                        break;
                }
            }
        }

        public void ScrollElementToMiddleOfScreen(IWebElement element)
        {
            if (element != null)
            {
                var windowHeight = (int)((IJavaScriptExecutor)driver).ExecuteScript("return window.innerHeight;");
                var middleOfScreen = windowHeight / 2;
                var elementPosition = (int)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].getBoundingClientRect().top;", element);
                var scrollPosition = elementPosition - middleOfScreen;
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, arguments[0]);", scrollPosition);
            }
        }

        public void ScrollElementoToTopOfScreen(IWebElement element)
        {
            if (element != null)
            {
                driver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", element);
            }
        }
        public void CloseAlert()
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            wait.Until(ExpectedConditions.AlertIsPresent());
            IAlert alert = driver.SwitchTo().Alert();
            alert.Accept();
        }

        public IWebDriver ConfigDriver()
        {
            // Cria instancia do WebDriver
            ChromeOptions chromeOptions = new();
            chromeOptions.AddArguments([
        "--disable-plugins-discovery",
        "--disable-popup-blocking",
        "--start-maximized",
        "disable-infobars",
        "--disable-notifications",
        "--ignore-certificate-errors",
        "--disable-extensions",
        "--silent",
        "--log-level=3",
        "--mute-audio",
        "--incognito",
        //"--headless" // Adicionar para não ter que carregar a parte gráfica, e assim otimizar o tempo
    ]);
            chromeOptions.AddExcludedArgument("enable-automation"); // remove mensagem de navegador de automação

            return new ChromeDriver(chromeOptions);
        }

        // Atualiza o webdriver para a versão mais recente
        public void AtualizacaoAutomaticaWebDriver() => new DriverManager().SetUpDriver(new ChromeConfig());

        public void NavigateTo(String url) => driver.Navigate().GoToUrl(url);

        public WebDriver GetDriver() => driver;
        public String GetCurrentUrl() => driver.Url;

        public void Exit() => driver.Quit();
    }
}
