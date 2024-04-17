using MemreDb.Memre;
using MemreDb.Demo;
using MemreDb.Tests;

namespace MemreDb;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("MemreDb sample app starting...");

        //TreeIndexTests.TestMultiLeafIterator();

        DemoSetup demo = new DemoSetup();
        demo.SetUp();
        demo.CreateData();
        demo.RunTests();
    }
}