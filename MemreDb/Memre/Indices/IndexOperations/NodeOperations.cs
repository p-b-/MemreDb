using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MemreDb.Memre.Indices.IndexOperations
{
    static internal class NodeOperations
    {
        static internal TreeNode Insert(bool uniqueKey, TreeNode node, IComparable indexValue, List<object> values, bool updateNotInsert, bool throwExceptionOnClash)
        {
            if (node == null)
            {
                return CreateNodeHere(uniqueKey, indexValue, values, updateNotInsert);
            }

            int comparison = indexValue.CompareTo(node.IndexValue);
            if (comparison < 0)
            {
                node.LeftNode = Insert(uniqueKey, node.LeftNode!, indexValue, values, updateNotInsert, throwExceptionOnClash);
            }
            else if (comparison > 0)
            {
                node.RightNode = Insert(uniqueKey, node.RightNode!, indexValue, values, updateNotInsert, throwExceptionOnClash);
            }
            else
            {
                return UpdateOrAddValueHere(uniqueKey, node, indexValue, values, updateNotInsert, throwExceptionOnClash);
            }
            node.Height = GetMaxNodeHeight(node!) + 1;
            return RebalanceAfterInsertion(node, indexValue);
        }

        static TreeNode CreateNodeHere(bool uniqueKey, IComparable indexValue, List<object> values, bool updateNotInsert)
        {
            if (updateNotInsert)
            {
                return null;
            }
            else
            {
                TreeNode newNode;
                if (uniqueKey)
                {
                    newNode = new TreeNode(indexValue, values[0]);
                }
                else
                {
                    newNode = new TreeNode(indexValue, values);
                }
                return newNode;
            }
        }

        static TreeNode UpdateOrAddValueHere(bool uniqueKey, TreeNode node, IComparable indexValue, List<object> values, bool updateNotInsert, bool throwExceptionOnClash)
        {
            if (uniqueKey)
            {
                if (!updateNotInsert)
                {
                    if (throwExceptionOnClash)
                    {
                        throw new Exception($"Cannot insert key {indexValue} into unique index");
                    }
                    else
                    {
                        return node;
                    }
                }
                node.Values[0] = values[0];
            }
            else
            {
                node.AddValues(values);
            }
            return node;
        }

        static TreeNode RebalanceAfterInsertion(TreeNode node, IComparable indexValue)
        {
            int balanceFactor = GetBalanceFactor(node);
            if (balanceFactor > 1)
            {
                int comparison = indexValue.CompareTo(node.LeftNode.IndexValue);
                if (comparison < 0)
                {
                    return RotateRight(node);
                }
            }
            if (balanceFactor < -1)
            {
                int comparison = indexValue.CompareTo(node.RightNode.IndexValue);
                if (comparison > 0)
                {
                    return RotateLeft(node);
                }
            }
            if (balanceFactor > 1)
            {
                int comparison = indexValue.CompareTo(node.LeftNode.IndexValue);
                if (comparison > 0)
                {
                    node.LeftNode = RotateLeft(node.LeftNode);
                    return RotateRight(node);
                }
            }
            if (balanceFactor < -1)
            {
                int comparison = indexValue.CompareTo(node.RightNode.IndexValue);
                if (comparison < 0)
                {
                    node.RightNode = RotateRight(node.RightNode);
                    return RotateLeft(node);
                }
            }

            return node;
        }

        static internal TreeNode DeleteNode(TreeNode considerNode, IComparable indexValue)
        {
            if (considerNode == null)
            {
                return null!;
            }

            int comparison = indexValue.CompareTo(considerNode.IndexValue);

            if (comparison < 0)
            {
                // Continue the search for the node to delete down the left node
                considerNode.LeftNode = DeleteNode(considerNode.LeftNode, indexValue);
            }
            else if (comparison > 0)
            {
                // Continue the search for the node to delete down the right node
                considerNode.RightNode = DeleteNode(considerNode.RightNode, indexValue);
            }
            else
            {
                // Found the node to actually delete.
                considerNode = DeleteNode(considerNode);
            }

            // No further descendants to process
            if (considerNode == null)
            {
                return considerNode!;
            }

            // Update current nodes height
            considerNode.Height = GetMaxNodeHeight(considerNode) + 1;

            return RebalanceAfterDeletion(considerNode);
        }

        static internal TreeNode DeleteNode(TreeNode considerNode, IComparable indexValue, Object valueToDelete)
        {
            if (considerNode == null)
            {
                return null;
            }

            int comparison = indexValue.CompareTo(considerNode.IndexValue);

            if (comparison < 0)
            {
                // Continue the search for the node to delete down the left node
                considerNode.LeftNode = DeleteNode(considerNode.LeftNode, indexValue);
            }
            else if (comparison > 0)
            {
                // Continue the search for the node to delete down the right node
                considerNode.RightNode = DeleteNode(considerNode.RightNode, indexValue);
            }
            else
            {
                // Found the node to actually delete.  May only be deleting a single value if this is a non-unique tree
                if (considerNode.Values.Count > 1)
                {
                    DeleteValueFromNode(considerNode, indexValue, valueToDelete);
                    return considerNode;
                }
                else
                {
                    considerNode = DeleteNode(considerNode);
                }
            }

            // No further descendants to process
            if (considerNode == null)
            {
                return considerNode;
            }

            // Update current nodes height
            considerNode.Height = GetMaxNodeHeight(considerNode) + 1;

            return RebalanceAfterDeletion(considerNode);
        }

        static internal void AddFromOtherTree(ITreeIndexInternals destTree, ITreeIndexInternals sourceTree, PropertyInfo destTreePrimaryKey)
        {
            ITreeIndexIterator iterator = sourceTree.GetTreeIndexIterator();
            while (iterator.Valid)
            {
                IComparable indexValue = destTreePrimaryKey.GetValue(iterator.CurrentValue) as IComparable;
                destTree.Insert(indexValue, iterator.CurrentValue);

                iterator.MoveNext();
            }
        }

        static private void DeleteValueFromNode(TreeNode considerNode, IComparable indexValue, Object valueToDelete)
        {
            considerNode.Values.Remove(valueToDelete);
        }

        static private TreeNode DeleteNode(TreeNode nodeToDelete)
        {
            if (nodeToDelete.LeftNode == null || nodeToDelete.RightNode == null)
            {
                nodeToDelete = DeleteNodeWithLessThanTwoChildNodes(nodeToDelete);
            }
            else
            {
                DeleteNodeWithTwoChildNodes(nodeToDelete);
            }

            return nodeToDelete;
        }

        static private TreeNode DeleteNodeWithLessThanTwoChildNodes(TreeNode nodeToDelete)
        {
            // Copy non-empty child to root
            if (nodeToDelete.LeftNode != null)
            {
                nodeToDelete = nodeToDelete.LeftNode;
            }
            else if (nodeToDelete.RightNode != null)
            {
                nodeToDelete = nodeToDelete.RightNode;
            }
            else
            {
                // No children, can simply delete the node
                nodeToDelete = null;
            }

            return nodeToDelete;
        }

        static private void DeleteNodeWithTwoChildNodes(TreeNode nodeToDelete)
        {
            // Get the inorder success (which is the smallest under the right node)
            TreeNode inorderSucessor = GetLowestDescendentNode(nodeToDelete.RightNode);

            // This copies the inorders successors data to the current node, but does not
            //  copy the height or left/right nodes
            nodeToDelete.CopyData(inorderSucessor);

            // Delete the inorder successor value that has just been moved to replace nodeToDelete
            nodeToDelete.RightNode = DeleteNode(nodeToDelete.RightNode, inorderSucessor.IndexValue);
        }

        static TreeNode RotateRight(TreeNode pivotRootNode)
        {
            TreeNode newLocalRootNode = pivotRootNode.LeftNode!;
            TreeNode moveNode = newLocalRootNode.RightNode!;

            // Rotate
            newLocalRootNode.RightNode = pivotRootNode;
            pivotRootNode.LeftNode = moveNode;

            UpdateNodeHeight(pivotRootNode);
            UpdateNodeHeight(newLocalRootNode);

            return newLocalRootNode;
        }

        static TreeNode RebalanceAfterDeletion(TreeNode considerNode)
        {
            int balance = GetBalanceFactor(considerNode);

            TreeNode nodeToReturn = considerNode;
            // Rebalance node if it has become unbalanced (if balance <-1 || >+1)
            if (balance > 1 && GetBalanceFactor(considerNode.LeftNode) >= 0)
            {
                nodeToReturn = RotateRight(considerNode);
            }
            else if (balance > 1 && GetBalanceFactor(considerNode.LeftNode) < 0)
            {
                considerNode.LeftNode = RotateLeft(considerNode.LeftNode);
                nodeToReturn = RotateRight(considerNode);
            }
            else if (balance < -1 && GetBalanceFactor(considerNode.RightNode) <= 0)
            {
                nodeToReturn = RotateLeft(considerNode);
            }
            else if (balance < -1 && GetBalanceFactor(considerNode.RightNode) > 0)
            {
                considerNode.RightNode = RotateRight(considerNode.RightNode);
                nodeToReturn = RotateLeft(considerNode);
            }
            return nodeToReturn;
        }

        static TreeNode RotateLeft(TreeNode pivotRootNode)
        {
            TreeNode newLocalRootNode = pivotRootNode.RightNode!;
            TreeNode moveNode = newLocalRootNode.LeftNode!;

            // Rotate
            newLocalRootNode.LeftNode = pivotRootNode;
            pivotRootNode.RightNode = moveNode;

            UpdateNodeHeight(pivotRootNode);
            UpdateNodeHeight(newLocalRootNode);

            return newLocalRootNode;
        }

        static int GetNodeHeight(TreeNode node)
        {
            if (node == null)
            {
                return 0;
            }
            return node.Height;
        }

        static int GetBalanceFactor(TreeNode node)
        {
            if (node == null)
            {
                return 0;
            }
            return GetNodeHeight(node.LeftNode) - GetNodeHeight(node.RightNode);
        }

        static int GetMaxNodeHeight(TreeNode node)
        {
            int leftPivotRootHeight = GetNodeHeight(node.LeftNode);
            int rightPivotRootHeight = GetNodeHeight(node.RightNode);

            if (leftPivotRootHeight > rightPivotRootHeight)
            {
                return leftPivotRootHeight;
            }
            else
            {
                return rightPivotRootHeight;
            }
        }

        static void UpdateNodeHeight(TreeNode nodeToUpdate)
        {
            nodeToUpdate.Height = GetMaxNodeHeight(nodeToUpdate) + 1;
        }

        // Find the minimum node under searchNode
        static TreeNode GetLowestDescendentNode(TreeNode searchNode)
        {
            while (searchNode.LeftNode != null)
            {
                searchNode = searchNode.LeftNode;
            }

            return searchNode;
        }
    }
}
