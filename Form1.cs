using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PickPro_2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filePath = @"C:\Users\dougb\Downloads\lottotexas (4).csv";

            string[] lines = System.IO.File.ReadAllLines(filePath);

            // Hot and Cold Numbers
            var hotNumbers = GetHotNumbers(lines, 6);
            var coldNumbers = GetColdNumbers(lines, 6);

            // Delta System
            var deltaValues = GetDeltaValues(lines);
            var topDeltaValues = GetTopDeltaValues(deltaValues, 6);

            // Last Digit Method
            var lastDigitCounts = GetLastDigitCounts(lines);
            var topLastDigits = GetTopLastDigits(lastDigitCounts, 6);

            for (int i = 0; i < 5; i++)
            {
                var set = GeneratePredictedNumbers(hotNumbers, topDeltaValues, topLastDigits, coldNumbers);
                WriteNumbersToFile(set, "predicted_numbers.txt");
            }
        }

        private void WriteNumbersToFile(int[] numbers, string fileName)
        {
            // Convert the numbers array to a comma-separated string
            string numbersString = string.Join(", ", numbers);

            // Append the numbers string to the text file, creating a new line for each set of numbers
            using (StreamWriter sw = new StreamWriter(fileName, true))
            {
                sw.WriteLine(numbersString);
            }
        }

        private int[] GeneratePredictedNumbers(string[] hotNumbers, int[] topDeltaValues, int[] topLastDigits, string[] coldNumbers)
        {
            // Set the weights for each strategy
            int hotNumbersWeight = 4;
            int deltaValuesWeight = 2;
            int lastDigitsWeight = 1;
            int coldNumbersWeight = 1;

            // Combine the strategies, applying their respective weights
            var weightedHotNumbers = Enumerable.Repeat(hotNumbers, hotNumbersWeight).SelectMany(x => x);
            var weightedTopDeltaValues = Enumerable.Repeat(topDeltaValues.Select(x => x.ToString()).ToArray(), deltaValuesWeight).SelectMany(x => x);
            var weightedTopLastDigits = Enumerable.Repeat(topLastDigits.Select(x => x.ToString()).ToArray(), lastDigitsWeight).SelectMany(x => x);
            var weightedColdNumbers = Enumerable.Repeat(coldNumbers, coldNumbersWeight).SelectMany(x => x);

            var combinedStrategies = weightedHotNumbers
                        .Union(weightedTopDeltaValues)
                        .Union(weightedTopLastDigits)
                        .Union(weightedColdNumbers)
                        .OrderBy(x => Guid.NewGuid())
                        .ToArray();

            // Generate the predicted numbers
            var predictedNumbers = new HashSet<int>();
            foreach (string number in combinedStrategies)
            {
                predictedNumbers.Add(int.Parse(number));
                if (predictedNumbers.Count == 6) break;
            }

            return predictedNumbers.ToArray();
        }


        private string[] GetHotNumbers(string[] lines, int count)
        {
            // Initialize the dictionary to hold the ball frequencies
            Dictionary<string, int> ballFrequencies = new Dictionary<string, int>();

            // Iterate over each row in the file
            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = lines[i].Split(',');

                // Iterate over each ball in the row and update the frequency count
                for (int j = 4; j <= 9; j++)
                {
                    string ball = columns[j];

                    if (ballFrequencies.ContainsKey(ball))
                    {
                        ballFrequencies[ball]++;
                    }
                    else
                    {
                        ballFrequencies[ball] = 1;
                    }
                }
            }

            // Get the top n most frequent balls
            var sortedBalls = ballFrequencies.OrderByDescending(x => x.Value).Take(count);

            // Extract the ball numbers
            string[] hotNumbers = sortedBalls.Select(x => x.Key).ToArray();

            return hotNumbers;
        }

        private string[] GetColdNumbers(string[] lines, int count)
        {
            // Initialize the dictionary to hold the ball frequencies
            Dictionary<string, int> ballFrequencies = new Dictionary<string, int>();

            // Iterate over each row in the file
            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = lines[i].Split(',');

                // Iterate over each ball in the row and update the frequency count
                for (int j = 4; j <= 9; j++)
                {
                    string ball = columns[j];

                    if (ballFrequencies.ContainsKey(ball))
                    {
                        ballFrequencies[ball]++;
                    }
                    else
                    {
                        ballFrequencies[ball] = 1;
                    }
                }
            }

            // Get the top n least frequent balls
            var sortedBalls = ballFrequencies.OrderBy(x => x.Value).Take(count);

            // Extract the ball numbers
            string[] coldNumbers = sortedBalls.Select(x => x.Key).ToArray();

            return coldNumbers;
        }

        private Dictionary<int, int> GetDeltaValues(string[] lines)
        {
            // Initialize the dictionary to hold the delta frequencies
            Dictionary<int, int> deltaFrequencies = new Dictionary<int, int>();

            // Iterate over each row in the file
            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = lines[i].Split(',');
                // Iterate over each ball in the row and compute the delta with the previous ball
                for (int j = 5; j <= 9; j++)
                {
                    int ball = int.Parse(columns[j]);
                    int previousBall = int.Parse(columns[j - 1]);
                    int delta = Math.Abs(ball - previousBall);

                    if (deltaFrequencies.ContainsKey(delta))
                    {
                        deltaFrequencies[delta]++;
                    }
                    else
                    {
                        deltaFrequencies[delta] = 1;
                    }
                }
            }
            return deltaFrequencies;
        }

        private int[] GetTopDeltaValues(Dictionary<int, int> deltaFrequencies, int count)
        {
            // Get the top n most frequent delta values
            var sortedDeltas = deltaFrequencies.OrderByDescending(x => x.Value).Take(count);
            // Extract the delta values
            int[] topDeltaValues = sortedDeltas.Select(x => x.Key).ToArray();

            return topDeltaValues;
        }

        private Dictionary<int, int> GetLastDigitCounts(string[] lines)
        {
            // Initialize the dictionary to hold the last digit frequencies
            Dictionary<int, int> lastDigitFrequencies = new Dictionary<int, int>();
            // Iterate over each row in the file
            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = lines[i].Split(',');

                // Iterate over each ball in the row and count the frequency of the last digit
                for (int j = 4; j <= 9; j++)
                {
                    int ball = int.Parse(columns[j]);
                    int lastDigit = ball % 10;

                    if (lastDigitFrequencies.ContainsKey(lastDigit))
                    {
                        lastDigitFrequencies[lastDigit]++;
                    }
                    else
                    {
                        lastDigitFrequencies[lastDigit] = 1;
                    }
                }
            }

            return lastDigitFrequencies;
        }

        private int[] GetTopLastDigits(Dictionary<int, int> lastDigitFrequencies, int count)
        {
            // Get the top n most frequent last digits
            var sortedLastDigits = lastDigitFrequencies.OrderByDescending(x => x.Value).Take(count);
            int[] topLastDigits = sortedLastDigits.Select(x => x.Key).ToArray();
            return topLastDigits;
        }
    }
}
