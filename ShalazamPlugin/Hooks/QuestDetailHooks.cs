using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace ShalazamPlugin.Hooks;

// Logs quest details from the NPC interaction window. We'll route this to Shalazam later; for now it just
// dumps to the log.
//
// Hook point: UINpcInteractionPopup.Update() — a safe, no-arg MonoBehaviour method. We watch
// renderingQuestId so we log the rendered quest detail (dialogue, currency, reward items off
// questRewardSlots) whenever the window switches quests. This fires for *every* viewed quest, including
// ones with no item rewards. (An earlier reward-slot SetItem hook missed no-reward quests entirely, since
// SetItem never fires for them; and RedrawAllQuestButtons only had the sparse id+name list, not useful.)
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

// Logs the rendered quest detail whenever the popup switches to showing a quest. We watch renderingQuestId
// (set by the by-ref render methods we can't patch) from the popup's Update, so this fires for *every*
// quest viewed — including quests with no item rewards, which never trigger a reward-slot fill.
[HarmonyPatch(typeof(UINpcInteractionPopup), nameof(UINpcInteractionPopup.Update))]
public class UINpcInteractionPopupUpdateHook
{
    // The quest currently reflected in the rendered window; -1 = none logged yet.
    private static int _lastRenderedQuestId = -1;

    private static void Postfix(UINpcInteractionPopup __instance)
    {
        try
        {
            var questId = __instance.renderingQuestId;
            if (questId <= 0 || questId == _lastRenderedQuestId)
            {
                return;
            }
            _lastRenderedQuestId = questId;

            Log.Verbose("[ShalazamQuest] ── quest detail ──────────────");
            Log.Verbose($"[ShalazamQuest] rendering quest #{questId}: {__instance.questName?.text}");

            // GiverName on ClientQuest is empty client-side, so source the NPC from the window.
            var npc = __instance.lastInteractingNpc?.TryCast<EntityNpcGameObject>();
            Log.Verbose(
                $"[ShalazamQuest]   NPC: {npc?.info?.DisplayName}   Zone: {PantheonGameClient.currentZone}");

            LogText("Dialogue", __instance.dialogue?.text);
            LogText("AcceptDialogue", __instance.questAcceptDialogue?.text);
            LogText("TalkToNpcDialogue", __instance.questTalkToNpcDialogue?.text);

            Log.Verbose(
                $"[ShalazamQuest]   Currency reward: {__instance.platinumRewardText?.text}p {__instance.goldRewardText?.text}g {__instance.silverRewardText?.text}s {__instance.copperRewardText?.text}c");
            Log.Verbose($"[ShalazamQuest]   XP reward: {__instance.experienceGainRewardText?.text}");
            Log.Verbose($"[ShalazamQuest]   Reputation reward: {__instance.heartReputationRewardText?.text}");

            // Reward items live on the reference-typed reward-slot Item (see header note); empty for
            // quests with no item rewards, in which case nothing below logs.
            var slots = __instance.questRewardSlots;
            if (slots != null)
            {
                foreach (var slot in slots)
                {
                    var item = slot?.Item;
                    if (item?.Template == null)
                    {
                        continue;
                    }

                    Log.Verbose(
                        $"[ShalazamQuest]   Reward item #{item.Template.ItemId}: {item.Template.ItemName}");

                    // Upload the reward item (deduped across every source by ItemId).
                    ItemCache.OnItemSeen(item);
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[ShalazamQuest] quest detail hook error: {ex.Message}");
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
