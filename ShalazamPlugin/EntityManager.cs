using Il2Cpp;
using Il2CppPantheonPersist;
using MelonLoader;

namespace ShalazamPlugin;

public static class EntityManager
{
    private static readonly string[] _blacklist = { "Banner of Arms", "Banner of Onslaught", "Challenger's Banner", "Rallying Banner", "Shieldman's Banner", "ghostly riddler" };
    private static string? _apiKey;

    private static readonly List<EntityPlayerGameObject> _players = new();
    private static readonly List<NetworkWorldItem> _worldItems = new();
    private static readonly List<EntityNpcGameObject> _monsters = new();
    private static readonly List<EntityNpcGameObject> _friendlyNpcs = new();
    private static readonly List<NetworkWorldItem> _craftingStations = new();

    // TODO: Remove once all functions are using the new client
    public static void SetApiKey(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            MelonLogger.Error("You need to set your configuration key in the config file");
        }

        _apiKey = apiKey;
    }

    public static void OnPlayerAdded(EntityPlayerGameObject entityPlayerGameObject)
    {
        if (_players.Contains(entityPlayerGameObject))
        {
            return;
        }

        _players.Add(entityPlayerGameObject);

        if (entityPlayerGameObject != Globals.LocalPlayer)
        {
            return;
        }

        if (ModMain.ShalazamClient.Username != null)
        {
            UIChatWindows.Instance.PassMessage($"Welcome, {ModMain.ShalazamClient.Username}. Thank you for contributing to Shalazam!", ChatChannelType.Info);
        }
        else
        {
            UIChatWindows.Instance.PassMessage("Failed to request your permissions from Shalazam, did you forget to add your API key to the config?", ChatChannelType.Info);
        }
    }

    public static void OnWorldItemAdded(NetworkWorldItem networkWorldItem)
    {
        if (networkWorldItem.displayName == null)
        {
            return;
        }

        // Network start is called twice for each object, so deduplicate here
        if (_worldItems.Contains(networkWorldItem))
        {
            return;
        }

        _worldItems.Add(networkWorldItem);

        // Filter items that can't be gathered, but are still network objects, e.g., doors
        if (networkWorldItem.worldItemType is WorldItemTypeEnum.Harvestable)
        {
            // Run this as a coroutine to avoid blocking the render thread with an http request
            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                ModMain.ShalazamClient.PostResourceLocation(networkWorldItem);
            }
        }

        if (networkWorldItem.worldItemType is WorldItemTypeEnum.CraftingStation)
        {
            _craftingStations.Add(networkWorldItem);

            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                ModMain.ShalazamClient.PostWorldItemLocation(networkWorldItem);
            }
        }

        if (networkWorldItem.displayName is "Treasure Chest" or "Supply Crate")
        {
            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                ModMain.ShalazamClient.PostWorldItemLocation(networkWorldItem);
            }
        }
    }

    public static void OnWorldItemRemoved(NetworkWorldItem networkWorldItem)
    {
        _worldItems.Remove(networkWorldItem);
    }

    public static void OnNpcAdded(EntityNpcGameObject entityNpcGameObject)
    {
        if (_monsters.Contains(entityNpcGameObject) || _friendlyNpcs.Contains(entityNpcGameObject))
        {
            return;
        }

        var npcName = entityNpcGameObject.Nameplate.nameText.text;

        if (entityNpcGameObject.Profession == NpcProfession.None)
        {
            _monsters.Add(entityNpcGameObject);

            if (entityNpcGameObject.Status.IsDead())
            {
                return;
            }

            // Weird behaviour in game, all NPCs have subname text set to Soandso's Minion, I guess as placeholder, but it
            // never displays this, so we'll rely on it I guess... sometimes minions are bugged and display as attackable
            // NPCs even if they're a player's summon. So we can't just rely on petmaster, as that's set to null in these cases.
            // I bet it's because the Summon enters the player's loadable area before the owner.
            if (entityNpcGameObject.PetMaster != null)
            {
                return;
            }

            var isOn = entityNpcGameObject.Nameplate.subNameText.isActiveAndEnabled;

            if (isOn)
            {
                return;
            }

            // Cheeky hack because the combat history and target is set to null when the npc pops in to the viewport, until
            // someone hits it the next time... so can't use those for determining if a mob is in combat.
            var pool = entityNpcGameObject.Pools.GetPool(PoolType.Health);
            var isFull = pool.Current == pool.Max;

            if (!isFull)
            {
                return;
            }

            if (_blacklist.Contains(npcName))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                ModMain.ShalazamClient.PostMonster(entityNpcGameObject);
            }
        }
        else
        {
            _friendlyNpcs.Add(entityNpcGameObject);

            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                ModMain.ShalazamClient.PostNpc(entityNpcGameObject);
            }
        }
    }

    public static void OnNpcRemoved(EntityNpcGameObject entityNpcGameObject)
    {
        _monsters.Remove(entityNpcGameObject);

        LootCache.OnNpcDeleted(entityNpcGameObject);
    }
}
