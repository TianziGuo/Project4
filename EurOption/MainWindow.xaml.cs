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
using System.Diagnostics;

namespace EurOption
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Stopwatch watch = new Stopwatch();
        public MainWindow()
        {
            InitializeComponent();
        }
        //define the global value of this project
        double SO, sigma, r, T, K,Divident;
        int StepNum, TrailNum,Anti,DeltaBase;


        public class Option
        {
            public bool isput = false;
            //define the function that can launch the random process
            public double[] GetSimulations(double SO, double K, double sigma, double r, double T, Int32 StepNum, Int32 TrailNum, double[,] Randoms,int Anti, double Divident,int DeltaBase)
            {   //utilize the simulate method to get results
                double[] Result = Simulator.Simulate(SO, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti,Divident,DeltaBase);

                return Result;
            }
        }
        //define the Greek class for calculating GreekValues of EurOption
        private class Greek
        {
            internal static double[] Greeks(double SO, double K, double sigma, double r, double T, Int32 StepNum, Int32 TrailNum, double[,] Randoms,int Anti, double Divident,int DeltaBase)
            {
                double[] GreekValues = new double[10];
                double df = 0.001;
                Option option = new Option();
                double a = option.GetSimulations(SO + SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti, Divident, DeltaBase)[0];
                double a1 = option.GetSimulations(SO + SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti, Divident, DeltaBase)[1];
                double b = option.GetSimulations(SO - SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti,Divident, DeltaBase)[0];
                double b1 = option.GetSimulations(SO - SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti,Divident, DeltaBase)[1];
                GreekValues[0] = (a - b) / (2 * SO * df);
                // the delta of EurCallOption
                GreekValues[1] = (a1 - b1) / (2 * SO * df);
                //the delta of EurPutOption.
                GreekValues[2] = (a - 2 * option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti, Divident, DeltaBase)[0] + b) / (SO * df * SO * df);
                //the gamma of EurCallOption.
                GreekValues[3] = (a1 - 2 * option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti, Divident, DeltaBase)[1] + b1) / (SO * df * SO * df);
                //the gamma of EurPutOption.
                GreekValues[4] = (option.GetSimulations(SO, K, sigma + sigma * df, r, T, StepNum, TrailNum, Randoms,Anti, Divident, DeltaBase)[0] - option.GetSimulations(SO, K, sigma - sigma * df, r, T, StepNum, TrailNum, Randoms,Anti, Divident, DeltaBase)[0]) / (2 * sigma * df);
                //the vega of EurCallOption
                GreekValues[5] = (option.GetSimulations(SO, K, sigma + sigma * df, r, T, StepNum, TrailNum, Randoms,Anti, Divident, DeltaBase)[1] - option.GetSimulations(SO, K, sigma - sigma * df, r, T, StepNum, TrailNum, Randoms,Anti, Divident, DeltaBase)[1]) / (2 * sigma * df);
                //the vega of EurPutOption.
                GreekValues[6] = (option.GetSimulations(SO, K, sigma, r, T - T * df, StepNum, TrailNum, Randoms,Anti, Divident, DeltaBase)[0] - option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti, Divident, DeltaBase)[0]) / (T * df);
                //the theta of EurCallOption
                GreekValues[7] = (option.GetSimulations(SO, K, sigma, r, T - T * df, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase)[1] - option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase)[1]) / (T * df);
                //the theta of EurPutOption.
                GreekValues[8] = (option.GetSimulations(SO, K, sigma, r + r * df, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase)[0] - option.GetSimulations(SO, K, sigma, r - r * df, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase)[0]) / (2 * r * df);
                //the rho of EurCallOption
                GreekValues[9] = (option.GetSimulations(SO, K, sigma, r + r * df, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase)[1] - option.GetSimulations(SO, K, sigma, r - r * df, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase)[1]) / (2 * r * df);
                //the rho of EurPutOption.
                return GreekValues;
            }
        }
        //define the class for Eur Call/Put Price,Call/Put StdEr calculation
        static class Simulator
        {
            internal static double[] Simulate(double SO, double K, double sigma, double r, double t, Int32 StepNum, Int32 TrailNum, double[,] Randoms,int Anti,double div, int Deltabase)
            {
                double[] Result = new double[4];
                double[,] set = new double[1, 4];
                double SumCall = 0.0;
                double SumPut = 0.0;
                double StdCallEr = 0.0;
                double StdCallSum = 0.0;
                double StdPutSum = 0.0;
                double StdPutEr = 0.0;
                double CallSd, PutSd;

                if (Anti==1)
                {
                    //double[,] sims = new double[TrailNum, StepNum];
                    //double[,] sims1 = new double[TrailNum, StepNum];
                    double x, y;
                    if (Deltabase == 1)
                    {
                        double dt, St, St1, Stn, Stn1, cv, cv1, CT, PT, Sum_CT, Sum_PT, Sum_PT2, Sum_CT2, t1, CallDelta = 0, CallDelta1 = 0, PutDelta = 0, PutDelta1 = 0, beta1, nudt, sigsdt, erddt;//I don't know what the beta1 is.
                        dt = t / (StepNum);
                        nudt = (r - div - 0.5 * Math.Pow(sigma, 2)) * dt;
                        sigsdt = sigma * Math.Sqrt(dt);
                        erddt = Math.Exp((r - div) * dt);
                        Sum_CT = 0;
                        Sum_CT2 = 0;
                        Sum_PT = 0;
                        Sum_PT2 = 0;
                        beta1 = -1;
                        double[] DeltaSet = new double[2];
                        double[] DeltaSet1 = new double[2];
                        //this loop is used for restoring the simulation result

                        for (int i = 0; i < TrailNum; i++)
                        {
                            St = SO;
                            St1 = SO;
                            cv = 0;
                            cv1 = 0;
                            for (int j = 1; j <= StepNum; j++)
                            {
                                t1 = t - dt* (j - 1);
                                DeltaSet = Black_Scholes_Delta.Black_Scholes_DeltaSet(St, t1, K, sigma, r);
                                DeltaSet1 = Black_Scholes_Delta.Black_Scholes_DeltaSet(St1, t1, K, sigma, r);
                                CallDelta = DeltaSet[0];
                                PutDelta = DeltaSet[1];
                                CallDelta1 = DeltaSet1[0];
                                PutDelta1 = DeltaSet1[1];
                                Stn = St * Math.Exp(nudt + sigsdt * Randoms[i, j-1]);
                                Stn1 = St1 * Math.Exp(nudt + sigsdt * (-Randoms[i, j-1]));
                                cv = cv + CallDelta * (Stn - St * erddt) + CallDelta1 * (Stn1 - St1 * erddt);
                                cv1 = cv1 + PutDelta * (Stn - St * erddt) + PutDelta1 * (Stn1 - St1 * erddt);
                                St = Stn;
                                St1 = Stn1;

                            }
                            CT = Math.Max(St - K, 0) + Math.Max(St1 - K, 0) + beta1 * cv;
                            Sum_CT = Sum_CT + CT;
                            Sum_CT2 = Sum_CT2 + CT * CT;
                            PT = Math.Max(K - St, 0) + Math.Max(K - St1, 0) + beta1 * cv1;
                            Sum_PT = Sum_PT + PT;
                            Sum_PT2 = Sum_PT2 + PT * PT;
                        }

                        x = Sum_CT * Math.Exp(-r * t) / (2 * TrailNum); ;
                        CallSd = Math.Sqrt((Sum_CT2 - Sum_CT * Sum_CT / (2 * TrailNum)) * Math.Exp(-2 * r * t) / (2 * TrailNum - 1));
                        StdCallEr = CallSd / Math.Sqrt(2 * TrailNum);
                        y = Sum_PT * Math.Exp(-r * t) / (2 * TrailNum);
                        PutSd = Math.Sqrt((Sum_PT2 - Sum_PT * Sum_PT / (2 * TrailNum)) * Math.Exp(-2 * r * t) / (2 * TrailNum - 1));
                        StdPutEr = PutSd / Math.Sqrt(2 * TrailNum);
                    }
                    else {
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
                                sims[i, j] = sims[i, j - 1] * Math.Exp((r - 0.5 * Math.Pow(sigma, 2)) * (t / (StepNum - 1)) + sigma * Math.Pow((t / (StepNum - 1)), 0.5) * Randoms[i, j]);
                                sims1[i, j] = sims1[i, j - 1] * Math.Exp((r - 0.5 * Math.Pow(sigma, 2)) * (t / (StepNum - 1)) + sigma * Math.Pow((t / (StepNum - 1)), 0.5) * (-Randoms[i, j]));
                            }
                        }

                        for (int a = 0; a < TrailNum; a++)
                        {
                            SumCall += Math.Max(sims[a, StepNum - 1] - K, 0) + Math.Max(sims1[a, StepNum - 1] - K, 0);
                            SumPut += Math.Max(K - sims[a, StepNum - 1], 0) + Math.Max(K - sims1[a, StepNum - 1], 0);
                        }

                        x = (SumCall / (2 * TrailNum)) * Math.Exp(-r * t);
                        y = (SumPut / (2 * TrailNum)) * Math.Exp(-r * t);
                        //calculate the sum of call and put
                        for (int q = 0; q < TrailNum; q++)
                        {
                            StdCallSum = StdCallSum + Math.Pow(((Math.Max(sims[q, StepNum - 1] - K, 0) + Math.Max(sims1[q, StepNum - 1] - K, 0)) / 2 - SumCall / (2 * TrailNum)), 2);
                            StdPutSum = StdPutSum + Math.Pow(((Math.Max(K - sims[q, StepNum - 1], 0) + Math.Max(K - sims1[q, StepNum - 1], 0)) / 2 - (SumPut / (2 * TrailNum))), 2);
                        }

                        //calculate the StdError
                        StdCallEr = Math.Sqrt(StdCallSum) * Math.Exp(-r * t) / TrailNum;
                        StdPutEr = Math.Sqrt(StdPutSum) * Math.Exp(-r * t) / TrailNum;
                    }

                    Result[0] = x;
                    Result[1] = y;
                    Result[2] = StdCallEr;
                    Result[3] = StdPutEr;

                }
                else{
                    double[,] sims = new double[TrailNum, StepNum];
                    double x, y;
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
                    if (Deltabase==1) {
                        double dt, St, Stn,cv,cv1, CT, PT, Sum_CT, Sum_PT, Sum_PT2, Sum_CT2, t1, CallDelta = 0,PutDelta = 0,beta1, nudt, sigsdt, erddt;//I don't know what the beta1 is.
                        dt = t / (StepNum);
                        nudt = (r - div - 0.5 * Math.Pow(sigma, 2)) * dt;
                        sigsdt = sigma * Math.Sqrt(dt);
                        erddt = Math.Exp((r - div) * dt);
                        Sum_CT = 0;
                        Sum_CT2 = 0;
                        Sum_PT = 0;
                        Sum_PT2 = 0;
                        beta1 = -1;
                        double[] DeltaSet = new double[2];
                        //this loop is used for restoring the simulation result

                        for (int i = 0; i < TrailNum; i++)
                        {
                            St = SO;
                            cv = 0;
                            cv1 = 0;
                            for (int j = 1; j <= StepNum; j++)
                            {
                                t1 = t - dt* (j - 1);
                                DeltaSet = Black_Scholes_Delta.Black_Scholes_DeltaSet(St, t1, K, sigma, r);
                                CallDelta = DeltaSet[0];
                                PutDelta = DeltaSet[1];
                                Stn = St * Math.Exp(nudt + sigsdt * Randoms[i, j-1]);
                                cv = cv + CallDelta * (Stn - St * erddt) ;
                                cv1 = cv1 + PutDelta * (Stn - St * erddt);
                                St = Stn;

                            }
                            CT = Math.Max(St - K, 0) + beta1 * cv;
                            Sum_CT = Sum_CT + CT;
                            Sum_CT2 = Sum_CT2 + CT * CT;
                            PT = Math.Max(K - St, 0) + beta1 * cv1;
                            Sum_PT = Sum_PT + PT;
                            Sum_PT2 = Sum_PT2 + PT * PT;
                        }

                        x = Sum_CT * Math.Exp(-r * t) / ( TrailNum); ;
                        CallSd = Math.Sqrt((Sum_CT2 - Sum_CT * Sum_CT / ( TrailNum)) * Math.Exp(-2 * r * t) / (TrailNum - 1));
                        StdCallEr = CallSd / Math.Sqrt( TrailNum);
                        y = Sum_PT * Math.Exp(-r * t) / (TrailNum);
                        PutSd = Math.Sqrt((Sum_PT2 - Sum_PT * Sum_PT / (TrailNum)) * Math.Exp(-2 * r * t) / (TrailNum - 1));
                        StdPutEr = PutSd / Math.Sqrt(TrailNum);
                    }
                    else {
                        for (int a = 0; a < TrailNum; a++)
                        {
                            SumCall += Math.Max(sims[a, StepNum - 1] - K, 0);
                            SumPut += Math.Max(K - sims[a, StepNum - 1], 0);
                        }

                        x = (SumCall / TrailNum) * Math.Exp(-r * t);
                        y = (SumPut / TrailNum) * Math.Exp(-r * t);
                        //calculate the sum of call and put
                        for (int q = 0; q < TrailNum; q++)
                        {
                            StdCallSum = StdCallSum + Math.Pow((Math.Max(sims[q, StepNum - 1] - K, 0) - x), 2);
                            StdPutSum = StdPutSum + Math.Pow((Math.Max(K - sims[q, StepNum - 1], 0) - y), 2);
                        }

                        //calculate the StdError
                        StdCallEr = Math.Sqrt((1.0 / (TrailNum - 1)) * StdCallSum) / Math.Sqrt(TrailNum);
                        StdPutEr = Math.Sqrt((1.0 / (TrailNum - 1)) * StdPutSum) / Math.Sqrt(TrailNum);
                    }
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

        static class Black_Scholes_Delta
        {
            internal static double[] Black_Scholes_DeltaSet(double St,double t1,double K,double sigma,double r)
            {
                double[] Black_Scholes_DeltaSet = new double[2];
                double d1 = (Math.Log(St / K) + (r + sigma * sigma / 2) * t1) / (sigma * Math.Sqrt(t1));
               // double d2 = -(d1 - sigma * Math.Sqrt(t1));
                double A1 = 0.31938153;
                double A2 = -0.356563782;
                double A3 = 1.781477937;
                double A4 = -1.821255978;
                double A5 = 1.330274429;
                double RSQRT2PI = 0.39894228040143267793994605993438;

                double M = 1.0 / (1.0 + 0.2316419 * Math.Abs(d1));
               // double M1 = 1.0 / (1.0 + 0.2316419 * Math.Abs(d2));
                Black_Scholes_DeltaSet[0] = RSQRT2PI * Math.Exp(-0.5 * d1 * d1) * (M * (A1 + M * (A2 + M * (A3 + M * (A4 + M * A5)))));
               // Black_Scholes_DeltaSet[1] = RSQRT2PI * Math.Exp(-0.5 * d2 * d2) * (M1 * (A1 + M1 * (A2 + M1 * (A3 + M1 * (A4 + M1 * A5)))));

                if (d1 > 0)
                {
                    Black_Scholes_DeltaSet[0] = 1.0 - Black_Scholes_DeltaSet[0];
                }
                Black_Scholes_DeltaSet[1] = Black_Scholes_DeltaSet[0] - 1;


                return Black_Scholes_DeltaSet;

            }



        }

            private class EuropOption : Option
        {
            //define a function will accept the values we need
            internal static double[] PriceEurp(double SO, double K, double sigma, double r, double T, Int32 StepNum, Int32 TrailNum, double[,] Randoms,int Anti,double Dividend,int DeltaBase)
            {
                double[] set = new double[4];
                Option option = new Option();
                set = option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti, Dividend, DeltaBase);
                return set;

            }

        }

        //define a trigger event
        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            watch.Start();
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
                Divident=Convert.ToDouble(this.Div.Text);
                DeltaBase= Convert.ToInt32(this.deltabased.Text);

                double[,] Randoms = RandomNum.RandomSet(TrailNum, StepNum);
                double[] EurpCall = EuropOption.PriceEurp(SO, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti, Divident,DeltaBase);
                double[] GreekValue = Greek.Greeks(SO, K, sigma, r, T, StepNum, TrailNum, Randoms,Anti, Divident, DeltaBase);
                string OutPut;
                //output the results
                watch.Stop();
                OutPut = Convert.ToString("EurCallprice : " + EurpCall[0] + "\n" + "EurPutprice : " + EurpCall[1] + "\n" + "EurCallStdEr : " + EurpCall[2] + "\n" + "EurPutStdEr : " + EurpCall[3] + "\n" + "EurCallDelta : " + GreekValue[0] + "\n" + "EurPutDelta : " + GreekValue[1] + "\n" + "EurCallGamma : " + GreekValue[2] + "\n" + "EurPutGamma : " + GreekValue[3] + "\n" + "EurCallVega : " + GreekValue[4] + "\n" + "EurPutVega : " + GreekValue[5] + "\n" + "EurCallTheta : " + GreekValue[6] + "\n" + "EurPutTheta : " + GreekValue[7] + "\n" + "EurCallRho : " + GreekValue[8] + "\n" + "EurPutRho : " + GreekValue[9]+"\n"+"Timer : "+watch.Elapsed.TotalSeconds.ToString());
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