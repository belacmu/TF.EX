using TowerFall;

namespace TF.EX.Domain.Models
{
    public static class Constants
    {
        public const string DRIVER_NAME = "TF.EX";

        public const int VANILLA_FPS = 60;
        public const int NETPLAY_FPS = 240;

        public const int MAX_PREDICTION_FRAMES = 15;
        public const int MAX_PREDICTION_TICKS = MAX_PREDICTION_FRAMES * NETPLAY_FPS / VANILLA_FPS;

        public const int SPECTATOR_CATCHUP_THRESHOLD = NETPLAY_FPS / 2;

        public const int CONNECTION_TIMEOUT_MS = 30000;

        public const int MAX_SEATS = 8;

        public const int MAX_PLAYERS = 4;

        public const int WIDE_MAX_PLAYERS = 4;

        public const int MAX_DEVICES = 4;

        public static int LobbyMaxPlayers => ServiceCollections.ResolveWiderSetModApi()?.IsWide == true ? Math.Min(WIDE_MAX_PLAYERS, TFGame.Players.Length) : MAX_PLAYERS;

        public static int LobbyMaxLocalPlayers => LobbyMaxPlayers - 1;

        public static readonly IEnumerable<string> NETPLAY_SAFE_MAP =  //Not all map are supported in netplay right now... //TODO: it actually is, remove this next time...One day lol
        [
            "SACRED GROUND",
            "TWILIGHT SPIRE",
            "BACKFIRE",
            "FLIGHT",
            "MIRAGE",
            "THORNWOOD",
            "FROSTFANG KEEP",
            "KING'S COURT",
            "SUNKEN CITY",
            "MOONSTONE",
            "TOWERFORGE",
            "ASCENSION",
            "THE AMARANTH",
            "DREADWOOD",
            "DARKFANG",
            "CATACLYSM"
        ];

        public const string RIGHT_STICK_VARIANT_NAME = "RightStickShot";
        public const string RIGHT_STICK_VARIANT_FULLNAME = "TF.EX/RightStickShot";
        public const string RIGHT_STICK_VARIANT_TITLE = "RIGHT STICK SHOT";

        public const string NETPLAY_INPUT_DELAY_TITLE = "NETPLAY INPUT DELAY";
        public const string NETPLAY_USERNAME_TITLE = "NETPLAY USERNAME";
    }
}
