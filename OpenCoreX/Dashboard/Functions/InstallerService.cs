using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenCoreX.Dashboard.Functions
{
    public class InstallableProgram
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public string SilentArgs { get; set; } = "";
        public string IconKind { get; set; } = "Application"; // Placeholder for icon logic
    }

    public class InstallerService
    {
        public List<InstallableProgram> GetPrograms()
        {
            return new List<InstallableProgram>
            {
                new InstallableProgram 
                { 
                    Name = "Google Chrome", 
                    Description = "Fast, secure, and free web browser.", 
                    DownloadUrl = "https://dl.google.com/tag/s/appguid%3D%7B8A69D345-D564-463C-AFF1-A69D9E530F96%7D%26iid%3D%7B36E22C9B-1919-C064-946D-24017C7E7796%7D%26lang%3Den%26browser%3D3%26usagestats%3D0%26appname%3DGoogle%2520Chrome%26needsadmin%3Dprefers%26ap%3Dx64-stable-statsdef_1%26installdataindex%3Dempty/update2/installers/ChromeSetup.exe",
                    SilentArgs = "/silent /install"
                },
                new InstallableProgram 
                { 
                    Name = "Mozilla Firefox", 
                    Description = "Free and open-source web browser.", 
                    DownloadUrl = "https://download.mozilla.org/?product=firefox-latest&os=win64&lang=en-US", 
                    SilentArgs = "-ms" 
                },
                new InstallableProgram 
                { 
                    Name = "VLC Media Player", 
                    Description = "Free and open source cross-platform multimedia player.", 
                    DownloadUrl = "https://get.videolan.org/vlc/3.0.18/win64/vlc-3.0.18-win64.exe", 
                    SilentArgs = "/S" 
                },
                new InstallableProgram 
                { 
                    Name = "7-Zip", 
                    Description = "File archiver with a high compression ratio.", 
                    DownloadUrl = "https://www.7-zip.org/a/7z2301-x64.exe", 
                    SilentArgs = "/S" 
                },
                new InstallableProgram 
                { 
                    Name = "MemReduct", 
                    Description = "Lightweight real-time memory management application.", 
                    DownloadUrl = "https://github.com/henrypp/memreduct/releases/latest/download/memreduct-3.4-setup.exe", 
                    SilentArgs = "/S" 
                }
            };
        }

        public async Task InstallProgramAsync(InstallableProgram program, IProgress<string> statusReporter)
        {
            string tempPath = Path.GetTempPath();
            string installerPath = Path.Combine(tempPath, $"{program.Name.Replace(" ", "")}_setup.exe");

            try
            {
                statusReporter.Report($"Downloading {program.Name}...");
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(program.DownloadUrl);
                    response.EnsureSuccessStatusCode();
                    using (var fs = new FileStream(installerPath, FileMode.Create))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }

                statusReporter.Report($"Installing {program.Name}...");
                var psi = new ProcessStartInfo
                {
                    FileName = installerPath,
                    Arguments = program.SilentArgs,
                    UseShellExecute = true, // needed for some installers if not running as admin directly, but mostly fine
                    Verb = "runas", // Request admin
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var process = Process.Start(psi);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    statusReporter.Report($"{program.Name} Installed Successfully!");
                }
            }
            catch (Exception ex)
            {
                statusReporter.Report($"Failed to install {program.Name}: {ex.Message}");
            }
            finally
            {
                if (File.Exists(installerPath))
                {
                    try { File.Delete(installerPath); } catch { }
                }
            }
        }
    }
}
