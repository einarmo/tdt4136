using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace SudokuSolver
{
    static class Utils
    {
        /// <summary>
        /// Return all possible combinations of values in a and b
        /// </summary>
        /// <returns>List of tuples of length count(a) * count(b)</returns>
        public static IEnumerable<(S, S)> GetAllPairs<S>(IEnumerable<S> a, IEnumerable<S> b)
        {
            var ret = new List<(S, S)>();
            foreach (var itA in a)
            {
                foreach (var itB in b)
                {
                    ret.Add((itA, itB));
                }
            }
            return ret;
        }
        /// <summary>
        /// Perform a deepcopy of an assignment state object
        /// </summary>
        /// <param name="original">Original to copy</param>
        /// <returns>Deep copy of the given object</returns>
        public static Dictionary<R, IEnumerable<T>> DeepCopy<R, T>(Dictionary<R, IEnumerable<T>> original)
        {
            var copy = new Dictionary<R, IEnumerable<T>>();
            foreach (var kvp in original)
            {
                // This makes a copy, in Linq.
                copy[kvp.Key] = kvp.Value.ToList();
            }
            return copy;
        }
        /// <summary>
        /// Pretty print the assignment state object, for debugging
        /// </summary>
        /// <param name="assignment">State to print</param>
        public static void PrintAssignment<R, T>(Dictionary<R, IEnumerable<T>> assignment)
        {
            foreach (var kvp in assignment)
            {
                Console.Write($"{kvp.Key}: ");
                foreach (var val in kvp.Value)
                {
                    Console.Write($"{val}, ");
                }
                Console.Write("\n");
            }
        }
        /// <summary>
        /// Read the sudoku information from a file, into a list of lists of integers
        /// </summary>
        /// <param name="path">Path to the file to read</param>
        /// <returns>A list of lists of integers</returns>
        public static List<List<int>> ReadSudokuFile(string path)
        {
            var res = new List<List<int>>();
            int cnt = 0;
            using (TextReader reader = File.OpenText(path))
            {
                string? line = reader.ReadLine();
                while (line != null)
                {
                    res.Add(new List<int>());
                    foreach (var chr in line)
                    {
                        res[cnt].Add(chr - '0');
                    }
                    cnt++;
                    line = reader.ReadLine();
                }
            }
            return res;
        }
        /// <summary>
        /// Verify that a given sudoku solution is valid, with the correct number of each number in each area, and the correct initial numbers
        /// </summary>
        /// <param name="final"></param>
        /// <param name="raw"></param>
        public static void TestSudoku(Dictionary<string, int> final, List<List<int>> raw)
        {
            // Verify that the initial positions are still in place
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 0; j++)
                {
                    if (raw[i][j] != 0 && !(final[$"{i}-{j}"] == raw[i][j]))
                    {
                        Console.WriteLine($"Incorrect number at {i}, {j}: {final[$"{i}-{j}"]}, expected {raw[i][j]}");
                    }
                }
            }
            // Verify cols

            for (int col = 0; col < 9; col++)
            {
                var numbers = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                for (int row = 0; row < 9; row++)
                {
                    numbers[final[$"{row}-{col}"] - 1]++;
                }
                for (int i = 0; i < 9; i++)
                {
                    if (numbers[i] != 1)
                    {
                        Console.WriteLine($"{numbers[i]} instances of {i + 1} in column {col}");
                    }
                }
            }
            // Verify rows
            for (int row = 0; row < 9; row++)
            {
                var numbers = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                for (int col = 0; col < 9; col++)
                {
                    numbers[final[$"{row}-{col}"] - 1]++;
                }
                for (int i = 0; i < 9; i++)
                {
                    if (numbers[i] != 1)
                    {
                        Console.WriteLine($"{numbers[i]} instances of {i + 1} in row {row}");
                    }
                }
            }
            // Verify boxes
            for (int boxRow = 0; boxRow < 3; boxRow++)
            {
                for (int boxCol = 0; boxCol < 3; boxCol++)
                {
                    var numbers = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    var cells = new List<string>();
                    for (int row = boxRow * 3; row < (boxRow + 1) * 3; row++)
                    {
                        for (int col = boxCol * 3; col < (boxCol + 1) * 3; col++)
                        {
                            cells.Add($"{row}-{col}");
                        }
                    }
                    for (int i = 0; i < 9; i++)
                    {
                        numbers[final[cells[i]] - 1]++;
                    }
                    for (int i = 0; i < 9; i++)
                    {
                        if (numbers[i] != 1)
                        {
                            Console.WriteLine($"{numbers[i]} instances of {i + 1} in box ({boxRow}, {boxCol})");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Pretty print the sudoku solution
        /// </summary>
        /// <param name="final"></param>
        public static void PrintSudoku(Dictionary<string, int> final)
        {
            for (int row = 0; row < 9; row++)
            {
                if (row == 3 || row == 6)
                {
                    Console.WriteLine("---*---*---");
                }

                for (int col = 0; col < 9; col++)
                {
                    if (col == 3 || col == 6)
                    {
                        Console.Write("|");
                    }
                    Console.Write(final[$"{row}-{col}"]);
                }
                Console.Write("\n");
            }
            Console.Write("\n");
        }
    }
}
