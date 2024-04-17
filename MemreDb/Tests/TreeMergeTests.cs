using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemreDb.Memre;
using MemreDb.Memre.Indices;

namespace MemreDb.Tests
{
    static internal class TreeMergeTests
    {
        static void TestTreeMerge()
        {
            ITreeIndexInternals intTree1 = new TreeIndex(true);
            ITreeIndexInternals intTree2 = new TreeIndex(true);
            ITreeIndexInternals intTree3 = new TreeIndex(true);

            for (int n = 10; n < 20; ++n)
            {
                intTree1.Insert(n, $"{n}");
            }
            for (int n = 0; n <= 30; ++n)
            {
                intTree2.Insert(n, $"{n}");
            }
            for (int n = 0; n <= 15; ++n)
            {
                intTree3.Insert((n * 2 + 1), $"{n * 2 + 1}");
            }

            TreeIndex intTree1AsTree = intTree1 as TreeIndex;
            TreeIndex intTree2AsTree = intTree2 as TreeIndex;

            Console.WriteLine("Tree 1");
            intTree1AsTree.WriteTreeToConsole();
            Console.WriteLine("\nTree 2");
            intTree2AsTree.WriteTreeToConsole();
            Console.WriteLine();
            Console.WriteLine();
            intTree1AsTree.WriteTreeAsOrderedListToConsoleUsingIterators(true);

            TreeIndex mergedTree1 = intTree2.SetOperation(intTree1, SetOperation.Or) as TreeIndex;
            Console.WriteLine("Merged OR \n\n");

            mergedTree1.WriteTreeToConsole();
            Console.WriteLine();
            mergedTree1.WriteTreeAsOrderedListToConsole();

            TreeIndex mergedTree2 = intTree3.SetOperation(intTree1, SetOperation.And) as TreeIndex;
            Console.WriteLine("Merged AND \n\n");

            mergedTree2.WriteTreeToConsole();
            Console.WriteLine();
            mergedTree2.WriteTreeAsOrderedListToConsole();
        }
    }
}
