using System;
using Celeste.Mod.HooliganHelper.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.HooliganHelper;

public class HooliganHelperModule : EverestModule {
    public static HooliganHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(HooliganHelperModuleSettings);
    public static HooliganHelperModuleSettings Settings => (HooliganHelperModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(HooliganHelperModuleSession);
    public static HooliganHelperModuleSession Session => (HooliganHelperModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(HooliganHelperModuleSaveData);
    public static HooliganHelperModuleSaveData SaveData => (HooliganHelperModuleSaveData) Instance._SaveData;

    
    public HooliganHelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(HooliganHelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(HooliganHelperModule), LogLevel.Info);
#endif
    }

    public override void Load() {
        On.Celeste.Player.DashEnd += PlayerOnDashEnd;
        On.Celeste.Player.DashBegin += PlayerOnDashBegin;
        On.Celeste.Player.Die += PlayerDiedLmao;
        On.Celeste.Level.End += LevelOnEnd;
        IL.Celeste.Player.ExplodeLaunch_Vector2_bool_bool += PlayerOnExplodeLaunch_Vector2_bool_bool;
    }

    private static void LevelOnEnd(On.Celeste.Level.orig_End orig, Level self)
    {
        orig(self);
        Session.MetamorphosisDashes = 0;
        Session.BumperDashes = 0;
        Session.DaisyDashes = 0;
    }

    private static void PlayerOnExplodeLaunch_Vector2_bool_bool(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        cursor.TryGotoNext(MoveType.Before, instruction => instruction.OpCode == OpCodes.Brtrue_S, instruction => instruction.MatchLdarg0(),
            instruction => instruction.MatchCallvirt<Player>("RefillDash"));

        cursor.EmitLdsfld(typeof(BumperRefill).GetField("ShouldSkipDashRefill"));
        cursor.EmitOr();

    }

    private static PlayerDeadBody PlayerDiedLmao(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        Session.MetamorphosisDashes = 0;
        Session.BumperDashes = 0;
        Session.DaisyDashes = 0;
        Session.BoughDashes = 0;
        return orig(self, direction, evenIfInvincible, registerDeathInStats);
    }

    private static void PlayerOnDashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);
        if (Session.MetamorphosisDashes > 0)
        {
            Session.DashingWithMetamorphosis = true;
        }

        if (Session.BumperDashes > 0)
        {
            Session.DashingWithBumper = true;
        }
        
        if (Session.DaisyDashes > 0)
        {
            self.Add(new Coroutine(DaisyRefill.DaisySpinnerPlacement(self)));
            Session.DaisyDashes --;
        }
        
        if (Session.BoughDashes > 0)
        {
            Session.DashingWithBough = true;
            
        }
        
    }
    

    private static void PlayerOnDashEnd(On.Celeste.Player.orig_DashEnd orig, Player self)
    {
        orig(self);
        if (Session.MetamorphosisDashes > 0 && Session.DashingWithMetamorphosis)
        {
            RandomStateRefill.ChangePlayerState(self);
            Session.MetamorphosisDashes = 0;
        }

        Session.DashingWithMetamorphosis = false;

        if (Session.BumperDashes > 0 && Session.DashingWithBumper)
        {
            if (Session.LastBumperCollected != null)
            {
                self.Add(new Coroutine(Session.LastBumperCollected.BumperRefillLaunch(self)));
                Session.BumperDashes --;
            }
        }
        Session.DashingWithBumper = false;
        
        if (Session.BoughDashes > 0 && Session.DashingWithBough)
        {
            BoughRefill.BoughState(self);
            Session.BoughDashes = 0;
        }

        Session.DashingWithBough = false;
        
    }


    public override void Unload() {
        On.Celeste.Player.DashEnd -= PlayerOnDashEnd;
        On.Celeste.Player.DashBegin -= PlayerOnDashBegin;
        On.Celeste.Player.Die -= PlayerDiedLmao;
        On.Celeste.Level.End -= LevelOnEnd;
        IL.Celeste.Player.ExplodeLaunch_Vector2_bool_bool -= PlayerOnExplodeLaunch_Vector2_bool_bool;
    }
}