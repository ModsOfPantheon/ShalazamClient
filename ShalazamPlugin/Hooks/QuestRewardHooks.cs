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
    // SelectQuestId fires twice when the journal opens; only log objectives when the selection actually
    // changes so we don't emit the list twice. -1 = nothing logged yet.
    private static int _lastLoggedObjectivesQuestId = -1;

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

                ItemCache.OnItemSeen(item);
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[ShalazamItem] QuestJournal.SelectQuestId hook error: {ex.Message}");
        }

        // Objectives: SelectQuestId → RefreshObjectives has already filled __instance.objectives by the time
        // this postfix runs. selectedQuestId is the same id space as the NPC dialog's renderingQuestId, so
        // this correlates the objective list to the quest we logged from the interaction popup. Each
        // UIQuestJournalObjective.task is a ClientQuestTask (Text/Progress/MaxProgress) — plain
        // reference/primitive fields, safe to read (unlike the Nullable<FormattedQuestItem> reward structs).
        try
        {
            var questId = __instance.selectedQuestId;
            if (questId == _lastLoggedObjectivesQuestId)
            {
                return;
            }
            _lastLoggedObjectivesQuestId = questId;

            var objectives = __instance.objectives;

            Log.Verbose($"[ShalazamQuest] ── objectives for quest #{questId} ──────────────");
            if (objectives == null || objectives.Count == 0)
            {
                Log.Verbose("[ShalazamQuest]   (no objectives)");
                return;
            }

            for (var i = 0; i < objectives.Count; i++)
            {
                var task = objectives[i]?.task;
                if (task == null)
                {
                    continue;
                }

                Log.Verbose(
                    $"[ShalazamQuest]   [{i}] {task.Text}  ({task.Progress}/{task.MaxProgress})");
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[ShalazamQuest] QuestJournal objectives hook error: {ex.Message}");
        }
    }
}
