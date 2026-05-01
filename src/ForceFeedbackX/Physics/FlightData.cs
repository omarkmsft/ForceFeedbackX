using System.Runtime.InteropServices;

namespace ForceFeedbackX.Physics;

/// <summary>
/// Snapshot of all SimConnect variables received from MSFS 2024 each sim frame.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct FlightData
{
    /// <summary>Indicated airspeed in knots.</summary>
    public double AirspeedIndicated;

    /// <summary>G-force (1.0 = normal gravity).</summary>
    public double GForce;

    /// <summary>Pitch rate in degrees per second (body axis X).</summary>
    public double PitchRateDegPerSec;

    /// <summary>Roll rate in degrees per second (body axis Y).</summary>
    public double RollRateDegPerSec;

    /// <summary>Elevator position, -1.0 (full down) to 1.0 (full up).</summary>
    public double ElevatorPosition;

    /// <summary>Aileron position, -1.0 (full left) to 1.0 (full right).</summary>
    public double AileronPosition;

    /// <summary>Elevator trim position in radians.</summary>
    public double ElevatorTrimPosition;

    /// <summary>Aileron trim position in radians.</summary>
    public double AileronTrimPosition;

    /// <summary>True when the sim is generating a stall warning.</summary>
    public double StallWarning;   // SimConnect returns bools as double

    /// <summary>True when the sim is generating an overspeed warning.</summary>
    public double OverspeedWarning;

    /// <summary>True when the aircraft is on the ground.</summary>
    public double SimOnGround;
}
