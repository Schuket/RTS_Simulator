using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation
{
    public class AStar
    {
        Node origin;
        Node target;
        
        List<Node> OpenList = new List<Node>();
        List<Node> ClosedList = new List<Node>();

        public List<Node> Path = new List<Node>();

        Thread PathThread;
        Steering.OnPathFoundHandler onPathFound;

        public void SearchPath(Vector3 beginPos, Vector3 targetPos, Steering.OnPathFoundHandler onPathFoundEvent)
        {
            TileNavGraph graph = TileNavGraph.Instance;
            origin = graph.GetNode(beginPos);
            target = graph.GetNode(targetPos);

            if (origin == null || target == null)
                return;

            OpenList.Clear();
            ClosedList.Clear();
            Path.Clear();

            onPathFound = onPathFoundEvent;
            PathThread = new Thread(new ThreadStart(FindPath));
            PathThread.Start();
        }

        private void FindPath()
        {
            TileNavGraph graph = TileNavGraph.Instance;

            OpenList.Add(origin);
            Node node = GetTargetedNode();
            int securityNb = 10000;
            while (node != null && securityNb != 0)
            {
                if(Path.IndexOf(node) != -1)
                    break;
                securityNb--;
                Path.Add(node);
                node = node.parent;
            }
            if (securityNb == 0)
                Debug.LogError("No Path Found ...");

            Path.Reverse();
            Reset();
            TileNavGraph.Instance.GuizmoPath = Path;
            TileNavGraph.Instance.OpenList = OpenList;
            TileNavGraph.Instance.ClosedList = ClosedList;

            onPathFound();
        }


        private Node GetTargetedNode()
        {
            while(OpenList.Count != 0)
            {
                Node currNode = GetBestNodeFromOpenList();
                if (currNode == target)
                    return currNode;

                List<Node> neighbours = TileNavGraph.Instance.GetNeighbours(currNode);

                foreach (Node node in neighbours)
                {
                    if (!ClosedList.Contains(node))
                    {
                        if (!OpenList.Contains(node))
                        {
                            OpenList.Add(node);
                            node.Weight = 1000000000;
                        }
                        SetUpNode(node, currNode);
                    }
                }

                OpenList.Remove(currNode);
                ClosedList.Add(currNode);
            }

            Debug.LogError("Can't Find Targeted Node ... ");
            return null;
        }


        private void SetUpNode(Node node, Node parent)
        {
            float newWeight = CalcWeight(node, parent);

            if (newWeight < node.Weight)
            {
                node.Weight = newWeight;
                node.parent = parent;
                node.Euristic = CalcEuristic(node);
            }
        }

        private Node GetBestNodeFromOpenList()
        {
            Node selectedNode = new Node();
            float SmallestEuristic = 10000000f;

            foreach (Node node in OpenList)
            {
                float cost = node.Euristic + node.Weight;

                if (cost < SmallestEuristic)
                {   
                    SmallestEuristic = cost;
                    selectedNode = node;
                }
            }

            return selectedNode;
        }

        float CalcWeight(Node child, Node parent)
        {
            return parent.Weight + Vector3.Distance(parent.Position, child.Position);
        }

        float CalcEuristic(Node node)
        {
            //Euclidian
            return Vector3.Distance(node.Position, target.Position);
            //Manhattan
            //return Mathf.Abs(node.Position.x - target.Position.x) + Mathf.Abs(node.Position.y - target.Position.y);
        }

        void Reset()
        {
            foreach(Node node in ClosedList)
            {
                node.parent = null;
                node.Euristic = -1;
                node.Weight = 0;
            }

            foreach (Node node in OpenList)
            {
                node.parent = null;
                node.Euristic = -1;
                node.Weight = 0;
            }
        }
    }
}
