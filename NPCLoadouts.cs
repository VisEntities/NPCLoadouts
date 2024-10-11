using Facepunch;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * Rewritten from scratch and maintained to present by VisEntities
 * Originally created by Orange, up to version 1.0.3
 */

namespace Oxide.Plugins
{
    [Info("NPC Loadouts", "VisEntities", "2.0.1")]
    [Description("Equip npcs with custom loadouts.")]
    public class NPCLoadouts : RustPlugin
    {
        #region Fields

        private static NPCLoadouts _plugin;
        private static Configuration _config;
        private Coroutine _coroutine;

        #endregion Fields

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Version")]
            public string Version { get; set; }

            [JsonProperty("NPC Groups")]
            public List<NPCGroupConfig> NPCGroups { get; set; }

            public void BuildLoadouts()
            {
                foreach (NPCGroupConfig npcGroup in NPCGroups)
                {
                    if (npcGroup.Enabled)
                        npcGroup.Loadout.BuildLoadout();
                }
            }
        }

        private class NPCGroupConfig
        {
            [JsonProperty("Enabled")]
            public bool Enabled { get; set; }

            [JsonProperty("NPC Short Prefab Names")]
            public string[] NPCShortPrefabNames { get; set; }

            [JsonProperty("Loadout")]
            public LoadoutConfig Loadout { get; set; }
        }

        private class LoadoutConfig
        {
            [JsonProperty("Randomize Active Weapon")]
            public bool RandomizeActiveWeapon { get; set; }

            [JsonProperty("Belt")]
            public List<ItemInfo> Belt { get; set; }

            [JsonProperty("Wear")]
            public List<ItemInfo> Wear { get; set; }

            [JsonIgnore]
            public PlayerInventoryProperties InventoryProperties { get; set; }

            public void BuildLoadout()
            {
                InventoryProperties = ScriptableObject.CreateInstance<PlayerInventoryProperties>();
                InventoryProperties.wear = new List<PlayerInventoryProperties.ItemAmountSkinned>();
                InventoryProperties.main = new List<PlayerInventoryProperties.ItemAmountSkinned>();
                InventoryProperties.belt = new List<PlayerInventoryProperties.ItemAmountSkinned>();

                foreach (ItemInfo itemInfo in Wear)
                {
                    ItemDefinition itemDef = ItemManager.FindItemDefinition(itemInfo.Shortname);
                    if (itemDef != null)
                    {
                        InventoryProperties.wear.Add(new PlayerInventoryProperties.ItemAmountSkinned
                        {
                            itemDef = itemDef,
                            amount = itemInfo.Amount,
                            skinOverride = itemInfo.SkinId
                        });
                    }
                }

                foreach (ItemInfo itemInfo in Belt)
                {
                    ItemDefinition itemDef = ItemManager.FindItemDefinition(itemInfo.Shortname);
                    if (itemDef != null)
                    {
                        InventoryProperties.belt.Add(new PlayerInventoryProperties.ItemAmountSkinned
                        {
                            itemDef = itemDef,
                            amount = itemInfo.Amount,
                            skinOverride = itemInfo.SkinId
                        });
                    }
                }
            }
        }

        public class ItemInfo
        {
            [JsonProperty("Shortname")]
            public string Shortname { get; set; }

            [JsonProperty("Skin Id")]
            public ulong SkinId { get; set; }

            [JsonProperty("Amount")]
            public int Amount { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<Configuration>();

            if (string.Compare(_config.Version, Version.ToString()) < 0)
                UpdateConfig();

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            _config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config, true);
        }

        private void UpdateConfig()
        {
            PrintWarning("Config changes detected! Updating...");

            Configuration defaultConfig = GetDefaultConfig();

            if (string.Compare(_config.Version, "1.0.0") < 0)
                _config = defaultConfig;

            PrintWarning("Config update complete! Updated from version " + _config.Version + " to " + Version.ToString());
            _config.Version = Version.ToString();
        }

        private Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                Version = Version.ToString(),
                NPCGroups = new List<NPCGroupConfig>
                {
                    new NPCGroupConfig
                    {
                        Enabled = false,
                        NPCShortPrefabNames = new string[]
                        {
                            "scientistnpc_oilrig",
                        },
                        Loadout = new LoadoutConfig
                        {
                            RandomizeActiveWeapon = true,
                            Belt = new List<ItemInfo>
                            {
                                new ItemInfo
                                {
                                    Shortname = "shotgun.spas12",
                                    SkinId = 0,
                                    Amount = 1
                                },
                                new ItemInfo
                                {
                                    Shortname = "syringe.medical",
                                    SkinId = 0,
                                    Amount = 2
                                }
                            },
                           Wear = new List<ItemInfo>
                            {
                                new ItemInfo
                                {
                                    Shortname = "halloween.mummysuit",
                                    SkinId = 0,
                                    Amount = 1
                                }
                            }
                        }
                    },
                    new NPCGroupConfig
                    {
                        Enabled = false,
                        NPCShortPrefabNames = new string[]
                        {
                            "scientistnpc_cargo",
                            "scientistnpc_cargo_turret_lr300"
                        },
                        Loadout = new LoadoutConfig
                        {
                            RandomizeActiveWeapon = true,
                            Belt = new List<ItemInfo>
                            {
                                new ItemInfo
                                {
                                    Shortname = "rifle.ak",
                                    SkinId = 0,
                                    Amount = 1
                                },
                                new ItemInfo
                                {
                                    Shortname = "syringe.medical",
                                    SkinId = 0,
                                    Amount = 2
                                }
                            },
                           Wear = new List<ItemInfo>
                            {
                                new ItemInfo
                                {
                                    Shortname = "gingerbreadsuit",
                                    SkinId = 0,
                                    Amount = 1
                                }
                            }
                        }
                    }
                }
            };
        }

        #endregion Configuration

        #region Coroutine

        private void StartCoroutine()
        {
            _coroutine = ServerMgr.Instance.StartCoroutine(EquipLoadoutForAll());
        }

        private void StopCoroutine()
        {
            if (_coroutine != null)
            {
                ServerMgr.Instance.StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }

        #endregion Coroutine

        #region Oxide Hooks

        private void Init()
        {
            _plugin = this;
            _config.BuildLoadouts();
        }

        private void Unload()
        {
            StopCoroutine();

            _config = null;
            _plugin = null;
        }

        private void OnServerInitialized()
        {
            StartCoroutine();
        }

        private void OnEntitySpawned(NPCPlayer npc)
        {
            if (npc != null && !npc.userID.IsSteamId())
            {
                foreach (NPCGroupConfig npcGroup in _config.NPCGroups)
                {
                    if (npcGroup.Enabled)
                    {
                        if (npcGroup.NPCShortPrefabNames.Contains(npc.ShortPrefabName))
                        {
                            EquipLoadout(npc, npcGroup.Loadout);
                            break;
                        }
                    }
                }
            }
        }

        #endregion Oxide Hooks

        #region Functions

        private IEnumerator EquipLoadoutForAll()
        {
            foreach (NPCPlayer npc in BaseNetworkable.serverEntities.OfType<NPCPlayer>())
            {
                if (npc != null && !npc.userID.IsSteamId())
                {
                    foreach (NPCGroupConfig npcGroup in _config.NPCGroups)
                    {
                        if (npcGroup.Enabled)
                        {
                            if (npcGroup.NPCShortPrefabNames.Contains(npc.ShortPrefabName))
                            {
                                EquipLoadout(npc, npcGroup.Loadout);
                                break;
                            }
                        }
                    }
                }

                yield return CoroutineEx.waitForSeconds(0.1f);
            }
        }

        private void EquipLoadout(NPCPlayer npc, LoadoutConfig loadout)
        {
            if (loadout.InventoryProperties != null)
            {
                StripInventory(npc);

                if (loadout.RandomizeActiveWeapon)
                {
                    var nonSyringeItems = loadout.InventoryProperties.belt.Where(item => !item.itemDef.shortname.Equals("syringe.medical", StringComparison.OrdinalIgnoreCase)).ToList();
                    if (nonSyringeItems.Count > 0)
                    {
                        int randomIndex = Random.Range(0, nonSyringeItems.Count);

                        var selectedWeapon = nonSyringeItems[randomIndex];
                        loadout.InventoryProperties.belt.Remove(selectedWeapon);
                        loadout.InventoryProperties.belt.Insert(0, selectedWeapon);
                    }
                }

                loadout.InventoryProperties.GiveToPlayer(npc);
                npc.EquipWeapon();
            }
        }

        public static void StripInventory(BasePlayer player)
        {
            List<Item> allItems = Pool.Get<List<Item>>();
            player.inventory.GetAllItems(allItems);

            for (int i = allItems.Count - 1; i >= 0; i--)
            {
                Item item = allItems[i];
                item.RemoveFromContainer();
                item.Remove();
            }

            Pool.FreeUnmanaged(ref allItems);
        }

        #endregion Functions
    }
}