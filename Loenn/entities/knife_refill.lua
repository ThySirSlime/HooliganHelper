local entity = {}

entity.name = "HooliganHelper/KnifeRefill"
entity.justification = {0.5, 0.5}

entity.placements = {
    {
        name = "knife_refill",
 	data = {
		oneUse = false,
		refillSprite = "objects/HooliganHelper/KnifeRefill/kniferefill",
        	outlineSprite = "objects/HooliganHelper/KnifeRefill/knifeoutline",
        	spinnerSprite = "objects/HooliganHelper/KnifeRefill/knifespinner"
	}
    }
}

entity.texture = "objects/HooliganHelper/KnifeRefill/kniferefill"

return entity