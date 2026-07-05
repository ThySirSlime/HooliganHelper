namespace Celeste.Mod.HooliganHelper.Entities;
[CustomEntity("HooliganHelper/BumperRefill")]
[Tracked]

public class BumperRefill : Entity
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
        ColorMode = ParticleType.ColorModes.Blink,
        Color = Calc.HexToColor("47b5cc"),
        Color2 = Calc.HexToColor("c4f4ff")
    };

    private static readonly ParticleType P_BumperGlow = new ParticleType(Refill.P_Glow)
    {
        ColorMode = ParticleType.ColorModes.Blink,
        Color = Calc.HexToColor("47b5cc"),
        Color2 = Calc.HexToColor("c4f4ff")
    };

    private static readonly ParticleType P_EvilBumperRegen = new ParticleType(Refill.P_Regen)
    {
        SpeedMin = 40f,
        SpeedMax = 60f,
        ColorMode = ParticleType.ColorModes.Blink,
        Color = Calc.HexToColor("ff350f"),
        Color2 = Calc.HexToColor("c21f11")
    };

    private static readonly ParticleType P_EvilBumperGlow = new ParticleType(Refill.P_Glow)
    {
        ColorMode = ParticleType.ColorModes.Blink,
        Color = Calc.HexToColor("ff350f"),
        Color2 = Calc.HexToColor("c21f11")
    };
    
    private readonly float DelayTime;
    private static Session.CoreModes CollectedInMode = Session.CoreModes.None;
    private readonly bool BumperDoesntRefillDash;
    private Sprite Sprite;
    private Sprite flash;
    private Level level;
    private readonly Image Outline;
    private readonly Wiggler wiggler;
    private readonly BloomPoint bloom;
    private readonly VertexLight light;
    private readonly SineWave sine;
    private readonly bool oneUse;
    private float respawnTimer;
    public static bool ShouldSkipDashRefill;
    private readonly bool alwaysHot;
    
    public BumperRefill(EntityData data, Vector2 offset)
        :base(data.Position + offset)
    {
        DelayTime = data.Float("delayTime", defaultValue: 0.1f);
        
        BumperDoesntRefillDash = data.Bool("bumperDoesntRefillDash", defaultValue: false);
        
        oneUse = data.Bool("oneUse", defaultValue:false);
        
        alwaysHot = data.Bool("alwaysHot", defaultValue:false);
        
        Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(OnPlayer));
        
        Add(Outline = new Image(GFX.Game["objects/HooliganHelper/BumperRefill/outline"]));
        Outline.CenterOrigin();
        Outline.Visible = false;
        
        Add(wiggler = Wiggler.Create(1f, 4f, (v) =>
        {
            Sprite.Scale = (Sprite.Scale = Vector2.One * (1f + v * 0.2f));
        }));

        Add(new MirrorReflection());
        Add(bloom = new BloomPoint(0.8f, 16f));
        Add(light = new VertexLight(Color.White, 1f, 16, 48));
        Add(sine = new SineWave(0.6f, 0f));
        sine.Randomize();
        Depth = -100;
        
        Add(new CoreModeListener(OnChangeMode));
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (level is null) return;
        
        Add(Sprite = new Sprite(GFX.Game,"objects/HooliganHelper/BumperRefill/"));
        Sprite.AddLoop("idle", "idle", 0.1f);
        Sprite.AddLoop("evilidle", "evilidle", 0.1f);
        bool coldMode = level.Session.CoreMode is Session.CoreModes.Cold or Session.CoreModes.None;
        if (coldMode && !alwaysHot)
        {
            Sprite.Play("idle");
        }
        else
        {
            Sprite.Play("evilidle");
        }
        Sprite.CenterOrigin();
        
        Add(flash = new Sprite(GFX.Game, "objects/HooliganHelper/BumperRefill/"));
        flash.Add("flash", "flash", 0.1f);
        flash.Add("evilflash", "evilflash", 0.1f);
        flash.OnFinish = delegate
        {
            flash.Visible = false;
        };
        flash.CenterOrigin();
    }

    private void OnChangeMode(Session.CoreModes coreMode)
    {
        bool coldMode = coreMode is Session.CoreModes.Cold or Session.CoreModes.None;
        if (coldMode && !alwaysHot)
        {
            Sprite.Play("idle");
        }
        else
        {
            Sprite.Play("evilidle");
        }
        Logger.Info($"teehee",$"{coreMode}");
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
        if (player.Dashes < 1 || HooliganHelperModule.Session.BumperDashes < 1)
        {
            Audio.Play("event:/new_content/game/10_farewell/pinkdiamond_touch", Position);
            Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(RefillRoutine(player)));
            respawnTimer = 2.5f;
            player.RefillDash();
            HooliganHelperModule.Session.LastBumperCollected = this;
        }
    }
    
    public IEnumerator BumperRefillLaunch(Player player)
    {
        if ((CollectedInMode == Session.CoreModes.Hot && level.CoreMode == Session.CoreModes.Hot) || alwaysHot)
        {
            player.Die(Vector2.Zero);
            Audio.Play("event:/game/09_core/hotpinball_activate", Position);
        }
        yield return DelayTime ;
        ShouldSkipDashRefill = BumperDoesntRefillDash;
        Vector2 dir = player.ExplodeLaunch(player.Center - player.lastAim, false);
        if (Calc.Random.NextSingle() < 0.000001)
        {
            Audio.Play("event:/HooliganHelper/crazythrain", player.Position);
        }
        if ((level.CoreMode == Session.CoreModes.Hot) || alwaysHot)
        {
            Audio.Play("event:/game/09_core/hotpinball_activate", Position);
        }
        else
        {
            Audio.Play("event:/game/06_reflection/pinballbumper_hit", player.Position);
        }
        
        SceneAs<Level>()?.Particles.Emit(Bumper.P_Launch, 12, player.Center - dir * 3f, Vector2.One * 3f, dir.Angle());
        ShouldSkipDashRefill = false;
    }
    
    public override void Update()
    {
        base.Update();
        if (level is null) return;

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
            level.ParticlesFG.Emit(((level.CoreMode == Session.CoreModes.Hot) || alwaysHot) ? P_EvilBumperGlow : P_BumperGlow, 1, Position, Vector2.One * 5f);
            
        }
        UpdateY();
        light.Alpha = Calc.Approach(light.Alpha, Sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
        bloom.Alpha = light.Alpha * 0.8f;
        if (Scene.OnInterval(2f) && Sprite.Visible)
        {
            if ((level.coreMode is Session.CoreModes.Cold  or Session.CoreModes.None) && !alwaysHot)
            {
                flash.Play("flash", restart: true);
            }
            else
            {
                flash.Play("evilflash", restart: true);
            }
            flash.Visible = true;
        }
    }
    
    private IEnumerator RefillRoutine(Player player)
    {
        CollectedInMode = level.CoreMode;
        Logger.Info($"teehee",$"{level.CoreMode}");
        HooliganHelperModule.Session.BumperDashes++;
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
            level.ParticlesFG.Emit(((level.CoreMode == Session.CoreModes.Hot) || alwaysHot) ? P_EvilBumperRegen : P_BumperRegen, 16, Position, Vector2.One * 2f);
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