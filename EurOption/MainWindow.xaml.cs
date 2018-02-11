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

        double SO, sigma, r, T,K;
        int StepNum,TrailNum;
        
        
        public class Option
        {
            public bool isput = false;
            public double[] GetSimulations(double SO, double K,double sigma, double r, double T, Int32 StepNum, Int32 TrailNum, double[,] Randoms)
            {
                double[] Result = Simulator.Simulate(SO, K,sigma,r,T, StepNum, TrailNum,Randoms);
                //for (int index = 0; index < TrailNum; index++)
                //{
                //    for (int index1 = 0; index1 < StepNum; index1++)
                //    {
                //        Console.Write("{0}\t", sims[index, index1].ToString());
                //    }
                //    Console.WriteLine("\n");
                //}

                return Result;
            }
        }
       private class Greek
        {
            internal static double[] Greeks(double SO, double K, double sigma, double r, double T, Int32 StepNum, Int32 TrailNum,double[,] Randoms)
            {
                double[] _Greeks = new double[10];
                double df = 0.001;
                Option option = new Option();
                _Greeks[0] = (option.GetSimulations(SO + SO*df, K, T, r, sigma, StepNum, TrailNum,Randoms)[0] - option.GetSimulations(SO - SO * df, K, T, r, sigma, StepNum, TrailNum, Randoms)[0]) / (2 * SO * df);
                // the delta greek of European Call Option
                _Greeks[1] = (option.GetSimulations(SO + SO * df, K, T, r, sigma, StepNum, TrailNum, Randoms)[1] - option.GetSimulations(SO - SO * df, K, T, r, sigma, StepNum, TrailNum, Randoms)[1]) / (2 * SO * df);
                //the delta greek of European Put Option.
                _Greeks[2] = (option.GetSimulations(SO+ SO * df, K, T, r, sigma, StepNum, TrailNum, Randoms)[0] - 2 * option.GetSimulations(SO, K, T, r, sigma, StepNum, TrailNum, Randoms)[0] + option.GetSimulations(SO - SO * df, K, T, r, sigma, StepNum, TrailNum, Randoms)[0]) / (SO * df* SO * df);
                //the gamma greek of European Call Option.
                _Greeks[3] = (option.GetSimulations(SO+SO * df, K, T, r, sigma, StepNum, TrailNum, Randoms)[1] - 2 * option.GetSimulations(SO, K, T, r, sigma, StepNum, TrailNum, Randoms)[1] + option.GetSimulations(SO - SO * df, K, T, r, sigma, StepNum, TrailNum, Randoms)[1]) / (SO * df* SO * df);
                //the gamma greek of European Put Option.
                _Greeks[4] = (option.GetSimulations(SO, K, T, r, sigma + sigma*df, StepNum, TrailNum, Randoms)[0] - option.GetSimulations(SO, K, T, r, sigma - sigma * df, StepNum, TrailNum, Randoms)[0]) / (2 * sigma * df);
                //the vega greek of European Call Option
                _Greeks[5] = (option.GetSimulations(SO, K, T, r, sigma + sigma * df, StepNum, TrailNum, Randoms)[1] - option.GetSimulations(SO, K, T, r, sigma - sigma * df, StepNum, TrailNum, Randoms)[1]) / (2 * sigma * df);
                //the vega greek of European Put Option.
                _Greeks[6] = (option.GetSimulations(SO, K, T - T * df, r, sigma, StepNum, TrailNum, Randoms)[0] - option.GetSimulations(SO, K, T, r, sigma, StepNum, TrailNum, Randoms)[0]) / (df*T);
                //the theta greek of European Call Option
                _Greeks[7] = (option.GetSimulations(SO, K, T + df*T, r, sigma, StepNum, TrailNum, Randoms)[1] - option.GetSimulations(SO, K, T, r, sigma, StepNum, TrailNum, Randoms)[1]) / (df*T);
                //the theta greek of European Put Option.
                _Greeks[8] = (option.GetSimulations(SO, K, T, r + r*df, sigma, StepNum, TrailNum, Randoms)[0] - option.GetSimulations(SO, K, T, r - r*df, sigma, StepNum, TrailNum, Randoms)[0]) / (2 *r* df);
                //the rho greek of European Call Option
                _Greeks[9] = (option.GetSimulations(SO, K, T, r + r*df, sigma, StepNum, TrailNum, Randoms)[1] - option.GetSimulations(SO, K, T, r - r*df, sigma, StepNum, TrailNum, Randoms)[1]) / (2 * r*df);
                //the rho greek of European Put Option.
                return _Greeks;
            }
        }
        static class Simulator
        {
            internal static double[] Simulate(double SO,double K, double sigma, double r, double t, Int32 StepNum, Int32 TrailNum,double[,] Randoms)
            {
                double[,] sims=new double[TrailNum, StepNum];
                for (int p = 0; p < TrailNum; p++) {
                    sims[p, 0] = SO;
                }
               
               
                for (int i=0;i< TrailNum;i++)
                {
                    for (int j = 1; j < StepNum; j++)
                    {
                        sims[i,j] = sims[i, j - 1] * Math.Exp((r -0.5*Math.Pow(sigma,2) ) * (1.0 / StepNum) + sigma * Math.Pow((1.0 /StepNum),0.5) * Randoms[i, j]);
                        
                    }
                }
                double[,] set = new double[1, 4];
                double SumCall = 0.0;
                double SumPut = 0.0;
                double StdCallEr = 0.0;
                double StdCallSum = 0.0;
                double StdPutSum = 0.0;
                double StdPutEr = 0.0;
                for (int a = 0; a < TrailNum; a++)
                {
                    SumCall += Math.Max(sims[a, StepNum - 1] - K, 0);
                    SumPut += Math.Max(K - sims[a, StepNum - 1], 0);
                }
                //MessageBox.Show(SO.ToString());
                double x = (SumCall / TrailNum) * Math.Exp(-r * t);
                double y = (SumPut / TrailNum) * Math.Exp(-r * t);
                for (int q = 0; q < TrailNum; q++)
                {
                    StdCallSum = StdCallSum + Math.Pow((Math.Max(sims[q, StepNum - 1] - K, 0) - x), 2);
                    StdPutSum = StdPutSum + Math.Pow((K - Math.Max(sims[q, StepNum - 1], 0) - y), 2);
                }
                StdCallEr = Math.Sqrt((1.0 / (TrailNum - 1)) * StdCallSum) * Math.Sqrt(1.0 / TrailNum);
                StdPutEr = Math.Sqrt((1.0 / (TrailNum - 1)) * StdPutSum) * Math.Sqrt(1.0 / TrailNum);
                double[] Result =new double[4];
                Result[0] = x;
                Result[1] = y;
                Result[2] = StdCallEr;
                Result[3] = StdPutEr;
                return Result;
            }
           
        }
       static class RandomNum
        {
            internal static double[,] RandomSet(int TrailNum, int StepNum)
            {
                
                double[,] RandomSet = new double[TrailNum, StepNum];
                double z1, z2, w=0.0, C, a,b;
                Random rand1 = new Random();

                for (int n = 0; n < TrailNum; n++)
                {
                    for (int m = 0; (m * 2) < StepNum; m++)
                    {
                       // Random  rand2 = new Random(~unchecked((int)DateTime.Now.Ticks));
                       
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
        private class EuropOption: Option
        {

            internal static double[] PriceEurp(double SO, double K, double sigma, double r, double T,  Int32 StepNum, Int32 TrailNum,double[,] Randoms)
            {
                double[] set = new double[4];
                Option option=new Option();
                set = option.GetSimulations(SO, K,sigma, r, T, StepNum, TrailNum,Randoms);
                return set;
                
            }
            
        }

       
        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            double[,] Randoms = RandomNum.RandomSet(TrailNum, StepNum);
            double[] EurpCall = EuropOption.PriceEurp( SO, K,sigma, r, T, StepNum, TrailNum,Randoms);
            double[] GreekValue = Greek.Greeks(SO, K, sigma, r, T,  StepNum, TrailNum,Randoms);
           
            Console.Write("{0}\t", EurpCall[0].ToString());
            Console.Write("{0}\t", EurpCall[1].ToString());
            Console.Write("{0}\t", EurpCall[2].ToString());
            Console.Write("{0}\t", EurpCall[3].ToString());
            Console.Write("{0}\t", GreekValue[0].ToString());
            Console.Write("{0}\t", GreekValue[1].ToString());
            Console.Write("{0}\t", GreekValue[2].ToString());
            Console.Write("{0}\t", GreekValue[3].ToString());
            Console.Write("{0}\t", GreekValue[4].ToString());
            Console.Write("{0}\t", GreekValue[5].ToString());
            Console.Write("{0}\t", GreekValue[6].ToString());
            Console.Write("{0}\t", GreekValue[7].ToString());
            Console.Write("{0}\t", GreekValue[8].ToString());
            Console.Write("{0}\t", GreekValue[9].ToString());

        }

        public void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            //int TrailNum = Convert.ToInt32(Trails);
            //int StepNum = Convert.ToInt32(Steps);
            if (!string.IsNullOrEmpty(this.So.Text)) {
             SO = Convert.ToDouble(this.So.Text);
            }
            if (!string.IsNullOrEmpty(this.k.Text))
            {
                 K = Convert.ToDouble(this.k.Text);
            }
            if (!string.IsNullOrEmpty(this.Sigma.Text))
            {
               sigma = Convert.ToDouble(this.Sigma.Text);
            }
            if (!string.IsNullOrEmpty(this.R.Text))
            {
              r = Convert.ToDouble(this.R.Text);
            }
            if (!string.IsNullOrEmpty(this.Tenor.Text))
            {
                T = Convert.ToDouble(this.Tenor.Text);
            }
            if (!string.IsNullOrEmpty(this.Trail.Text))
            {
               TrailNum = Convert.ToInt32(this.Trail.Text);
            }
            if (!string.IsNullOrEmpty(this.Step.Text))
            {
                StepNum = Convert.ToInt32(this.Step.Text);
            }
            

        }
        
    }

}
