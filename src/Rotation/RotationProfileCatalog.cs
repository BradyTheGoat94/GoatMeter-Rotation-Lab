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
                new RotationRule("Heart Gore",700,new[]
                {
                    new RotationCondition(RotationConditionKind.SignalPresent,"CriticalHitWindow",Reason:"passively observed critical hit can enable Heart Gore")
                }),
                new RotationRule("Insignia Explosion",600,new[]
                {
                    new RotationCondition(RotationConditionKind.SignalPresent,"InsigniaReady",Reason:"passively observed Insignia state supports explosion")
                }),
                new RotationRule("Shadowstrike",500,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"AssassinBurstWindow",Reason:"use Shadowstrike inside a proven burst/rear-access window")}),
                new RotationRule("Savage Roar",400,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"AssassinFillerWindow",Reason:"build Insignias while core spenders are unavailable")}),
                new RotationRule("Quick Slice",300,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"AssassinFillerWindow",Reason:"weave Quick Slice while core spenders are unavailable")})
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
                new RotationRule("Marking Shot",700,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"RangerMarkWindow",Reason:"maintain Precision and Deadshot support when the mark needs refresh")}),
                new RotationRule("Deadshot",650,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"RangerDeadshotWindow",Reason:"use charged Deadshot in a proven damage window")}),
                new RotationRule("Snipe",600,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"RangerFillerWindow",Reason:"cycle Snipe while higher-priority state actions are unavailable")}),
                new RotationRule("Rupture Arrow",500,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"RangerRuptureWindow",Reason:"passively observed Ranger state supports Rupture Arrow")}),
                new RotationRule("Destruction Trap",300,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"RangerTrapWindow",Reason:"passively observed Ranger state supports Destruction Trap")})
            },
            "PROVISIONAL Global Season 1 single-target fixture. Rupture Arrow and Destruction Trap remain gated on passive state signals; no recommendation is emitted until those states are proven.");
    }

    public static RotationProfile CreateProvisionalSorcererSingleTarget()
    {
        return new RotationProfile(
            AionClass.Sorcerer,
            "global-sorcerer-provisional",
            RotationMode.SingleTarget,
            ProfileValidation.Provisional,
            new[]
            {
                new RotationRule("Element Enhancement",800,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SorcererOpenerWindow",Reason:"establish the primary damage enhancement before the burst sequence")}),
                new RotationRule("Delayed Explosion",700,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SorcererOpenerWindow",Reason:"apply delayed burst early in the damage sequence")}),
                new RotationRule("Hellfire",650,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SorcererBurstWindow",Reason:"use Hellfire during a proven burst window")}),
                new RotationRule("Fire Wall",600,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SorcererBurstWindow",Reason:"maintain high-value fire damage during burst")}),
                new RotationRule("Cold Storm",550,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SorcererBurstWindow",Reason:"use Cold Storm in the sustained burst sequence")}),
                new RotationRule("Flame Harpoon",500,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SorcererBurstWindow",Reason:"passively observed Sorcerer burst state supports Flame Harpoon")}),
                new RotationRule("Flame Cage",400,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SorcererDotWindow",Reason:"refresh damage-over-time state when proven necessary")}),
                new RotationRule("Flame Arrow",200,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SorcererFillerWindow",Reason:"basic damage/MP filler while higher priorities are unavailable")})
            },
            "PROVISIONAL Global Season 1 single-target fixture. Burst and damage-over-time decisions are represented only behind passive state signals; no recommendation is emitted until those states are proven.");
    }

    public static RotationProfile CreateProvisionalSpiritmasterSingleTarget()
    {
        return new RotationProfile(
            AionClass.Spiritmaster,
            "global-spiritmaster-provisional",
            RotationMode.SingleTarget,
            ProfileValidation.Provisional,
            new[]
            {
                new RotationRule("Flame Blessing",850,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterOpenerWindow",Reason:"buff before Ancient Spirit so the summon snapshots the damage state")}),
                new RotationRule("Spirit's Benediction",800,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterOpenerWindow",Reason:"apply spirit damage buff before Ancient Spirit")}),
                new RotationRule("Summon: Ancient Spirit",750,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterAncientWindow",Reason:"summon Ancient Spirit after opener buffs")}),
                new RotationRule("Jointstrike: Corrode",700,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterCorrodeWindow",Reason:"maintain the high-value Corrode damage-over-time effect")}),
                new RotationRule("Elemental Fusion",650,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterBurstWindow",Reason:"fire Elemental Fusion when its proc/state is available")}),
                new RotationRule("Jointstrike: Destructive Attack",550,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterFillerWindow",Reason:"use on cooldown without sacrificing higher-priority Fusion or stack state")}),
                new RotationRule("Earth Tremor",450,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterFillerWindow",Reason:"maintain basic-attack buff stacks during sustained damage")}),
                new RotationRule("Disenchant",400,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterDispelWindow",Reason:"passively observed target state supports Disenchant")})
            },
            "PROVISIONAL Global Season 1 single-target fixture. Dispel and spirit-burst decisions remain gated on passive signals; no recommendation is emitted until the relevant state is proven.");
    }

    public static RotationProfile CreateProvisionalClericSingleTarget()
    {
        return new RotationProfile(
            AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,ProfileValidation.Provisional,
            new[]
            {
                new RotationRule("Earth Punishment",800,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ClericDamageWindow",Reason:"establish Earth Punishment before the Condemnation damage loop")}),
                new RotationRule("Condemnation",750,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ClericCondemnationWindow",Reason:"consume/reset Condemnation opportunity for primary single-target damage")}),
                new RotationRule("Chain of Torment",650,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ClericMarkWindow",Reason:"maintain the mark required by the Condemnation loop")}),
                new RotationRule("Divine Aura",550,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ClericDamageWindow",Reason:"use Divine Aura when its damage window is available")}),
                new RotationRule("Judgment Thunder",300,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ClericFillerWindow",Reason:"damage filler while higher priorities are unavailable")}),
                new RotationRule("Earth's Retribution",200,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ClericFillerWindow",Reason:"weave the MP-restoring basic damage action")}),
                new RotationRule("Healing Light",100,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ClericHealWindow",Reason:"interrupt damage priority only when healing state requires it")})
            },
            "PROVISIONAL Global Season 1 fixture. Damage and healing decisions remain gated on passive signals; no recommendation is emitted until relevant state is proven.");
    }

    public static RotationProfile CreateProvisionalChanterSingleTarget()
    {
        return new RotationProfile(
            AionClass.Chanter,"global-chanter-provisional",RotationMode.SingleTarget,ProfileValidation.Provisional,
            new[]
            {
                new RotationRule("Mountain Crash",500,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ChanterDamageWindow",Reason:"passively observed Chanter state supports Mountain Crash")}),
                new RotationRule("Healing Burst",300,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ChanterHealWindow",Reason:"passively observed healing state supports Healing Burst")})
            },
            "PROVISIONAL Global Season 1 fixture. Damage and healing decisions remain gated on passive signals; no recommendation is emitted until relevant state is proven.");
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
