using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.HooliganHelper.Entities;

public class DaisySpinners : Entity
{
    private Image Outline;
    
    
    public DaisySpinners(Vector2 offset, string spinnerSprite) 
        : base(offset)
    {
        Add(Outline = new Image(GFX.Game[spinnerSprite]));
        Outline.Rotation = Calc.Random.Next(4) * Single.Pi / 2;
        Outline.CenterOrigin();
    }

    private IEnumerator AddHitbox()
    {
        yield return .1f;
        base.Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
        Add(new PlayerCollider(OnPlayer));
    }

    public override void Added(Scene scene)
    {
        base.Added(scene); 
        Add(new Coroutine(AddHitbox()));
    }
    
    private void OnPlayer(Player player)
    {
        player.Die((player.Position - Position).SafeNormalize());
    }
    
}
