using HarmonyLib;
using Il2Cpp;
using Il2CppViNL;

namespace ShalazamPlugin.Hooks;

[HarmonyPatch(typeof(NetworkWorldItem))]
[HarmonyPatch(nameof(NetworkWorldItem.NetworkStart))]
public class NetworkStartHook
{
    private static void Postfix(NetworkWorldItem __instance, NetworkObject networkObject)
    {
        EntityManager.OnWorldItemAdded(__instance);
    }
}

[HarmonyPatch(typeof(NetworkWorldItem))]
[HarmonyPatch(nameof(NetworkWorldItem.NetworkStop))]
public class NetworkStopHook
{
    private static void Prefix(NetworkWorldItem __instance, NetworkObject networkObject)
    {
        EntityManager.OnWorldItemRemoved(__instance);
    }
}