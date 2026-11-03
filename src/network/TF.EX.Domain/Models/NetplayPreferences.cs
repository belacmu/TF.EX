namespace TF.EX.Domain.Models
{
    public enum AutoAdjustInputDelayMode
    {
        Disabled,
        Propose,
        Enabled,
    }

    public enum CustomSkinMode
    {
        Disabled,
        Full,
    }

    public enum GameLogicSpeedMode
    {
        Best,
        Hz240,
        Hz60,
    }

    public static class GameLogicSpeedModeExtensions
    {
        public static int? FixedTickRate(this GameLogicSpeedMode mode)
        {
            return mode switch
            {
                GameLogicSpeedMode.Hz240 => Constants.NETPLAY_FPS,
                GameLogicSpeedMode.Hz60 => Constants.VANILLA_FPS,
                _ => null,
            };
        }

        public static GameLogicSpeedMode ToGameLogicSpeedMode(this string value)
        {
            return Enum.TryParse<GameLogicSpeedMode>(value, out var mode) ? mode : GameLogicSpeedMode.Hz240;
        }
    }

    public static class NetplayPreferences
    {
        public const int MinInputDelay = 0;
        public const int MaxInputDelay = 332;
        public const int InputDelayStep = 4;
        public const int MinAcceptedRollbackFrames = 0;
        public const int MaxAcceptedRollbackFrames = 6;
        public const double RollbackFrameMs = 1000.0 / 60;
        public const int MaxNameLength = 10;
        public const string OfficialServer = "wss://tfex-server.balatro-vs-matchmaking.eu";
        public const string LocalServer = "ws://127.0.0.1:3000";

        public static int InputDelay = 20;
        public static int AcceptedRollbackFrames = 2;
        public static string Name = "PLAYER";
        public static string PlayerId = "";
        public static string Server = OfficialServer;
        public static AutoAdjustInputDelayMode AutoAdjustInputDelay = AutoAdjustInputDelayMode.Propose;
        public static CustomSkinMode CustomSkins = CustomSkinMode.Full;
        public static bool AutoUpdate = true;
        public static GameLogicSpeedMode GameLogicSpeed = GameLogicSpeedMode.Best;

        public static bool IsOfficialServer => IsOfficial(Server);

        public static int InputDelayFrames(int fps) => ToFrames(InputDelay, fps);

        public static int ToFrames(int ms, int fps) => (ms * fps + 999) / 1000;

        public static bool IsOfficial(string server)
        {
            return server.Equals(OfficialServer);
        }
    }
}
