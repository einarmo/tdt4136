using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AStarSearch
{
    class SimpleBoardState : BaseNodeState
    {
        /// <summary>
        /// The board, target and endTarget are stored as static variables.
        /// </summary>
        public static int[,] board;
        public static (int, int) target;
        public static int rows;
        public static int cols;
        public static (int, int)? endTargetPos = null;
        private static int tickCounter = 0;
        /// <summary>
        /// Internal representation of the state: position on the board.
        /// </summary>
        private (int, int) position;
        public override object State { get => position; set { position = ((int, int))value; } }
        /// <summary>
        /// Use Manhattan cost. If endTargetPos is specified, use the average of the end and the current target.
        /// </summary>
        public override int H
        {
            get
            {
                if (endTargetPos != null)
                {
                    return Math.Abs((int)((target.Item1 + endTargetPos?.Item1) / 2 - position.Item1))
                        + Math.Abs((int)((target.Item2 + endTargetPos?.Item2) / 2 - position.Item2));
                }
                return Math.Abs(target.Item1 - position.Item1) + Math.Abs(target.Item2 - position.Item2);
            }
        }
        /// <summary>
        /// Arc cost is simply the value in the board: the cost of moving to the target.
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public override int ArcCost(BaseNodeState to)
        {
            var targetPos = ((int, int))to.State;
            if (Math.Abs(targetPos.Item1 - position.Item1) == 1 ^ Math.Abs(targetPos.Item2 - position.Item2) == 1 && board[targetPos.Item1, targetPos.Item2] >= 0)
            {
                return board[targetPos.Item1, targetPos.Item2];
            }
            throw new Exception("Cannot calculate arc distance for non-adjecent nodes");
        }
        /// <summary>
        /// Just adds any neighbouring tiles that are passable to `children`, create any that do not exist.
        /// </summary>
        /// <param name="existing">List of all existing nodes</param>
        public override void CreateChildren(IEnumerable<BaseNodeState> existing)
        {
            var children = new List<BaseNodeState>();
            for (int i = position.Item1 - 1; i <= position.Item1 + 1; i++)
            {
                for (int j = position.Item2 - 1; j <= position.Item2 + 1; j++)
                {
                    if (!(i == position.Item1 ^ j == position.Item2)) continue;
                    if (i < 0 || j < 0 || i >= rows || j >= cols || board[i,j] < 0) continue;
                    SimpleBoardState newChild = new SimpleBoardState();
                    newChild.position = (i, j);
                    if (existing.Contains(newChild))
                    {
                        newChild = (SimpleBoardState)existing.First(node => node.Equals(newChild));
                    }
                    children.Add(newChild);
                }
            }
            Children = children;
        }
        /// <summary>
        /// Equal for equal position and type.
        /// </summary>
        public override bool Equals(BaseNodeState other)
        {
            if (!(other is SimpleBoardState)) return false;
            var otherPos = ((int, int))other.State;
            return position.Item1 == otherPos.Item1 && position.Item2 == otherPos.Item2;
        }
        /// <summary>
        /// HashCode is used to make the code more efficient, by allowing us to use sets to compare easily.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return position.GetHashCode();
        }
        /// <summary>
        /// Return the next position in a move tick.
        /// </summary>
        private (int, int) GetNextTargetPos()
        {
            if (target.Item1 < endTargetPos?.Item1)
            {
                return (target.Item1 + 1, target.Item2);
            }
            else if (target.Item1 > endTargetPos?.Item1)
            {
                return (target.Item1 - 1, target.Item2);
            }
            else if (target.Item2 < endTargetPos?.Item2)
            {
                return (target.Item1, target.Item2 + 1);
            }
            else if (target.Item2 > endTargetPos?.Item2)
            {
                return (target.Item1, target.Item2 - 1);
            }
            else
            {
                return target;
            }
        }
        /// <summary>
        /// Tick here should only be run on the target node.
        /// </summary>
        /// <param name="allNodes">List of all nodes. Not really used here, but should be included to make the interface properly generic.</param>
        public override void Tick(IEnumerable<BaseNodeState> allNodes)
        {
            if (endTargetPos == null) return;
            if (position != target) throw new Exception("Tick on the target node");
            if (++tickCounter % 4 == 0)
            {
                var next = GetNextTargetPos();
                position = next;
                target = next;
            }
        }
    }
}
