using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

class RegionMap {
    List<List<Vector2I>> _regions = new List<List<Vector2I>>();
    Dictionary<Vector2I, int> _regionIndexes = new Dictionary<Vector2I, int>();

    public const int Invalid = -1;

    public static void DoWithWalls(TileMap tileMap, IEnumerable<Vector2I> wallPositions, Action action) {
        var oldValues = wallPositions.ToDictionary(v => v, v => tileMap.GetCell(v.x, v.y));
        foreach (var pair in oldValues)
            tileMap.SetCell(pair.Key.x, pair.Key.y, (int)LevelFile.PlayTile.Wall);
        action();
        foreach (var pair in oldValues)
            tileMap.SetCell(pair.Key.x, pair.Key.y, pair.Value);
    }

    public RegionMap(TileMap tileMap, IEnumerable<Vector2I> wallPositions, Func<TileMap, Vector2I, IEnumerable<Vector2I>> neighbors) {
        DoWithWalls(tileMap, wallPositions, () => {
            HashSet<Vector2I> toVisit = tileMap.GetUsedCells().Cast<Vector2>().Select(v => (Vector2I)v.Floor())
                .Where(v => tileMap.GetCell(v.x, v.y) != (int)LevelFile.PlayTile.Wall).ToHashSet();
            HashSet<Vector2I> valid = toVisit.ToHashSet();
            var bounds = (Rect2I)tileMap.GetUsedRect();

            // Separate positions set to handle going through illegal positions
            HashSet<Vector2I> positions = Enumerable.Range(bounds.Position.y, bounds.Size.y)
                .SelectMany(y => Enumerable.Range(bounds.Position.x, bounds.Size.x).Select(x => new Vector2I(x, y)))
                .ToHashSet();

            while (toVisit.Count > 0) {
                _regions.Add(new List<Vector2I>());
                var stack = new List<Vector2I>(){ toVisit.First() };
                while (Util.TryPop(stack, out var cell)) {
                    if (!positions.Remove(cell))
                        continue;
                    toVisit.Remove(cell);
                    if (valid.Contains(cell)) {
                        _regions.Last().Add(cell);
                        _regionIndexes[cell] = _regions.Count - 1;
                    }
                    stack.AddRange(neighbors(tileMap, cell).Where(v => bounds.HasPoint(v)));
                }
            }

            //GD.Print("");
            //foreach (var region in _regions) {
            //    GD.Print(string.Join(", ", region.Select(v => $"{v}")));
            //}
        });
    }

    public IEnumerable<Vector2I> RegionTiles(int index) {
        return _regions[index];
    }

    public int Region(Vector2I pos) {
        if (_regionIndexes.TryGetValue(pos, out var region))
            return region;
        return Invalid;
    }
}