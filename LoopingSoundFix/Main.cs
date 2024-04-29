using BepInEx;
using BepInEx.Configuration;
using System.Diagnostics;

namespace LoopingSoundFix
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "LoopingSoundFix";
        public const string PluginVersion = "1.0.0";

        internal static Main Instance { get; private set; }

        internal static ConfigEntry<SoundCancelMode> SoundCancelModeConfig;
        public static SoundCancelMode SoundCancelMode => SoundCancelModeConfig.Value;

        void Awake()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Log.Init(Logger);

            Instance = SingletonHelper.Assign(Instance, this);

            SoundCancelModeConfig = Config.Bind("General", "Sound Stop Mode", SoundCancelMode.NextLoop, $"""
                Determines how the mod handles an active sound when its audio source is removed.
                
                {nameof(SoundCancelMode.Immediate)}: The sound is cut off immediately. This guarantees that all sounds stop, but can cause certain sounds to not play fully.
                
                {nameof(SoundCancelMode.NextLoop)}: Stops the sound after it finishes playing the current loop, if the sound is non-looping, it plays to completion
                """);

            if (RiskOfOptionsCompat.IsActive)
                RiskOfOptionsCompat.Init();

            EnsureSoundEventsStopOnDestroy.ApplyPatches();
            MultPowerModeFix.Apply();

            stopwatch.Stop();
            Log.Info_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }

        void OnDestroy()
        {
            EnsureSoundEventsStopOnDestroy.UndoPatches();
            MultPowerModeFix.Undo();

            Instance = SingletonHelper.Unassign(Instance, this);
        }
    }
}
