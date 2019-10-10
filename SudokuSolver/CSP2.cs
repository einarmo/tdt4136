using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver
{
    /// <summary>
    /// The main CSP state object, containing all functions and persistent states, alternative solution with a single delegate for each constraint.
    /// Note that this is slightly more limited than the other solution, as it only allows for a single constraint per arc. It is sufficient for both tasks here.
    /// It would be fairly simple to keep a list of delegates instead.
    /// </summary>
    /// <typeparam name="R">The key-type used for variable names</typeparam>
    /// <typeparam name="T">The type used for variable values</typeparam>
    class CSP2<R, T>
    {
        /// <summary>
        /// The initial domains of each variable
        /// </summary>
        readonly Dictionary<R, IEnumerable<T>> domains = new Dictionary<R, IEnumerable<T>>();
        /// <summary>
        /// The constraints on each variable, where constraints[i][j] is a function returning true for legal combinations of the values of i and j
        /// </summary>
        readonly Dictionary<R, Dictionary<R, Func<T, T, bool>>> constraints = new Dictionary<R, Dictionary<R, Func<T, T, bool>>>();
        /// <summary>
        /// Count of the number of times the Backtrack function returned failure
        /// </summary>
        public int BacktrackFailures { get; private set; }
        /// <summary>
        /// Count of the number of times the Backtrack function was called.
        /// </summary>
        public int BacktrackCalls { get; private set; }
        /// <summary>
        /// Adds a single variable with the given initial domain. Also initializes an empty list of constraints.
        /// </summary>
        /// <param name="name">Name of the new variable</param>
        /// <param name="domain">List of initial possible values for the new variable</param>
        public void AddVariable(R name, IEnumerable<T> domain)
        {
            domains[name] = domain;
            constraints[name] = new Dictionary<R, Func<T, T, bool>>();
        }
        /// <summary>
        /// Get all the arcs in the form of a list of tuples, where (x, y) describes an arc from x to y.
        /// </summary>
        /// <returns>A list of tuples where (x, y) describes an arc from x to y</returns>
        private IEnumerable<(R, R)> GetAllArcs()
        {
            var arcs = new List<(R, R)>();
            foreach (var keyA in constraints.Keys)
            {
                foreach (var keyB in constraints[keyA].Keys)
                {
                    arcs.Add((keyA, keyB));
                }
            }
            return arcs;
        }
        /// <summary>
        /// Get all arcs that neighbour the given variable
        /// </summary>
        /// <param name="name">Variable to look for neighbours for</param>
        /// <returns>A list of tuples where (x, y) describes an arc from x to y. `x` will be `name`.</returns>
        private IEnumerable<(R, R)> GetAllNeighbouringArcs(R name)
        {
            var arcs = new List<(R, R)>();
            foreach (var keyB in constraints[name].Keys)
            {
                arcs.Add((name, keyB));
            }
            return arcs;
        }
        /// <summary>
        /// Adds a constraint from one variable to another, given by specified constraint function.
        /// </summary>
        /// <param name="i">Origin of the constraint</param>
        /// <param name="j">Target of the constraint</param>
        /// <param name="constraint">Constraint function</param>
        public void AddOneWayConstraint(R i, R j, Func<T, T, bool> constraint)
        {
            constraints[i][j] = constraint;
        }
        /// <summary>
        /// Add a constraint to the list of variables, specifying that they must all be inequal
        /// </summary>
        /// <param name="variables"></param>
        public void AddAllDifferentConstraint(IEnumerable<R> variables)
        {
            foreach (var pair in Utils.GetAllPairs(variables, variables))
            {
                if (!pair.Item1.Equals(pair.Item2))
                {
                    AddOneWayConstraint(pair.Item1, pair.Item2, (x, y) => !x.Equals(y));
                }
            }
        }
        /// <summary>
        /// Initiate the backtracking search, will return either null or the solved problem
        /// </summary>
        /// <returns>A dictionary from CSP key to value of the final states, which should fulfil all constraints</returns>
        public Dictionary<R, T>? BacktrackingSearch()
        {
            var assignment = Utils.DeepCopy(domains);
            Interference(assignment, GetAllArcs());
            // PrintConstraints();
            return Backtrack(assignment);
        }
        /// <summary>
        /// Main backtrack iteration.
        /// </summary>
        /// <param name="assignment">The state to be considered</param>
        /// <returns>null if this branch is a failure, or a dictionary of the final states if not</returns>
        private Dictionary<R, T>? Backtrack(Dictionary<R, IEnumerable<T>> assignment)
        {
            // Utils.PrintAssignment(assignment);
            BacktrackCalls++;
            if (assignment.All(kvp => kvp.Value.Count() == 1)) return assignment.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.First());
            var variable = SelectUnassignedVariable(assignment);
            // With the way the calls are designed, the list of possible values in assignment will be equal to the list of
            // possible values that are also consistent with the constraints
            // This considers the initial domains as a constraint, which is reasonable. If the initial positions are unconstrained,
            // they should be initialized to the full range of possible values
            foreach (var value in assignment[variable])
            {
                var lAssignment = Utils.DeepCopy(assignment);

                lAssignment[variable] = new List<T> { value };
                if (Interference(lAssignment, GetAllNeighbouringArcs(variable)))
                {
                    var result = Backtrack(lAssignment);
                    if (result != default) return result;
                }
            }
            BacktrackFailures++;
            return default;
        }
        /// <summary>
        /// Return a single unassigned variable. Will fail if none are available.
        /// </summary>
        /// <param name="assignment">The current state of the problem</param>
        /// <returns>A single unassigned variable</returns>
        public R SelectUnassignedVariable(Dictionary<R, IEnumerable<T>> assignment)
        {
            return assignment.FirstOrDefault(kvp => kvp.Value.Count() > 1).Key;
        }
        /// <summary>
        /// Perform interference on all variables, removing any possible values that do not fulfil the constraints
        /// </summary>
        /// <param name="assignment">Current state of the problem</param>
        /// <param name="queue">Initial list of variables to be considered</param>
        /// <returns>True if a valid result is produced</returns>
        public bool Interference(Dictionary<R, IEnumerable<T>> assignment, IEnumerable<(R, R)> queue)
        {
            while (queue.Any())
            {
                var pair = queue.First();

                queue = queue.Skip(1);
                if (Revise(assignment, pair.Item1, pair.Item2))
                {
                    if (!assignment[pair.Item1].Any()) return false;
                    queue = queue.Concat(GetAllNeighbouringArcs(pair.Item1)).ToHashSet();
                }
            }
            return true;
        }
        /// <summary>
        /// Revise the constraints from one variable to another
        /// </summary>
        /// <param name="assignment">Current state of the problem</param>
        /// <param name="i">Source of the constraints to be tested</param>
        /// <param name="j">Target of the constraints to be tested</param>
        /// <returns>True if a revision was performed</returns>
        public bool Revise(Dictionary<R, IEnumerable<T>> assignment, R i, R j)
        {
            bool revised = false;
            List<T> illegal = new List<T>();
            foreach (var x in assignment[i])
            {
                if (!assignment[j].Any(y => constraints[i][j](x, y)))
                {
                    revised = true;
                    illegal.Add(x);
                }
            }
            if (illegal.Any())
            {
                assignment[i] = assignment[i].Except(illegal);
            }

            return revised;
        }
    }
}
