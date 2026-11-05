using System.Collections.Generic;
using FortRise;
using HarmonyLib;
using MonoMod.Utils;
using TF.EX.Domain.Extensions;
using TowerFall;

namespace TF.EX.Patchs.Entity.MenuItem
{
    [HarmonyPatch(typeof(VersusModeButton))]
    public class VersusModeButtonPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static void VersusModeButton_Update_Prefix(VersusModeButton __instance, out ModeSelection __state)
        {
            __state = ModeSelection.Capture(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void VersusModeButton_Update_Postfix(VersusModeButton __instance, ModeSelection __state)
        {
            var settings = TowerFall.MainMenu.VersusMatchSettings;
            var mode = settings.Mode;

            if (mode == __state.Mode || !mode.IsNetplay())
            {
                return;
            }

            // The Netplay mode isn't offered in local play: step over it in the direction the player moved, so the modes after it stay
            // reachable (every mode registered after TF.EX's). Back to the previous mode only when there is none beyond it.
            var button = DynamicData.For(__instance);
            var index = button.Get<int>("currentIndex");
            var modes = button.Get<List<TowerFall.Modes>>("modeSelect");
            var next = index + (index > __state.CurrentIndex ? 1 : -1);

            if (modes == null || next < 0 || next >= modes.Count)
            {
                __state.Restore(__instance);
                return;
            }

            var nextMode = modes[next];
            var isCustom = GameModeRegistry.ModesToVersusGameMode.TryGetValue(nextMode, out var entry);

            settings.Mode = nextMode;
            settings.IsCustom = isCustom;
            if (isCustom)
            {
                DynamicData.For(settings).Set("CustomVersusModeName", entry.Name);
            }

            button.Set("currentIndex", next);
            button.Invoke("UpdateSides");
        }

        public struct ModeSelection
        {
            public TowerFall.Modes Mode;
            public bool IsCustom;
            public string CustomVersusModeName;
            public int CurrentIndex;

            public static ModeSelection Capture(VersusModeButton button)
            {
                var settings = TowerFall.MainMenu.VersusMatchSettings;

                return new ModeSelection
                {
                    Mode = settings.Mode,
                    IsCustom = settings.IsCustom,
                    CustomVersusModeName = settings.CustomVersusModeName,
                    CurrentIndex = DynamicData.For(button).Get<int>("currentIndex"),
                };
            }

            public readonly void Restore(VersusModeButton button)
            {
                var settings = TowerFall.MainMenu.VersusMatchSettings;

                settings.Mode = Mode;
                settings.IsCustom = IsCustom;
                DynamicData.For(settings).Set("CustomVersusModeName", CustomVersusModeName);

                var dynButton = DynamicData.For(button);
                dynButton.Set("currentIndex", CurrentIndex);
                dynButton.Invoke("UpdateSides");
            }
        }
    }
}
