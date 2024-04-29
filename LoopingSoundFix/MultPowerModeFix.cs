using EntityStates.Toolbot;
using RoR2;

namespace LoopingSoundFix
{
    static class MultPowerModeFix
    {
        public static void Apply()
        {
            On.EntityStates.Toolbot.ToolbotDualWieldBase.OnExit += ToolbotDualWieldBase_OnExit;
        }

        public static void Undo()
        {
            On.EntityStates.Toolbot.ToolbotDualWieldBase.OnExit -= ToolbotDualWieldBase_OnExit;
        }

        static void ToolbotDualWieldBase_OnExit(On.EntityStates.Toolbot.ToolbotDualWieldBase.orig_OnExit orig, ToolbotDualWieldBase self)
        {
            orig(self);

            if (self is ToolbotDualWieldStart)
            {
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                Util.PlaySound(ToolbotDualWield.stopLoopSoundString, self.gameObject);
#pragma warning restore Publicizer001 // Accessing a member that was not originally public
            }
        }
    }
}
