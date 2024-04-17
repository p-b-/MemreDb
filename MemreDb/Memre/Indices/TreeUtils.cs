using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre.Indices
{
    static internal class TreeUtils
    {
        static internal void WriteTreeAsOrderedListToConsole(TreeNode rootNode)
        {
            if (rootNode.LeftNode != null)
            {
                WriteTreeAsOrderedListToConsole(rootNode.LeftNode);
            }
            Console.WriteLine($"Node Value {rootNode.IndexValue}");
            if (rootNode.RightNode != null)
            {
                WriteTreeAsOrderedListToConsole(rootNode.RightNode);
            }
        }

        static internal void WriteTreeAsOrderedListToConsole(ITreeNodeIterator iterator)
        {
            TreeNode node = iterator.Node;
            while (node != null)
            {
                if (node.Values!=null && node.Values.Count==1)
                {
                    Console.WriteLine($"Node {node.IndexValue} = {node.Values[0]}");
                }
                else if (node.Values != null && node.Values.Count > 1)
                {
                    Console.WriteLine($"Node {node.IndexValue} = {node.Values[0]} ... {node.Values.Count-1} more");
                }
                else
                {
                    Console.WriteLine($"Node {node.IndexValue}: no value");
                }
                node = iterator.MoveNext();
            }
        }

        static internal void WriteTreeToConsole(TreeNode root)
        {
            List<TreeNode> nodesToWrite = new List<TreeNode>();
            List<int> nodesXCoord = new List<int>();
            nodesToWrite.Add(root);
            nodesXCoord.Add(60);
            int maxNodeValueLength = 3;
            int spacingBetweenNodes = 28;

            while (nodesToWrite.Count > 0)
            {
                WriteNodeListToConsole(nodesToWrite, nodesXCoord, maxNodeValueLength);
                List<TreeNode> nextNodesToWrite = new List<TreeNode>();
                List<int> nextNodesXCoord = new List<int>();
                for (int n = 0; n < nodesToWrite.Count; ++n)
                {
                    TreeNode node = nodesToWrite[n];
                    int currentXCoord = nodesXCoord[n];

                    if (node.LeftNode != null)
                    {
                        nextNodesToWrite.Add(node.LeftNode);
                        nextNodesXCoord.Add(currentXCoord - spacingBetweenNodes);
                    }
                    if (node.RightNode != null)
                    {
                        nextNodesToWrite.Add(node.RightNode);
                        nextNodesXCoord.Add(currentXCoord + spacingBetweenNodes);
                    }
                }
                spacingBetweenNodes = spacingBetweenNodes / 2;
                nodesToWrite = nextNodesToWrite;
                nodesXCoord = nextNodesXCoord;
            }
        }

        static void WriteNodeListToConsole(List<TreeNode> nodesToWrite, List<int> nodesXCoord, int maxNodeValueLength)
        {
            int currentXCoord = 0;
            for (int n = 0; n < nodesToWrite.Count; ++n)
            {
                int nextXCoord = nodesXCoord[n];

                Console.Write(CreateSpaces(nextXCoord - currentXCoord));
                currentXCoord = nextXCoord;
                WriteNodeValueWithMaxLength(nodesToWrite[n].IndexValue, maxNodeValueLength);
                currentXCoord += maxNodeValueLength;
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        static string CreateSpaces(int spaceCount)
        {
            if (spaceCount < 0)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder(spaceCount);
            for (int i = 0; i < spaceCount; i++)
            {
                sb.Append(" ");
            }
            return sb.ToString();
        }

        static int WriteNodeValueWithMaxLength(IComparable value, int maxLength)
        {
            string v = value.ToString();
            int valueLength = v.Length;
            if (valueLength > maxLength)
            {
                v = v.Substring(0, maxLength);
                valueLength = maxLength;
            }
            Console.Write(v);
            if (valueLength < maxLength)
            {
                Console.Write(CreateSpaces(maxLength - v.Length));
            }
            return maxLength;
        }
    }

}
