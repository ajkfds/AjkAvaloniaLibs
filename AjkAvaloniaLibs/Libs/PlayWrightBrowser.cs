using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace AjkAvaloniaLibs.Libs
{
    public class PlayWrightBrowser
    {
        private System.Random rand = new Random();

        public IBrowser? Browser = null;
        public IBrowserContext? BrowserContext = null;
        public IPage? Page = null;
        public async Task Browse(string url)
        {
            using var playwright = await Playwright.CreateAsync();

            // Edge (Chromium) を起動
            Browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Channel = "msedge",
                Headless = false,
                Args = new[]
                {
                "--disable-blink-features=AutomationControlled",
                "--no-first-run",
                "--no-default-browser-check"
            }
            });

            BrowserContext = await Browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                            "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0",
                Locale = "ja-JP",
                TimezoneId = "Asia/Tokyo",
                ViewportSize = new ViewportSize { Width = 1366, Height = 768 }
            });

           Page = await BrowserContext.NewPageAsync();

            //// 最後まで待つ or スクリーンショットを取る
            //await page.WaitForTimeoutAsync(3000);
            //await page.ScreenshotAsync(new PageScreenshotOptions { Path = "sannysoft.png", FullPage = true });
            await Page.GotoAsync(url);
            await Task.Delay(rand.Next(100, 200));

//            await Page.FillAsync("#APjFqb", "Playwright");
//            await Page.Keyboard.PressAsync("Enter");

//            await Task.Delay(rand.Next(100, 200));
        }
    }
}
