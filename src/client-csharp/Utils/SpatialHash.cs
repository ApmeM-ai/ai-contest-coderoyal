using System.Collections.Generic;
using BrainAI.Pathfinding;

namespace AiCup22.Utils
{
    public class SpatialHash<T>
    {
        private float inverseCellSize;
        private Dictionary<long, List<T>> data = new Dictionary<long, List<T>>();
        private List<T> emptyList = new List<T>();

        public SpatialHash(int cellSize = 100)
        {
            inverseCellSize = 1f / cellSize;
        }

        private Point CellCoords(int x, int y)
        {
            return new Point((int)(x * inverseCellSize), (int)(y * inverseCellSize));
        }

        private Point CellCoords(float x, float y)
        {
            return new Point((int)(x * inverseCellSize), (int)(y * inverseCellSize));
        }

        private List<T> CellAtPosition(int x, int y)
        {
            List<T> cell = null;
            var key = GetKey(x, y);
            if (!data.TryGetValue(key, out cell))
            {
                cell = new List<T>();
                data.Add(key, cell);
            }

            return cell;
        }

        public void Register(T item, int x, int y, int sizeX, int sizeY)
        {
            var p1 = CellCoords(x, y);
            var p2 = CellCoords(x + sizeX, y + sizeY);

            for (var x1 = p1.X; x1 <= p2.X; x1++)
            {
                for (var y1 = p1.Y; y1 <= p2.Y; y1++)
                {
                    CellAtPosition(x1, y1).Add(item);
                }
            }
        }

        public HashSet<T> GetAtPoint(int x, int y)
        {
            var key = GetKey(x, y);
            data.TryGetValue(key, out var cell);
            return new HashSet<T>(cell ?? emptyList);
        }

        public HashSet<T> GetAtRect(int x, int y, int sizeX, int sizeY)
        {
            var result = new HashSet<T>();
            var p1 = CellCoords(x, y);
            var p2 = CellCoords(x + sizeX, y + sizeY);

            for (var x1 = p1.X; x1 <= p2.X; x1++)
            {
                for (var y1 = p1.Y; y1 <= p2.Y; y1++)
                {
                    var key = GetKey(x1, y1);
                    data.TryGetValue(key, out var cell);
                    result.UnionWith(cell ?? emptyList);
                }
            }

            return result;
        }

        public HashSet<T> GetAllObjects()
        {
            var set = new HashSet<T>();

            foreach (var list in data.Values)
            {
                set.UnionWith(list);
            }

            return set;
        }

        public void Remove(T item, int x, int y, int sizeX, int sizeY)
        {
            var p1 = CellCoords(x, y);
            var p2 = CellCoords(x + sizeX, y + sizeY);

            for (var x1 = p1.X; x1 <= p2.X; x1++)
            {
                for (var y1 = p1.Y; y1 <= p2.Y; y1++)
                {
                    CellAtPosition(x1, y1).Remove(item);
                }
            }
        }

        public void RemoveEverywhere(T item)
        {
            foreach (var list in data.Values)
            {
                list.Remove(item);
            }
        }

        public void Clear()
        {
            data.Clear();
        }

        private long GetKey(int x, int y)
        {
            return (long)x << 32 | (uint)y;
        }
    }
}