using System;
using System.Collections.Generic;
using System.Linq;

namespace SuffixTree
{
    class Program
    {
        static int root, blank;
        static List<Vertex> tree;
        static string sample;

        static void Main(string[] args)
        {
            int n = int.Parse(Console.ReadLine()) + 1;
            sample = Console.ReadLine() + "@" + Console.ReadLine() + "[";
            tree = new List<Vertex>();
            BuildTree();
            Stack<Tuple<int, int, bool>> stack = new Stack<Tuple<int,int, bool>>();
            HashSet<int> visited = new HashSet<int>();
            int maxBothTypeDepth = 0;
            int maxBothTypeDepthVertex = 1;
            stack.Push(new Tuple<int, int, bool>(root, 0, false));
            Link[] parLink = new Link[tree.Count - 2];
            while (stack.Count != 0)
            {
                var t = stack.Peek();
                if (t.Item3)
                {
                    stack.Pop();
                    var isFirstType = n*2-t.Item2 < n;
                    tree[t.Item1].FirstType = isFirstType;
                    tree[t.Item1].SecondType = !isFirstType;
                    var par = tree[t.Item1].Parent;
                    if (par != 0)
                        if (isFirstType)
                            tree[par].FirstType = true;
                        else tree[par].SecondType = true;
                    visited.Add(t.Item1);
                }
                else if (visited.Contains(t.Item1))
                {
                    stack.Pop();
                    if (tree[t.Item1].FirstType && tree[t.Item1].SecondType)
                        if (maxBothTypeDepth < t.Item2)
                        {
                            maxBothTypeDepth = t.Item2;
                            maxBothTypeDepthVertex = t.Item1;
                        }
                    var par = tree[t.Item1].Parent;
                    if (par != 0)
                    {
                        if (tree[t.Item1].FirstType)
                            tree[par].FirstType = tree[t.Item1].FirstType;
                        if (tree[t.Item1].SecondType)
                            tree[par].SecondType = tree[t.Item1].SecondType;
                    }
                }
                else
                {
                    foreach (Link l in tree[t.Item1].Links)
                        if (l.To != -1)
                        {
                            bool nextIsLeaf = l.End == int.MaxValue;
                            int lengthNext = nextIsLeaf ? t.Item2 + (sample.Length - l.Start) : t.Item2 + (l.End - l.Start);
                            stack.Push(new Tuple<int, int, bool>(l.To, lengthNext, nextIsLeaf));
                            parLink[l.To - 2] = l;
                        }
                    visited.Add(t.Item1);
                }
            }
            List<string> lcs = new List<string>();
            while(maxBothTypeDepthVertex > 1)
            {
                var substringLength = parLink[maxBothTypeDepthVertex - 2].End - parLink[maxBothTypeDepthVertex - 2].Start;
                lcs.Add(sample.Substring(parLink[maxBothTypeDepthVertex - 2].Start, substringLength));
                maxBothTypeDepthVertex = tree[maxBothTypeDepthVertex].Parent;
            }
            lcs.Reverse();
            if (!tree[root].FirstType || !tree[root].SecondType)
            {
                var maxStart = 0;
                foreach (var l in tree[root].Links)
                    if (l.Start > maxStart)
                        maxStart = l.Start;
                Console.WriteLine(sample.Substring(maxStart, n - maxStart));
            }
            else
                Console.WriteLine(String.Join("", lcs));
        }

        static byte T(int i)
        {
            return (byte)(i < 0 ? -i - 1 : sample[i] - 64);
        }

        static int NewVertex(int parent)
        {
            int i = tree.Count();
            tree.Add(new Vertex(parent));
            return i;

        }

        static void Link(int from, int start, int end, int to)
        {
            tree[from].Links[T(start)] = new Link(start, end, to);
        }

        static ref int F(int v)
        {
            ref int s = ref tree[v].suffix;
            return ref s;
        }

        static void InitTree()
        {
            tree.Clear();
            blank = NewVertex(-1);
            root = NewVertex(0);
            F(root) = blank;
            for (int i = 0; i < 28; i++)
                Link(blank, -i - 1, -i, root);
        }

        static Tuple<int, int> Canonize(int v, int start, int end)
        {
            if (end <= start)
                return new Tuple<int, int>(v, start);
            else
            {
                Link cur = tree[v].Links[T(start)];
                while (end - start >= cur.End - cur.Start)
                {
                    start += cur.End - cur.Start;
                    v = cur.To;
                    if (end > start)
                        cur = tree[v].Links[T(start)];
                }
                return new Tuple<int, int>(v, start);
            }
        }

        static Tuple<bool, int> TestAndSplit(int v, int start, int end, byte c)
        {
            if (end <= start)
                return new Tuple<bool, int>(tree[v].Links[c].To != -1, v);
            else
            {
                Link cur = tree[v].Links[T(start)];
                if (c == T(cur.Start + end - start))
                    return new Tuple<bool, int>(true, v);
                int middle = NewVertex(v);
                tree[cur.To].Parent = middle;
                Link(v, cur.Start, cur.Start + end - start, middle);
                Link(middle, cur.Start + end - start, cur.End, cur.To);
                return new Tuple<bool, int>(false, middle);

            }
        }

        static Tuple<int, int> Update(int v, int start, int end)
        {
            Tuple<bool, int> splitRes;
            int oldR = root;
            splitRes = TestAndSplit(v, start, end, T(end));
            while (!splitRes.Item1)
            {
                Link(splitRes.Item2, end, int.MaxValue, NewVertex(splitRes.Item2));
                if (oldR != root)
                    F(oldR) = splitRes.Item2;
                oldR = splitRes.Item2;
                Tuple<int, int> newPoint = Canonize(F(v), start, end);
                v = newPoint.Item1;
                start = newPoint.Item2;
                splitRes = TestAndSplit(v, start, end, T(end));
            }
            if (oldR != root)
                F(oldR) = splitRes.Item2;
            return new Tuple<int, int>(v, start);
        }

        static void BuildTree()
        {
            InitTree();
            Tuple<int, int> activePoint = new Tuple<int, int>(root, 0);
            for (int i = 0; i < sample.Length; i++)
            {
                activePoint = Update(activePoint.Item1, activePoint.Item2, i);
                activePoint = Canonize(activePoint.Item1, activePoint.Item2, i + 1);
            }
        }
    }

    class Link
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int To { get; set; }

        public Link()
        {
            To = -1;
        }

        public Link(int start, int end, int to)
        {
            Start = start;
            End = end;
            To = to;
        }
    }

    class Vertex
    {
        public Link[] Links { get; set; }
        public int suffix;
        public int Parent { get; set; }
        public bool FirstType { get; set; }
        public bool SecondType { get; set; }

        public Vertex(int parent)
        {
            Links = new Link[28];
            for (int i = 0; i < Links.Length; i++)
            {
                Links[i] = new Link();
                suffix = -1;
            }
            Parent = parent;
        }
    }
}
