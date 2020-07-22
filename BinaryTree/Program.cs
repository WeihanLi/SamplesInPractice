using System;

namespace BinaryTree
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Console.WriteLine("Hello World!");

            var tree = new TreeNode<int>(1)
            {
                Left = new TreeNode<int>(2)
                {
                    Left = new TreeNode<int>(3),
                    Right = new TreeNode<int>(4),
                },
                Right = new TreeNode<int>(5)
                {
                    Right = new TreeNode<int>(6)
                },
            };

            // pre
            Console.WriteLine("-- PreIterate --");
            tree.PreIterate();
            Console.WriteLine();
            // middle
            Console.WriteLine("-- MidIterate --");
            tree.MidIterate();
            Console.WriteLine();
            // post
            Console.WriteLine("-- PostIterate --");
            tree.PostIterate();
            Console.WriteLine();

            Console.ReadLine();
        }
    }

    public class TreeNode<T>
    {
        public TreeNode(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public TreeNode<T> Left { get; set; }

        public TreeNode<T> Right { get; set; }

        public void PreIterate()
        {
            Console.Write(Value);
            Console.Write("-->");
            Left?.PreIterate();
            Right?.PreIterate();
        }

        public void MidIterate()
        {
            Left?.MidIterate();
            Console.Write(Value);
            Console.Write("-->");
            Right?.MidIterate();
        }

        public void PostIterate()
        {
            Left?.PostIterate();
            Right?.PostIterate();
            Console.Write(Value);
            Console.Write("-->");
        }
    }
}
