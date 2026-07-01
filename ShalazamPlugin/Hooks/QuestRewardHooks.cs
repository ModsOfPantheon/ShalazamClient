using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace ShalazamPlugin.Hooks;

// Captures quest reward items when you pick a quest in the journal. Reads the resolved Item off each
// populated reward slot and routes it through ItemCache (shared dedup with normal item uploads).
//
// We deliberately work off the reward slots' materialised Item rather than FormattedQuestRewards:
// RefreshQuestRewards takes a `FormattedQuestRewards&` by-ref struct (crashes Il2CppInterop's Harmony DMD,
// per MasteryHooks), and pulling FormattedQuestItem values out of the struct means unwrapping
// Il2CppSystem.Nullable<struct>, which faults with an AccessViolation. Reference-typed Items are safe to
// read, and — crucially — only the Item carries the stat modifiers (ItemTemplate.StatModifiers is always
// null on the client), so the Item is the only source that uploads complete data.

[HarmonyPatch(typeof(UIQuestJournal), nameof(UIQuestJournal.SelectQuestId))]
public class QuestJournalSelectHook
{
    private static void Postfix(UIQuestJournal __instance)
    {
        try
        {
            var slots = __instance.questRewardSlots;
            if (slots == null)
            {
                return;
            }

            foreach (var slot in slots)
            {
                var item = slot?.Item;
                if (item == null)
                {
                    continue;
                }

                ItemCache.OnItemSeen(item, "QuestJournal.SelectQuestId");
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[ShalazamItem] QuestJournal.SelectQuestId hook error: {ex.Message}");
        }
    }
}
