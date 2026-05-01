using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ForceFeedbackX.Profiles;

/// <summary>
/// Loads, matches, and saves aircraft force-feedback profiles.
/// Profiles are stored in %APPDATA%\ForceFeedbackX\profiles.json.
/// </summary>
public sealed class ProfileManager
{
    private static readonly string ProfilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ForceFeedbackX", "profiles.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    private readonly ILogger<ProfileManager> _logger;
    private List<AircraftProfile> _profiles = new();

    public AircraftProfile Default { get; } = new AircraftProfile { Name = "Default" };

    public ProfileManager(ILogger<ProfileManager> logger)
    {
        _logger = logger;
        Load();
    }

    /// <summary>
    /// Match the loaded aircraft title against known profiles.
    /// Falls back to the Default profile when nothing matches.
    /// </summary>
    public AircraftProfile Match(string aircraftTitle)
    {
        if (string.IsNullOrWhiteSpace(aircraftTitle))
            return Default;

        foreach (var profile in _profiles)
        {
            foreach (var pattern in profile.TitlePatterns)
            {
                if (aircraftTitle.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Profile matched: {Profile} for aircraft: {Title}",
                        profile.Name, aircraftTitle);
                    return profile;
                }
            }
        }

        _logger.LogInformation("No profile matched for '{Title}', using Default.", aircraftTitle);
        return Default;
    }

    /// <summary>Load profiles from disk. Creates default file if missing.</summary>
    public void Load()
    {
        try
        {
            if (!File.Exists(ProfilePath))
            {
                CreateDefaultFile();
                return;
            }

            var json = File.ReadAllText(ProfilePath);
            _profiles = JsonSerializer.Deserialize<List<AircraftProfile>>(json, JsonOptions)
                        ?? new List<AircraftProfile>();
            _logger.LogInformation("Loaded {Count} profiles from {Path}", _profiles.Count, ProfilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load profiles, using empty list.");
            _profiles = new List<AircraftProfile>();
        }
    }

    /// <summary>Save all profiles to disk.</summary>
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ProfilePath)!);
            var json = JsonSerializer.Serialize(_profiles, JsonOptions);
            File.WriteAllText(ProfilePath, json);
            _logger.LogInformation("Profiles saved to {Path}", ProfilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save profiles.");
        }
    }

    private void CreateDefaultFile()
    {
        _profiles = new List<AircraftProfile>
        {
            new()
            {
                Name           = "Airbus A320 (FBW)",
                AircraftType   = Physics.AircraftType.FlyByWire,
                TitlePatterns  = new List<string> { "A320", "A319", "A321", "Airbus" },
                ForceMultiplier = 0.5,
                DamperGain     = 0.2,
                TrimScale      = 0.0,
                GForceScale    = 0.0,
            },
            new()
            {
                Name           = "Boeing 737",
                AircraftType   = Physics.AircraftType.HydraulicCable,
                TitlePatterns  = new List<string> { "737", "B737" },
                ForceMultiplier = 0.75,
                DamperGain     = 0.35,
                TrimScale      = 0.5,
                GForceScale    = 0.3,
            },
            new()
            {
                Name           = "General Aviation (Default)",
                AircraftType   = Physics.AircraftType.HydraulicCable,
                TitlePatterns  = new List<string>(),
                ForceMultiplier = 0.8,
                DamperGain     = 0.3,
                TrimScale      = 0.5,
                GForceScale    = 0.4,
            },
        };

        Save();
    }
}
