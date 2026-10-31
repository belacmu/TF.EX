using MonoMod.Utils;
using TF.State.Domain;
using TF.State.Domain.Models;
using TowerFall;

namespace TF.State.TowerFallExtensions
{
    /// <summary>
    /// Mods' entities in the rollback state (ModExports.RegisterEntityStateEvents), carried in the mods' data: one entry per type,
    /// each entity as its depth and its bytes.
    /// </summary>
    public static class ModEntitiesExtensions
    {
        private const string KeyPrefix = "entities:";

        public static void AddModEntitiesState(this GameState gameState, Level level)
        {
            foreach (var events in ServiceCollections.ResolveAPIManager().GetEntityStateEvents())
            {
                var entries = LiveEntities(level, events.Owns)
                    .Select(entity => $"{BitConverter.DoubleToInt64Bits(ActualDepth(entity))}:{Convert.ToBase64String(events.OnSaveState(entity) ?? [])}");

                gameState.AdditionnalData[KeyPrefix + events.Key] = string.Join(";", entries);
            }
        }

        public static void LoadModEntitiesState(this GameState gameState, Level level)
        {
            foreach (var events in ServiceCollections.ResolveAPIManager().GetEntityStateEvents())
            {
                if (!gameState.AdditionnalData.TryGetValue(KeyPrefix + events.Key, out var saved))
                {
                    continue;
                }

                var live = LiveEntities(level, events.Owns);

                foreach (var entry in saved.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var separator = entry.IndexOf(':');
                    var actualDepth = BitConverter.Int64BitsToDouble(long.Parse(entry.Substring(0, separator)));
                    var bytes = Convert.FromBase64String(entry.Substring(separator + 1));

                    var entity = live.FirstOrDefault(candidate => ActualDepth(candidate) == actualDepth);

                    if (entity == null && events.Create != null)
                    {
                        entity = events.Create();
                        level.Spawn(entity, actualDepth);
                    }

                    if (entity != null)
                    {
                        live.Remove(entity);
                        events.OnLoadState(entity, bytes);
                    }
                }

                if (events.Create != null)
                {
                    foreach (var extra in live)
                    {
                        level.RemoveEntity(extra);
                    }
                }
            }
        }

        private static List<Monocle.Entity> LiveEntities(Level level, Func<Monocle.Entity, bool> owns)
        {
            var layer = level.GetGameplayLayer();
            var toAdd = DynamicData.For(layer).Get<List<Monocle.Entity>>("toAdd") ?? [];

            return layer.Entities.Concat(toAdd)
                .Where(entity => owns(entity) && !entity.MarkedForRemoval)
                .Distinct()
                .OrderBy(ActualDepth)
                .ToList();
        }

        private static double ActualDepth(Monocle.Entity entity)
        {
            return DynamicData.For(entity).Get<double>("actualDepth");
        }
    }
}
