using Aion2DPSPro;
using Aion2DPSPro.Protocol;
using Aion2DPSPro.Storage;
using Aion2DPSPro.Rotation;

int checks=0;
void Equal(double actual,double expected,string label) {checks++;if(Math.Abs(actual-expected)>.00001)throw new Exception($"{label}: expected {expected}, got {actual}");}
void True(bool actual,string label) {checks++;if(!actual)throw new Exception(label);}
var t=new DateTime(2026,10,3,0,0,0,DateTimeKind.Utc);
var now=t;
var e=new CombatEngine(()=>now);
CombatEvent Hit(int seconds,long id,long amount,string name="Player",bool boss=false)=>new(t.AddSeconds(seconds),CombatKind.Damage,id,name,99,"Boss","Strike",amount,IsBoss:boss);
e.Apply(new(t,CombatKind.PlayerName,1,"Player"));e.Apply(Hit(0,1,100));e.Apply(Hit(5,1,200));
Equal(e.Snapshot().FightDamage,300,"damage sum");Equal(e.Snapshot().FightDps,60,"encounter DPS");
now=t.AddSeconds(7);Equal(e.Snapshot().FightDps,60,"DPS frozen during idle");
e.Apply(new(t.AddSeconds(2),CombatKind.BuffApply,1,"Player",1,"Player",Effect:"Power",Stacks:1));
e.Apply(new(t.AddSeconds(6),CombatKind.BuffRemove,1,"Player",1,"Player",Effect:"Power"));
// Late timestamps must not reset encounter activity.
now=t.AddSeconds(13);True(!e.Snapshot().InFight,"timeout completes");Equal(e.Snapshot(MeterSegment.Previous,MeterCategory.Damage).FightDamage,300,"completed preserved");Equal(e.History.Count,1,"single completion");
e.Apply(new(t.AddSeconds(50),CombatKind.PlayerName,2,"Player"));e.Apply(Hit(50,2,400,"Player"));Equal(e.Snapshot(MeterSegment.Previous,MeterCategory.Damage).FightDamage,300,"previous fight");
Equal(e.Snapshot(MeterSegment.Overall,MeterCategory.Damage).FightDamage,700,"overall sum");
e.Apply(new(t.AddSeconds(51),CombatKind.Heal,2,"Player",1,"Player","Heal",50));Equal(e.Snapshot(MeterSegment.Current,MeterCategory.Healing).Players.Single().Damage,50,"healing independent");
Equal(e.Snapshot(MeterSegment.Current,MeterCategory.DamageTaken).Players.Single().Damage,400,"damage taken independent");
e.Apply(new(t.AddSeconds(52),CombatKind.Death,TargetId:2));Equal(e.Snapshot(MeterSegment.Current,MeterCategory.Deaths).Players.Single().Damage,1,"death target");
e.Apply(new(t.AddSeconds(53),CombatKind.Interrupt,2));Equal(e.Snapshot(MeterSegment.Current,MeterCategory.Interrupts).Players.Single().Damage,1,"interrupt count");
e.Apply(new(t.AddSeconds(54),CombatKind.Dispel,2));Equal(e.Snapshot(MeterSegment.Current,MeterCategory.Dispels).Players.Single().Damage,1,"dispel count");
var b=new CombatEngine(()=>t.AddSeconds(7));b.Apply(Hit(0,1,100));b.Apply(new(t.AddSeconds(2),CombatKind.BuffApply,1,"Player",1,"Player",Effect:"Power",Stacks:2));b.Apply(new(t.AddSeconds(4),CombatKind.BuffRemove,1,"Player",1,"Player",Effect:"Power"));b.Apply(Hit(5,1,100));
Equal(b.Snapshot().Buffs.Single().Uptime,40,"buff actual fight denominator");
b.Apply(new(t.AddSeconds(6),CombatKind.PlayerName,1,"Resolved",SourceClass:"Cleric"));b.Apply(new(t.AddSeconds(6),CombatKind.PlayerName,2,"Player"));b.Apply(Hit(6,2,300,"Player"));Equal(b.Snapshot().Players.Count,2,"same names remain separate");Equal(b.Snapshot().Skills.Count,2,"skills per actor");
True(b.Snapshot().Players.Any(p=>p.Name=="Resolved"&&p.ClassName=="Cleric"&&p.Damage==200),"late identity");
b.Apply(new(t.AddSeconds(6),CombatKind.Despawn,1));b.Apply(new(t.AddSeconds(7),CombatKind.PlayerName,1,"Replacement"));b.Apply(Hit(7,1,50,"Replacement"));True(b.Snapshot().Players.Any(p=>p.Name=="Resolved"&&p.Damage==200),"reused ID history preserved");True(b.Snapshot().Players.Any(p=>p.Name=="Replacement"&&p.Damage==50),"reused ID new actor");
var pet=new CombatEngine(()=>t);pet.Apply(new(t,CombatKind.PlayerName,1,"Owner"));pet.Apply(new(t,CombatKind.Ownership,5,OwnerId:1));pet.Apply(Hit(0,5,90,"Summon"));True(pet.Snapshot().Players.Single().Name=="Owner","explicit pet ownership");
var bossNow=t;var bossEngine=new CombatEngine(()=>bossNow);bossEngine.Apply(new(t,CombatKind.PlayerName,1,"Player"));bossEngine.Apply(Hit(0,1,100,boss:true));bossNow=t.AddSeconds(7);True(bossEngine.Snapshot().InFight,"boss retained inside inactivity window");bossEngine.Apply(new(t.AddSeconds(7),CombatKind.TargetHp,TargetId:99,CurrentHp:0,MaxHp:1000,IsBoss:true));True(!bossEngine.Snapshot().InFight,"boss death completes");
var tcp=new TcpStreamReassembler();
Equal(tcp.Push("a",100,new byte[]{1,2,3},t).SelectMany(x=>x).Count(),3,"initial TCP");
Equal(tcp.Push("a",102,new byte[]{3,4,5},t).SelectMany(x=>x).Count(),2,"overlap keeps new suffix");
Equal(tcp.Push("a",102,new byte[]{3,4,5},t).Count,0,"duplicate discarded");
Equal(tcp.Push("a",108,new byte[]{9},t).Count,0,"out of order waits");Equal(tcp.Push("a",105,new byte[]{6,7,8},t).SelectMany(x=>x).Count(),4,"gap filled flushes");
int resets=0;tcp.StreamReset+=_=>resets++;tcp.Push("a",120,new byte[]{1},t);tcp.Push("a",121,new byte[]{2},t.AddSeconds(6));True(resets>0,"gap recovery reset");
Equal(tcp.Push("wrap",uint.MaxValue-1,new byte[]{1,2,3},t).Count,1,"sequence wrap first");Equal(tcp.Push("wrap",1,new byte[]{4},t).Count,1,"sequence wrap continuation");
var profile=new ProtocolProfile("fixture","Global","fixture",13328,new Dictionary<string,PacketTag>{{"damage",new(4,56)},{"entityRemoved",new(33,141)}});
// Observed frame regressions, with logged values independently encoded here. These are NOT a ground-truth fight total.
var observed=new[]{("210438E3A00204008D3540B7B70009020B95C34701000000D658E7020100",359L),("210438E3A00204008D3540B7B70009021595C34702000000D658E8020200",360L),("240438E3A00224008D3540B7B70009021595C34703000000D65888030203030300",392L),("280438E3A00226008D351042B7000B020800004BCE954701000000D658D50F031414140100",2005L)};
foreach(var (hex,amount) in observed)
{
 var decoder=new CurrentClientDecoder(profile);var evt=decoder.Decode(Convert.FromHexString(hex),t).Single(x=>x.Kind==CombatKind.Damage);Equal(evt.Amount,amount,"observed damage replay");Equal(evt.SourceId,6797,"observed actor");Equal(evt.TargetId,36963,"observed target");
 var split=new CurrentClientDecoder(profile);var bytes=Convert.FromHexString(hex);True(!split.DecodeStream("in",bytes[..8],t).Any(),"split frame waits");Equal(split.DecodeStream("in",bytes[8..],t).Single(x=>x.Kind==CombatKind.Damage).Amount,amount,"split frame replay");
}
var directions=new CurrentClientDecoder(profile);var f=Convert.FromHexString(observed[0].Item1);directions.DecodeStream("in",f[..8],t).ToArray();directions.DecodeStream("out",f[..5],t).ToArray();Equal(directions.DecodeStream("in",f[8..],t).Single().Amount,359,"directions isolated");
var removal=new CurrentClientDecoder(profile).Decode(Convert.FromHexString("0B218DED8A010000"),t).Single();True(removal.Kind==CombatKind.Despawn,"observed removal");Equal(removal.SourceId,17773,"observed removal ID");
Equal(Lz4BlockDecoder.Decompress(new byte[]{0,0,0},new byte[10]),-1,"bad LZ4 rejects");
var folder=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N"));try {var store=new FightStore(folder);await store.SaveAsync(b.Snapshot());var saved=await store.LoadAsync(store.List().Single());Equal(saved!.FightDamage,b.Snapshot().FightDamage,"history roundtrip");Equal(saved.Skills.Count,b.Snapshot().Skills.Count,"history skill roundtrip");}finally{Directory.Delete(folder,true);}
var reuseRow=b.Snapshot().Players.Single(p=>p.Name=="Replacement");Equal(b.Snapshot().RecentEvents.Where(x=>x.Kind==CombatKind.Damage&&x.SourceId==reuseRow.ActorId).Sum(x=>x.Amount),50,"reused actor report events isolated");
var savedSnapshot=e.Snapshot();e.ResetFight();True(e.History.Last().Categories[MeterCategory.Healing].Players.Count>0,"completed history retains healing dataset");
// Sanitized identity prefixes observed in the new log: no private names or inventory data.
byte[] Frame(string bodyHex)
{
 var body=Convert.FromHexString(bodyHex);var length=new List<byte>();uint v=(uint)body.Length+4;
 do {byte part=(byte)(v&127);v>>=7;length.Add((byte)(part|(v>0?128:0)));}while(v>0);
 return length.Concat(body).ToArray();
}
var identityProfile=profile with {Tags=new Dictionary<string,PacketTag>(profile.Tags) {{"selfInfo",new(51,54)},{"otherInfo",new(69,54)}}};
var identityDecoder=new CurrentClientDecoder(identityProfile);
var self=identityDecoder.Decode(Frame("3336FD235F81C1283708546573744865726F000000"),t).Single(x=>x.Kind==CombatKind.PlayerName);
True(self.Source=="TestHero"&&self.SourceId==4605,"observed self identity prefix");
// Sanitized replay of selfInfo -> local entity removal -> damage in the 13:00 capture.
var localDecoder=new CurrentClientDecoder(identityProfile);
localDecoder.Decode(Frame("3336D10B5F81C1283708546573744865726F000000"),t).ToArray();
True(!localDecoder.Decode(Convert.FromHexString("0A218DD10B0001"),t.AddSeconds(3)).Any(x=>x.Kind==CombatKind.Despawn),"local removal must not invalidate self identity");
var localHit=localDecoder.Decode(Convert.FromHexString("230438E3A0022400D10B40B7B70001020B95C34701000000D658F10201030100"),t.AddSeconds(3)).Single(x=>x.Kind==CombatKind.Damage);
True(localHit.SourceId==1489&&localHit.Source=="TestHero","captured local removal sequence retains character name");
True(localDecoder.Decode(Convert.FromHexString("0B218DED8A010000"),t.AddSeconds(4)).Any(x=>x.Kind==CombatKind.Despawn),"other entities still despawn");
localDecoder.ResetConnection();
True(localDecoder.Decode(Convert.FromHexString("0A218DD10B0001"),t.AddSeconds(5)).Any(x=>x.Kind==CombatKind.Despawn),"reset clears protected self identity");
var ally=identityDecoder.Decode(Frame("4536A2210120A401070854657374416C6C7913000000"),t).Last(x=>x.Kind==CombatKind.PlayerName);
True(ally.Source=="TestAlly"&&ally.SourceId==4258,"observed other identity prefix");
True(!identityDecoder.Decode(Frame("218DA2210001"),t.AddSeconds(1)).Any(x=>x.Kind==CombatKind.Despawn),"known ally visibility removal preserves identity");
var allyDamage=identityDecoder.Decode(Convert.FromHexString("210438E3A0020400A22140B7B70009020B95C34701000000D658E7020100"),t.AddSeconds(2)).Single(x=>x.Kind==CombatKind.Damage);
True(allyDamage.Source=="TestAlly","ally name remains after visibility removal");
var nameReuse=new CombatEngine(()=>t);
nameReuse.Apply(new(t,CombatKind.PlayerName,4258,"FirstPlayer"));
nameReuse.Apply(Hit(0,4258,100,"FirstPlayer"));
nameReuse.Apply(new(t.AddSeconds(1),CombatKind.PlayerName,4258,"SecondPlayer"));
nameReuse.Apply(Hit(2,4258,50,"SecondPlayer"));
True(nameReuse.Snapshot().Players.Any(p=>p.Name=="FirstPlayer"&&p.Damage==100),"explicit name reuse preserves old totals");
True(nameReuse.Snapshot().Players.Any(p=>p.Name=="SecondPlayer"&&p.Damage==50),"explicit name reuse separates new totals");
var bridge=new Aion2DPSPro.Capture.CaptureIdentityBridge();string scope="adapter|local|server";
bridge.Observe(scope,new(t,CombatKind.PlayerName,self.SourceId,self.Source));bridge.Observe(scope,new(t,CombatKind.PlayerName,ally.SourceId,ally.Source));
var bridged=new CombatEngine(()=>t);
foreach(var identity in bridge.Identities(scope,t))bridged.Apply(identity);
bridged.Apply(bridge.Resolve(scope,Hit(0,4605,100,"Actor 4605")));bridged.Apply(bridge.Resolve(scope,Hit(0,4258,50,"Actor 4258")));
True(bridged.Snapshot().Players.Any(p=>p.Name=="TestHero"&&p.EntityId==4605),"pre-lock self identity reaches combat rows");
True(bridged.Snapshot().Players.Any(p=>p.Name=="TestAlly"&&p.EntityId==4258),"sibling identity reaches exact actor");
True(bridge.Resolve(scope,Hit(0,4605,100,"FreshName")).Source=="FreshName","bridge preserves explicitly decoded names");
True(bridge.Resolve("adapter|local|different-server",Hit(0,4605,100,"Actor 4605")).Source=="Actor 4605","identity scope isolation");
bridge.Observe(scope,new(t,CombatKind.Despawn,4605));True(bridge.Resolve(scope,Hit(0,4605,100,"Actor 4605")).Source=="Actor 4605","bridge removes stale identity");
True(bridge.Identities(scope,t.AddMinutes(11)).Count==0,"identity cache expires");
var reportEngine=new CombatEngine(()=>t);reportEngine.Apply(new(t,CombatKind.PlayerName,1,"Tester",SourceClass:"Templar"));reportEngine.Apply(new(t,CombatKind.Damage,1,"Tester",2,"Dummy","One",100,DamageFlags:DamageFlags.Critical|DamageFlags.Back|DamageFlags.Perfect));reportEngine.Apply(new(t.AddSeconds(1),CombatKind.Heal,1,"Tester",1,"Tester","Mend",50));
var full=reportEngine.Snapshot(MeterSegment.Current,MeterCategory.Damage,true);Equal(full.Categories[MeterCategory.Healing].Skills.Single().Damage,50,"report retains healing skills");Equal(full.Skills.Single().FlagHits[DamageFlags.Perfect],1,"perfect counts");Equal(full.Targets.Single().Damage,100,"actor target totals");
reportEngine.ResetFight();reportEngine.Apply(Hit(10,1,999));Equal(reportEngine.SnapshotEncounter(full.EncounterId)!.FightDamage,100,"open report stays on its encounter");
True(PublicGameData.EnglishTargetLabel("Training Scarecrow")=="Training Scarecrow","English target label preserved");
True(PublicGameData.EnglishTargetLabel("노인 주민")==null,"untranslated Korean NPC falls back to English target label");
True(PublicGameData.EnglishTargetLabel("Boss 마족")==null,"mixed language target label rejected");
// Rotation Lab fail-closed safety regressions.
var stubs=RotationProfileCatalog.CreateUnvalidatedGlobalStubs();
Equal(stubs.Count,8,"all eight rotation class stubs");
True(stubs.All(x=>x.Validation==ProfileValidation.Unvalidated&&x.Rules.Count==0),"unvalidated profiles contain no guessed rules");
var rotationEngine=new RotationEngine();
var rotationState=new RotationState(t,AionClass.Templar,"global-unvalidated",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),100,100,1,false,true,1.0);
var emptyDecision=rotationEngine.Evaluate(rotationState,stubs.Single(x=>x.ClassName==AionClass.Templar));
True(emptyDecision.Next is null,"unvalidated empty profile yields no recommendation");
var validatedProfile=new RotationProfile(AionClass.Templar,"fixture",RotationMode.SingleTarget,ProfileValidation.Validated,
    new[]{new RotationRule("Fixture Strike",100,new[]{new RotationCondition(RotationConditionKind.CooldownReady,"Fixture Strike")})},
    "Regression fixture only");
var validatedState=rotationState with {BuildId="fixture",CooldownSeconds=new Dictionary<string,double>{{"Fixture Strike",0}},ObservationConfidence=.95};
var recommendation=rotationEngine.Evaluate(validatedState,validatedProfile);
True(recommendation.Next?.Skill=="Fixture Strike"&&recommendation.Next.Actionable,"validated high-confidence rule can recommend");
var lowConfidence=rotationEngine.Evaluate(validatedState with {ObservationConfidence=.4},validatedProfile);
True(lowConfidence.Next is not null&&!lowConfidence.Next.Actionable,"low-confidence observation is informational only");
var tracker=new PassiveRotationStateTracker();
tracker.Observe(new(t,CombatKind.PlayerName,77,"Tester",SourceClass:"Templar",SourceIdentityConfirmed:true));
tracker.Observe(new(t.AddSeconds(1),CombatKind.Cast,77,"Tester",Skill:"Observed Cast",SourceClass:"Templar"));
tracker.Observe(new(t.AddSeconds(2),CombatKind.Damage,77,"Tester",99,"Dummy","Observed Strike",100,SourceClass:"Templar"));
tracker.Observe(new(t.AddSeconds(3),CombatKind.BuffApply,77,"Tester",77,"Tester",Effect:"Observed Buff",SourceClass:"Templar"));
var observedRotation=tracker.Snapshot();
True(observedRotation.PlayerId==77&&observedRotation.ClassName==AionClass.Templar,"passive tracker resolves confirmed class");
True(observedRotation.LastSkillUse.ContainsKey("Observed Cast")&&observedRotation.LastSkillUse.ContainsKey("Observed Strike"),"passive tracker records observed skill use");
True(observedRotation.Buffs.Contains("Observed Buff"),"passive tracker records observed buff");
tracker.Observe(new(t.AddSeconds(4),CombatKind.BuffRemove,77,"Tester",77,"Tester",Effect:"Observed Buff",SourceClass:"Templar"));
True(!tracker.Snapshot().Buffs.Contains("Observed Buff"),"passive tracker removes observed buff");
tracker.Observe(new(t.AddSeconds(5),CombatKind.Zone));
True(tracker.Snapshot().PlayerId==0&&tracker.Snapshot().LastSkillUse.Count==0,"passive tracker clears across zone");
var cooldownTracker=new PassiveRotationStateTracker();
cooldownTracker.Observe(new(t,CombatKind.PlayerName,77,"Tester",SourceClass:"Templar",SourceIdentityConfirmed:true));
cooldownTracker.Observe(new(t,CombatKind.Damage,77,"Tester",99,"Dummy","Punishment",100,SourceClass:"Templar"));
var knownCooldowns=ValidatedCooldownCatalog.Remaining(cooldownTracker.Snapshot(),AionClass.Templar,t.AddSeconds(12));
Equal(knownCooldowns["Punishment"],18,"validated Punishment cooldown reconstruction");
True(!knownCooldowns.ContainsKey("Unknown Skill"),"unknown cooldown is never guessed");
True(ValidatedCooldownCatalog.Entries.All(x=>x.GlobalVersion=="1.0.21.0"),"cooldown evidence pinned to Global version");
var templarProvisional=RotationProfileCatalog.CreateProvisionalTemplarSingleTarget();
True(templarProvisional.Validation==ProfileValidation.Provisional,"Templar fixture remains provisional");
True(templarProvisional.Rules.Single(x=>x.Skill=="Judgment").Conditions.Any(x=>x.Kind==RotationConditionKind.SignalPresent&&x.Key=="JudgmentWindow"),"Judgment requires observed trigger signal");
var templarReady=rotationEngine.Evaluate(new RotationState(t,AionClass.Templar,"global-templar-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>{{"Punishment",0},{"Empyrean Lord's Punishment",12}},new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),templarProvisional);
True(templarReady.Next?.Skill=="Punishment"&&!templarReady.Next.Actionable,"provisional Templar emits informational Punishment when observed ready");
var templarUnknown=rotationEngine.Evaluate(new RotationState(t,AionClass.Templar,"global-templar-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),templarProvisional);
True(templarUnknown.Next is null,"provisional Templar fails closed when cooldown readiness is unknown");
var judgmentTracker=new PassiveRotationStateTracker();
judgmentTracker.Observe(new(t,CombatKind.PlayerName,77,"Tester",SourceClass:"Templar",SourceIdentityConfirmed:true));
judgmentTracker.Observe(new(t.AddSeconds(1),CombatKind.Damage,77,"Tester",99,"Dummy","Shield Smite",100,SourceClass:"Templar"));
var shieldWindow=judgmentTracker.Snapshot();
True(shieldWindow.JudgmentTrigger=="Shield Smite"&&shieldWindow.JudgmentWindowActive(t.AddSeconds(2.9)),"Shield Smite opens observed Judgment window");
True(!shieldWindow.JudgmentWindowActive(t.AddSeconds(3.1)),"Shield Smite Judgment window expires after 2s");
judgmentTracker.Observe(new(t.AddSeconds(4),CombatKind.Cast,77,"Tester",Skill:"Doom Shield",SourceClass:"Templar"));
var doomWindow=judgmentTracker.Snapshot();
True(doomWindow.JudgmentTrigger=="Doom Shield"&&doomWindow.JudgmentWindowActive(t.AddSeconds(6.9)),"Doom Shield opens observed 3s Judgment window");
True(!doomWindow.JudgmentWindowActive(t.AddSeconds(7.1)),"Doom Shield Judgment window expires after 3s");
var wardingTracker=new PassiveRotationStateTracker();
wardingTracker.Observe(new(t,CombatKind.PlayerName,77,"Tester",SourceClass:"Templar",SourceIdentityConfirmed:true));
wardingTracker.Observe(new(t.AddSeconds(1),CombatKind.Damage,77,"Tester",99,"Dummy","Warding Strike",100,SourceClass:"Templar"));
True(wardingTracker.Snapshot().JudgmentWindowActive(t.AddSeconds(2.9)),"Warding Strike opens observed 2s Judgment window");
var rushTracker=new PassiveRotationStateTracker();
rushTracker.Observe(new(t,CombatKind.PlayerName,77,"Tester",SourceClass:"Templar",SourceIdentityConfirmed:true));
rushTracker.Observe(new(t.AddSeconds(1),CombatKind.Damage,77,"Tester",99,"Dummy","Shield Rush",100,SourceClass:"Templar"));
True(rushTracker.Snapshot().JudgmentWindowActive(t.AddSeconds(2.9)),"Shield Rush opens observed 2s Judgment window");
var judgmentState=new RotationState(t,AionClass.Templar,"global-templar-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"JudgmentWindow"}};
var judgmentDecision=rotationEngine.Evaluate(judgmentState,templarProvisional);
True(judgmentDecision.Next?.Skill=="Judgment"&&!judgmentDecision.Next.Actionable,"observed Judgment window yields provisional informational Judgment");
var noJudgmentSignal=rotationEngine.Evaluate(judgmentState with {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase)},templarProvisional);
True(noJudgmentSignal.Next is null,"Judgment remains ineligible without observed trigger signal");
var assassinProvisional=RotationProfileCatalog.CreateProvisionalAssassinSingleTarget();
True(assassinProvisional.Validation==ProfileValidation.Provisional,"Assassin fixture remains provisional");
var assassinUnknown=rotationEngine.Evaluate(new RotationState(t,AionClass.Assassin,"global-assassin-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),assassinProvisional);
True(assassinUnknown.Next is null,"Assassin fails closed without passively proven crit or Insignia state");
var assassinCrit=rotationEngine.Evaluate(new RotationState(t,AionClass.Assassin,"global-assassin-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"CriticalHitWindow"}},assassinProvisional);
True(assassinCrit.Next?.Skill=="Heart Gore"&&!assassinCrit.Next.Actionable,"observed Assassin crit signal yields informational Heart Gore");
var assassinInsignia=rotationEngine.Evaluate(new RotationState(t,AionClass.Assassin,"global-assassin-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"InsigniaReady"}},assassinProvisional);
True(assassinInsignia.Next?.Skill=="Insignia Explosion"&&!assassinInsignia.Next.Actionable,"observed Assassin Insignia signal yields informational explosion");
var assassinTracker=new PassiveRotationStateTracker();
assassinTracker.Observe(new(t,CombatKind.PlayerName,88,"AssassinTester",SourceClass:"Assassin",SourceIdentityConfirmed:true));
assassinTracker.Observe(new(t.AddSeconds(1),CombatKind.Damage,88,"AssassinTester",99,"Dummy","Observed Crit",100,
    SourceClass:"Assassin",DamageFlags:DamageFlags.Critical));
True(assassinTracker.Snapshot().CriticalHitWindowActive(t.AddSeconds(2.9)),"observed Assassin critical opens passive 2s Heart Gore window");
True(!assassinTracker.Snapshot().CriticalHitWindowActive(t.AddSeconds(3.1)),"Assassin critical window expires after 2s");
var identityTracker=new PassiveRotationStateTracker();
identityTracker.Observe(new(t,CombatKind.PlayerName,101,"Local",SourceClass:"Templar",SourceIdentityConfirmed:true));
identityTracker.Observe(new(t.AddMilliseconds(10),CombatKind.PlayerName,202,"Ally",SourceClass:"Sorcerer",SourceIdentityConfirmed:false));
var identitySnapshot=identityTracker.Snapshot();
True(identitySnapshot.PlayerId==101&&identitySnapshot.ClassName==AionClass.Templar,
    "unconfirmed ally identity cannot replace confirmed passive player");
var gladiatorProvisional=RotationProfileCatalog.CreateProvisionalGladiatorSingleTarget();
True(gladiatorProvisional.Validation==ProfileValidation.Provisional,"Gladiator fixture remains provisional");
var gladiatorUnknown=rotationEngine.Evaluate(new RotationState(t,AionClass.Gladiator,"global-gladiator-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),gladiatorProvisional);
True(gladiatorUnknown.Next is null,"Gladiator fails closed without passively proven chain state");
var gladiatorFinisher=rotationEngine.Evaluate(new RotationState(t,AionClass.Gladiator,"global-gladiator-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"GladiatorFinisherWindow"}},gladiatorProvisional);
True(gladiatorFinisher.Next?.Skill=="Seismic Crash"&&!gladiatorFinisher.Next.Actionable,"observed Gladiator finisher signal yields informational Seismic Crash");
var rangerProvisional=RotationProfileCatalog.CreateProvisionalRangerSingleTarget();
True(rangerProvisional.Validation==ProfileValidation.Provisional,"Ranger fixture remains provisional");
var rangerUnknown=rotationEngine.Evaluate(new RotationState(t,AionClass.Ranger,"global-ranger-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),rangerProvisional);
True(rangerUnknown.Next is null,"Ranger fails closed without passively proven proc/state");
var rangerRupture=rotationEngine.Evaluate(new RotationState(t,AionClass.Ranger,"global-ranger-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"RangerRuptureWindow"}},rangerProvisional);
True(rangerRupture.Next?.Skill=="Rupture Arrow"&&!rangerRupture.Next.Actionable,"observed Ranger rupture signal yields informational Rupture Arrow");
var sorcererProvisional=RotationProfileCatalog.CreateProvisionalSorcererSingleTarget();
True(sorcererProvisional.Validation==ProfileValidation.Provisional,"Sorcerer fixture remains provisional");
var sorcererUnknown=rotationEngine.Evaluate(new RotationState(t,AionClass.Sorcerer,"global-sorcerer-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),sorcererProvisional);
True(sorcererUnknown.Next is null,"Sorcerer fails closed without passively proven burst/state");
var sorcererBurst=rotationEngine.Evaluate(new RotationState(t,AionClass.Sorcerer,"global-sorcerer-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SorcererBurstWindow"}},sorcererProvisional);
True(sorcererBurst.Next?.Skill=="Hellfire"&&!sorcererBurst.Next.Actionable,"observed Sorcerer burst signal follows expanded priority with Hellfire first");
var spiritmasterProvisional=RotationProfileCatalog.CreateProvisionalSpiritmasterSingleTarget();
True(spiritmasterProvisional.Validation==ProfileValidation.Provisional,"Spiritmaster fixture remains provisional");
var spiritmasterUnknown=rotationEngine.Evaluate(new RotationState(t,AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),spiritmasterProvisional);
True(spiritmasterUnknown.Next is null,"Spiritmaster fails closed without passively proven target/spirit state");
var spiritmasterBurst=rotationEngine.Evaluate(new RotationState(t,AionClass.Spiritmaster,"global-spiritmaster-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95)
    {Signals=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"SpiritmasterBurstWindow"}},spiritmasterProvisional);
True(spiritmasterBurst.Next?.Skill=="Elemental Fusion"&&!spiritmasterBurst.Next.Actionable,"observed Spiritmaster burst signal follows expanded priority with Elemental Fusion");
var clericProvisional=RotationProfileCatalog.CreateProvisionalClericSingleTarget();
var clericUnknown=rotationEngine.Evaluate(new RotationState(t,AionClass.Cleric,"global-cleric-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),clericProvisional);
True(clericUnknown.Next is null,"Cleric fails closed without passively proven damage/heal state");
var chanterProvisional=RotationProfileCatalog.CreateProvisionalChanterSingleTarget();
var chanterUnknown=rotationEngine.Evaluate(new RotationState(t,AionClass.Chanter,"global-chanter-provisional",RotationMode.SingleTarget,
    new Dictionary<string,double>(),new HashSet<string>(),new HashSet<string>(),0,100,1,false,true,.95),chanterProvisional);
True(chanterUnknown.Next is null,"Chanter fails closed without passively proven damage/heal state");
True(clericProvisional.Validation==ProfileValidation.Provisional&&chanterProvisional.Validation==ProfileValidation.Provisional,
    "Cleric and Chanter fixtures remain provisional");

Console.WriteLine($"PASS: {checks} regression assertions");
