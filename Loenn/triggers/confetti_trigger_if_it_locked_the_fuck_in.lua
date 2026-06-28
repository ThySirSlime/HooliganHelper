local trigger = {}

trigger.name = "HooliganHelper/ConfettiTriggerIfItLockedTFIn"
trigger.placements = {
    name = "confetti_trigger_if_it_locked_the_fuck_in",
    data = {
        width = 16,
        height = 16,
	eventName = "event:/game/07_summit/checkpoint_confetti",
	confettiState = 0
    }
}

return trigger