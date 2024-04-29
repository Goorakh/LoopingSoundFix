using BepInEx.Bootstrap;
using RiskOfOptions;
using RiskOfOptions.Options;

namespace LoopingSoundFix
{
    static class RiskOfOptionsCompat
    {
        public static bool IsActive => Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");

        public static void Init()
        {
            ModSettingsManager.AddOption(new ChoiceOption(Main.SoundCancelModeConfig));
        }
    }
}
