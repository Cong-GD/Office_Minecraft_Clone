using CongTDev.Collection;
using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace CongTDev.AStarPathFinding
{
    public enum SearchState
    {
        Unvisited,
        Open,
        Closed
    }

    public abstract class SearchNode<T> : IComparable<SearchNode<T>>
    {
        public uint G { get; set; }

        public uint H { get; set; }

        public uint F => G + H;

        public SearchState State { get; set; }

        public T Conection { get; set; }

        public int CompareTo(SearchNode<T> other)
        {
            Debug.Log("Compare");
            var compare1 = F.CompareTo(other.F);
            if (compare1 == 0)
            {
                return H.CompareTo(other.H);
            }
            return compare1;
        }
    }

    public interface ISearchContext<Node> where Node : SearchNode<Node>
    {
        Node Start { get; }
        Node End { get; }
        int MaxNeightbours { get; }
        bool IsGoal(Node node);
        uint Cost(Node from, Node to);
        Span<Node> GetNeighbours(Node node, Span<Node> buffer);
        List<Node> GeneratedPath { get; set; }

    }

    public static class AStarPathFinding
    {
        public static void FindPath<Node>(ISearchContext<Node> context) where Node : SearchNode<Node>, IComparable<Node>
        {
            using var timer = TimeExcute.Start("Find a path");
            int count = 0;

            BinaryHeap<Node> openList = ThreadSafePool<BinaryHeap<Node>>.Get();
            openList.Add(context.Start);

            using var pooledBuffer = ArrayPoolHelper.Rent<Node>(context.MaxNeightbours);
            Span<Node> buffer = pooledBuffer.Value;

            while(openList.Count > 0)
            {
                var current = openList.Extract();
                current.State = SearchState.Closed;

                if(context.IsGoal(current))
                {
                    GeneratePath(context.GeneratedPath ,current);
                    break;
                }

                foreach (var neighbour in context.GetNeighbours(current, buffer))
                {
                    if(neighbour.State == SearchState.Closed)
                        continue;

                    var isOpen = neighbour.State == SearchState.Open;
                    var costToNeighbour = current.G + context.Cost(current, neighbour);

                    if(isOpen && costToNeighbour >= neighbour.G)
                        continue;

                    neighbour.Conection = current;
                    neighbour.G = costToNeighbour;

                    if(isOpen)
                    {
                        openList.Update(neighbour);
                    }
                    else
                    {
                        neighbour.H = context.Cost(neighbour ,context.End);
                        neighbour.State = SearchState.Open;
                        openList.Add(neighbour);
                        count++;
                    }
                }
            }

            Debug.Log($"Total node: {count}");
            ThreadSafePool<BinaryHeap<Node>>.Release(openList);
        }

        private static void GeneratePath<Node>(List<Node> path, Node node) where Node : SearchNode<Node>
        {
            path.Clear();
            while (node != null)
            {
                path.Add(node);
                node = node.Conection;
            }
            path.Reverse();
        }
    }
}