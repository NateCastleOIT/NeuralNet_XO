// Program.cs
// Neural Net Example in C#
// by Pete Myers
// OIT, Summer 2017


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NeuralNetExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Instantiate a net with 64 inputs and 2 outputs
            Net theNet = new Net();

            // Add 2 outputs
            Neuron outX = new Neuron();
            Neuron outO = new Neuron();
            theNet.AddOutput(outX);
            theNet.AddOutput(outO);

            // Add 64 inputs
            Neuron[,] inputs = new Neuron[8, 8];

            // Create an 8x8 2d array
            for (int i = 0; i < 64; i++)
            {
                inputs[i / 8, i % 8] = new Neuron();
                theNet.AddInput(inputs[i / 8, i % 8]);

                // Connect each input to both outputs
                theNet.Connect(inputs[i / 8, i % 8], outX);
                theNet.Connect(inputs[i / 8, i % 8], outO);
            }

            // 2 & 3 Read in the training files and create training data

            // Create training data
            List<TrainingData> data = new List<TrainingData>();
            foreach (string trainingFileName in Directory.EnumerateFiles("Training Files"))
            {
                //string filePath = Path.Combine("Train Files", trainingFileName);
                string[] lines = File.ReadAllLines(trainingFileName);

                double[] expectedOutput = new double[2];

                // Determine expected output
                if (lines[0][0] == 'X')
                {
                    expectedOutput[0] = 1.0;
                    expectedOutput[1] = 0.0;
                }
                else if (lines[0][0] == 'O')
                {
                    expectedOutput[0] = 0.0;
                    expectedOutput[1] = 1.0;
                }
                else
                {
                    Console.WriteLine("Error: Invalid training file: " + trainingFileName);
                    continue;
                }

                // Determine the input 8x8 array
                double[] inputValues = new double[64];
                for (int i = 0; i < 8; i++)
                {
                    string[] values = lines[i+1].Split(',');

                    // for each input value in a line
                    for (int j = 0; j < 8; j++)
                    {
                        inputValues[i * 8 + j] = double.Parse(values[j]);
                    }
                }

                data.Add(new TrainingData(inputValues, expectedOutput));

            }
            Console.WriteLine("Read in " + data.Count + " training files.");

            //
            // 4. Train the net
            //

            theNet.Train(data.ToArray());

            //
            // 5. Save the weights in an output file
            //



            //
            // 6. Predict X/O for each test file
            //

            // Create a list of test files
            foreach (string testFileName in Directory.EnumerateFiles("Test Files"))
            {
                string[] lines = File.ReadAllLines(testFileName);

                if (lines[0][0] != '?')
                {
                    Console.WriteLine("Error: Invalid test file: " + testFileName);
                    continue;
                }

                // Determine the input 8x8 array
                for (int i = 0; i < 8; i++)
                {
                    string[] values = lines[i + 1].Split(',');
                    // for each input value in a line
                    for (int j = 0; j < 8; j++)
                    {
                        inputs[i,j].Value = double.Parse(values[j]);

                        // Print the input values
                        Console.Write(inputs[i, j].Value + " ");
                    }
                    Console.WriteLine();
                }

                // Evaluate the net
                theNet.Evaluate();

                // Make a prediction
                Console.WriteLine("outX = " + outX.Value.ToString());
                Console.WriteLine("outO = " + outO.Value.ToString());
                Console.WriteLine("Predicting: " + ((outX.Value > outO.Value) ? 'X' : 'O') + "\n");
            }


            /*
            // create the net with two inputs and one output
            Net theNet = new Net();

            Neuron outy = new Neuron();
            Neuron inx0 = new Neuron();
            Neuron inx1 = new Neuron();

            theNet.AddOutput(outy);
            theNet.AddInput(inx0);
            theNet.AddInput(inx1);

            // connect both inputs to the single output
            theNet.Connect(inx0, outy);
            theNet.Connect(inx1, outy);

            // create training data
            List<TrainingData> data = new List<TrainingData>();
            data.Add(new TrainingData(new double[] { 2.0, 3.0 }, new double[] { 27.0 }));
            data.Add(new TrainingData(new double[] { 4.0, 5.0 }, new double[] { 47.0 }));
            data.Add(new TrainingData(new double[] { -1.0, 6.0 }, new double[] { 39.0 }));
            data.Add(new TrainingData(new double[] { 5.0, -2.0 }, new double[] { 1.0 }));

            // train the net
            theNet.Train(data.ToArray());

            // print trained weights
            Console.WriteLine("Trained:");
            Console.WriteLine("Weight w0 = " + theNet.GetSynapse(inx0, outy).Weight);
            Console.WriteLine("Weight w1 = " + theNet.GetSynapse(inx1, outy).Weight);
            Console.WriteLine();

            // use the trained net to calculate and output for a given input point
            inx0.Value = 1.0;
            inx1.Value = 2.0;
            theNet.Evaluate();

            // print the result
            Console.WriteLine("Activate:");
            Console.WriteLine("In x0 = " + inx0.Value);
            Console.WriteLine("In x1 = " + inx1.Value);
            Console.WriteLine("Out y = " + outy.Value);
            */
        }
    }

    class Net
    {
        public Net()
        {
            inputs = new List<Neuron>();
            outputs = new List<Neuron>();
            synapses = new List<Synapse>();
        }

        private List<Neuron> inputs;
        private List<Neuron> outputs;
        private List<Synapse> synapses;

        public void AddInput(Neuron n) { inputs.Add(n); }
        public void AddOutput(Neuron n) { outputs.Add(n); }

        public void Connect(Neuron from, Neuron to, double weight = 0.0)
        {
            Synapse s = new Synapse();
            s.Axon = from;
            s.Dentrite = to;
            s.Weight = weight;
            synapses.Add(s);
        }

        public Synapse GetSynapse(Neuron from, Neuron to)
        {
            foreach (Synapse s in synapses)
            {
                if (s.Axon == from && s.Dentrite == to)
                    return s;
            }
            return null;
        }

        public void Evaluate()
        {
            foreach (Neuron outNeuron in outputs)
            {
                double value = 0.0;
                foreach (Neuron inNeuron in inputs)
                {
                    Synapse s = GetSynapse(inNeuron, outNeuron);
                    value += s.Weight * inNeuron.Value;
                }
                outNeuron.Value = value;
            }
        }

        public void Train(TrainingData[] data)
        {
            // train the net using gradient descent

            // set weights to random values
            Random r = new Random();
            foreach (Synapse s in synapses)
            {
                s.Weight = r.NextDouble() * 2 - 1.0;  // value between -1.0 and 1.0
            }

            // minimize the error
            double learningRate = 0.01;
            double precision = 0.01;
            double lastError;
            double currentError = double.MaxValue;
            do
            {
                lastError = currentError;
                currentError = 0.0;
                foreach (Synapse s in synapses)
                    s.dW = 0.0;

                // for each training point...
                foreach (TrainingData d in data)
                {
                    // for each output neuron...
                    for (int j = 0; j < outputs.Count; j++)
                    {
                        // calculate Yj from inputs and weights
                        outputs[j].Value = 0.0;
                        for (int i = 0; i < inputs.Count; i++)
                        {
                            Synapse s = GetSynapse(inputs[i], outputs[j]);
                            outputs[j].Value += s.Weight * d.X[i];
                        }

                        // determine error contribution from this output node and training point
                        currentError += Math.Pow(d.T[j] - outputs[j].Value, 2.0);

                        // determine weight gradient for each synapse
                        for (int i = 0; i < inputs.Count; i++)
                        {
                            Synapse s = GetSynapse(inputs[i], outputs[j]);
                            s.dW += (d.T[j] - outputs[j].Value) * d.X[i];
                        }
                    }
                }

                // update error for number of training points
                currentError /= data.Length;

                // adjust weights
                foreach (Synapse s in synapses)
                    s.Weight += learningRate * s.dW;
            }
            while (Math.Abs(currentError - lastError) > precision);
        }
    }

    class Neuron
    {
        public double Value { get; set; }
    }

    class Synapse
    {
        public Neuron Axon { get; set; }        // output
        public Neuron Dentrite { get; set; }    // input
        public double Weight { get; set; }
        public double dW { get; set; }          // for training only
    }

    public class TrainingData
    {
        public TrainingData()
        {
            X = new List<double>();
            T = new List<double>();
        }

        public TrainingData(double[] input, double[] expected)
        {
            X = new List<double>(input);
            T = new List<double>(expected);
        }

        public List<double> X { get; set; }     // input values
        public List<double> T { get; set; }     // expected output values
    }
}
