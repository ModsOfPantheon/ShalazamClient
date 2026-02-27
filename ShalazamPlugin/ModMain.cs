using Il2Cpp;
using MelonLoader;
using ShalazamPlugin.SDK;

namespace ShalazamPlugin;

public class ModMain : MelonMod
{
    public static IShalazamClient ShalazamClient;
    
    public override void OnInitializeMelon()
    {
        var category = MelonPreferences.CreateCategory("ShalazamApi");
        var apiKey = category.CreateEntry<string>("ApiKey", "");

        category.SaveToFile(false);
        
        EntityManager.SetApiKey(apiKey.Value);
        
        // Having this isn't very useful because there is no event fired when someone saves the config file
        ShalazamClient = new ShalazamWebsocketClient(apiKey.Value);
    }

    public override void OnApplicationQuit()
    {
    }

    public const string PluginVersion = "2.1.0";

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