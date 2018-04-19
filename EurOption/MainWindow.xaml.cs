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
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;

namespace EurOption
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Stopwatch watch = new Stopwatch();
        public static int cores = Environment.ProcessorCount;
        //private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        public MainWindow()
        {
            InitializeComponent();
        }

        delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        //This delegate tell your GUI the background calculation is finished
        //it has no parameter
        delegate void finish();
        void increase(int input)
        {
            //Test a control component on the Form1 to see if invoke is required.
            if (!CheckAccess())
            {
                //if invoke required, begin invoke and increase the bar
                UpdateProgressBarDelegate progressdelegate = new UpdateProgressBarDelegate(ProgressBar1.SetValue);
                Value = Value + input;
                Dispatcher.BeginInvoke(progressdelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(Value) });
            }
            else
            {
                //if invoke is not necessary, just increase the bar.
                //the form is in a different thread, you need to invoke.

                UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(ProgressBar1.SetValue);

                for (int i = 0; i < 100; i++)
                {
                    Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(i + input) });
                }
            }

        }

        //define the global value of this project
        double SO, sigma, r, T, K, Divident,Rebate,BarrierNumber;
        int StepNum, TrailNum, Anti, DeltaBase, MultiThread, AsiaOption, EurOption,RangeOption,DigitalOption,LookbackOption,BarrierOption,Value = 0;

        private void Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        public class Option
        {
            public bool isput = false;
            //define the function that can launch the random process
            public double[,] GetSimulations(double SO, double K, double sigma, double r, double T, Int32 StepNum, Int32 TrailNum, double[,] Randoms, int Anti, double Divident, int DeltaBase, int MultiThread, int Eur)
            {   //utilize the simulate method to get results
                double[,] Result;
                if (Eur!=1)
                {
                    Result = AsiaSimulate.Simulate(SO, sigma, r, T, StepNum, TrailNum, Randoms, Anti, MultiThread);
                    //MessageBox.Show(Convert.ToString(Result[0,0]));
                }
                else {
                    Result = Simulator.Simulate(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread);
                }
                return Result;
            }
        }
        //define the Greek class for calculating GreekValues of EurOption
        private class Greek
        {
            internal static double[] Greeks(double SO, double K, double sigma, double r, double T, Int32 StepNum, Int32 TrailNum, double[,] Randoms, int Anti, double Divident, int DeltaBase, int MultiThread,int Eur, double Rebate, int BarrierOption, double BarrierNumber)
            {
                double[] GreekValues = new double[10];
                double df = 0.001;
                Option option = new Option();
                if (Eur == 1)
                {
                    double[] a = EuropOption.PriceEurp(SO + SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread,Eur, Rebate, BarrierOption, BarrierNumber);
                    // double a1 = EuropOption.PriceEurp(SO + SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread)[1];
                    double[] b = EuropOption.PriceEurp(SO - SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                    //double b1 = EuropOption.PriceEurp(SO - SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread)[1];
                    double[] c = EuropOption.PriceEurp(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                    double[] d = EuropOption.PriceEurp(SO, K, sigma + sigma * df, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                    double[] e = EuropOption.PriceEurp(SO, K, sigma - sigma * df, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                    double[] f = EuropOption.PriceEurp(SO, K, sigma, r, T - T * df, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                    double[] g = EuropOption.PriceEurp(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                    double[] h = EuropOption.PriceEurp(SO, K, sigma, r + r * df, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                    double[] i = EuropOption.PriceEurp(SO, K, sigma, r - r * df, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber); ;
                    GreekValues[0] = (a[0] - b[0]) / (2 * SO * df);
                    // the delta of EurCallOption
                    GreekValues[1] = (a[1] - b[1]) / (2 * SO * df);
                    //the delta of EurPutOption.
                    GreekValues[2] = (a[0] - 2 *c [0] + b[0]) / (SO * df * SO * df);
                    //the gamma of EurCallOption.
                    GreekValues[3] = (a[1] - 2 * c[1] + b[1]) / (SO * df * SO * df);
                    //the gamma of EurPutOption.
                    GreekValues[4] = (d[0] - e[0]) / (2 * sigma * df);
                    //the vega of EurCallOption
                    GreekValues[5] = (d[1] - e[1]) / (2 * sigma * df);
                    //the vega of EurPutOption.
                    GreekValues[6] = (f[0] - g[0]) / (T * df);
                    //the theta of EurCallOption
                    GreekValues[7] = (f[1] - g[1]) / (T * df);
                    //the theta of EurPutOption.
                    GreekValues[8] = (h[0] - i[0]) / (2 * r * df);
                    //the rho of EurCallOption
                    GreekValues[9] = (h[1] - i[1]) / (2 * r * df);
                    //the rho of EurPutOption.
                }
                else
                {
                    if (Eur==2) {
                        double[] a = AsianOption.PriceAsian(SO + SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                        // double a1 = EuropOption.PriceEurp(SO + SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread)[1];
                        double[] b = AsianOption.PriceAsian(SO - SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                        //double b1 = EuropOption.PriceEurp(SO - SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread)[1];
                        double[] c = AsianOption.PriceAsian(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                        double[] d = AsianOption.PriceAsian(SO, K, sigma + sigma * df, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                        double[] e = AsianOption.PriceAsian(SO, K, sigma - sigma * df, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                        double[] f = AsianOption.PriceAsian(SO, K, sigma, r, T - T * df, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                        double[] g = AsianOption.PriceAsian(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                        double[] h = AsianOption.PriceAsian(SO, K, sigma, r + r * df, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                        double[] i = AsianOption.PriceAsian(SO, K, sigma, r - r * df, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                        GreekValues[0] = (a[0] - b[0]) / (2 * SO * df);
                        // the delta of EurCallOption
                        GreekValues[1] = (a[1] - b[1]) / (2 * SO * df);
                        //the delta of EurPutOption.
                        GreekValues[2] = (a[0] - 2 * c[0] + b[0]) / (SO * df * SO * df);
                        //the gamma of EurCallOption.
                        GreekValues[3] = (a[1] - 2 * c[1] + b[1]) / (SO * df * SO * df);
                        //the gamma of EurPutOption.
                        GreekValues[4] = (d[0] - e[0]) / (2 * sigma * df);
                        //the vega of EurCallOption
                        GreekValues[5] = (d[1] - e[1]) / (2 * sigma * df);
                        //the vega of EurPutOption.
                        GreekValues[6] = (f[0] - g[0]) / (T * df);
                        //the theta of EurCallOption
                        GreekValues[7] = (f[1] - g[1]) / (T * df);
                        //the theta of EurPutOption.
                        GreekValues[8] = (h[0] - i[0]) / (2 * r * df);
                        //the rho of EurCallOption
                        GreekValues[9] = (h[1] - i[1]) / (2 * r * df);
                        //the rho of EurPutOption.
                    }
                    else
                    {
                        if (Eur == 3)
                        {
                            double[] a = DigitalsOption.PriceDigital(SO + SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                            // double a1 = EuropOption.PriceEurp(SO + SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread)[1];
                            double[] b = DigitalsOption.PriceDigital(SO - SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                            //double b1 = EuropOption.PriceEurp(SO - SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread)[1];
                            double[] c = DigitalsOption.PriceDigital(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                            double[] d = DigitalsOption.PriceDigital(SO, K, sigma + sigma * df, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                            double[] e = DigitalsOption.PriceDigital(SO, K, sigma - sigma * df, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                            double[] f = DigitalsOption.PriceDigital(SO, K, sigma, r, T - T * df, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                            double[] g = DigitalsOption.PriceDigital(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                            double[] h = DigitalsOption.PriceDigital(SO, K, sigma, r + r * df, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                            double[] i = DigitalsOption.PriceDigital(SO, K, sigma, r - r * df, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                            GreekValues[0] = (a[0] - b[0]) / (2 * SO * df);
                            // the delta of EurCallOption
                            GreekValues[1] = (a[1] - b[1]) / (2 * SO * df);
                            //the delta of EurPutOption.
                            GreekValues[2] = (a[0] - 2 * c[0] + b[0]) / (SO * df * SO * df);
                            //the gamma of EurCallOption.
                            GreekValues[3] = (a[1] - 2 * c[1] + b[1]) / (SO * df * SO * df);
                            //the gamma of EurPutOption.
                            GreekValues[4] = (d[0] - e[0]) / (2 * sigma * df);
                            //the vega of EurCallOption
                            GreekValues[5] = (d[1] - e[1]) / (2 * sigma * df);
                            //the vega of EurPutOption.
                            GreekValues[6] = (f[0] - g[0]) / (T * df);
                            //the theta of EurCallOption
                            GreekValues[7] = (f[1] - g[1]) / (T * df);
                            //the theta of EurPutOption.
                            GreekValues[8] = (h[0] - i[0]) / (2 * r * df);
                            //the rho of EurCallOption
                            GreekValues[9] = (h[1] - i[1]) / (2 * r * df);
                            //the rho of EurPutOption.
                        }
                        else
                        {
                            if (Eur==4) {
                                double[] a = RangeOptions.PriceRange(SO + SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                // double a1 = EuropOption.PriceEurp(SO + SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread)[1];
                                double[] b = RangeOptions.PriceRange(SO - SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                //double b1 =  RangeOption.PriceRange(SO - SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread)[1];
                                double[] c = RangeOptions.PriceRange(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                double[] d = RangeOptions.PriceRange(SO, K, sigma + sigma * df, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                double[] e = RangeOptions.PriceRange(SO, K, sigma - sigma * df, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                double[] f = RangeOptions.PriceRange(SO, K, sigma, r, T - T * df, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                double[] g = RangeOptions.PriceRange(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                double[] h = RangeOptions.PriceRange(SO, K, sigma, r + r * df, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                double[] i = RangeOptions.PriceRange(SO, K, sigma, r - r * df, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                GreekValues[0] = (a[0] - b[0]) / (2 * SO * df);
                                // the delta of EurCallOption
                                GreekValues[1] = (a[1] - b[1]) / (2 * SO * df);
                                //the delta of EurPutOption.
                                GreekValues[2] = (a[0] - 2 * c[0] + b[0]) / (SO * df * SO * df);
                                //the gamma of EurCallOption.
                                GreekValues[3] = (a[1] - 2 * c[1] + b[1]) / (SO * df * SO * df);
                                //the gamma of EurPutOption.
                                GreekValues[4] = (d[0] - e[0]) / (2 * sigma * df);
                                //the vega of EurCallOption
                                GreekValues[5] = (d[1] - e[1]) / (2 * sigma * df);
                                //the vega of EurPutOption.
                                GreekValues[6] = (f[0] - g[0]) / (T * df);
                                //the theta of EurCallOption
                                GreekValues[7] = (f[1] - g[1]) / (T * df);
                                //the theta of EurPutOption.
                                GreekValues[8] = (h[0] - i[0]) / (2 * r * df);
                                //the rho of EurCallOption
                                GreekValues[9] = (h[1] - i[1]) / (2 * r * df);
                                //the rho of EurPutOption.
                            }
                            else
                            {
                                if (Eur==5) {
                                    double[] a = LookbackOptions.PriceLookback(SO + SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    // double a1 = Option.PriceEurp(SO + SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread)[1];
                                    double[] b = LookbackOptions.PriceLookback(SO - SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    //double b1 = EuropOption.PriceEurp(SO - SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread)[1];
                                    double[] c = LookbackOptions.PriceLookback(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    double[] d = LookbackOptions.PriceLookback(SO, K, sigma + sigma * df, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    double[] e = LookbackOptions.PriceLookback(SO, K, sigma - sigma * df, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    double[] f = LookbackOptions.PriceLookback(SO, K, sigma, r, T - T * df, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    double[] g = LookbackOptions.PriceLookback(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    double[] h = LookbackOptions.PriceLookback(SO, K, sigma, r + r * df, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    double[] i = LookbackOptions.PriceLookback(SO, K, sigma, r - r * df, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    GreekValues[0] = (a[0] - b[0]) / (2 * SO * df);
                                    // the delta of EurCallOption
                                    GreekValues[1] = (a[1] - b[1]) / (2 * SO * df);
                                    //the delta of EurPutOption.
                                    GreekValues[2] = (a[0] - 2 * c[0] + b[0]) / (SO * df * SO * df);
                                    //the gamma of EurCallOption.
                                    GreekValues[3] = (a[1] - 2 * c[1] + b[1]) / (SO * df * SO * df);
                                    //the gamma of EurPutOption.
                                    GreekValues[4] = (d[0] - e[0]) / (2 * sigma * df);
                                    //the vega of EurCallOption
                                    GreekValues[5] = (d[1] - e[1]) / (2 * sigma * df);
                                    //the vega of EurPutOption.
                                    GreekValues[6] = (f[0] - g[0]) / (T * df);
                                    //the theta of EurCallOption
                                    GreekValues[7] = (f[1] - g[1]) / (T * df);
                                    //the theta of EurPutOption.
                                    GreekValues[8] = (h[0] - i[0]) / (2 * r * df);
                                    //the rho of EurCallOption
                                    GreekValues[9] = (h[1] - i[1]) / (2 * r * df);
                                    //the rho of EurPutOption.
                                }
                               else
                                {
                                    double[] a = BarrierOptions.PriceBarrier(SO + SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    // double a1 = EuropOption.PriceEurp(SO + SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread)[1];
                                    double[] b = BarrierOptions.PriceBarrier(SO - SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    //double b1 =  RangeOption.PriceRange(SO - SO * df, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread)[1];
                                    double[] c = BarrierOptions.PriceBarrier(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    double[] d = BarrierOptions.PriceBarrier(SO, K, sigma + sigma * df, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    double[] e = BarrierOptions.PriceBarrier(SO, K, sigma - sigma * df, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    double[] f = BarrierOptions.PriceBarrier(SO, K, sigma, r, T - T * df, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    double[] g = BarrierOptions.PriceBarrier(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    double[] h = BarrierOptions.PriceBarrier(SO, K, sigma, r + r * df, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    double[] i = BarrierOptions.PriceBarrier(SO, K, sigma, r - r * df, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, Eur, Rebate, BarrierOption, BarrierNumber);
                                    GreekValues[0] = (a[0] - b[0]) / (2 * SO * df);
                                    // the delta of EurCallOption
                                    GreekValues[1] = (a[1] - b[1]) / (2 * SO * df);
                                    //the delta of EurPutOption.
                                    GreekValues[2] = (a[0] - 2 * c[0] + b[0]) / (SO * df * SO * df);
                                    //the gamma of EurCallOption.
                                    GreekValues[3] = (a[1] - 2 * c[1] + b[1]) / (SO * df * SO * df);
                                    //the gamma of EurPutOption.
                                    GreekValues[4] = (d[0] - e[0]) / (2 * sigma * df);
                                    //the vega of EurCallOption
                                    GreekValues[5] = (d[1] - e[1]) / (2 * sigma * df);
                                    //the vega of EurPutOption.
                                    GreekValues[6] = (f[0] - g[0]) / (T * df);
                                    //the theta of EurCallOption
                                    GreekValues[7] = (f[1] - g[1]) / (T * df);
                                    //the theta of EurPutOption.
                                    GreekValues[8] = (h[0] - i[0]) / (2 * r * df);
                                    //the rho of EurCallOption
                                    GreekValues[9] = (h[1] - i[1]) / (2 * r * df);
                                    //the rho of EurPutOption.
                                }
                            }
                        }
                    }
                }

                return GreekValues;
            }
        }
        //define the class for Eur Call/Put Price,Call/Put StdEr calculation
        static class Simulator
        {
            internal static double[,] Simulate(double SO, double K, double sigma, double r, double t, Int32 StepNum, Int32 TrailNum, double[,] Randoms, int Anti, double div, int Deltabase, int MultiThread)
            {
                //if (MultiThread == 0) { cores = 1; }
                if (Anti == 1)
                {
                    if (Deltabase == 1)
                    {
                        double dt, nudt, sigsdt, erddt;//I don't know what the beta1 is.
                        dt = t / (StepNum);
                        nudt = (r - div - 0.5 * Math.Pow(sigma, 2)) * dt;
                        sigsdt = sigma * Math.Sqrt(dt);
                        erddt = Math.Exp((r - div) * dt);
                        double[,] sims = new double[2*TrailNum, StepNum+1];
                        

                        //this loop is used for restoring the simulation result
                        if (MultiThread == 1)
                        {
                            Action<object> MyAct1 = x1 =>
                            {
                                Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, a =>
                               {
                                 sims[a, 0] = SO;
                                 sims[a+ TrailNum, 0] = SO;
                                 for (int j = 1; j <= StepNum; j++)
                                 {
                                     sims[a, j] = sims[a, j - 1] * Math.Exp(nudt + sigsdt * Randoms[a, j - 1]);
                                     sims[a+ TrailNum, j] = sims[a + TrailNum, j - 1] * Math.Exp(nudt + sigsdt * (-Randoms[a, j - 1]));
                                 }

                                });
                            };
                            Thread th1 = new Thread(new ParameterizedThreadStart(MyAct1));
                            th1.Start();
                            th1.Join();
                            th1.Abort();
                            return sims;
                        }
                        else
                        {
                          Action<object> MyAct = x1 =>
                          {
                            for (int i = 0; i < TrailNum; i++)
                            {
                                  sims[i, 0] = SO;
                                  sims[i + TrailNum, 0] = SO;
                                  for (int j = 1; j <= StepNum; j++)
                                  {
                                      sims[i, j] = sims[i, j - 1] * Math.Exp(nudt + sigsdt * Randoms[i, j - 1]);
                                      sims[i + TrailNum, j] = sims[i + TrailNum, j - 1] * Math.Exp(nudt + sigsdt * (-Randoms[i, j - 1]));
                                  }
                            }
                          };
                          Thread th = new Thread(new ParameterizedThreadStart(MyAct));
                          th.Start();
                          th.Join();
                          th.Abort();
                          return sims;
                        }
                    }
                    else
                    {
                        double[,] sims = new double[2*TrailNum, StepNum];
                        //int Thread1 = 1;
                        if (MultiThread == 1)
                        {
                            Action<object> MyAct = x1 =>
                            {
                                Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, a =>
                              {
                                sims[a, 0] = SO;
                                sims[a+ TrailNum, 0] = SO;
                                for (int j = 1; j < StepNum; j++)
                                {
                                    sims[a, j] = sims[a, j - 1] * Math.Exp((r - 0.5 * Math.Pow(sigma, 2)) * (t / (StepNum - 1)) + sigma * Math.Pow((t / (StepNum - 1)), 0.5) * Randoms[a, j]);
                                    sims[a+ TrailNum, j] = sims[a + TrailNum, j - 1] * Math.Exp((r - 0.5 * Math.Pow(sigma, 2)) * (t / (StepNum - 1)) + sigma * Math.Pow((t / (StepNum - 1)), 0.5) * (-Randoms[a, j]));

                                }
                              });
                            };
                            Thread th = new Thread(new ParameterizedThreadStart(MyAct));
                            th.Start();
                            th.Join();
                            th.Abort();
                            return sims;

                        }
                        else
                        {
                            Action<object> MyAct = x1 =>
                            {
                                //this loop is used for restoring the simulation result
                                for (int i = 0; i < TrailNum; i++)
                                {
                                    sims[i, 0] = SO;
                                    sims[i + TrailNum, 0] = SO;
                                    for (int j = 1; j < StepNum; j++)
                                    {
                                        sims[i, j] = sims[i, j - 1] * Math.Exp((r - 0.5 * Math.Pow(sigma, 2)) * (t / (StepNum - 1)) + sigma * Math.Pow((t / (StepNum - 1)), 0.5) * Randoms[i, j]);
                                        sims[i + TrailNum, j] = sims[i + TrailNum, j - 1] * Math.Exp((r - 0.5 * Math.Pow(sigma, 2)) * (t / (StepNum - 1)) + sigma * Math.Pow((t / (StepNum - 1)), 0.5) * (-Randoms[i, j]));
                                    }
                                }
                            };
                            Thread th = new Thread(new ParameterizedThreadStart(MyAct));
                            th.Start();
                            th.Join();
                            th.Abort();
                            return sims;
                        }
                    }

                }
                else
                {
                    if (Deltabase == 1)
                    {
                        double dt, t1,nudt, sigsdt, erddt;//I don't know what the beta1 is.
                        dt = t / (StepNum);
                        nudt = (r - div - 0.5 * Math.Pow(sigma, 2)) * dt;
                        sigsdt = sigma * Math.Sqrt(dt);
                        erddt = Math.Exp((r - div) * dt);
                        double[,] sims = new double[TrailNum,StepNum+1]; 

                        if (MultiThread==1) {
                            Action<object> MyAct = x1 =>
                            {
                                //this loop is used for restoring the simulation result

                                   Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, a =>
                                   {
                                        sims[a, 0] = SO;
                                        for (int j = 1; j <= StepNum; j++)
                                        {
                                            t1 = t - dt * (j - 1);
                                            sims[a, j] = sims[a, j - 1] * Math.Exp(nudt + sigsdt * Randoms[a, j - 1]);
                                        }
                                   });
                                  
                            };
                            Thread th = new Thread(new ParameterizedThreadStart(MyAct));
                            th.Start();
                            th.Join();
                            th.Abort();
                            return sims;
                        }
                        else {
                            Action<object> MyAct = x1 =>
                            {
                                //this loop is used for restoring the simulation result

                                for (int i = 0; i < TrailNum; i++)
                                {
                                    sims[i, 0] = SO;
                                    for (int j = 1; j <= StepNum; j++)
                                    {
                                        t1 = t - dt * (j - 1);
                                        sims[i, j] = sims[i, j - 1] * Math.Exp(nudt + sigsdt * Randoms[i, j - 1]);
                                    }
                                }
                            };
                            Thread th = new Thread(new ParameterizedThreadStart(MyAct));
                            th.Start();
                            th.Join();
                            th.Abort();
                            return sims;
                        }
                    }
                    else
                    {
                        double[,] sims = new double[TrailNum, StepNum];
                        if (MultiThread == 1)
                        {
                            Action<object> MyAct = x1 =>
                            {
                                Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, a =>
                                {
                                    sims[a, 0] = SO;
                                    for (int j = 1; j < StepNum; j++)
                                    {
                                        sims[a, j] = sims[a, j - 1] * Math.Exp((r - 0.5 * Math.Pow(sigma, 2)) * (t / (StepNum - 1)) + sigma * Math.Pow((t / (StepNum - 1)), 0.5) * Randoms[a, j]);
                                    }
                                });
                            };
                            Thread th1 = new Thread(new ParameterizedThreadStart(MyAct));
                            th1.Start();
                            th1.Join();
                            th1.Abort();
                            return sims;
                        }
                        else
                        {
                            Action<object> MyAct2 = x2 =>
                            {
                                //this loop is used for restoring the simulation result
                                for (int i = 0; i < TrailNum; i++)
                                {
                                    sims[i,0] = SO;
                                    for (int j = 1; j < StepNum; j++)
                                    {
                                        sims[i, j] = sims[i, j - 1] * Math.Exp((r - 0.5 * Math.Pow(sigma, 2)) * (t / (StepNum - 1)) + sigma * Math.Pow((t / (StepNum - 1)), 0.5) * Randoms[i, j]);

                                    }
                                }
                            };
                            Thread th4 = new Thread(new ParameterizedThreadStart(MyAct2));
                            th4.Start();
                            th4.Join();
                            th4.Abort();
                            return sims;
                            
                        }
                    }
                }
                
            }

        }
        static class Ienum
        {
            public static IEnumerable<long> Step(long startIndex, long endIndex, long stepSize)
            {
                for (long i = startIndex; i < endIndex; i = i + stepSize)
                {
                    yield return i;
                }
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

            internal static double[,] RandomSetMulti1(int TrailNum, int StepNum,int Asia)
            {

                //this is the road which use the multithreading
                double[,] RandomSetMulti;
                Random rand2 = new Random();
                //int Thread1 = 1;
                if (Asia!=1) {
                    RandomSetMulti = new double[TrailNum, StepNum+1];
                    for (long a = 0; a < TrailNum; a++)
                    {
                        for (long b = 0; b <= StepNum; b++)
                        {
                            RandomSetMulti[a, b] = mt_rand();//Call box muller to generate ONE NORMAL random
                        }

                    }
                }
                else{ 
                RandomSetMulti = new double[TrailNum, StepNum];
                for (long a = 0; a < TrailNum; a++)
                {
                    for (long b = 0; b < StepNum; b++)
                    {
                        RandomSetMulti[a, b] = mt_rand();//Call box muller to generate ONE NORMAL random
                    }

                }
            }
                return RandomSetMulti;

                double mt_rand()
                //Box Muller Norm Random  have to lock when nulti threading

                {
                    //var obj = new Object();
                    double randn1 = 0, randn2 = 0;

                    //for parallel computing in the future, lock to ensure serial access
                    // The item you locked is your Random Class
                    randn1 = rand2.NextDouble();
                    randn2 = rand2.NextDouble();

                    double z1 = 0;
                    z1 = Math.Sqrt((-2) * Math.Log(randn1)) * Math.Cos(2 * Math.PI * randn2);
                    return z1;
                }
            }



            internal static double[,] RandomSetMulti(int TrailNum, int StepNum,int AsiaO)
            {

                //this is the road which use the multithreading
                double[,] RandomSetMulti;
                Random rand2 = new Random();
                if (AsiaO != 1)
                {
                   // MessageBox.Show("1");
                    RandomSetMulti = new double[TrailNum, StepNum+1];
                  
                    // int Thread1 = 1;

                    Action<object> MyAct = x =>
                    {
                        Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, a =>
                        {
                            for (long b = 0; b <=StepNum; b++)
                            {
                                RandomSetMulti[a, b] = mt_rand();//Call box muller to generate ONE NORMAL random
                            }

                        });
                    };
                    Thread th = new Thread(new ParameterizedThreadStart(MyAct));
                    th.Start();
                    th.Join();
                    th.Abort();
                }
                else
                {
                    
                    RandomSetMulti = new double[TrailNum, StepNum];
                 //  Random rand2 = new Random();
                    // int Thread1 = 1;

                    Action<object> MyAct = x =>
                    {
                        Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, a =>
                        {
                            for (long b = 0; b < StepNum; b++)
                            {
                                RandomSetMulti[a, b] = mt_rand();//Call box muller to generate ONE NORMAL random
                        }

                        });
                    };
                    Thread th = new Thread(new ParameterizedThreadStart(MyAct));
                    th.Start();
                    th.Join();
                    th.Abort();
                }
                return RandomSetMulti;

                double mt_rand()
                //Box Muller Norm Random  have to lock when nulti threading
                {
                    //var obj = new Object();
                    double randn1 = 0, randn2 = 0;

                    //for parallel computing in the future, lock to ensure serial access
                    // The item you locked is your Random Class
                    lock (rand2) randn1 = rand2.NextDouble();
                    lock (rand2) randn2 = rand2.NextDouble();

                    double z1 = 0;
                    z1 = Math.Sqrt((-2) * Math.Log(randn1)) * Math.Cos(2 * Math.PI * randn2);
                    return z1;
                }
            }

        }

        static class Black_Scholes_Delta
        {
            internal static double Black_Scholes_DeltaSet(double St, double t1, double K, double sigma, double r)
            {
                double d1 = (Math.Log(St / K) + (r + 0.5 * sigma * sigma) * t1) / (sigma * Math.Sqrt(t1));
                
                    return cnd(d1);
            }
        }

        static class AsiaSimulate
        {

            //Stock price generator:
            internal static double[,] Simulate(double S, double sigma, double drift, double T, int steps, int Sim_Number, double[,] rnd_mtx, int Reduction_or_Not,int Multi) //drift is risk-free-rate.
            {
                //MessageBox.Show("3");
                int Trials = Sim_Number + Reduction_or_Not * Sim_Number;
                double[,] StockPrice_matrix = new double[Trials, steps + 1];
                double Time_Interval = T / steps;
                if (Multi==0)
                {
                    cores = 1;
                }
                Action<object> MyAct1 = x1 =>
                {
                    Parallel.ForEach(Ienum.Step(0, Sim_Number, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, i =>
                    {
                        if (Reduction_or_Not == 0)
                        {
                            StockPrice_matrix[i, 0] = S;
                        }
                        else
                        {
                            StockPrice_matrix[i, 0] = S;
                            StockPrice_matrix[i + Sim_Number, 0] = S;
                        }
                        //StockPrice_matrix[i, 0] = S;
                        for (int j = 1; j <= steps; j++)
                        {
                            if (Reduction_or_Not == 0)
                            {
                                StockPrice_matrix[i, j] = StockPrice_matrix[i, j - 1] * Math.Exp((drift - sigma * sigma / 2) * Time_Interval + sigma * Math.Sqrt(Time_Interval) * rnd_mtx[i, j]);
                            }
                            else
                            {
                                StockPrice_matrix[i, j] = StockPrice_matrix[i, j - 1] * Math.Exp((drift - sigma * sigma / 2) * Time_Interval + sigma * Math.Sqrt(Time_Interval) * rnd_mtx[i, j]);
                                StockPrice_matrix[i + Sim_Number, j] = StockPrice_matrix[i + Sim_Number, j - 1] * Math.Exp((drift - sigma * sigma / 2) * Time_Interval - sigma * Math.Sqrt(Time_Interval) * rnd_mtx[i, j]);
                            }
                        }
                        });
                    };
                Thread th = new Thread(new ParameterizedThreadStart(MyAct1));
                th.Start();
                th.Join();
                th.Abort();
                return StockPrice_matrix;
            }
        }

        public static double cnd(double x)
        {
            double L = 0.0;
            double K = 0.0;
            double dCND = 0.0;
            const double a1 = 0.31938153;
            const double a2 = -0.356563782;
            const double a3 = 1.781477937;
            const double a4 = -1.821255978;
            const double a5 = 1.330274429;
            L = Math.Abs(x);
            K = 1.0 / (1.0 + 0.2316419 * L);
            dCND = 1.0 - 1.0 / Math.Sqrt(2 * Convert.ToDouble(Math.PI.ToString())) *
            Math.Exp(-L * L / 2.0) * (a1 * K + a2 * K * K + a3 * K * K * K +
            a4 * K * K * K * K + a5 * K * K * K * K * K);
            if (x < 0)
            {
                return 1.0 - dCND;
            }
            else
            {
                return dCND;
            }
        }

        //BarrierOption
        private class BarrierOptions : Option
        {
            //define a function will accept the values we need
            internal static double[] PriceBarrier(double SO, double K, double sigma, double r, double T, Int32 StepNum, Int32 TrailNum, double[,] Randoms, int Anti, double Dividend, int DeltaBase, int MultiThread, int Eur, double Rebate, int BarrierOption, double BarrierNumber)
            {
                int trails = TrailNum + Anti * TrailNum;
                Option option = new Option();
                double[,] sims = option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Dividend, DeltaBase, MultiThread, Eur);
                double[,] CV1_mtx = new double[TrailNum, StepNum + 1];
                double[,] CV2_mtx = new double[TrailNum, StepNum + 1];
                double[,] CV3_mtx = new double[TrailNum, StepNum + 1];
                double[,] CV4_mtx = new double[TrailNum, StepNum + 1];
                double dt = T / Convert.ToDouble(StepNum); // Time steps
                double CT; // Temporary Payoff at each node.
                double Sum_CT = 0.0; // Sum of payoff at each node.
                double Sum_CT2 = 0.0; // Sum of squared payoff at each node.
                double PT; // Temporary Payoff at each node.
                double Sum_PT = 0.0; // Sum of payoff at each node.
                double Sum_PT2 = 0.0; // Sum of squared payoff at each node.
                double[,] delta1 = new double[TrailNum, StepNum];
                double[,] delta2 = new double[TrailNum, StepNum];
                double[] Indicator = new double[trails];
                Action<object> MyAct = x1 =>
                {
                    if (MultiThread == 0)
                {
                    cores = 1;
                }
                if (DeltaBase == 1)  // W/ CV.
                {
                    double Beta = -1.0;
                    double erddt = Math.Exp(r * dt);
                    //double CV1 = 0.0, CV2 = 0.0; // The total change in stock price after each delta hedging.

                    if (Anti == 1) //anti
                    {
                        Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, i =>
                        {
                            //Calculate payoff at each node.
                            for (int j = 1; j <= StepNum; j++)
                            {
                                delta1[i, j - 1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(sims[i, j - 1], T - dt * (j - 1), K, sigma, r);
                                delta2[i, j - 1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(sims[i + TrailNum, j - 1], T - dt * (j - 1), K, sigma, r);

                                CV1_mtx[i, j] = delta1[i, j - 1] * (sims[i, j] - sims[i, j - 1] * erddt);
                                CV2_mtx[i, j] = delta2[i, j - 1] * (sims[i + TrailNum, j] - sims[i + TrailNum, j - 1] * erddt);
                                CV3_mtx[i, j] = (delta1[i, j - 1] - 1) * (sims[i, j] - sims[i, j - 1] * erddt);
                                CV4_mtx[i, j] = (delta2[i, j - 1] - 1) * (sims[i + TrailNum, j] - sims[i + TrailNum, j - 1] * erddt);
                            }

                        });

                        //Calculate Indicator of each node.
                        for (int i = 0; i < trails; i++)
                        {
                            double minimum = SO;
                            double maximum = SO;
                            for (int j = 0; j <= StepNum; j++)
                            {
                                if (sims[i, j] < minimum)
                                    minimum = sims[i, j];
                                if (sims[i, j] > maximum)
                                    maximum = sims[i, j];
                            }
                            if (BarrierOption == 1 && minimum > BarrierNumber)
                                Indicator[i] = 1;
                            else if (BarrierOption == 2 && maximum < BarrierNumber)
                                Indicator[i] = 1;
                            else if (BarrierOption == 3 && minimum <= BarrierNumber)
                                Indicator[i] = 1;
                            else if (BarrierOption == 4 && maximum >= BarrierNumber)
                                Indicator[i] = 1;
                        }

                        for (int i = 0; i < TrailNum; i++)
                        {
                            for (int j = 0; j <= StepNum; j++)
                            {
                                CV1_mtx[i, 0] += CV1_mtx[i, j];
                                CV2_mtx[i, 0] += CV2_mtx[i, j];
                                CV3_mtx[i, 0] += CV3_mtx[i, j];
                                CV4_mtx[i, 0] += CV4_mtx[i, j];
                            }
                            CT = 0.5 * (Math.Max(0, sims[i, StepNum] - K) * Indicator[i] + Beta * CV1_mtx[i, 0] + Math.Max(0, sims[i + TrailNum, StepNum] - K) * Indicator[i + TrailNum] + Beta * CV2_mtx[i, 0]);
                            Sum_CT += CT;
                            Sum_CT2 += CT * CT;
                            PT = 0.5 * (Math.Max(0, K - sims[i, StepNum]) * Indicator[i] + Beta * CV3_mtx[i, 0] + Math.Max(0, K - sims[i + TrailNum, StepNum]) * Indicator[i + TrailNum] + Beta * CV4_mtx[i, 0]);
                            Sum_PT += PT;
                            Sum_PT2 += PT * PT;
                        }
                    }
                    else //without anti
                    {
                        Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism =cores }, i =>
                        {
                            double minimum = SO;
                            double maximum = SO;
                            //Calculate payoff at each node.
                            for (int j = 1; j <= StepNum; j++)
                            {
                                delta1[i, j - 1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(sims[i, j - 1], T - dt * (j - 1), K, sigma, r);
                                CV1_mtx[i, j] = delta1[i, j - 1] * (sims[i, j] - sims[i, j - 1] * erddt);
                                CV2_mtx[i, j] = (delta1[i, j - 1] - 1) * (sims[i, j] - sims[i, j - 1] * erddt);
                              
                                if (sims[i, j] < minimum)
                                    minimum = sims[i, j];
                                if (sims[i, j] > maximum)
                                    maximum = sims[i, j];
                            }
                            if (BarrierOption == 1 && minimum > BarrierNumber)
                                Indicator[i] = 1;
                            else if (BarrierOption == 2 && maximum < BarrierNumber)
                                Indicator[i] = 1;
                            else if (BarrierOption == 3 && minimum <= BarrierNumber)
                                Indicator[i] = 1;
                            else if (BarrierOption == 4 && maximum >= BarrierNumber)
                                Indicator[i] = 1;
                        });


                        for (int i = 0; i < TrailNum; i++)
                        {
                            for (int j = 0; j <= StepNum; j++)
                            {
                                CV1_mtx[i, 0] += CV1_mtx[i, j];
                                CV2_mtx[i, 0] += CV2_mtx[i, j];
                            }
                          
                            CT = Math.Max(0, sims[i, StepNum] - K) * Indicator[i] + Beta * CV1_mtx[i, 0];
                            Sum_CT += CT;
                            Sum_CT2 += CT * CT;
                            PT = Math.Max(0, K - sims[i, StepNum]) * Indicator[i] + Beta * CV2_mtx[i, 0];
                            Sum_PT += PT;
                            Sum_PT2 += PT * PT;
                        }
                    }
                }

                //W/O CV.
                else
                {
                    double[] CT_mtx = new double[TrailNum];
                    double[] PT_mtx = new double[TrailNum];
                    //Calculate Indicator of each node.
                    for (int i = 0; i < trails; i++)
                    {
                        double minimum = SO;
                        double maximum = SO;
                        for (int j = 1; j <= StepNum; j++)
                        {
                            if (sims[i, j] < minimum)
                                minimum = sims[i, j];
                            if (sims[i, j] > maximum)
                                maximum = sims[i, j];
                        }
                        if (BarrierOption == 1 && minimum > BarrierNumber)
                            Indicator[i] = 1;
                        else if (BarrierOption == 2 && maximum < BarrierNumber)
                            Indicator[i] = 1;
                        else if (BarrierOption == 3 && minimum <= BarrierNumber)
                            Indicator[i] = 1;
                        else if (BarrierOption == 4 && maximum >= BarrierNumber)
                            Indicator[i] = 1;
                    }

                    Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, i =>
                    {
                        if (Anti == 0) // without Antithetic 
                        {
                          
                             CT_mtx[i] = Math.Max(sims[i, StepNum] - K, 0) * Indicator[i];
                             PT_mtx[i] = Math.Max(K - sims[i, StepNum], 0) * Indicator[i];
                        }
                        else // With antithetic.
                        {
                             CT_mtx[i] = 0.5 * (Math.Max(sims[i, StepNum] - K, 0) * Indicator[i] + Math.Max(sims[i + TrailNum, StepNum] - K, 0) * Indicator[i + TrailNum]);
                             PT_mtx[i] = 0.5 * (Math.Max(K - sims[i, StepNum], 0) * Indicator[i] + Math.Max(K - sims[i + TrailNum, StepNum], 0) * Indicator[i + TrailNum]);
                        }
                    });
                    for (int i = 0; i < TrailNum; i++)
                    {
                        Sum_CT += CT_mtx[i];
                        Sum_CT2 += CT_mtx[i] * CT_mtx[i];
                        Sum_PT+= PT_mtx[i];
                        Sum_PT2 += PT_mtx[i] * PT_mtx[i];
                    }

                }
                };
                Thread th = new Thread(new ParameterizedThreadStart(MyAct));
                th.Start();
                th.Join();
                th.Abort();
                double[] result = new double[4];

                result[0] = Sum_CT / TrailNum * Math.Exp(-r * T);
                result[1] = Sum_PT / TrailNum * Math.Exp(-r * T);
                double SD = Math.Sqrt((Sum_CT2 - Sum_CT * Sum_CT / TrailNum) * Math.Exp(-2 * r * T) / (TrailNum - 1));
                result[2] = SD / Math.Sqrt(TrailNum);
                double SD1 = Math.Sqrt((Sum_PT2 - Sum_PT * Sum_PT / TrailNum) * Math.Exp(-2 * r * T) / (TrailNum - 1));
                result[3] = SD1 / Math.Sqrt(TrailNum);
                return result;

            }

        }

        //LookbackOption
            private class LookbackOptions : Option
        {
            //define a function will accept the values we need
            internal static double[] PriceLookback(double SO, double K, double sigma, double r, double T, Int32 StepNum, Int32 TrailNum, double[,] Randoms, int Anti, double Dividend, int DeltaBase, int MultiThread, int Eur, double Rebate, int BarrierOption,double BarrierNumber)
            {
                int trails = TrailNum + Anti * TrailNum;
                Option option = new Option();
                double[,] sims = option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Dividend, DeltaBase, MultiThread, Eur);
                double[,] CV1_mtx = new double[TrailNum, StepNum + 1];
                double[,] CV2_mtx = new double[TrailNum, StepNum + 1];
                double[,] CV3_mtx = new double[TrailNum, StepNum + 1];
                double[,] CV4_mtx = new double[TrailNum, StepNum + 1];
                double dt = T / Convert.ToDouble(StepNum); // Time steps
                double CT; // Temporary Payoff at each node.
                double Sum_CT = 0.0; // Sum of payoff at each node.
                double Sum_CT2 = 0.0; // Sum of squared payoff at each node.
                double PT; // Temporary Payoff at each node.
                double Sum_PT = 0.0; // Sum of payoff at each node.
                double Sum_PT2 = 0.0; // Sum of squared payoff at each node.
                double[,] delta1 = new double[TrailNum, StepNum];
                double[,] delta2 = new double[TrailNum, StepNum];
                double[] ExtremeValue_St = new double[trails];
                double[] ExtremeValue_St1 = new double[trails];
                Action<object> MyAct = x1 =>
                {
                    if (MultiThread == 0)
                {
                    cores = 1;
                }
                //Find Max stock price at each node

                for (int i = 0; i < trails; i++)

                    {
                        double max_st = SO;
                        for (int j = 1; j <= StepNum; j++)
                        {
                            if (sims[i, j] > max_st)
                                max_st = sims[i, j];
                        }
                        ExtremeValue_St[i] = max_st;
                    }
               
                    for (int i = 0; i < trails; i++)
                    {
                        double min_st = SO;
                        for (int j = 1; j <= StepNum; j++)
                        {
                            if (sims[i, j] < min_st)
                                min_st = sims[i, j];
                        }
                        ExtremeValue_St1[i] = min_st;
                    }

                if (DeltaBase == 1)  // W/ CV.
                {
                    double Beta = -1.0;
                    double erddt = Math.Exp(r * dt);
                    //double CV1 = 0.0, CV2 = 0.0; // The total change in stock price after each delta hedging.

                    if (Anti == 1) //anti
                    {
                        Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, i =>
                        {
                            //Calculate payoff at each node.
                            for (int j = 1; j <= StepNum; j++)
                            {
                                delta1[i, j - 1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(sims[i, j - 1], T - dt * (j - 1), K, sigma, r);
                                delta2[i, j - 1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(sims[i + TrailNum, j - 1], T - dt * (j - 1), K, sigma, r);

                                CV1_mtx[i, j] = delta1[i, j - 1] * (sims[i, j] - sims[i, j - 1] * erddt);
                                CV2_mtx[i, j] = delta2[i, j - 1] * (sims[i + TrailNum, j] - sims[i + TrailNum, j - 1] * erddt);
                                CV3_mtx[i, j] = (delta1[i, j - 1] - 1) * (sims[i, j] - sims[i, j - 1] * erddt);
                                CV4_mtx[i, j] = (delta2[i, j - 1] - 1) * (sims[i + TrailNum, j] - sims[i + TrailNum, j - 1] * erddt);

                            }
                        });

                        for (int i = 0; i < TrailNum; i++)
                        {
                            for (int j = 0; j <= StepNum; j++)
                            {
                                CV1_mtx[i, 0] += CV1_mtx[i, j];
                                CV2_mtx[i, 0] += CV2_mtx[i, j];
                                CV3_mtx[i, 0] += CV3_mtx[i, j];
                                CV4_mtx[i, 0] += CV4_mtx[i, j];
                            }
                           
                            CT = 0.5 * (Math.Max(0, ExtremeValue_St[i] - K) + Beta * CV1_mtx[i, 0] + Math.Max(0, ExtremeValue_St[i + TrailNum] - K) + Beta * CV2_mtx[i, 0]);
                            Sum_CT += CT;
                            Sum_CT2 += CT * CT;
                            PT = 0.5 * (Math.Max(0, K - ExtremeValue_St1[i]) + Beta * CV3_mtx[i, 0] + Math.Max(0, K - ExtremeValue_St1[i + TrailNum]) + Beta * CV4_mtx[i, 0]);
                            Sum_PT += PT;
                            Sum_PT2 += PT * PT;
                        }
                    }
                    else //without anti
                    {
                        Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism =cores}, i =>
                        {
                            //Calculate payoff at each node.
                            for (int j = 1; j <= StepNum; j++)
                            {
                                delta1[i, j - 1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(sims[i, j - 1], T - dt * (j - 1), K, sigma, r);
                                CV1_mtx[i, j] = delta1[i, j - 1] * (sims[i, j] - sims[i, j - 1] * erddt);
                                CV2_mtx[i, j] =  (delta1[i, j - 1] - 1) * (sims[i, j] - sims[i, j - 1] * erddt);
                              
                            }
                        });

                        for (int i = 0; i < TrailNum; i++)
                        {
                            for (int j = 0; j <= StepNum; j++)
                            {
                                CV1_mtx[i, 0] += CV1_mtx[i, j];
                                CV2_mtx[i, 0] += CV1_mtx[i, j];
                            }
                          
                            CT = Math.Max(0, ExtremeValue_St[i] - K) + Beta * CV1_mtx[i, 0];
                            Sum_CT += CT;
                            Sum_CT2 += CT * CT;
                            PT = Math.Max(0, K - ExtremeValue_St1[i]) + Beta * CV2_mtx[i, 0];
                            Sum_PT += PT;
                            Sum_PT2 += PT * PT;
                        }
                    }
                }

                //W/O CV.
                else
                {
                    double[] CT_mtx = new double[TrailNum];
                    double[] PT_mtx = new double[TrailNum];
                    Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism =cores}, i =>
                    {
                        if (Anti == 0) // without Antithetic 
                        {
                           
                                CT_mtx[i] = Math.Max(ExtremeValue_St[i] - K, 0);
                                PT_mtx[i] = Math.Max(K - ExtremeValue_St1[i], 0);
                        }
                        else // With antithetic.
                        {
                           
                                CT_mtx[i] = 0.5 * (Math.Max(ExtremeValue_St[i] - K, 0) + Math.Max(ExtremeValue_St[i + TrailNum] - K, 0));
                           
                                PT_mtx[i] = 0.5 * (Math.Max(K - ExtremeValue_St1[i], 0) + Math.Max(K - ExtremeValue_St1[i + TrailNum], 0));
                        }
                    });
                    for (int i = 0; i < TrailNum; i++)
                    {
                        Sum_CT += CT_mtx[i];
                        Sum_CT2 += CT_mtx[i] * CT_mtx[i];
                        Sum_PT += PT_mtx[i];
                        Sum_PT2 += PT_mtx[i] * PT_mtx[i];
                    }

                }
                };
                Thread th = new Thread(new ParameterizedThreadStart(MyAct));
                th.Start();
                th.Join();
                th.Abort();
                double[] result = new double[4];

                result[0] = Sum_CT / TrailNum * Math.Exp(-r * T);
                result[1] = Sum_PT / TrailNum * Math.Exp(-r * T);
                double SD = Math.Sqrt((Sum_CT2 - Sum_CT * Sum_CT / TrailNum) * Math.Exp(-2 * r * T) / (TrailNum - 1));
                result[2] = SD / Math.Sqrt(TrailNum);
                double SD1 = Math.Sqrt((Sum_PT2 - Sum_PT * Sum_PT / TrailNum) * Math.Exp(-2 * r * T) / (TrailNum - 1));
                result[3] = SD1 / Math.Sqrt(TrailNum);
                return result;

            }
        }

        //RangeOption
            private class RangeOptions : Option
        {
            //define a function will accept the values we need
            internal static double[] PriceRange(double SO, double K, double sigma, double r, double T, Int32 StepNum, Int32 TrailNum, double[,] Randoms, int Anti, double Dividend, int DeltaBase, int MultiThread, int Eur, double Rebate,int BarrierOption, double BarrierNumber)
            {

                // Simulator sim = new Simulator();Digital,  range,  Lookback,  barrier
                int trails = TrailNum + Anti * TrailNum;
                Option option = new Option();
                double[,] sims = option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Dividend, DeltaBase, MultiThread, Eur);
                double[,] CV1_mtx = new double[TrailNum, StepNum + 1];
                double[,] CV2_mtx = new double[TrailNum, StepNum + 1];
                double[,] CV3_mtx = new double[TrailNum, StepNum + 1];
                double[,] CV4_mtx = new double[TrailNum, StepNum + 1];
                double dt = T / Convert.ToDouble(StepNum); // Time steps
                double CT; // Temporary Payoff at each node.
                double Sum_CT = 0.0; // Sum of payoff at each node.
                double Sum_CT2 = 0.0; // Sum of squared payoff at each node.
                double PT; // Temporary Payoff at each node.
                double Sum_PT = 0.0; // Sum of payoff at each node.
                double Sum_PT2 = 0.0; // Sum of squared payoff at each node.
                double[] Range_Mtx = new double[trails];
               // double[] Range_Mtx1 = new double[trails];
                double[,] delta1 = new double[TrailNum, StepNum];
                double[,] delta2 = new double[TrailNum, StepNum];
                Action<object> MyAct = x1 => 
                { 
                if (MultiThread == 0)
                {
                    cores = 1;
                }

                //Find Range
                for (int i = 0; i < trails; i++)
                {
                    double min = SO;
                    double max = SO;
                    for (int j = 0; j <= StepNum; j++)
                    {
                        if (sims[i, j] < min)
                            min = sims[i, j];
                        if (sims[i, j] > max)
                            max = sims[i, j];
                    }
                    Range_Mtx[i] = max - min;
                }

                if (DeltaBase == 1)  // W/ CV.
                {
                    double Beta = -1.0;
                    double erddt = Math.Exp(r * dt);
                    //double CV1 = 0.0, CV2 = 0.0; // The total change in stock price after each delta hedging.

                    if (Anti == 1) //anti
                    {
                        Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, i =>
                        {
                            //Calculate payoff at each node.
                            for (int j = 1; j <= StepNum; j++)
                            {
                                delta1[i, j - 1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(sims[i, j - 1], T - dt * (j - 1), K, sigma, r);
                                delta2[i, j - 1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(sims[i + TrailNum, j - 1], T - dt * (j - 1), K, sigma, r);

                                CV1_mtx[i, j] = delta1[i, j - 1] * (sims[i, j] - sims[i, j - 1] * erddt);
                                CV2_mtx[i, j] = delta2[i, j - 1] * (sims[i + TrailNum, j] - sims[i + TrailNum, j - 1] * erddt);
                                CV3_mtx[i, j] = (delta1[i, j - 1] - 1) * (sims[i, j] - sims[i, j - 1] * erddt);
                                CV4_mtx[i, j] = (delta2[i, j - 1] - 1) * (sims[i + TrailNum, j] - sims[i + TrailNum, j - 1] * erddt);
                            }
                        });

                        for (int i = 0; i < TrailNum; i++)
                        {
                            for (int j = 0; j <= StepNum; j++)
                            {
                                CV1_mtx[i, 0] += CV1_mtx[i, j];
                                CV2_mtx[i, 0] += CV2_mtx[i, j];
                                CV3_mtx[i, 0] += CV3_mtx[i, j];
                                CV4_mtx[i, 0] += CV4_mtx[i, j];
                            }
                            CT = 0.5 * (Range_Mtx[i] + Beta * CV1_mtx[i, 0] + Range_Mtx[i + TrailNum] + Beta * CV2_mtx[i, 0]);
                            Sum_CT += CT;
                            Sum_CT2 += CT * CT;
                            PT = 0.5 * (Range_Mtx[i] + Beta * CV3_mtx[i, 0] + Range_Mtx[i + TrailNum] + Beta * CV4_mtx[i, 0]);
                            Sum_PT += PT;
                            Sum_PT2 += PT * PT;
                        }
                    }
                    else //without anti
                    {
                        Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism =cores }, i =>
                        {
                            //Calculate payoff at each node.
                            for (int j = 1; j <= StepNum; j++)
                            {
                                delta1[i, j - 1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(sims[i, j - 1], T - dt * (j - 1), K, sigma, r);
                                CV1_mtx[i, j] = delta1[i, j - 1] * (sims[i, j] - sims[i, j - 1] * erddt);
                                CV2_mtx[i, j] = (delta1[i, j - 1] - 1) * (sims[i, j] - sims[i, j - 1] * erddt);
                            }
                        });

                        for (int i = 0; i < TrailNum; i++)
                        {
                            for (int j = 0; j <= StepNum; j++)
                            {
                                CV1_mtx[i, 0] += CV1_mtx[i, j];
                                CV2_mtx[i, 0] += CV1_mtx[i, j];
                            }
                            CT = Range_Mtx[i] + Beta * CV1_mtx[i, 0];
                            Sum_CT += CT;
                            Sum_CT2 += CT * CT;
                            PT = Range_Mtx[i] + Beta * CV2_mtx[i, 0];
                            Sum_PT += PT;
                            Sum_PT2 += PT * PT;
                        }
                    }
                }

                //W/O CV.
                else
                {
                    double[] CT_mtx = new double[TrailNum];
                    double[] PT_mtx = new double[TrailNum];
                    Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, i =>
                    {
                        if (Anti== 0) // without Antithetic 
                        {
                            CT_mtx[i] = Range_Mtx[i];
                            PT_mtx[i] = Range_Mtx[i];
                        }
                        else // With antithetic.
                        {
                            CT_mtx[i] = 0.5 * (Range_Mtx[i] + Range_Mtx[i + TrailNum]);
                            PT_mtx[i] = 0.5 * (Range_Mtx[i] + Range_Mtx[i + TrailNum]);
                        }
                    });
                    for (int i = 0; i < TrailNum; i++)
                    {
                        Sum_CT += CT_mtx[i];
                        Sum_CT2 += CT_mtx[i] * CT_mtx[i];
                        Sum_PT += PT_mtx[i];
                        Sum_PT2 += PT_mtx[i] * PT_mtx[i];
                    }

                }
            };
            Thread th = new Thread(new ParameterizedThreadStart(MyAct));
            th.Start();
                th.Join();
                th.Abort();

                double[] result = new double[4];

                result[0] = Sum_CT / TrailNum * Math.Exp(-r * T);
                result[1] = Sum_PT / TrailNum * Math.Exp(-r * T);
                double SD = Math.Sqrt((Sum_CT2 - Sum_CT * Sum_CT / TrailNum) * Math.Exp(-2 * r * T) / (TrailNum - 1));
                result[2] = SD / Math.Sqrt(TrailNum);
                double SD1 = Math.Sqrt((Sum_PT2 - Sum_PT * Sum_PT / TrailNum) * Math.Exp(-2 * r * T) / (TrailNum - 1));
                result[3] = SD1 / Math.Sqrt(TrailNum);
                return result;

            }
        }


        //DigitalOption
            private class DigitalsOption : Option
        {
            //define a function will accept the values we need
            internal static double[] PriceDigital(double SO, double K, double sigma, double r, double T, Int32 StepNum, Int32 TrailNum, double[,] Randoms, int Anti, double Dividend, int DeltaBase, int MultiThread, int Eur, double Rebate,int BarrierOption, double BarrierNumber)
            {

                // Simulator sim = new Simulator();Digital,  range,  Lookback,  barrier
                int trails = TrailNum + Anti * TrailNum;
                Option option = new Option();
                double[,] sims = option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Dividend, DeltaBase, MultiThread, Eur);
                double[,] CV1_mtx = new double[TrailNum, StepNum + 1];
                double[,] CV2_mtx = new double[TrailNum, StepNum + 1];
                double[,] CV3_mtx = new double[TrailNum, StepNum + 1];
                double[,] CV4_mtx = new double[TrailNum, StepNum + 1];
                double dt = T / Convert.ToDouble(StepNum); // Time steps
                double CT; // Temporary Payoff at each node.
                double Sum_CT = 0.0; // Sum of payoff at each node.
                double Sum_CT2 = 0.0; // Sum of squared payoff at each node.
                double PT; // Temporary Payoff at each node.
                double Sum_PT = 0.0; // Sum of payoff at each node.
                double Sum_PT2 = 0.0; // Sum of squared payoff at each node.
                double[] Rebate_mtx = new double[trails];
                double[] Rebate_mtx1 = new double[trails];
                double[,] delta1 = new double[TrailNum, StepNum];
                double[,] delta2 = new double[TrailNum, StepNum];
                Action<object> MyAct = x1 =>
                {
                    if (MultiThread==0) {
                    cores = 1;
                }
                if (DeltaBase == 1)  // W/ CV.
                {
                    double Beta = -1.0;
                    double erddt = Math.Exp(r * dt);
                    //double CV1 = 0.0, CV2 = 0.0; // The total change in stock price after each delta hedging.

                    if (Anti == 1) // CV + Anti
                    {
                        Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, i =>
                        {
                            //Calculate payoff at each node.
                            for (int j = 1; j <= StepNum; j++)
                            {
                                delta1[i, j - 1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(sims[i, j - 1], T - dt * (j - 1), K, sigma, r);
                                delta2[i, j - 1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(sims[i + TrailNum, j - 1], T - dt * (j - 1), K, sigma, r);
                              
                                CV1_mtx[i, j] = delta1[i, j - 1] * (sims[i, j] - sims[i, j - 1] * erddt);
                                CV2_mtx[i, j] = delta2[i, j - 1] * (sims[i + TrailNum, j] - sims[i + TrailNum, j - 1] * erddt);
                                CV3_mtx[i, j] = (delta1[i, j - 1] - 1) * (sims[i, j] - sims[i, j - 1] * erddt);
                                CV4_mtx[i, j] = (delta2[i, j - 1] - 1) * (sims[i + TrailNum, j] - sims[i + TrailNum, j - 1] * erddt);
                             }

                            if (sims[i, StepNum] > K)
                            {
                                Rebate_mtx[i] = Rebate;
                            }
                            else
                            {
                                Rebate_mtx[i] = 0;
                            }
                            if (sims[i, StepNum] < K) {
                                Rebate_mtx1[i] = Rebate;

                            }
                            else
                            {
                                Rebate_mtx1[i] = 0;
                            }
                            if (sims[i + TrailNum, StepNum] > K)
                            {
                                Rebate_mtx[i + TrailNum] = Rebate;
                            }
                            else
                            {
                                Rebate_mtx[i + TrailNum] = 0;
                            }
                            if (sims[i + TrailNum, StepNum] <K)
                            {
                                Rebate_mtx1[i + TrailNum] = Rebate;
                            }
                            else
                            {
                                Rebate_mtx1[i + TrailNum] = 0;
                            }
                        });

                        for (int i = 0; i < TrailNum; i++)
                        {
                            for (int j = 0; j <= StepNum; j++)
                            {
                                CV1_mtx[i, 0] += CV1_mtx[i, j];
                                CV2_mtx[i, 0] += CV2_mtx[i, j];
                                CV3_mtx[i, 0] += CV3_mtx[i, j];
                                CV4_mtx[i, 0] += CV4_mtx[i, j];
                            }
                            CT = 0.5 * (Rebate_mtx[i] + Beta * CV1_mtx[i, 0] + Rebate_mtx[i + TrailNum] + Beta * CV2_mtx[i, 0]);
                            Sum_CT += CT;
                            Sum_CT2 += CT * CT;
                            PT = 0.5 * (Rebate_mtx1[i] + Beta * CV3_mtx[i, 0] + Rebate_mtx1[i + TrailNum] + Beta * CV4_mtx[i, 0]);
                            Sum_PT += PT;
                            Sum_PT2 += PT * PT;
                        }
                    }
                    else // Only CV, no anti
                    {
                        Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism =cores }, i =>
                        {
                            if (sims[i, StepNum] > K)
                            {
                                Rebate_mtx[i] = Rebate;
                            }
                            else
                            {
                                Rebate_mtx[i] = 0;
                            }
                            if (sims[i, StepNum] < K)
                            {
                                Rebate_mtx1[i] = Rebate;

                            }
                            else
                            {
                                Rebate_mtx1[i] = 0;
                            }
                            //Calculate payoff at each node.
                            for (int j = 1; j <= StepNum; j++)
                            {
                               
                                delta1[i, j - 1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(sims[i, j - 1], T - dt * (j - 1), K, sigma, r);
                                CV1_mtx[i, j] = delta1[i, j - 1] * (sims[i, j] - sims[i, j - 1] * erddt);
                                CV2_mtx[i, j] = (delta1[i, j - 1] - 1) * (sims[i, j] - sims[i, j - 1] * erddt);
                            }
                        });

                        for (int i = 0; i < TrailNum; i++)
                        {
                            for (int j = 0; j <= StepNum; j++)
                            {
                                CV1_mtx[i, 0] += CV1_mtx[i, j];
                                CV2_mtx[i, 0] += CV2_mtx[i, j];
                            }
                            CT = Rebate_mtx[i] + Beta * CV1_mtx[i, 0];
                            Sum_CT += CT;
                            Sum_CT2 += CT * CT;
                            PT = Rebate_mtx1[i] + Beta * CV2_mtx[i, 0];
                            Sum_PT += PT;
                            Sum_PT2 += PT * PT;
                        }
                    }
                }

                //NO CV.
                else
                {
                    double[] CT_mtx = new double[TrailNum];
                    double[] PT_mtx = new double[TrailNum];
                    Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, i =>
                    {

                        if (Anti == 0) // NO CV, NO ANTI
                        {
                            if (sims[i, StepNum] > K)
                            {
                                Rebate_mtx[i] = Rebate;
                            }
                            else
                            {
                                Rebate_mtx[i] = 0;
                            }
                           if( sims[i, StepNum] < K) { 

                                Rebate_mtx1[i] = Rebate;
                           }
                           else
                           {
                               Rebate_mtx1[i] = 0;
                           }
                           CT_mtx[i] = Rebate_mtx[i];
                           PT_mtx[i] = Rebate_mtx1[i];
                        }
                        else // No CV, ONLY ANTI.
                        {
                        if (sims[i, StepNum] > K)
                        {
                            Rebate_mtx[i] = Rebate;
                        }
                        else
                        {
                            Rebate_mtx[i] = 0;
                        }
                        if (sims[i, StepNum] < K)
                        {
                            Rebate_mtx1[i] = Rebate;

                        }
                        else
                        {
                            Rebate_mtx1[i] = 0;
                        }
                        if (sims[i + TrailNum, StepNum] > K)
                        {
                            Rebate_mtx[i + TrailNum] = Rebate;
                        }
                        else
                        {
                            Rebate_mtx[i + TrailNum] = 0;
                        }
                        if (sims[i + TrailNum, StepNum] < K)
                        {
                            Rebate_mtx1[i + TrailNum] = Rebate;
                        }
                        else
                        {
                            Rebate_mtx1[i + TrailNum] = 0;
                        }
                        CT_mtx[i] = 0.5 * (Rebate_mtx[i] + Rebate_mtx[i + TrailNum]);
                        PT_mtx[i] = 0.5 * (Rebate_mtx1[i] + Rebate_mtx1[i + TrailNum]);
                        }
                    });
                    for (int i = 0; i < TrailNum; i++)
                    {
                        Sum_CT += CT_mtx[i];
                        Sum_CT2 += CT_mtx[i] * CT_mtx[i];
                        Sum_PT += PT_mtx[i];
                        Sum_PT2 += PT_mtx[i] * PT_mtx[i];
                    }

                }
               };
               Thread th = new Thread(new ParameterizedThreadStart(MyAct));
                th.Start();
                th.Join();
                th.Abort();

                double[] result = new double[4];

                result[0] = Sum_CT / TrailNum * Math.Exp(-r * T);
                result[1] = Sum_PT / TrailNum * Math.Exp(-r * T);
                double SD = Math.Sqrt((Sum_CT2 - Sum_CT * Sum_CT / TrailNum) * Math.Exp(-2 * r * T) / (TrailNum - 1));
                result[2] = SD / Math.Sqrt(TrailNum);
                double SD1 = Math.Sqrt((Sum_PT2 - Sum_PT * Sum_PT / TrailNum) * Math.Exp(-2 * r * T) / (TrailNum - 1));
                result[3] = SD1 / Math.Sqrt(TrailNum);
                return result;

            }
        }

        //AsiaOption
        private class AsianOption : Option
        {
            //define a function will accept the values we need
            internal static double[] PriceAsian(double SO, double K, double sigma, double r, double T, Int32 StepNum, Int32 TrailNum, double[,] Randoms, int Anti, double Dividend, int DeltaBase, int MultiThread,int Eur, double Rebate,int BarrierOption,double BarrierNumber)
            {

                // Simulator sim = new Simulator();Digital,  range,  Lookback,  barrier
                int trails = TrailNum + Anti * TrailNum;
                Option option = new Option();
                double[,] sims = option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Dividend, DeltaBase, MultiThread, Eur);
                //MessageBox.Show(Convert.ToString(sims[0,0]));
                // MessageBox.Show(Convert.ToString(sims[trails,StepNum+1]));
                double[,] CV1_mtx = new double[TrailNum, StepNum + 1];
                double[,] CV2_mtx = new double[TrailNum, StepNum + 1];
                double[,] CV3_mtx = new double[TrailNum, StepNum + 1];
                double[,] CV4_mtx = new double[TrailNum, StepNum + 1];
                double dt = T / Convert.ToDouble(StepNum); // Time steps
                double CT,PT; // Temporary Payoff at each node.
                double Sum_CT = 0.0, Sum_PT=0.0; // Sum of payoff at each node.
                double Sum_CT2 = 0.0, Sum_PT2 = 0.0; // Sum of squared payoff at each node.
                double True_Price, True_Price1 ; // Price of European Option
                double[] Average = new double[trails];
                double[,] delta1= new double[trails,StepNum];
                double[,] delta2 = new double[trails, StepNum];
                //Calculate Average at each node
                 Action<object> MyAct = x1 => 
                { 
                for (int i = 0; i < trails; i++)
                {
                    double sum = 0.0;
                    for (int j = 1; j <= StepNum; j++)
                    {
                        sum += sims[i, j];
                    }
                    Average[i] = sum / StepNum;
                }

                if (DeltaBase == 1)  // W/ CV.
                {
                    double Beta = -1.0;
                    double erddt = Math.Exp(r * dt);
                    //double CV1 = 0.0, CV2 = 0.0; // The total change in stock price after each delta hedging.

                    if (Anti == 1) //anti
                    {
                        
                        Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, i =>
                        {
                            //Calculate payoff at each node.
                            for (int j = 1; j <= StepNum; j++)
                            {
                                //double cons_t = (j - 1) * dt; //Time till Exercise = T - cons_t
                                delta1[i,j-1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(sims[i, j - 1], T - dt * (j - 1), K, sigma, r);
                                delta2[i, j - 1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(sims[i + TrailNum, j - 1], T - dt * (j - 1), K, sigma, r);
                                CV1_mtx[i, j] = delta1[i,j-1] * (sims[i, j] - sims[i, j - 1] * erddt);
                                CV2_mtx[i, j] = delta2[i,j-1] * (sims[i + TrailNum, j] - sims[i + TrailNum, j - 1] * erddt);
                                CV3_mtx[i, j] = (delta1[i, j - 1]-1) * (sims[i, j] - sims[i, j - 1] * erddt);
                                CV4_mtx[i, j] = (delta2[i, j - 1]-1) * (sims[i + TrailNum, j] - sims[i + TrailNum, j - 1] * erddt);
                            }
                        });

                        for (int i = 0; i < TrailNum; i++)
                        {
                            for (int j = 0; j <= StepNum; j++)
                            {
                                CV1_mtx[i, 0] += CV1_mtx[i, j];
                                CV2_mtx[i, 0] += CV2_mtx[i, j];
                                CV3_mtx[i, 0] += CV3_mtx[i, j];
                                CV4_mtx[i, 0] += CV4_mtx[i, j];
                            }
                           
                            CT = 0.5 * (Math.Max(0, Average[i] - K) + Beta * CV1_mtx[i, 0] + Math.Max(0, Average[i + TrailNum] - K) + Beta * CV2_mtx[i, 0]);
                            Sum_CT += CT;
                            Sum_CT2 += CT * CT;
                            PT = 0.5 * (Math.Max(0, K - Average[i]) + Beta * CV3_mtx[i, 0] + Math.Max(0, K - Average[i + TrailNum]) + Beta * CV4_mtx[i, 0]);
                            Sum_PT += PT;
                            Sum_PT2 += PT * PT;
                        }
                    }
                    else //without anti
                    {
                        Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, i =>
                        {
                            //Calculate payoff at each node.
                            for (int j = 1; j <= StepNum; j++)
                            {
                               
                                delta1[i, j - 1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(sims[i, j - 1], T - dt * (j - 1), K, sigma, r);
                                CV1_mtx[i, j] = delta1[i, j - 1] * (sims[i, j] - sims[i, j - 1] * erddt);
                                CV2_mtx[i, j] = (delta1[i, j - 1] - 1) * (sims[i, j] - sims[i, j - 1] * erddt);
                            }
                        });

                        for (int i = 0; i < TrailNum; i++)
                        {
                            for (int j = 0; j <= StepNum; j++)
                            {
                                CV1_mtx[i, 0] += CV1_mtx[i, j];
                                CV2_mtx[i, 0] += CV2_mtx[i, j];
                            }
                           
                            CT = Math.Max(0, Average[i] - K) + Beta * CV1_mtx[i, 0];
                            Sum_CT += CT;
                            Sum_CT2 += CT * CT;
                            PT = Math.Max(0, K - Average[i]) + Beta * CV2_mtx[i, 0];
                            Sum_PT += PT;
                            Sum_PT2 += PT * PT;
                        }
                    }
                }

                //W/O CV.
                else
                {
                    double[] CT_mtx = new double[TrailNum];
                    double[] PT_mtx = new double[TrailNum];
                    Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores}, i =>
                    {
                        if (Anti == 0) // without Anti 
                        {
                            CT_mtx[i] = Math.Max(Average[i] - K, 0);
                            PT_mtx[i] = Math.Max(K - Average[i], 0);
                        }
                        else // With antithetic.
                        {
                           
                            CT_mtx[i] = 0.5 * (Math.Max(Average[i] - K, 0) + Math.Max(Average[i + TrailNum] - K, 0));
                            PT_mtx[i] = 0.5 * (Math.Max(K - Average[i], 0) + Math.Max(K - Average[i + TrailNum], 0));
                        }
                    });
                    for (int i = 0; i < TrailNum; i++)
                    {
                        Sum_CT += CT_mtx[i];
                        Sum_CT2 += CT_mtx[i] * CT_mtx[i];
                        Sum_PT += PT_mtx[i];
                        Sum_PT2 += PT_mtx[i] * PT_mtx[i];
                    }

                }
                };
                Thread th = new Thread(new ParameterizedThreadStart(MyAct));
                th.Start();
                th.Join();
                th.Abort();
                 double[] result = new double[4] ;
                
                result[0] = Sum_CT / TrailNum * Math.Exp(-r * T);
                result[1] = Sum_PT / TrailNum * Math.Exp(-r * T);
                double SD = Math.Sqrt((Sum_CT2 - Sum_CT * Sum_CT / TrailNum) * Math.Exp(-2 * r * T) / (TrailNum - 1));
                result[2] = SD / Math.Sqrt(TrailNum);
                double SD1 = Math.Sqrt((Sum_PT2 - Sum_PT * Sum_PT / TrailNum) * Math.Exp(-2 * r * T) / (TrailNum - 1));
                result[3] = SD1 / Math.Sqrt(TrailNum);
                return result;
            }
  }

        private class EuropOption : Option
        {
            //define a function will accept the values we need,int 
            internal static double[] PriceEurp(double SO, double K, double sigma, double r, double T, Int32 StepNum, Int32 TrailNum, double[,] Randoms, int Anti, double Dividend, int DeltaBase, int MultiThread,int Eur, double rebate,int  BarrierOption,double BarrierNumber)
            {//Digital, double Rebate, int range, int Lookback, int barrier
                double[] set = new double[4];
                Option option = new Option();
                double SumCall = 0.0;
                double SumPut = 0.0;
                double StdCallEr = 0.0;
                double StdCallSum = 0.0;
                double StdPutSum = 0.0;
                double StdPutEr = 0.0;
                double CallSd, PutSd;
                if (MultiThread == 0) {
                    cores = 1;
                }
                if (Anti == 1)
                {
                    double dt, CT, PT, Sum_CT, Sum_PT, Sum_PT2, Sum_CT2, t1, beta1, nudt, sigsdt, erddt, CallDelta_1, CallDelta_2;//I don't know what the beta1 is.
                    dt = T / (StepNum);
                    nudt = (r - Dividend - 0.5 * Math.Pow(sigma, 2)) * dt;
                    sigsdt = sigma * Math.Sqrt(dt);
                    erddt = Math.Exp((r - Dividend) * dt);
                    Sum_CT = 0;
                    Sum_CT2 = 0;
                    Sum_PT = 0;
                    Sum_PT2 = 0;
                    beta1 = -1;
                    
                    double[,] cv = new double[TrailNum, StepNum + 1];
                    double[,] cv1 = new double[TrailNum, StepNum + 1];
                    double[,] cv_ = new double[2*TrailNum, StepNum + 1];
                    double[,] cv_1 = new double[2*TrailNum, StepNum + 1];
                    double[,] cv2 = new double[TrailNum, StepNum + 1];
                    double[,] cv3 = new double[TrailNum, StepNum + 1];
                    double[,] CallDelta1 = new double[2*TrailNum, StepNum];
                    double[,] CallDelta2 = new double[2*TrailNum, StepNum]; 
                    if (DeltaBase == 1)
                    {
                        double[,] Simulations = new double[2 * TrailNum, StepNum + 1];
                        Simulations = option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Dividend, DeltaBase, MultiThread,Eur);
                       
                        //Calculate Average at each node

                        Action<object> MyAct1 = gtz =>
                            {
                                Parallel.ForEach(Ienum.Step(0, 2 * TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, a =>
                                  {
                                      for (int j = 1; j <= StepNum; j++)
                                      {

                                          CallDelta1[a, j - 1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(Simulations[a, j - 1], T - dt * (j - 1), K, sigma, r);
                                          cv_[a, j] = CallDelta1[a, j - 1] * (Simulations[a, j] - Simulations[a, j - 1] * erddt);
                                          cv_1[a, j] = (CallDelta1[a, j - 1] - 1) * (Simulations[a, j] - Simulations[a, j - 1] * erddt);
                                        //  CallDelta1[a+ TrailNum, j] = Black_Scholes_Delta.Black_Scholes_DeltaSet(Simulations[a+TrailNum, j - 1], t1, K, sigma, r);

                                    }

                                  });
                            };
                            Thread th1 = new Thread(new ParameterizedThreadStart(MyAct1));
                            th1.Start();
                            th1.Join();
                            th1.Abort();
                        
                            for (int b = 0; b < 2 * TrailNum; b++)
                            {
                                for (int a = 1; a <= StepNum; a++)
                                {
                                    cv_[b, 0] += cv_[b, a];
                                    cv_1[b, 0] += cv_1[b, a];
                                }
                                // CT = CT = 0.5 * (Math.Max(0, Average[b] - K) + beta1 * cv[b, 0] + Math.Max(0, Average[b + TrailNum] - K) + beta1 * cv1[b, 0]);
                                CT = Math.Max(0, Simulations[b, StepNum] - K) + beta1 * cv_[b, 0];
                                Sum_CT = Sum_CT + CT;
                                Sum_CT2 = Sum_CT2 + CT * CT;
                                PT = Math.Max(0, K - Simulations[b, StepNum]) + beta1 * cv_1[b, 0];
                                Sum_PT = Sum_PT + PT;
                                Sum_PT2 = Sum_PT2 + PT * PT;
                            }
                       
                        set[0] = Sum_CT * Math.Exp(-r * T) / (2 * TrailNum); ;
                        CallSd = Math.Sqrt((Sum_CT2 - Sum_CT * Sum_CT / (2 * TrailNum)) * Math.Exp(-2 * r * T) / (2 * TrailNum - 1));
                        set[2] = CallSd / Math.Sqrt(2 * TrailNum);
                        set[1] = Sum_PT * Math.Exp(-r * T) / (2 * TrailNum);
                        PutSd = Math.Sqrt((Sum_PT2 - Sum_PT * Sum_PT / (2 * TrailNum)) * Math.Exp(-2 * r * T) / (2 * TrailNum - 1));
                        set[3] = PutSd / Math.Sqrt(2 * TrailNum);


                    }
                    else
                    {
                        Action<object> MyAct = x1 =>
                        {
                             double[,] Simulations = new double[2 * TrailNum, StepNum];
                             Simulations = option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Dividend, DeltaBase, MultiThread,Eur);
                       
                              for (int a = 0; a < 2*TrailNum; a++)
                                {
                                    SumCall += Math.Max(Simulations[a, StepNum - 1] - K, 0);
                                    SumPut += Math.Max(K - Simulations[a, StepNum - 1], 0) ;
                                }


                                //calculate the sum of call and put
                                for (int q = 0; q < 2*TrailNum; q++)
                                {
                                    StdCallSum = StdCallSum + Math.Pow(((Math.Max(Simulations[q, StepNum - 1] - K, 0))  - (SumCall / (2 * TrailNum))), 2);
                                    StdPutSum = StdPutSum + Math.Pow(((Math.Max(K - Simulations[q, StepNum - 1], 0)) - (SumPut / (2 * TrailNum))), 2);
                                }

                                //calculate the StdError
                                
                                set[0] = (SumCall / (2 * TrailNum)) * Math.Exp(-r * T);
                                set[1] = (SumPut / (2 * TrailNum)) * Math.Exp(-r * T);
                                set[2] = Math.Sqrt(StdCallSum) * Math.Exp(-r * T) / (2*TrailNum);
                                set[3] = Math.Sqrt(StdPutSum) * Math.Exp(-r * T) / (2*TrailNum);
                            };
                            Thread th = new Thread(new ParameterizedThreadStart(MyAct));
                            th.Start();
                            th.Join();
                            th.Abort();
                            
                        }
                }
                else
                {
                    if (DeltaBase == 1)
                    {
                        double[,] Simulations = new double[TrailNum, StepNum + 1];
                        Simulations = option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Dividend, DeltaBase, MultiThread,Eur);
                        double dt, CT, PT, Sum_CT, Sum_PT, Sum_PT2, Sum_CT2, t1, beta1, nudt, sigsdt, erddt, CallDelta_1, CallDelta_2;//I don't know what the beta1 is.
                        dt = T / (StepNum);
                        nudt = (r - Dividend - 0.5 * Math.Pow(sigma, 2)) * dt;
                        sigsdt = sigma * Math.Sqrt(dt);
                        erddt = Math.Exp((r - Dividend) * dt);
                        Sum_CT = 0;
                        Sum_CT2 = 0;
                        Sum_PT = 0;
                        Sum_PT2 = 0;
                        beta1 = -1;
                        double[,] cv_ = new double[TrailNum, StepNum + 1];
                        double[,] cv_1 = new double[TrailNum, StepNum + 1];
                        
                        double[,] CallDelta1 = new double[TrailNum, StepNum];
                        double[,] CallDelta2 = new double[TrailNum, StepNum];
                        
                            Action<object> MyAct1 = gtz =>
                            {
                                Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, a =>
                                {
                                    for (int j = 1; j <= StepNum; j++)
                                    {

                                        CallDelta1[a, j - 1] = Black_Scholes_Delta.Black_Scholes_DeltaSet(Simulations[a, j - 1], T - dt * (j - 1), K, sigma, r);
                                        cv_[a, j] = CallDelta1[a, j - 1] * (Simulations[a, j] - Simulations[a, j - 1] * erddt);
                                        cv_1[a, j] = (CallDelta1[a, j - 1] - 1) * (Simulations[a, j] - Simulations[a, j - 1] * erddt);
                                        //  CallDelta1[a+ TrailNum, j] = Black_Scholes_Delta.Black_Scholes_DeltaSet(Simulations[a+TrailNum, j - 1], t1, K, sigma, r);

                                    }

                                });
                            };
                            Thread th1 = new Thread(new ParameterizedThreadStart(MyAct1));
                            th1.Start();
                            th1.Join();
                            th1.Abort();
                        
                            for (int b = 0; b < TrailNum; b++)
                            {
                                for (int a = 1; a <= StepNum; a++)
                                {
                                    cv_[b, 0] += cv_[b, a];
                                    cv_1[b, 0] += cv_1[b, a];
                                }
                                // CT = CT = 0.5 * (Math.Max(0, Average[b] - K) + beta1 * cv[b, 0] + Math.Max(0, Average[b + TrailNum] - K) + beta1 * cv1[b, 0]);
                                CT = Math.Max(0, Simulations[b, StepNum] - K) + beta1 * cv_[b, 0];
                                Sum_CT = Sum_CT + CT;
                                Sum_CT2 = Sum_CT2 + CT * CT;
                                PT = Math.Max(0, K - Simulations[b, StepNum]) + beta1 * cv_1[b, 0];
                                Sum_PT = Sum_PT + PT;
                                Sum_PT2 = Sum_PT2 + PT * PT;
                            }
                       
                        set[0] = Sum_CT * Math.Exp(-r * T) / TrailNum; ;
                        CallSd = Math.Sqrt((Sum_CT2 - Sum_CT * Sum_CT /TrailNum) * Math.Exp(-2 * r * T) / ( TrailNum - 1));
                        set[2] = CallSd / Math.Sqrt(TrailNum);
                        set[1] = Sum_PT * Math.Exp(-r * T) / TrailNum;
                        PutSd = Math.Sqrt((Sum_PT2 - Sum_PT * Sum_PT / TrailNum) * Math.Exp(-2 * r * T) / (TrailNum - 1));
                        set[3] = PutSd / Math.Sqrt(TrailNum);

                    }
                    else
                    {
                        double[,] Simulations = new double[TrailNum, StepNum];
                        Simulations = option.GetSimulations(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Dividend, DeltaBase, MultiThread,Eur);
                        
                            Action<object> MyAct = x1 =>
                            {
                                Parallel.ForEach(Ienum.Step(0, TrailNum, 1), new ParallelOptions { MaxDegreeOfParallelism = cores }, a =>
                                {
                                    Simulations[a, 0] = SO;
                                    for (int j = 1; j < StepNum; j++)
                                    {
                                        Simulations[a, j] = Simulations[a, j - 1] * Math.Exp((r - 0.5 * Math.Pow(sigma, 2)) * (T / (StepNum - 1)) + sigma * Math.Pow((T / (StepNum - 1)), 0.5) * Randoms[a, j]);

                                    }
                                });

                                for (int a = 0; a < TrailNum; a++)
                                {
                                    SumCall += Math.Max(Simulations[a, StepNum - 1] - K, 0);
                                    SumPut += Math.Max(K - Simulations[a, StepNum - 1], 0);
                                }

                                double x = (SumCall / TrailNum) * Math.Exp(-r * T);
                                double y = (SumPut / TrailNum) * Math.Exp(-r * T);
                                //calculate the sum of call and put
                                for (int a = 0; a < TrailNum; a++)
                                {
                                    StdCallSum = StdCallSum + Math.Pow((Math.Max(Simulations[a, StepNum - 1] - K, 0) - x), 2);
                                    StdPutSum = StdPutSum + Math.Pow((Math.Max(K - Simulations[a, StepNum - 1], 0) - y), 2);
                                }
                                set[0] = x;
                                set[1] = y;
                            };
                            Thread th1 = new Thread(new ParameterizedThreadStart(MyAct));
                            th1.Start();
                            th1.Join();
                            th1.Abort();
                       
                        //calculate the StdError
                        set[2] = Math.Sqrt((1.0 / (TrailNum - 1)) * StdCallSum) / Math.Sqrt(TrailNum);
                        set[3] = Math.Sqrt((1.0 / (TrailNum - 1)) * StdPutSum) / Math.Sqrt(TrailNum);

                    }
                }
               
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
                Rebate = Convert.ToDouble(this.Digitalrebate.Text);
                TrailNum = Convert.ToInt32(this.Trail.Text);
                StepNum = Convert.ToInt32(this.Step.Text);
                Anti = Convert.ToInt32(this.AntiOrNot.Text);
                Divident = Convert.ToDouble(this.Div.Text);
                DeltaBase = Convert.ToInt32(this.deltabased.Text);
                MultiThread = Convert.ToInt32(this.Multithread.Text);
              //  AsiaOption = Convert.ToInt32(this.Asianoption.Text);
                EurOption = Convert.ToInt32(this.Euroption.Text);
              //  RangeOption = Convert.ToInt32(this.Rangeoption.Text);
               // DigitalOption = Convert.ToInt32(this.Digitaloption.Text);
                //LookbackOption= Convert.ToInt32(this.Digitaloption.Text);
                BarrierOption = Convert.ToInt32(this.Barrieroption.Text);
                BarrierNumber = Convert.ToInt32(this.BarrierNum.Text);

                // int m;
                double[,] Randoms;

                if (EurOption!=1)
                {
                    if (MultiThread == 1)
                    {
                        Randoms = RandomNum.RandomSetMulti(TrailNum, StepNum, EurOption);
                    }
                    else
                    {
                        Randoms = RandomNum.RandomSetMulti1(TrailNum, StepNum, EurOption);
                    }
                }
                else {
                    if (MultiThread == 1)
                    {
                        Randoms = RandomNum.RandomSetMulti(TrailNum, StepNum, EurOption);
                    }
                    else
                    {
                        Randoms = RandomNum.RandomSetMulti1(TrailNum, StepNum, EurOption);
                    } 
}
                increase(1);
                double[] Set;
                double[] GreekValue;
                if (EurOption == 1)
                {
                    Set = EuropOption.PriceEurp(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, EurOption, Rebate, BarrierOption, BarrierNumber);

                    GreekValue = Greek.Greeks(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, EurOption,Rebate, BarrierOption, BarrierNumber);

                }
                else {
                    if (EurOption==2) {
                        Set = AsianOption.PriceAsian(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, EurOption, Rebate, BarrierOption, BarrierNumber);

                        GreekValue = Greek.Greeks(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, EurOption, Rebate, BarrierOption, BarrierNumber);
                    }
                    else
                    {
                        if (EurOption==3) {
                            Set = DigitalsOption.PriceDigital(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, EurOption, Rebate, BarrierOption, BarrierNumber);

                            GreekValue = Greek.Greeks(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, EurOption, Rebate, BarrierOption, BarrierNumber);
                        }
                        else {
                            if (EurOption == 4)
                            {
                                Set = RangeOptions.PriceRange(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, EurOption, Rebate, BarrierOption, BarrierNumber);

                                GreekValue = Greek.Greeks(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, EurOption, Rebate, BarrierOption, BarrierNumber);
                            }
                            else {
                                if (EurOption == 5) {
                                    Set = LookbackOptions.PriceLookback(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, EurOption, Rebate, BarrierOption, BarrierNumber);

                                    GreekValue = Greek.Greeks(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, EurOption, Rebate, BarrierOption, BarrierNumber);
                                }
                                else
                                { 
                                    Set = BarrierOptions.PriceBarrier(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, EurOption, Rebate,BarrierOption,BarrierNumber);

                                    GreekValue = Greek.Greeks(SO, K, sigma, r, T, StepNum, TrailNum, Randoms, Anti, Divident, DeltaBase, MultiThread, EurOption, Rebate, BarrierOption, BarrierNumber);
                                }
                            }
                        }
                    }

                }


                string OutPut;
                //output the results
                watch.Stop();
                OutPut = Convert.ToString("Callprice : " + Set[0] + "\n" + "Putprice : " + Set[1] + "\n" + "CallStdEr : " + Set[2] + "\n" + "PutStdEr : " + Set[3] + "\n" + "CallDelta : " + GreekValue[0] + "\n" + "PutDelta : " + GreekValue[1] + "\n" + "CallGamma : " + GreekValue[2] + "\n" + "PutGamma : " + GreekValue[3] + "\n" + "CallVega : " + GreekValue[4] + "\n" + "PutVega : " + GreekValue[5] + "\n" + "CallTheta : " + GreekValue[6] + "\n" + "PutTheta : " + GreekValue[7] + "\n" + "CallRho : " + GreekValue[8] + "\n" + "PutRho : " + GreekValue[9] + "\n" + "Timer : " + watch.Elapsed.TotalSeconds.ToString() + "\n" + "Cores : " + cores);
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