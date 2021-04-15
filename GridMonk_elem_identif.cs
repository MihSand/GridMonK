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
using System.IO;
using System.Diagnostics;

namespace GridMonC
{
    public partial class GridMonk : Form
    {
        string Gph_element_identified = "";

        private void Gph_element_identification_trafos(int xm, int ym, int draw_param)
        {
            int x1, y1, inside;

            for (int i1 = 0; i1 < trafos_no; i1++) // scanare obiecte de tip "loads"
            {
                if ((trafos[i1, trafos_PROP_x0] != "") && (trafos[i1, trafos_PROP_y0] != ""))
                // daca coordonatele transformatorului au fost initializate
                {
                    x1 = int.Parse(trafos[i1, trafos_PROP_x0]) - X0_shift;
                    y1 = int.Parse(trafos[i1, trafos_PROP_y0]) - Y0_shift;
                    inside = Inside_rect(x1, y1, x1 + object_dx, y1 + line_dy, xm, ym);
                    if (inside == 1) // afiseaza in fereastra Console_2
                    {
                        trafos[i1, trafos_PROP_gph_selected] = "1";
                        richTextBox_console2.Text = "trafo=#" + i1.ToString() + "\nname=" + trafos[i1, trafos_PROP_name]
                            + "\nwindings=" + trafos[i1, trafos_PROP_windings]
                            + "\nBuses=" + trafos[i1, trafos_PROP_busses] + "\nConns=" + trafos[i1, trafos_PROP_conns]
                            + "\nkVs=" + trafos[i1, trafos_PROP_kVs] + "\nkVAs=" + trafos[i1, trafos_PROP_kVAs]
                            + "\n%noloadloss=" + trafos[i1, trafos_PROP_noloadloss] + "\n%loadloss=" + trafos[i1, trafos_PROP_loadloss]
                            + "\n%imag=" + trafos[i1, trafos_PROP_imag] + "\nxhl=" + trafos[i1, trafos_PROP_xhl]
                            + "\nwdg=" + trafos[i1, trafos_PROP_wdg] + "\ntap=" + trafos[i1, trafos_PROP_tap]
                            + "\nmaxtap=" + trafos[i1, trafos_PROP_maxtap] + "\nmintap=" + trafos[i1, trafos_PROP_mintap]
                            + "\nP1=" + trafos[i1, trafos_PROP_P1] + "\nP2=" + trafos[i1, trafos_PROP_P2] + "\nP3=" + trafos[i1, trafos_PROP_P3]
                            + "\nQ1=" + trafos[i1, trafos_PROP_Q1] + "\nQ2=" + trafos[i1, trafos_PROP_Q2] + "\nQ3=" + trafos[i1, trafos_PROP_Q3]
                            ;
                        double P3ph = 0;
                        if ((trafos[i1, trafos_PROP_P1] != "") && (trafos[i1, trafos_PROP_P2] != "") && (trafos[i1, trafos_PROP_P3] != ""))
                        {
                            P3ph = double.Parse(trafos[i1, trafos_PROP_P1]) + double.Parse(trafos[i1, trafos_PROP_P2]) + double.Parse(trafos[i1, trafos_PROP_P3]);
                            richTextBox_console2.Text += "\nP=" + P3ph.ToString("####0.0");
                        }
                        double Q3ph = 0;
                        if ((trafos[i1, trafos_PROP_Q1] != "") && (trafos[i1, trafos_PROP_Q2] != "") && (trafos[i1, trafos_PROP_Q3] != ""))
                        {
                            Q3ph = double.Parse(trafos[i1, trafos_PROP_Q1]) + double.Parse(trafos[i1, trafos_PROP_Q2]) + double.Parse(trafos[i1, trafos_PROP_Q3]);
                            richTextBox_console2.Text += "\nQ=" + Q3ph.ToString("####0.0");
                        }
                        richTextBox_console2.Text += "\nbus1=" + trafos[i1, trafos_PROP_bus1] + "\nbus2=" + trafos[i1, trafos_PROP_bus2];
                        richTextBox_console2.Text += "\nbrk1=" + trafos[i1, trafos_PROP_brk1] + "\nbrk2=" + trafos[i1, trafos_PROP_brk2];

                        richTextBox_console_cd.Text = "object=trafo\n" + "name=" + trafos[i1, trafos_PROP_name] + "\n"
                            + "attrib=" + "\n" + "value=";
                        Gph_element_identified = "trafo";
                    }
                    else trafos[i1, trafos_PROP_gph_selected] = "0";
                }
            }

        }

        private void Gph_element_identification_loads(int xm, int ym, int draw_param)
        {
            int x1, dx, y1, dy, inside;
            string s1 = "";

            for (int i1 = 0; i1 < loads_no; i1++) // scanare obiecte de tip "loads"
            {
                if ((loads[i1, loads_PROP_x0] != "") && (loads[i1, loads_PROP_y0] != ""))
                // daca coordonatele au fost initializate
                {
                    x1 = int.Parse(loads[i1, loads_PROP_x0]) - X0_shift;
                    y1 = int.Parse(loads[i1, loads_PROP_y0]) - Y0_shift;
                    if(loads[i1, loads_PROP_sim_type] == "EV") {
                        dx = object_dx_EVtot; dy = object_dysmall_EV;
                    }
                    else {
                        dx = object_dx; dy = line_dy;
                    }
                    inside = Inside_rect(x1, y1, x1 + dx, y1 + dy, xm, ym);
                    if (inside == 1)
                    {
                        loads[i1, loads_PROP_gph_selected] = "1";
                        s1 = "load=#" + i1.ToString() + "\nname=" + loads[i1, loads_PROP_name]
                            + "\nBus=" + loads[i1, loads_PROP_bus];

                        // Afisare P1, Q1 din rezulatele OpenDSS
                        if (loads[i1, loads_PROP_P] != "") s1 += "\nP=" + loads[i1, loads_PROP_P] + " kW";
                        if (loads[i1, loads_PROP_Q] != "") s1 += "\nQ=" + loads[i1, loads_PROP_Q] + " kvar";

                        s1 += "\nPn=" + loads[i1, loads_PROP_Pn]
                            + "\nQn=" + loads[i1, loads_PROP_Qn];
                        s1 += "\nPF=" + loads[i1, loads_PROP_PF]
                            + "\nmodel=" + loads[i1, loads_PROP_model] + "\nstatus=" + loads[i1, loads_PROP_status]
                            + "\ndaily=" + loads[i1, loads_PROP_daily] + "\nconn=" + loads[i1, loads_PROP_conn]
                            + "\nVminpu=" + loads[i1, loads_PROP_Vminpu] + "\nVmaxpu=" + loads[i1, loads_PROP_Vmaxpu]

                            + "\nSim_type=" + loads[i1, loads_PROP_sim_type];
                        if (loads[i1, loads_PROP_sim_type].ToLower() == "storage")
                        {
                            s1 += "\nsim_storage=" + loads[i1, loads_PROP_sim_storage];
                            s1 += "\nsim_storage_attr=" + loads[i1, loads_PROP_sim_storage_attr];
                        }
                        if (loads[i1, loads_PROP_sim_type].ToLower() == "ev")
                        {
                            s1 += "\nsim_type_attr=" + loads[i1, loads_PROP_sim_type_attr];
                        }

                        s1 += "\nU1=" + loads[i1, loads_PROP_U1] + "\nU2=" + loads[i1, loads_PROP_U2] + "\nU3=" + loads[i1, loads_PROP_U3]
                            + "\nU1fi=" + loads[i1, loads_PROP_U1fi] + "\nU2fi=" + loads[i1, loads_PROP_U2fi] + "\nU3fi=" + loads[i1, loads_PROP_U3fi]
                            + "\nI1=" + loads[i1, loads_PROP_I1] + "\nI2=" + loads[i1, loads_PROP_I2] + "\nI3=" + loads[i1, loads_PROP_I3]
                            + "\nI1fi=" + loads[i1, loads_PROP_I1fi] + "\nI2fi=" + loads[i1, loads_PROP_I2fi] + "\nI3fi=" + loads[i1, loads_PROP_I3fi]
                            + "\nU4=" + loads[i1, loads_PROP_U4] + "\nU4fi=" + loads[i1, loads_PROP_U4fi]
                            + "\nI4=" + loads[i1, loads_PROP_I4] + "\nI4fi=" + loads[i1, loads_PROP_I4fi];

                        // Afisare P1, Q1 din rezulatele OpenDSS
                        if (loads[i1, loads_PROP_P1] != "") s1 += "\nP1=" + loads[i1, loads_PROP_P1] + " kW";
                        if (loads[i1, loads_PROP_Q1] != "") s1 += "\nQ1=" + loads[i1, loads_PROP_Q1] + " kvar";

                        if (loads[i1, loads_PROP_P2] != "") s1 += "\nP2=" + loads[i1, loads_PROP_P2] +" kW";
                        if (loads[i1, loads_PROP_Q2] != "") s1 += "\nQ2=" + loads[i1, loads_PROP_Q2] + " kvar";

                        if (loads[i1, loads_PROP_P3] != "") s1 += "\nP3=" + loads[i1, loads_PROP_P3] + " kW";
                        if (loads[i1, loads_PROP_Q3] != "") s1 += "\nQ3=" + loads[i1, loads_PROP_Q3] + " kvar";

                        s1 +=
                            //"\nP1=" + loads[i1, loads_PROP_P1] + "\nP2=" + loads[i1, loads_PROP_P2] + "\nP3=" + loads[i1, loads_PROP_P3]
                            //+ "\nQ1=" + loads[i1, loads_PROP_Q1] + "\nQ2=" + loads[i1, loads_PROP_Q2] + "\nQ3=" + loads[i1, loads_PROP_Q3] 
                            "\nx0=" + loads[i1, loads_PROP_x0] + "\ny0=" + loads[i1, loads_PROP_y0]
                            + "\nP=" + loads[i1, loads_PROP_P] + "\nQ=" + loads[i1, loads_PROP_Q]
                            + "\nbrk=" + loads[i1, loads_PROP_brk]
                            ;

                        richTextBox_console2.Text = s1;

                        richTextBox_console_cd.Text = "object=load\n" + "name=" + loads[i1, loads_PROP_name] + "\n"
                            + "attrib=" + "\n" + "value=";

                        for (int h1 = LPs_scenarios_multi_LF_start + 2; h1 < LPs_scenarios_multi_LF_start + 24 + 2; h1++)
                        {
                            // salvare in canalul grafic 1
                            if (loads_values_set[i1, loads_PROP_P, h1] != "") {
                                if (graph_smallgph[0, graph_smallgph_PROP_P_type] == "Q")
                                    SimpleGph_channels1[channel_free1, h1 - LPs_scenarios_multi_LF_start - 2] = double.Parse(loads_values_set[i1, loads_PROP_Q, h1]);
                                else
                                    SimpleGph_channels1[channel_free1, h1 - LPs_scenarios_multi_LF_start - 2] = double.Parse(loads_values_set[i1, loads_PROP_P, h1]);
                            }
                            // salvare in canalul grafic 2
                            if (loads_values_set[i1, loads_PROP_U1, h1] != "")
                                SimpleGph_channels2[channel_free2, h1 - LPs_scenarios_multi_LF_start - 2] = double.Parse(loads_values_set[i1, loads_PROP_U1, h1]);
                        }
                        channel_name[channel_free1] = loads[i1, loads_PROP_name];
                        channel_free1++; if (channel_free1 >= SimpleGph_channels_MAX) channel_free1 = 0;
                        channel_free2++; if (channel_free2 >= SimpleGph_channels_MAX) channel_free2 = 0;

                        Gph_element_identified = "load";

                    }
                    else loads[i1, loads_PROP_gph_selected] = "0";
                }
            }
        }

        private void Gph_element_identification_line_attributes(int crt_line)
        {
            double dbl_tmp = 0;

            lines[crt_line, lines_PROP_gph_selected] = "1";

            richTextBox_console2.Text += "line=#" + crt_line.ToString() + "\tname=" + lines[crt_line, lines_PROP_name]
                + "\nBus1=" + lines[crt_line, lines_PROP_bus1] + "\tBus2=" + lines[crt_line, lines_PROP_bus2];
            richTextBox_console2.Text += "\n--------------------------------------------\n";

            richTextBox_console2.Text += "P=" + lines[crt_line, lines_PROP_P];
            if (lines[crt_line, lines_PROP_P1] != "")
            {
                dbl_tmp = double.Parse(lines[crt_line, lines_PROP_P1]);
                richTextBox_console2.Text += " [" + dbl_tmp.ToString("###.0");
            }
            else richTextBox_console2.Text += " [";
            if (lines[crt_line, lines_PROP_P2] != "")
            {
                dbl_tmp = double.Parse(lines[crt_line, lines_PROP_P2]);
                richTextBox_console2.Text += ", " + dbl_tmp.ToString("###.0");
            }
            else richTextBox_console2.Text += ", ";
            if (lines[crt_line, lines_PROP_P3] != "")
            {
                dbl_tmp = double.Parse(lines[crt_line, lines_PROP_P3]);
                richTextBox_console2.Text += ", " + dbl_tmp.ToString("###.0") + "]";
            }
            else richTextBox_console2.Text += ", ";
            richTextBox_console2.Text += "\nP_t2=" + lines[crt_line, lines_PROP_P_t2];
            if (lines[crt_line, lines_PROP_P1_t2] != "")
            {
                dbl_tmp = double.Parse(lines[crt_line, lines_PROP_P1_t2]);
                richTextBox_console2.Text += " [" + dbl_tmp.ToString("###.0");
            }
            else richTextBox_console2.Text += " [";
            if (lines[crt_line, lines_PROP_P2_t2] != "")
            {
                dbl_tmp = double.Parse(lines[crt_line, lines_PROP_P2_t2]);
                richTextBox_console2.Text += ", " + dbl_tmp.ToString("###.0");
            }
            else richTextBox_console2.Text += ", ";
            if (lines[crt_line, lines_PROP_P3_t2] != "")
            {
                dbl_tmp = double.Parse(lines[crt_line, lines_PROP_P3_t2]);
                richTextBox_console2.Text += ", " + dbl_tmp.ToString("###.0") + "]";
            }
            else richTextBox_console2.Text += ", ";

        }

        private void Gph_element_identification_lines(int xm, int ym, int draw_param)
        {
            int x1, y1, inside;

            for (int i1 = 0; i1 < lines_no; i1++) // scanare obiecte de tip "lines"
            {
                double dbl_tmp = 0;
                int selected_line = -1;
                if ((lines[i1, lines_PROP_x0] != "") && (lines[i1, lines_PROP_x0] != ""))
                // daca coordonatele au fost initializate pentru pozitai obiectelor de tip "lines"
                {
                    x1 = int.Parse(lines[i1, lines_PROP_x0])- X0_shift;
                    y1 = int.Parse(lines[i1, lines_PROP_y0]) - Y0_shift;
                    inside = Inside_rect(x1, y1, x1 + object_dx, y1 + line_dy, xm, ym);
                    if (inside == 1)
                    { // linia curenta este obiectul selectat
                      // se afiseaza in console 2 atributele obiectlui selectat
                        selected_line = i1;
                        if (draw_param == 1)
                        {

                            richTextBox_console2.Text = "";

                            // se cheama rutina care scrie atributele obiectului in "richTextBox_console2"
                            Gph_element_identification_line_attributes(i1); // se vor muta toate afisarile de atribute de mai jso in ac. functie


                            richTextBox_console2.Text += "\nQ=" + lines[i1, lines_PROP_Q];
                            if (lines[i1, lines_PROP_Q1] != "")
                            {
                                 dbl_tmp = double.Parse(lines[i1, lines_PROP_Q1]);
                                richTextBox_console2.Text += " [" + dbl_tmp.ToString("###.0");
                            }
                            else richTextBox_console2.Text += " [";
                            if (lines[i1, lines_PROP_Q2] != "")
                            {
                                dbl_tmp = double.Parse(lines[i1, lines_PROP_Q2]);
                                richTextBox_console2.Text += ", " + dbl_tmp.ToString("###.0");
                            }
                            else richTextBox_console2.Text += ", ";
                            if (lines[i1, lines_PROP_Q3] != "")
                            {
                                dbl_tmp = double.Parse(lines[i1, lines_PROP_Q3]);
                                richTextBox_console2.Text += ", " + dbl_tmp.ToString("###.0") + "]";
                            }
                            else richTextBox_console2.Text += ", ";
                            richTextBox_console2.Text += "\nQ_t2=" + lines[i1, lines_PROP_Q_t2];
                            if (lines[i1, lines_PROP_Q1_t2] != "")
                            {
                                dbl_tmp = double.Parse(lines[i1, lines_PROP_Q1_t2]);
                                richTextBox_console2.Text += " [" + dbl_tmp.ToString("###.0");
                            }
                            else richTextBox_console2.Text += " [";
                            if (lines[i1, lines_PROP_Q2_t2] != "")
                            {
                                dbl_tmp = double.Parse(lines[i1, lines_PROP_Q2]);
                                richTextBox_console2.Text += ", " + dbl_tmp.ToString("###.0");
                            }
                            else richTextBox_console2.Text += ", ";
                            if (lines[i1, lines_PROP_Q3_t2] != "")
                            {
                                dbl_tmp = double.Parse(lines[i1, lines_PROP_Q3]);
                                richTextBox_console2.Text += ", " + dbl_tmp.ToString("###.0") + "]";
                            }
                            else richTextBox_console2.Text += ", ";

                            richTextBox_console2.Text += "\n--------------------------------------------";
                            double val1 = 0, val1fi = 0;
                            if ((lines[i1, lines_PROP_U1] != "") && (lines[i1, lines_PROP_U1fi] != ""))
                            {
                                val1 = double.Parse(lines[i1, lines_PROP_U1]);
                                val1fi = double.Parse(lines[i1, lines_PROP_U1fi]);
                                richTextBox_console2.Text += "\nU1=" + val1.ToString("##0.000") + "\tfi=" + val1fi.ToString("##0.000");
                            }
                            if ((lines[i1, lines_PROP_U2] != "") && (lines[i1, lines_PROP_U2fi] != ""))
                            {
                                val1 = double.Parse(lines[i1, lines_PROP_U2]);
                                val1fi = double.Parse(lines[i1, lines_PROP_U2fi]);
                                richTextBox_console2.Text += "\nU2=" + val1.ToString("##0.000") + "\tfi=" + val1fi.ToString("##0.000");
                            }
                            if ((lines[i1, lines_PROP_U3] != "") && (lines[i1, lines_PROP_U3fi] != ""))
                            {
                                val1 = double.Parse(lines[i1, lines_PROP_U3]);
                                val1fi = double.Parse(lines[i1, lines_PROP_U3fi]);
                                richTextBox_console2.Text += "\nU3=" + val1.ToString("##0.000") + "\tfi=" + val1fi.ToString("##0.000");
                            }
                            richTextBox_console2.Text += "\n--------------------------------------------";
                            if ((lines[i1, lines_PROP_I1] != "") && (lines[i1, lines_PROP_I1fi] != ""))
                            {
                                val1 = double.Parse(lines[i1, lines_PROP_I1]);
                                val1fi = double.Parse(lines[i1, lines_PROP_I1fi]);
                                richTextBox_console2.Text += "\nI1=" + val1.ToString("##0.000") + "\tfi=" + val1fi.ToString("##0.000");
                            }
                            if ((lines[i1, lines_PROP_I2] != "") && (lines[i1, lines_PROP_I2fi] != ""))
                            {
                                val1 = double.Parse(lines[i1, lines_PROP_I2]);
                                val1fi = double.Parse(lines[i1, lines_PROP_I2fi]);
                                richTextBox_console2.Text += "\nI2=" + val1.ToString("##0.000") + "\tfi=" + val1fi.ToString("##0.000");
                            }
                            if ((lines[i1, lines_PROP_I3] != "") && (lines[i1, lines_PROP_I3fi] != ""))
                            {
                                val1 = double.Parse(lines[i1, lines_PROP_I3]);
                                val1fi = double.Parse(lines[i1, lines_PROP_I3fi]);
                                richTextBox_console2.Text += "\nI3=" + val1.ToString("##0.000") + "\tfi=" + val1fi.ToString("##0.000");
                            }
                            if ((lines[i1, lines_PROP_delta_U1] != "") && (lines[i1, lines_PROP_delta_U1fi] != ""))
                            {
                                val1 = double.Parse(lines[i1, lines_PROP_delta_U1]);
                                val1fi = double.Parse(lines[i1, lines_PROP_delta_U1fi]);
                                richTextBox_console2.Text += "\nΔU1= " + val1.ToString("##0.000") + "\tΔU1fi=" + val1fi.ToString("##0.000");
                            }
                            if (lines[i1, lines_PROP_X1] != "")
                            {
                                val1 = double.Parse(lines[i1, lines_PROP_X1]);
                                val1fi = double.Parse(lines[i1, lines_PROP_R1]);
                                richTextBox_console2.Text += "\nX1= " + val1.ToString("##0.000") + "\tR1=" + val1fi.ToString("##0.000");
                            }
                            if (lines[i1, lines_PROP_length] != "")
                            {
                            if (lines[i1, lines_PROP_X1] != "")
                            {
                                val1 = double.Parse(lines[i1, lines_PROP_X1]);
                                richTextBox_console2.Text += "\nlength= " + val1.ToString("##0.000") + " km";
                            }
                        }

                        richTextBox_console2.Text += "\nbrk1=" + lines[i1, lines_PROP_brk1]
                            + "\nbrk2=" + lines[i1, lines_PROP_brk2];
                        richTextBox_console2.Text += "\nImax=" + lines[i1, lines_PROP_Imax] + " A";
                        richTextBox_console2.Text += "\nUn_line= " + lines[i1, lines_PROP_voltage] + " V";
                            
                        richTextBox_console2.Text += "\nx0=" + lines[i1, lines_PROP_x0] + "\ty0=" + lines[i1, lines_PROP_y0];
                        richTextBox_console2.Text += "\npin1x=" + lines[i1, lines_PROP_pin1_x] + "\tpin1y=" + lines[i1, lines_PROP_pin1_y];
                        richTextBox_console2.Text += "\npin2x=" + lines[i1, lines_PROP_pin2_x] + "\tpin2y=" + lines[i1, lines_PROP_pin2_y];
                        richTextBox_console2.Text += "\nOutUI=" + lines[i1, lines_PROP_OutUI] + "";
                        richTextBox_console2.Text += "\n-------Recorded data----";
                        for (int h1 = 0; h1 < LPs_scenarios_multi_LF_start + lines_values_set_no; h1++)
                        {
                            richTextBox_console2.Text += "\n" + "P,Q[" + h1.ToString() + "]=\t";
                            try
                            {
                                if (h1 <= LPs_scenarios_multi_LF_start) richTextBox_console2.Text +=
                                    // powers
                                    lines_values_set[i1, lines_PROP_P1, h1] + "\t" +
                                    lines_values_set[i1, lines_PROP_Q1, h1] + "\t" +
                                    lines_values_set[i1, lines_PROP_P2, h1] + "\t" +
                                    lines_values_set[i1, lines_PROP_Q2, h1] + "\t" +
                                    lines_values_set[i1, lines_PROP_P3, h1] + "\t" +
                                    lines_values_set[i1, lines_PROP_Q3, h1] + "\t" +
                                    // lines
                                    lines_values_set[i1, lines_PROP_U1, h1] + "\t" +
                                    lines_values_set[i1, lines_PROP_U2, h1] + "\t" +
                                    lines_values_set[i1, lines_PROP_U3, h1];
                                else richTextBox_console2.Text +=
                                    double.Parse(lines_values_set[i1, lines_PROP_P1, h1]).ToString("##0.00") + "\t" +
                                    double.Parse(lines_values_set[i1, lines_PROP_Q1, h1]).ToString("##0.00") + "\t" +
                                    double.Parse(lines_values_set[i1, lines_PROP_P2, h1]).ToString("##0.00") + "\t" +
                                    double.Parse(lines_values_set[i1, lines_PROP_Q2, h1]).ToString("##0.00") + "\t" +
                                    double.Parse(lines_values_set[i1, lines_PROP_P3, h1]).ToString("##0.00") + "\t" +
                                    double.Parse(lines_values_set[i1, lines_PROP_Q3, h1]).ToString("##0.00") + "\t" +
                                    double.Parse(lines_values_set[i1, lines_PROP_U1, h1]).ToString("000.00") + "\t" +
                                    double.Parse(lines_values_set[i1, lines_PROP_U2, h1]).ToString("000.00") + "\t" +
                                    double.Parse(lines_values_set[i1, lines_PROP_U3, h1]).ToString("000.00") + "\t";
                            }
                            catch { Console.WriteLine("Gph_element_identification_lines-Err1"); }
                        }
                        for (int h1 = LPs_scenarios_multi_LF_start + 2; h1 < LPs_scenarios_multi_LF_start + 24 + 2; h1++)
                        {
                            // salvare in canalul grafic 1
                            if (lines_values_set[i1, lines_PROP_P, h1] != "")
                                SimpleGph_channels1[channel_free1, h1 - LPs_scenarios_multi_LF_start - 2] = double.Parse(lines_values_set[i1, lines_PROP_P, h1]);
                            // salvare in canalul grafic 2
                            if (lines_values_set[i1, lines_PROP_U1, h1] != "")
                                SimpleGph_channels2[channel_free2, h1 - LPs_scenarios_multi_LF_start - 2] = double.Parse(lines_values_set[i1, lines_PROP_U1, h1]);
                        }
                        channel_name[channel_free1] = lines[i1, lines_PROP_name];
                        channel_free1++; if (channel_free1 >= SimpleGph_channels_MAX) channel_free1 = 0;
                        channel_free2++; if (channel_free2 >= SimpleGph_channels_MAX) channel_free2 = 0;
                    }
                        Gph_element_identified = "line";
                    }
                    else lines[i1, lines_PROP_gph_selected] = "0";

                    if (selected_line != -1) richTextBox_console_cd.Text = "object=line\n" + "name=" + lines[selected_line, lines_PROP_name] + "\n"
                         + "attrib=" + "\n" + "value=";
                }
            }

        }

        private void Gph_element_identification_generators(int xm, int ym, int draw_param)
        {
            int x1, y1, inside;
            for (int i1 = 0; i1 < generators_no; i1++) // scanare obiecte de tip "generators"
            {
                if ((generators[i1, generators_PROP_x0] != "") && (generators[i1, generators_PROP_y0] != ""))
                {
                    x1 = int.Parse(generators[i1, generators_PROP_x0]) - X0_shift;
                    y1 = int.Parse(generators[i1, generators_PROP_y0]) - Y0_shift;
                    inside = Inside_rect(x1, y1, x1 + object_dx, y1 + line_dy, xm, ym);
                    if (inside == 1)
                    {
                        generators[i1, generators_PROP_gph_selected] = "1";
                        richTextBox_console2.Text = "generator=#" + i1.ToString() + "\nname=" + generators[i1, generators_PROP_name]
                            + "\nBus=" + generators[i1, generators_PROP_bus] + "\nPn=" + generators[i1, generators_PROP_Pn]
                            + "\nPn= " + generators[i1, generators_PROP_Pn] + "\nQn= " + generators[i1, generators_PROP_Qn]
                            + "\nPn_max= " + generators[i1, generators_PROP_Pn_max] 
                            + "\nQn_max= " + generators[i1, generators_PROP_Qn_max]
                            + "\nPF= " + generators[i1, generators_PROP_PF] + "\nmodel= " + generators[i1, generators_PROP_model]
                            + "\nP1= " + generators[i1, generators_PROP_P1]
                            + "\nP2= " + generators[i1, generators_PROP_P2] + "\nP3= " + generators[i1, generators_PROP_P3]
                            + "\nQ1= " + generators[i1, generators_PROP_Q1]
                            + "\nQ2= " + generators[i1, generators_PROP_Q2] + "\nQ3= " + generators[i1, generators_PROP_Q3]
                            + "\nP= " + generators[i1, generators_PROP_Preal] + " [kW]\nQ= " + generators[i1, generators_PROP_Qreal] + " [kvar]"
                            + "\nU1= " + generators[i1, generators_PROP_U1]
                            + "\nU2= " + generators[i1, generators_PROP_U2] + "\nU3= " + generators[i1, generators_PROP_U3]
                            + "\nU1fi= " + generators[i1, generators_PROP_U1fi]
                            + "\nU2fi= " + generators[i1, generators_PROP_U2fi] + "\nU3fi= " + generators[i1, generators_PROP_U3fi]
                            + "\nI1= " + generators[i1, generators_PROP_I1]
                            + "\nI2= " + generators[i1, generators_PROP_I2] + "\nI3= " + generators[i1, generators_PROP_I3]
                            + "\nI1fi= " + generators[i1, generators_PROP_I1fi]
                            + "\nI2fi= " + generators[i1, generators_PROP_I2fi] + "\nI3fi= " + generators[i1, generators_PROP_I3fi]
                            ;

                        richTextBox_console_cd.Text = "object=generator\n" + "name=" + generators[i1, generators_PROP_name] + "\n"
                            + "attrib=" + "\n" + "value=";
                        // associate with a channel in the SmalGph area
                        for (int h1 = LPs_scenarios_multi_LF_start + 2; h1 < LPs_scenarios_multi_LF_start + 24 + 2; h1++)
                        {
                            // salvare in canalul grafic 1
                            if (generators_values_set[i1, generators_PROP_P, h1] != "")
                                SimpleGph_channels1[channel_free1, h1 - LPs_scenarios_multi_LF_start - 2] = double.Parse(generators_values_set[i1, generators_PROP_P, h1]);
                            // salvare in canalul grafic 2
                            if (generators_values_set[i1, generators_PROP_U1, h1] != "")
                                SimpleGph_channels2[channel_free2, h1 - LPs_scenarios_multi_LF_start - 2] = double.Parse(generators_values_set[i1, generators_PROP_U1, h1]);
                        }
                        channel_name[channel_free1] = generators[i1, generators_PROP_name];
                        channel_free1++; if (channel_free1 >= SimpleGph_channels_MAX) channel_free1 = 0;
                        channel_free2++; if (channel_free2 >= SimpleGph_channels_MAX) channel_free2 = 0;

                        Gph_element_identified = "generator";
                    }
                    else generators[i1, generators_PROP_gph_selected] = "0";
                }
            }
            for (int i1 = 0; i1 < interracts_no; i1++) // scanare obiecte de tip "interracts"
            {
                if ((interracts[i1, interracts_PROP_x0] != "") && (interracts[i1, interracts_PROP_y0] != ""))
                {
                    string s1 = "";
                    x1 = int.Parse(interracts[i1, interracts_PROP_x0]);
                    y1 = int.Parse(interracts[i1, interracts_PROP_y0]);
                    inside = Inside_rect(x1, y1, x1 + 20, y1 + 11, xm, ym);
                    if (inside == 1)
                    {
                        interracts[i1, interracts_PROP_gph_selected] = "1";
                        s1 = interracts[i1, interracts_PROP_command];
                        s1 = s1.Replace('>', '='); s1 = s1.Replace(';', '\n');
                        richTextBox_console_cd.Text = s1;
                    }
                    else interracts[i1, interracts_PROP_gph_selected] = "0";
                }
            }

        }

        private void Gph_element_identification_measurements(int xm, int ym, int draw_param)
        {
            int x1, y1, inside;

            for (int i1 = 0; i1 < measurements_no; i1++) // scanare obiecte de tip "interracts"
            {
                if ((measurements[i1, interracts_PROP_x0] != "") && (measurements[i1, interracts_PROP_y0] != ""))
                {
                    //string s1 = "";
                    x1 = int.Parse(measurements[i1, measurements_PROP_x0]) - X0_shift;
                    y1 = int.Parse(measurements[i1, measurements_PROP_y0]) - Y0_shift;
                    inside = Inside_rect(x1, y1, x1 + 20, y1 + 95, xm, ym);
                    if (inside == 1)
                    {
                        measurements[i1, measurements_PROP_gph_selected] = "1";
                        richTextBox_console2.Text = "measurement=#" + i1.ToString() + "\nname=" + measurements[i1, measurements_PROP_name]
                            + "\nP=" + trafos[i1, trafos_PROP_P] + "\nQ=" + trafos[i1, trafos_PROP_Q]
                             + "\nS=" + trafos[i1, trafos_PROP_S];

                        richTextBox_console_cd.Text = "object=measurement\n" + "name=" + measurements[i1, trafos_PROP_name] + "\n"
                            + "attrib=" + "\n" + "value=";
                    }
                    else measurements[i1, measurements_PROP_gph_selected] = "0";
                    //richTextBox_console_cd.Text = "object=line\n" + "name=" + lines[selected_line, lines_PROP_name] + "\n"
                    //     + "attrib=" + "\n" + "value=";
                }
            }
        }

        private void Graph_phasors_identification(int xm, int ym, int draw_param)
        {
            int x1, y1, inside, inside_enrage;

            for (int i1 = 0; i1 < graph_phasors_no; i1++) // scanare obiecte de tip "interracts"
            {
                if ((graph_phasors[i1, graph_phasors_PROP_x0] != "") && (graph_phasors[i1, graph_phasors_PROP_y0] != ""))
                {
                    //string s1 = "";
                    x1 = int.Parse(graph_phasors[i1, graph_phasors_PROP_x0]) - X0_shift;
                    y1 = int.Parse(graph_phasors[i1, graph_phasors_PROP_y0]) - Y0_shift;
                    inside = Inside_rect(x1, y1, x1 + gph_phasors_legend_dx, y1 + gph_phasors_legend_dx, xm, ym);
                    inside_enrage = Inside_rect(x1+50, y1, x1 + 110, y1 + 20, xm, ym);
                    if (inside_enrage == 1)
                    {
                        if ((graph_phasors[i1, graph_phasors_PROP_enlarge] == "") || (graph_phasors[i1, graph_phasors_PROP_enlarge] == "0"))
                            graph_phasors[i1, graph_phasors_PROP_enlarge] = "1";
                        else graph_phasors[i1, graph_phasors_PROP_enlarge] = "0";
                    }
                    if (inside == 1)
                    {
                        graph_phasors[i1, graph_phasors_PROP_gph_selected] = "1";
                        richTextBox_console2.Text = "graph_phasors=#" + i1.ToString() + "\nname=" + graph_phasors[i1, graph_phasors_PROP_name]
                            + "\nx0=" + graph_phasors[i1, graph_phasors_PROP_x0] + "\ty0=" + graph_phasors[i1, graph_phasors_PROP_y0]
                            + "\ncommand=" + graph_phasors[i1, graph_phasors_PROP_command];

                        // pre-completing cd text
                        richTextBox_console_cd.Text = "object=graph_phasors\n" + "name=" + graph_phasors[i1, graph_phasors_PROP_name] + "\n"
                             + "attrib=" + "\n" + "value=";
                    }
                    else graph_phasors[i1, graph_phasors_PROP_gph_selected] = "0";

                }
            }
        }

        private void Gph_element_identification_nodes(int xm, int ym, int draw_param)
        {
            int x1, y1, inside;

            for (int i1 = 0; i1 < nodes_no; i1++) // scanare obiecte de tip "nodes"
            {
                if ((nodes[i1, nodes_PROP_x0] != "") && (nodes[i1, nodes_PROP_y0] != ""))
                {
                    x1 = int.Parse(nodes[i1, nodes_PROP_x0]) - X0_shift;
                    y1 = int.Parse(nodes[i1, nodes_PROP_y0]) - Y0_shift;
                    inside = Inside_rect(x1, y1, x1 + object_dx, y1 + 12, xm, ym);
                    if (inside == 1)
                    {
                        nodes[i1, nodes_PROP_gph_selected] = "1";
                        richTextBox_console2.Text = "node=#" + i1.ToString() + "\nname=" + nodes[i1, nodes_PROP_name]
                             + "\nbus_name=" + nodes[i1, nodes_PROP_bus_name]
                            + "\nBus=" + nodes[i1, nodes_PROP_bus] + "\nU_source=" 
                            + nodes[i1, nodes_PROP_U_source_object] 
                            + "\nU_source_no=" + nodes[i1, nodes_PROP_U_source_object_number]
                            +"\nU_source_name=" + nodes[i1, nodes_PROP_U_source_object_name]
                            +"\nU_source_terminal=" + nodes[i1, nodes_PROP_U_source_object_terminal];
                        richTextBox_console2.Text += "\nU1=" + nodes[i1, nodes_PROP_U1];
                        richTextBox_console2.Text += "\nU2=" + nodes[i1, nodes_PROP_U2];
                        richTextBox_console2.Text += "\nU3=" + nodes[i1, nodes_PROP_U3];
                        richTextBox_console2.Text += "\nUn=" + nodes[i1, nodes_PROP_voltage];
                        richTextBox_console2.Text += "\nConnected obj=" + nodes[i1, nodes_PROP_number_of_connected_objects];
                        richTextBox_console2.Text += "\nConnect=" + nodes[i1, nodes_PROP_list_of_connected_objects];
                        richTextBox_console2.Text += "\nDraw_U_proc=" + nodes_Draw_U_proc[i1].Attrib_text;
                        richTextBox_console2.Text += "\nPolylines=" + nodes[i1, nodes_PROP_plylines];

                        richTextBox_console_cd.Text = "object=node\n" + "name=" + nodes[i1, nodes_PROP_bus] + "\n"
                            + "attrib=" + "Polylines\n" + "value=";
                    }
                    else nodes[i1, nodes_PROP_gph_selected] = "0";
                }
            }

        }

        private void Gph_element_identification_labels(int xm, int ym, int draw_param)
        {
            int x1, y1, inside;

            for (int i1 = 0; i1 < labels_no; i1++) // scanare obiecte de tip "label"
            {
                if ((labels[i1, labels_PROP_x0] != "") && (labels[i1, labels_PROP_y0] != ""))
                {
                    x1 = int.Parse(labels[i1, labels_PROP_x0]) - X0_shift;
                    y1 = int.Parse(labels[i1, labels_PROP_y0]) - Y0_shift;
                    inside = Inside_rect(x1, y1, x1 + object_dx, y1 + 12, xm, ym);
                    if (inside == 1)
                    {
                        labels[i1, labels_PROP_gph_selected] = "1";
                        richTextBox_console2.Text = "label=#" + i1.ToString() + "\nname=" + labels[i1, labels_PROP_name]
                            + "\naction=" + labels[i1, labels_PROP_action];// + "\nU_source=" + nodes[i1, nodes_PROP_U_source_object];

                        if (labels[i1, labels_PROP_action] == "Grid_sumary")
                        {
                            calculate_grid_data1_string = "";
                            Calculate_grid_data1();
                            richTextBox_console2.Text += calculate_grid_data1_string;
                        }
                    }
                    else labels[i1, labels_PROP_gph_selected] = "0";
                }
            }

        }

        private void Gph_element_identification_graph_pies(int xm, int ym, int draw_param)
        {
            int x1, y1, inside;
            int graph_pies_dx = 120;
            int graph_pies_dy = 60;

            for (int i1 = 0; i1 < graph_pies_no; i1++) // scanare obiecte de tip "label"
            {
                if ((graph_pies[i1, graph_pies_PROP_x0] != "") && (graph_pies[i1, graph_pies_PROP_y0] != ""))
                {
                    x1 = int.Parse(graph_pies[i1, graph_pies_PROP_x0]) - X0_shift;
                    y1 = int.Parse(graph_pies[i1, graph_pies_PROP_y0]) - Y0_shift;
                    inside = Inside_rect(x1, y1, x1 + graph_pies_dx, y1 + graph_pies_dy, xm, ym);
                    if (inside == 1)
                    {
                        graph_pies[i1, graph_pies_PROP_gph_selected] = "1";
                        richTextBox_console2.Text = "graph_pies=#" + i1.ToString() + "\nname=" + graph_pies[i1, graph_pies_PROP_name]
                            + "\ncommand=" + graph_pies[i1, graph_pies_PROP_command]
                            + "\nobj=" + graph_pies[i1, graph_pies_PROP_obj]
                            + "\nnumber=" + graph_pies[i1, graph_pies_PROP_number]
                            + "\nMin=" + graph_pies[i1, graph_pies_PROP_min]
                            + "\nMax=" + graph_pies[i1, graph_pies_PROP_max]
                            + "\nx0=" + graph_pies[i1, graph_pies_PROP_x0]
                            + "\ny0=" + graph_pies[i1, graph_pies_PROP_y0]
                            ;
                    }
                    else graph_pies[i1, graph_pies_PROP_gph_selected] = "0";
                }
            }

        }

        private void Gph_element_identification_graphs(int xm, int ym, int draw_param)
        {
            int x1, dx1, y1, dy1, inside;

            for (int i1 = 0; i1 < graphs_no; i1++) // scanare obiecte de tip "graphs"
            {
                if ((graphs[i1, graphs_PROP_x0] != "") && (graphs[i1, graphs_PROP_y0] != ""))
                // daca coordonatele graficului au fost initializate
                {
                    x1 = int.Parse(graphs[i1, graphs_PROP_x0]) - X0_shift;
                    y1 = int.Parse(graphs[i1, graphs_PROP_y0]) - Y0_shift;
                    dx1 = Gph_dXlegend + int.Parse(graphs[i1, graphs_PROP_Samples_max]) * int.Parse(graphs[i1, graphs_PROP_Samples_X_width]);
                    dy1 = int.Parse(graphs[i1, graphs_PROP_graph_dY]);
                    inside = Inside_rect(x1, y1, x1 + dx1, y1 + dy1, xm, ym);
                    if (inside == 1) // afiseaza in fereastra Console_2
                    {
                        graphs[i1, graphs_PROP_gph_selected] = "1";
                        richTextBox_console2.Text = "graph=#" + i1.ToString()
                            + "\nname=" + graphs[i1, graphs_PROP_name] + "   [_cfg]"
                            + "\nTitle=" + graphs[i1, graphs_PROP_gph_title] + "   [_cfg]"
                            + "\nx0=" + graphs[i1, graphs_PROP_x0]  + "   [0 .. 1800]"
                            + "\ny0=" + graphs[i1, graphs_PROP_y0] + "   [0 .. 1000]"
                            + "\nSamples_max=" + graphs[i1, graphs_PROP_Samples_max] + "   [24 .. 1200]"
                            + "\nSamples_X_width=" + graphs[i1, graphs_PROP_Samples_X_width] + "   [1 .. 5]"
                            + "\nY_min=" + graphs[i1, graphs_PROP_Y_min] + "   [..] (lowest value on Y axis)"
                            + "\nY_max=" + graphs[i1, graphs_PROP_Y_max] + "   [..] (highest value on Y axis)"
                            + "\nGraph_dy=" + graphs[i1, graphs_PROP_graph_dY] + "   [..] ()"
                            + "\nGraph_dx_legend=" + graphs[i1, graphs_PROP_dX_legend] + "   [..] ()"
                            ;

                        Gph_element_identified = "graph";
                    }
                    else graphs[i1, graphs_PROP_gph_selected] = "0";

                    richTextBox_console_cd.Text = "object=graph\n" + "name=" + graphs[i1, graphs_PROP_name] + "\n"
                        + "attrib=" + "\n" + "value=";
                }
            }
        }


        private void Gph_element_identification_smart_meters(int xm, int ym, int draw_param)
        {
            int x1, y1, inside;
            int smart_meters_dx = 200;
            int smart_meters_dy = 200;

            for (int i1 = 0; i1 < smart_meters_no; i1++) // scanare obiecte de tip "label"
            {
                if ((smart_meters[i1, smart_meters_PROP_x0] != "") && (smart_meters[i1, smart_meters_PROP_y0] != ""))
                {
                    x1 = int.Parse(smart_meters[i1, smart_meters_PROP_x0]) - X0_shift;
                    y1 = int.Parse(smart_meters[i1, smart_meters_PROP_y0]) - Y0_shift;
                    inside = Inside_rect(x1, y1, x1 + smart_meters_dx, y1 + smart_meters_dy, xm, ym);
                    if (inside == 1)
                    {
                        smart_meters[i1, smart_meters_PROP_gph_selected] = "1";
                        richTextBox_console2.Text = "smart_meters=#" + i1.ToString() + "\nname=" + smart_meters[i1, smart_meters_PROP_name]
                            + "\nobj=" + smart_meters[i1, smart_meters_PROP_obj]
                            + "\nnumber=" + smart_meters[i1, smart_meters_PROP_number]
                            + "\nx0=" + smart_meters[i1, smart_meters_PROP_x0]
                            + "\ny0=" + smart_meters[i1, smart_meters_PROP_y0]
                            ;
                    }
                    else smart_meters[i1, smart_meters_PROP_gph_selected] = "0";
                }
            }

        }

        private void Gph_element_identification_HIL_FrontEnds(int xm, int ym, int draw_param)
        {
            int x1, y1, inside;

            for (int i1 = 0; i1 < HIL_FrontEnd_no; i1++) // scanare obiecte de tip "HIL_FrontEnd_no"
            {
                if ((HIL_FrontEnd[i1, HIL_FrontEnd_PROP_X0] != "") && (HIL_FrontEnd[i1, HIL_FrontEnd_PROP_X0] != ""))
                {
                    x1 = int.Parse(HIL_FrontEnd[i1, HIL_FrontEnd_PROP_X0]) - X0_shift;
                    y1 = int.Parse(HIL_FrontEnd[i1, HIL_FrontEnd_PROP_Y0]) - Y0_shift;
                    inside = Inside_rect(x1, y1, x1 + HIL_FrontEnd_dX, y1 + HIL_FrontEnd_dY, xm, ym);
                    if (inside == 1)
                    {
                        HIL_FrontEnd[i1, HIL_FrontEnd_PROP_gph_selected] = "1";
                        richTextBox_console2.Text = "HIL_FrontEnd=#" + i1.ToString()
                            + "\nName=" + HIL_FrontEnd[i1, HIL_FrontEnd_PROP_Name]
                            + "\nComType=" + HIL_FrontEnd[i1, HIL_FrontEnd_PROP_ComType]
                            + "\nAddr/COM=" + HIL_FrontEnd[i1, HIL_FrontEnd_PROP_ComAddr]
                            + "\nBaudrate=" + HIL_FrontEnd[i1, HIL_FrontEnd_PROP_Baudrate]
                            + "\nProtocol=" + HIL_FrontEnd[i1, HIL_FrontEnd_PROP_ProtocolType]
                            + "\nProtocol_Ver=" + HIL_FrontEnd[i1, HIL_FrontEnd_PROP_ProtocolVer]
                            + "\nObjType=" + HIL_FrontEnd[i1, HIL_FrontEnd_PROP_ObjType]
                            + "\nLine=" + lines[int.Parse(HIL_FrontEnd[i1, HIL_FrontEnd_PROP_ObjNo]), lines_PROP_name]
                            +"\nTerminal=" + HIL_FrontEnd[i1, HIL_FrontEnd_PROP_Terminal]
                            + "\nPhases=" + HIL_FrontEnd[i1, HIL_FrontEnd_PROP_Phases]
                            + "\nPhaseNo=" + HIL_FrontEnd[i1, HIL_FrontEnd_PROP_Phase_Number]
                            + "\nScaleMaxU=" + HIL_FrontEnd[i1, HIL_FrontEnd_PROP_ScaleMaxU]
                            + "\nScaleMaxI=" + HIL_FrontEnd[i1, HIL_FrontEnd_PROP_ScaleMaxI]
                            + "\nX0=" + HIL_FrontEnd[i1, HIL_FrontEnd_PROP_X0]
                            + "\nY0=" + HIL_FrontEnd[i1, HIL_FrontEnd_PROP_Y0]
                        ;

                    }
                    else HIL_FrontEnd[i1, HIL_FrontEnd_PROP_gph_selected] = "0";
                }
            }

        }


        private void Gph_element_identification(int xm, int ym, int draw_param)
        {
            // In urma clicarii pe un obiect, atributele acestuia sunt afisate in consola 2


            Gph_element_identification_trafos(xm, ym, draw_param);

            Gph_element_identification_loads(xm, ym, draw_param);

            Gph_element_identification_lines(xm, ym, draw_param);

            Gph_element_identification_generators(xm, ym, draw_param);

            Gph_element_identification_measurements(xm, ym, draw_param);

            Graph_phasors_identification(xm, ym, draw_param);

            Gph_element_identification_nodes(xm, ym, draw_param);

            Gph_element_identification_graph_pies(xm, ym, draw_param);

            Gph_element_identification_smart_meters(xm, ym, draw_param);

            Gph_element_identification_HIL_FrontEnds(xm, ym, draw_param);

            Gph_element_identification_labels(xm, ym, draw_param);

            Gph_element_identification_graphs(xm, ym, draw_param);

            Scan_SimpleGph(xm, ym); // Fixed 2x graphs at the bottom of the GUI

            Scan_console_Training(xm, ym); // Training console at the bottom of the GUI, designed initially for WiseGrid, but useful and expandable for all purposes
        }

        double P_bilant_grid_before = 0, lossess_proc_before = 0;
        string calculate_grid_data1_string = "";

        private void Calculate_grid_data1()
        {
            double dval1 = 0, dval2 = 0;
            double P_cons = 0, P_storage = 0, P_gen = 0, P_trafo = 0, P_EV = 0, P_bilant, losses_proc=0;
            double P_PV = 0; double P_RES = 0; double RES_share = 0;
            string details1 = "";
            string details2 = "";
            details1 = "\nGenerators:";
            for (int i1 = 0; i1 < generators_no; i1++) // scanare obiecte de tip "generators"
            {
                if (generators[i1, generators_PROP_Pn] != "") dval1 += double.Parse(generators[i1, generators_PROP_Pn]);
                if (generators[i1, generators_PROP_P] != "") dval2 += double.Parse(generators[i1, generators_PROP_P]);
            }
            details1 += "\nP_tot_inst=" + dval1.ToString("##0.0") + " kW";
            details1 += "\nP_tot_real=" + dval2.ToString("##0.0") + " kW";
            P_gen = dval2;

            dval1 = 0; dval2 = 0;
            details1 += "\nLoads:";
            for (int i1 = 0; i1 < loads_no; i1++) // scanare obiecte de tip "loads"
            {
                if ((loads[i1, loads_PROP_voltage] == "0.4") && ((loads[i1, loads_PROP_sim_type] == ""))) { 
                    // avem sarcini clasice, nu storage sau EV
                    if (loads[i1, loads_PROP_Pn] != "") dval1 += double.Parse(loads[i1, loads_PROP_Pn]);
                    if (loads[i1, loads_PROP_P] != "") dval2 += double.Parse(loads[i1, loads_PROP_P]);
                    details2 += i1.ToString() + ":" + loads[i1, loads_PROP_name] + " Pn=" + loads[i1, loads_PROP_Pn] + " P=" + loads[i1, loads_PROP_P] + "\n";
                }
            }
            details1 += "\nPcons_tot_inst[0.4kV]=" + dval1.ToString("##0.0") + " kW";
            details1 += "\nPcons_tot_real[0.4kV]=" + dval2.ToString("##0.0") + " kW";
            P_cons = dval2;

            dval1 = 0; dval2 = 0;
            for (int i1 = 0; i1 < loads_no; i1++) // scanare obiecte de tip "loads-storage"
            {
                if ((loads[i1, loads_PROP_voltage] == "0.4") && (loads[i1, loads_PROP_sim_type] == "storage"))
                {
                    // avem storage
                    if (loads[i1, loads_PROP_Pn] != "") dval1 += double.Parse(loads[i1, loads_PROP_Pn]);
                    if (loads[i1, loads_PROP_P] != "") dval2 += double.Parse(loads[i1, loads_PROP_P]);
                    details2 += i1.ToString() + ":" + loads[i1, loads_PROP_name] + " Pns=" + loads[i1, loads_PROP_Pn] + " Ps=" + loads[i1, loads_PROP_P] + "\n";
                }
            }
            details1 += "\nPstor_tot_inst[0.4kV]=" + dval1.ToString("##0.0") + " kW";
            details1 += "\nPstor_tot_real[0.4kV]=" + dval2.ToString("##0.0") + " kW";
            P_storage = dval2;

            dval1 = 0; dval2 = 0;
            details1 += "\nTransformers:";
            for (int i1 = 0; i1 < trafos_no; i1++) // scanare obiecte de tip "trafos"
            {
                 if (trafos[i1, trafos_PROP_P] != "") dval2 += double.Parse(trafos[i1, trafos_PROP_P]);
            }
            details1 += "\nPtrafo_tot_real[0.4kV]=" + dval2.ToString("##0.0") + " kW";
            P_trafo = dval2;

            dval1 = 0; dval2 = 0;
            details1 += "\nEVs:";
            for (int i1 = 0; i1 < loads_no; i1++) // scanare obiecte de tip "EV"
            {
                if ((loads[i1, loads_PROP_P] != "")  && (loads[i1, loads_PROP_sim_type] == "EV"))
                    dval2 += double.Parse(loads[i1, loads_PROP_P]);
            }
            details1 += "\nP_EV_tot_real[0.4kV]=" + dval2.ToString("##0.0") + " kW";
            P_EV = dval2;

            // calcul bilant
            P_bilant = -P_gen + P_trafo - P_cons - P_storage - P_EV;
            losses_proc = P_bilant / P_cons * 100;
            // calcule legate de regenerabile
            P_RES = P_gen; // in viitor se va mai rafina
            RES_share = -P_RES / P_cons * 100;
            calculate_grid_data1_string += "\n--------- Summary -----";
            calculate_grid_data1_string += "\nConsumption= " + P_cons.ToString("###0.0##") +" kW";
            calculate_grid_data1_string += "\nProduction= " + P_gen.ToString("###0.0##") + " kW";
            calculate_grid_data1_string += "\nTrafo contrib= " + P_trafo.ToString("###0.0##") + " kW";
            calculate_grid_data1_string += "\nStorage contrib= " + P_storage.ToString("###0.0##") + " kW";
            calculate_grid_data1_string += "\nEV cons= " + P_EV.ToString("###0.0##") + " kW";
            if (P_storage > 0.001) calculate_grid_data1_string += " (cons)";
            if (P_storage < -0.001) calculate_grid_data1_string += " (prod)";
            calculate_grid_data1_string += "\nBalance= " + P_bilant.ToString("###0.0##") + " kW";
            calculate_grid_data1_string += "\nLosses= " + losses_proc.ToString("##0.00") + " [%]";
            calculate_grid_data1_string += "\nLosses before= " + lossess_proc_before.ToString("##0.00") + " [%]";
            calculate_grid_data1_string += "\nRES_share= " + RES_share.ToString("##0.00") + " [%]";
            calculate_grid_data1_string += "\n--------- Details --------------";
            calculate_grid_data1_string += details1;
            calculate_grid_data1_string += "\n--------- Breakdown ------------\n";
            calculate_grid_data1_string += details2;
            int mem_space1 = 0, mem_space2 = 0;
            for (int i1 = 0; i1 < lines_MAX; i1++) for (int j1 = 0; j1 < lines_prop_MAX; j1++) mem_space1 += lines[i1, j1].Length;
            for (int i1 = 0; i1 < lines_MAX; i1++) for (int j1 = 0; j1 < lines_prop_MAX; j1++)
                    for (int k1 = 0; k1 < historical_values_depth_MAX; k1++) mem_space2 += lines_values_set[i1, j1, k1].Length;
            //mem_space = lines.Length;
            calculate_grid_data1_string += "\nlines: " + lines.Length.ToString() + "  scen: " + lines_values_set.Length.ToString();
            calculate_grid_data1_string += "\nMemory used lines: " + mem_space1.ToString();
            calculate_grid_data1_string += "\nMemory used lines scen: " + mem_space2.ToString();
            P_bilant_grid_before = P_bilant;
            lossess_proc_before = losses_proc;
        }

        private void Calculate_grid_data_scenarios_array()
        {
            //int nr_entries = 24;
            int nr_voltage_levels = _GridMonK_max_nr_grid_clusters;
            string[] Voltages = new string[_GridMonK_max_nr_grid_clusters];
            string[] Microgrids = new string[_GridMonK_max_nr_grid_clusters];

            // Predefined microgrids clusters
            Voltages[0] = "0.4"; Microgrids[0] = "A";
            Voltages[1] = "0.4"; Microgrids[1] = "B";
            Voltages[2] = "0.4"; Microgrids[2] = "C";
            //Voltages[3] = "0.4"; Microgrids[3] = "D";
            Voltages[3] = "20"; Microgrids[3] = "A";
            Voltages[4] = "20"; Microgrids[4] = "B";
            Voltages[5] = "110"; Microgrids[5] = "A";
            //Voltages[7] = "110"; Microgrids[7] = "B";
            Voltages[6] = "220"; Microgrids[6] = "A";
            //Voltages[9] = "400"; Microgrids[9] = "A";
            //if (LPs_scenarios_LF_forecasts_length_MAX > 1440) nr_entries = 1440;

            double dval1 = 0, dval2 = 0;

            // consumption
            double[,] P_cons = new double[LPs_scenarios_LF_forecasts_length_MAX, _GridMonK_max_nr_grid_clusters];
            double[,] P_EV = new double[LPs_scenarios_LF_forecasts_length_MAX, _GridMonK_max_nr_grid_clusters];
            double[,] P_storage = new double[LPs_scenarios_LF_forecasts_length_MAX, _GridMonK_max_nr_grid_clusters];
            // generation
            double[,] P_gen = new double[LPs_scenarios_LF_forecasts_length_MAX, _GridMonK_max_nr_grid_clusters];
            double[,] P_PV = new double[LPs_scenarios_LF_forecasts_length_MAX, _GridMonK_max_nr_grid_clusters];
            double[,] P_RES = new double[LPs_scenarios_LF_forecasts_length_MAX, _GridMonK_max_nr_grid_clusters];
            // transformers
            double[,] P_trafo = new double[LPs_scenarios_LF_forecasts_length_MAX, _GridMonK_max_nr_grid_clusters];
            // tie-lines between two clusters at the same voltage level
            double[,] P_tielines = new double[LPs_scenarios_LF_forecasts_length_MAX, _GridMonK_max_nr_grid_clusters];
            // general data
            double[,] P_bilant = new double[LPs_scenarios_LF_forecasts_length_MAX, _GridMonK_max_nr_grid_clusters];
            double[,] losses_proc = new double[LPs_scenarios_LF_forecasts_length_MAX, _GridMonK_max_nr_grid_clusters];
            double[,] RES_share = new double[LPs_scenarios_LF_forecasts_length_MAX, _GridMonK_max_nr_grid_clusters];

            string file_data = "";
            string file_data2 = "";
            file_data2 = "Description,P_cons_tot,P_storage_tot,P_EV_tot,P_PV_tot,P_trafo_tot,P_RES_tot,P_bilant_tot,losses_proc_tot,RES_share_tot\n";

            // Full report of summed data
            double[] P_cons_tot = new double[_GridMonK_max_nr_grid_clusters];
            double[] P_storage_tot = new double[_GridMonK_max_nr_grid_clusters];
            double[] P_EV_tot = new double[_GridMonK_max_nr_grid_clusters];
            double[] P_PV_tot = new double[_GridMonK_max_nr_grid_clusters];
            double[] P_trafo_tot = new double[_GridMonK_max_nr_grid_clusters];
            double[] P_tielines_tot = new double[_GridMonK_max_nr_grid_clusters];
            double[] P_RES_tot = new double[_GridMonK_max_nr_grid_clusters];
            double[] P_bilant_tot = new double[_GridMonK_max_nr_grid_clusters];
            double[] losses_proc_tot = new double[_GridMonK_max_nr_grid_clusters];
            double[] RES_share_tot = new double[_GridMonK_max_nr_grid_clusters];
            string[] GM_objects = new string[_GridMonK_max_nr_grid_clusters];

            //OpenDSS_solve_number, LPs_scenarios_LF_forecasts_length_MAX
            int scenarios_number = int.Parse(OpenDSS_solve_number);
            if (scenarios_number > LPs_scenarios_LF_forecasts_length_MAX) scenarios_number = LPs_scenarios_LF_forecasts_length_MAX;

            for (int MG_cluster=0; MG_cluster< _GridMonK_max_nr_grid_clusters; MG_cluster++)
            {
                GM_objects[MG_cluster] = "";
                for (int timeframe = 1; timeframe < scenarios_number+1; timeframe++) 
                {
                    // scanare obiecte de tip "loads"
                    P_cons[timeframe, MG_cluster] = 0;
                    for (int l1 = 0; l1 < loads_no; l1++)
                    {
                        // classical loads, they do not have any string description in the attribute "loads_PROP_sim_type", 
                        // meaning that there is no !!sim_type=??? declaration in the load object line
                        if ((loads[l1, loads_PROP_voltage] == Voltages[MG_cluster]) && (loads[l1, loads_PROP_MicroGrid1] == Microgrids[MG_cluster]) 
                                && (loads[l1, loads_PROP_sim_type] == ""))
                        {
                            if(timeframe == 1) GM_objects[MG_cluster] += "Ld="+ loads[l1, loads_PROP_name] + ",";
                            if (loads_values_set[l1, loads_PROP_P, timeframe + LPs_scenarios_multi_LF_start] != "") { 
                                dval1 = double.Parse(loads_values_set[l1, loads_PROP_P, timeframe + LPs_scenarios_multi_LF_start]);
                                P_cons[timeframe, MG_cluster] += dval1;
                            }
                        }
                    }

                    // scanare obiecte de tip "loads" with !!sim_type=storage
                    P_storage[timeframe, MG_cluster] = 0;
                    for (int l1 = 0; l1 < loads_no; l1++)
                    {
                        if ((loads[l1, loads_PROP_voltage] == Voltages[MG_cluster]) && (loads[l1, loads_PROP_MicroGrid1] == Microgrids[MG_cluster]) 
                                && (loads[l1, loads_PROP_sim_type] == "storage"))
                        {
                            if (loads_values_set[l1, loads_PROP_P, timeframe + LPs_scenarios_multi_LF_start] != "") { 
                                dval1 = double.Parse(loads_values_set[l1, loads_PROP_P, timeframe + LPs_scenarios_multi_LF_start]);
                                P_storage[timeframe, MG_cluster] += dval1;
                            }
                        }
                    }

                    // scanare obiecte de tip "loads" with !!sim_type=EV
                    P_EV[timeframe, MG_cluster] = 0;
                    for (int l1 = 0; l1 < loads_no; l1++)
                    {
                        if ((loads[l1, loads_PROP_voltage] == Voltages[MG_cluster]) && (loads[l1, loads_PROP_MicroGrid1] == Microgrids[MG_cluster]) 
                                && (loads[l1, loads_PROP_sim_type] == "EV"))
                        {
                            if (loads_values_set[l1, loads_PROP_P, timeframe + LPs_scenarios_multi_LF_start] != "") { 
                                dval1 = double.Parse(loads_values_set[l1, loads_PROP_P, timeframe + LPs_scenarios_multi_LF_start]);
                                P_EV[timeframe, MG_cluster] += dval1;
                            }
                        }
                    }

                    // scanare obiecte de tip "generators"
                    P_PV[timeframe, MG_cluster] = 0;
                    for (int g1 = 0; g1 < generators_no; g1++)
                    {
                        if ((generators[g1, generators_PROP_voltage] == Voltages[MG_cluster]) 
                                && (generators[g1, generators_PROP_MicroGrid1] == Microgrids[MG_cluster]))
                        {
                            if (timeframe == 1) GM_objects[MG_cluster] += "G=" + generators[g1, generators_PROP_name] + ",";
                            if (generators_values_set[g1, generators_PROP_P, timeframe + LPs_scenarios_multi_LF_start] != "") { 
                                dval1 = double.Parse(generators_values_set[g1, generators_PROP_P, timeframe + LPs_scenarios_multi_LF_start]);
                                P_PV[timeframe, MG_cluster] += dval1;
                            }
                        }
                    }

                    // scanare obiecte de tip "trafos"
                    double P3 = 0;
                    for (int t1 = 0; t1 < trafos_no; t1++)
                    {
                        // Calculate 3-phase power 
                        if ((trafos_values_set[t1, trafos_PROP_P1, timeframe + LPs_scenarios_multi_LF_start] != "") &&
                            (trafos_values_set[t1, trafos_PROP_P2, timeframe + LPs_scenarios_multi_LF_start] != "") &&
                            (trafos_values_set[t1, trafos_PROP_P3, timeframe + LPs_scenarios_multi_LF_start] != "")) // if we have active power on each phase
                            P3 = double.Parse(trafos_values_set[t1, trafos_PROP_P1, timeframe + LPs_scenarios_multi_LF_start]) +
                                 double.Parse(trafos_values_set[t1, trafos_PROP_P2, timeframe + LPs_scenarios_multi_LF_start]) +
                                 double.Parse(trafos_values_set[t1, trafos_PROP_P3, timeframe + LPs_scenarios_multi_LF_start]);
                        trafos_values_set[t1, trafos_PROP_P, timeframe + LPs_scenarios_multi_LF_start] = P3.ToString();

                        if ((trafos_values_set[t1, trafos_PROP_P1_t2, timeframe + LPs_scenarios_multi_LF_start] != "") && 
                            (trafos_values_set[t1, trafos_PROP_P2_t2, timeframe + LPs_scenarios_multi_LF_start] != "") &&
                            (trafos_values_set[t1, trafos_PROP_P3_t2, timeframe + LPs_scenarios_multi_LF_start] != "")) // if we have active power on each phase
                            P3 = double.Parse(trafos_values_set[t1, trafos_PROP_P1_t2, timeframe + LPs_scenarios_multi_LF_start]) +
                                 double.Parse(trafos_values_set[t1, trafos_PROP_P2_t2, timeframe + LPs_scenarios_multi_LF_start]) +
                                 double.Parse(trafos_values_set[t1, trafos_PROP_P3_t2, timeframe + LPs_scenarios_multi_LF_start]);
                        trafos_values_set[t1, trafos_PROP_P_t2, timeframe + LPs_scenarios_multi_LF_start] = P3.ToString();
                    }

                    // make sumations of trafo powers on the same microgrid
                    P_trafo[timeframe, MG_cluster] = 0;
                    for (int t1 = 0; t1 < trafos_no; t1++)
                    {
                        if ((trafos[t1, trafos_PROP_U_Sec1_nom] == Voltages[MG_cluster]) 
                                && (trafos[t1, trafos_PROP_MicroGridSec1] == Microgrids[MG_cluster])) // if we have the voltage on firts secondary
                        {
                            if (timeframe == 1) GM_objects[MG_cluster] += "T=" + trafos[t1, generators_PROP_name] + ",";
                            if (trafos_values_set[t1, trafos_PROP_P_t2, timeframe + LPs_scenarios_multi_LF_start] != "") { 
                                dval1 = double.Parse(trafos_values_set[t1, trafos_PROP_P_t2, timeframe + LPs_scenarios_multi_LF_start]);
                                P_trafo[timeframe, MG_cluster] += dval1;
                            }
                        }
                    }

                    // scanare obiecte de tip "tielines"
                    P_tielines[timeframe, MG_cluster] = 0;
                    for (int ln1 = 0; ln1 < lines_no; ln1++)
                    {
                        if ((lines[ln1, lines_PROP_voltage] == Voltages[MG_cluster])
                                && (lines[ln1, lines_PROP_MicroGrid1] == Microgrids[MG_cluster])) // if we have the right voltage and microgrid
                        {
                            if (timeframe == 1) GM_objects[MG_cluster] += "Ln=" + lines[ln1, lines_PROP_name] + ".1,";
                            if (lines_values_set[ln1, lines_PROP_P, timeframe + LPs_scenarios_multi_LF_start] != "") { 
                                dval1 = double.Parse(lines_values_set[ln1, lines_PROP_P, timeframe + LPs_scenarios_multi_LF_start]);
                                P_tielines[timeframe, MG_cluster] += dval1;
                            }
                        }
                        if ((lines[ln1, lines_PROP_voltage] == Voltages[MG_cluster])
                                && (lines[ln1, lines_PROP_MicroGrid2] == Microgrids[MG_cluster])) // if we have the right voltage and microgrid
                        {
                            if (timeframe == 1) GM_objects[MG_cluster] += "Ln=" + lines[ln1, lines_PROP_name] + ".2,";
                            if (lines_values_set[ln1, lines_PROP_P_t2, timeframe + LPs_scenarios_multi_LF_start] != "") { 
                                dval1 = double.Parse(lines_values_set[ln1, lines_PROP_P_t2, timeframe + LPs_scenarios_multi_LF_start]);
                                P_tielines[timeframe, MG_cluster] += dval1;
                            }
                        }
                    }

                    // Make boundaries calculations
                    P_bilant[timeframe, MG_cluster] = P_cons[timeframe, MG_cluster] + P_storage[timeframe, MG_cluster] + P_EV[timeframe, MG_cluster] +
                        P_PV[timeframe, MG_cluster] + P_trafo[timeframe, 0] + P_tielines[timeframe, MG_cluster];
                    P_bilant[timeframe, MG_cluster] = -P_bilant[timeframe, MG_cluster];
                    losses_proc[timeframe, MG_cluster] = P_bilant[timeframe, MG_cluster] / P_cons[timeframe, MG_cluster] * 100.0;
                    RES_share[timeframe, MG_cluster] = -P_PV[timeframe, MG_cluster] / P_cons[timeframe, MG_cluster] * 100.0;
                }

            // Write file to disk
                file_data = "Timeframe,P_cons,P_storage,P_EV,P_PV,P_trafo,P_RES,P_bilant,losses_proc,RES_share,Tielines\n";
                for (int timeframe = 1; timeframe < scenarios_number + 1; timeframe++)
                {
                    file_data += timeframe.ToString() + ","
                        + P_cons[timeframe, MG_cluster].ToString() + ","
                    //file_data +=
                        + P_storage[timeframe, MG_cluster].ToString() + ","
                        + P_EV[timeframe, MG_cluster].ToString() + ","
                        + P_PV[timeframe, MG_cluster].ToString() + ","
                        + P_trafo[timeframe, MG_cluster].ToString() + ","
                        + P_RES[timeframe, MG_cluster].ToString() + ","
                        + P_bilant[timeframe, MG_cluster].ToString() + ","
                        + losses_proc[timeframe, MG_cluster].ToString() + ","
                        + RES_share[timeframe, MG_cluster].ToString() + ","
                        + P_tielines[timeframe, MG_cluster].ToString()
                        + "\n";
                }
                string GridMonk2OpenDSS_MG_file = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" +
                    "Report_Microgrid_" + Voltages[MG_cluster] + "_" + Microgrids[MG_cluster] + ".csv";
                File.WriteAllText(GridMonk2OpenDSS_MG_file, file_data);

                P_cons_tot[MG_cluster] = 0; P_storage_tot[MG_cluster] = 0; P_EV_tot[MG_cluster] = 0;
                P_PV_tot[MG_cluster] = 0; P_trafo_tot[MG_cluster] = 0;
                P_RES_tot[MG_cluster] = 0; P_bilant_tot[MG_cluster] = 0;
                losses_proc_tot[MG_cluster] = 0; RES_share_tot[MG_cluster] = 0;

                for (int timeframe = 1; timeframe < scenarios_number + 1; timeframe++)
                {
                    P_cons_tot[MG_cluster] += P_cons[timeframe, 0];
                    P_storage_tot[MG_cluster] += P_storage[timeframe, 0];
                    P_EV_tot[MG_cluster] += P_EV[timeframe, 0];
                    P_PV_tot[MG_cluster] += P_PV[timeframe, 0];
                    P_trafo_tot[MG_cluster] += P_trafo[timeframe, 0];
                    P_RES_tot[MG_cluster] += P_RES[timeframe, 0];
                    P_bilant_tot[MG_cluster] += P_bilant[timeframe, 0];
                    losses_proc_tot[MG_cluster] += losses_proc[timeframe, 0];
                    RES_share_tot[MG_cluster] += RES_share[timeframe, 0];
                }
                if (Scenario_timeframe_length == "1s")
                {
                    P_cons_tot[MG_cluster] = P_cons_tot[MG_cluster] * 1 / 3600;
                    P_storage_tot[MG_cluster] = P_storage_tot[MG_cluster] * 1 / 3600;
                    P_EV_tot[MG_cluster] = P_EV_tot[MG_cluster] * 1 / 3600;
                    P_PV_tot[MG_cluster] = P_PV_tot[MG_cluster] * 1 / 3600;
                    P_trafo_tot[MG_cluster] = P_trafo_tot[MG_cluster] * 1 / 3600;
                    P_RES_tot[MG_cluster] = P_RES_tot[MG_cluster] * 1 / 3600;
                    P_bilant_tot[MG_cluster] = P_bilant_tot[MG_cluster] * 1 / 3600;
                }
                if (Scenario_timeframe_length == "1m")
                {
                    P_cons_tot[MG_cluster] = P_cons_tot[MG_cluster] * 1 / 60;
                    P_storage_tot[MG_cluster] = P_storage_tot[MG_cluster] * 1 / 60;
                    P_EV_tot[MG_cluster] = P_EV_tot[MG_cluster] * 1 / 60;
                    P_PV_tot[MG_cluster] = P_PV_tot[MG_cluster] * 1 / 60;
                    P_trafo_tot[MG_cluster] = P_trafo_tot[MG_cluster] * 1 / 60;
                    P_RES_tot[MG_cluster] = P_RES_tot[MG_cluster] * 1 / 60;
                    P_bilant_tot[MG_cluster] = P_bilant_tot[MG_cluster] * 1 / 60;
                }
                losses_proc_tot[MG_cluster] = losses_proc_tot[MG_cluster] / scenarios_number;
                RES_share_tot[MG_cluster] = RES_share_tot[MG_cluster] / scenarios_number;

                file_data2 += "MG_" + Voltages[MG_cluster] + "_" + Microgrids[MG_cluster] + "," 
                    + P_cons_tot[MG_cluster].ToString() + ","
                    + P_storage_tot[MG_cluster].ToString() + ","
                    + P_EV_tot[MG_cluster].ToString("#0.000") + ","
                    + P_PV_tot[MG_cluster].ToString("#0.000") + ","
                    + P_trafo_tot[MG_cluster].ToString("#0.000") + ","
                    + P_RES_tot[MG_cluster].ToString("#0.000") + ","
                    + P_bilant_tot[MG_cluster].ToString("#0.000") + ","
                    + losses_proc_tot[MG_cluster].ToString("#0.00") + ","
                    + RES_share_tot[MG_cluster].ToString("#0.00") + ","
                    + GM_objects[MG_cluster] + "\n";

            }

            string GridMonk2OpenDSS_MG_report_file = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + "Report_Microgrid_totals_clusters.csv";
            File.WriteAllText(GridMonk2OpenDSS_MG_report_file, file_data2);

        }
    }
}