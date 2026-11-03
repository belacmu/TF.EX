using TF.EX.Domain.Models;

namespace TF.EX.Domain.Extensions
{
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

        public static string ToLabel(this GameLogicSpeedMode mode)
        {
            return mode switch
            {
                GameLogicSpeedMode.Hz240 => "240 HZ",
                GameLogicSpeedMode.Hz60 => "60 HZ",
                _ => "BEST",
            };
        }
    }
}
