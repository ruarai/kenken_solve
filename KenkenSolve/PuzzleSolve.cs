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

            solveCell(puzzle, puzzle.All.Where(p => p.PossibleValues.Count > 0).MinBy(p => p.PossibleValues.Count));
        }
        

        static bool solveCell(Puzzle puzzle, Cell cell)
        {
            cell.Busy = true;//Set ourselves to 'busy' so we don't loop around in our search tree

            int initialValue = cell.Value;//Store our initial state so we can revert to it later if we need to
            List<int> initialPossibleValues = cell.PossibleValues;

            var neighbours = getCellNeighbours(cell);//Find our 'neighbours'
            //That is all cells we will have an effect on:
            //Eg, group, column, row cells

            foreach (var possibleValue in cell.PossibleValues)
            {
                cell.Value = possibleValue;

                if (cell.Group.Cells.All(c => c.Value != 0) && !isSpanValid(cell.Group))
                    continue;

                if (isPuzzleValid(puzzle))
                    return true;//Did we just solve the puzzle? If so, return

                //Whenever we change our own value, we limit the possible values of all neighbours
                //Update these so we know where to go next
                foreach (var neighbour in neighbours)
                    updateCellPossibles(puzzle, neighbour);

                //Look for a neighbour that has both: a search space still to explore (eg: not constant)
                //And is not currently busy (part of our recursive tree)
                var validNeighbours = neighbours.Where(p => p.PossibleValues.Count > 0 && !p.Busy);

                //No valid neighbours? We've made a wrong choice earlier
                if (!validNeighbours.Any())
                    continue;


                //Continue expanding the recursive tree down into the valid neighbour with the least possible choices
                if (solveCell(puzzle, validNeighbours.MinBy(c => c.PossibleValues.Count)))
                    return true;//If a solution is found down the recursive tree, we can finish
                

            }

            //No solution found: time to clean up
            cell.Value = initialValue;
            cell.PossibleValues = initialPossibleValues;
            cell.Busy = false;
            return false;
        }

        static void initPuzzle(Puzzle puzzle)
        {
            foreach (var cell in puzzle.All)
            {
                //Fill in constant value cells with their constant value
                if (cell.Group.Behaviour == Behavior.Constant)
                {
                    cell.Value = cell.Group.Goal;
                }
            }
        }

        static List<Cell> getCellNeighbours(Cell cell)
        {
            var neighbours = cell.Group.Cells.ToList();
            neighbours.AddRange(cell.Column.Cells);
            neighbours.AddRange(cell.Row.Cells);

            return neighbours.Where(c => c.Group.Behaviour != Behavior.Constant && c != cell).Distinct().ToList();
        }

        public static void updateCellPossibles(Puzzle puzzle, Cell cell)
        {
            //generate initial limitations of possible cells based on already present values in rows/cols
            var invalids = generateInvalids(cell.Row);
            invalids.AddRange(generateInvalids(cell.Column));

            //generate possible valid integers based on group limitations
            var valids = generateValids(puzzle, cell.Group);

            //find the set of valids not exclusing any invalids
            cell.PossibleValues = valids.Except(invalids).ToList();
        }

        public static List<int> generateValids(Puzzle puzzle, Span s)
        {
            List<int> valids = new List<int>();
            var sequence = Enumerable.Range(1, puzzle.Size);//Generate a sequence 1, 2, ... n

            if (s.Behaviour == Behavior.Add)
            {
                //Find any integers that add from our current sum to some integer less than our goal
                int currentSum = s.Cells.Sum(c => c.Value);

                foreach (var i in sequence)
                {
                    if (currentSum + i <= s.Goal)
                    {
                        valids.Add(i);
                    }
                }
            }
            //This and divide may be overkill: Unsure if kenken ever has puzzles with groups larger than 2 for subtract or divide
            if (s.Behaviour == Behavior.Subtract)
            {
                foreach (var i in sequence)
                {
                    foreach (var j in sequence)
                    {
                        if (i - j == s.Goal || j - i == s.Goal)
                        {
                            valids.Add(i);
                        }
                    }
                }
            }
            if (s.Behaviour == Behavior.Multiply)
            {
                if (s.Cells.Count == 1)
                {
                    valids.Add(s.Goal / s.Cells[0].Value);
                }
                else
                {
                    //Find any integers that will multiply to some integer less than our equal to our goal
                    int currentProduct = 1;
                    s.Cells.ForEach(c => currentProduct *= c.Value != 0 ? c.Value : 1);
                    foreach (var i in sequence)
                    {
                        if (currentProduct * i <= s.Goal)
                            valids.Add(i);
                    }
                }
            }
            if (s.Behaviour == Behavior.Divide)
            {
                foreach (var i in sequence)
                {
                    foreach (var j in sequence)
                    {
                        if (i / j == s.Goal || j / i == s.Goal)
                        {
                            valids.Add(i);
                        }
                    }
                }
            }

            valids = valids.Where(i => i > 0).Distinct().ToList();
            return valids;
        }

        public static List<int> generateInvalids(Span s)
        {
            if (s.Behaviour == Behavior.Unique)
            {
                var present = s.Cells.Select(c => c.Value); //Values already present

                return present.Where(i => i > 0).ToList();
            }
            return new List<int>(0);//We shouldn't hit this
        }

        public static bool isPuzzleValid(Puzzle p)
        {
            bool valid = p.Columns.All(isSpanValid);
            valid = valid && p.Rows.All(isSpanValid);
            valid = valid && p.Groups.All(isSpanValid);
            return valid;
        }

        public static bool isSpanValid(Span s)
        {
            if (s.Behaviour == Behavior.Constant)//Constant is always true
                return true;

            if (s.Behaviour == Behavior.Unique)
            {
                //Find the set difference between the sequence 1, 2 ... n and the row/column we have
                var difference = s.Cells.Select(c => c.Value).Except(Enumerable.Range(1, s.Goal));

                //If this has any values in it, it's not a valid row/column
                return !difference.Any();
            }


            if (s.Behaviour == Behavior.Add)
            {
                return s.Cells.Sum(c => c.Value) == s.Goal;
            }
            if (s.Behaviour == Behavior.Multiply)
            {
                int product = 1;
                s.Cells.ForEach(c => product *= c.Value);

                if (product == s.Goal)
                    return true;
            }
            if (s.Behaviour == Behavior.Subtract)
            {
                if (s.Cells[0].Value - s.Cells[1].Value == s.Goal)
                    return true;
                if (s.Cells[1].Value - s.Cells[0].Value == s.Goal)
                    return true;
            }
            if (s.Behaviour == Behavior.Divide)
            {
                if (s.Cells[0].Value / s.Cells[1].Value == s.Goal)
                    return true;
                if (s.Cells[1].Value / s.Cells[0].Value == s.Goal)
                    return true;
            }
            return false;

        }
    }
}
