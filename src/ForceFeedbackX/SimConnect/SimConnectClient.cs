using ForceFeedbackX.Physics;
using Microsoft.Extensions.Logging;

namespace ForceFeedbackX.SimConnect;

/// <summary>
/// Connects to Microsoft Flight Simulator 2024 via SimConnect,
/// subscribes to flight data variables, and raises <see cref="FlightDataReceived"/>
/// each sim frame.
/// </summary>
/// <remarks>
/// Requires SimConnect.dll and Microsoft.FlightSimulator.SimConnect.dll
/// in the application directory (see lib/README.md).
/// </remarks>
public sealed class SimConnectClient : IDisposable
{
    // ── SimConnect identifiers ─────────────────────────────────────────────────

    private enum DataDefinitionId { FlightData }
    private enum DataRequestId    { FlightData }
    private enum EventId          { SimStart, SimStop, AircraftLoaded }

    // ── State ──────────────────────────────────────────────────────────────────

    private readonly ILogger<SimConnectClient> _logger;
    private Microsoft.FlightSimulator.SimConnect.SimConnect? _simConnect;
    private System.Windows.Threading.DispatcherTimer? _retryTimer;
    private bool _disposed;

    /// <summary>Window handle used for SimConnect message pump.</summary>
    public IntPtr WindowHandle { get; set; }

    /// <summary>Fired each sim frame with fresh flight telemetry.</summary>
    public event EventHandler<FlightData>? FlightDataReceived;

    /// <summary>Fired when the connection to MSFS opens or closes.</summary>
    public event EventHandler<bool>? ConnectionChanged;

    /// <summary>Title of the currently loaded aircraft (or empty string).</summary>
    public string AircraftTitle { get; private set; } = string.Empty;

    public bool IsConnected => _simConnect is not null;

    public SimConnectClient(ILogger<SimConnectClient> logger)
    {
        _logger = logger;
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Attempt to open a SimConnect session.
    /// Call this from the WPF window after it has a valid HWND.
    /// </summary>
    public void Connect()
    {
        if (IsConnected) return;

        try
        {
            _simConnect = new Microsoft.FlightSimulator.SimConnect.SimConnect(
                "ForceFeedbackX", WindowHandle,
                SimConnectConstants.WM_USER_SIMCONNECT, null, 0);

            RegisterDataDefinition();
            RegisterEvents();

            _simConnect.RequestDataOnSimObject(
                DataRequestId.FlightData,
                DataDefinitionId.FlightData,
                Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_OBJECT_ID_USER,
                Microsoft.FlightSimulator.SimConnect.SIMCONNECT_PERIOD.SIM_FRAME,
                Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATA_REQUEST_FLAG.CHANGED,
                0, 0, 0);

            _logger.LogInformation("SimConnect session opened.");
            ConnectionChanged?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("SimConnect connect failed: {Message}", ex.Message);
            _simConnect = null;
            ScheduleRetry();
        }
    }

    /// <summary>
    /// Must be called from the WPF window's WndProc when
    /// <c>msg == SimConnectConstants.WM_USER_SIMCONNECT</c>.
    /// </summary>
    public void ReceiveMessage()
    {
        try
        {
            _simConnect?.ReceiveMessage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SimConnect ReceiveMessage error — disconnecting.");
            Disconnect();
            ScheduleRetry();
        }
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private void RegisterDataDefinition()
    {
        var sc = _simConnect!;

        // Each AddToDataDefinition must match the field order in FlightData (sequential pack)
        sc.AddToDataDefinition(DataDefinitionId.FlightData, "AIRSPEED INDICATED",   "knots",        Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.FLOAT64, 0, Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
        sc.AddToDataDefinition(DataDefinitionId.FlightData, "G FORCE",              "GForce",       Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.FLOAT64, 0, Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
        sc.AddToDataDefinition(DataDefinitionId.FlightData, "ROTATION VELOCITY BODY X", "degrees per second", Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.FLOAT64, 0, Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
        sc.AddToDataDefinition(DataDefinitionId.FlightData, "ROTATION VELOCITY BODY Y", "degrees per second", Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.FLOAT64, 0, Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
        sc.AddToDataDefinition(DataDefinitionId.FlightData, "ELEVATOR POSITION",    "position",     Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.FLOAT64, 0, Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
        sc.AddToDataDefinition(DataDefinitionId.FlightData, "AILERON POSITION",     "position",     Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.FLOAT64, 0, Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
        sc.AddToDataDefinition(DataDefinitionId.FlightData, "ELEVATOR TRIM POSITION","radians",     Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.FLOAT64, 0, Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
        sc.AddToDataDefinition(DataDefinitionId.FlightData, "AILERON TRIM POSITION", "radians",     Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.FLOAT64, 0, Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
        sc.AddToDataDefinition(DataDefinitionId.FlightData, "STALL WARNING",        "bool",         Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.FLOAT64, 0, Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
        sc.AddToDataDefinition(DataDefinitionId.FlightData, "OVERSPEED WARNING",    "bool",         Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.FLOAT64, 0, Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
        sc.AddToDataDefinition(DataDefinitionId.FlightData, "SIM ON GROUND",        "bool",         Microsoft.FlightSimulator.SimConnect.SIMCONNECT_DATATYPE.FLOAT64, 0, Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);

        sc.RegisterDataDefineStruct<FlightData>(DataDefinitionId.FlightData);
    }

    private void RegisterEvents()
    {
        var sc = _simConnect!;

        sc.OnRecvSimobjectDataBytype += OnRecvSimobjectDataBytype;
        sc.OnRecvSimobjectData       += OnRecvSimobjectData;
        sc.OnRecvQuit                += OnRecvQuit;
        sc.OnRecvException           += OnRecvException;
        sc.OnRecvOpen                += OnRecvOpen;
    }

    private void OnRecvOpen(Microsoft.FlightSimulator.SimConnect.SimConnect sender,
        Microsoft.FlightSimulator.SimConnect.SIMCONNECT_RECV_OPEN data)
    {
        _logger.LogInformation("MSFS SimConnect opened: {App} v{Major}.{Minor}",
            data.szApplicationName, data.dwApplicationVersionMajor, data.dwApplicationVersionMinor);
    }

    private void OnRecvSimobjectData(Microsoft.FlightSimulator.SimConnect.SimConnect sender,
        Microsoft.FlightSimulator.SimConnect.SIMCONNECT_RECV_SIMOBJECT_DATA data)
    {
        if ((DataRequestId)data.dwRequestID != DataRequestId.FlightData) return;

        var fd = (FlightData)data.dwData[0];
        FlightDataReceived?.Invoke(this, fd);
    }

    private void OnRecvSimobjectDataBytype(Microsoft.FlightSimulator.SimConnect.SimConnect sender,
        Microsoft.FlightSimulator.SimConnect.SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data) { }

    private void OnRecvQuit(Microsoft.FlightSimulator.SimConnect.SimConnect sender,
        Microsoft.FlightSimulator.SimConnect.SIMCONNECT_RECV data)
    {
        _logger.LogInformation("MSFS quit event received.");
        Disconnect();
        ScheduleRetry();
    }

    private void OnRecvException(Microsoft.FlightSimulator.SimConnect.SimConnect sender,
        Microsoft.FlightSimulator.SimConnect.SIMCONNECT_RECV_EXCEPTION data)
    {
        _logger.LogWarning("SimConnect exception: {Exception}", data.dwException);
    }

    private void Disconnect()
    {
        if (_simConnect is null) return;
        try { _simConnect.Dispose(); } catch { /* best-effort */ }
        _simConnect = null;
        ConnectionChanged?.Invoke(this, false);
        _logger.LogInformation("SimConnect disconnected.");
    }

    private void ScheduleRetry()
    {
        if (_retryTimer is not null) return;

        _retryTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _retryTimer.Tick += (_, _) =>
        {
            _retryTimer.Stop();
            _retryTimer = null;
            Connect();
        };
        _retryTimer.Start();
        _logger.LogInformation("Will retry SimConnect in 5 s.");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Disconnect();
    }
}

/// <summary>Windows message constant for SimConnect pump.</summary>
public static class SimConnectConstants
{
    public const int WM_USER_SIMCONNECT = 0x0402;
}
