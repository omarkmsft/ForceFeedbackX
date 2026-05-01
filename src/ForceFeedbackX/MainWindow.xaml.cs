using ForceFeedbackX.FFB;
using ForceFeedbackX.Physics;
using ForceFeedbackX.Profiles;
using ForceFeedbackX.SimConnect;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ForceFeedbackX;

public partial class MainWindow : Window
{
    private readonly SimConnectClient  _simConnect;
    private readonly FfbEngine         _ffbEngine;
    private readonly ForceCalculator   _forceCalc;
    private readonly ProfileManager    _profileManager;
    private readonly ILogger<MainWindow> _logger;

    private AircraftProfile _activeProfile;
    private string          _aircraftTitle = string.Empty;

    public MainWindow(
        SimConnectClient  simConnect,
        FfbEngine         ffbEngine,
        ForceCalculator   forceCalc,
        ProfileManager    profileManager,
        ILogger<MainWindow> logger)
    {
        _simConnect     = simConnect;
        _ffbEngine      = ffbEngine;
        _forceCalc      = forceCalc;
        _profileManager = profileManager;
        _logger         = logger;
        _activeProfile  = profileManager.Default;

        InitializeComponent();
    }

    // ── Window lifecycle ───────────────────────────────────────────────────────

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var hwnd = new WindowInteropHelper(this).Handle;

        // Hook WndProc for SimConnect message pump
        HwndSource.FromHwnd(hwnd)!.AddHook(WndProc);

        // Wire SimConnect events
        _simConnect.WindowHandle = hwnd;
        _simConnect.FlightDataReceived += OnFlightDataReceived;
        _simConnect.ConnectionChanged  += OnConnectionChanged;
        _simConnect.Connect();

        // Try to acquire FFB device
        if (_ffbEngine.TryAcquire(hwnd))
        {
            FfbStatusDot.Fill  = new SolidColorBrush(Color.FromRgb(0xA6, 0xE3, 0xA1)); // green
            FfbStatusText.Text = $"FFB: {_ffbEngine.DeviceName}";
        }
        else
        {
            FfbStatusText.Text = "FFB: No device found";
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _simConnect.Dispose();
        _ffbEngine.Dispose();
        base.OnClosed(e);
    }

    // ── SimConnect WndProc hook ────────────────────────────────────────────────

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == SimConnectConstants.WM_USER_SIMCONNECT)
        {
            _simConnect.ReceiveMessage();
            handled = true;
        }
        return IntPtr.Zero;
    }

    // ── SimConnect event handlers ──────────────────────────────────────────────

    private void OnConnectionChanged(object? sender, bool connected)
    {
        Dispatcher.InvokeAsync(() =>
        {
            if (connected)
            {
                MsfsStatusDot.Fill  = new SolidColorBrush(Color.FromRgb(0xA6, 0xE3, 0xA1));
                MsfsStatusText.Text = "MSFS: Connected";
            }
            else
            {
                MsfsStatusDot.Fill  = new SolidColorBrush(Color.FromRgb(0xF3, 0x8B, 0xA8));
                MsfsStatusText.Text = "MSFS: Disconnected";
                AircraftTitleText.Text = "— none loaded —";
                AircraftTypeText.Text  = "Type: —";
            }
        });
    }

    private void OnFlightDataReceived(object? sender, FlightData data)
    {
        // Compute forces on background thread, update UI on dispatcher
        var forces = _forceCalc.Calculate(data, _activeProfile);
        _ffbEngine.UpdateForces(forces);

        Dispatcher.InvokeAsync(() => UpdateTelemetryDisplay(data));
    }

    private void UpdateTelemetryDisplay(FlightData data)
    {
        AirspeedText.Text = $"{data.AirspeedIndicated:F0} kts";
        GForceText.Text   = $"{data.GForce:F2} G";

        if (data.StallWarning > 0.5)
        {
            StallText.Text       = "STALL";
            StallText.Foreground = new SolidColorBrush(Color.FromRgb(0xF3, 0x8B, 0xA8));
        }
        else
        {
            StallText.Text       = "OK";
            StallText.Foreground = new SolidColorBrush(Color.FromRgb(0xA6, 0xE3, 0xA1));
        }
    }

    // ── UI event handlers ──────────────────────────────────────────────────────

    private void ForceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ForceSliderValue is null) return;
        ForceSliderValue.Text      = $"{e.NewValue:F2}";
        _activeProfile.ForceMultiplier = e.NewValue;
    }

    private void DamperSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (DamperSliderValue is null) return;
        DamperSliderValue.Text  = $"{e.NewValue:F2}";
        _activeProfile.DamperGain = e.NewValue;
    }

    private void ProfilesButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Profile editor coming in Phase 2!", "ForceFeedbackX",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        _ffbEngine.UpdateForces(ForceOutput.Zero);
        _logger.LogInformation("Forces reset to zero.");
    }
}
