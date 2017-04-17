using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KenkenSolve
{

    abstract class Span//Collection of cells, may be a row, column or a group
    {
        public List<Cell> Cells = new List<Cell>();

        public int Goal;
        public char Character;
        public abstract bool isValid();
        public abstract List<int> generateValids(Puzzle p, List<int> invalids);

        public abstract char CharCode { get; }
    }

    class ConstantSpan : Span
    {
        public override bool isValid()
        {
            return true;//Constants are always valid
        }

        public override List<int> generateValids(Puzzle p, List<int> invalids)//Not necessary for constants
        {
            return new List<int>();
        }

        public override char CharCode { get { return 'c'; } }
    }

    class AddSpan : Span
    {
        public override bool isValid()
        {
            return Cells.Sum(c => c.Value) == Goal;
        }

        public override List<int> generateValids(Puzzle p, List<int> invalids)
        {
            List<int> valids = new List<int>();
            var sequence = Enumerable.Range(1, p.Size);//Generate a sequence 1, 2, ... n

            //Find any integers that add from our current sum to some integer less than our goal
            int currentSum = Cells.Sum(c => c.Value);

            foreach (var i in sequence)
            {
                if (currentSum + i <= Goal)
                {
                    if (!invalids.Contains(i))
                        valids.Add(i);
                }
            }
            return valids;
        }
        public override char CharCode { get { return '+'; } }
    }
    class SubtractSpan : Span
    {
        public override bool isValid()
        {
            return Cells[0].Value - Cells[1].Value == Goal || Cells[1].Value - Cells[0].Value == Goal;
        }

        public override List<int> generateValids(Puzzle p, List<int> invalids)
        {
            List<int> valids = new List<int>();
            var sequence = Enumerable.Range(1, p.Size);//Generate a sequence 1, 2, ... n

            foreach (var i in sequence)
            {
                foreach (var j in sequence)
                {
                    if (i - j == Goal || j - i == Goal)
                    {
                        if (!invalids.Contains(i))
                            valids.Add(i);
                    }
                }
            }
            return valids;
        }
        public override char CharCode { get { return '-'; } }
    }
    class MultiplySpan : Span
    {
        public override bool isValid()
        {
            int product = 1;
            Cells.ForEach(c => product *= c.Value);

            return product == Goal;
        }

        public override List<int> generateValids(Puzzle p, List<int> invalids)
        {
            List<int> valids = new List<int>();
            var sequence = Enumerable.Range(1, p.Size);//Generate a sequence 1, 2, ... n

            if (Cells.Count == 1)
            {
                valids.Add(Goal / Cells[0].Value);
            }
            else
            {
                //Find any integers that will multiply to some integer less than our equal to our goal
                int currentProduct = 1;
                Cells.ForEach(c => currentProduct *= c.Value != 0 ? c.Value : 1);
                foreach (var i in sequence)
                {
                    if (currentProduct * i <= Goal)
                        if (!invalids.Contains(i))
                            valids.Add(i);
                }
            }
            return valids;
        }
        public override char CharCode { get { return 'x'; } }
    }
    class DivideSpan : Span
    {
        public override bool isValid()
        {
            if (Cells[0].Value == 0 || Cells[1].Value == 0)
                return false;

            return Cells[0].Value / Cells[1].Value == Goal || Cells[1].Value / Cells[0].Value == Goal;
        }

        public override List<int> generateValids(Puzzle p, List<int> invalids)
        {
            List<int> valids = new List<int>();
            var sequence = Enumerable.Range(1, p.Size);//Generate a sequence 1, 2, ... n


            foreach (var i in sequence)
            {
                foreach (var j in sequence)
                {
                    if (i / j == Goal || j / i == Goal)
                    {
                        if (!invalids.Contains(i))
                            valids.Add(i);
                    }
                }
            }

            return valids;
        }
        public override char CharCode { get { return '/'; } }
    }

    class UniqueSpan : Span {
        public override bool isValid()
        {
            //Find the set difference between the sequence 1, 2 ... n and the row/column we have
            var difference = Cells.Select(c => c.Value).Except(Enumerable.Range(1, Goal));

            //If this has any values in it, it's not a valid row/column
            return !difference.Any();
        }

        public override List<int> generateValids(Puzzle p, List<int> invalids)
        {throw new NotImplementedException();}

        public override char CharCode { get { return ' '; } }
    }
}
