using Microsoft.Xna.Framework;
using Monocle;
using TF.EX.Domain.Extensions;
using TF.EX.Domain.Models;
using TF.EX.Domain.Models.WebSocket;
using TF.EX.Domain.Utils;
using TowerFall;

namespace TF.EX.Domain.CustomComponent
{
    public class LobbyPanel : Entity
    {
        private Image block;

        private Image icon;

        private List<Image> variantsImages = new List<Image>();

        private OutlineText _title;

        private OutlineText _mode;

        private Vector2 initialPosition;

        private MatchSettings.MatchLengths matchLength = MatchSettings.MatchLengths.Standard;

        private Color lengthColor = Calc.HexToColor("3CBCFC");
        private int tickRate = Constants.NETPLAY_FPS;

        private string[] lengthNames = new string[4] { "INSTANT MATCH", "QUICK MATCH", "STANDARD MATCH", "EPIC MATCH" };

        public Vector2 InitialPosition => initialPosition;

        private OutlineText cantJoinReason;

        private OutlineText liveIndicator;

        private OutlineText seriesIndicator;

        public LobbyPanel(float x, float y) : base(-1)
        {
            Position = new Vector2(x, y);
            initialPosition = Position;
        }

        public override void Render()
        {
            if (TFGame.Instance.Scene != null && TFGame.Instance.Scene is MainMenu && (TFGame.Instance.Scene as MainMenu).State != TF.EX.Domain.Models.MenuState.LobbyBrowser.ToTFModel())
            {
                return;
            }

            var position = new Vector2(Position.X, Position.Y);
            position.Y -= 10;

            var length = lengthNames[(int)matchLength];
            var speed = $"{tickRate} HZ";
            var gap = 6f;
            var lengthWidth = TFGame.Font.MeasureString(length).X;
            var speedWidth = TFGame.Font.MeasureString(speed).X;
            var left = position.X - (lengthWidth + gap + speedWidth) / 2f;
            var y = position.Y - 6f;

            Draw.OutlineTextCentered(TFGame.Font, length, new Vector2(left + lengthWidth / 2f, y), lengthColor, 1f);
            Draw.OutlineTextCentered(TFGame.Font, speed, new Vector2(left + lengthWidth + gap + speedWidth / 2f, y), SpeedColor(tickRate), 1f);

            base.Render();
        }

        public override void Removed()
        {
            base.Removed();
            RemoveComponents();
        }

        private void RemoveComponents()
        {
            if (block != null)
            {
                Remove(block);
            }

            if (icon != null)
            {
                Remove(icon);
            }

            if (_title != null)
            {
                Remove(_title);
            }

            if (_mode != null)
            {
                Remove(_mode);
            }

            foreach (var variant in variantsImages)
            {
                Remove(variant);
            }

            if (cantJoinReason != null)
            {
                Remove(cantJoinReason);
            }

            if (liveIndicator != null)
            {
                Remove(liveIndicator);
                liveIndicator = null;
            }

            if (seriesIndicator != null)
            {
                Remove(seriesIndicator);
                seriesIndicator = null;
            }

            variantsImages.Clear();
        }

        public void UpdateInfo(Lobby lobby)
        {
            RemoveComponents();

            UpdateMapIcon(lobby.GameData.MapId);
            UpdateMode(lobby.GameData.Mode);
            UpdateTitle(lobby.GameData.MapId);

            tickRate = lobby.TickRate;

            var (canJoin, reason) = (lobby.CanJoin, lobby.CanNotJoinReason);

            if (!canJoin && lobby.MissingVariants.Count == 0)
            {
                cantJoinReason = new OutlineText(TFGame.Font, reason)
                {
                    Scale = Vector2.One * 1.3f,
                    Color = Color.Red,
                    OutlineColor = Color.Black
                };

                cantJoinReason.Position.Y += 5;

                Add(cantJoinReason);
                return;
            }

            if (lobby.InGame || lobby.IsSeriesInProgress)
            {
                liveIndicator = new OutlineText(TFGame.Font, lobby.InGame ? "LIVE, SPECTATE ONLY" : "SERIES LIVE, SPECTATE ONLY")
                {
                    Scale = Vector2.One * 1.3f,
                    Color = Color.Yellow,
                    OutlineColor = Color.Black
                };

                liveIndicator.Position.Y += 20;

                Add(liveIndicator);
            }
            else if (lobby.TickRateWith(NetplayPreferences.GameLogicSpeed) != tickRate)
            {
                liveIndicator = new OutlineText(TFGame.Font, $"JOINING SETS {lobby.TickRateWith(NetplayPreferences.GameLogicSpeed)} HZ")
                {
                    Scale = Vector2.One * 1.3f,
                    Color = Color.Yellow,
                    OutlineColor = Color.Black
                };

                liveIndicator.Position.Y += 20;

                Add(liveIndicator);
            }

            if (lobby.IsSeriesLobby)
            {
                seriesIndicator = new OutlineText(TFGame.Font, $"BEST OF {lobby.GameData.BestOf} SERIES")
                {
                    Color = Color.Gold,
                    OutlineColor = Color.Black
                };

                seriesIndicator.Position.Y += 34;

                Add(seriesIndicator);
            }

            UpdateVariant(lobby.GameData.Variants);
            UpdateMatchLength(lobby.GameData.MatchLength);
        }

        public static Color SpeedColor(int tickRate) => tickRate == Constants.NETPLAY_FPS ? Color.WhiteSmoke : Color.Orange;

        private void UpdateMatchLength(int matchLength)
        {
            this.matchLength = (MatchSettings.MatchLengths)matchLength;
        }

        private void UpdateMapIcon(int mapId)
        {
            var imgs = mapId == -1 ? MapButton.InitRandomVersusGraphics() : VersusGraphics.GetVersusGraphics(mapId);

            if (block != null)
            {
                block.RemoveSelf();
            }

            if (icon != null)
            {
                icon.RemoveSelf();
            }

            block = imgs[0];
            icon = imgs[1];

            block.Position.Y -= 60;
            icon.Position.Y -= 60;

            Add(block);
            Add(icon);
        }


        private void UpdateTitle(int mapId)
        {
            var towerName = mapId == -1 ? "RANDOM" : TowerFall.GameData.VersusTowers[mapId].Theme.Name;

            _title = new OutlineText(TFGame.Font, towerName)
            {
                Scale = Vector2.One * 1.4f,
                Color = Color.WhiteSmoke,
                OutlineColor = Color.Black
            };

            _title.Position.Y -= 38;

            Add(_title);
        }

        private void UpdateMode(int mode)
        {
            var modeEnum = (TowerFall.Modes)mode;

            switch (modeEnum)
            {
                case Modes.LastManStanding:
                    _mode = new OutlineText(TowerFall.TFGame.Font, "LAST MAN STANDING");
                    _mode.Color = QuestDifficultySelect.LegendaryColor;
                    _mode.OutlineColor = Color.Transparent;
                    Add(_mode);
                    break;
                case Modes.HeadHunters:
                    _mode = new OutlineText(TFGame.Font, "HEADHUNTERS");
                    _mode.Color = QuestDifficultySelect.HardcoreColor;
                    _mode.OutlineColor = Color.Transparent;
                    Add(_mode);
                    break;
                case Modes.TeamDeathmatch:
                    _mode = new OutlineText(TFGame.Font, "TEAM DEATHMATCH");
                    _mode.Color = QuestDifficultySelect.NormalColor;
                    _mode.OutlineColor = Color.Transparent;
                    Add(_mode);
                    break;
                default:
                    _mode = new OutlineText(TFGame.Font, "LAST MAN STANDING ?");
                    _mode.Color = QuestDifficultySelect.LegendaryColor;
                    _mode.OutlineColor = Color.Transparent;
                    Add(_mode);
                    break;
            }

            _mode.Position.Y -= 30;
        }


        private void UpdateVariant(ICollection<string> variants)
        {
            var startX = -40;
            var startY = 0;

            foreach (string var in variants)
            {
                var variant = MainMenu.VersusMatchSettings.Variants.Variants.FirstOrDefault(v => v.Title == var);

                var sub = variant != null ? variant.Icon : TFGame.MenuAtlas["controls/keyboard/undefined"];

                var img = new Image(sub);
                img.CenterOrigin();
                img.Position.X = startX;
                img.Position.Y = startY;

                Add(img);

                variantsImages.Add(img);

                startX += 20;

                if (variantsImages.Count % 5 == 0)
                {
                    startX = -40;
                    startY += 20;
                }
            }
        }
    }

}

