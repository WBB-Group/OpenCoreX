using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace OpenCoreX.Dashboard.Functions
{
    public class IntegrityService
    {
        public event EventHandler<string>? StatusUpdated;
        public event EventHandler<double>? ProgressUpdated;

        public async Task StartRepairAsync()
        {
            await RunCommandAsync("System File Checker", "sfc /scannow", 0, 50);
            await RunCommandAsync("DISM RestoreHealth", "DISM /Online /Cleanup-Image /RestoreHealth", 50, 50);
        }

        private async Task RunCommandAsync(string name, string command, double startProgress, double progressRange)
        {
            StatusUpdated?.Invoke(this, $"Starting {name}...");

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Using Task.Run to simulate progress since reading parsing stdout accurately for percentage is flaky across locales
            // But we will read output to log/update status
            
            using (var process = new Process { StartInfo = psi })
            {
                process.OutputDataReceived += (s, e) => 
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        // In a real scenario we might parse "20% complete"
                        StatusUpdated?.Invoke(this, $"{name}: {e.Data}");
                    }
                };
                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                         StatusUpdated?.Invoke(this, $"{name} Error: {e.Data}");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Artificial progress simulation while the real process runs
                // Because sfc/dism take a variable amount of time
                var progressTask = Task.Run(async () =>
                {
                    double current = 0;
                    while (!process.HasExited)
                    {
                        await Task.Delay(500);
                        if (current < 0.9)
                        {
                            current += 0.01; // slow increment
                            ProgressUpdated?.Invoke(this, startProgress + (current * progressRange));
                        }
                    }
                });

                await process.WaitForExitAsync();
                await progressTask;
                ProgressUpdated?.Invoke(this, startProgress + progressRange);
            }
        }
    }
}
