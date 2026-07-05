namespace Celeste.Mod.HooliganHelper.Entities;
[CustomEntity("HooliganHelper/BoughRefill")]
[Tracked]

public class BoughRefill : Entity
{
    private static readonly ParticleType P_BumperShatter = new ParticleType(Refill.P_Shatter)
    {
        Color = Calc.HexToColor("ffffff"),
        Color2 = Calc.HexToColor("dddddd"),
    };

    private static readonly ParticleType P_BumperRegen = new ParticleType(Refill.P_Regen)
    {
        SpeedMin = 40f,
        SpeedMax = 60f,
        ColorMode = ParticleType.ColorModes.Fade,
        Color = Calc.HexToColor("f1ce8d"),
        Color2 = Calc.HexToColor("d89f48")
    };

    private static readonly ParticleType P_BumperGlow = new ParticleType(Refill.P_Glow)
    {
        ColorMode = ParticleType.ColorModes.Fade,
        Color = Calc.HexToColor("f1ce8d"),
        Color2 = Calc.HexToColor("d89f48")
    };
    
    private readonly Image Sprite;
    private Level level;
    private readonly Image Outline;
    private readonly Wiggler wiggler;
    private readonly BloomPoint bloom;
    private readonly VertexLight light;
    private readonly SineWave sine;
    private readonly bool oneUse;
    private float respawnTimer;
    
    public BoughRefill(EntityData data, Vector2 offset)
        :base(data.Position + offset)
    {
        Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(OnPlayer));

        oneUse = data.Bool("oneUse", defaultValue:false);
        
        Add(Outline = new Image(GFX.Game["objects/HooliganHelper/BoughRefill/outline"]));
        Outline.CenterOrigin();
        Outline.Visible = false;
        
        Add(Sprite = new Image(GFX.Game["objects/HooliganHelper/BoughRefill/idle"]));
        Sprite.CenterOrigin();
        
        Add(wiggler = Wiggler.Create(1f, 4f, (v) =>
        {
           Sprite.Scale = (Sprite.Scale = Vector2.One * (1f + v * 0.2f));
        }));
        Add(new MirrorReflection());
        Add(bloom = new BloomPoint(0.3f, 16f));
        Add(light = new VertexLight(Color.White, 1f, 16, 48));
        Add(sine = new SineWave(0.6f, 0f));
        sine.Randomize();
        UpdateY();
        Depth = -100;
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
        if (player.Dashes < 1 || HooliganHelperModule.Session.BoughDashes < 1)
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
        else if (Scene.OnInterval(0.1f))
        {
            level.ParticlesFG.Emit(P_BumperGlow, 1, Position, Vector2.One * 5f);
        }
        UpdateY();
        light.Alpha = Calc.Approach(light.Alpha, Sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
        bloom.Alpha = light.Alpha * 0.3f;
    }
    
    private IEnumerator RefillRoutine(Player player)
    {
        HooliganHelperModule.Session.BoughDashes++;
        Celeste.Freeze(0.05f);
        yield return null;
        level.Shake();
        Sprite.Visible = false;
        Outline.Visible = true;
        if (oneUse)
        {
            Outline.Visible = false;
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
            Depth = -100;
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