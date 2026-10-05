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
                new RotationRule("Judgment",500,new[]
                {
                    new RotationCondition(RotationConditionKind.SignalPresent,"JudgmentWindow",Reason:"observed shield skill opened Judgment window")
                }),
                new RotationRule("Punishment",300,new[]
                {
                    new RotationCondition(RotationConditionKind.CooldownReady,"Punishment",Reason:"validated 30s base cooldown is ready")
                }),
                new RotationRule("Empyrean Lord's Punishment",200,new[]
                {
                    new RotationCondition(RotationConditionKind.CooldownReady,"Empyrean Lord's Punishment",Reason:"validated 60s base cooldown is ready")
                })
            },
            "PROVISIONAL Global Season 1 single-target fixture. Current guides prioritize Punishment and Judgment; only evidence-gated cooldown-ready skills are emitted here. Judgment is emitted only inside an evidence-backed passively observed shield-skill window; specialization/build modifiers remain unproven.");
    }

    public static RotationProfile CreateProvisionalAssassinSingleTarget()
    {
        return new RotationProfile(
            AionClass.Assassin,
            "global-assassin-provisional",
            RotationMode.SingleTarget,
            ProfileValidation.Provisional,
            new[]
            {
                new RotationRule("Heart Gore",500,new[]
                {
                    new RotationCondition(RotationConditionKind.SignalPresent,"CriticalHitWindow",Reason:"passively observed critical hit can enable Heart Gore")
                }),
                new RotationRule("Insignia Explosion",400,new[]
                {
                    new RotationCondition(RotationConditionKind.SignalPresent,"InsigniaReady",Reason:"passively observed Insignia state supports explosion")
                })
            },
            "PROVISIONAL Global Season 1 single-target fixture. Heart Gore and Insignia Explosion are gated on passive signals; until those signals can be proven by the live decoder, the profile intentionally emits no recommendation.");
    }

    public static RotationProfile CreateProvisionalGladiatorSingleTarget()
    {
        return new RotationProfile(
            AionClass.Gladiator,
            "global-gladiator-provisional",
            RotationMode.SingleTarget,
            ProfileValidation.Provisional,
            new[]
            {
                new RotationRule("Seismic Crash",400,new[]
                {
                    new RotationCondition(RotationConditionKind.SignalPresent,"GladiatorFinisherWindow",Reason:"passively observed chain state supports Seismic Crash")
                }),
                new RotationRule("Rupture",300,new[]
                {
                    new RotationCondition(RotationConditionKind.SignalPresent,"GladiatorRuptureWindow",Reason:"passively observed chain state supports Rupture")
                })
            },
            "PROVISIONAL Global Season 1 single-target fixture. High-value finishers are represented only behind passive chain signals; until the live decoder proves those signals, the profile intentionally emits no recommendation.");
    }

    public static RotationProfile CreateProvisionalRangerSingleTarget()
    {
        return new RotationProfile(
            AionClass.Ranger,
            "global-ranger-provisional",
            RotationMode.SingleTarget,
            ProfileValidation.Provisional,
            new[]
            {
                new RotationRule("Rupture Arrow",500,new[]
                {
                    new RotationCondition(RotationConditionKind.SignalPresent,"RangerRuptureWindow",Reason:"passively observed Ranger state supports Rupture Arrow")
                }),
                new RotationRule("Destruction Trap",300,new[]
                {
                    new RotationCondition(RotationConditionKind.SignalPresent,"RangerTrapWindow",Reason:"passively observed Ranger state supports Destruction Trap")
                })
            },
            "PROVISIONAL Global Season 1 single-target fixture. Rupture Arrow and Destruction Trap remain gated on passive state signals; no recommendation is emitted until those states are proven.");
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
