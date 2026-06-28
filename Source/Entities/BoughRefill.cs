using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.HooliganHelper.Entities;

[Tracked]

[CustomEntity("HooliganHelper/BoughRefill")]
public class BoughRefill : Entity
{
    public static ParticleType P_BumperShatter = new ParticleType(Refill.P_Shatter)
    {
        Color = Calc.HexToColor("ffffff"),
        Color2 = Calc.HexToColor("dddddd"),
    };

    public static ParticleType P_BumperRegen = new ParticleType(Refill.P_Regen)
    {
        SpeedMin = 40f,
        SpeedMax = 60f,
        ColorMode = ParticleType.ColorModes.Fade,
        Color = Calc.HexToColor("f1ce8d"),
        Color2 = Calc.HexToColor("d89f48")
    };
    
    public static ParticleType P_BumperGlow = new ParticleType(Refill.P_Glow)
    {
        ColorMode = ParticleType.ColorModes.Fade,
        Color = Calc.HexToColor("f1ce8d"),
        Color2 = Calc.HexToColor("d89f48")
    };
    
    private Image Sprite;
    
    private Level level;
    
    private Image Outline;

    public static Vector2 PlayerDelayed;
    
    private Wiggler wiggler;

    private BloomPoint bloom;

    private VertexLight light;

    private SineWave sine;
    
    private bool oneUse;
    
    private float respawnTimer;

    private static int BoughDashes
    {
        get => HooliganHelperModule.Session.BoughDashes; 
        set => HooliganHelperModule.Session.BoughDashes = value;
    }
    private static bool DashingWithBough
    {
        get => HooliganHelperModule.Session.DashingWithBough; 
        set => HooliganHelperModule.Session.DashingWithBough = value;
    }
    
    public BoughRefill(EntityData data, Vector2 offset)
        :base(data.Position + offset)
    {
        base.Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(OnPlayer));

        oneUse = data.Bool("oneUse", defaultValue:false);
        
        Add(Outline = new Image(GFX.Game["objects/HooliganHelper/BoughRefill/outline"]));
        Outline.CenterOrigin();
        Outline.Visible = false;
        
        Add(Sprite = new Image(GFX.Game["objects/HooliganHelper/BoughRefill/idle"]));
        Sprite.CenterOrigin();
        
        Add(wiggler = Wiggler.Create(1f, 4f, (float v) =>
        {
           Sprite.Scale = (Sprite.Scale = Vector2.One * (1f + v * 0.2f));
        }));
        Add(new MirrorReflection());
        Add(bloom = new BloomPoint(0.3f, 16f));
        Add(light = new VertexLight(Color.White, 1f, 16, 48));
        Add(sine = new SineWave(0.6f, 0f));
        sine.Randomize();
        UpdateY();
        base.Depth = -100;
    }

    private void UpdateY()
    {
        Image obj = Sprite;
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
        if (player.Dashes < 1 || BoughDashes < 1)
        {
            Audio.Play("event:/new_content/game/10_farewell/pinkdiamond_touch", Position);
            Audio.Play("event:/char/madeline/footstep", Position, "surface_index", 33);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(RefillRoutine(player)));
            respawnTimer = 2.5f;
            player.RefillDash();
        }
    }

    public static void BoughState(Player player)
    {
        player.StateMachine.State = 13;
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
            level.ParticlesFG.Emit(P_BumperGlow, 1, Position, Vector2.One * 5f);
        }
        UpdateY();
        light.Alpha = Calc.Approach(light.Alpha, Sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
        bloom.Alpha = light.Alpha * 0.3f;
    }
    private IEnumerator RefillRoutine(Player player)
    {
        BoughDashes++;
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
        level.ParticlesFG.Emit(P_BumperShatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
        level.ParticlesFG.Emit(P_BumperShatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse)
        {
            RemoveSelf();
        }
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
            Audio.Play("event:/char/madeline/footstep", Position, "surface_index", 33);
            level.ParticlesFG.Emit(P_BumperRegen, 16, Position, Vector2.One * 2f);
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
