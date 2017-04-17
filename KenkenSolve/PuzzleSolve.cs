using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;

namespace KenkenSolve
{
    class PuzzleSolve
    {
        public static void Solve(Puzzle puzzle)
        {
            initPuzzle(puzzle);

            puzzle.All.ForEach(c => updateCellPossibles(puzzle, c));

            solveCell(puzzle, puzzle.All.Where(p => p.PossibleValues.Any()).MinBy(p => p.PossibleValueCount));
        }


        private static int i = 0;
        static bool solveCell(Puzzle puzzle, Cell cell)
        {
            cell.Busy = true;//Set ourselves to 'busy' so we don't loop around in our search tree

            int initialValue = cell.Value;//Store our initial state so we can revert to it later if we need to
            IEnumerable<int> initialPossibleValues = cell.PossibleValues;

            foreach (var possibleValue in cell.PossibleValues)
            {
                cell.Value = possibleValue;
                i++;

                //Don't bother continuing if we just filled in a final cell of a group and it resulted in an invalid configuration
                if (cell.Group.Cells.All(c => c.Value != 0) && !cell.Group.isValid())
                    continue;

                if (isPuzzleValid(puzzle))
                    return true;//Did we just solve the puzzle? If so, return

                //Whenever we change our own value, we limit the possible values of all neighbours
                //Update these so we know where to go next
                foreach (var neighbour in cell.Neighbours)
                    updateCellPossibles(puzzle, neighbour);
                
                if (i % 50000 == 0)
                    showProgress(puzzle);

                Cell minNeighbour = null;
                int minNeighbourCount = int.MaxValue;

                foreach (var n in cell.Neighbours)
                {
                    if (!n.Busy && n.PossibleValueCount > 0)
                    {
                        if (n.PossibleValueCount < minNeighbourCount)
                        {
                            minNeighbourCount = n.PossibleValueCount;
                            minNeighbour = n;
                        }
                    }
                }
                //No valid neighbours? We've made a wrong choice earlier
                if (minNeighbour == null)
                    continue;


                //Continue expanding the recursive tree down into the valid neighbour with the least possible choices
                if (solveCell(puzzle, minNeighbour))
                    return true;//If a solution is found down the recursive tree, we can finish
            }

            //No solution found: time to clean up
            cell.Value = initialValue;
            cell.PossibleValues = initialPossibleValues;
            cell.Busy = false;
            return false;
        }

        static void showProgress(Puzzle puzzle)
        {
            int col = puzzle.Columns.Sum(c => c.isValid() ? 1 : 0);
            int row = puzzle.Rows.Sum(r => r.isValid() ? 1 : 0);
            int group = puzzle.Groups.Sum(g => g.isValid() ? 1 : 0);

            int correct = col + row + group;


            int num = puzzle.Columns.Count + puzzle.Groups.Count + puzzle.Rows.Count;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(new string('G', group));
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(new string('C', col));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(new string('R', row));
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(new string('_', num - correct));
            Console.WriteLine(" ({0}/{1})", correct, num);
        }

        static void initPuzzle(Puzzle puzzle)
        {
            foreach (var cell in puzzle.All)
            {
                //Fill in constant value cells with their constant value
                if (cell.Group is ConstantSpan)
                {
                    cell.Value = cell.Group.Goal;
                }
            }
        }

        public static void updateCellPossibles(Puzzle puzzle, Cell cell)
        {
            if (cell.Group is ConstantSpan)
                return;

            var invalids = new List<int>();

            foreach (var c in cell.Row.Cells)
                if (c.Value > 0)
                    invalids.Add(c.Value);

            foreach (var c in cell.Column.Cells)
                if (c.Value > 0)
                    invalids.Add(c.Value);

            if (cell.Group.ChangedSinceGeneration)
                cell.Group.GenerateValids(puzzle);

            //find the set of valids not including any invalids
            cell.PossibleValues = cell.Group.Valids.Except(invalids);
            
            cell.PossibleValueCount = cell.PossibleValues.Count();
        }

        public static bool isPuzzleValid(Puzzle p)
        {
            bool valid = p.Columns.All(c => c.isValid());
            valid = valid && p.Rows.All(r => r.isValid());
            valid = valid && p.Groups.All(g => g.isValid());
            return valid;
        }
    }
}
