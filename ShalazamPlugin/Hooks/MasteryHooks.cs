using System.Text.Json;
using System.Text.Json.Serialization;
using HarmonyLib;
using Il2Cpp;
using Il2CppAdventuringMastery;
using MelonLoader;
using ShalazamPlugin.Extensions;
using ShalazamPlugin.SDK;
using ShalazamPlugin.SDK.Models;
using ShalazamPlugin.SDK.Models.Websockets;

namespace ShalazamPlugin.Hooks;

// Fires when the player opens the mastery tree UI.
// TryBuildInitialUIData has ref parameters that crash Il2CppInterop's Harmony DMD, so we
// hook HandleOpen instead and navigate to the Logic via the shared GameObject.
[HarmonyPatch(typeof(MasteryLocalDispatcher), nameof(MasteryLocalDispatcher.HandleOpen))]
public class MasteryHandleOpenHook
{
    private static bool _sent;

    private static void Prefix(MasteryLocalDispatcher __instance)
    {
        if (_sent) return;

        try
        {
            var logic = __instance.GetComponent<MasteryTree>()?.logic;
            if (logic == null) return;

            var context = logic.BuildPlayerContext();
            var characterClass = context?.CharacterClass.ToString() ?? "Unknown";

            var states = logic.GetDerivedStates();
            var nodeDataList = new List<MasteryNodeData>();
            foreach (var kvp in states)
            {
                var node = logic.GetNodeDataByGuid(kvp.Key);
                if (node != null) nodeDataList.Add(node);
            }

            var payload = nodeDataList.ToMasteryPayload(characterClass);
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true,
            });
            MelonLogger.Msg($"[Mastery:HandleOpen] {json}");

            ModMain.ShalazamClient?.PostMastery(payload);

            _sent = true;
        }
        catch (Exception ex) { MelonLogger.Warning($"[Mastery:HandleOpen] {ex.Message}"); }
    }
}
