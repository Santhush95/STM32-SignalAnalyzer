using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Signal_Analyzer
{
    public partial class Form1 : Form
    {
        //global variables
        private String connectedPort { get; set; }
        private SerialPort port { get; set; }

        private DataTable fftTable1 { get; set; }

        public Form1()
        {
            InitializeComponent();
            string[] coms = SerialPort.GetPortNames();

            foreach (var com in coms)
            {
                comboBox1.Items.Add(com);
            }
        }

        private void btnBegin_Click(object sender, EventArgs e)
        {
            btnBegin.Enabled = false;
            btnEnd.Enabled = true;

            port = new SerialPort(connectedPort, 115200, Parity.None, 8, StopBits.One);
            port.ReadTimeout = 500;
            port.WriteTimeout = 500;
            port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            port.Open();
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            btnBegin.Enabled = true;
            btnEnd.Enabled = false;
            port.Close();
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string message = port.ReadLine();
                //string[] tokens = message.Split(new[] { "*" }, StringSplitOptions.None);
                fftTable1 = new DataTable();
                fftTable1.Clear();
                fftTable1.Columns.Add("ID");
                fftTable1.Columns.Add("Frequency");
                fftTable1.Columns.Add("FFT Amplitude");

                List<string> result = message.Split(',').ToList();
                for (int i = 0; i < result.Count; i++) {
                    float freq = i * 33.3f;
                    string dataString = i + "," + freq.ToString() + "," + result[i];
                    List<string> dataRow = dataString.Split(',').ToList();
                    fftTable1.Rows.Add(dataRow.ToArray());
                }
                ChartRefresher();
            }
            catch (Exception ex)
            {

            }

        }

        private void ChartRefresher() {
            chartFFT.Invoke(new MethodInvoker(delegate {
                chartFFT.Series.Clear();
                chartFFT.DataSource = fftTable1;
                chartFFT.ChartAreas[0].AxisX.Interval = 33;
                chartFFT.ChartAreas[0].AxisY.Interval = 100;
                chartFFT.ChartAreas[0].AxisX.LabelStyle.Angle = -90;
                chartFFT.ChartAreas[0].AxisX.IsStartedFromZero = true;
                Series series = new Series();
                series.XValueMember = fftTable1.Columns[1].ColumnName;
                series.YValueMembers = fftTable1.Columns[2].ColumnName;
                series.ChartType = SeriesChartType.Line;
                series.IsVisibleInLegend = true;
                series.IsValueShownAsLabel = false;
                series.BorderWidth = 2;
                series.LegendText = fftTable1.Columns[2].ColumnName;               
                chartFFT.Series.Add(series);
                chartFFT.DataBind();
            }));
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            connectedPort = comboBox1.SelectedItem.ToString();
            btnBegin.Enabled = true;
        }
    }
}
