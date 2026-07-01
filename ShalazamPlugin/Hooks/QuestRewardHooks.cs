using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace ShalazamPlugin.Hooks;

// Two complementary ways of catching quest reward item templates so we can compare them in the console:
//
//  1. QuestJournalSelectHook — fires when you pick a quest in the journal (matches "made visible in the UI").
//     Reads the resolved Item off each populated reward slot. We deliberately avoid touching
//     FormattedQuestRewards here: RefreshQuestRewards takes a `FormattedQuestRewards&` by-ref struct
//     (crashes Il2CppInterop's Harmony DMD, per MasteryHooks), and pulling FormattedQuestItem values out
//     of the struct means unwrapping Il2CppSystem.Nullable<struct>, which faults with an
//     AccessViolation. The reward slots only ever hold reference-typed Items, which are safe to read.
//
//  2. QuestRewardAddItemHook — the lower-level per-item chokepoint. Fires whenever a reward item is added
//     to a FormattedQuestRewards (e.g. NPC quest previews the journal hook won't see), handing us the
//     ItemTemplate directly. In practice the journal path already has a materialised Item, so this mostly
//     backstops sources that don't.
//
// Both funnel into ItemCache, which shares one dedup cache with normal item uploads.

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

[HarmonyPatch(typeof(FormattedQuestRewards), nameof(FormattedQuestRewards.AddItem))]
public class QuestRewardAddItemHook
{
    private static void Postfix(ItemTemplate template)
    {
        try
        {
            if (template == null)
            {
                return;
            }

            ItemCache.OnItemTemplateSeen(template, "FormattedQuestRewards.AddItem");
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[ShalazamItem] FormattedQuestRewards.AddItem hook error: {ex.Message}");
        }
    }
}
