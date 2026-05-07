using HarmonyLib;
using Il2Cpp;
using Il2CppPantheonPersist;
using MelonLoader;

namespace ShalazamPlugin.Hooks;

[HarmonyPatch(typeof(UIChatWindows), nameof(UIChatWindows.PassMessage), typeof(string), typeof(string), typeof(ChatChannelType))]
public class NoLootHook
{
    private static void Prefix(UIChatWindows __instance, string name, string message, ChatChannelType channel)
    {
        if (message == "You didn't find anything useful." && channel == ChatChannelType.Loot)
        {
            var offensiveTarget = Globals.LocalPlayer.Targets.Offensive.TryCast<EntityNpcGameObject>();

            if (offensiveTarget == null)
            {
                MelonLogger.Error($"Failed to cast offensive target to EntityNpcGameObject, this should never happen");
                return;
            }
            
            LootCache.OnLootReceived(offensiveTarget, false, Array.Empty<Item>());
        }
    }
}