This plugin lets you equip all types of npcs on the server with custom loadouts, which include items for both belt and wear containers, replacing their original loadouts.

If you want an npc to equip a specific item as their weapon, place that item first in the belt list. For npcs capable of using healing items (as determined by the npc type or npcs spawned by plugins), include medical syringes in their belt inventory.

[Demonstration](https://youtu.be/5cnqI65NRjk)

------------

## Configuration

```json
{
  "Version": "2.0.0",
  "NPC Groups": [
    {
      "Enabled": true,
      "NPC Short Prefab Names": [
        "scientistnpc_oilrig"
      ],
      "Loadout": {
        "Randomize Active Weapon": true,
        "Belt": [
          {
            "Shortname": "shotgun.spas12",
            "Skin Id": 0,
            "Amount": 1
          },
          {
            "Shortname": "syringe.medical",
            "Skin Id": 0,
            "Amount": 2
          }
        ],
        "Wear": [
          {
            "Shortname": "halloween.mummysuit",
            "Skin Id": 0,
            "Amount": 1
          }
        ]
      }
    },
    {
      "Enabled": true,
      "NPC Short Prefab Names": [
        "scientistnpc_cargo",
        "scientistnpc_cargo_turret_lr300"
      ],
      "Loadout": {
        "Randomize Active Weapon": true,
        "Belt": [
          {
            "Shortname": "rifle.ak",
            "Skin Id": 0,
            "Amount": 1
          },
          {
            "Shortname": "syringe.medical",
            "Skin Id": 0,
            "Amount": 2
          }
        ],
        "Wear": [
          {
            "Shortname": "gingerbreadsuit",
            "Skin Id": 0,
            "Amount": 1
          }
        ]
      }
    }
  ]
}
```

## Credits
 * Rewritten from scratch and maintained to present by **VisEntities**
 * Originally created by **Orange**, up to version 1.0.3
