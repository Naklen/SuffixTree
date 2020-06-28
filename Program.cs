using System;
using System.Collections.Generic;
using System.Linq;

namespace SuffixTree
{
    class Program
    {
        static int root, blank; //Индексы корневой и вспомогательной вершин
        static List<Vertex> tree; //Дерево представляется набором вершин
        static string sample; //Строка, для которой строится дерево

        static void Main(string[] args)
        { 
            sample = Console.ReadLine();
            tree = new List<Vertex>();
            BuildTree();
            PrintTree(root);
            Console.ReadKey();
        }

        //Отдает номер символа в таблице ASCII по заданному индексу
        static byte T(int i) 
        {
            return (byte)(i < 0 ? -i - 1 : sample[i]);
        }

        //Создает новую вершину в суффиксном дереве
        static int NewVertex() 
        {
            int i = tree.Count();
            tree.Add(new Vertex());
            return i;

        }

        //Создает ребро в суффиксном дереве
        //from, to - вершины
        //start, end - позиции начала и конца метки в строке
        static void Link(int from, int start, int end, int to)
        {
            tree[from].Links[T(start)] = new Link(start, end, to);
        }

        //Функция прохождения по суффиксной ссылке
        static ref int F(int v)
        {
            ref int s = ref tree[v].suffix;
            return ref s;
        }

        //Инициализация суффиксного дерева
        //При инициализации создаются две вершины root и 
        //вспомогательная blank(родитель корневой вершины)
        static void InitTree()
        {
            tree.Clear();
            blank = NewVertex();
            root = NewVertex();
            F(root) = blank;
            for (int i = 0; i < 256; i++)
                Link(blank, -i - 1, -i, root);
        }

        //Приводит reference pair к каноническому виду
        //Reference pair позволяет представить некончающийся в листе суффикс
        //в неявном суффиксном дереве
        //Reference pair представляет собой пару вида [вершина, [начало, конец]]
        //Вершина - это вершина, до которой нужно дочитать начало суффикса, а начало и конец - это
        //индексы начала и конца подстроки заданной строки, которые представляют конец суффикса
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

        //Выполняет проверку возможности перехода по символу из состояния [v, [start, end]]
        //по символу с
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

        //Расширение суффиксного дерева
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

        //Построение целого суффиксного дерева
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

        //Вывод представления дерева на консоль
        static void PrintTree(int v, int start = 0, int end = 0, string prefix = "")
        {
            Console.Write(prefix);
            for (int i = start; i < end && i < sample.Length; i++)
                Console.Write((char)T(i));
            if (end == int.MaxValue)
                Console.Write("@");
            Console.Write($" [{v}]");
            if (F(v) != 1)
                Console.Write($" f = {F(v)}");
            Console.WriteLine("");

            for (int i = 0; i < 256; i++)
                if (tree[v].Links[i].To != -1)
                {
                    var link = tree[v].Links[i];
                    PrintTree(link.To, link.Start, link.End, prefix + "   ");
                }
        }
    }

    //Представляет ребро в суффиксном дереве
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

    //Представляет вершину в суффиксном дереве
    class Vertex
    {
        public Link[] Links { get; set; }
        public int suffix;

        public Vertex()
        {
            Links = new Link[256];
            for (int i = 0; i < Links.Length; i++)
                Links[i] = new Link();
            suffix = -1;
        }
    }
}
