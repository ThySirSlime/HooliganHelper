using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.HooliganHelper.Entities;

[CustomEntity("HooliganHelper/RandomStateRefill")]
public class RandomStateRefill : Entity
{
    private static readonly int[] MStates = [5, 9, 14, 19];

    public static ParticleType P_MetamorphosisShatter = new ParticleType(Refill.P_Shatter)
    {
        Color = Calc.HexToColor("ffffff"),
        Color2 = Calc.HexToColor("dddddd"),
    };

    public static ParticleType P_MetamorphosisRegen = new ParticleType(Refill.P_Regen)
    {
        SpeedMin = 40f,
        SpeedMax = 60f,
        Color = Calc.HexToColor("70cdff"),
        Color2 = Calc.HexToColor("ff66b6")
    };
    
    public static ParticleType P_MetamorphosisGlow = new ParticleType(Refill.P_Glow)
    {
        Color = Calc.HexToColor("70cdff"),
        Color2 = Calc.HexToColor("ff66b6")
    };
    
    private Sprite Sprite;
    
    private Sprite PSprite;

    private Level level;
    
    private Image Outline;
    
    private Wiggler wiggler;

    private BloomPoint bloom;

    private VertexLight light;

    private SineWave sine;
    
    private bool oneUse;
    
    public static int state;
    
    private float respawnTimer;

    private static int MetamorphosisDashes
    {
        get => HooliganHelperModule.Session.MetamorphosisDashes; 
        set => HooliganHelperModule.Session.MetamorphosisDashes = value;
    }
    private static bool DashingWithMetamorphosis
    {
        get => HooliganHelperModule.Session.DashingWithMetamorphosis; 
        set => HooliganHelperModule.Session.DashingWithMetamorphosis = value;
    }
    
    public RandomStateRefill(EntityData data, Vector2 offset)
    :base(data.Position + offset)
    {
        base.Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(OnPlayer));
        
        oneUse = data.Bool("oneUse", defaultValue:false);
        
        Add(Outline = new Image(GFX.Game["objects/HooliganHelper/MetamorphosisRefill/outline"]));
        Outline.CenterOrigin();
        Outline.Visible = false;
        
        Add(Sprite = new Sprite(GFX.Game,"objects/HooliganHelper/MetamorphosisRefill/"));
        Sprite.AddLoop("idle", "butterflyrefill", 0.1f);
        Sprite.Play("idle");
        Sprite.CenterOrigin();
        
        Add(PSprite = new Sprite(GFX.Game,"objects/HooliganHelper/MetamorphosisRefill/"));
        PSprite.Add("redbooster", "absorb_booster", .066f);
        PSprite.Add("dreamblock", "absorb_dream", .066f);
        PSprite.Add("respawn", "absorb_respawn", .066f);
        PSprite.Add("feather", "absorb_feather", .066f);
        PSprite.CenterOrigin();
        
        Add(wiggler = Wiggler.Create(1f, 4f, (float v) =>
        {
           Sprite.Scale = (Sprite.Scale = Vector2.One * (1f + v * 0.2f));
        }));
        Add(new MirrorReflection());
        Add(bloom = new BloomPoint(0.8f, 16f));
        Add(light = new VertexLight(Color.White, 1f, 16, 48));
        Add(sine = new SineWave(0.6f, 0f));
        sine.Randomize();
        UpdateY();
        base.Depth = -100;
    }
    

    private void UpdateY()
    {
        Sprite obj = Sprite;
        float num = (bloom.Y = sine.Value * 2f);
        float y = (obj.Y = num);
        obj.Y = y;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = scene as Level;
    }
    private void OnPlayer(Player player)
    {
        state=Calc.Random.Choose(MStates);
        Audio.Play("event:/new_content/game/10_farewell/pinkdiamond_touch", Position);
        if (state == 5)
        {
            Audio.Play("event:/game/05_mirror_temple/redbooster_enter", Position);
            PSprite.Play("redbooster", true, false);
        }
        if (state == 9)
        {
            Audio.Play("event:/char/madeline/dreamblock_enter", Position);
            PSprite.Play("dreamblock", true, false);
        }
        if (state == 14)
        {
            Audio.Play("event:/game/04_cliffside/snowball_impact", Position);
            PSprite.Play("respawn", true, false);
        }
        if (state == 19)
        {
            Audio.Play("event:/game/06_reflection/feather_renew", Position);
            PSprite.Play("feather", true, false);
        }
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        Collidable = false;
        Add(new Coroutine(RefillRoutine(player)));
        respawnTimer = 2.5f;
    }
    
    
    public override void Update()
    {
        base.Update();
        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                Respawn();
            }
        }
        else if (base.Scene.OnInterval(0.1f))
        {
            level.ParticlesFG.Emit(P_MetamorphosisGlow, 1, Position, Vector2.One * 5f);
        }
        UpdateY();
        light.Alpha = Calc.Approach(light.Alpha, Sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
        bloom.Alpha = light.Alpha * 0.8f;
        
        // Player player = Scene.Tracker.GetEntity<Player>();
        // if (player != null)
        // {
        //     PSprite.Position = player.Position;
        // }

    }
    private IEnumerator RefillRoutine(Player player)
    {
        MetamorphosisDashes++;
        global::Celeste.Celeste.Freeze(0.05f);
        yield return null;
        level.Shake();
        Sprite.Visible = false;
        Outline.Visible = true;
        if (!oneUse)
        {
            Outline.Visible = true;
        }
        Depth = 8999;
        yield return 0.05f;
        float num = player.Speed.Angle();
        level.ParticlesFG.Emit(P_MetamorphosisShatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
        level.ParticlesFG.Emit(P_MetamorphosisShatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse)
        {
            RemoveSelf();
        }
    }

    public static void ChangePlayerState(Player player)
    {
        player.StateMachine.State = state;
    }
    private void Respawn()
    {
        if (!Collidable)
        {
            Collidable = true;
            Sprite.Visible = true;
            Outline.Visible = false;
            base.Depth = -100;
            wiggler.Start();
            Audio.Play("event:/new_content/game/10_farewell/pinkdiamond_return", Position);
            level.ParticlesFG.Emit(P_MetamorphosisRegen, 16, Position, Vector2.One * 2f);
        }
    }
    public override void Render()
    {
        if (Sprite.Visible)
        {
            Sprite.DrawOutline();
        }
        base.Render();
    }

}