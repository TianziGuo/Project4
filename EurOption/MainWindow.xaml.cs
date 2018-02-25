using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EurOption
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }
        //define the global value of this project
        double SO, sigma, r, T, K;
        int StepNum, TrailNum,Anti;


        public class Option
        {
            public bool isput = false;
            //define the function that can launch the random process
            public double[] GetSimulations(double SO, double K, double sigma, double r, double T, Int32 StepNum, Int32 TrailNum, double[,] Randoms,int Anti)
            {   //utilize the simulate method to get results
                double[] Result = Simulator.Simulate(SO, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti);

                return Result;
            }
        }
        //define the Greek class for calculating GreekValues of EurOption
        private class Greek
        {
            internal static double[] Greeks(double SO, double K, double sigma, double r, double T, Int32 StepNum, Int32 TrailNum, double[,] Randoms,int Anti)
            {
                double[] GreekValues = new double[10];
                double df = 0.001;
                Option option = new Option();
                double a = option.GetSimulations(SO + SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti)[0];
                double a1 = option.GetSimulations(SO + SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti)[1];
                double b = option.GetSimulations(SO - SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti)[0];
                double b1 = option.GetSimulations(SO - SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti)[1];
                GreekValues[0] = (a - b) / (2 * SO * df);
                // the delta of EurCallOption
                GreekValues[1] = (a1 - b1) / (2 * SO * df);
                //the delta of EurPutOption.
                GreekValues[2] = (a - 2 * option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti)[0] + b) / (SO * df * SO * df);
                //the gamma of EurCallOption.
                GreekValues[3] = (a1 - 2 * option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti)[1] + b1) / (SO * df * SO * df);
                //the gamma of EurPutOption.
                GreekValues[4] = (option.GetSimulations(SO, K, sigma + sigma * df, r, T, StepNum, TrailNum, Randoms,Anti)[0] - option.GetSimulations(SO, K, sigma - sigma * df, r, T, StepNum, TrailNum, Randoms,Anti)[0]) / (2 * sigma * df);
                //the vega of EurCallOption
                GreekValues[5] = (option.GetSimulations(SO, K, sigma + sigma * df, r, T, StepNum, TrailNum, Randoms,Anti)[1] - option.GetSimulations(SO, K, sigma - sigma * df, r, T, StepNum, TrailNum, Randoms,Anti)[1]) / (2 * sigma * df);
                //the vega of EurPutOption.
                GreekValues[6] = (option.GetSimulations(SO, K, sigma, r, T - T * df, StepNum, TrailNum, Randoms,Anti)[0] - option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti)[0]) / (T * df);
                //the theta of EurCallOption
                GreekValues[7] = (option.GetSimulations(SO, K, sigma, r, T - T * df, StepNum, TrailNum, Randoms, Anti)[1] - option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti)[1]) / (T * df);
                //the theta of EurPutOption.
                GreekValues[8] = (option.GetSimulations(SO, K, sigma, r + r * df, T, StepNum, TrailNum, Randoms, Anti)[0] - option.GetSimulations(SO, K, sigma, r - r * df, T, StepNum, TrailNum, Randoms, Anti)[0]) / (2 * r * df);
                //the rho of EurCallOption
                GreekValues[9] = (option.GetSimulations(SO, K, sigma, r + r * df, T, StepNum, TrailNum, Randoms, Anti)[1] - option.GetSimulations(SO, K, sigma, r - r * df, T, StepNum, TrailNum, Randoms, Anti)[1]) / (2 * r * df);
                //the rho of EurPutOption.
                return GreekValues;
            }
        }
        //define the class for Eur Call/Put Price,Call/Put StdEr calculation
        static class Simulator
        {
            internal static double[] Simulate(double SO, double K, double sigma, double r, double t, Int32 StepNum, Int32 TrailNum, double[,] Randoms,int Anti)
            {
                double[] Result = new double[4];
                double[,] set = new double[1, 4];
                double SumCall = 0.0;
                double SumPut = 0.0;
                double StdCallEr = 0.0;
                double StdCallSum = 0.0;
                double StdPutSum = 0.0;
                double StdPutEr = 0.0;

                if (Anti==1)
                {
                    double[,] sims = new double[TrailNum, StepNum];
                    double[,] sims1 = new double[TrailNum, StepNum];
                    for (int p = 0; p < TrailNum; p++)
                    {
                        sims[p, 0] = SO;
                        sims1[p, 0] = SO;
                    }
                    //this loop is used for restoring the simulation result
                    for (int i = 0; i < TrailNum; i++)
                    {
                        for (int j = 1; j < StepNum; j++)
                        {
                            sims[i, j] = sims[i, j - 1] * Math.Exp((r - 0.5 * Math.Pow(sigma, 2)) * (t / (StepNum-1)) + sigma * Math.Pow((t / (StepNum - 1)), 0.5) * Randoms[i, j]);
                            sims1[i, j] = sims1[i, j - 1] * Math.Exp((r - 0.5 * Math.Pow(sigma, 2)) * (t / (StepNum-1)) + sigma * Math.Pow((t / (StepNum - 1)), 0.5) *( -Randoms[i, j]));
                        }
                    }

                    for (int a = 0; a < TrailNum; a++)
                    {
                        SumCall += Math.Max(sims[a, StepNum - 1] - K, 0)+ Math.Max(sims1[a, StepNum - 1] - K, 0);
                        SumPut += Math.Max(K - sims[a, StepNum - 1], 0)+ Math.Max(K - sims1[a, StepNum - 1], 0);
                    }

                    double x = (SumCall / (2*TrailNum)) * Math.Exp(-r * t);
                    double y = (SumPut / (2*TrailNum)) * Math.Exp(-r * t);
                    //calculate the sum of call and put
                    for (int q = 0; q < TrailNum; q++)
                    {
                        StdCallSum = StdCallSum + Math.Pow(((Math.Max(sims[q, StepNum - 1] - K, 0) + Math.Max(sims1[q, StepNum - 1] - K, 0)) / 2 - SumCall / (2 * TrailNum)), 2);
                        StdPutSum = StdPutSum + Math.Pow(((Math.Max(K - sims[q, StepNum - 1], 0)  + Math.Max(K - sims1[q, StepNum - 1], 0))/2 - (SumPut / (2 * TrailNum))), 2);
                    }

                    //calculate the StdError
                    StdCallEr = Math.Sqrt(StdCallSum) / TrailNum;
                    StdPutEr = Math.Sqrt(StdPutSum) / TrailNum;
                   
                    Result[0] = x;
                    Result[1] = y;
                    Result[2] = StdCallEr;
                    Result[3] = StdPutEr;

                }
                else{
                    double[,] sims = new double[TrailNum, StepNum];

                    for (int p = 0; p < TrailNum; p++)
                    {
                        sims[p, 0] = SO;
                    }
                    //this loop is used for restoring the simulation result
                    for (int i = 0; i < TrailNum; i++)
                    {
                        for (int j = 1; j < StepNum; j++)
                        {
                            sims[i, j] = sims[i, j - 1] * Math.Exp((r - 0.5 * Math.Pow(sigma, 2)) * (t /( StepNum-1)) + sigma * Math.Pow((t / (StepNum-1)), 0.5) * Randoms[i, j]);

                        }
                    }

                    for (int a = 0; a < TrailNum; a++)
                    {
                        SumCall += Math.Max(sims[a, StepNum - 1] - K, 0);
                        SumPut += Math.Max(K - sims[a, StepNum - 1], 0);
                    }

                    double x = (SumCall / TrailNum) * Math.Exp(-r * t);
                    double y = (SumPut / TrailNum) * Math.Exp(-r * t);
                    //calculate the sum of call and put
                    for (int q = 0; q < TrailNum; q++)
                    {
                        StdCallSum = StdCallSum + Math.Pow((Math.Max(sims[q, StepNum - 1] - K, 0) - x), 2);
                        StdPutSum = StdPutSum + Math.Pow((Math.Max(K - sims[q, StepNum - 1], 0) - y), 2);
                    }

                    //calculate the StdError
                    StdCallEr = Math.Sqrt((1.0 / (TrailNum - 1)) * StdCallSum) / Math.Sqrt(TrailNum);
                    StdPutEr = Math.Sqrt((1.0 / (TrailNum - 1)) * StdPutSum) / Math.Sqrt(TrailNum);
                    
                    Result[0] = x;
                    Result[1] = y;
                    Result[2] = StdCallEr;
                    Result[3] = StdPutEr;
                }
                return Result;
            }

        }

        //the class is used for generating randomset wich will be used for all the pricing process
        static class RandomNum
        {
            internal static double[,] RandomSet(int TrailNum, int StepNum)
            {

                double[,] RandomSet = new double[TrailNum, StepNum];
                double z1, z2, w = 0.0, C, a, b;
                Random rand1 = new Random();

                for (int n = 0; n < TrailNum; n++)
                {
                    for (int m = 0; (m * 2) < StepNum; m++)
                    {

                        do
                        {
                            a = 2 * rand1.NextDouble() - 1;
                            b = 2 * rand1.NextDouble() - 1;
                            w = Math.Pow(a, 2) + Math.Pow(b, 2);
                        }
                        while (w >= 1);

                        C = Math.Sqrt(-2 * Math.Log(w) / w);
                        z1 = C * a;
                        z2 = C * b;

                        RandomSet[n, 2 * m] = z1;
                        if ((2 * m + 1) < StepNum)
                        {
                            RandomSet[n, 2 * m + 1] = z2;
                        }

                    }
                }
    
                return RandomSet;
            }
        }
        private class EuropOption : Option
        {
            //define a function will accept the values we need
            internal static double[] PriceEurp(double SO, double K, double sigma, double r, double T, Int32 StepNum, Int32 TrailNum, double[,] Randoms,int Anti)
            {
                double[] set = new double[4];
                Option option = new Option();
                set = option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti);
                return set;

            }

        }

        //define a trigger event
        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            //catch the exception
            try
            {
                SO = Convert.ToDouble(this.So.Text);
                K = Convert.ToDouble(this.k.Text);
                sigma = Convert.ToDouble(this.Sigma.Text);
                r = Convert.ToDouble(this.R.Text);
                T = Convert.ToDouble(this.Tenor.Text);
                TrailNum = Convert.ToInt32(this.Trail.Text);
                StepNum = Convert.ToInt32(this.Step.Text);
                Anti= Convert.ToInt32(this.AntiOrNot.Text);


                double[,] Randoms = RandomNum.RandomSet(TrailNum, StepNum);
                double[] EurpCall = EuropOption.PriceEurp(SO, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti);
                double[] GreekValue = Greek.Greeks(SO, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti);
                string OutPut;
                //output the results
                OutPut = Convert.ToString("EurCallprice : " + EurpCall[0] + "\n" + "EurPutprice : " + EurpCall[1] + "\n" + "EurCallStdEr : " + EurpCall[2] + "\n" + "EurPutStdEr : " + EurpCall[3] + "\n" + "EurCallDelta : " + GreekValue[0] + "\n" + "EurPutDelta : " + GreekValue[1] + "\n" + "EurCallGamma : " + GreekValue[2] + "\n" + "EurPutGamma : " + GreekValue[3] + "\n" + "EurCallVega : " + GreekValue[4] + "\n" + "EurPutVega : " + GreekValue[5] + "\n" + "EurCallTheta : " + GreekValue[6] + "\n" + "EurPutTheta : " + GreekValue[7] + "\n" + "EurCallRho : " + GreekValue[8] + "\n" + "EurPutRho : " + GreekValue[9]);
                MessageBox.Show(OutPut);
            }
            catch (Exception)
            {
                MessageBox.Show("All the inputs need to be numbers.");
            }
        }
        public void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }

}