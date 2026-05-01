namespace ForceFeedbackX.Physics;

/// <summary>
/// FFB parameters to apply to the joystick this frame.
/// All force values are normalized –10000 to +10000 (DirectInput units).
/// </summary>
public record ForceOutput
{
    /// <summary>Spring center offset on pitch axis (Y). Positive = pull back.</summary>
    public int SpringCenterY { get; init; }

    /// <summary>Spring center offset on roll axis (X). Positive = right.</summary>
    public int SpringCenterX { get; init; }

    /// <summary>Spring coefficient (0–10000). Scales how hard the spring resists deflection.</summary>
    public int SpringCoefficient { get; init; }

    /// <summary>Damper coefficient (0–10000). Reduces oscillation.</summary>
    public int DamperCoefficient { get; init; }

    /// <summary>Constant force on pitch axis. Used for G-force loading.</summary>
    public int ConstantForceY { get; init; }

    /// <summary>Constant force on roll axis. Used for sideslip/bank loading.</summary>
    public int ConstantForceX { get; init; }

    /// <summary>Vibration magnitude (0–10000). Used for stall buffet and engine vibration.</summary>
    public int VibrationMagnitude { get; init; }

    /// <summary>Vibration frequency in Hz.</summary>
    public double VibrationFrequencyHz { get; init; }

    public static readonly ForceOutput Zero = new();
}
