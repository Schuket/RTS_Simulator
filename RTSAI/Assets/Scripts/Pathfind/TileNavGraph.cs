using UnityEngine;
using System.Collections.Generic;
using System.Threading;

namespace Navigation
{
    public class TileNavGraph : MonoBehaviour
    {
	    static TileNavGraph instance = null;
	    static public TileNavGraph Instance
	    {
		    get
		    {
			    if (instance == null)
				    instance = FindObjectOfType<TileNavGraph>();
			    return instance;
		    }
	    }

        [SerializeField]
        private int GrassCost = 1;
        [SerializeField]
        private int UnreachableCost = int.MaxValue;

        [SerializeField]
        private int GridSizeH = 100;
        [SerializeField]
        private int GridSizeV = 100;
        [SerializeField]
        private int SquareSize = 1;
        [SerializeField]
        private float MaxHeight = 10f;
        [SerializeField]
        private float MaxWalkableHeight = 2.5f;

        // enable / disable debug Gizmos
        [SerializeField]
        private bool DrawGrid = false;
        [SerializeField]
        private bool DisplayAllNodes = false;
        [SerializeField]
        private bool DisplayAllLinks = false;

        private Vector3 gridStartPos = Vector3.zero;
        private int NbTilesH = 0;
        private int NbTilesV = 0;
        private List<Node> LNode = new List<Node>();
        private Dictionary<Node, List<Connection>> connectionsGraph = new Dictionary<Node, List<Connection>>();

        public Dictionary<Node, List<Connection>> ConnectionsGraph { get { return connectionsGraph; } }

        public List<Node> GuizmoPath = new List<Node>();
        public List<Node> OpenList = new List<Node>();
        public List<Node> ClosedList = new List<Node>();

        // threading
        Thread GraphThread = null;

        private void Awake ()
        {
            CreateTiledGrid();
	    }

        private void Start()
        {
            ThreadStart threadStart = new ThreadStart(CreateGraph);
            GraphThread = new Thread(threadStart);
            GraphThread.Start();
        }

        #region graph construction
        // Create all nodes for the tiled grid
        private void CreateTiledGrid()
	    {
		    LNode.Clear();

            gridStartPos = transform.position + new Vector3(- GridSizeH / 2f, 0f, - GridSizeV / 2f);

		    NbTilesH = GridSizeH / SquareSize;
		    NbTilesV = GridSizeV / SquareSize;

		    for(int i = 0; i < NbTilesV; i++)
		    {
			    for(int j = 0; j < NbTilesH; j++)
			    {
				    Node node = new Node();
                    Vector3 nodePos = gridStartPos + new Vector3((j + 0.5f) * SquareSize, 0f, (i + 0.5f) * SquareSize);

				    int Weight = 0;
				    RaycastHit hitInfo = new RaycastHit();

                    // always compute node Y pos from floor collision
                    if (Physics.Raycast(nodePos + Vector3.up * MaxHeight, Vector3.down, out hitInfo, MaxHeight + 1, 1 << LayerMask.NameToLayer("Floor")))
                    {
                        if (Weight == 0)
                            Weight = hitInfo.point.y >= MaxWalkableHeight ? UnreachableCost : GrassCost;
                        nodePos.y = hitInfo.point.y;
                    }

                    if (Weight != UnreachableCost)
                    {
                        // compute speficic area tiles
                        if (RaycastNode(nodePos, "Factory", out hitInfo))
                        {
                            Weight = UnreachableCost;
                        }
                    }

                    node.Weight = Weight;
				    node.Position = nodePos;
				    LNode.Add(node);
			    }
		    }
        }

        // cast a ray for each possible corner of a tile node for better accuracy
        private bool RaycastNode(Vector3 nodePos, string layerName, out RaycastHit hitInfo)
        {
            if (Physics.Raycast(nodePos - new Vector3(0f, 0f, SquareSize / 2f) + Vector3.up * 5, Vector3.down, out hitInfo, MaxHeight + 1, 1 << LayerMask.NameToLayer(layerName)))
                return true;
            else if (Physics.Raycast(nodePos + new Vector3(0f, 0f, SquareSize / 2f) + Vector3.up * 5, Vector3.down, out hitInfo, MaxHeight + 1, 1 << LayerMask.NameToLayer(layerName)))
                return true;
            else if (Physics.Raycast(nodePos - new Vector3(SquareSize / 2f, 0f, 0f) + Vector3.up * 5, Vector3.down, out hitInfo, MaxHeight + 1, 1 << LayerMask.NameToLayer(layerName)))
                return true;
            else if (Physics.Raycast(nodePos + new Vector3(SquareSize / 2f, 0f, 0f) + Vector3.up * 5, Vector3.down, out hitInfo, MaxHeight + 1, 1 << LayerMask.NameToLayer(layerName)))
                return true;
            return false;
        }
        
        // Compute possible connections between each nodes
        private void CreateGraph()
        {
            foreach (Node node in LNode)
            {
                if (IsNodeWalkable(node))
                {
                    connectionsGraph.Add(node, new List<Connection>());
                    foreach (Node neighbour in GetNeighbours(node))
                    {
                        Connection connection = new Connection();
                        connection.Cost = ComputeConnectionCost(node, neighbour);
                        connection.FromNode = node;
                        connection.ToNode = neighbour;
                        connectionsGraph[node].Add(connection);
                    }
                }
            }
        }

        private int ComputeConnectionCost(Node fromNode, Node toNode)
        {
            return (int)(fromNode.Weight + toNode.Weight);
        }

        public List<Node> GetNeighbours(Node node)
        {
            Vector2Int tileCoord = GetTileCoordFromPos(node.Position);
            int x = tileCoord.x;
            int y = tileCoord.y;

            List<Node> nodes = new List<Node>();

            if (x > 0)
            {
                if (y > 0)
                    TryToAddNode(nodes, GetNode(x - 1, y - 1));
                TryToAddNode(nodes, LNode[(x - 1) + y * NbTilesH]);
                if (y < NbTilesV - 1)
                    TryToAddNode(nodes, LNode[(x - 1) + (y + 1) * NbTilesH]);
            }

            if (y > 0)
                TryToAddNode(nodes, LNode[x + (y - 1) * NbTilesH]);
            if (y < NbTilesV - 1)
                TryToAddNode(nodes, LNode[x + (y + 1) * NbTilesH]);

            if (x < NbTilesH - 1)
            {
                if (y > 0)
                    TryToAddNode(nodes, LNode[(x + 1) + (y - 1) * NbTilesH]);
                TryToAddNode(nodes, LNode[(x + 1) + y * NbTilesH]);
                if (y < NbTilesV - 1)
                    TryToAddNode(nodes, LNode[(x + 1) + (y + 1) * NbTilesH]);
            }

            return nodes;
        }

        public bool IsNodeWalkable(Node node)
        {
            return node.Weight < UnreachableCost;
        }

        private void TryToAddNode(List<Node> list, Node node)
        {
            if (IsNodeWalkable(node))
            {
                list.Add(node);
            }
        }
        #endregion

        #region node / pos methods
        public Node GetNode(Vector3 pos)
        {
            return GetNode(GetTileCoordFromPos(pos));
        }

        public bool IsPosValid(Vector3 pos)
        {
            if (GraphThread.ThreadState == ThreadState.Running)
                return false;

            if (pos.x > (-GridSizeH / 2) && pos.x < (GridSizeH / 2) && pos.z > (-GridSizeV / 2) && pos.z < (GridSizeV / 2))
                return true;
            return false;
        }

        // converts world 3d pos to tile 2d pos
        private Vector2Int GetTileCoordFromPos(Vector3 pos)
	    {
            Vector3 realPos = pos - gridStartPos;
            Vector2Int tileCoords = Vector2Int.zero;
            tileCoords.x = Mathf.FloorToInt(realPos.x / SquareSize);
            tileCoords.y = Mathf.FloorToInt(realPos.z / SquareSize);
		    return tileCoords;
	    }

        private Node GetNode(Vector2Int pos)
        {
            return GetNode(pos.x, pos.y);
        }

        private Node GetNode(int x, int y)
        {
            int index = y * NbTilesH + x;
            if (index >= LNode.Count || index < 0)
                return null;

            return LNode[index];
        }

        public Node GetNearestWalkableNodeInDirection(Vector3 position, Vector3 dir)
        {
            Node currNode = GetNode(position);

            List<Node> nodeList = GetNeighbours(currNode);
            for(int i = 1; nodeList.Count == 0; i++)
            {

                Node node = GetNode(position + dir * i);
                if (node != null)
                    nodeList = GetNeighbours(node);
            }
            return nodeList[0];
        }
        #endregion

        #region Gizmos
        private void OnDrawGizmos()
	    {
            if (OpenList.Count > 0)
            {
                for (int i = 0; i < OpenList.Count; i++)
                {
                    Node node = OpenList[i];
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(node.Position, Vector3.one * 0.25f);
                }
            }
            if (ClosedList.Count > 0)
            {
                for (int i = 0; i < ClosedList.Count; i++)
                {
                    Node node = ClosedList[i];
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(node.Position, Vector3.one * 0.25f);
                }
            }

            if (GuizmoPath.Count > 0)
            {
                for (int i = 0; i < GuizmoPath.Count; i++)
                {
                    Node node = GuizmoPath[i];
                    Gizmos.color = Color.blue;
                    Gizmos.DrawCube(node.Position, Vector3.one * 0.25f);
                }
            }
            if (DrawGrid)
            {
                float gridHeight = 0.01f;
                Gizmos.color = Color.yellow;
                for (int i = 0; i < NbTilesV + 1; i++)
                {
                    Vector3 startPos = new Vector3(-GridSizeH / 2f, gridHeight, -GridSizeV / 2f + i * SquareSize);
                    Gizmos.DrawLine(startPos, startPos + Vector3.right * GridSizeV);

                    for (int j = 0; j < NbTilesH + 1; j++)
                    {
                        startPos = new Vector3(-GridSizeH / 2f + j * SquareSize, gridHeight, -GridSizeV / 2f);
                        Gizmos.DrawLine(startPos, startPos + Vector3.forward * GridSizeV);
                    }
                }
            }

            if (DisplayAllNodes)
            {
		        for(int i = 0; i < LNode.Count; i++)
		        {
                    Node node = LNode[i];
                    Gizmos.color = IsNodeWalkable(node) ? Color.green : Color.red;
                    Gizmos.DrawCube(node.Position, Vector3.one * 0.25f);
		        }
            }
            if (DisplayAllLinks)
            {
                foreach (Node crtNode in LNode)
                {
                    if (connectionsGraph.ContainsKey(crtNode))
                    {
                        foreach (Connection c in connectionsGraph[crtNode])
                        {
                            Gizmos.color = Color.green;
                            Gizmos.DrawLine(c.FromNode.Position, c.ToNode.Position);
                        }
                    }
                }
            }
	    }
#endregion
    }
}


