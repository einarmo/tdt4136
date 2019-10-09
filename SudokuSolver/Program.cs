using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SudokuSolver
{
    class Program
    {
        static void Main()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            //ColorTest();
            SolveSudoku("easy.txt");
            SolveSudoku("medium.txt");
            SolveSudoku("hard.txt");
            SolveSudoku("veryhard.txt");
            watch.Stop();
            Console.WriteLine($"Solution took: {watch.ElapsedMilliseconds} milliseconds");
            
        }

        /// <summary>
        /// Solve the sudoku contained in the file given by "path"
        /// </summary>
        /// <param name="path">Path to the sudoku to be solved</param>
        static void SolveSudoku(string path)
        {
            var raw = Utils.ReadSudokuFile(path);
            var values = Enumerable.Range(1, 9);
            var field = new List<string>();
            var csp = new CSP2<string, int>();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (raw[i][j] == 0)
                    {
                        csp.AddVariable($"{i}-{j}", values);
                    }
                    else
                    {
                        csp.AddVariable($"{i}-{j}", new List<int> { raw[i][j] });
                    }
                }
            }
            for (int row = 0; row < 9; row++)
            {
                csp.AddAllDifferentConstraint(Enumerable.Range(0, 9).Select(col => $"{row}-{col}"));
                csp.AddAllDifferentConstraint(Enumerable.Range(0, 9).Select(col => $"{col}-{row}"));
            }
            for (int boxRow = 0; boxRow < 3; boxRow++)
            {
                for (int boxCol = 0; boxCol < 3; boxCol++)
                {
                    var cells = new List<string>();
                    for (int row = boxRow * 3; row < (boxRow + 1) * 3; row++)
                    {
                        for (int col = boxCol * 3; col < (boxCol + 1) * 3; col++)
                        {
                            cells.Add($"{row}-{col}");
                        }
                    }
                    csp.AddAllDifferentConstraint(cells);
                }
            }
            var result = csp.BacktrackingSearch();
            if (result == null)
            {
                Console.WriteLine("No result found");
                return;
            }
            Console.WriteLine($"Solved with {csp.BacktrackCalls} calls to Backtrack and {csp.BacktrackFailures} failures.");
            Utils.PrintSudoku(result);
            Utils.TestSudoku(result, raw);
        }
        /// <summary>
        /// Reproduction of the "color" test from the python sample
        /// </summary>
        static void ColorTest()
        {
            var colors = new List<string> { "Red", "Green", "Blue" };
            var states = new List<string> { "WA", "NT", "Q", "NSW", "V", "SA", "T" };
            var edges = new Dictionary<string, List<string>> {
                { "SA", new List<string> { "WA", "NT", "Q", "NSW", "V" } },
                { "NT", new List<string> { "WA", "Q" } },
                { "NSW", new List<string> { "Q", "V" } }
            };
            var csp = new CSP<string, string>();
            foreach (var state in states)
            {
                csp.AddVariable(state, colors);
            }
            foreach (var kvp in edges)
            {
                foreach (var state in kvp.Value)
                {
                    csp.AddOneWayConstraint(kvp.Key, state, (x, y) => x != y);
                    csp.AddOneWayConstraint(state, kvp.Key, (x, y) => x != y);
                }
            }
            var result = csp.BacktrackingSearch();
            if (result == null)
            {
                Console.WriteLine("No result found");
            }
            else
            {
                Console.WriteLine("\nSolution found: ");
                foreach (var kvp in result)
                {
                    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                }
            }
        }
    }
}
