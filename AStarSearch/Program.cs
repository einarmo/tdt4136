using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AStarSearch
{
    class Program
    {
        /// <summary>
        /// Run A* with specified map as csv file, integer weights separated by , with one blank line at the end.
        /// Negative numbers are treated as impassable.
        /// </summary>
        /// <param name="path">Path to csv file</param>
        /// <param name="targetPos">Position of target</param>
        /// <param name="startPos">Position of start</param>
        /// <param name="endTargetPos">If specified, target will move towards this at a rate of one step per 4 units of time.</param>
        private static void TestSimpleBoardSearch(string path, (int, int) targetPos, (int, int) startPos, (int, int)? endTargetPos = null)
        {
            string rawBoard = File.ReadAllText(path, Encoding.UTF8);
            string[] lines = rawBoard.Split('\n');
            var cols = lines[0].Split(',').Length;
            int[,] board = new int[lines.Length - 1, cols];
            for (int i = 0; i < lines.Length - 1; i++)
            {
                var chars = lines[i].Split(',');
                for (int j = 0; j < cols; j++)
                {
                    var charact = chars[j];
                    board[i, j] = int.Parse(chars[j]);
                }
            }
            SimpleBoardState.board = board;
            SimpleBoardState.target = targetPos;
            SimpleBoardState.cols = cols;
            SimpleBoardState.rows = lines.Length - 1;
            SimpleBoardState.endTargetPos = endTargetPos;
            var target = new SimpleBoardState
            {
                State = targetPos
            };
            var initial = new SimpleBoardState
            {
                State = startPos
            };
            var final = AStarSearch(initial, target);
            if (final == null)
            {
                Console.WriteLine("No solution found");
            }
            var visitedNodes = new HashSet<(int, int)>();
            while (final != null)
            {
                visitedNodes.Add(((int, int))final.State);
                final = final.Parent;
            }
            var tpstr = new string('#', SimpleBoardState.cols + 2) + "\n";
            var charMap = new Dictionary<int, char>
            {
                { 1, '.' },
                { 2, ',' },
                { 3, ':' },
                { 4, ';' }
            };
            for (int i = 0; i < SimpleBoardState.rows; i++)
            {
                tpstr += '#';
                for (int j = 0; j < SimpleBoardState.cols; j++)
                {
                    if (visitedNodes.Contains((i, j)))
                    {
                        tpstr += 'o';
                    }
                    else if (board[i, j] < 0)
                    {
                        tpstr += '#';
                    }
                    else
                    {
                        tpstr += charMap[board[i, j]];
                    }
                }
                tpstr += "#\n";
            }
            tpstr += new string('#', SimpleBoardState.cols + 2) + "\n";
            Console.WriteLine(tpstr);
        }
        static void Main()
        {
            TestSimpleBoardSearch("Samfundet_map_1.csv", (40, 32), (27, 18));
            TestSimpleBoardSearch("Samfundet_map_1.csv", (8, 5), (40, 32));
            TestSimpleBoardSearch("Samfundet_map_2.csv", (6, 32), (28, 32));
            TestSimpleBoardSearch("Samfundet_map_Edgar_full.csv", (6, 32), (28, 32));
            TestSimpleBoardSearch("Samfundet_map_2.csv", (6, 36), (14, 18), (6, 8));
        }
        /// <summary>
        /// The main A* search function. This is more or less word for word the algorithm from the document, with a few modifications.
        /// First, we check each iteration for completion, because the goal is able to move.
        /// Second, we track "all" nodes, for convenience.
        /// It's generic, which I did as a challenge, and because it makes for very clean code.
        /// I wrote this.
        /// </summary>
        /// <param name="initial">Initial node</param>
        /// <param name="solution">Target node</param>
        /// <returns></returns>
        static BaseNodeState AStarSearch(BaseNodeState initial, BaseNodeState solution)
        {
            var all = new HashSet<BaseNodeState>();
            var open = new List<BaseNodeState>();
            var closed = new HashSet<BaseNodeState>();
            open.Add(initial);
            all.Add(initial);
            while (open.Any())
            {
                if (closed.Contains(solution))
                {
                    return closed.First(node => node.Equals(solution));
                }
                open.Sort();
                var x = open.First();
                open.Remove(x);
                closed.Add(x);
                if (x.Equals(solution))
                {
                    return x;
                }
                x.CreateChildren(all);
                foreach (var child in x.Children)
                {
                    if (!all.Contains(child))
                    {
                        AttachAndEval(child, x);
                        open.Add(child);
                        all.Add(child);
                    }
                    else if (x.G + x.ArcCost(child) < child.G)
                    {
                        AttachAndEval(child, x);
                        if (closed.Contains(child))
                        {
                            PropogatePathImprovements(child);
                        }
                    }
                }
                solution.Tick(all);
            }
            return null;
        }
        /// <summary>
        /// Attach and eval function from document
        /// </summary>
        /// <param name="c"></param>
        /// <param name="p"></param>
        static void AttachAndEval(BaseNodeState c, BaseNodeState p)
        {
            c.Parent = p;
            c.G = p.G + p.ArcCost(c);
        }
        /// <summary>
        /// PropogatePathImprovement function from document
        /// </summary>
        /// <param name="p"></param>
        static void PropogatePathImprovements(BaseNodeState p)
        {
            foreach (var child in p.Children)
            {
                if (p.G + p.ArcCost(child) < child.G)
                {
                    child.Parent = p;
                    child.G = p.G + p.ArcCost(child);
                    PropogatePathImprovements(child);
                }
            }
        }
    }
}
