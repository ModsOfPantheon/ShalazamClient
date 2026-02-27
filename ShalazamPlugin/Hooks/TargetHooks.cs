using HarmonyLib;
using Il2Cpp;
using Il2CppPantheonPersist;

namespace ShalazamPlugin.Hooks;

[HarmonyPatch(typeof(Targets.Logic), nameof(Targets.logic.SetOffensive))]
public class TargetHooks
{
    private static void Postfix(Targets.Logic __instance)
    {
        if (__instance.Entity.NetworkId.Value != EntityPlayerGameObject.LocalPlayerId.Value)
        {
            return;
        }

        if (__instance.Offensive == null)
        {
            Globals.TrackedOffensiveEntity = null;
        }
    }
}