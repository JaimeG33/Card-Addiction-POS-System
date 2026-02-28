using System;
using System.Globalization;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Threading;

namespace Card_Addiction_POS_System.Functions.Pricing
{
    /// <summary>
    /// FindPrice_TCG
    /// ----------------
    /// Responsible for retrieving a market price from a TCGPlayer product page.
    /// 
    /// Design notes:
    /// - Uses a single shared ChromeDriver instance (managed by the nested DriverManager) to avoid repeatedly starting
    ///   and stopping browser processes. This reduces resource usage and avoids leaving many browser windows/tabs open.
    /// - The shared driver reuses one tab/window for each lookup. The code navigates the existing tab to the requested URL.
    /// - Consumers should call <see cref="ShutdownSharedDriver"/> during application shutdown to properly quit the browser.
    /// - This class is disposable but does NOT dispose the shared driver; disposal here only follows the pattern so instances
    ///   can be used in using-blocks without implying the shared browser will be torn down.
    /// 
    /// Usage template (how to call this from other forms or services):
    /// --------------------------------------------------------------
    /// // Synchronous call (not recommended on UI thread):
    /// var finder = new FindPrice_TCG(timeoutSeconds: 30, headless: false);
    /// try
    /// {
    ///     decimal price = finder.GetMarketPrice("https://www.tcgplayer.com/product/...");
    ///     // Use price in UI (invoke on UI thread if needed)
    /// }
    /// finally
    /// {
    ///     finder.Dispose(); // optional: does not shutdown shared driver
    /// }
    ///
    /// // Recommended pattern: create a single instance per logical unit or request and optionally call
    /// // FindPrice_TCG.ShutdownSharedDriver() at application shutdown to completely quit the shared browser.
    ///
    /// // If you need the shared browser for many lookups, reuse the same instance or rely on the shared driver:
    /// using (var f = new FindPrice_TCG())
    /// {
    ///     var price = f.GetMarketPrice(url);
    ///     // ...
    /// }
    ///
    /// // To fully stop the background browser at app exit:
    /// FindPrice_TCG.ShutdownSharedDriver();
    /// </summary>
    public class FindPrice_TCG : IDisposable
    {
        private readonly int _timeoutSeconds;
        private readonly bool _headless;
        private bool _disposed;

        public FindPrice_TCG(int timeoutSeconds = 30, bool headless = false)
        {
            _timeoutSeconds = timeoutSeconds;
            _headless = headless;
            // Do not create browser here; it will be created lazily by DriverManager when first needed.
        }

        /// <summary>
        /// Get the market price from a TCGPlayer product page using a single shared browser tab.
        /// </summary>
        public decimal GetMarketPrice(string tcgUrl)
        {
            if (string.IsNullOrWhiteSpace(tcgUrl))
                throw new ArgumentException("Price URL is empty.", nameof(tcgUrl));

            // Use the shared driver managed by DriverManager.
            var driver = DriverManager.GetOrCreate(_timeoutSeconds, _headless);
            var wait = DriverManager.GetWait();

            // We'll attempt a couple of times to handle transient failures.
            const int attempts = 2;
            Exception lastEx = null;

            for (int attempt = 1; attempt <= attempts; attempt++)
            {
                try
                {
                    // Ensure we're using the same main tab/window
                    DriverManager.EnsureMainWindow();

                    // Navigate the single tab to the requested URL (this reuses the existing tab)
                    driver.Navigate().GoToUrl(tcgUrl);

                    string[] selectors =
                    {
                        "span.price-points__upper__price",
                        "span[itemprop='price']",
                        ".product-price",
                        ".price",
                    };

                    IWebElement priceElement = null;

                    foreach (var sel in selectors)
                    {
                        try
                        {
                            priceElement = wait.Until(drv =>
                            {
                                try
                                {
                                    var el = drv.FindElement(By.CssSelector(sel));
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

                    // Give client-side frameworks a moment to finish binding
                    Thread.Sleep(1000);

                    var rawText = priceElement.Text?.Trim() ?? string.Empty;

                    // If text empty try JS read
                    if (string.IsNullOrEmpty(rawText))
                    {
                        try
                        {
                            var script = "return (document.querySelector(arguments[0]) || {}).innerText || '';";

                            rawText = ((IJavaScriptExecutor)driver).ExecuteScript(script, "span.price-points__upper__price") as string ?? string.Empty;
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

                    // Minimize the browser window/tab now so the main application won't be obstructed.
                    // Do this before returning so any UI notifications in the app won't be covered.
                    DriverManager.MinimizeMainWindow();

                    return price;
                }
                catch (WebDriverTimeoutException ex)
                {
                    lastEx = ex;
                    Thread.Sleep(800);
                    continue;
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    break;
                }
            }

            // Attempt to minimize even on failure so any driver window isn't left in front of the app.
            try { DriverManager.MinimizeMainWindow(); } catch { /* swallow */ }

            throw lastEx ?? new Exception("Unknown error while retrieving price.");
        }

        /// <summary>
        /// Dispose pattern: this instance does not dispose the shared driver.
        /// To explicitly shut down the shared browser, call <see cref="ShutdownSharedDriver"/>.
        /// </summary>

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;
            // Nothing to dispose per-instance: driver is managed by DriverManager.
        }

        /// <summary>
        /// Shutdown the shared browser (quit and dispose). Call on application exit or when you want to free resources.
        /// Safe to call multiple times.
        /// </summary>
        public static void ShutdownSharedDriver()
        {
            DriverManager.QuitAndDispose();
        }

        // Internal driver manager: single shared ChromeDriver and WebDriverWait.
        private static class DriverManager
        {
            private static readonly object _sync = new();
            private static IWebDriver? _sharedDriver;
            private static WebDriverWait? _sharedWait;
            private static string? _mainWindowHandle;
            private static int _currentTimeout = 30;
            private static bool _currentHeadless = false;

            public static IWebDriver GetOrCreate(int timeoutSeconds, bool headless)
            {
                lock (_sync)
                {
                    // If parameters changed and driver exists, recreate to respect new options
                    if (_sharedDriver != null && (_currentTimeout != timeoutSeconds || _currentHeadless != headless))
                    {
                        QuitAndDisposeInternal();
                    }

                    // If driver exists, check health
                    if (_sharedDriver != null)
                    {
                        try
                        {
                            // access a property to detect if the driver is alive
                            var _ = _sharedDriver.WindowHandles;
                            return _sharedDriver;
                        }
                        catch
                        {
                            // driver is dead; dispose and recreate
                            QuitAndDisposeInternal();
                        }
                    }

                    // Create options
                    var options = new ChromeOptions();
                    if (headless)
                    {
                        options.AddArgument("--headless=new");
                        options.AddArgument("--disable-gpu");
                    }

                    options.AddArgument("--no-sandbox");
                    options.AddArgument("--disable-dev-shm-usage");
                    options.AddArgument("--disable-blink-features=AutomationControlled");
                    options.AddExcludedArgument("enable-automation");
                    options.AddArgument("--window-size=1920,1080");
                    // Try to start minimized so it doesn't obstruct the user.
                    options.AddArgument("--start-minimized");
                    options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36");

                    _sharedDriver = new ChromeDriver(options);
                    _sharedWait = new WebDriverWait(_sharedDriver, TimeSpan.FromSeconds(timeoutSeconds));
                    _mainWindowHandle = _sharedDriver.CurrentWindowHandle;
                    _currentTimeout = timeoutSeconds;
                    _currentHeadless = headless;

                    return _sharedDriver;
                }
            }

            public static WebDriverWait GetWait()
            {
                lock (_sync)
                {
                    if (_sharedWait == null)
                        throw new InvalidOperationException("Shared driver not initialized. Call GetOrCreate first.");
                    return _sharedWait;
                }
            }

            public static void EnsureMainWindow()
            {
                lock (_sync)
                {
                    if (_sharedDriver == null) return;

                    try
                    {
                        // If our remembered handle isn't present (user closed it), grab the first available and remember it.
                        var handles = _sharedDriver.WindowHandles;
                        if (string.IsNullOrEmpty(_mainWindowHandle) || !handles.Contains(_mainWindowHandle))
                        {
                            _mainWindowHandle = handles.Count > 0 ? handles[0] : null;
                        }

                        if (!string.IsNullOrEmpty(_mainWindowHandle))
                        {
                            _sharedDriver.SwitchTo().Window(_mainWindowHandle);
                        }
                    }
                    catch
                    {
                        // If switching fails, attempt to recreate the driver next time by disposing now.
                        QuitAndDisposeInternal();
                    }
                }
            }

            /// <summary>
            /// Minimize the main driver window. Safe to call repeatedly. Swallows exceptions to avoid interfering with lookup flow.
            /// </summary>
            public static void MinimizeMainWindow()
            {
                lock (_sync)
                {
                    if (_sharedDriver == null) return;

                    try
                    {
                        // Try to minimize the window. In headless mode or some environments this may be a no-op or throw; swallow errors.
                        _sharedDriver.Manage().Window.Minimize();
                    }
                    catch
                    {
                        // swallow any minimize errors to avoid impacting the caller
                    }
                }
            }

            public static void QuitAndDispose()
            {
                lock (_sync)
                {
                    QuitAndDisposeInternal();
                }
            }

            private static void QuitAndDisposeInternal()
            {
                if (_sharedDriver != null)
                {
                    try { _sharedDriver.Quit(); } catch { /* swallow */ }
                    try { _sharedDriver.Dispose(); } catch { /* swallow */ }
                }

                _sharedDriver = null;
                _sharedWait = null;
                _mainWindowHandle = null;
            }
        }
    }
}
