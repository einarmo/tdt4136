using System;
using System.Collections.Generic;
using System.Text;

namespace AStarSearch
{
    /// <summary>
    /// Base class for all nodes in A* algorithm. Contains a few common functions, and a number of abstract functions to be implemented in children.
    /// </summary>
    abstract class BaseNodeState : IComparable<BaseNodeState>, IEquatable<BaseNodeState>
    {
        /// <summary>
        /// The state object as externally accessible property. Not used in the algorithm itself, but convenient.
        /// </summary>
        public abstract object State { get; set; }
        /// <summary>
        /// Calculated total weight as property.
        /// </summary>
        public int G { get; set; }
        /// <summary>
        /// Approximated distance to target.
        /// </summary>
        public abstract int H { get; }
        /// <summary>
        /// Convenience method for the sum of H and G, used as performance measure.
        /// </summary>
        public int F { get { return G + H; } }
        /// <summary>
        /// Parent node. This is null only for the first node.
        /// </summary>
        public BaseNodeState Parent { get; set; }
        /// <summary>
        /// List of references to child nodes.
        /// </summary>
        public IEnumerable<BaseNodeState> Children { get; protected set; }
        /// <summary>
        /// Used to make sorting easier.
        /// </summary>
        /// <param name="other">Node to compare to</param>
        /// <returns>Positive if earlier in order, 0 if equivalent in order, negative if later in order</returns>
        public int CompareTo(BaseNodeState other)
        {
            return F - other.F;
        }
        /// <summary>
        /// Required to simplify equality testing. Should return true if two states are equivalent.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if equivalent</returns>
        public abstract bool Equals(BaseNodeState other);
        /// <summary>
        /// Initialize children, takes a list of existing nodes. Must not create duplicates.
        /// </summary>
        /// <param name="existing">List of all currently existing nodes</param>
        public abstract void CreateChildren(IEnumerable<BaseNodeState> existing);
        /// <summary>
        /// Calculate cost of moving from current node to target. Will throw an error if movement is impossible.
        /// </summary>
        /// <param name="to">Node to get arc cost to</param>
        /// <returns>Arc cost to node `to`</returns>
        public abstract int ArcCost(BaseNodeState to);
        /// <summary>
        /// Executed once every iteration.
        /// </summary>
        /// <param name="allNodes">All known nodes</param>
        public abstract void Tick(IEnumerable<BaseNodeState> allNodes);
    }
}
