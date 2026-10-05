namespace Aion2DPSPro.Rotation;

/// <summary>
/// Derives recommendation signals only from facts already observed passively.
/// Generic combat activity may unlock sustained filler; exact sequence windows
/// require the observed local class and prerequisite skill.
/// </summary>
public static class PassiveRotationSignalDeriver
{
    public static HashSet<string> Derive(PassiveRotationObservation observed,AionClass observedClass,DateTime now)
    {
        var signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if(observed.JudgmentWindowActive(now))signals.Add("JudgmentWindow");
        if(observed.CriticalHitWindowActive(now))signals.Add("CriticalHitWindow");

        bool observedCombatAction=observed.LastSkillUse.Values.Any(used=>now-used>=TimeSpan.Zero && now-used<=TimeSpan.FromSeconds(8));
        if(observedCombatAction)
        {
            // Generic activity is evidence only for the passively resolved local class.
            // Never broadcast filler readiness across classes: an unresolved/mismatched
            // class must fail closed instead of manufacturing recommendation state.
            var fillerSignal=observedClass switch
            {
                AionClass.Assassin => "AssassinFillerWindow",
                AionClass.Gladiator => "GladiatorFillerWindow",
                AionClass.Ranger => "RangerFillerWindow",
                AionClass.Sorcerer => "SorcererFillerWindow",
                AionClass.Spiritmaster => "SpiritmasterFillerWindow",
                AionClass.Cleric => "ClericFillerWindow",
                AionClass.Chanter => "ChanterFillerWindow",
                AionClass.Templar => "TemplarFillerWindow",
                _ => null
            };
            if(fillerSignal is not null)signals.Add(fillerSignal);
        }

        if(observedClass==AionClass.Templar)
        {
            // Current Global main attack chain. Each continuation requires the
            // immediately preceding cast to be observed; no chain state is guessed.
            if(observed.UsedRecently("Vicious Strike",now,3))signals.Add("TemplarDecisiveStrikeWindow");
            if(observed.UsedRecently("Decisive Strike",now,3))signals.Add("TemplarDesperateStrikeWindow");
            if(observed.UsedRecently("Desperate Strike",now,3))signals.Add("TemplarThreateningBlowWindow");
        }
        if(observedClass==AionClass.Gladiator)
        {
            if(observed.UsedRecently("Rending Blow",now,3))signals.Add("GladiatorSmashingWindow");
            if(observed.UsedRecently("Keen Strike",now,3))signals.Add("GladiatorRuptureWindow");
            if(observed.UsedRecently("Rupture Strike",now,3))signals.Add("GladiatorWrathfulWindow");
            if(observed.UsedRecently("Rage Burst",now,10))signals.Add("GladiatorOverheadWindow");
            if(observed.UsedRecently("Overhead Slam",now,3))signals.Add("GladiatorUpwardStrikeWindow");
            if(observed.UsedRecently("Crushing Wave",now,3))signals.Add("GladiatorFrenziedWaveWindow");
        }
        if(observedClass==AionClass.Assassin)
        {
            if(observed.UsedRecently("Quick Slice",now,3))signals.Add("AssassinBreakingSliceWindow");
            if(observed.UsedRecently("Breaking Slice",now,3))signals.Add("AssassinSwiftSliceWindow");
            if(observed.UsedRecently("Savage Roar",now,3))signals.Add("AssassinSavageBackKickWindow");
            if(observed.UsedRecently("Savage Back Kick",now,3))signals.Add("AssassinSavageSmashWindow");
            // Illusive Clone is a directly observed burst activation. Current Global
            // guidance gives it a 20s burst duration; do not infer specialty effects.
            if(observed.UsedRecently("Illusive Clone",now,20))signals.Add("AssassinBurstWindow");
        }
        if(observedClass==AionClass.Ranger)
        {
            // Precision must be observed, never inferred from generic activity.
            // Current Global evidence makes Deadshot the immediate high-value payoff.
            if(observed.Buffs.Contains("Precision"))
                signals.Add("RangerDeadshotWindow");
            // Burst Arrow is legal only when Slow or Root is actually observed on target.
            // Never infer crowd-control state from generic Ranger activity.
            if(observed.Debuffs.Contains("Slow")||observed.Debuffs.Contains("Root"))
                signals.Add("RangerBurstArrowWindow");
            if(observed.UsedRecently("Snipe",now,3))
                signals.Add("RangerRapidFireWindow");
            if(observed.UsedRecently("Rapid Fire",now,3))
                signals.Add("RangerSpiralArrowWindow");
        }
        if(observedClass==AionClass.Sorcerer)
        {
            // Current Global Wish of Concentration grants its self-buff for 10s.
            // Build-dependent cooldown effects remain excluded.
            if(observed.UsedRecently("Wish of Concentration",now,10))
                signals.Add("SorcererBurstWindow");
            if(observed.UsedRecently("Ice Chain",now,3))
                signals.Add("SorcererColdWaveWindow");

            // Current Global Fire Mark lasts 5s after a Fire hit. Reconstruct only
            // from named fire skills with current evidence; never treat generic combat
            // or specialty-dependent cooldown behavior as proof of the target mark.
            string[] fireMarkSources={"Flame Arrow","Blaze","Hellfire","Firestorm","Fire Wall","Delayed Explosion"};
            if(fireMarkSources.Any(skill=>observed.UsedRecently(skill,now,5)))
                signals.Add("SorcererFireMarkWindow");
        }
        if(observedClass==AionClass.Cleric)
        {
            // Condemnation itself requires Chain of Torment on the target.
            if(observed.UsedRecently("Chain of Torment",now,10))
                signals.Add("ClericCondemnationWindow");

            // Earth's Punishment is a separate, directly observable target state.
            // It materially changes the Condemnation loop in current Global builds,
            // but specialty-dependent guaranteed-crit/reset behavior is not inferred here.
            if(observed.Debuffs.Contains("Earth Punishment"))
                signals.Add("ClericEarthPunishmentWindow");
        }
        if(observedClass==AionClass.Chanter)
        {
            // Current Global Wave Blow requires a stunned target. Only an observed
            // target status can open this path; generic Chanter activity cannot.
            if(observed.Debuffs.Contains("Stun"))
                signals.Add("ChanterWaveBlowWindow");
            // Current Global changed Dark Crush to activate after using a ranged skill.
            // Spinning Strike and Impactful Crush are both observed base-kit ranged
            // openers for the short Dark Crush window. Stigma-only ranged triggers
            // remain excluded until the equipped loadout can be observed.
            // Current Global Dark Crush activates after use of a ranged skill.
            // These are named, documented ranged setup skills in the current PvE loop;
            // do not infer activation from generic combat or melee activity.
            string[] darkCrushSetups={"Spinning Strike","Impactful Crush"};
            if(darkCrushSetups.Any(skill=>observed.UsedRecently(skill,now,3)))
                signals.Add("ChanterDarkCrushWindow");
            if(observed.UsedRecently("Onslaught",now,3))
                signals.Add("ChanterResonanceCrushWindow");
            if(observed.UsedRecently("Resonance Crush",now,3))
                signals.Add("ChanterBoltCrushWindow");
        }
        if(observedClass==AionClass.Spiritmaster)
        {
            if(observed.UsedRecently("Flame Blessing",now,8)||observed.UsedRecently("Spirit's Benediction",now,8))
                signals.Add("SpiritmasterAncientWindow");
            if(observed.UsedRecently("Summon: Ancient Spirit",now,8))
                signals.Add("SpiritmasterCorrodeWindow");

            // Current Global base Corrode lasts 20s. Preserve that target state from
            // an observed local Corrode cast when a separate debuff event is unavailable.
            // The specialization extension to 30s is intentionally not inferred.
            if(observed.Debuffs.Contains("Corrode") || observed.UsedRecently("Jointstrike: Corrode",now,20))
                signals.Add("SpiritmasterCorrodeActiveWindow");
            else
                signals.Add("SpiritmasterCorrodeMissingWindow");

            // Elemental Fusion is enabled by the four-element state. Consume only a
            // passively observed state name; never infer stacks from generic combat.
            if(observed.Buffs.Contains("Four Elements"))
                signals.Add("SpiritmasterBurstWindow");

            string[] normalSpiritSummons={"Summon: Fire Spirit","Summon: Water Spirit","Summon: Earth Spirit","Summon: Wind Spirit"};
            if(normalSpiritSummons.Any(skill=>observed.UsedRecently(skill,now,3)))
                signals.Add("SpiritmasterDimensionalControlWindow");
        }
        return signals;
    }
}
