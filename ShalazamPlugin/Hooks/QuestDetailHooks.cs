using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace ShalazamPlugin.Hooks;

// Logs quest details from the NPC interaction window. We'll route this to Shalazam later; for now it just
// dumps to the log.
//
// Two safe hook points, both on UI MonoBehaviours with plain reference/primitive signatures:
//
//   1. UINpcInteractionPopup.RedrawAllQuestButtons(IEntityNpc) — fires when the NPC's quest list draws
//      (on hail). Gives the set of quests the NPC offers, but at that point clientQuests is sparse
//      (id + name only); the full text/rewards arrive later over the quest-info RPC.
//
//   2. UIItemSlot.SetItem(IEntity, Item, Int32) — fires whenever an item is placed in *any* slot. We
//      filter to UIItemQuestRewardSlot, so it fires exactly as the quest reward panel populates — which
//      is where reward item ids live, and by which point the popup has rendered its dialogue/currency
//      text. We reach those by walking up from the slot to the UINpcInteractionPopup.
//
// Why not the obvious methods:
//   * PlayerInteraction.InformServerOfClicked* are ViNL networking/RPC stubs — patching them caused an
//     infinite startup error loop, so they're off-limits.
//   * UINpcInteractionPopup.RedrawWindowForQuestAcceptScreen / RefreshQuestRewardsPanel and the RPC
//     handlers take `FormattedQuestRewards&` by-ref structs / `ReadOnlySpan<Byte>`, which crash
//     Il2CppInterop's Harmony DMD (see QuestRewardHooks / MasteryHooks).
//   * OnClickedQuestId never fired for the observed view+accept flow.
//   * ClientQuest.Rewards.Item0..7 are Il2CppSystem.Nullable<FormattedQuestItem> — unwrapping them faults
//     with an AccessViolation (documented in QuestRewardHooks), so reward items must come off the
//     reference-typed reward-slot Item instead.

[HarmonyPatch(typeof(UINpcInteractionPopup), nameof(UINpcInteractionPopup.RedrawAllQuestButtons))]
public class RedrawAllQuestButtonsHook
{
    // Re-log a quest when its content grows (data arrives after the initial sparse hail render).
    private static readonly Dictionary<int, int> LoggedSignature = new();

    private static void Postfix(UINpcInteractionPopup __instance, IEntityNpc npcQuestGiver)
    {
        try
        {
            var logic = (npcQuestGiver ?? __instance?.lastInteractingNpc)?.NpcQuests;
            if (logic?.clientQuests == null)
            {
                return;
            }

            foreach (var quest in logic.clientQuests)
            {
                if (quest == null)
                {
                    continue;
                }

                var tasks = quest.Tasks;
                var rewards = quest.Rewards;
                var taskCount = tasks?.Count ?? 0;

                // Signature captures how much data has loaded so we re-log once it's populated.
                var signature = taskCount
                    + (int)rewards.TotalCurrency
                    + rewards.Experience
                    + (rewards.HasAnyItems() ? 1 : 0);

                if (LoggedSignature.TryGetValue(quest.QuestId, out var prev) && prev == signature)
                {
                    continue;
                }
                LoggedSignature[quest.QuestId] = signature;

                Log.Verbose("[ShalazamQuest] ───────────────────────────────");
                Log.Verbose($"[ShalazamQuest] quest #{quest.QuestId}: {quest.Name}");
                Log.Verbose($"[ShalazamQuest]   Giver: {quest.GiverName}   Zone: {quest.Zone}");

                if (!string.IsNullOrWhiteSpace(quest.AcceptText))
                {
                    Log.Verbose($"[ShalazamQuest]   AcceptText: {quest.AcceptText}");
                }

                if (logic.Internal_TryGetQuestDialogue(quest.QuestId, out var dialogue) &&
                    !string.IsNullOrWhiteSpace(dialogue))
                {
                    Log.Verbose($"[ShalazamQuest]   Dialogue: {dialogue}");
                }

                if (tasks != null)
                {
                    foreach (var task in tasks)
                    {
                        Log.Verbose($"[ShalazamQuest]   Task: {task.Text} ({task.Progress}/{task.MaxProgress})");
                    }
                }

                Log.Verbose(
                    $"[ShalazamQuest]   Rewards: xp={rewards.Experience} rep={rewards.Reputation} currency={rewards.TotalCurrency} hasItems={rewards.HasAnyItems()}");
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[ShalazamQuest] RedrawAllQuestButtons hook error: {ex.Message}");
        }
    }
}

// Fires as the quest reward panel populates: captures reward item ids and the rendered dialogue/currency.
[HarmonyPatch(typeof(UIItemSlot), nameof(UIItemSlot.SetItem))]
public class QuestRewardSlotSetItemHook
{
    // Dedup per (quest, item) and per-quest header so repeated redraws don't spam.
    private static readonly HashSet<(int, int)> LoggedItems = new();
    private static readonly HashSet<int> LoggedHeaders = new();

    private static void Postfix(UIItemSlot __instance, Item item)
    {
        try
        {
            if (item?.Template == null || __instance.TryCast<UIItemQuestRewardSlot>() == null)
            {
                return;
            }

            var popup = __instance.GetComponentInParent<UINpcInteractionPopup>();
            if (popup == null)
            {
                return;
            }

            var questId = popup.renderingQuestId;

            if (LoggedHeaders.Add(questId))
            {
                Log.Verbose("[ShalazamQuest] ── reward panel ──────────────");
                Log.Verbose($"[ShalazamQuest] rendering quest #{questId}: {popup.questName?.text}");

                // GiverName on ClientQuest is empty client-side, so source the NPC from the window.
                var npc = popup.lastInteractingNpc?.TryCast<EntityNpcGameObject>();
                Log.Verbose(
                    $"[ShalazamQuest]   NPC: {npc?.info?.DisplayName}   Zone: {PantheonGameClient.currentZone}");

                LogText("Dialogue", popup.dialogue?.text);
                LogText("AcceptDialogue", popup.questAcceptDialogue?.text);
                LogText("TalkToNpcDialogue", popup.questTalkToNpcDialogue?.text);

                Log.Verbose(
                    $"[ShalazamQuest]   Currency reward: {popup.platinumRewardText?.text}p {popup.goldRewardText?.text}g {popup.silverRewardText?.text}s {popup.copperRewardText?.text}c");
                Log.Verbose($"[ShalazamQuest]   XP reward: {popup.experienceGainRewardText?.text}");
                Log.Verbose($"[ShalazamQuest]   Reputation reward: {popup.heartReputationRewardText?.text}");
            }

            if (LoggedItems.Add((questId, item.Template.ItemId)))
            {
                Log.Verbose(
                    $"[ShalazamQuest]   Reward item #{item.Template.ItemId}: {item.Template.ItemName}");

                // Upload the reward item (deduped across every source by ItemId).
                ItemCache.OnItemSeen(item);
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[ShalazamQuest] reward slot hook error: {ex.Message}");
        }
    }

    private static void LogText(string label, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            Log.Verbose($"[ShalazamQuest]   {label}: {value}");
        }
    }
}
