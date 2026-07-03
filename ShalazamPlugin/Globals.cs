using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace ShalazamPlugin;

public class Globals
{
    public static bool IsPTR = false;
    public static bool HasSetUpUI = false;
    public static EntityPlayerGameObject? LocalPlayer = null;
    public static EntityNpcGameObject? TrackedOffensiveEntity = null;
    public static Vector3? _lastPosition = null;
    public static float MinimumTrackingDistance = 3f;
    public static bool VerboseLogging = false;
}