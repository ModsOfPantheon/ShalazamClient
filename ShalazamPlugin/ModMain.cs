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

    public const string PluginVersion = "2.5.0";
}