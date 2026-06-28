using Celeste.Mod.Entities;
using Celeste.Pico8;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.HooliganHelper.Triggers;

[CustomEntity("HooliganHelper/StaminaSetTriggerForRemixAndNothingElse")]
public class StaminaSetTriggerForRemixAndNothingElse : Trigger
{
    
    private int StaminaValue;

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