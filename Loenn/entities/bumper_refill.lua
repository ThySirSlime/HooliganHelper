local entity = {}

entity.name = "HooliganHelper/BumperRefill"
entity.justification = {0.5, 0.5}

entity.placements = {
    {
        name = "bumper_refill",
 	data = {
		delayTime = 0.1,
		bumperDoesntRefillDash = false,
		oneUse = false,
		alwaysHot = false
	}
    }
}

entity.texture = "objects/HooliganHelper/BumperRefill/idle00"

return entity