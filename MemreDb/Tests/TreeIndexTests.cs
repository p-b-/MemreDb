using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemreDb.Memre;
using MemreDb.Memre.Indices;
using MemreDb.Memre.Indices.Iterators;

namespace MemreDb.Tests
{
    static internal class TreeIndexTests
    {
        static internal void TestMultiLeafIterator()
        {
            ITreeIndexInternals tree = new TreeIndex(false);

            tree.Insert(1, "1");
            tree.Insert(1, "1ish");
            tree.Insert(1, "1 as well");
            tree.Insert(2, "2");
            tree.Insert(3, "3");
            tree.Insert(8, "8");
            tree.Insert(10, "10");
            tree.Insert(10, "0x0a");
            tree.Insert(8, "0x08");

            ITreeIndexIterator iterator = (tree as TreeIndex).GetTreeIndexIterator();

            while(iterator.Valid)
            {
                Console.WriteLine($"Index {iterator.CurrentIndex} Value {iterator.CurrentValue}");
                iterator.MoveNext();
            }
        }

        static internal void TestTreeIndex()
        {
            ITreeIndexInternals tree = new TreeIndex(true);

            tree.Insert(50, "Fifty");
            tree.Insert(45, "Forty-five");
            tree.Insert(25, "Twenty-five");
            tree.Insert(48, "Forty-eight");
            tree.Insert(75, "Seventy-five");
            tree.Insert(30, "Thirty");
            tree.Insert(49, "Forty-nine");

            TreeIndex ti = tree as TreeIndex;

            Console.WriteLine($"75 = {ti.GetValueSingular(75)}");
            Console.WriteLine($"45 = {ti.GetValueSingular(45)}");
            Console.WriteLine($"25 = {ti.GetValueSingular(25)}");
            Console.WriteLine($"48 = {ti.GetValueSingular(48)}");
            Console.WriteLine($"30 = {ti.GetValueSingular(30)}");
            Console.WriteLine($"49 = {ti.GetValueSingular(49)}");
            Console.WriteLine($"50 = {ti.GetValueSingular(50)}");

            ITreeIndexInternals stringTree = new TreeIndex(true);
            stringTree.Insert("Thirty", 30);
            stringTree.Insert("Twenty", 20);
            stringTree.Insert("Eighteen", 18);
            stringTree.Insert("Ninety", 90);
            stringTree.Insert("Four", 4);
            stringTree.Insert("Five", 5);

            TreeIndex stringTreeAsTree = stringTree as TreeIndex;

            Console.WriteLine($"Thirty = {stringTreeAsTree.GetValueSingular("Thirty")}");
            Console.WriteLine($"Twenty = {stringTreeAsTree.GetValueSingular("Twenty")}");
            Console.WriteLine($"Eighteen = {stringTreeAsTree.GetValueSingular("Eighteen")}");
            Console.WriteLine($"Ninety = {stringTreeAsTree.GetValueSingular("Ninety")}");
            Console.WriteLine($"Four = {stringTreeAsTree.GetValueSingular("Four")}");
            Console.WriteLine($"Five= {stringTreeAsTree.GetValueSingular("Five")}");

            ITreeIndexInternals intTree2 = new TreeIndex(true);
            intTree2.Insert(50, "Fifty");
            intTree2.Insert(40, "Forty");
            intTree2.Insert(75, "Seventy-five");
            intTree2.Insert(35, "Thirty-five");
            intTree2.Insert(47, "Forty-seven");
            intTree2.Insert(49, "Forty-nine");
            intTree2.Insert(42, "Forty-two");
            intTree2.Insert(70, "Seventy");
            intTree2.Insert(80, "Eighty");
            intTree2.Insert(33, "Thirty-three");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Sorting");
            Console.WriteLine("=======");
            Console.WriteLine();

            (intTree2 as TreeIndex).WriteTreeToConsole();

            TreeIndex returnedTree;

            ITreeNodeIterator iterator = intTree2.GetIterator();
            while (iterator.Node != null)
            {
                int compareTo = (int)iterator.Node.IndexValue;
                string equalsSign = "=";

                for (int i = 0; i < 4; ++i)
                {
                    IncludeEquals equals = ((i % 2) == 0) ? IncludeEquals.Include : IncludeEquals.DoNotInclude;
                    string equalsDisplayed = equals == IncludeEquals.Include ? equalsSign : String.Empty;
                    string greaterOrLess = i < 2 ? "<" : ">";
                    Console.WriteLine($"{greaterOrLess}{equalsDisplayed} to {compareTo}");

                    if (i < 2)
                    {
                        returnedTree = intTree2.GetValuesLessThan(compareTo, equals) as TreeIndex;
                    }
                    else
                    {
                        returnedTree = intTree2.GetValuesGreaterThan(compareTo, equals) as TreeIndex;
                    }
                    returnedTree.WriteTreeAsOrderedListToConsoleUsingIterators(true);
                }
                iterator.MoveNext();
            }
        }

        static internal void TestTreeBalancing()
        {
            ITreeIndexInternals intTree = new TreeIndex(true);
            for (int n = 1; n <= 10; ++n)
            {
                intTree.Insert(n * 3, $"{n * 3}");
            }
            for (int n = 1; n <= 10; ++n)
            {
                intTree.Insert(-n, $"{-n}");
            }
            for (int n = 11; n <= 19; ++n)
            {
                intTree.Insert(-n * 2, $"{-n * 2}");
            }

            TreeIndex ti = intTree as TreeIndex;
            ti.WriteTreeToConsole();


            ti.Delete(4);

            ti.WriteTreeToConsole();
            ti.Delete(1);

            ti.WriteTreeToConsole();
            Random r = new Random((int)DateTime.Now.Ticks);

            int duplicates = 0;
            for (int i = 0; i < 40; ++i)
            {
                try
                {
                    intTree.Insert(r.Next(0, 999), new object());
                }
                catch 
                {
                    ++duplicates;
                }
            }

            ti.WriteTreeToConsole();
            Console.WriteLine($"{duplicates} duplicates.");
        }
    }
}
