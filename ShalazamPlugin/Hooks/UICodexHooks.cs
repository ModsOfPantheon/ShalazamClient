using HarmonyLib;
using Il2Cpp;

namespace ShalazamPlugin.Hooks;

[HarmonyPatch(typeof(UICodex), nameof(UICodex.RebuildAllAbilities))]
public class OnRebuildAllAbilities
{
    public static void Postfix(UICodex __instance)
    {
        foreach (var ability in __instance.abilities)
        {
            AbilityCache.PostAbility(ability.Ability);
        }
    }
}