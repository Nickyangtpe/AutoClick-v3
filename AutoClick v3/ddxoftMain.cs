using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace MouseMovementLibraries.ddxoftSupport
{
    internal class DdxoftMain
    {
        public static ddxoftMouse ddxoftInstance = new ddxoftMouse();
        private static readonly string ddxoftPath = "ddxoft.dll";
        private static readonly string ddxoftUri = "https://gitlab.com/marsqq/extra-files/-/raw/main/ddxoft.dll";

        private static async Task DownloadDdxoft(ProgressBar progressBar, Label label)
        {
            try
            {

                using (HttpClient httpClient = new HttpClient())
                {
                    progressBar.Visible = true;
                    label.Text = "Downloading...";

                    using (var response = await httpClient.GetAsync(ddxoftUri, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            MessageBox.Show($"Failed to download {ddxoftPath}. HTTP Status: {response.StatusCode}");
                            return;
                        }

                        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                        var buffer = new byte[8192]; // 8KB buffer
                        long bytesRead = 0;

                        using (var fileStream = new FileStream(ddxoftPath, FileMode.Create, FileAccess.Write))
                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            int read;
                            while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, read);
                                bytesRead += read;

                                if (totalBytes != -1)
                                {
                                    int progress = (int)((bytesRead * 100) / totalBytes);
                                    progressBar.Value = progress;
                                    label.Text = $"Downloaded {progress}%";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ddxoftPath} installation failed. Error: {ex.Message}");
            }
        }

        public static async Task<bool> DLLLoading(ProgressBar progressBar, Label label)
        {
            try
            {
                if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                {
                    MessageBox.Show("AutoClick v3 must be run as administrator to use the ddxoft Virtual Input Driver.", "AutoClick v3");
                    return false;
                }

                if (!File.Exists(ddxoftPath))
                {
                    await DownloadDdxoft(progressBar, label);
                    Process.Start(Application.ExecutablePath); // 重新啟動應用程式
                    File.WriteAllText(Path.Combine(Path.GetTempPath(),"Restart.bat"), $"taskkill /pid \"AutoClick v3.exe\" /f\n\"{Application.ExecutablePath}\"");
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(Path.GetTempPath(), "Restart.bat"),
                        UseShellExecute = false, // 不使用外殼執行
                        CreateNoWindow = true // 不顯示窗口
                    };

                    Process process = Process.Start(startInfo);
                    return true;
                }

                if (ddxoftInstance.Load(ddxoftPath) != 1 || ddxoftInstance.btn(0) != 1)
                {
                    MessageBox.Show("The ddxoft virtual input driver is not compatible with your PC.", "AutoClick");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load ddxoft virtual input driver.\n\n{ex}", "AutoClick v3");
                return false;
            }
        }

        public static async Task<bool> Load(ProgressBar progressBar, Label label) => await DLLLoading(progressBar, label);
    }
}
