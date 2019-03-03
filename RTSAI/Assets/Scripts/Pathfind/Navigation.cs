using UnityEngine;

namespace Navigation
{
    struct Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int _x = 0, int _y = 0) { x = _x; y = _y; }

        static public Vector2Int zero { get { return new Vector2Int(); } }
    }

    public class Node
    {
        public Vector3 Position = Vector3.zero;
        public Node parent = null;
        public float Weight = 0;
        public float Euristic = -1;
    }

    public class Connection
    {
        public int Cost;
        public Node FromNode;
        public Node ToNode;
    }
}
