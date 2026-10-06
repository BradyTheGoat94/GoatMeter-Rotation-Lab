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
            if(observed.PendingFollowUp("Vicious Strike","Decisive Strike",now,3))signals.Add("TemplarDecisiveStrikeWindow");
            if(observed.PendingFollowUp("Decisive Strike","Desperate Strike",now,3))signals.Add("TemplarDesperateStrikeWindow");
            if(observed.PendingFollowUp("Desperate Strike","Threatening Blow",now,3))signals.Add("TemplarThreateningBlowWindow");

            // Pummel has a guaranteed Punishing Strike chain activation for 3s.
            if(observed.PendingFollowUp("Pummel","Punishing Strike",now,3))
                signals.Add("TemplarPunishingStrikeWindow");

            // Empyrean Lord's Punishment is a stigma. Recommend later uses only
            // after the current zone/session has directly observed it once.
            if(observed.LastSkillUse.ContainsKey("Empyrean Lord's Punishment"))
                signals.Add("TemplarEmpyreanKnownWindow");

            // Current Global Punishment grants Executor for 20s. Reconstruct only
            // the base duration from a directly observed local cast.
            if(observed.UsedRecently("Punishment",now,20))
                signals.Add("TemplarExecutorWindow");

            // Current Global Annihilate is enabled by observed target Stun or Knockdown.
            // Do not infer its low-chance Incapacitated-Immunity activation.
            if(observed.Debuffs.Contains("Stun") || observed.Debuffs.Contains("Knockdown"))
                signals.Add("TemplarAnnihilateWindow");
        }
        if(observedClass==AionClass.Gladiator)
        {
            if(observed.PendingFollowUp("Rending Blow","Smashing Blow",now,3))signals.Add("GladiatorSmashingWindow");
            if(observed.PendingFollowUp("Keen Strike","Rupture Strike",now,3))signals.Add("GladiatorRuptureWindow");
            if(observed.PendingFollowUp("Rupture Strike","Wrathful Strike",now,3))signals.Add("GladiatorWrathfulWindow");
            if(observed.UsedRecently("Rage Burst",now,10))signals.Add("GladiatorOverheadWindow");
            if(observed.PendingFollowUp("Overhead Slam","Upward Strike",now,3))signals.Add("GladiatorUpwardStrikeWindow");
            if(observed.PendingFollowUp("Crushing Wave","Frenzied Wave",now,3))signals.Add("GladiatorFrenziedWaveWindow");

            // Rage Burst is a loadout-dependent stigma. Recommend later uses only
            // after the current zone/session has directly observed it once.
            if(observed.LastSkillUse.ContainsKey("Rage Burst"))
                signals.Add("GladiatorRageBurstKnownWindow");

            // Current Global Ruinous Blow grants Prepare for Battle for 20s.
            // Preserve only the base window from a directly observed local cast.
            if(observed.UsedRecently("Ruinous Blow",now,20))
                signals.Add("GladiatorPrepareForBattleWindow");
        }
        if(observedClass==AionClass.Assassin)
        {
            if(observed.PendingFollowUp("Quick Slice","Breaking Slice",now,3))signals.Add("AssassinBreakingSliceWindow");
            if(observed.PendingFollowUp("Breaking Slice","Swift Slice",now,3))signals.Add("AssassinSwiftSliceWindow");
            if(observed.PendingFollowUp("Savage Roar","Savage Back Kick",now,3))signals.Add("AssassinSavageBackKickWindow");
            if(observed.PendingFollowUp("Savage Back Kick","Savage Smash",now,3))signals.Add("AssassinSavageSmashWindow");
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
            if(observed.PendingFollowUp("Snipe","Rapid Fire",now,3))
                signals.Add("RangerRapidFireWindow");
            if(observed.PendingFollowUp("Rapid Fire","Spiral Arrow",now,3))
                signals.Add("RangerSpiralArrowWindow");
        }
        if(observedClass==AionClass.Sorcerer)
        {
            // Current Global Wish and Element Enhancement each create a 10s personal
            // burst window. Delayed Explosion proves a shorter 4s self-damage-amplification
            // window. Specialty cooldown reductions are never inferred.
            if(observed.UsedRecently("Wish of Concentration",now,10)
                || observed.UsedRecently("Element Enhancement",now,10)
                || observed.UsedRecently("Delayed Explosion",now,4))
                signals.Add("SorcererBurstWindow");

            if(observed.PendingFollowUp("Ice Chain","Cold Wave",now,3))
                signals.Add("SorcererColdWaveWindow");

            // Stigmas are loadout-dependent. Future recommendations are enabled only
            // after the current session has directly observed that stigma being used.
            if(observed.LastSkillUse.ContainsKey("Element Enhancement"))
                signals.Add("SorcererElementEnhancementKnownWindow");
            if(observed.LastSkillUse.ContainsKey("Delayed Explosion"))
                signals.Add("SorcererDelayedExplosionKnownWindow");
            if(observed.LastSkillUse.ContainsKey("Fire Wall"))
                signals.Add("SorcererFireWallKnownWindow");
            if(observed.LastSkillUse.ContainsKey("Cold Storm"))
                signals.Add("SorcererColdStormKnownWindow");

            // Since the Sep-16 Global change, every landed Fire attack applies Fire Mark
            // for 5s and Blaze no longer consumes it. Reconstruct only from named Fire
            // actions; generic combat still cannot manufacture the target mark.
            string[] fireMarkSources={"Flame Arrow","Burst","Pyroclasm","Flame Harpoon","Blaze","Hellfire","Firestorm","Fire Wall","Delayed Explosion"};
            if(fireMarkSources.Any(skill=>observed.UsedRecently(skill,now,5)))
                signals.Add("SorcererFireMarkWindow");
        }
        if(observedClass==AionClass.Cleric)
        {
            // Base Chain of Torment lasts 10s. A directly observed debuff remains
            // authoritative beyond that window (for example a specialization extension),
            // but the +3s specialization is never inferred from the cast alone.
            if(observed.Debuffs.Contains("Chain of Torment") || observed.UsedRecently("Chain of Torment",now,10))
                signals.Add("ClericCondemnationWindow");

            // Earth Punishment is a stigma, so only recommend future casts after this
            // loadout has been proven by an observed local use in the current zone/session.
            if(observed.LastSkillUse.ContainsKey("Earth Punishment"))
                signals.Add("ClericEarthPunishmentKnownWindow");

            // Base Earth Punishment lasts 10s. Preserve that state from an observed
            // local cast when a separate debuff event is unavailable. A longer directly
            // observed debuff is honored, but the +10s specialization is not inferred.
            if(observed.Debuffs.Contains("Earth Punishment") || observed.UsedRecently("Earth Punishment",now,10))
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
            // Current Global capture evidence currently proves Impactful Crush as the
            // passive Dark Crush trigger. Do not broaden this to other ranged attacks
            // until their activation relationship is directly corroborated.
            if(observed.UsedRecently("Impactful Crush",now,3))
                signals.Add("ChanterDarkCrushWindow");
            if(observed.PendingFollowUp("Onslaught","Resonance Crush",now,3))
                signals.Add("ChanterResonanceCrushWindow");
            if(observed.PendingFollowUp("Resonance Crush","Bolt Crush",now,3))
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
