﻿using System;
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
            Console.ReadLine();
            sample = Console.ReadLine();
            var secondStr = Console.ReadLine();
            tree = new List<Vertex>();
            BuildTree();
            var lcs = "";
            for (int i = 0; i < secondStr.Length; i++)
            {
                var commonString = new List<char>();
                var suffix = secondStr.Substring(i);
                var currentVertex = root;
                int index = 0;
                Link currentLink = tree[currentVertex].Links[(byte)suffix[index] - 65];
                while(true)
                {
                    if (currentLink.To == -1)
                        break;
                    for (int j = currentLink.Start; j < currentLink.End; j++)
                    {
                        if (sample[j] == suffix[index])
                            commonString.Add(sample[j]);
                        else 
                            goto End;
                        if (index == suffix.Length - 1)
                            goto End;
                        index++;
                        if (j == currentLink.End - 1 || j == sample.Length - 1)
                        {
                            currentVertex = currentLink.To;
                            currentLink = tree[currentVertex].Links[(byte)suffix[index] - 65];
                            j = currentLink.Start - 1;
                        }
                    }
                }
            End:
                var candidate = string.Join("", commonString);
                if (lcs.Length < candidate.Length)
                    lcs = candidate;
            }
            Console.WriteLine(lcs);
        }        

        static byte T(int i)
        {
            return (byte)(i < 0 ? -i - 1 : sample[i] - 65);
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
            for (int i = 0; i < 26; i++)
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

        public Vertex(int parent)
        {
            Links = new Link[26];
            for (int i = 0; i < Links.Length; i++)
            {
                Links[i] = new Link();
                suffix = -1;
            }           
        }
    }
}