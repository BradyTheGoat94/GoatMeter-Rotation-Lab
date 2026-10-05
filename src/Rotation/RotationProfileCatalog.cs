namespace Aion2DPSPro.Rotation;

public static class RotationProfileCatalog
{
    /// <summary>
    /// All eight launch classes are registered immediately, but intentionally have no
    /// rules until class-specific data has been researched and validated. This prevents
    /// guessed rotations from becoming actionable recommendations.
    /// </summary>
    public static IReadOnlyList<RotationProfile> CreateUnvalidatedGlobalStubs()
    {
        return Enum.GetValues<AionClass>()
            .Select(className => new RotationProfile(
                className,
                "global-unvalidated",
                RotationMode.SingleTarget,
                ProfileValidation.Unvalidated,
                Array.Empty<RotationRule>(),
                "Placeholder only; rotation data not yet validated."))
            .ToArray();
    }
}
