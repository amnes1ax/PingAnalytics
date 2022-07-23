using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace PingAnalytics
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.BlueSeries = new ColumnSeries()
            {
                Values = new ChartValues<long>()
            };

            CartesianMapper<long> mapper = Mappers.Xy<long>()
             .X((value, index) => index)
             .Y(value => value)
             .Fill((value, index) => value == -5 ? Brushes.Black : value > 250 ? Brushes.Red : value > 100 ? Brushes.Orange : Brushes.Green);

            this.BlueSeries.Configuration = mapper;
            this.SeriesCollection = new SeriesCollection() { this.BlueSeries };

            Labels = new List<string>();
            LiveChart.DataContext = this;
        }

        private int counter = 0;
        private Ping p = new Ping();
        private PingReply r;

        private string s;
        private byte[] array = new byte[6000];
        private StreamWriter sw;
        public string TimeNow
        {
            get => System.DateTime.Now.Hour.ToString() + ":" + System.DateTime.Now.Minute.ToString() + ":" + System.DateTime.Now.Second.ToString();
        }

        public delegate void NextPrimeDelegate();
        public SeriesCollection SeriesCollection { get; set; }

        private ColumnSeries BlueSeries { get; set; }

        public List<string> Labels { get; set; }

        string tempresult = string.Empty;
        private bool continueCalculating = false;

        private void btRun_Click(object sender, RoutedEventArgs e)
        {
            s = tbIP.Text;
            if (continueCalculating)
            {
                sw.Close();
                continueCalculating = false;
                return;
            }
            sw = new StreamWriter(Environment.CurrentDirectory + $@"\\{tbIP.Text}.txt", true, Encoding.ASCII);
            continueCalculating = true;
            LiveChart.Dispatcher.BeginInvoke(DispatcherPriority.Background, new NextPrimeDelegate(Pinging));
        }
        private void Pinging()
        {
            //counter = 1;
            //while (counter % 100000 != 0)
            //{
            //    counter++;
            //}
            try
            {
                r = p.Send(s, 1000, array);
                if (r.Status == IPStatus.Success)
                {
                    SeriesCollection[0].Values.Add(r.RoundtripTime);
                    tempresult += TimeNow + ": Ping to " + s.ToString() + "[" + r.Address.ToString() + "]" + " Successful"
                       + " Response delay = " + r.RoundtripTime.ToString() + " ms" + "\n";
                }
                else
                {
                    SeriesCollection[0].Values.Add((long)1000);
                    tempresult = TimeNow + ": Connection lost: no answer\n";
                }
            }
            catch (Exception)
            {
                SeriesCollection[0].Values.Add((long)-5);
                tempresult = TimeNow + ": No connection\n";
            }

            Labels.Add(TimeNow);
            LiveChart.AxisX[0].MaxValue = SeriesCollection[0].Values.Count;
            LiveChart.AxisX[0].MinValue = LiveChart.AxisX[0].MaxValue - 30;

            if (continueCalculating)
            {
                sw.Write(tempresult);
                LiveChart.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background,
                    new NextPrimeDelegate(this.Pinging));
            }
        }

        private void btPrev_Click(object sender, RoutedEventArgs e)
        {
            LiveChart.AxisX[0].MinValue -= 15;
            LiveChart.AxisX[0].MaxValue -= 15;
        }

        private void btNext_Click(object sender, RoutedEventArgs e)
        {
            LiveChart.AxisX[0].MinValue += 15;
            LiveChart.AxisX[0].MaxValue += 15;
        }
    }
}
