using Il2Cpp;
using Il2CppPantheonPersist;
using MelonLoader;
using ShalazamPlugin.SDK;
using UnityEngine;

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

    public const string PluginVersion = "2.2.0";

    public static void TrackOffensiveTarget()
    {
        if (Globals.LocalPlayer == null)
        {
            return;
        }

        var offensiveTarget = Globals.LocalPlayer.Targets.Offensive;
        if (offensiveTarget != null)
        {
            Globals.TrackedOffensiveEntity = offensiveTarget.TryCast<EntityNpcGameObject>();
            Globals._lastPosition = null;
        }
    }
}