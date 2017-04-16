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
            puzzle.Print(c => string.Join("", c.PossibleValues));

            foreach (var cell in puzzle.All)
            {
                solveCell(puzzle, cell);

                if (isPuzzleValid(puzzle))
                    break;

                Console.WriteLine("continuing");
            }
        }

        static void solveCell(Puzzle puzzle, Cell cell)
        {
            if (cell.Busy)
                return;

            cell.Busy = true;


            int initialValue = cell.Value;

            var neighbours = getCellNeighbours(cell);

            foreach (var possibleValue in cell.PossibleValues)
            {
                cell.Value = possibleValue;


                foreach (var neighbour in neighbours)
                {
                    updateCellPossibles(puzzle, neighbour);
                }

                //Console.WriteLine("At cell {0},{1}", cell.X, cell.Y);

                //puzzle.Print(c => string.Join("", c.PossibleValues));
                //puzzle.Print(c => c.Value.ToString());
                //Console.ReadKey();

                var validNeighbours = neighbours.Where(p => p.PossibleValues.Count > 0);

                if (!validNeighbours.Any())
                    break;

                solveCell(puzzle, validNeighbours.MinBy(c => c.PossibleValues.Count));

            }


            if (isPuzzleValid(puzzle))
            {
                return;
            }


            cell.Value = initialValue;
            cell.Busy = false;
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
            var invalids = generateInvalids(cell.Row).ToList();
            invalids.AddRange(generateInvalids(cell.Column));

            var valids = generateValids(puzzle, cell.Group);

            cell.PossibleValues = valids.Except(invalids).ToList();
        }

        public static List<int> generateValids(Puzzle puzzle, Span s)
        {
            List<int> valids = new List<int>();
            var sequence = Enumerable.Range(1, puzzle.Size);//Generate a sequence 1, 2, ... n

            if (s.Behaviour == Behavior.Add)
            {
                int currentSum = s.Cells.Sum(c => c.Value);

                foreach (var i in sequence)
                {
                    if (currentSum + i <= s.Goal)
                    {
                        valids.Add(i);
                    }
                }
            }
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
                int currentProduct = 1;
                s.Cells.ForEach(c => currentProduct *= c.Value != 0 ? c.Value : 1);
                foreach (var i in sequence)
                {
                    if (currentProduct * i <= s.Goal)
                        valids.Add(i);
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
            return new List<int>(0);
        }

        public static bool isPuzzleValid(Puzzle p)
        {
            return p.All.All(c => isCellValid(c));
        }

        public static bool isCellValid(Cell c)
        {
            return isSpanValid(c.Row) && isSpanValid(c.Column) && isSpanValid(c.Group);
        }

        public static bool isSpanValid(Span s)
        {
            if (s.Behaviour == Behavior.Constant)//Constant is always true
                return true;

            if (s.Behaviour == Behavior.Unique)
            {
                return !s.Cells.Select(c => c.Value).Except(Enumerable.Range(1, s.Goal)).Any();
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

            //Need to generate permutations for subtractions and divisions
            var permutations = GetPermutations(s.Cells);

            if (s.Behaviour == Behavior.Subtract)
            {
                foreach (var p in permutations)
                {
                    int start = p.First().Value;


                    //start - a - b ... - z == start - (a + b + ... z)

                    int skipped = p.Skip(1).Sum(c => c.Value);
                    if (start - skipped == s.Goal)
                        return true;
                }
            }
            if (s.Behaviour == Behavior.Divide)
            {
                foreach (var p in permutations)
                {
                    int start = p.First().Value;

                    foreach (var cell in p.Skip(1))
                    {
                        start /= cell.Value;
                    }
                    if (start == s.Goal)
                        return true;
                }
            }
            return false;

        }

        static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length = -1)
        {
            if (length == -1) length = list.Count();
            if (length == 1) return list.Select(t => new T[] { t });

            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }
    }
}
