using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KenkenSolve
{
    class Puzzle
    {
        public Puzzle(string file)
        {
            string[] lines = File.ReadAllLines(file);

            Size = int.Parse(lines[0]);
            char[,] grid = new char[Size, Size];

            for (int y = 0; y < Size; y++)
            {
                string row = lines[1 + y];//Take a row from the game file
                for (int x = 0; x < Size; x++)
                    grid[x, y] = row[x];
            }

            for (int i = Size + 1; i < lines.Length; i++)
            {
                Span span;
                var pieces = lines[i].Split(';');

                switch (pieces[2][0])
                {
                    case '+':
                        span = new AddSpan();
                        break;
                    case '-':
                        span = new SubtractSpan();
                        break;
                    case 'x':
                        span = new MultiplySpan();
                        break;
                    case '/':
                        span = new DivideSpan();
                        break;
                    case 'c':
                        span = new ConstantSpan();
                        break;
                    default:
                        span = new AddSpan();
                        break;
                }

                span.Character = pieces[0][0];
                span.Goal = int.Parse(pieces[1]);

                Groups.Add(span);
            }

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    char c = grid[x, y];
                    Cell cell = new Cell();

                    cell.X = x;
                    cell.Y = y;

                    //build a bidirectional relationship between groups and cells
                    cell.Group = Groups.FirstOrDefault(g => g.Character == c);
                    cell.Group.Cells.Add(cell);

                    All.Add(cell);
                }
            }

            for (int i = 0; i < Size; i++)
            {
                Span column = new UniqueSpan { Cells = All.Where(c => c.X == i).ToList(), Goal = Size };
                Span row = new UniqueSpan { Cells = All.Where(c => c.Y == i).ToList(), Goal = Size };

                Columns.Add(column);
                Rows.Add(row);

                //build a bidirectional relationship between cells and columns/rows
                column.Cells.ForEach(c => c.Column = column);
                row.Cells.ForEach(c => c.Row = row);
            }

            All.ForEach(c=>c.Neighbours = getCellNeighbours(c));
        }

        static List<Cell> getCellNeighbours(Cell cell)
        {
            var neighbours = cell.Group.Cells.ToList();
            neighbours.AddRange(cell.Column.Cells);
            neighbours.AddRange(cell.Row.Cells);

            return neighbours.Where(c => !(c.Group is ConstantSpan) && c != cell).Distinct().ToList();
        }

        public List<Cell> All = new List<Cell>();

        public List<Span> Groups = new List<Span>();

        public List<Span> Columns = new List<Span>();
        public List<Span> Rows = new List<Span>();

        public int Size;

        //Tries to print the puzzle in a way that the original grid is visible, not perfect but works
        public void Print(Func<Cell, string> value)
        {
            foreach (var puzzleRow in Rows)
            {
                foreach (var cell in puzzleRow.Cells)
                {
                    Console.Write(value(cell).PadLeft(4, ' '));

                    int colI = puzzleRow.Cells.IndexOf(cell);
                    if (colI + 1 < puzzleRow.Cells.Count)//Check if we're at the end of the row
                    {
                        //If we're next to a neighbour of the same group, connect them on the same row
                        if (cell.Group == puzzleRow.Cells[colI + 1].Group)
                            Console.Write(" ");
                        else
                            Console.Write("|");
                    }

                }
                Console.WriteLine();

                int rowI = Rows.IndexOf(puzzleRow);
                if (rowI + 1 < Rows.Count)
                {
                    foreach (var cell in puzzleRow.Cells)
                    {
                        int colI = puzzleRow.Cells.IndexOf(cell);

                        //If we're next to a neighbour of the same group, connect them on the same column
                        if (cell.Group == Rows[rowI + 1].Cells[colI].Group)
                            Console.Write("     ");
                        else
                            Console.Write("-----");
                    }
                }
                Console.WriteLine();

            }
        }

    }

    class Cell
    {
        public Span Column;
        public Span Row;
        public Span Group;

        public int X;
        public int Y;

        public bool Busy = false;

        public int Value;

        public IEnumerable<int> PossibleValues = new List<int>();

        public int PossibleValueCount = 0;

        public List<Cell> Neighbours = new List<Cell>();
    }


}
