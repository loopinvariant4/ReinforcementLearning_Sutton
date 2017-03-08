using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RL_Exercises
{
    public class GridWorldIterativePolicy
    {
        const int GRIDLEN = 4;
        double[,] grid = new double[GRIDLEN, GRIDLEN];
        double[,] newgrid = new double[GRIDLEN, GRIDLEN];
        List<Tuple<int, int>> terminalSquares = new List<Tuple<int, int>>();
        const double probabilityCost = 0.25;
        const double gamma = 1.0;

        public GridWorldIterativePolicy()
        {
            terminalSquares.Add(new Tuple<int, int>(0, 0));
            terminalSquares.Add(new Tuple<int, int>(GRIDLEN - 1, GRIDLEN - 1));

            for (int i = 0; i < GRIDLEN; i++)
            {
                for (int j = 0; j < GRIDLEN; j++)
                {
                    newgrid[i, j] = 0.0;
                    if (isTerminalSquare(i, j))
                    {
                        grid[i, j] = 0.0;
                    }
                    else
                    {
                        grid[i, j] = -1;
                    }
                }
            }

        }

        private void resetGrid(double[,] g)
        {
            g = new double[4, 4];
            for (int i = 0; i < GRIDLEN; i++)
            {
                for (int j = 0; j < GRIDLEN; j++)
                {
                    g[i, j] = 0.0;
                }
            }
        }

        private bool isTerminalSquare(int i, int j)
        {
            Tuple<int, int> t = new Tuple<int, int>(i, j);
            return terminalSquares.Contains(t);
        }

        public void Run()
        {
            drawGrid(0);
            foreach (var i in Enumerable.Range(1, 150))
            {
                nextStep(i);
            }
        }

        private void nextStep(int step)
        {
            for (int i = 0; i < GRIDLEN; i++)
            {
                for (int j = 0; j < GRIDLEN; j++)
                {
                    if (isTerminalSquare(i, j))
                    {
                        newgrid[i, j] = 0.0;
                        continue;
                    }

                    List<Tuple<int, int>> surroundingSquares = getSurroundingSquares(i, j);
                    double sqValue = 0.0;
                    foreach (var sq in surroundingSquares)
                    {
                        sqValue += costOfNextSquare(new Tuple<int, int>(i, j), sq);
                    }
                    newgrid[i, j] = sqValue;
                }
            }
            copyValues(newgrid, grid);
            resetGrid(newgrid);
            drawGrid(step);
        }

        private void copyValues(double[,] src, double[,] dest)
        {
            for (int i = 0; i < GRIDLEN; i++)
            {
                for (int j = 0; j < GRIDLEN; j++)
                {
                    dest[i, j] = src[i, j];
                }
            }
        }
        private double costOfNextSquare(Tuple<int, int> curr, Tuple<int, int> next)
        {
            double reward = -1;
            double vprime;
            if (next.Item1 == GRIDLEN || next.Item1 == -1 || next.Item2 == GRIDLEN || next.Item2 == -1)
            {
                vprime = valueOf(curr);
            }
            else
            {
                vprime = valueOf(next);
            }
            /*
            if (isTerminalSquare(next.Item1, next.Item2))
            {
                reward = 0;
            }
            else
            {
                reward = -1;
            }
            */
            return probabilityCost * (reward + (gamma * vprime));
        }

        private double valueOf(Tuple<int, int> t)
        {
            return grid[t.Item1, t.Item2];
        }

        private List<Tuple<int, int>> getSurroundingSquares(int i, int j)
        {
            var list = new List<Tuple<int, int>>();
            list.Add(new Tuple<int, int>(i + 1, j));
            list.Add(new Tuple<int, int>(i - 1, j));
            list.Add(new Tuple<int, int>(i, j + 1));
            list.Add(new Tuple<int, int>(i, j - 1));
            return list;
        }

        public void drawGrid(int step)
        {
            Console.WriteLine("k={0}", step);
            for (int i = 0; i < GRIDLEN; i++)
            {
                Console.Write("|");
                for (int j = 0; j < GRIDLEN; j++)
                {
                    Console.Write(grid[i, j].ToString("F2") + "\t|");
                }
                Console.WriteLine("");
            }
            Console.WriteLine("\n");
        }


    }
}
