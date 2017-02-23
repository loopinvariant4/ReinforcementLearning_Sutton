using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RL_Exercises
{
    public class Tictactoe
    {

        /*
        States are lists of two lists and an index, e.g., ((1 2 3) (4 5 6) index), 
; where the first list is the location of the X's and the second list is 
; the location of the O's.   The index is into a large array holding the value 
; of the states.  There is a one-to-one mapping from index to the lists.  
; The locations refer not to the standard positions, but to the "magic square" 
; positions:
;
;    2 9 4
;    7 5 3
;    6 1 8
;
; Labelling the locations of the Tic-Tac-Toe board in this way is useful because 
; then we can just add up any three positions, and if the sum is 15, then we 
; know they are three in a row.  The following function then tells us if a list 
; of X or O positions contains any that are three in a row.
    */

        int[] magicSquare = new int[] { 2, 9, 4, 7, 5, 3, 6, 1, 8 };
        List<double?> valueTable = new List<double?>(512 * 512);
        List<int> powersOfTwo = new List<int>();
        State initialState = new State(new List<int>(), new List<int>(), 0);
        Random rand = new Random(DateTime.Now.Millisecond);
        const double alpha = 0.5;
        const double epsilon = 10;

        public Tictactoe()
        {
            foreach (var i in Enumerable.Range(0, 9))
            {
                powersOfTwo.Add((int)Math.Pow(2, i));
            }

            foreach(var i in Enumerable.Range(0, 512*512))
            {
                valueTable.Add(null);
            }
            valueTable[initialState.index] = 0.5;
        }
        public bool anyNSumTok(int n, int k, List<int> list)
        {
            if (n == 0 && k == 0)
            {
                return true;
            }
            if (k < 0 || list == null || list.Count == 0)
            {
                return false;
            }
            if (anyNSumTok(n - 1, k - list[0], list.GetRange(1, list.Count - 1)))
            {
                return true;
            }
            if (anyNSumTok(n, k, list.GetRange(1, list.Count - 1)))
            {
                return true;
            }
            return false;
        }

        public void ShowState(State s, bool quiet)
        {
            if (quiet)
            {
                return;
            }

            var i = 0;
            foreach (var location in magicSquare)
            {

                if (s.XMoves.Contains(location))
                {
                    Console.Write(" X");
                }
                else if (s.OMoves.Contains(location))
                {
                    Console.Write(" O");
                }
                else
                {
                    Console.Write(" -");
                }
                if (i == 5)
                {
                    Console.Write("    {0}", valueTable[s.index]);
                }
                if (i % 3 == 2)
                {
                    Console.WriteLine("");
                }
                i++;
            }
        }

        private int StateIndex(List<int> xMoves, List<int> oMoves)
        {
            return xMoves.Sum((x) => powersOfTwo[x - 1]) + (512 * (oMoves.Sum((o) => powersOfTwo[o - 1])));
        }

        private void SetValueState(State s, double value)
        {
            valueTable[s.index] = value;
        }

        private State NextState(string player, State state, int move)
        {
            State s = state.Clone() as State;
            if (player == "X")
            {
                s.XMoves.Add(move);
            }
            else
            {
                s.OMoves.Add(move);
            }
            s.index = StateIndex(s.XMoves, s.OMoves);
            if (valueTable[s.index] == null)
            {
                var value = 0.5;
                if (anyNSumTok(3, 15, s.XMoves))
                {
                    value = 0;
                }
                else if (anyNSumTok(3, 15, s.OMoves))
                {
                    value = 1;
                }
                else if (s.XMoves.Count + s.OMoves.Count == 9)
                {
                    value = 0;
                }
                valueTable[s.index] = value;
            }
            return s;
        }

        private bool IsTerminalState(State s)
        {
            return valueTable[s.index] == 0 || valueTable[s.index] == 1;
        }

        private List<int> PossibleMoves(State s)
        {
            return Enumerable.Range(1, 9).Except(s.XMoves.Union(s.OMoves)).ToList();
        }

        private int RandomMove(State s)
        {
            var moves = PossibleMoves(s);
            return moves[rand.Next(moves.Count)];
        }

        private int GreedyMove(string player, State s)
        {
            var moves = PossibleMoves(s);
            if (moves.Count == 0)
            {
                return 0;
            }
            double? bestValue = -1d;
            var bestMove = 0;

            foreach (var move in moves)
            {
                var val = valueTable[NextState(player, s, move).index];
                if (val > bestValue)
                {
                    bestMove = move;
                    bestValue = val;
                }
            }
            return bestMove;
        }

        private void Update(State s, State newState, bool quiet)
        {
            valueTable[s.index] = valueTable[s.index] + (alpha * valueTable[newState.index] - valueTable[s.index]);
            if(!quiet)
            {
                Console.WriteLine("Value: {0}", valueTable[s.index]);
            }
            
        }

        public double Game(bool quiet = false)
        {
            // X moves first. O is the learning player

            State s = initialState;
            if (!quiet)
            {
                ShowState(s, quiet);
            }

            State newState = null;
            while (true) // X moves first and then O in every loop
            {
                newState = NextState("X", s, RandomMove(s));
                if (IsTerminalState(newState))
                {
                    ShowState(newState, quiet);
                    Update(s, newState, quiet);
                    return valueTable[newState.index].Value; 
                }
                bool explore = rand.Next() < epsilon;
                newState = NextState("O", newState, explore ? RandomMove(newState) : GreedyMove("O", newState));

                if(!explore)
                {
                    Update(s, newState, quiet);
                }
                if(IsTerminalState(newState))
                {
                    ShowState(newState, quiet);
                    return valueTable[newState.index].Value;
                }
                s = newState;
            }
        }

        public void Run()
        {
            int outerloop = 40;
            int innerloop = 100;
            foreach(var i in Enumerable.Range(1, outerloop))
            {
                var k = 0.0;
                foreach(var j in Enumerable.Range(1, innerloop))
                {
                    k += Game(true);
                }
                Console.WriteLine("Value for 100 iterations: {0}", k / innerloop);
            }
        }
    }

    public class State : ICloneable
    {
        public List<int> XMoves = new List<int>();
        public List<int> OMoves = new List<int>();
        public int index;

        public State(List<int> xMoves, List<int> oMoves, int index)
        {
            this.XMoves = xMoves;
            this.OMoves = oMoves;
            this.index = index;

        }

        public object Clone()
        {
            List<int> x = XMoves.GetRange(0, XMoves.Count);
            List<int> o = OMoves.GetRange(0, OMoves.Count);

            return new State(x, o, index);
        }
    }

}
