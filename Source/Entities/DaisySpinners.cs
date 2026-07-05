namespace Celeste.Mod.HooliganHelper.Entities;

public class DaisySpinners : Entity
{
    public DaisySpinners(Vector2 offset, string spinnerSprite) 
        : base(offset)
    {
        Image outline;
        Add(outline = new Image(GFX.Game[spinnerSprite]));
        outline.Rotation = Calc.Random.Next(4) * Single.Pi / 2;
        outline.CenterOrigin();
    }

    private IEnumerator AddHitbox()
    {
        yield return .1f;
        Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
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