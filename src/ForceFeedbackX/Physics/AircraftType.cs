namespace ForceFeedbackX.Physics;

/// <summary>
/// Describes the flight control system type of the loaded aircraft.
/// Used to select the appropriate force-feedback model.
/// </summary>
public enum AircraftType
{
    /// <summary>Type has not been determined yet (no aircraft loaded).</summary>
    Unknown,

    /// <summary>
    /// Traditional hydraulic or cable-actuated controls.
    /// Stick forces scale with IAS² and control surface deflection.
    /// Examples: C172, A2A PA-28, PMDG 737.
    /// </summary>
    HydraulicCable,

    /// <summary>
    /// Fly-by-wire with active sidestick/column law.
    /// Forces are light centering only; no aerodynamic spring scaling.
    /// Examples: FBW A320, Fenix A320, PDMG 777.
    /// </summary>
    FlyByWire,
}
