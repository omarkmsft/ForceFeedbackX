using System;
using Vortice.DirectInput;

namespace ForceFeedbackX.FFB;

/// <summary>
/// Thin wrapper around a single DirectInput force-feedback effect:
/// owns the parameters block and the live <see cref="IDirectInputEffect"/> handle.
/// </summary>
public sealed class FfbEffect : IDisposable
{
    /// <summary>Friendly identifier (Spring, Damper, ConstantForce, Sine, ...).</summary>
    public string Name { get; }

    /// <summary>The DirectInput effect-type GUID (e.g. <see cref="EffectGuid.Spring"/>).</summary>
    public Guid EffectGuid { get; }

    /// <summary>Parameter block — mutated then reapplied via <see cref="Update"/>.</summary>
    public EffectParameters Parameters { get; }

    /// <summary>The live device-side effect, or null until <see cref="Create"/> succeeds.</summary>
    public IDirectInputEffect? Effect { get; private set; }

    public FfbEffect(string name, Guid effectGuid, EffectParameters parameters)
    {
        Name = name;
        EffectGuid = effectGuid;
        Parameters = parameters;
    }

    /// <summary>Bind this effect to a device. Safe to call once per session.</summary>
    public void Create(IDirectInputDevice8 device)
    {
        if (Effect is not null) return;
        Effect = device.CreateEffect(EffectGuid, Parameters);
    }

    /// <summary>Push the (possibly-mutated) parameter block to the device.</summary>
    public void Update(EffectParameterFlags flags = EffectParameterFlags.TypeSpecificParameters
                                                   | EffectParameterFlags.Direction
                                                   | EffectParameterFlags.Gain)
    {
        Effect?.SetParameters(Parameters, flags);
    }

    /// <summary>Start playing the effect (looping = int.MaxValue iterations).</summary>
    public void Start(int iterations = int.MaxValue)
    {
        Effect?.Start(iterations);
    }

    public void Stop() => Effect?.Stop();

    public void Dispose()
    {
        try { Effect?.Stop(); } catch { }
        Effect?.Dispose();
        Effect = null;
    }
}
