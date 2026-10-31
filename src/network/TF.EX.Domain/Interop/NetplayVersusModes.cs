using FortRise;
using Microsoft.Extensions.Logging;
using TF.EX.Domain.Extensions;
using TF.EX.Domain.Ports;
using TowerFall;

namespace TF.EX.Domain.Interop
{
    /// <summary>
    /// Versus modes of other mods that can be played online, inside the Netplay mode.
    /// </summary>
    public static class NetplayVersusModes
    {
        private static readonly List<string> ids = [];
        private static readonly Dictionary<string, Action<TowerFall.RoundLogic>> spawners = [];

        /// <summary>
        /// The FortRise ids ("ModName/ModeName") of the registered modes, in registration order.
        /// </summary>
        public static IReadOnlyList<string> Ids => ids;

        public static void Register(string id, Action<TowerFall.RoundLogic> spawnPlayers)
        {
            if (!spawners.ContainsKey(id))
            {
                ids.Add(id);
            }

            spawners[id] = spawnPlayers;
        }

        public static void Unregister(string id)
        {
            ids.Remove(id);
            spawners.Remove(id);
        }

        public static IVersusGameModeEntry Resolve(string id)
        {
            return !string.IsNullOrEmpty(id) && spawners.ContainsKey(id) && GameModeRegistry.RegistryVersusGameModes.TryGetValue(id, out var entry)
                ? entry
                : null;
        }

        /// <summary>
        /// The mod mode of the own lobby, null when it plays a built-in mode.
        /// </summary>
        public static IVersusGameModeEntry Selected()
        {
            var lobby = ServiceCollections.ResolveMatchmakingService().GetOwnLobby();

            return lobby.IsEmpty ? null : Resolve(lobby.GameData.CustomMode);
        }

        /// <summary>
        /// Spawns the players of a round of the selected mod mode (with its VersusStart, as a round starts), false when the round logic
        /// isn't one: the Netplay mode plays a built-in mode.
        /// </summary>
        public static bool TrySpawnRoundPlayers(TowerFall.RoundLogic roundLogic, bool withVersusStart)
        {
            var selected = Selected();

            if (selected == null || roundLogic == null || roundLogic is IRoundPlayerSpawner || !roundLogic.Session.MatchSettings.Mode.IsNetplay())
            {
                return false;
            }

            if (withVersusStart)
            {
                roundLogic.Session.CurrentLevel.Add(new VersusStart(roundLogic.Session));
            }

            spawners[selected.Name](roundLogic);

            return true;
        }

        /// <summary>
        /// What a round logic played online does once its level is loaded: the netplay session starts on the first round
        /// (the players spawn once it is established), the next rounds spawn their players.
        /// </summary>
        public static void OnLevelLoadFinish(TowerFall.RoundLogic roundLogic, Action spawnRoundPlayers)
        {
            var netplayManager = ServiceCollections.ResolveNetplayManager();

            if (netplayManager.IsReplayMode())
            {
                if (roundLogic.Session.RoundIndex > 0)
                {
                    spawnRoundPlayers();
                }

                return;
            }
            if (netplayManager.HasSessionDisconnected())
            {
                return;
            }

            if (!netplayManager.IsInit())
            {
                ServiceCollections.ResolveLogger().LogInformation("Configuring and initializing current netplay session");
                netplayManager.Init(roundLogic);

                var lobby = ServiceCollections.ResolveMatchmakingService().GetOwnLobby();
                ServiceCollections.ResolveReplayService().Initialize(lobby.GameData, lobby.Mods);
                if (lobby.IsReportEnabled)
                {
                    MatchReport.Begin(netplayManager.IsServerMode() ? lobby.SessionPlayers : null);
                }

                TowerFall.TFGame.ConsoleEnabled = false;
            }
            else
            {
                spawnRoundPlayers();
            }
        }
    }
}
