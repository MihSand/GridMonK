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
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace GridMonC
{
    public partial class GridMonk : Form
    {
        // GridMonk can handle commands from a console, on the left lower side of the GUI

        private void button_console_cd_Click(object sender, EventArgs e)
        {
            // Prin apasrea acestui buton se interpreteaza comenzile date
            DateTime t1 = DateTime.Now;
            string st1 = "Timestamp:" + t1.Year.ToString() + "." + t1.Month.ToString("00") + "." + t1.Day.ToString("00")
                + "    " + t1.Hour.ToString("00") + ":" + t1.Minute.ToString("00") + ":" + t1.Second.ToString("00") + "\n";
            string richTextBox_console_answers_str = "";

            string s1 = richTextBox_console_cd.Text;
            string crt_object = ""; string crt_name = ""; string crt_atrib = ""; string crt_value = "";
            string[] item;
            string[] line_in = s1.Split('\n'); ;
            foreach (string column_in in line_in)
            {
                item = column_in.Split('=');
                if (item[0] == "object") crt_object = item[1];
                if (item[0] == "name") crt_name = item[1];
                if (item[0] == "attrib") crt_atrib = item[1];
                if (item[0] == "value") crt_value = item[1];
            }

            if (crt_object == "_GlobalVar")
            {
                try
                {
                    if (crt_name == "Global_PVs_factor") Global_PVs_factor = double.Parse(crt_value);
                    if (crt_name == "Global_loads_factor") Global_loads_factor = double.Parse(crt_value);
                    if (crt_name.ToLower() == "grid_frequency") grid_frequency = double.Parse(crt_value);
                    richTextBox_console_answers.Text = st1 + "\n";
                    richTextBox_console_answers.Text += "crt_name changed to: " + crt_value;
                }
                catch { }
            }

            if (crt_object == "load")
            {
                int load_pos = -1;
                for (int i1 = 0; i1 < loads_no; i1++) if (loads[i1, loads_PROP_name] == crt_name) { load_pos = i1; i1 = loads_no; }

                if (load_pos != -1)
                {
                    if ((crt_atrib == "brk") && (crt_value == "off"))
                    {
                        loads[load_pos, loads_PROP_bus] = loads[load_pos, loads_PROP_bus] + "@" + loads[load_pos, loads_PROP_name];
                        loads[load_pos, loads_PROP_brk] = "off";
                    }
                    if ((crt_atrib == "brk") && (crt_value == "on"))
                    {
                        loads[load_pos, loads_PROP_bus] = loads[load_pos, loads_PROP_bus].Replace("@" + loads[load_pos, loads_PROP_name], "");
                        loads[load_pos, loads_PROP_brk] = "on";
                    }
                    if (crt_atrib == "PF")
                    {
                        double PF_tmp = -10;
                        try
                        {
                            PF_tmp = double.Parse(loads[load_pos, loads_PROP_PF]);
                            richTextBox_console_answers_str = st1;
                            richTextBox_console_answers_str += "PF init.val:" + PF_tmp.ToString("0.000") + "\n";
                            PF_tmp = double.Parse(crt_value);
                            if (PF_tmp > 1.0) { PF_tmp = 1; richTextBox_console_answers_str += "Rounded up to 1\n"; }
                            if (PF_tmp < -1.0) { PF_tmp = -1; richTextBox_console_answers_str += "Rounded down to -1\n"; }
                            loads[load_pos, loads_PROP_PF] = PF_tmp.ToString("0.000");
                            richTextBox_console_answers_str += "PF changed to: " + PF_tmp.ToString("0.000") + "\n";
                            richTextBox_console_answers_str += "End of job";
                        }
                        catch { }
                    }
                    if (crt_atrib == "Pn")
                    {
                        double Pn_tmp = -10;
                        try
                        {
                            Pn_tmp = double.Parse(loads[load_pos, loads_PROP_Pn]);
                            richTextBox_console_answers_str = st1;
                            richTextBox_console_answers_str += "Pn init.val:" + Pn_tmp.ToString("0.000") + "\n";
                            Pn_tmp = double.Parse(crt_value);
                            // se limiteaza sarcina unui load la +/- 1000 kW
                            if (Pn_tmp > 1000.0) { Pn_tmp = 1000; richTextBox_console_answers_str += "Rounded up to 1000\n"; }
                            if (Pn_tmp < -1000) { Pn_tmp = -1000; richTextBox_console_answers_str += "Rounded down to -1000\n"; }
                            loads[load_pos, loads_PROP_Pn] = Pn_tmp.ToString("0.000");
                            richTextBox_console_answers_str += "Pn changed to: " + Pn_tmp.ToString("0.000") + "\n";
                            richTextBox_console_answers_str += "End of job";
                        }
                        catch { }
                    }
                    if (crt_atrib == "Qn")
                    {
                        double Qn_tmp = -10;
                        try
                        {
                            Qn_tmp = double.Parse(loads[load_pos, loads_PROP_Qn]);
                            richTextBox_console_answers_str = st1;
                            richTextBox_console_answers_str += "Qn init.val:" + Qn_tmp.ToString("0.000") + "\n";
                            Qn_tmp = double.Parse(crt_value);
                            // se limiteaza sarcina unui load la +/- 1000 kW
                            if (Qn_tmp > 500.0) { Qn_tmp = 1000; richTextBox_console_answers_str += "Rounded up to 500\n"; }
                            if (Qn_tmp < -500) { Qn_tmp = -1000; richTextBox_console_answers_str += "Rounded down to -500\n"; }
                            loads[load_pos, loads_PROP_Qn] = Qn_tmp.ToString("0.000");
                            richTextBox_console_answers_str += "Pn changed to: " + Qn_tmp.ToString("0.000") + "\n";
                            richTextBox_console_answers_str += "End of job";
                        }
                        catch { }
                    }
                }
                if (richTextBox_console_answers_str != "") richTextBox_console_answers.Text = richTextBox_console_answers_str;
            }

            if (crt_object == "trafo")
            {
                int trafo_pos = -1;
                for (int i1 = 0; i1 < trafos_no; i1++) if (trafos[i1, trafos_PROP_name] == crt_name) { trafo_pos = i1; i1 = trafos_no; }

                if (trafo_pos != -1)
                {
                    if (crt_atrib == "tap")
                    {
                        double tap_tmp = -10;
                        try
                        {
                            tap_tmp = double.Parse(trafos[trafo_pos, trafos_PROP_tap]);
                            richTextBox_console_answers_str = st1;
                            richTextBox_console_answers_str += "tap init.val:" + tap_tmp.ToString("0.000") + "\n";
                            tap_tmp = double.Parse(crt_value);
                            if (tap_tmp > 1.05) { tap_tmp = 1.05; richTextBox_console_answers_str += "Rounded up to 1\n"; }
                            if (tap_tmp < 0.85) { tap_tmp = 0.85; richTextBox_console_answers_str += "Rounded down to -1\n"; }
                            trafos[trafo_pos, trafos_PROP_tap] = tap_tmp.ToString("0.00");
                            richTextBox_console_answers_str += "Tap changed to: " + tap_tmp.ToString("0.000") + "\n";
                            richTextBox_console_answers_str += "End of job";
                        }
                        catch { }
                    }
                    if (crt_atrib == "brk1") // breaker de trafo
                    { // de extras bus-urile din trafos_PROP_busses si pus in trafos_PROP_bus1 si trafos_PROP_bus2
                        try
                        {
                            if (crt_value == "off")
                            {
                                trafos[trafo_pos, trafos_PROP_bus1] = trafos[trafo_pos, trafos_PROP_bus1] + "@" + trafos[trafo_pos, trafos_PROP_name];
                                trafos[trafo_pos, trafos_PROP_brk1] = "off";
                            }
                            if (crt_value == "on")
                            {
                                trafos[trafo_pos, trafos_PROP_bus1] = trafos[trafo_pos, trafos_PROP_bus1].Replace("@" + trafos[trafo_pos, trafos_PROP_name], "");
                                trafos[trafo_pos, lines_PROP_brk1] = "on";
                            }
                            trafos[trafo_pos, trafos_PROP_busses] = "(" + trafos[trafo_pos, trafos_PROP_bus1] + "," + trafos[trafo_pos, trafos_PROP_bus2] + ")";
                        }
                        catch { }
                    }
                }
                if (richTextBox_console_answers_str != "") richTextBox_console_answers.Text = richTextBox_console_answers_str;
            }

            if (crt_object == "generator")
            {
                int generator_pos = -1;
                for (int i1 = 0; i1 < generators_no; i1++) if (generators[i1, generators_PROP_name] == crt_name) { generator_pos = i1; i1 = generators_no; }

                if (generator_pos != -1)
                {
                    if (crt_atrib == "Pn")
                    {
                        double Pn_tmp = -10;
                        try
                        {
                            Pn_tmp = double.Parse(generators[generator_pos, generators_PROP_Pn]);
                            richTextBox_console_answers_str = st1;
                            richTextBox_console_answers_str += "Pn init.val:" + Pn_tmp.ToString("0.000") + "\n";
                            Pn_tmp = double.Parse(crt_value);
                            // se limiteaza sarcina unui load la +/- 1000 kW
                            if (Pn_tmp > 1000.0) { Pn_tmp = 1000; richTextBox_console_answers_str += "Rounded up to 1000\n"; }
                            if (Pn_tmp < 0) { Pn_tmp = 0; richTextBox_console_answers_str += "Rounded down to 0\n"; }
                            generators[generator_pos, generators_PROP_Pn] = Pn_tmp.ToString("0.000");
                            richTextBox_console_answers_str += "Pn changed to: " + Pn_tmp.ToString("0.000") + "\n";
                            richTextBox_console_answers_str += "End of job";
                        }
                        catch { richTextBox_console_answers_str = "Error in processing command Pn"; }
                    }
                    if (crt_atrib == "PF")
                    {
                        double PF_tmp = -10;
                        try
                        {
                            try { 
                            PF_tmp = double.Parse(generators[generator_pos, generators_PROP_PF]);
                            richTextBox_console_answers_str = st1;
                            richTextBox_console_answers_str += "PF init.val:" + PF_tmp.ToString("0.000") + "\n";
                            } catch { }
                            PF_tmp = double.Parse(crt_value);
                            // se limiteaza PF
                            if (PF_tmp > 1.0) { PF_tmp = 1.000; richTextBox_console_answers_str += "Rounded to uper limit = 1.0\n"; }
                            if (PF_tmp < -1.0) { PF_tmp = -1.000; richTextBox_console_answers_str += "Rounded to lower limit = -1.0\n"; }
                            generators[generator_pos, generators_PROP_PF] = PF_tmp.ToString("0.000");
                            generators[generator_pos, generators_PROP_Qn] = "";
                            richTextBox_console_answers_str += "PF changed to: " + PF_tmp.ToString("0.000") + "\n";
                            richTextBox_console_answers_str += "Qn deleted\n";
                            richTextBox_console_answers_str += "End of job";
                        }
                        catch { richTextBox_console_answers_str = "Error in processing command PF"; }
                    }
                    if (crt_atrib == "Qn")
                    {
                        double Qn_tmp = 0;
                        try
                        {
                            try
                            {
                                Qn_tmp = double.Parse(generators[generator_pos, generators_PROP_Qn]);
                                richTextBox_console_answers_str = st1;
                                richTextBox_console_answers_str += "Qn init.val:" + Qn_tmp.ToString("0.000") + "\n";
                            }
                            catch { };
                            Qn_tmp = double.Parse(crt_value);
                            // se limiteaza Qn
                            if (Qn_tmp > 1000.0) { Qn_tmp = 1000; richTextBox_console_answers_str += "Rounded to uper limit = 1000\n"; }
                            if (Qn_tmp < -1000.0) { Qn_tmp = -1000; richTextBox_console_answers_str += "Rounded to lower limit = -1000\n"; }
                            generators[generator_pos, generators_PROP_Qn] = Qn_tmp.ToString("0.000");
                            generators[generator_pos, generators_PROP_PF] = "";
                            richTextBox_console_answers_str += "Qn changed to: " + Qn_tmp.ToString("0.000") + "\n";
                            richTextBox_console_answers_str += "PF deleted\n";
                            richTextBox_console_answers_str += "End of job";
                        }
                        catch { richTextBox_console_answers_str = "Error in processing command Qn"; }
                    }
                }
                if (richTextBox_console_answers_str != "") richTextBox_console_answers.Text = richTextBox_console_answers_str;
            }

            if (crt_object == "node")
            {
                int node_pos = -1;
                for (int i1 = 0; i1 < nodes_no; i1++) if (nodes[i1, nodes_PROP_bus] == crt_name) { node_pos = i1; i1 = nodes_no; }

                if (node_pos != -1)
                {
                    if (crt_atrib == "Polylines")
                    {
                        double Pn_tmp = -10;
                        try
                        {
                            richTextBox_console_answers_str = nodes[node_pos, nodes_PROP_plylines];
                        }
                        catch { richTextBox_console_answers_str = "Error in processing command"; }
                    }
                    if (crt_atrib == "select")
                    {
                        nodes[node_pos, nodes_PROP_gph_selected] = "1";
                    }
                    if (crt_atrib == "deselect")
                    {
                        nodes[node_pos, nodes_PROP_gph_selected] = "0";
                    }
                }
                if (richTextBox_console_answers_str != "") richTextBox_console_answers.Text = richTextBox_console_answers_str;
            }

            if (crt_object == "line")
            {
                int line_pos = -1;
                for (int i1 = 0; i1 < lines_no; i1++) if (lines[i1, lines_PROP_name] == crt_name) { line_pos = i1; i1 = lines_no; }
                // value=JT185mm
                if (line_pos != -1)
                {
                    if ((crt_atrib == "Cod")) lines[line_pos, lines_PROP_linecode] = crt_value;
                    if ((crt_atrib == "brk1") && (crt_value == "off"))
                    {
                        lines[line_pos, lines_PROP_bus1] = lines[line_pos, lines_PROP_bus1] + "@" + lines[line_pos, lines_PROP_name];
                        lines[line_pos, lines_PROP_brk1] = "off";
                    }
                    if ((crt_atrib == "brk1") && (crt_value == "on"))
                    {
                        lines[line_pos, lines_PROP_bus1] = lines[line_pos, lines_PROP_bus1].Replace("@" + lines[line_pos, lines_PROP_name], "");
                        lines[line_pos, lines_PROP_brk1] = "on";
                    }
                    if ((crt_atrib == "brk2") && (crt_value == "off"))
                    {
                        lines[line_pos, lines_PROP_bus2] = lines[line_pos, lines_PROP_bus2] + "@" + lines[line_pos, lines_PROP_name];
                        lines[line_pos, lines_PROP_brk2] = "off";
                    }
                    if ((crt_atrib == "brk2") && (crt_value == "on"))
                    {
                        lines[line_pos, lines_PROP_bus2] = lines[line_pos, lines_PROP_bus2].Replace("@" + lines[line_pos, lines_PROP_name], "");
                        lines[line_pos, lines_PROP_brk2] = "on";
                    }
                    if (crt_atrib.ToLower() == "length")
                    {
                        double x1r;
                        try { 
                            x1r = double.Parse(crt_value);
                            if (Math.Abs(x1r) > 1000)
                            {
                                if(x1r>=0) x1r = 1000; else x1r = -1000;
                                richTextBox_console_answers.Text = "length limited to: " + x1r.ToString("##0.0000");
                            }
                            lines[line_pos, lines_PROP_length] = x1r.ToString("##0.0000");
                            richTextBox_console_answers.Text += "length changed to: " + x1r.ToString("##0.0000");
                        } catch
                        {
                            richTextBox_console_answers.Text = "Error for this length input";
                        }
                    }
                }
            }
            //int idx = polylines.FindIndex(x => x.name == "N4-PYL2");
            //polylinesPaintAttrib[idx].penStyle = new Pen(Color.Red);
            if (crt_object == "polyline1")
            {
                Poly crt = polylines.Find(x => x.name == crt_name);
                if (crt != null)
                {
                    Pen np = new Pen(Color.Red, 3);
                    //np.EndCap = LineCap.ArrowAnchor;
                    //np.CustomEndCap = new CustomLineCap()
                    crt.penStyle = np;
                }
            }
        }

        private void button_console_cd_and_calc_Click(object sender, EventArgs e)
        {
            // Execuat comanda si apleaza OpenDSS pentru recalculare regim retea 
            // de implementat
            button_console_cd_Click(sender, e);
            button_Compute_Click(sender, e);
        }


    }
}