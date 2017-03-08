using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RL_Exercises
{
    public class CarRental
    {
        const int MAXCARSATLOC = 20;
        const int MAXTRANSFERS = 5;
        const double DISCOUNT = 0.9;
        const int RENTAL_AT_A = 3;
        const int RENTAL_AT_B = 4;
        const int RETURN_AT_A = 3;
        const int RETURN_AT_B = 2;
        const int RENTAL_REWARD = 10;
        const int TRANSFER_COST = 2;
        const double DELTA = 0.0001;

        Dictionary<int, double> poissonRentalAtA = new Dictionary<int, double>();
        Dictionary<int, double> poissonRentalAtB = new Dictionary<int, double>();
        Dictionary<int, double> poissonReturnAtA = new Dictionary<int, double>();
        Dictionary<int, double> poissonReturnAtB = new Dictionary<int, double>();

        List<Tuple<int, int>> states = new List<Tuple<int, int>>();
        int[,] policy = new int[MAXCARSATLOC + 1, MAXCARSATLOC + 1];

        List<int> actions = new List<int>();

        public CarRental()
        {
            for (int i = 0; i < MAXCARSATLOC + 1; i++)
            {
                poissonRentalAtA.Add(i, computePoission(i, RENTAL_AT_A));
                poissonRentalAtB.Add(i, computePoission(i, RENTAL_AT_B));
                poissonReturnAtA.Add(i, computePoission(i, RETURN_AT_A));
                poissonReturnAtB.Add(i, computePoission(i, RETURN_AT_B));
            }

            for (int i = 0; i <= MAXCARSATLOC; i++)
                for (int j = 0; j <= MAXCARSATLOC; j++)
                    states.Add(new Tuple<int, int>(i, j));

            foreach (var i in Enumerable.Range(-5, 11))
            {
                actions.Add(i);
            }

        }

        #region poisson pre-compute and functions
        private enum CUSTACT
        {
            RentalA,
            RentalB,
            ReturnA,
            ReturnB
        }

        private double poisson(CUSTACT ca, int n)
        {
            switch (ca)
            {
                case CUSTACT.RentalA:
                    return poissonRentalAtA[n];
                case CUSTACT.RentalB:
                    return poissonRentalAtB[n];
                case CUSTACT.ReturnA:
                    return poissonReturnAtA[n];
                case CUSTACT.ReturnB:
                    return poissonReturnAtB[n];
            }
            throw new Exception("poisson must always return a valid value");
        }
        private double computePoission(int n, int lambda)
        {
            return Math.Pow(lambda, n) * Math.Exp(-lambda) / fact(n);

        }

        private int fact(int n)
        {
            if (n == 0)
            {
                return 1;
            }
            int r = 1;
            for (int i = 1; i <= n; i++)
                r *= i;
            return r;
        }
        #endregion

        public void Run()
        {
            double[,] stateValue = new double[MAXCARSATLOC + 1, MAXCARSATLOC + 1];

            int i = 0;
            while (true)
            {
                i++;
                var newStateValue = iterateState(stateValue);
                if (getStateDiff(stateValue, newStateValue) <= DELTA)
                {
                    Console.WriteLine("Converge Iterations: " + i);
                    //print(newStateValue);
                    stateValue = newStateValue;
                    int changes = improvePolicy(stateValue, policy);
                    if (changes == 0)
                        break;
                }
                else
                {
                    stateValue = newStateValue;
                }
            }
            print(policy, "policy");

            print(stateValue, "state");
        }

        private void print(object newStateValue, string item)
        {
            double[,] state = null;
            int[,] policy = null;

            if(item == "state")
            {
                state = (double[,])newStateValue;
            } else
            {
                policy = (int[,])newStateValue;
            }
            foreach (var i in Enumerable.Range(0, MAXCARSATLOC + 1))
            {
                foreach (var j in Enumerable.Range(0, MAXCARSATLOC))
                {
                    if (item == "state")
                    {
                        Console.Write("{0},\t", state[i, j].ToString("F5"));
                    } else
                    {
                        Console.WriteLine("A: {0}, B: {1}, Action: {2}", i, j, policy[i, j]);
                    }
                }
                Console.WriteLine("");
            }
        }

        private int improvePolicy(double[,] stateValue, int[,] policy)
        {
            int policyChangeCount = 0;
            foreach (var state in states)
            {
                var returnPerAction = new List<Tuple<int, double>>();
                foreach (var action in actions)
                {
                    if ((action >= 0 && state.Item1 >= action) || (action < 0 && state.Item2 >= Math.Abs(action)))
                    {
                        returnPerAction.Add(new Tuple<int, double>(action, getValueOfState(state, action, stateValue)));
                    }
                    else
                    {
                        returnPerAction.Add(new Tuple<int, double>(action, double.NegativeInfinity));
                    }
                }
                var newAction = returnPerAction.OrderByDescending(t => t.Item2).First();
                if (policy[state.Item1, state.Item2] != newAction.Item1)
                {
                    policy[state.Item1, state.Item2] = newAction.Item1;
                    policyChangeCount++;
                }
            }
            Console.WriteLine("Policies change count: " + policyChangeCount);
            return policyChangeCount;
        }

        private double[,] iterateState(double[,] stateValue)
        {
            double[,] newStateValue = new double[MAXCARSATLOC + 1, MAXCARSATLOC + 1];
            foreach (var state in states)
            {
                newStateValue[state.Item1, state.Item2] = getValueOfState(state, policy[state.Item1, state.Item2], stateValue);
            }
            return newStateValue;
        }

        /// <summary>
        /// Here is where we compute the value of a state like [1,2] which means there is 1 car available at Loc1 and 2 cars available at Loc2.
        /// For this, iterate over all possibilities of rental requests, car transfers and multiply the probabilities
        /// </summary>
        /// <param name="state"></param>
        /// <param name="v"></param>
        /// <param name="stateValue"></param>
        /// <returns></returns>
        private double getValueOfState(Tuple<int, int> state, int action, double[,] stateValue)
        {
            double returns = -1 * Math.Abs(action) * TRANSFER_COST;
            foreach (var rentalCountA in Enumerable.Range(0, 11))
            {
                foreach (var rentalCountB in Enumerable.Range(0, 11))
                {
                    int carsAvailableAtA = Math.Min(state.Item1 - action, MAXCARSATLOC);
                    int carsAvailableAtB = Math.Min(state.Item2 + action, MAXCARSATLOC);

                    int actualRentalsAtA = Math.Min(carsAvailableAtA, rentalCountA);
                    int actualRentalsAtB = Math.Min(carsAvailableAtB, rentalCountB);

                    int reward = (actualRentalsAtA + actualRentalsAtB) * RENTAL_REWARD;

                    carsAvailableAtA -= actualRentalsAtA;
                    carsAvailableAtB -= actualRentalsAtB;

                    int carsLeftAtA = Math.Min(carsAvailableAtA + RETURN_AT_A, MAXCARSATLOC);
                    int carsLeftAtB = Math.Min(carsAvailableAtB + RETURN_AT_B, MAXCARSATLOC);

                    double prob = poisson(CUSTACT.RentalA, rentalCountA) * poisson(CUSTACT.RentalB, rentalCountB);
                    returns += prob * (reward + (DISCOUNT * stateValue[carsLeftAtA, carsLeftAtB]));
                    //Console.WriteLine(returns);
                }
            }
            return returns;
        }

        private double getStateDiff(double[,] state, double[,] newState)
        {
            double res = 0;
            for (var i = 0; i < MAXCARSATLOC + 1; i++)
                for (var j = 0; j < MAXCARSATLOC + 1; j++)
                {
                    res += Math.Abs(state[i, j] - newState[i, j]);
                }
            return res;
        }
    }
}
