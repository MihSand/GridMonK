using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Diagnostics;

namespace GridMonC
{
    public partial class GridMonk : Form
    {

        string Load_PQ_circular_scan_file = "PQ.txt";
        private void Generate_Load_PQ_circular_scan_file(double amplitude_max)
        {
            string s1 = "";
            double P = 0, Q = 0;
            for (int angl_no = 0; angl_no < 25; angl_no++)
                for (int amplitude = 0; amplitude < 60; amplitude++)
                {
                    P = amplitude_max * amplitude / 50 * Math.Cos(angl_no * 15 / 180.0 * Math.PI);
                    Q = amplitude_max * amplitude / 50 * Math.Sin(angl_no * 15 / 180.0 * Math.PI);
                    s1 += P.ToString("##0.000") + " ";
                    s1 += Q.ToString("##0.000") + "\n";
                }
            Load_PQ_circular_scan_file = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + "Load_PQ_circular_scan.txt";
            File.WriteAllText(Load_PQ_circular_scan_file, s1);
        }

        string Load_PQ_constant_1500_file = "Load_1500.txt";
        private void Generate_Load_PQ_constant_file(int lines_max)
        {
            string s1 = "";
            double P = 0, Q = 0;
            for (int i1 = 0; i1 < lines_max; i1++)
                {
                    P = 1.00;
                    Q = 1.00;
                    s1 += P.ToString("##0.000") + " ";
                    s1 += Q.ToString("##0.000") + "\n";
                }

            Load_PQ_constant_1500_file = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + "Load_PQ_constant_"+lines_max.ToString()+".txt";
            File.WriteAllText(Load_PQ_constant_1500_file, s1);
        }

        double S_max_consumption = 1000000; // maximum S power 
        int textBox_U_stability_Load_no_value = 0;

        private void button_U_sability_Click(object sender, EventArgs e)
        {
            // in this routine the OpenDSS LF is requested to provide 60 x 24 LF's
            DateTime t1 = DateTime.Now;
            string st1 = ">> Grid compute to obtain node U stability limits\n";
            int t1s = t1.Second, t1ms = t1.Millisecond;
            st1 += "T(ini):" + t1.Year.ToString() + "." + t1.Month.ToString("00") + "." + t1.Day.ToString("00")
                + " " + t1.Hour.ToString("00") + ":" + t1.Minute.ToString("00") + ":" + t1.Second.ToString("00")
                 + "." + t1.Millisecond.ToString("000") + "\n";
            string richTextBox_console_answers_str = st1;

            // get max S for the node scanning; only valid value will overwrite Smax=1 GVA
            try { S_max_consumption = double.Parse(textBox_S_max_U_stability.Text); }
            catch { }

            // Get the load node
            try { textBox_U_stability_Load_no_value = int.Parse(textBox_U_stability_Load_no.Text); }
            catch { }

            // Generate limits
            Generate_Load_PQ_circular_scan_file(1.0);
            // generate constant profiles for all other loads
            Generate_Load_PQ_constant_file(1500);

            generate_output_dss("multi_LP_Scan_1440", "U_stability");  // producere fisier de iesire compatibil dss.

            OpenDSS_invoke("multi_LP_Scan_1440"); // se lanseaza OpenDSS

            //read_OpenDSS_results("multi_LP_Scan_1440"); // se citesc rezultatele din fiserele de iesire ale OpenDSS

            // see time after calculations and display the timing details of the U_Stability process
            DateTime t2;
            t2 = DateTime.Now;
            richTextBox_console_answers_str += "T(end):" + t2.Year.ToString() + "." + t2.Month.ToString("00") + "." + t2.Day.ToString("00")
                            + " " + t2.Hour.ToString("00") + ":" + t2.Minute.ToString("00") + ":" + t2.Second.ToString("00")
                            + "." + t2.Millisecond.ToString("000") + "\n";
            int dt_msec = t2.Second * 1000 + t2.Millisecond - t1s * 1000 - t1ms;
            if (dt_msec < 0) dt_msec = dt_msec + 60000;
            //richTextBox_console_answers_str = "Command not accepted";
            richTextBox_console_answers_str += "Total time (msec)= " + dt_msec.ToString();
            richTextBox_console_answers.Text = richTextBox_console_answers_str;
        }
    }

}