using System;
using FortRise;
using MonoMod.ModInterop;
using TF.EX.Domain.Extensions;
using TF.EX.Domain.Interop;
using TF.EX.Domain.Ports;
using TowerFall;

namespace TF.EX.API
{
    /// <summary>
    /// Mod interop with other mods.
    /// </summary>
    [ModExportName("TF.EX.API")]
    public static class ModExports
    {
        /// <summary>
        /// Determine if the game is currently playing online.
        ///
        /// <para>Might be useful i dont know</para>
        /// </summary>
        public static bool IsPlayingOnline()
        {
            return TF.EX.Domain.ServiceCollections.ResolveNetplayManager().IsInit();
        }

        /// <summary>
        /// Make a versus mode of the mod playable online: the lobby offers it and the Netplay mode runs its round logic.
        ///
        /// <para>name is the one given to RegisterVersusGameMode. spawnPlayers spawns the players of a round (and sets
        /// RoundLogic.Players), as the mode's OnLevelLoadFinish does offline; online, TF.EX calls it once the session
        /// is established, at the next rounds and when a replay starts.</para>
        /// </summary>
        public static void RegisterNetplayVersusMode(Mod module, string name, Action<RoundLogic> spawnPlayers)
        {
            NetplayVersusModes.Register($"{module.Meta.Name}/{name}", spawnPlayers);
        }

        /// <summary>
        /// The mode is no longer offered online.
        /// </summary>
        public static void UnregisterNetplayVersusMode(Mod module, string name)
        {
            NetplayVersusModes.Unregister($"{module.Meta.Name}/{name}");
        }

        /// <summary>
        /// To call in the mode's RoundLogic.OnLevelLoadFinish, after base.OnLevelLoadFinish(). True when the round is
        /// played online: TF.EX spawns the players, so the round logic must not.
        /// </summary>
        public static bool OnNetplayLevelLoadFinish(RoundLogic roundLogic)
        {
            if (NetplayVersusModes.Selected() == null || !roundLogic.Session.MatchSettings.Mode.IsNetplay() || roundLogic is IRoundPlayerSpawner)
            {
                return false;
            }

            NetplayVersusModes.OnLevelLoadFinish(roundLogic, () => NetplayVersusModes.TrySpawnRoundPlayers(roundLogic, withVersusStart: true));

            return true;
        }
    }
}
