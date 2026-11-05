using Microsoft.Xna.Framework;
using Monocle;
using TF.EX.Domain.Extensions;
using TF.EX.Domain.Models;
using TowerFall;

namespace TF.EX.Domain.CustomComponent
{
    public class LocalPlayersPanel : Entity
    {
        private const float PANEL_X = 24f;
        private const float PANEL_Y = 150f;
        private const float HINTS_RIGHT_X = 316f;
        private const float SLOT_SPACING = 28f;
        private const int SLOTS_PER_COLUMN = 4;
        private const float COLUMN_SPACING = 44f;
        private const float LABEL_OFFSET = 12f;
        private const float ICON_OFFSET = 12f;
        private const float HINT_LINE_OFFSET = 6f;
        private const float GAP = 3f;
        private const string JOIN_HINT = "JOIN";
        private const string SWAP_HINT = "SWAP";
        private const string QUIT_HINT = "QUIT";

        private static readonly Color GuestColor = Calc.HexToColor("FFDC6B");

        private static LocalPlayersPanel current;

        private readonly MainMenu menu;
        private float playersAlpha;
        private float hintsAlpha;

        private LocalPlayersPanel(MainMenu menu) : base(new Vector2(PANEL_X, PANEL_Y))
        {
            this.menu = menu;
        }

        public static void Ensure(MainMenu menu)
        {
            if (current?.menu == menu)
            {
                return;
            }

            current = new LocalPlayersPanel(menu);
            menu.Add(current);
        }

        public static bool CanJoin(MainMenu menu)
        {
            return menu.State.ToDomainModel() == MenuState.NetplaySelect && menu.CanAct;
        }

        public override void Update()
        {
            base.Update();

            var inputService = ServiceCollections.ResolveInputService();
            var isNetplayScreen = menu.State.ToDomainModel().IsNetplayScreen() && ServiceCollections.ResolveMatchmakingService().GetOwnLobby().IsEmpty;
            var canJoin = isNetplayScreen && CanJoin(menu);
            var hasGuests = inputService.GetGuestInputs().Count > 0;
            var hasJoinable = canJoin && inputService.GetJoinableDevices().Any();

            playersAlpha = CalcApproach(playersAlpha, isNetplayScreen && (hasGuests || hasJoinable));
            hintsAlpha = CalcApproach(hintsAlpha, canJoin && (hasGuests || hasJoinable));
        }

        private static float CalcApproach(float alpha, bool isShown)
        {
            return Calc.Approach(alpha, isShown ? 1f : 0f, 0.08f * Engine.TimeMult);
        }

        public static void UpdateGuests(Ports.TF.IInputService inputService)
        {
            foreach (var guest in inputService.GetGuestInputs().ToList())
            {
                if (!guest.Attached || guest.MenuBack)
                {
                    inputService.RemoveGuestInput(guest);
                    Sounds.ui_clickBack.Play();
                }
            }

            var claiming = inputService.GetJoinableDevices().FirstOrDefault(input => input.MenuAlt2);

            if (claiming != null)
            {
                inputService.ClaimLocalInput(claiming);
                Sounds.ui_click.Play();
                return;
            }

            if (inputService.GetLocalPlayerCount() < Constants.LobbyMaxLocalPlayers)
            {
                var joining = inputService.GetJoinableDevices().FirstOrDefault(input => input.MenuStart);

                if (joining != null && inputService.TryAddGuestInput(joining))
                {
                    Sounds.ui_click.Play();
                }
            }
        }

        public override void Render()
        {
            var inputService = ServiceCollections.ResolveInputService();
            var guests = inputService.GetGuestInputs();

            if (playersAlpha > 0f)
            {
                RenderPlayers(inputService.GetLocalInput(), guests);
            }

            if (hintsAlpha > 0f)
            {
                RenderHints(inputService.GetJoinableDevices().ToList(), guests);
            }
        }

        private void RenderPlayers(PlayerInput owner, IReadOnlyList<PlayerInput> guests)
        {
            var columns = (guests.Count + SLOTS_PER_COLUMN) / SLOTS_PER_COLUMN;

            DrawPlayer(1, owner?.Icon, Color.White, columns);

            for (int i = 0; i < guests.Count; i++)
            {
                DrawPlayer(i + 2, guests[i].Icon, GuestColor, columns);
            }
        }

        private void DrawPlayer(int number, Subtexture icon, Color color, int columns)
        {
            var x = Position.X - (columns - 1 - (number - 1) / SLOTS_PER_COLUMN) * COLUMN_SPACING;
            var y = Position.Y + (number - 1) % SLOTS_PER_COLUMN * SLOT_SPACING;
            Draw.OutlineTextCentered(TFGame.Font, $"P{number}", new Vector2(x - LABEL_OFFSET, y), color * playersAlpha, Color.Black * playersAlpha);

            if (icon != null)
            {
                Draw.TextureCentered(icon, new Vector2(x + ICON_OFFSET, y), Color.White * playersAlpha);
            }
        }

        private void RenderHints(List<PlayerInput> joinable, IReadOnlyList<PlayerInput> guests)
        {
            var y = Position.Y;

            if (joinable.Count > 0)
            {
                DrawHint("P1", DistinctTextures(joinable.Select(input => input.Alt2Icon)), SWAP_HINT, Color.Gray, y);
            }

            if (guests.Count > 2)
            {
                y += SLOT_SPACING;
                DrawHint($"P2-P{guests.Count + 1}", DistinctTextures(guests.Select(guest => guest.BackIcon)), QUIT_HINT, GuestColor, y);
            }
            else
            {
                for (int i = 0; i < guests.Count; i++)
                {
                    y += SLOT_SPACING;
                    DrawHint($"P{i + 2}", [guests[i].BackIcon], QUIT_HINT, GuestColor, y);
                }
            }

            if (guests.Count + 2 <= Constants.LobbyMaxLocalPlayers && joinable.Count > 0)
            {
                y += SLOT_SPACING;
                DrawHint($"P{guests.Count + 2}", DistinctTextures(joinable.Select(input => input.StartIcon)), JOIN_HINT, Color.Gray, y);
            }
        }

        private static List<Subtexture> DistinctTextures(IEnumerable<Subtexture> icons)
        {
            return [.. icons.Where(icon => icon != null).Distinct()];
        }

        private void DrawHint(string label, IReadOnlyList<Subtexture> icons, string action, Color color, float y)
        {
            var text = $"{label} {action}";
            var textWidth = TFGame.Font.MeasureString(text).X;
            Draw.OutlineTextCentered(TFGame.Font, text, new Vector2(HINTS_RIGHT_X - textWidth / 2f, y - HINT_LINE_OFFSET), color * hintsAlpha, Color.Black * hintsAlpha);

            var x = HINTS_RIGHT_X - icons.Sum(icon => icon.Width + GAP) + GAP;

            foreach (var icon in icons)
            {
                Draw.TextureCentered(icon, new Vector2(x + icon.Width / 2f, y + HINT_LINE_OFFSET), Color.White * hintsAlpha);
                x += icon.Width + GAP;
            }
        }
    }
}
