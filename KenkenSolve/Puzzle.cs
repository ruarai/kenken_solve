﻿using System;
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
                Span span = new Span();

                var pieces = lines[i].Split(';');

                span.Character = pieces[0][0];
                span.N = int.Parse(pieces[1]);
                span.Behaviour = getBehaviour(pieces[2][0]);

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

                    All.Cells.Add(cell);
                }
            }

            for (int i = 0; i < Size; i++)
            {
                Span column = new Span { Cells = All.Cells.Where(c => c.X == i).ToList() };
                Span row = new Span { Cells = All.Cells.Where(c => c.Y == i).ToList() };

                Columns.Add(column);
                Rows.Add(row);
            }
        }

        public Span All = new Span();

        public List<Span> Groups = new List<Span>();

        public List<Span> Columns = new List<Span>();
        public List<Span> Rows = new List<Span>();

        public int Size;

        public void Print()
        {
            foreach (var puzzleRow in Rows)
            {
                foreach (var cell in puzzleRow.Cells)
                {
                    Console.Write(cell.Group.N.ToString().PadLeft(3, '0'));
                    Console.Write("|");
                }
                Console.WriteLine();
                Console.WriteLine("".PadLeft(4 * (Size), '-'));
            }
        }

        static Behavior getBehaviour(char c)
        {
            switch (c)
            {
                case 'x':
                    return Behavior.Multiply;
                case '/':
                    return Behavior.Divide;
                case '+':
                    return Behavior.Add;
                case '-':
                    return Behavior.Subtract;
                case 'c':
                    return Behavior.Constant;
            }
            return Behavior.Unique;//Shouldn't happen anyway
        }

    }

    class Span//Collection of cells, may be a row, column or a group
    {
        public List<Cell> Cells = new List<Cell>();
        public Behavior Behaviour;

        public int N;
        public char Character;
    }

    class Cell
    {
        public Span Column;
        public Span Row;
        public Span Group;

        public int X;
        public int Y;
    }

    enum Behavior
    {
        Multiply,//Combination of cells multiply to n
        Divide,//Permutation of cells divide to n
        Add,//Combination of cells add to n
        Subtract,//Permutation of cells subtract to n
        Constant,//All cells are always some n
        Unique//All cells form a distinct set of a sequence 1, 2, ..., n
    }


}