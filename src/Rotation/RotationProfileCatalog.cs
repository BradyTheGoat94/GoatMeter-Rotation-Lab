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
                new RotationRule("Punishment",800,new[]{new RotationCondition(RotationConditionKind.CooldownReady,"Punishment",Reason:"validated 30s base cooldown is ready; current Global guidance prioritizes Punishment")}),
                new RotationRule("Judgment",850,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"JudgmentWindow",Reason:"observed shield skill opened the Judgment damage window")}),
                new RotationRule("Annihilate",775,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"TemplarAnnihilateWindow",Reason:"current Global Annihilate requires an observed Stun or Knockdown target"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Annihilate",Reason:"validated 20s base cooldown is ready; level-12 -10s specialization excluded")
                }),
                new RotationRule("Empyrean Lord's Punishment",650,new[]{new RotationCondition(RotationConditionKind.CooldownReady,"Empyrean Lord's Punishment",Reason:"validated 60s base cooldown is ready")}),
                new RotationRule("Decisive Strike",575,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"TemplarDecisiveStrikeWindow",Reason:"continue the observed Global Vicious Strike chain")}),
                new RotationRule("Desperate Strike",570,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"TemplarDesperateStrikeWindow",Reason:"continue the observed Global Vicious Strike chain")}),
                new RotationRule("Threatening Blow",565,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"TemplarThreateningBlowWindow",Reason:"finish the observed Global Vicious Strike chain")}),
                new RotationRule("Pummel",500,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"TemplarFillerWindow",Reason:"weave Pummel between Judgment/Punishment opportunities")}),
                new RotationRule("Vicious Strike",300,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"TemplarFillerWindow",Reason:"start the Global main attack chain while higher priorities recover")})
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
                    new RotationCondition(RotationConditionKind.SignalPresent,"CriticalHitWindow",Reason:"passively observed critical hit can enable Heart Gore"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Heart Gore",Reason:"validated 5s base cooldown is ready; specialization resets are not assumed")
                }),
                new RotationRule("Insignia Explosion",600,new[]
                {
                    new RotationCondition(RotationConditionKind.SignalPresent,"InsigniaReady",Reason:"passively observed Insignia state supports explosion"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Insignia Explosion",Reason:"validated 10s base cooldown is ready; specialization reductions are not assumed")
                }),
                new RotationRule("Shadowstrike",625,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"AssassinBurstWindow",Reason:"use Shadowstrike inside the directly observed Illusive Clone burst window"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Shadowstrike",Reason:"validated 20s Global base cooldown is ready; rear-position safety is not inferred")
                }),
                new RotationRule("Breaking Slice",590,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"AssassinBreakingSliceWindow",Reason:"complete the current Global Quick Slice chain after its observed opener")}),
                new RotationRule("Swift Slice",585,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"AssassinSwiftSliceWindow",Reason:"complete the current Global Quick Slice chain after observed Breaking Slice")}),
                new RotationRule("Savage Back Kick",575,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"AssassinSavageBackKickWindow",Reason:"complete the current Global Savage Roar chain after its observed opener")}),
                new RotationRule("Savage Smash",570,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"AssassinSavageSmashWindow",Reason:"complete the current Global Savage chain after observed Savage Back Kick")}),
                new RotationRule("Savage Roar",400,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"AssassinFillerWindow",Reason:"build Insignias while core spenders are unavailable")}),
                new RotationRule("Savage Fang",375,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"AssassinFillerWindow",Reason:"use the repeatedly observed Global sustained strike while spender state is unavailable")}),
                new RotationRule("Exploit Weakness",350,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"AssassinFillerWindow",Reason:"include the repeatedly observed Global damage action without inferring hidden Insignia state")}),
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
                new RotationRule("Upward Strike",850,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"GladiatorUpwardStrikeWindow",Reason:"complete the observed Overhead Slam chain immediately")}),
                new RotationRule("Overhead Slam",800,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"GladiatorOverheadWindow",Reason:"consume the observed Rage Burst Overhead Slam opportunity"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Overhead Slam",Reason:"validated 5s Global base cooldown is ready; level-16 no-cooldown specialization excluded")
                }),
                new RotationRule("Ruinous Blow",700,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"GladiatorDamageWindow",Reason:"establish the enabling damage state"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Ruinous Blow",Reason:"validated 45s Global base cooldown is ready; specialization reductions excluded")
                }),
                new RotationRule("Frenzied Wave",690,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"GladiatorFrenziedWaveWindow",Reason:"consume the current-Global 3s chain activation after observed Crushing Wave")}),
                new RotationRule("Crushing Wave",610,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"GladiatorFillerWindow",Reason:"current Global sustained damage action opens Frenzied Wave"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Crushing Wave",Reason:"validated 20s Global base cooldown is ready; rank-12 critical reset specialization excluded")
                }),
                new RotationRule("Rending Blow",600,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"GladiatorFillerWindow",Reason:"core sustained single-target damage after conditional attacks")}),
                new RotationRule("Smashing Blow",675,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"GladiatorSmashingWindow",Reason:"observed Rending Blow chain state supports Smashing Blow")}),
                new RotationRule("Seismic Crash",500,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"GladiatorFinisherWindow",Reason:"passively observed chain state supports Seismic Crash")}),
                new RotationRule("Wrathful Strike",670,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"GladiatorWrathfulWindow",Reason:"observed Keen Strike chain state supports the later Global chain attack")}),
                new RotationRule("Rupture Strike",665,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"GladiatorRuptureWindow",Reason:"observed Keen Strike chain state supports Rupture Strike")}),
                new RotationRule("Keen Strike",200,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"GladiatorFillerWindow",Reason:"resource-building weave while higher-priority attacks are unavailable")})
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
                new RotationRule("Marking Shot",700,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"RangerMarkWindow",Reason:"maintain Precision and Deadshot support only when a passively proven refresh window exists"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Marking Shot",Reason:"validated 10s current-Global base cooldown is ready; Precision state is never inferred from the cast")
                }),
                new RotationRule("Deadshot",650,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"RangerDeadshotWindow",Reason:"observed current-Global Precision state enables Deadshot's 35% damage bonus"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Deadshot",Reason:"validated 20s current Global base cooldown is ready; charge level and specialization modifiers remain player-controlled")
                }),
                new RotationRule("Burst Arrow",610,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"RangerBurstArrowWindow",Reason:"current Global Burst Arrow requires passively proven Slow or Root target state"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Burst Arrow",Reason:"validated current Global 20s base cooldown is ready")
                }),
                new RotationRule("Gale Arrow",600,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"RangerFillerWindow",Reason:"maintain the documented Global sustained Gale Arrow priority")}),
                new RotationRule("Drill Dart",585,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"RangerFillerWindow",Reason:"use a high-value sustained Global Ranger attack while stronger state actions are unavailable"),new RotationCondition(RotationConditionKind.CooldownReady,"Drill Dart",Reason:"validated 5s Global base cooldown is ready")}),
                new RotationRule("Tempest Shot",575,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"RangerFillerWindow",Reason:"continue the sustained Global Ranger loop while stronger state actions are unavailable")}),
                new RotationRule("Rapid Fire",640,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"RangerRapidFireWindow",Reason:"continue the observed Snipe chain")}),
                new RotationRule("Spiral Arrow",635,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"RangerSpiralArrowWindow",Reason:"finish the observed Snipe chain")}),
                new RotationRule("Snipe",620,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"RangerFillerWindow",Reason:"start the documented sustained Snipe chain while stronger state actions are unavailable")}),
            },
            "PROVISIONAL Global Season 1 single-target fixture. Precision, crowd-control and Snipe-chain decisions are passive/evidence-gated. Older Rupture Arrow/Destruction Trap hooks were removed because current Global captures do not yet reconcile that vocabulary; no hidden state is invented.");
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
                new RotationRule("Wish of Concentration",775,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"SorcererFillerWindow",Reason:"current Global self-buff grants +10% Attack and +100 Accuracy for 10s"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Wish of Concentration",Reason:"validated 60s Global base cooldown is ready; level-16 all-skill cooldown reduction specialization excluded")
                }),
                new RotationRule("Element Enhancement",800,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"SorcererFillerWindow",Reason:"align the proven stigma buff with active combat"),
                    new RotationCondition(RotationConditionKind.SignalPresent,"SorcererElementEnhancementKnownWindow",Reason:"the current loadout has passively proven Element Enhancement is equipped"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Element Enhancement",Reason:"validated 60s Global base cooldown is ready")
                }),
                new RotationRule("Delayed Explosion",700,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"SorcererBurstWindow",Reason:"apply the 4s self-damage-amplification effect inside a proven Sorcerer burst"),
                    new RotationCondition(RotationConditionKind.SignalPresent,"SorcererDelayedExplosionKnownWindow",Reason:"the current loadout has passively proven Delayed Explosion is equipped"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Delayed Explosion",Reason:"validated 30s Global base cooldown is ready; -10s specialization excluded")
                }),
                new RotationRule("Hellfire",650,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"SorcererBurstWindow",Reason:"use Hellfire during a proven burst window"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Hellfire",Reason:"validated 45s Global base cooldown is ready; charge choice remains situational")
                }),
                new RotationRule("Fire Wall",600,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"SorcererBurstWindow",Reason:"maintain high-value fire damage during burst"),
                    new RotationCondition(RotationConditionKind.SignalPresent,"SorcererFireWallKnownWindow",Reason:"the current loadout has passively proven Fire Wall is equipped"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Fire Wall",Reason:"validated 60s Global base cooldown is ready")
                }),
                new RotationRule("Cold Storm",550,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"SorcererBurstWindow",Reason:"use Cold Storm in the sustained burst sequence"),
                    new RotationCondition(RotationConditionKind.SignalPresent,"SorcererColdStormKnownWindow",Reason:"the current loadout has passively proven Cold Storm is equipped"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Cold Storm",Reason:"validated 60s Global base cooldown is ready")
                }),
                new RotationRule("Cold Wave",375,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SorcererColdWaveWindow",Reason:"consume the explicit 3s Global chain opportunity after observed Ice Chain")}),
                new RotationRule("Firestorm",565,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"SorcererFillerWindow",Reason:"current Global sustained APL places Firestorm above basic filler"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Firestorm",Reason:"validated 5s Global base cooldown is ready; specialty Hellfire reduction excluded")
                }),
                new RotationRule("Bittercold Wind",560,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"SorcererFillerWindow",Reason:"current Global sustained APL includes Bittercold Wind before basic filler"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Bittercold Wind",Reason:"validated 15s Global base cooldown is ready")
                }),
                new RotationRule("Blaze",575,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"SorcererFillerWindow",Reason:"use Blaze in the sustained Global damage loop"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Blaze",Reason:"validated 5s Global base cooldown is ready; specialty reductions excluded"),
                    new RotationCondition(RotationConditionKind.SignalPresent,"SorcererFireMarkWindow",Reason:"a directly observed Global fire hit establishes the Fire Mark window")
                }),
                new RotationRule("Blaze",574,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"SorcererFillerWindow",Reason:"use Blaze in the sustained Global damage loop"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Blaze",Reason:"validated 5s Global base cooldown is ready; specialty reductions excluded"),
                    new RotationCondition(RotationConditionKind.DebuffPresent,"Fire Mark",Reason:"an explicitly decoded Fire Mark target also proves Blaze eligibility")
                }),
                new RotationRule("Ice Chain",325,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SorcererFillerWindow",Reason:"continue with the repeatedly observed Global sustained spell without inferring hidden burst state")}),
                new RotationRule("Flame Arrow",200,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SorcererFillerWindow",Reason:"basic damage/MP filler while higher priorities are unavailable")})
            },
            "PROVISIONAL Global Season 1 single-target fixture. Current-Global Fire Mark and burst windows are reconstructed only from observed actions. Loadout-dependent stigmas are not recommended until observed in the current session; unreconciled Flame Cage/Flame Harpoon hooks are excluded rather than guessed.");
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
                new RotationRule("Summon: Ancient Spirit",750,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterAncientWindow",Reason:"summon Ancient Spirit after observed opener buffs so the summon snapshots the damage state"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Summon: Ancient Spirit",Reason:"validated 90s current-Global base cooldown is ready; specialization effects excluded")
                }),
                new RotationRule("Jointstrike: Corrode",700,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterCorrodeWindow",Reason:"apply the current-Global Corrode target state after observed Ancient Spirit setup"),
                    new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterCorrodeMissingWindow",Reason:"do not reapply while the proven base 20s Corrode window remains active"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Jointstrike: Corrode",Reason:"validated 45s current-Global base cooldown is ready; cooldown reduction is not inferred")
                }),
                new RotationRule("Elemental Fusion",650,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterBurstWindow",Reason:"fire Elemental Fusion when its proc/state is available")}),
                new RotationRule("Dimensional Control",625,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterDimensionalControlWindow",Reason:"consume the brief observed post-summon activation before returning to filler")}),
                new RotationRule("Jointstrike: Destructive Attack",490,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterFillerWindow",Reason:"use on cooldown without sacrificing higher-priority Fusion or stack state")}),
                new RotationRule("Cold Shock",575,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterFillerWindow",Reason:"current Global PvE priority places Cold Shock ahead of secondary filler")}),
                new RotationRule("Jointstrike: Curse",565,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterFillerWindow",Reason:"current Global PvE priority maintains Jointstrike: Curse before secondary filler"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Jointstrike: Curse",Reason:"validated 10s Global base cooldown is ready")
                }),
                new RotationRule("Combustion",550,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterFillerWindow",Reason:"use the repeatedly observed Global sustained damage action while stronger spirit windows are unavailable")}),
                new RotationRule("Earth Tremor",450,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterFillerWindow",Reason:"maintain basic-attack buff stacks during sustained damage")}),
                new RotationRule("Disenchant",400,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"SpiritmasterDispelWindow",Reason:"passively observed target state supports Disenchant")})
            },
            "PROVISIONAL Global Season 1 single-target fixture. Ancient Spirit may react to directly observed opener buffs, but unsupported pre-buff recommendations are not invented. Corrode, dispel and spirit-burst decisions remain gated on passive signals; no recommendation is emitted until the relevant state is proven.");
    }

    public static RotationProfile CreateProvisionalClericSingleTarget()
    {
        return new RotationProfile(
            AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,ProfileValidation.Provisional,
            new[]
            {
                new RotationRule("Earth Punishment",800,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"ClericFillerWindow",Reason:"maintain the proven current-Global damage loop during active combat"),
                    new RotationCondition(RotationConditionKind.SignalPresent,"ClericEarthPunishmentKnownWindow",Reason:"the current loadout has passively proven Earth Punishment is equipped"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Earth Punishment",Reason:"validated 30s Global base cooldown is ready; +10s duration specialization excluded")
                }),
                new RotationRule("Condemnation",825,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"ClericCondemnationWindow",Reason:"target has the observed Chain of Torment prerequisite"),
                    new RotationCondition(RotationConditionKind.SignalPresent,"ClericEarthPunishmentWindow",Reason:"observed Earth Punishment state supports the high-value Condemnation damage window"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Condemnation",Reason:"validated 3s Global base cooldown must still be ready in the Earth Punishment window")
                }),
                new RotationRule("Condemnation",750,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"ClericCondemnationWindow",Reason:"Chain of Torment prerequisite is observed; do not infer specialty-dependent reset state"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Condemnation",Reason:"validated 3s Global base cooldown is ready unless an observed specialization reset proves otherwise")
                }),
                new RotationRule("Chain of Torment",650,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"ClericFillerWindow",Reason:"maintain the mark required by the Condemnation loop during observed combat"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Chain of Torment",Reason:"validated 20s Global base cooldown is ready; +3s duration specialization excluded")
                }),
                new RotationRule("Divine Aura",550,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"ClericFillerWindow",Reason:"use Divine Aura on validated cooldown during observed combat"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Divine Aura",Reason:"validated 30s Global base cooldown is ready; level-16 -10s specialization excluded")
                }),
                new RotationRule("Bolt",525,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"ClericFillerWindow",Reason:"current Global PvE APL treats Bolt as a major damage action during observed combat"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Bolt",Reason:"validated 45s Global base cooldown is ready; Discharge-driven cooldown reduction is not inferred")
                }),
                new RotationRule("Judgment Thunder",300,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ClericFillerWindow",Reason:"damage filler while higher priorities are unavailable")}),
                new RotationRule("Earth's Retribution",200,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ClericFillerWindow",Reason:"weave the MP-restoring basic damage action")}),
            },
            "PROVISIONAL Global Season 1 single-target damage fixture. Base cooldowns and directly observed debuff state are passive-gated; stigma/loadout-dependent Earth Punishment is not recommended until observed in the current session, and healing remains outside this damage profile.");
    }

    public static RotationProfile CreateProvisionalChanterSingleTarget()
    {
        return new RotationProfile(
            AionClass.Chanter,"global-chanter-provisional",RotationMode.SingleTarget,ProfileValidation.Provisional,
            new[]
            {
                new RotationRule("Dark Crush",800,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"ChanterDarkCrushWindow",Reason:"use immediately after the observed current-Global ranged setup"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Dark Crush",Reason:"validated 5s base cooldown is ready; level-16 no-cooldown specialization excluded")
                }),
                new RotationRule("Resonance Crush",575,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ChanterResonanceCrushWindow",Reason:"continue the observed Onslaught chain")}),
                new RotationRule("Bolt Crush",570,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ChanterBoltCrushWindow",Reason:"finish the observed Onslaught chain")}),
                new RotationRule("Heat Wave Blow",700,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"ChanterBurstWindow",Reason:"place heavy burst inside a proven vulnerability/damage window"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Heat Wave Blow",Reason:"validated 10s Global base cooldown is ready")
                }),
                new RotationRule("Wave Blow",650,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"ChanterWaveBlowWindow",Reason:"current Global Wave Blow consumes the observed Stun opportunity and then applies Knockdown"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Wave Blow",Reason:"validated 20s current-Global base cooldown is ready; incapacitation-immunity specialization behavior excluded")
                }),
                new RotationRule("Impactful Crush",625,new[]{
                    new RotationCondition(RotationConditionKind.SignalPresent,"ChanterFillerWindow",Reason:"current Global APL places Impactful Crush above the Onslaught chain"),
                    new RotationCondition(RotationConditionKind.CooldownReady,"Impactful Crush",Reason:"validated current Global 15s base cooldown is ready")
                }),
                new RotationRule("Incandescent Blow",550,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ChanterFillerWindow",Reason:"core sustained damage while reaction/burst skills are unavailable")}),
                new RotationRule("Bursting Blow",525,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ChanterFillerWindow",Reason:"use the repeatedly observed Global sustained strike while stronger Chanter windows are unavailable")}),
                new RotationRule("Fracturing Blow",510,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ChanterFillerWindow",Reason:"continue the observed Global sustained strike sequence without inventing hidden proc state")}),
                new RotationRule("Onslaught",500,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ChanterFillerWindow",Reason:"sustain damage and MP while higher priorities recover")}),
                new RotationRule("Spinning Strike",450,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ChanterFillerWindow",Reason:"fill downtime and support the reaction-damage loop")}),
                new RotationRule("Healing Burst",100,new[]{new RotationCondition(RotationConditionKind.SignalPresent,"ChanterHealWindow",Reason:"interrupt damage priority only when healing state requires it")})
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
