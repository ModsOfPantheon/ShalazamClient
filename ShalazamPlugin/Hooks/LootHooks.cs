using HarmonyLib;
using Il2Cpp;

namespace ShalazamPlugin.Hooks;

[HarmonyPatch(typeof(LootWithPersyst.Logic), nameof(LootWithPersyst.Logic.Internal_ClientLoadedLoot))]
public class LootHooksPrefix
{
    private static void Prefix(LootWithPersyst.Logic __instance, LootWithPersyst.LootLoadedMessage message)
    {
        // IsSkinnable is true in the Prefix for ClientLoadedLoot but not PostFix
        var cast = __instance.Entity.TryCast<EntityNpcGameObject>();

        if (cast == null)
        {
            return;
        }
        
        LootCache.OnLootReceived(cast, __instance.IsSkinnable, message.DeserializedItems);
    }
}