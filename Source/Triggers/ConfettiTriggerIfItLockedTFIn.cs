using Celeste.Mod.Entities;
using Celeste.Pico8;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.HooliganHelper.Triggers;

// ReSharper disable once InconsistentNaming
[CustomEntity("HooliganHelper/ConfettiTriggerIfItLockedTFIn")]
public class ConfettiTriggerIfItLockedTFIn : Trigger
{
    private string EventName;
    private int ConfettiState; 

    public ConfettiTriggerIfItLockedTFIn(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        EventName = data.Attr("eventName", defaultValue: "event:/game/07_summit/checkpoint_confetti");
        ConfettiState = data.Int("confettiState", defaultValue: 0);
    }
    
    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        
        Level level = base.Scene as Level;
        
        level.Add(new SummitCheckpoint.ConfettiRenderer(player.Position));
        Audio.Play(EventName, player.Position);
        player.StateMachine.State = ConfettiState;
    }
}