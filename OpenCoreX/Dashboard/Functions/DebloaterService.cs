using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace OpenCoreX.Dashboard.Functions
{
    public class DebloatOptions
    {
        // App Removal
        public bool RemoveOneDrive { get; set; }
        public bool RemoveCortana { get; set; }
        public bool RemoveXbox { get; set; }
        public bool RemoveSkype { get; set; }
        public bool RemoveWeather { get; set; }
        public bool RemoveNews { get; set; }
        public bool RemoveFeedback { get; set; }
        public bool RemoveGetHelp { get; set; }
        public bool RemoveTips { get; set; }
        public bool RemoveMaps { get; set; }
        public bool RemoveSolitaire { get; set; }
        public bool RemovePeople { get; set; }
        public bool RemoveYourPhone { get; set; }
        public bool RemovePhotos { get; set; }
        public bool RemoveCalculator { get; set; }

        // Privacy
        public bool DisableTelemetry { get; set; }
        public bool DisableAds { get; set; }
        public bool DisableLocation { get; set; }
        public bool DisableCortanaVoice { get; set; }
        public bool DisableErrorReporting { get; set; }
        public bool DisableFeedbackNotif { get; set; }
        public bool RestrictBackgroundApps { get; set; }
        public bool DisableStartSuggestions { get; set; }

        // Gaming & Performance
        public bool EnableUltimatePerformance { get; set; }
        public bool DisableGameDVR { get; set; }
        public bool DisableHibernation { get; set; }
        public bool DisableVisualEffects { get; set; }
        public bool DisableMouseAccel { get; set; }
        public bool DisableStickyKeys { get; set; }
        public bool OptimizeNetwork { get; set; }
    }

    public class DebloaterService
    {
        public string GenerateScript(DebloatOptions options)
        {
            var sb = new StringBuilder();
            sb.AppendLine("$ProgressPreference = 'SilentlyContinue'");
            sb.AppendLine("Write-Host 'Starting System Optimization...' -ForegroundColor Cyan");
            sb.AppendLine("Start-Sleep -Seconds 1");

            // --- App Removal ---
            if (options.RemoveOneDrive)
            {
                sb.AppendLine("Write-Host 'Removing OneDrive...'");
                sb.AppendLine("taskkill /f /im OneDrive.exe 2>$null");
                sb.AppendLine("if (Test-Path \"$env:SystemRoot\\System32\\OneDriveSetup.exe\") { Start-Process \"$env:SystemRoot\\System32\\OneDriveSetup.exe\" -ArgumentList \"/uninstall\" -Wait }");
                sb.AppendLine("Write-Host 'OneDrive removed.' -ForegroundColor Green");
            }

            if (options.RemoveCortana)
            {
                sb.AppendLine("Write-Host 'Removing Cortana...'");
                sb.AppendLine("Get-AppxPackage -allusers Microsoft.549981C3F5F10 | Remove-AppxPackage");
                sb.AppendLine("Write-Host 'Cortana removed.' -ForegroundColor Green");
            }

            if (options.RemoveXbox)
            {
                sb.AppendLine("Write-Host 'Removing Xbox Apps...'");
                sb.AppendLine("Get-AppxPackage -AllUsers Microsoft.XboxGamingOverlay | Remove-AppxPackage");
                sb.AppendLine("Get-AppxPackage -AllUsers Microsoft.XboxApp | Remove-AppxPackage");
                sb.AppendLine("Get-AppxPackage -AllUsers Microsoft.Xbox.TCUI | Remove-AppxPackage");
                sb.AppendLine("Get-AppxPackage -AllUsers Microsoft.XboxSpeechToTextOverlay | Remove-AppxPackage");
                sb.AppendLine("Get-AppxPackage -AllUsers Microsoft.XboxGameOverlay | Remove-AppxPackage");
                sb.AppendLine("Write-Host 'Xbox Apps removed.' -ForegroundColor Green");
            }

            if (options.RemoveSkype)
            {
                sb.AppendLine("Write-Host 'Removing Skype...'");
                sb.AppendLine("Get-AppxPackage -AllUsers Microsoft.SkypeApp | Remove-AppxPackage");
                sb.AppendLine("Write-Host 'Skype removed.' -ForegroundColor Green");
            }

            // ... Add simplified removal commands for other apps to save space, but functional ...
            void RemoveApp(bool condition, string name, string packageName)
            {
                if (condition)
                {
                    sb.AppendLine($"Write-Host 'Removing {name}...'");
                    sb.AppendLine($"Get-AppxPackage -AllUsers {packageName} | Remove-AppxPackage");
                    sb.AppendLine($"Write-Host '{name} removed.' -ForegroundColor Green");
                }
            }

            RemoveApp(options.RemoveWeather, "Weather", "Microsoft.BingWeather");
            RemoveApp(options.RemoveNews, "News", "Microsoft.BingNews");
            RemoveApp(options.RemoveFeedback, "Feedback Hub", "Microsoft.WindowsFeedbackHub");
            RemoveApp(options.RemoveGetHelp, "Get Help", "Microsoft.GetHelp");
            RemoveApp(options.RemoveTips, "Tips", "Microsoft.Getstarted");
            RemoveApp(options.RemoveMaps, "Maps", "Microsoft.WindowsMaps");
            RemoveApp(options.RemoveSolitaire, "Solitaire Collection", "Microsoft.MicrosoftSolitaireCollection");
            RemoveApp(options.RemovePeople, "People", "Microsoft.People");
            RemoveApp(options.RemoveYourPhone, "Your Phone", "Microsoft.YourPhone");
            RemoveApp(options.RemovePhotos, "Photos", "Microsoft.Windows.Photos");
            RemoveApp(options.RemoveCalculator, "Calculator", "Microsoft.WindowsCalculator");


            // --- Privacy & Security ---
            if (options.DisableTelemetry)
            {
                sb.AppendLine("Write-Host 'Disabling Telemetry...'");
                sb.AppendLine("Set-ItemProperty -Path 'HKLM:\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection' -Name 'AllowTelemetry' -Type DWord -Value 0");
                sb.AppendLine("Set-ItemProperty -Path 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\DataCollection' -Name 'AllowTelemetry' -Type DWord -Value 0");
                sb.AppendLine("Disable-ScheduledTask -TaskName 'Microsoft\\Windows\\Customer Experience Improvement Program\\Consolidator' -ErrorAction SilentlyContinue");
            }

            if (options.DisableAds)
            {
                sb.AppendLine("Write-Host 'Disabling Advertising ID...'");
                sb.AppendLine("Set-ItemProperty -Path 'HKLM:\\SOFTWARE\\Policies\\Microsoft\\Windows\\AdvertisingInfo' -Name 'DisabledByGroupPolicy' -Type DWord -Value 1");
                sb.AppendLine("Set-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\AdvertisingInfo' -Name 'Enabled' -Type DWord -Value 0");
            }

            if (options.DisableLocation)
            {
                sb.AppendLine("Write-Host 'Disabling Location Tracking...'");
                sb.AppendLine("Set-ItemProperty -Path 'HKLM:\\SOFTWARE\\Policies\\Microsoft\\Windows\\LocationAndSensors' -Name 'DisableLocation' -Type DWord -Value 1");
            }

            if (options.DisableCortanaVoice)
            {
                sb.AppendLine("Write-Host 'Disabling Cortana Voice...'");
                 sb.AppendLine("Set-ItemProperty -Path 'HKLM:\\SOFTWARE\\Policies\\Microsoft\\Windows\\Windows Search' -Name 'AllowCortana' -Type DWord -Value 0");
            }

            if (options.DisableErrorReporting)
            {
                sb.AppendLine("Write-Host 'Disabling Error Reporting...'");
                sb.AppendLine("Set-ItemProperty -Path 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting' -Name 'Disabled' -Type DWord -Value 1");
            }

            if (options.DisableFeedbackNotif)
            {
                sb.AppendLine("Write-Host 'Disabling Feedback Notifications...'");
                sb.AppendLine("Set-ItemProperty -Path 'HKLM:\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection' -Name 'DoNotShowFeedbackNotifications' -Type DWord -Value 1");
            }

            // --- Gaming & Performance ---
            if (options.EnableUltimatePerformance)
            {
                sb.AppendLine("Write-Host 'Enabling Ultimate Performance Plan...'");
                sb.AppendLine("powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61");
                sb.AppendLine("Write-Host 'Ultimate Performance Plan added. Please select it in Power Options.' -ForegroundColor Yellow");
            }

            if (options.DisableGameDVR)
            {
                sb.AppendLine("Write-Host 'Disabling GameDVR...'");
                sb.AppendLine("Set-ItemProperty -Path 'HKCU:\\System\\GameConfigStore' -Name 'GameDVR_Enabled' -Type DWord -Value 0");
                sb.AppendLine("Set-ItemProperty -Path 'HKLM:\\SOFTWARE\\Policies\\Microsoft\\Windows\\GameDVR' -Name 'AllowGameDVR' -Type DWord -Value 0");
            }

            if (options.DisableHibernation)
            {
                sb.AppendLine("Write-Host 'Disabling Hibernation...'");
                sb.AppendLine("powercfg /h off");
            }

            if (options.DisableVisualEffects)
            {
                sb.AppendLine("Write-Host 'Disabling Transparency...'");
                sb.AppendLine("Set-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize' -Name 'EnableTransparency' -Type DWord -Value 0");
            }

             if (options.DisableStickyKeys)
            {
                sb.AppendLine("Write-Host 'Disabling Sticky Keys...'");
                sb.AppendLine("Set-ItemProperty -Path 'HKCU:\\Control Panel\\Accessibility\\StickyKeys' -Name 'Flags' -Type String -Value '506'");
            }
            
            if (options.OptimizeNetwork)
            {
                sb.AppendLine("Write-Host 'Optimizing Network Throttling...'");
                sb.AppendLine("Set-ItemProperty -Path 'HKLM:\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile' -Name 'NetworkThrottlingIndex' -Type DWord -Value 4294967295");
            }


            sb.AppendLine("Start-Sleep -Seconds 1");
            sb.AppendLine("Write-Host '----------------------------------------'");
            sb.AppendLine("Write-Host 'Operation Completed Successfully.' -ForegroundColor Green");
            sb.AppendLine("Write-Host 'Some changes may require a system restart.' -ForegroundColor Yellow");
            
            return sb.ToString();
        }

        public async Task ExecuteScriptAsync(string script, Action<string> outputCallback)
        {
            await Task.Run(async () =>
            {
                string tempFilePath = null;
                try
                {
                    // Check if we are on Windows
                    bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

                    if (!isWindows)
                    {
                        // Simulation Mode for Non-Windows (e.g. Linux Sandbox)
                        outputCallback("Environment detected: Non-Windows. Running in Simulation Mode.\n");
                        outputCallback("------------------------------------------------------------\n");
                        
                        var lines = script.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        foreach (var line in lines)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            
                            // Emulate delay
                            if (line.Contains("Start-Sleep"))
                            {
                                await Task.Delay(500);
                                continue;
                            }

                            if (line.Contains("Write-Host"))
                            {
                                var msg = line.Replace("Write-Host", "").Replace("'", "").Replace("-ForegroundColor Green", "").Replace("-ForegroundColor Cyan", "").Replace("-ForegroundColor Yellow", "").Trim();
                                outputCallback($"> {msg}\n");
                                await Task.Delay(200); // Typing effect delay
                            }
                            else
                            {
                                outputCallback($"Executing: {line.Trim()}\n");
                                await Task.Delay(100);
                            }
                        }
                        return;
                    }

                    // Write script to a temporary file
                    tempFilePath = System.IO.Path.GetTempFileName() + ".ps1";
                    System.IO.File.WriteAllText(tempFilePath, script);

                    // Real Execution Logic
                    var startInfo = new ProcessStartInfo()
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{tempFilePath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = new Process { StartInfo = startInfo })
                    {
                        process.OutputDataReceived += (sender, e) => { if (e.Data != null) outputCallback(e.Data + "\n"); };
                        process.ErrorDataReceived += (sender, e) => { if (e.Data != null) outputCallback("ERROR: " + e.Data + "\n"); };

                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        await process.WaitForExitAsync();
                    }
                }
                catch (Exception ex)
                {
                    outputCallback($"FATAL ERROR: {ex.Message}\n");
                }
                finally
                {
                    // Clean up temp file
                    if (tempFilePath != null && System.IO.File.Exists(tempFilePath))
                    {
                        try { System.IO.File.Delete(tempFilePath); } catch { }
                    }
                }
            });
        }
    }
}
