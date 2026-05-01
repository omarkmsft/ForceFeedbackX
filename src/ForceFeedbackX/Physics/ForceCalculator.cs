using ForceFeedbackX.Profiles;
using Microsoft.Extensions.Logging;

namespace ForceFeedbackX.Physics;

/// <summary>
/// Converts a <see cref="FlightData"/> snapshot into a <see cref="ForceOutput"/>
/// that can be sent directly to the FFB engine.
/// </summary>
public sealed class ForceCalculator
{
    private const int MaxDiForce = 10000;
    private const double MaxIasKnots = 350.0;   // IAS at which spring hits maximum
    private const double StallBuffetFreqHz = 8.0;

    private readonly ILogger<ForceCalculator> _logger;

    public ForceCalculator(ILogger<ForceCalculator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Compute FFB output for the current sim frame.
    /// </summary>
    /// <param name="data">Latest flight data from SimConnect.</param>
    /// <param name="profile">Active aircraft profile.</param>
    /// <returns>Normalized force parameters ready for <see cref="FFB.FfbEngine"/>.</returns>
    public ForceOutput Calculate(FlightData data, AircraftProfile profile)
    {
        return profile.AircraftType switch
        {
            AircraftType.HydraulicCable => CalculateHydraulic(data, profile),
            AircraftType.FlyByWire      => CalculateFlyByWire(data, profile),
            _                           => ForceOutput.Zero,
        };
    }

    // ── Hydraulic / cable model ────────────────────────────────────────────────

    private ForceOutput CalculateHydraulic(FlightData data, AircraftProfile profile)
    {
        // Spring force scales with IAS²
        double iasRatio = Math.Clamp(data.AirspeedIndicated / MaxIasKnots, 0.0, 1.0);
        double springNorm = iasRatio * iasRatio * profile.ForceMultiplier;
        int spring = (int)(springNorm * MaxDiForce);

        // Trim offset shifts the spring center
        int trimY = (int)(-data.ElevatorTrimPosition * profile.TrimScale * MaxDiForce);
        int trimX = (int)(-data.AileronTrimPosition  * profile.TrimScale * MaxDiForce);
        trimY = Math.Clamp(trimY, -MaxDiForce, MaxDiForce);
        trimX = Math.Clamp(trimX, -MaxDiForce, MaxDiForce);

        // Damper
        int damper = (int)(profile.DamperGain * MaxDiForce);

        // Constant force from G loading (subtract 1g baseline)
        double gDelta = data.GForce - 1.0;
        int cfY = (int)Math.Clamp(-gDelta * profile.GForceScale * MaxDiForce,
                                   -MaxDiForce, MaxDiForce);

        // Stall buffet vibration
        (int vibMag, double vibFreq) = ComputeVibration(data);

        return new ForceOutput
        {
            SpringCenterY        = trimY,
            SpringCenterX        = trimX,
            SpringCoefficient    = spring,
            DamperCoefficient    = damper,
            ConstantForceY       = cfY,
            ConstantForceX       = 0,
            VibrationMagnitude   = vibMag,
            VibrationFrequencyHz = vibFreq,
        };
    }

    // ── Fly-by-wire model ──────────────────────────────────────────────────────

    private ForceOutput CalculateFlyByWire(FlightData data, AircraftProfile profile)
    {
        // FBW: very light centering spring only; no aero scaling
        int lightSpring = (int)(0.08 * profile.ForceMultiplier * MaxDiForce);

        // Stall shaker only — no constant force
        (int vibMag, double vibFreq) = ComputeVibration(data);

        return new ForceOutput
        {
            SpringCenterY        = 0,
            SpringCenterX        = 0,
            SpringCoefficient    = lightSpring,
            DamperCoefficient    = (int)(profile.DamperGain * 0.5 * MaxDiForce),
            ConstantForceY       = 0,
            ConstantForceX       = 0,
            VibrationMagnitude   = vibMag,
            VibrationFrequencyHz = vibFreq,
        };
    }

    // ── Shared helpers ─────────────────────────────────────────────────────────

    private static (int magnitude, double frequencyHz) ComputeVibration(FlightData data)
    {
        if (data.StallWarning > 0.5)
        {
            // Buffet scales up as you approach / enter stall
            return (4000, StallBuffetFreqHz);
        }

        if (data.SimOnGround < 0.5 && data.AirspeedIndicated > 5)
        {
            // Light airframe rumble proportional to IAS (turbulence placeholder)
            int rumble = (int)Math.Clamp(data.AirspeedIndicated * 2, 0, 500);
            return (rumble, 20.0);
        }

        return (0, 0.0);
    }
}
