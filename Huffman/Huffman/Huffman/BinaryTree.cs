using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huffman
{
    class BinaryTree : IComparable
    {
        private BinaryTree m_left;
        public BinaryTree Left
        {
            get
            {
                return m_left;
            }
            set
            {
                m_left = value;
            }
        }

        private BinaryTree m_right;
        public BinaryTree Right
        {
            get
            {
                return m_right;
            }
            set
            {
                m_right = value;
            }
        }

        private Node m_node;
        public Node Node
        {
            get
            {
                return m_node;
            }
            set
            {
                m_node = value;
            }
        }

        private bool m_bIsInTheCurrentLevel;
        public bool IsInTheCurrentLevel
        {
            get
            {
                return m_bIsInTheCurrentLevel;
            }
            set
            {
                m_bIsInTheCurrentLevel = value;
            }
        }

        public BinaryTree()
        {
            IsInTheCurrentLevel = true;
            Node = new Node();
            Left = null;
            Right = null;
        }

        public BinaryTree(Node i_node)
        {
            IsInTheCurrentLevel = true;
            Node = i_node;
            Left = null;
            Right = null;
        }

        public BinaryTree(Node i_node, ref BinaryTree i_left, ref BinaryTree i_right)
        {
            IsInTheCurrentLevel = true;
            Node = i_node;
            Left = i_left;
            Right = i_right;
        }

        public static bool operator >(BinaryTree i_first, BinaryTree i_second)
        {
            if (i_first == null)
                return false;
            if (i_second == null)
                return true;
            if (i_first.IsInTheCurrentLevel && !i_second.IsInTheCurrentLevel)
                return false;
            if (i_second.IsInTheCurrentLevel && !i_first.IsInTheCurrentLevel)
                return true;
            return i_first.Node.Frequency > i_second.Node.Frequency;
        }

        public static bool operator <(BinaryTree i_first, BinaryTree i_second)
        {
            if (i_first == null)
                return true;
            if (i_second == null)
                return false;
            if (i_first.IsInTheCurrentLevel && !i_second.IsInTheCurrentLevel)
                return true;
            if (i_second.IsInTheCurrentLevel && !i_first.IsInTheCurrentLevel)
                return false;
            return i_first.Node.Frequency < i_second.Node.Frequency;
        }

        public int CompareTo(object obj)
        {
            if (obj is BinaryTree)
            {
                if (this < (BinaryTree)obj)
                {
                    return 1;
                }
                else if (this > (BinaryTree)obj)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
            throw new ArgumentException("Object is not a Binary Tree!");
        }

        public override string ToString()
        {
            string tree = "( " + Node + " , " + ((Left == null) ? " null " : Left.ToString()) + " , " + ((Right == null) ? " null " : Right.ToString()) + " )";
            return tree;
        }
    }
}
