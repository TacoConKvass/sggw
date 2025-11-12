using Console = System.Console;
using Math = System.Math;
using Random = System.Random;
using BitConvert = System.BitConverter;

namespace Gen
{
    public readonly struct Program {
        static readonly double[] res = new double[] { 0, 0 };
        static readonly Random rand = new Random();

        public static void Main(string[] args) {
            var opts = Args.From(args);
            if (opts.invalid) return;

            Console.WriteLine($"Simulating {opts.population_size} specimen over {opts.generations} generations");

            double[] population = new double[opts.population_size];
            for (int i = 0; i < population.Length; i++) {
                population[i] = (rand.NextDouble() - 0.5) * 4; // [0.0, 1.0] -> [-0.5, 0.5] -> [-2.0, 2.0]
            }
 
            double winner = RunSimulation(population, opts.generations);
            Console.WriteLine(
                $"Category | \t\tX\t\t| Fitness\n" +
                $"---------|------------------------------|--------------\n" +
                $"First    | {res[0], -29}| {EvaluateFitness(res[0])}\n" +
                $"Final    | {winner, -29}| {EvaluateFitness(winner)}\n"
            );
        }

        public static double RunSimulation(double[] population, int generations) {
            double winner = -Math.PI; 
            Console.Write("Processing: [");
            for (int gen = 0; gen < generations; gen++) {
                if (gen % Math.Ceiling(generations / 15f) == 0) {
                    Console.Write("|");
                }
                var round_winners = GetFittest(population);
                if (gen == 0)
                    res[0] = round_winners[0];
                winner = round_winners[0];

                Crossover(population, round_winners[1], round_winners[0]);
                Mutate(population);
            }

            Console.Write("]\n\n");
            return winner;
        }

        public static void Crossover(double[] population, double parent_1, double parent_2) {
            for (int i = 0; i < population.Length; i++) {
                int crossing_point = rand.Next(8, 50); // Crossover around the middle

                long parent_1_bits = BitConvert.DoubleToInt64Bits(parent_1);
                long parent_2_bits = BitConvert.DoubleToInt64Bits(parent_2);
                
                bool parent_2_first = rand.Next(0, 2) == 1;
                if (parent_2_first) {
                    var temp = parent_1_bits;
                    parent_1_bits = parent_2_bits;
                    parent_2_bits = temp;
                }

                long full = long.MaxValue << 1 >> 1;
                long mask = full << crossing_point;

                long child_bits = (parent_1_bits & mask) | (parent_2_bits & ~mask);
                double child = BitConvert.Int64BitsToDouble(child_bits);
                population[i] = child;
            }
        }

        public static void Mutate(double[] population) {
            for (int i = 0; i < population.Length; i++) {
                long specimen_bits = BitConvert.DoubleToInt64Bits(population[i]);
                int bit_to_flip = rand.Next(63); // flip any bit
                long mask = 1L << bit_to_flip;
                specimen_bits ^= mask;
                population[i] = BitConvert.Int64BitsToDouble(specimen_bits);
            }
        }

        public static double[] GetFittest(double[] population) {
            double round_winner_1 = 0;
            double fitness_1 = -100;
            double round_winner_2 = 0;
            double fitness_2 = -100;
            
            foreach (double specimen in population) {
                var fitness = EvaluateFitness(specimen);
                if (fitness > fitness_1) {
                    round_winner_2 = round_winner_1;
                    fitness_2 = fitness_1;
                    round_winner_1 = specimen;
                    fitness_1 = fitness;
                } else if (fitness > fitness_2) {
                    round_winner_2 = specimen;
                    fitness_2 = fitness;
                }
            }

            return new double[2] { round_winner_1, round_winner_2 };
        }

        public static double EvaluateFitness(double specimen) {
            if (specimen > 2 || specimen < -2) return -100000000000;
            return Math.Round(specimen * Math.Sin(specimen) * Math.Sin(10 * specimen), 9);
        }
    }

    public struct Args {
        public int population_size;
        public int generations;
        public bool invalid;

        public static readonly Args Invalid = new Args{ population_size = 0, generations = 0, invalid = true };

        public static Args From(string[] args) {
            if (args.Length < 2) {
                ShowUsage();
                return Invalid;
            }

            if (!int.TryParse(args[0], out int population_size)) {
                ShowUsage($"population_size: '{args[0]}' is not an integer\n\n");
                return Invalid;
            }

            if (!int.TryParse(args[1], out int generations)) {
                ShowUsage($"generations: '{args[1]}' is not an integer\n\n");
                return Invalid;
            } return new Args{ population_size = population_size, generations = generations, invalid = false };
        }

        public static void ShowUsage(string additional = "") {
            Console.WriteLine(
                $"{additional}Usage:\n" + 
                $"    dotnet gen.dll population_size generations\n" +
                $"\n" +
                $"Args:\n" +
                $"    population_size:    int\n" +
                $"    generations:        int\n"
            );
        }
    }
}
