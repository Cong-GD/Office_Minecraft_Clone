using CongTDev.Collection;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;


namespace MyTesting
{
    public class PathFindingTest : MonoBehaviour
    {
        public enum DistanceType
        {
            Euclidean,
            Manhattan,
            SquaredEuclidean
        }

        [Min(0)]
        public int size;

        public int obstaclePercentage;

        public bool stepByStep;

        public float stepTime = 0.1f;

        public DistanceType distanceType;

        public Image nodePrefab;

        public Image[,] nodeImages;
        public TextMeshProUGUI[,] nodesText;
        public BaseNode[,] nodes;

        public Color baseColor;
        public Color searchingColor;
        public Color processedColor;
        public Color startNodeColor;
        public Color endNodeColor;
        public Color pathColor;

        public int2 startNodePos;

        public int2 endNodePos;

        private int2 lastStartNode;
        private int2 lastEndNode;

        private BaseNode startNode;
        private BaseNode endNode;

        private BinaryHeap<BaseNode> openList = new();
        private List<BaseNode> path = new();

        private bool _isFindingPath;
        private Coroutine _findPathCoroutine;

        private void Awake()
        {
            nodeImages = new Image[size, size];
            nodesText = new TextMeshProUGUI[size, size];
            nodes = new BaseNode[size, size];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var node = Instantiate(nodePrefab, transform);
                    nodeImages[x, y] = node;
                    nodesText[x, y] = node.GetComponentInChildren<TextMeshProUGUI>();
                    nodes[x, y] = new BaseNode();
                    nodes[x, y].position = new int2(x, y);
                }
            }
            startNodePos = ClampInMap(startNodePos);
            endNodePos = ClampInMap(endNodePos);
            ResetNodes();
        }

        private void Update()
        {
            startNodePos = ClampInMap(startNodePos);
            endNodePos = ClampInMap(endNodePos);

            if (!startNodePos.Equals(lastStartNode) || !endNodePos.Equals(lastStartNode))
            {
                RefreshVisual();
                lastStartNode = startNodePos;
                lastEndNode = endNodePos;
            }
        }

        public int2 ClampInMap(int2 pos)
        {
            return math.clamp(pos, 0, size - 1);
        }

        public int ClampInMap(int pos)
        {
            return math.clamp(pos, 0, size - 1);
        }

        public bool IsInMap(int2 pos)
        {
            return pos.x >= 0 && pos.x < size && pos.y >= 0 && pos.y < size;
        }

        public bool IsInMap(int pos)
        {
            return pos >= 0 && pos < size;
        }

        public float Distance(int2 a, int2 b)
        {
            return distanceType switch
            {
                DistanceType.Euclidean => math.distance(a, b),
                DistanceType.Manhattan => math.abs(a.x - b.x) + math.abs(a.y - b.y),
                DistanceType.SquaredEuclidean => math.distancesq(a, b),
                _ => throw new NotImplementedException()
            };
        }

        [Button("Reset", EButtonEnableMode.Playmode)]
        public void ResetNodes()
        {
            _isFindingPath = false;
            if (_findPathCoroutine != null)
            {
                StopCoroutine(_findPathCoroutine);
            }
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var node = nodes[x, y];
                    node.g = 0;
                    node.h = 0;
                    node.state = BaseNode.State.None;
                    node.canWalk = true;
                    openList.Clear();
                    path.Clear();
                    node.connection = null;
                }
            }
            RandomizeObstacles();
            RandomizeStartAndEnd();
            RefreshVisual();
        }

        [Button("Restart", EButtonEnableMode.Playmode)]

        public void Restart()
        {
            _isFindingPath = false;
            if (_findPathCoroutine != null)
            {
                StopCoroutine(_findPathCoroutine);
            }
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var node = nodes[x, y];
                    node.g = 0;
                    node.h = 0;
                    node.state = BaseNode.State.None;
                    openList.Clear();
                    path.Clear();
                    node.connection = null;
                }
            }
            RefreshVisual();
        }

        [Button("Randomize Obstacles", EButtonEnableMode.Playmode)]
        public void RandomizeObstacles()
        {
            if (_isFindingPath)
            {
                return;
            }
            var random = new System.Random();

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var randomValue = random.Next(0, 100);
                    if (randomValue < obstaclePercentage)
                    {
                        nodes[x, y].canWalk = false;
                    }
                }
            }
            RefreshVisual();
        }

        [Button("Randomize Start and End", EButtonEnableMode.Playmode)]
        public void RandomizeStartAndEnd()
        {
            if (_isFindingPath)
            {
                return;
            }
            var random = new System.Random();

            startNodePos = new int2(random.Next(0, size), random.Next(0, size));
            endNodePos = new int2(random.Next(0, size), random.Next(0, size));
            nodes[startNodePos.x, startNodePos.y].canWalk = true;
            nodes[endNodePos.x, endNodePos.y].canWalk = true;
            RefreshVisual();
        }

        public void RefreshVisual()
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var node = nodes[x, y];
                    if (node.canWalk)
                    {
                        if (path.Contains(node))
                        {
                            nodeImages[x, y].color = pathColor;
                        }
                        else if (node.state == BaseNode.State.Searching)
                        {
                            nodeImages[x, y].color = searchingColor;
                        }
                        else if (node.state == BaseNode.State.Processed)
                        {
                            nodeImages[x, y].color = processedColor;
                        }
                        else
                        {
                            nodeImages[x, y].color = baseColor;
                        }
                        nodesText[x, y].text = node.ToString();
                    }
                    else
                    {
                        nodeImages[x, y].color = Color.black;
                        nodesText[x, y].text = "X";
                    }

                }
            }
            nodeImages[startNodePos.x, startNodePos.y].color = startNodeColor;
            nodeImages[endNodePos.x, endNodePos.y].color = endNodeColor;
        }


        [Button("Find Path", EButtonEnableMode.Playmode)]
        public void FindPath()
        {
            if (_isFindingPath)
            {
                return;
            }
            _isFindingPath = true;
            _findPathCoroutine = StartCoroutine(CalculatePath());
        }

        private IEnumerator CalculatePath()
        {
            path.Clear();
            startNode = nodes[startNodePos.x, startNodePos.y];
            endNode = nodes[endNodePos.x, endNodePos.y];

            openList.Clear();
            openList.Add(startNode);

            while (openList.Any())
            {
                if (stepByStep)
                {
                    RefreshVisual();
                    yield return Wait.ForSeconds(stepTime);
                }

                var current = openList.Extract();
                current.state = BaseNode.State.Processed;

                if (current == endNode)
                {
                    path = new List<BaseNode>();
                    var currentPathNode = endNode;
                    while (currentPathNode != startNode)
                    {
                        path.Add(currentPathNode);
                        currentPathNode = currentPathNode.connection;
                    }
                    yield break;
                }

                foreach (var neighbour in GetNeighbours(current))
                {
                    if(neighbour.state == BaseNode.State.Processed)
                        continue;

                    var inSearch = neighbour.state == BaseNode.State.Searching;

                    var costToNeightbour = current.g + Distance(current.position ,neighbour.position);

                    if (costToNeightbour < neighbour.g || !inSearch)
                    {
                        neighbour.g = costToNeightbour;
                        neighbour.connection = current;
                        if (!inSearch)
                        {
                            neighbour.h = Distance(neighbour.position ,endNode.position);
                            neighbour.state = BaseNode.State.Searching;
                            openList.Add(neighbour);
                        }
                        else
                        {
                            openList.Update(neighbour);
                        }
                    }
                }
            }
            RefreshVisual();
        }


        private IEnumerable<BaseNode> GetNeighbours(BaseNode currentNode)
        {
            int2 start = ClampInMap(currentNode.position - 1);
            int2 end = ClampInMap(currentNode.position + 1);
            for (int x = start.x; x <= end.x; x++)
            {
                for (int y = start.y; y <= end.y; y++)
                {
                    var node = nodes[x, y];
                    if (!node.Equals(currentNode) && CanWalk(currentNode, node))
                    {
                        yield return node;
                    }
                }
            }
        }

        private bool CanWalk(BaseNode from, BaseNode to)
        {
            if (!to.canWalk)
            {
                return false;
            }
            if (from.position.x == to.position.x || from.position.y == to.position.y)
            {
                return true;
            }
            int2 direction = to.position - from.position;
            int2 left = from.position + new int2(direction.x, 0);
            int2 right = from.position + new int2(0, direction.y);
            return nodes[left.x, left.y].canWalk || nodes[right.x, right.y].canWalk;
        }
    }

    public class BaseNode : IComparable<BaseNode>
    {
        public BaseNode connection;

        public bool canWalk = true;

        public int2 position;

        public float g;
        public float h;
        public float f => g + h;

        public State state { get; set;}

        public enum State
        {
            None, Searching, Processed
        }

        public override string ToString()
        {
            return $"G:{g:0.00}\nH:{h:0.00}\nF:{f:0.00}";
        }

        public int CompareTo(BaseNode other)
        {
            if(other == null)
            {
                return 1;
            }
            int compare = f.CompareTo(other.f);
            if (compare == 0)
            {
                return h.CompareTo(other.h);
            }
            return compare;
        }
    }
}
