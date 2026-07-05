using Celeste.Mod.HooliganHelper.Entities;

namespace Celeste.Mod.HooliganHelper.Extras;

public class RefillHandling
{
    [OnLoad]
    internal static void Load()
    {
        On.Celeste.Player.DashEnd += PlayerOnDashEnd;
        On.Celeste.Player.DashBegin += PlayerOnDashBegin;
        On.Celeste.Player.Die += PlayerDiedLmao;
        On.Celeste.Level.End += LevelOnEnd;
        On.Celeste.Player.Added += PlayerRespawns;
        IL.Celeste.Player.ExplodeLaunch_Vector2_bool_bool += PlayerOnExplodeLaunch_Vector2_bool_bool;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.Player.DashEnd -= PlayerOnDashEnd;
        On.Celeste.Player.DashBegin -= PlayerOnDashBegin;
        On.Celeste.Player.Die -= PlayerDiedLmao;
        On.Celeste.Level.End -= LevelOnEnd;
        On.Celeste.Player.Added -= PlayerRespawns;
        IL.Celeste.Player.ExplodeLaunch_Vector2_bool_bool -= PlayerOnExplodeLaunch_Vector2_bool_bool;
    }
    
    private static void PlayerRespawns(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
    {
        HooliganHelperModule.Session.MetamorphosisDashes = 0;
        HooliganHelperModule.Session.BumperDashes = 0;
        HooliganHelperModule.Session.DaisyDashes = 0;
        HooliganHelperModule.Session.KnifeDashes = 0;
        HooliganHelperModule.Session.BoughDashes = 0;
        orig(self,scene);
    }

    private static void LevelOnEnd(On.Celeste.Level.orig_End orig, Level self)
    {
        orig(self);
        HooliganHelperModule.Session.MetamorphosisDashes = 0;
        HooliganHelperModule.Session.BumperDashes = 0;
        HooliganHelperModule.Session.DaisyDashes = 0;
        HooliganHelperModule.Session.KnifeDashes = 0;
    }

    private static void PlayerOnExplodeLaunch_Vector2_bool_bool(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        cursor.TryGotoNext(MoveType.Before, instruction => instruction.OpCode == OpCodes.Brtrue_S, instruction => instruction.MatchLdarg0(),
            instruction => instruction.MatchCallvirt<Player>("RefillDash"));

        cursor.EmitLdsfld(typeof(BumperRefill).GetField("ShouldSkipDashRefill")!);
        cursor.EmitOr();
    }

    private static PlayerDeadBody PlayerDiedLmao(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        HooliganHelperModule.Session.MetamorphosisDashes = 0;
        HooliganHelperModule.Session.BumperDashes = 0;
        HooliganHelperModule.Session.DaisyDashes = 0;
        HooliganHelperModule.Session.BoughDashes = 0;
        HooliganHelperModule.Session.KnifeDashes = 0;
        return orig(self, direction, evenIfInvincible, registerDeathInStats);
    }

    private static void PlayerOnDashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);
        if (HooliganHelperModule.Session.MetamorphosisDashes > 0)
        {
            HooliganHelperModule.Session.DashingWithMetamorphosis = true;
        }

        if (HooliganHelperModule.Session.BumperDashes > 0)
        {
            HooliganHelperModule.Session.DashingWithBumper = true;
        }
        
        if (HooliganHelperModule.Session.DaisyDashes > 0)
        {
            self.Add(new Coroutine(DaisyRefill.DaisySpinnerPlacement(self)));
            HooliganHelperModule.Session.DaisyDashes --;
        }
        
        if (HooliganHelperModule.Session.KnifeDashes > 0)
        {
            self.Add(new Coroutine(KnifeRefill.KnifeSpinnerPlacement(self)));
            HooliganHelperModule.Session.KnifeDashes --;
        }
        
        if (HooliganHelperModule.Session.BoughDashes > 0)
        {
            HooliganHelperModule.Session.DashingWithBough = true;
        }
    }
    

    private static void PlayerOnDashEnd(On.Celeste.Player.orig_DashEnd orig, Player self)
    {
        orig(self);
        if (HooliganHelperModule.Session.MetamorphosisDashes > 0 && HooliganHelperModule.Session.DashingWithMetamorphosis)
        {
            RandomStateRefill.ChangePlayerState(self);
            HooliganHelperModule.Session.MetamorphosisDashes = 0;
        }

        HooliganHelperModule.Session.DashingWithMetamorphosis = false;

        if (HooliganHelperModule.Session.BumperDashes > 0 && HooliganHelperModule.Session.DashingWithBumper)
        {
            if (HooliganHelperModule.Session.LastBumperCollected != null)
            {
                self.Add(new Coroutine(HooliganHelperModule.Session.LastBumperCollected.BumperRefillLaunch(self)));
                HooliganHelperModule.Session.BumperDashes --;
            }
        }
        HooliganHelperModule.Session.DashingWithBumper = false;
        
        if (HooliganHelperModule.Session.BoughDashes > 0 && HooliganHelperModule.Session.DashingWithBough)
        {
            BoughRefill.BoughState(self);
            HooliganHelperModule.Session.BoughDashes = 0;
        }

        HooliganHelperModule.Session.DashingWithBough = false;
    }
}