using ForceFeedbackX.Physics;
using Microsoft.Extensions.Logging;
using Vortice.DirectInput;

namespace ForceFeedbackX.FFB;

/// <summary>
/// Manages the DirectInput force-feedback device (Microsoft Sidewinder FFB2).
/// Creates and maintains Spring, Damper, ConstantForce, and Periodic (vibration) effects.
/// </summary>
public sealed class FfbEngine : IDisposable
{
    private const int DiMax   =  10000;
    private const int DiMin   = -10000;

    private readonly ILogger<FfbEngine> _logger;

    private IDirectInput8?       _directInput;
    private IDirectInputDevice8? _device;
    private IDirectInputEffect?  _springEffect;
    private IDirectInputEffect?  _damperEffect;
    private IDirectInputEffect?  _constantForceEffect;
    private IDirectInputEffect?  _vibrationEffect;

    private bool _disposed;

    public string? DeviceName { get; private set; }
    public bool IsAcquired    => _device is not null;

    public FfbEngine(ILogger<FfbEngine> logger)
    {
        _logger = logger;
    }

    // ── Initialisation ─────────────────────────────────────────────────────────

    /// <summary>
    /// Enumerate FFB-capable devices and acquire the first one found.
    /// </summary>
    /// <param name="ownerHwnd">Handle of a window to satisfy cooperative-level requirements.</param>
    /// <returns>True if a device was found and acquired.</returns>
    public bool TryAcquire(IntPtr ownerHwnd)
    {
        try
        {
            _directInput = DInput.DirectInput8Create();

            DeviceInstance? ffbDevice = null;
            foreach (var di in _directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly | DeviceEnumerationFlags.ForceFeedback))
            {
                _logger.LogInformation("FFB device found: {Name}", di.InstanceName);
                ffbDevice ??= di;
            }

            if (ffbDevice is null)
            {
                _logger.LogWarning("No FFB device found.");
                return false;
            }

            _device = _directInput.CreateDevice(ffbDevice.InstanceGuid);
            _device.SetDataFormat<RawJoystickState>();
            _device.SetCooperativeLevel(ownerHwnd,
                CooperativeLevel.Background | CooperativeLevel.Exclusive);
            _device.Acquire();

            DeviceName = ffbDevice.InstanceName;
            _logger.LogInformation("FFB device acquired: {Name}", DeviceName);

            CreateEffects();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acquire FFB device.");
            return false;
        }
    }

    // ── Effect updates ─────────────────────────────────────────────────────────

    /// <summary>
    /// Apply a new <see cref="ForceOutput"/> to all active FFB effects.
    /// Call this each sim frame (~30–60 Hz).
    /// </summary>
    public void UpdateForces(ForceOutput fo)
    {
        if (_device is null) return;

        try
        {
            UpdateSpring(fo);
            UpdateDamper(fo);
            UpdateConstantForce(fo);
            UpdateVibration(fo);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error updating FFB forces.");
        }
    }

    // ── Private effect management ──────────────────────────────────────────────

    private void CreateEffects()
    {
        _springEffect       = CreateSpring();
        _damperEffect       = CreateDamper();
        _constantForceEffect = CreateConstantForce();
        _vibrationEffect    = CreateVibration();
    }

    private IDirectInputEffect? CreateSpring()
    {
        // TODO: Create GUID_Spring effect with Vortice.DirectInput
        // EffectParameters → Condition parameters for X and Y axes
        _logger.LogDebug("TODO: CreateSpring effect");
        return null;
    }

    private IDirectInputEffect? CreateDamper()
    {
        // TODO: Create GUID_Damper effect
        _logger.LogDebug("TODO: CreateDamper effect");
        return null;
    }

    private IDirectInputEffect? CreateConstantForce()
    {
        // TODO: Create GUID_ConstantForce effect
        _logger.LogDebug("TODO: CreateConstantForce effect");
        return null;
    }

    private IDirectInputEffect? CreateVibration()
    {
        // TODO: Create GUID_Sine periodic effect
        _logger.LogDebug("TODO: CreateVibration effect");
        return null;
    }

    private void UpdateSpring(ForceOutput fo)
    {
        if (_springEffect is null) return;
        // TODO: call _springEffect.SetParameters with new condition params
        //       Offset  = fo.SpringCenterX / fo.SpringCenterY
        //       Coefficient = fo.SpringCoefficient
    }

    private void UpdateDamper(ForceOutput fo)
    {
        if (_damperEffect is null) return;
        // TODO: update damper coefficient from fo.DamperCoefficient
    }

    private void UpdateConstantForce(ForceOutput fo)
    {
        if (_constantForceEffect is null) return;
        // TODO: update magnitude from fo.ConstantForceY (or X)
    }

    private void UpdateVibration(ForceOutput fo)
    {
        if (_vibrationEffect is null) return;
        // TODO: update magnitude and period from fo.VibrationMagnitude / fo.VibrationFrequencyHz
        //       If magnitude == 0, stop the effect; otherwise start/update
    }

    // ── Dispose ────────────────────────────────────────────────────────────────

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _springEffect?.Dispose();
        _damperEffect?.Dispose();
        _constantForceEffect?.Dispose();
        _vibrationEffect?.Dispose();

        try { _device?.Unacquire(); } catch { /* best-effort */ }
        _device?.Dispose();
        _directInput?.Dispose();

        _logger.LogInformation("FfbEngine disposed.");
    }
}
