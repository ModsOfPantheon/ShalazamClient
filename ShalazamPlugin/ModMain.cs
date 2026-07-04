using Il2Cpp;
using Il2CppPantheonPersist;
using MelonLoader;
using ShalazamPlugin.SDK;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShalazamPlugin;

public class ModMain : MelonMod
{
    public static IShalazamClient ShalazamClient;

    public override void OnInitializeMelon()
    {
        var category = MelonPreferences.CreateCategory("ShalazamApi");
        var apiKey = category.CreateEntry<string>("ApiKey", "");
        Globals.MinimumTrackingDistance = category.CreateEntry("MinimumTrackingDistance", 3f).Value;
        Globals.VerboseLogging = category.CreateEntry("VerboseLogging", false).Value;

        category.SaveToFile(false);

        EntityManager.SetApiKey(apiKey.Value);

        // Having this isn't very useful because there is no event fired when someone saves the config file
        ShalazamClient = new ShalazamWebsocketClient(apiKey.Value);
    }

    public override void OnApplicationQuit()
    {
    }

    public override void OnUpdate()
    {
        if (Globals.TrackedOffensiveEntity == null)
        {
            return;
        }

        var currentTargetPos = Globals.TrackedOffensiveEntity.transform.position;

        if (Globals._lastPosition == null)
        {
            Globals._lastPosition = Globals.TrackedOffensiveEntity.transform.position;
            ShalazamClient.PostMonster(Globals.TrackedOffensiveEntity);

            return;
        }

        if (Vector3.Distance(currentTargetPos, Globals._lastPosition.Value) > Globals.MinimumTrackingDistance)
        {
            ShalazamClient.PostMonster(Globals.TrackedOffensiveEntity);
            Globals._lastPosition = currentTargetPos;
        }
    }

    public static void TrackOffensiveTarget()
    {
        if (Globals.LocalPlayer == null)
        {
            return;
        }

        var offensiveTarget = Globals.LocalPlayer.Targets.Offensive;
        if (offensiveTarget == null)
        {
            return;
        }

        var targetEntity = offensiveTarget.TryCast<EntityNpcGameObject>();

        if (targetEntity == null)
        {
            return;
        }

        if (targetEntity.Profession != NpcProfession.None)
        {
            UIChatWindows.Instance.PassMessage($"Can't track {targetEntity.info.DisplayName} because they are not a monster", ChatChannelType.Info);
            return;
        }

        Globals.TrackedOffensiveEntity = targetEntity;
        Globals._lastPosition = null;

        UIChatWindows.Instance.PassMessage($"Started tracking {Globals.TrackedOffensiveEntity?.info.DisplayName}", ChatChannelType.Info);
    }

    public static void StopTrackingOffensiveTarget()
    {
        if (Globals.TrackedOffensiveEntity == null)
        {
            return;
        }

        Globals.TrackedOffensiveEntity = null;
        Globals._lastPosition = null;

        UIChatWindows.Instance.PassMessage("Stopped tracking offensive target", ChatChannelType.Info);
    }

    // On-demand bulk upload of every ability baked into the client build
    // (CachedInBuildData.abilityData), routed through the same AbilityCache.PostAbility
    // path the codex uses. Triggered by the /shalazamuploadabilities chat command so it
    // doesn't run on every client load. Note: ToRequestPayload sources ClassName from
    // Globals.LocalPlayer, so every uploaded ability is tagged with the local player's class.
    public static void UploadAllAbilitiesFromCache()
    {
        var abilities = CachedInBuildData.data?.abilityData;
        if (abilities == null)
        {
            UIChatWindows.Instance.PassMessage("Shalazam: ability cache not loaded yet", ChatChannelType.Info);
            return;
        }

        var posted = 0;
        var failed = 0;
        foreach (var ability in abilities)
        {
            // Only upload the current live set, identified by a "P4" DesignerId prefix.
            if (ability == null || ability.DesignerId == null ||
                !ability.DesignerId.StartsWith("P4", StringComparison.Ordinal)) continue;

            try
            {
                AbilityCache.PostAbility(ability);
                posted++;
            }
            catch (Exception ex)
            {
                failed++;
                MelonLogger.Warning($"[UploadAbilities] {ability.Id} ({ability.DesignerId}): {ex.Message}");
            }
        }

        UIChatWindows.Instance.PassMessage(
            $"Shalazam: queued {posted} abilities for upload ({failed} failed)", ChatChannelType.Info);
    }

    // On-demand bulk upload of every buff baked into the client build (CachedInBuildData.buffData),
    // routed through ShalazamClient.PostBuffs. Triggered by the /shalazamuploadbuffs chat command.
    public static void DumpBuffsFromCache()
    {
        var buffs = CachedInBuildData.data?.buffData;
        if (buffs == null)
        {
            UIChatWindows.Instance.PassMessage("Shalazam: buff cache not loaded yet", ChatChannelType.Info);
            return;
        }

        ShalazamClient.PostBuffs(buffs);

        UIChatWindows.Instance.PassMessage("Shalazam: queued buffs for upload", ChatChannelType.Info);
    }

    public const string PluginVersion = "2026.07.04";
}
