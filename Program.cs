using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuffixTree
{
    class Program
    {
        static int root, blank;
        static List<Vertex> tree;
        static string sample;

        static void Main(string[] args)
        { 
            sample = Console.ReadLine();
            tree = new List<Vertex>();
            BuildTree();
            int ssc = 0;
            for (int i = 1; i < tree.Count-1; i++)
                for (int j = 0; j < tree[i].Links.Length; j++)
                {
                    if (tree[i].Links[j].To != -1) 
                        if (tree[i].Links[j].End == int.MaxValue)
                            ssc += sample.Length - tree[i].Links[j].Start;
                        else
                            ssc += tree[i].Links[j].End - tree[i].Links[j].Start;
                }
            Console.WriteLine(ssc);
        }

        static byte T(int i)
        {
            return (byte)(i < 0 ? -i - 1 : sample[i]);
        }

        static int NewVertex()
        {
            int i = tree.Count();
            tree.Add(new Vertex());
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
            blank = NewVertex();
            root = NewVertex();
            F(root) = blank;
            for (int i = 0; i < 256; i++)
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
                int middle = NewVertex();
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
                Link(splitRes.Item2, end, int.MaxValue, NewVertex());
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

        public Vertex()
        {
            Links = new Link[256];
            for (int i = 0; i < Links.Length; i++)
            {
                Links[i] = new Link();
                suffix = -1;
            }
        }
    }
}
