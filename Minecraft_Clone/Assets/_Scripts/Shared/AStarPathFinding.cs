using CongTDev.Collection;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CongTDev.AStarPathFinding
{
    public enum SearchState
    {
        Unvisited,
        Open,
        Closed
    }

    public abstract class SearchNode<Node> : IComparable<SearchNode<Node>>
    {
        private uint _g;
        private uint _h;
        private uint _f;

        public uint G
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _g;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _g = value;
                _f = _g + _h;
            }
        }

        public uint H
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _h;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _h = value;
                _f = _g + _h;
            }
        }


        public uint F
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _f;
        }

        public SearchState State
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set;
        }

        public Node Conection
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(SearchNode<Node> other)
        {
            var compare1 = F.CompareTo(other.F);
            return compare1 == 0 ? H.CompareTo(other.H) : compare1;
        }
    }

    public interface ISearchContext<Node> where Node : SearchNode<Node>
    {
        Node Start { get; }
        Node End { get; }
        int MaxNeightbours { get; }
        bool Cancelled { get; }
        bool IsGoal(Node node);
        uint CostBetween(Node from, Node to);
        void PathCompleteAt(Node node);
        ReadOnlySpan<Node> GetNeighbours(Node node, Span<Node> buffer);
    }

    public static class AStarPathFinding
    {
        public static void FindPath<Node>(ISearchContext<Node> context, int maxNodeProcess = 100000)
            where Node : SearchNode<Node>
        {
            using var _ = TimeExcute.Start("Find a path");
            int count = 0;

            BinaryHeap<Node> openList = ThreadSafePool<BinaryHeap<Node>>.Get();
            openList.Clear();
            openList.Add(context.Start);
            using var pooledBuffer = ArrayPoolHelper.Rent<Node>(context.MaxNeightbours, true);
            Span<Node> buffer = pooledBuffer.Value;
            while (openList.TryExtract(out Node current))
            {
                if (++count == maxNodeProcess || context.Cancelled)
                    break;

                current.State = SearchState.Closed;

                if (context.IsGoal(current))
                {
                    context.PathCompleteAt(current);
                    break;
                }

                foreach (Node neighbour in context.GetNeighbours(current, buffer))
                {
                    if (neighbour.State == SearchState.Closed)
                        continue;

                    bool isOpened = neighbour.State == SearchState.Open;
                    uint costToNeighbour = current.G + context.CostBetween(current, neighbour);

                    if (isOpened && costToNeighbour >= neighbour.G)
                        continue;

                    neighbour.Conection = current;
                    neighbour.G = costToNeighbour;

                    if (isOpened)
                    {
                        openList.Update(neighbour);
                    }
                    else
                    {
                        neighbour.H = context.CostBetween(neighbour, context.End);
                        neighbour.State = SearchState.Open;
                        openList.Add(neighbour);
                    }
                }
            }

            Debug.Log($"Total nodes processed: {count}");
            openList.Clear();
            ThreadSafePool<BinaryHeap<Node>>.Release(openList);
        }

    }
}