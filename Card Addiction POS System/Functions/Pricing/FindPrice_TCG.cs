using System;
using System.Globalization;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Threading;

namespace Card_Addiction_POS_System.Functions.Pricing
{
    public class FindPrice_TCG : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly int _timeoutSeconds;
        private readonly bool _headless;

        /// <summary>
        /// Create a finder.
        /// timeoutSeconds: how long to wait for page elements (default 30).
        /// headless: by default false (websites often block headless); set true for CI if needed.
        /// </summary>
        public FindPrice_TCG(int timeoutSeconds = 30, bool headless = false)
        {
            _timeoutSeconds = timeoutSeconds;
            _headless = headless;

            var options = new ChromeOptions();

            if (_headless)
            {
                options.AddArgument("--headless=new");
                options.AddArgument("--disable-gpu");
            }

            // Helpful flags to reduce bot-detection and improve rendering
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddExcludedArgument("enable-automation");
            options.AddArgument("--window-size=1920,1080");

            // Set a real user-agent to reduce basic blocking
            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36");

            // Create driver and wait helper
            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(_timeoutSeconds));
        }

        /// <summary>
        /// Get the market price from a TCGPlayer product page.
        /// Throws on timeout or parse failure.
        /// </summary>
        public decimal GetMarketPrice(string tcgUrl)
        {
            if (string.IsNullOrWhiteSpace(tcgUrl))
                throw new ArgumentException("Price URL is empty.", nameof(tcgUrl));

            // We'll attempt a couple of times to handle transient failures.
            const int attempts = 2;
            Exception lastEx = null;

            for (int attempt = 1; attempt <= attempts; attempt++)
            {
                try
                {
                    _driver.Navigate().GoToUrl(tcgUrl);

                    // Primary selector used previously; keep as first choice.
                    string[] selectors =
                    {
                        "span.price-points__upper__price",
                        "span[itemprop='price']",          // fallback common selectors
                        ".product-price",                  // fallback
                        ".price",                          // very broad fallback
                    };

                    IWebElement priceElement = null;

                    foreach (var sel in selectors)
                    {
                        try
                        {
                            priceElement = _wait.Until(driver =>
                            {
                                try
                                {
                                    var el = driver.FindElement(By.CssSelector(sel));
                                    if (el != null && el.Displayed && !string.IsNullOrWhiteSpace(el.Text))
                                        return el;
                                }
                                catch (NoSuchElementException) { }
                                catch (StaleElementReferenceException) { }
                                return null;
                            });

                            if (priceElement != null)
                                break;
                        }
                        catch (WebDriverTimeoutException)
                        {
                            // try next selector
                        }
                    }

                    if (priceElement == null)
                        throw new WebDriverTimeoutException($"Timed out after {_timeoutSeconds}s waiting for price element.");

                    // Let client JS render/bind any final text (replicates your old Thread.Sleep)
                    Thread.Sleep(1000);

                    var rawText = priceElement.Text?.Trim() ?? string.Empty;

                    // If element text is empty try reading via JS (some pages use innerHTML updates)
                    if (string.IsNullOrEmpty(rawText))
                    {
                        try
                        {
                            var script = "return (document.querySelector(arguments[0]) || {}).innerText || '';";

                            rawText = ((IJavaScriptExecutor)_driver).ExecuteScript(script, "span.price-points__upper__price") as string ?? string.Empty;
                            rawText = rawText.Trim();
                        }
                        catch { rawText = string.Empty; }
                    }

                    if (string.IsNullOrEmpty(rawText))
                        throw new Exception("Price element contained no text.");

                    // Clean "$12.34" -> "12.34"
                    var rawPrice = rawText.Replace("$", string.Empty).Replace(",", string.Empty);

                    if (!decimal.TryParse(rawPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                        throw new FormatException($"Could not parse price from '{rawText}'.");

                    return price;
                }
                catch (WebDriverTimeoutException ex)
                {
                    lastEx = ex;
                    // small backoff before retry
                    Thread.Sleep(800);
                    continue;
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    // don't retry on parse failures — surface them
                    break;
                }
            }

            throw lastEx ?? new Exception("Unknown error while retrieving price.");
        }

        public void Dispose()
        {
            try
            {
                _driver?.Quit();
                _driver?.Dispose();
            }
            catch { /* ignore shutdown errors */ }
        }
    }
}
