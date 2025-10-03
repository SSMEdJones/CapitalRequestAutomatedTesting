using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IPdfService
    {
        Task<byte[]> GeneratePdfFromHtmlAsync(string html);
        Task<byte[]> GeneratePdfFromUrlAsync(string url);
    }

    public class PdfService : IPdfService
    {
        private readonly ILogger<PdfService> _logger;
        private static bool _browserDownloaded = false;
        private static readonly SemaphoreSlim _downloadSemaphore = new(1, 1);

        public PdfService(ILogger<PdfService> logger)
        {
            _logger = logger;
        }

        private async Task EnsureBrowserDownloadedAsync()
        {
            if (_browserDownloaded) return;

            await _downloadSemaphore.WaitAsync();
            try
            {
                if (!_browserDownloaded)
                {
                    _logger.LogInformation("Downloading Chromium for PDF generation...");
                    await new BrowserFetcher().DownloadAsync();
                    _browserDownloaded = true;
                }
            }
            finally
            {
                _downloadSemaphore.Release();
            }
        }

        public async Task<byte[]> GeneratePdfFromHtmlAsync(string html)
        {
            try
            {
                await EnsureBrowserDownloadedAsync();

                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
                });

                using var page = await browser.NewPageAsync();
                await page.SetContentAsync(html);

                var pdfOptions = new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "20px",
                        Bottom = "20px",
                        Left = "15px",
                        Right = "15px"
                    }
                };

                return await page.PdfDataAsync(pdfOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF");
                throw;
            }
        }

        public async Task<byte[]> GeneratePdfFromUrlAsync(string url)
        {
            try
            {
                await EnsureBrowserDownloadedAsync();

                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
                });

                using var page = await browser.NewPageAsync();
                await page.GoToAsync(url);

                var pdfOptions = new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true
                };

                return await page.PdfDataAsync(pdfOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF from URL: {Url}", url);
                throw;
            }
        }
    }
}