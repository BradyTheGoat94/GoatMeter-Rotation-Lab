namespace Aion2DPSPro.Rotation;

public static class RotationProfileCatalog
{
    /// <summary>
    /// All eight launch classes are registered immediately, but intentionally have no
    /// rules until class-specific data has been researched and validated. This prevents
    /// guessed rotations from becoming actionable recommendations.
    /// </summary>
    public static RotationProfile CreateProvisionalTemplarSingleTarget()
    {
        return new RotationProfile(
            AionClass.Templar,
            "global-templar-provisional",
            RotationMode.SingleTarget,
            ProfileValidation.Provisional,
            new[]
            {
                new RotationRule("Punishment",300,new[]
                {
                    new RotationCondition(RotationConditionKind.CooldownReady,"Punishment",Reason:"validated 30s base cooldown is ready")
                }),
                new RotationRule("Empyrean Lord's Punishment",200,new[]
                {
                    new RotationCondition(RotationConditionKind.CooldownReady,"Empyrean Lord's Punishment",Reason:"validated 60s base cooldown is ready")
                })
            },
            "PROVISIONAL Global Season 1 single-target fixture. Current guides prioritize Punishment and Judgment; only evidence-gated cooldown-ready skills are emitted here. Judgment triggers/build state are not yet passively proven, so Judgment is deliberately omitted.");
    }

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
