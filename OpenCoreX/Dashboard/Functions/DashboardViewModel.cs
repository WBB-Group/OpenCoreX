using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Runtime.CompilerServices;
using System.Timers;

namespace OpenCoreX.Dashboard.Functions;

public class GpuItem
{
    public string Name { get; set; } = "Unknown GPU";
    public string Usage { get; set; } = "N/A";
    public string Memory { get; set; } = "N/A";
}

/// <summary>
/// ViewModel for the Dashboard providing live system data.
/// </summary>
public class DashboardViewModel : INotifyPropertyChanged, IDisposable
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly System.Timers.Timer _updateTimer;
    private readonly PerformanceCounter? _cpuCounter;
    private readonly PerformanceCounter? _ramCounter;
    private readonly DateTime _startTime;
    private bool _disposed;

    // GPU
    private int _gpuUsagePercent = 0;
    
    // CPU
    private int _cpuUsagePercent = 0;
    
    // Memory
    private int _memoryUsagePercent = 0;
    private string _memoryUsageDetails = "Calculating...";
    
    // Uptime
    private string _uptime = "00:00:00";

    // Platform Info
    private string _platformName = "Loading...";
    private string _kernelVersion = "Loading...";

    private string _secureBootStatus = "Checking...";
    private string _userName = "User";

    // Hardware Info
    private string _processorInfo = "Loading...";
    private string _processorCores = "Loading...";
    private string _processorSpeed = "Loading...";
    private string _graphicsCard = "Loading...";
    private string _graphicsDriver = "Loading...";

    public ObservableCollection<GpuItem> GpuList { get; } = new();

    // Status
    private string _coreIntegrityStatus = "Verified";
    private string _watchdogStatus = "Active";

    public DashboardViewModel()
    {
        _startTime = DateTime.Now;
        UserName = Environment.UserName;

        // Initialize performance counters
        try
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            
            // Initial read to initialize counters
            _cpuCounter.NextValue();
            _ramCounter.NextValue();
        }
        catch
        {
            // Performance counters may not be available
        }

        // Load static system info
        LoadStaticSystemInfo();

        // Set up timer for live updates (every 1 second)
        _updateTimer = new System.Timers.Timer(1000);
        _updateTimer.Elapsed += OnTimerElapsed;
        _updateTimer.AutoReset = true;
        _updateTimer.Start();

        // Initial update
        UpdateLiveData();
    }

    private void LoadStaticSystemInfo()
    {
        try
        {
            // Get OS Info
            using var osQuery = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (var os in osQuery.Get())
            {
                PlatformName = $"{os["Caption"]} (Build {os["BuildNumber"]})";
                KernelVersion = os["Version"]?.ToString() ?? "Unknown";
            }

            // Get Processor Info
            using var cpuQuery = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (var cpu in cpuQuery.Get())
            {
                ProcessorInfo = cpu["Name"]?.ToString() ?? "Unknown";
                ProcessorCores = $"{cpu["NumberOfCores"]} Cores / {cpu["NumberOfLogicalProcessors"]} Threads";
                ProcessorSpeed = $"{cpu["MaxClockSpeed"]} MHz base";
            }

            // Get Graphics Info
            GpuList.Clear();
            using var gpuQuery = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (var gpu in gpuQuery.Get())
            {
                var name = gpu["Name"]?.ToString() ?? "Unknown";
                GraphicsCard = name; // Last one wins for summary
                GraphicsDriver = $"Driver {gpu["DriverVersion"]}";

                // Try to get VRAM
                string vram = "Unknown";
                try
                {
                    var ramBytes = Convert.ToInt64(gpu["AdapterRAM"]);
                    vram = $"{ramBytes / (1024 * 1024)} MB";
                }
                catch { }

                GpuList.Add(new GpuItem
                {
                    Name = name,
                    Usage = "N/A", // Usage difficult via WMI
                    Memory = vram
                });
            }

            // Check Secure Boot
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State");
                if (key != null)
                {
                    var value = key.GetValue("UEFISecureBootEnabled");
                    SecureBootStatus = value?.ToString() == "1" ? "Enabled" : "Disabled";
                }
                else
                {
                    SecureBootStatus = "Unknown";
                }
            }
            catch
            {
                SecureBootStatus = "Unknown";
            }
        }
        catch
        {
            // Fallback values if WMI fails
            PlatformName = "Windows";
            KernelVersion = Environment.OSVersion.Version.ToString();
            ProcessorInfo = "Unknown Processor";
        }
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        UpdateLiveData();
    }

    private void UpdateLiveData()
    {
        try
        {
            // Update CPU
            if (_cpuCounter != null)
            {
                var cpuUsage = (int)_cpuCounter.NextValue();
                CpuUsagePercent = cpuUsage;
            }

            // Update Memory
            if (_ramCounter != null)
            {
                var availableMB = _ramCounter.NextValue();
                var totalMemoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
                var totalMemoryGB = totalMemoryBytes / (1024.0 * 1024 * 1024);
                
                if (totalMemoryGB > 0)
                {
                    var usedMemoryGB = totalMemoryGB - (availableMB / 1024.0);
                    MemoryUsagePercent = (int)((usedMemoryGB / totalMemoryGB) * 100);
                    MemoryUsageDetails = $"{usedMemoryGB:F1} GB / {totalMemoryGB:F1} GB";
                }
                else
                {
                    MemoryUsagePercent = 0;
                    MemoryUsageDetails = "Unknown";
                }
            }

            // Update Uptime
            var uptime = DateTime.Now - _startTime + TimeSpan.FromMilliseconds(Environment.TickCount64);
            uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            Uptime = $"{(int)uptime.TotalHours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
        }
        catch
        {
            // Ignore update errors
        }
    }

    #region Properties

    public int GpuUsagePercent
    {
        get => _gpuUsagePercent;
        set => SetProperty(ref _gpuUsagePercent, value);
    }

    public int CpuUsagePercent
    {
        get => _cpuUsagePercent;
        set => SetProperty(ref _cpuUsagePercent, value);
    }

    public int MemoryUsagePercent
    {
        get => _memoryUsagePercent;
        set => SetProperty(ref _memoryUsagePercent, value);
    }

    public string MemoryUsageDetails
    {
        get => _memoryUsageDetails;
        set => SetProperty(ref _memoryUsageDetails, value);
    }

    public string Uptime
    {
        get => _uptime;
        set => SetProperty(ref _uptime, value);
    }

    public string PlatformName
    {
        get => _platformName;
        set => SetProperty(ref _platformName, value);
    }

    public string KernelVersion
    {
        get => _kernelVersion;
        set => SetProperty(ref _kernelVersion, value);
    }

    public string SecureBootStatus
    {
        get => _secureBootStatus;
        set => SetProperty(ref _secureBootStatus, value);
    }

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public string ProcessorInfo
    {
        get => _processorInfo;
        set => SetProperty(ref _processorInfo, value);
    }

    public string ProcessorCores
    {
        get => _processorCores;
        set => SetProperty(ref _processorCores, value);
    }

    public string ProcessorSpeed
    {
        get => _processorSpeed;
        set => SetProperty(ref _processorSpeed, value);
    }

    public string GraphicsCard
    {
        get => _graphicsCard;
        set => SetProperty(ref _graphicsCard, value);
    }

    public string GraphicsDriver
    {
        get => _graphicsDriver;
        set => SetProperty(ref _graphicsDriver, value);
    }

    public string CoreIntegrityStatus
    {
        get => _coreIntegrityStatus;
        set => SetProperty(ref _coreIntegrityStatus, value);
    }

    public string WatchdogStatus
    {
        get => _watchdogStatus;
        set => SetProperty(ref _watchdogStatus, value);
    }

    #endregion

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _updateTimer.Stop();
        _updateTimer.Dispose();
        _cpuCounter?.Dispose();
        _ramCounter?.Dispose();
    }
}