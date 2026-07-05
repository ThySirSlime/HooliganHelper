namespace Celeste.Mod.HooliganHelper.Triggers;
[CustomEntity("HooliganHelper/ConfettiTriggerIfItLockedTFIn")]

public class ConfettiTriggerIfItLockedTFIn : Trigger
{
    private readonly string EventName;
    private readonly int ConfettiState; 

    public ConfettiTriggerIfItLockedTFIn(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        EventName = data.Attr("eventName", defaultValue: "event:/game/07_summit/checkpoint_confetti");
        ConfettiState = data.Int("confettiState", defaultValue: 0);
    }
    
    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        Level level = Scene as Level;
        
        level?.Add(new SummitCheckpoint.ConfettiRenderer(player.Position));
        Audio.Play(EventName, player.Position);
        player.StateMachine.State = ConfettiState;
    }
}