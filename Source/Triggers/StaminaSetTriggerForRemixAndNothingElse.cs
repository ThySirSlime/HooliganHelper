namespace Celeste.Mod.HooliganHelper.Triggers;

[CustomEntity("HooliganHelper/StaminaSetTriggerForRemixAndNothingElse")]
public class StaminaSetTriggerForRemixAndNothingElse : Trigger
{
    private readonly int StaminaValue;

    public StaminaSetTriggerForRemixAndNothingElse(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        StaminaValue = data.Int("staminaValue", defaultValue: 200);
    }
    
    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        player.Stamina = StaminaValue;
    }
}