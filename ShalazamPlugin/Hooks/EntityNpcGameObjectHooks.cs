using HarmonyLib;
using Il2Cpp;
using Il2CppViNL;

namespace ShalazamPlugin.Hooks;

[HarmonyPatch(typeof(EntityNpcGameObject))]
[HarmonyPatch(nameof(EntityNpcGameObject.NetworkStart))]
public class NetworkStart
{
    private static void Postfix(EntityNpcGameObject __instance, NetworkObject networkObject)
    {
        EntityManager.OnNpcAdded(__instance);
    }
}

[HarmonyPatch(typeof(EntityNpcGameObject))]
[HarmonyPatch(nameof(EntityNpcGameObject.NetworkStop))]
public class NetworkStop
{
    private static void Prefix(EntityNpcGameObject __instance)
    {
        EntityManager.OnNpcRemoved(__instance);
    }
}