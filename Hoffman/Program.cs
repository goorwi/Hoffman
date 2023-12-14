using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Hoffman
{
    public class Node
    {
        public Node(double prob, int code, Node next) 
        {
            //this.prev = prev; 
            this.prob = prob;
            this.code = code;
            this.next = next;
        }
        //public Node* prev;
        public bool isMerged = false;
        public bool isSummed = false;
        public double prob;
        public int code;
        public Node next;
    }
    unsafe internal class Program
    {
        private static int N, M;
        private static List<Node> firstList;
        private static List<List<Node>> tree = new List<List<Node>>();
        static void Main(string[] args)
        {
            List<double> prob = readDataFromFile();
            List<Node> secondIter = new List<Node>();

            if (prob != null)
            {
                firstList = initNodes(prob);
                firstIteration(ref firstList, ref secondIter);
                tree.Add(firstList);
                tree.Add(secondIter);
            }
            do
            {
                secondIter = iteration(secondIter);
                tree.Add(secondIter);
            } while (secondIter.Count > 1);
            

            print();
            getWords();
            Console.ReadKey();
        }

        private static void print()
        {
            for (int i = 0, j = 0; j < tree[0].Count; j++)
            {
                while (i < tree.Count && j < tree[i].Count)
                {
                    if (tree[i][j].isSummed == true && tree[i][j].isMerged == true) Console.Write($"->{tree[i][j].prob}-\t");
                    else if (tree[i][j].isSummed == true) Console.Write($"{tree[i][j].prob}-\t");
                    else if (tree[i][j].isMerged == true) Console.Write($"->{tree[i][j].prob}\t");
                    else Console.Write($"{tree[i][j].prob}\t");
                    i++;
                }
                Console.WriteLine();
                i = 0;
            }
            Console.WriteLine();
        }

        private static List<Node> iteration(List<Node> nodes)
        {
            int countProbs = M;
            List<Node> newListOfNodes = new List<Node>();
            for (int i = 0; i < nodes.Count - countProbs; i++)
            {
                Node n = new Node(nodes[i].prob, nodes[i].code, null);
                nodes[i].next = n;
                newListOfNodes.Add(n);
            }

            List<Node> currentNodes = new List<Node>();
            for (int i = nodes.Count - 1, j = 0; j < countProbs; i--, j++)
                currentNodes.Add(new Node(nodes[i].prob, nodes[i].code, null));

            double sumOfNodes = 0;
            foreach (Node curNode in currentNodes)
                sumOfNodes += curNode.prob;
            Node newNode = new Node(sumOfNodes, -1, null);
            newNode.isMerged = true;

            int count = 0;
            for (int i = nodes.Count - countProbs; i < nodes.Count; i++)
            {
                nodes[i].next = newNode;
                nodes[i].code = count++;
                nodes[i].isSummed = true;
            }

            newListOfNodes.Add(newNode);
            newListOfNodes.Sort((a, b) => b.prob.CompareTo(a.prob));

            return newListOfNodes;
        }

        private static void firstIteration(ref List<Node> previous, ref List<Node> newList)
        {
            int countProbs = ((N - 1) % (M - 1));
            if (countProbs == 0) countProbs = M;
            else countProbs++;

            if (countProbs == 1) countProbs++;
            
            for (int i = 0; i < previous.Count - countProbs; i++)
            {
                Node n = new Node(previous[i].prob, previous[i].code, null);
                previous[i].next = n;
                newList.Add(n);
            }

            List<Node> currentNodes = new List<Node>();
            for (int i = previous.Count - 1, j = 0; j < countProbs; i--, j++)
                currentNodes.Add(new Node(previous[i].prob, previous[i].code, null));

            double sumOfNodes = 0;
            foreach (Node curNode in currentNodes)
            {
                sumOfNodes += curNode.prob;
            }
            Node newNode = new Node(sumOfNodes, -1, null);
            newNode.isMerged = true;

            int count = 0;
            for (int i = previous.Count - countProbs; i < previous.Count; i++)
            {
                previous[i].next = newNode;
                previous[i].code = count++;
                previous[i].isSummed = true;
            }

            newList.Add(newNode);
            newList.Sort((a, b) => b.prob.CompareTo(a.prob));
        }

        private static List<Node> initNodes(List<double> prob)
        {
            List<Node> nodes = new List<Node>();
            for (int i = 0; i < prob.Count; i++)
                nodes.Add(new Node(prob[i], -1, null));
            return nodes;
        }

        private static List<double> readData()
        {
            Console.Write("Введите количество вероятностей: ");
            N = int.Parse(Console.ReadLine());
            Console.WriteLine("Введите последовательно элементы: ");
            List<double> prob = new List<double>();
            for (int i = 0; i < N; i++)
                prob.Add(double.Parse(Console.ReadLine()));

            prob.Sort((x,y) => y.CompareTo(x));

            Console.Write("Введите количество букв в кодировке: ");
            M = int.Parse(Console.ReadLine());

            if (prob.Sum() != 1)
            {
                Console.WriteLine("Сумма введённой последовательности не равна 1! Попробуйте снова.");
                return null;
            }
            else return prob;
        }

        private static List<double> readDataFromFile()
        {
            using (StreamReader sr = new StreamReader("D:\\Методы оптимизации\\Hoffman\\Hoffman\\input.txt"))
            {
                //количество вероятностей
                N = int.Parse(sr.ReadLine());

                List<double> prob = new List<double>();
                for (int i = 0; i < N; i++)
                    prob.Add(double.Parse(sr.ReadLine()));

                prob.Sort((x, y) => y.CompareTo(x));

                //количество букв в кодировке
                M = int.Parse(sr.ReadLine());

                if (prob.Sum() != 1)
                {
                    Console.WriteLine("Сумма последовательности не равна 1! Попробуйте снова.");
                    return null;
                }
                else return prob;
            }
        }

        private static void getWords()
        {
            List<Node> firstPosition = tree[0];

            List<String> words = new List<String>();

            double length = 0;

            foreach (Node node in firstPosition)
            {
                Node iterator = node;
                String word = "";
                while (iterator != null)
                {
                    if (iterator.code != -1)
                        word += iterator.code.ToString();
                    iterator = iterator.next;
                }
                length += word.Length * node.prob;
                word = String.Concat(word.Reverse());
                words.Add(word);
            }

            char letter = 'a';
            foreach (String word in words)
            {
                Console.WriteLine($"a({letter++}) = {word}");
            }
            Console.WriteLine($"lср = {length}");
        }
    }
}
