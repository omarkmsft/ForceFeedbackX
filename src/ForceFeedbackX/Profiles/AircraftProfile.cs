using ForceFeedbackX.Physics;

namespace ForceFeedbackX.Profiles;

/// <summary>
/// Configuration for a specific aircraft or aircraft family.
/// Serialized to/from profiles.json.
/// </summary>
public sealed class AircraftProfile
{
    /// <summary>Display name for this profile.</summary>
    public string Name { get; set; } = "Default";

    /// <summary>
    /// Substrings to match against the TITLE SimVar.
    /// If any match (case-insensitive), this profile is selected.
    /// Empty list = default/fallback profile.
    /// </summary>
    public List<string> TitlePatterns { get; set; } = new();

    /// <summary>Control system type.</summary>
    public AircraftType AircraftType { get; set; } = AircraftType.HydraulicCable;

    /// <summary>Overall force multiplier (0.0–1.0).</summary>
    public double ForceMultiplier { get; set; } = 0.8;

    /// <summary>Damper effect gain (0.0–1.0).</summary>
    public double DamperGain { get; set; } = 0.3;

    /// <summary>How much trim position shifts the spring center (0.0–1.0).</summary>
    public double TrimScale { get; set; } = 0.5;

    /// <summary>How strongly G-force contributes to constant force (0.0–1.0).</summary>
    public double GForceScale { get; set; } = 0.4;
}
