﻿using System;
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

        private void Gph_element_identification_trafos(int xm, int ym)
        {
            int x1, x2, y1, y2, inside;

            for (int i1 = 0; i1 < trafos_no; i1++) // scanare obiecte de tip "loads"
            {
                if ((trafos[i1, trafos_PROP_x0] != "") && (trafos[i1, trafos_PROP_y0] != ""))
                // daca coordonatele transformatorului au fost initializate
                {
                    x1 = int.Parse(trafos[i1, trafos_PROP_x0]);
                    y1 = int.Parse(trafos[i1, trafos_PROP_y0]);
                    inside = inside_rect(x1, y1, x1 + object_dx, y1 + line_dy, xm, ym);
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
                    }
                    else trafos[i1, trafos_PROP_gph_selected] = "0";
                }
            }

        }

        private void Gph_element_identification_loads(int xm, int ym)
        {
            int x1, dx, y1, dy, inside;

            for (int i1 = 0; i1 < loads_no; i1++) // scanare obiecte de tip "loads"
            {
                if ((loads[i1, loads_PROP_x0] != "") && (loads[i1, loads_PROP_y0] != ""))
                // daca coordonatele au fost initializate
                {
                    x1 = int.Parse(loads[i1, loads_PROP_x0]);
                    y1 = int.Parse(loads[i1, loads_PROP_y0]);
                    if(loads[i1, loads_PROP_sim_type] == "EV") {
                        dx = object_dx_EVtot; dy = object_dysmall_EV;
                    }
                    else {
                        dx = object_dx; dy = line_dy;
                    }
                    inside = inside_rect(x1, y1, x1 + dx, y1 + dy, xm, ym);
                    if (inside == 1)
                    {
                        loads[i1, loads_PROP_gph_selected] = "1";
                        richTextBox_console2.Text = "load=#" + i1.ToString() + "\nname=" + loads[i1, loads_PROP_name]
                            + "\nBus=" + loads[i1, loads_PROP_bus] + "\nPn=" + loads[i1, loads_PROP_Pn]
                            + "\nQn=" + loads[i1, loads_PROP_Qn] + "\nPF=" + loads[i1, loads_PROP_PF]
                            + "\nmodel=" + loads[i1, loads_PROP_model] + "\nstatus=" + loads[i1, loads_PROP_status]
                            + "\ndaily=" + loads[i1, loads_PROP_daily] + "\nconn=" + loads[i1, loads_PROP_conn]
                            + "\nVminpu=" + loads[i1, loads_PROP_Vminpu] + "\nVmaxpu=" + loads[i1, loads_PROP_Vmaxpu]
                            + "\nU1=" + loads[i1, loads_PROP_U1] + "\nU2=" + loads[i1, loads_PROP_U2] + "\nU3=" + loads[i1, loads_PROP_U3]
                            + "\nU1fi=" + loads[i1, loads_PROP_U1fi] + "\nU2fi=" + loads[i1, loads_PROP_U2fi] + "\nU3fi=" + loads[i1, loads_PROP_U3fi]
                            + "\nI1=" + loads[i1, loads_PROP_I1] + "\nI2=" + loads[i1, loads_PROP_I2] + "\nI3=" + loads[i1, loads_PROP_I3]
                            + "\nI1fi=" + loads[i1, loads_PROP_I1fi] + "\nI2fi=" + loads[i1, loads_PROP_I2fi] + "\nI3fi=" + loads[i1, loads_PROP_I3fi]
                            + "\nU4=" + loads[i1, loads_PROP_U4] + "\nU4fi=" + loads[i1, loads_PROP_U4fi]
                            + "\nI4=" + loads[i1, loads_PROP_I4] + "\nI4fi=" + loads[i1, loads_PROP_I4fi];

                        // Afisare P1, Q1 din rezulatele OpneDSS
                        if (loads[i1, loads_PROP_P1] != "") richTextBox_console2.Text += "\nP1=" + loads[i1, loads_PROP_P1] + " kW";
                        if (loads[i1, loads_PROP_Q1] != "") richTextBox_console2.Text += "\nQ1=" + loads[i1, loads_PROP_Q1] + " kvar";

                        if (loads[i1, loads_PROP_P2] != "") richTextBox_console2.Text += "\nP2=" + loads[i1, loads_PROP_P2] +" kW";
                        if (loads[i1, loads_PROP_Q2] != "") richTextBox_console2.Text += "\nQ2=" + loads[i1, loads_PROP_Q2] + " kvar";

                        if (loads[i1, loads_PROP_P3] != "") richTextBox_console2.Text += "\nP3=" + loads[i1, loads_PROP_P3] + " kW";
                        if (loads[i1, loads_PROP_Q3] != "") richTextBox_console2.Text += "\nQ3=" + loads[i1, loads_PROP_Q3] + " kvar";

                        richTextBox_console2.Text +=
                            //"\nP1=" + loads[i1, loads_PROP_P1] + "\nP2=" + loads[i1, loads_PROP_P2] + "\nP3=" + loads[i1, loads_PROP_P3]
                            //+ "\nQ1=" + loads[i1, loads_PROP_Q1] + "\nQ2=" + loads[i1, loads_PROP_Q2] + "\nQ3=" + loads[i1, loads_PROP_Q3] 
                            "\nx0=" + loads[i1, loads_PROP_x0] + "\ny0=" + loads[i1, loads_PROP_y0]
                            + "\nP=" + loads[i1, loads_PROP_P] + "\nQ=" + loads[i1, loads_PROP_Q]
                            + "\nbrk=" + loads[i1, loads_PROP_brk]
                            ;

                        richTextBox_console_cd.Text = "object=load\n" + "name=" + loads[i1, loads_PROP_name] + "\n"
                            + "attrib=" + "\n" + "value=";
                        
                        for (int h1 = LPs_scenarios_multi_LF_start + 2; h1 < LPs_scenarios_multi_LF_start + 24 + 2; h1++)
                        {
                            // salvare in canalul grafic 1
                            if (loads_values_set[i1, loads_PROP_P, h1] != "") {
                                if (graph_smallgph[0, graph_smallgph_PROP_P_type] == "Q")
                                    SimpleGph_channels1[channel_free1, h1 - LPs_scenarios_multi_LF_start - 1] = double.Parse(loads_values_set[i1, loads_PROP_Q, h1]);
                                else
                                    SimpleGph_channels1[channel_free1, h1 - LPs_scenarios_multi_LF_start - 1] = double.Parse(loads_values_set[i1, loads_PROP_P, h1]);
                            }
                            // salvare in canalul grafic 2
                            if (loads_values_set[i1, loads_PROP_U1, h1] != "")
                                SimpleGph_channels2[channel_free2, h1 - LPs_scenarios_multi_LF_start - 1] = double.Parse(loads_values_set[i1, loads_PROP_U1, h1]);
                        }
                        channel_name[channel_free1] = loads[i1, loads_PROP_name];
                        channel_free1++; if (channel_free1 >= SimpleGph_channels_MAX) channel_free1 = 0;
                        channel_free2++; if (channel_free2 >= SimpleGph_channels_MAX) channel_free2 = 0;
                        
                    }
                    else loads[i1, loads_PROP_gph_selected] = "0";
                }
            }
        }

        private void Gph_element_identification_lines(int xm, int ym)
        {
            int x1, dx, y1, dy, inside;

            for (int i1 = 0; i1 < lines_no; i1++) // scanare obiecte de tip "lines"
            {
                double dbl_tmp = 0;
                int selected_line = -1;
                if ((lines[i1, lines_PROP_x0] != "") && (lines[i1, lines_PROP_x0] != ""))
                // daca coordonatele au fost initializate pentru pozitai obiectelor de tip "lines"
                {
                    x1 = int.Parse(lines[i1, lines_PROP_x0]); y1 = int.Parse(lines[i1, lines_PROP_y0]);
                    inside = inside_rect(x1, y1, x1 + object_dx, y1 + line_dy, xm, ym);
                    if (inside == 1)
                    { // linia curenta este obiectul selectat
                      // se afiseaza in console 2 atributele obiectlui selectat
                        selected_line = i1;
                        richTextBox_console2.Text = "";
                        lines[i1, lines_PROP_gph_selected] = "1";

                        richTextBox_console2.Text += "P=" + lines[i1, lines_PROP_P];
                        if (lines[i1, lines_PROP_P1] != "")
                        {
                            dbl_tmp = double.Parse(lines[i1, lines_PROP_P1]);
                            richTextBox_console2.Text += " [" + dbl_tmp.ToString("###.0");
                        }
                        else richTextBox_console2.Text += " [";
                        if (lines[i1, lines_PROP_P2] != "")
                        {
                            dbl_tmp = double.Parse(lines[i1, lines_PROP_P2]);
                            richTextBox_console2.Text += ", " + dbl_tmp.ToString("###.0");
                        }
                        else richTextBox_console2.Text += ", ";
                        if (lines[i1, lines_PROP_P3] != "")
                        {
                            dbl_tmp = double.Parse(lines[i1, lines_PROP_P3]);
                            richTextBox_console2.Text += ", " + dbl_tmp.ToString("###.0") + "]";
                        }
                        else richTextBox_console2.Text += ", ";

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
                            if(lines[i1, lines_PROP_X1] != "") { 
                                val1 = double.Parse(lines[i1, lines_PROP_X1]);
                                richTextBox_console2.Text += "\nlength= " + val1.ToString("##0.000") + " km";
                            }
                        }

                        richTextBox_console2.Text += "\n--------------------------------------------";
                        richTextBox_console2.Text += "\nline=#" + i1.ToString() + "\tname=" + lines[i1, lines_PROP_name]
                            + "\nBus1=" + lines[i1, lines_PROP_bus1] + "\tBus2=" + lines[i1, lines_PROP_bus2];
                        richTextBox_console2.Text += "\n--------------------------------------------";

                        richTextBox_console2.Text += "\nbrk1=" + lines[i1, lines_PROP_brk1]
                            + "\nbrk2=" + lines[i1, lines_PROP_brk2];
                        richTextBox_console2.Text += "\nx0=" + lines[i1, lines_PROP_x0] + "\ty0=" + lines[i1, lines_PROP_y0];
                        richTextBox_console2.Text += "\npin1x=" + lines[i1, lines_PROP_pin1_x] + "\tpin1y=" + lines[i1, lines_PROP_pin1_y];
                        richTextBox_console2.Text += "\npin2x=" + lines[i1, lines_PROP_pin2_x] + "\tpin2y=" + lines[i1, lines_PROP_pin2_y];
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
                            catch { }
                        }
                        for (int h1 = LPs_scenarios_multi_LF_start + 2; h1 < LPs_scenarios_multi_LF_start + 24 + 2; h1++)
                        {
                            // salvare in canalul grafic 1
                            if(lines_values_set[i1, lines_PROP_P, h1] != "")
                            SimpleGph_channels1[channel_free1, h1 - LPs_scenarios_multi_LF_start - 1] = double.Parse(lines_values_set[i1, lines_PROP_P, h1]);
                            // salvare in canalul grafic 2
                            if (lines_values_set[i1, lines_PROP_U1, h1] != "")
                            SimpleGph_channels2[channel_free2, h1 - LPs_scenarios_multi_LF_start - 1] = double.Parse(lines_values_set[i1, lines_PROP_U1, h1]);
                        }
                        channel_name[channel_free1] = lines[i1, lines_PROP_name];
                        channel_free1++; if (channel_free1 >= SimpleGph_channels_MAX) channel_free1 = 0;
                        channel_free2++; if (channel_free2 >= SimpleGph_channels_MAX) channel_free2 = 0;
                    }
                    else lines[i1, lines_PROP_gph_selected] = "0";

                    if (selected_line != -1) richTextBox_console_cd.Text = "object=line\n" + "name=" + lines[selected_line, lines_PROP_name] + "\n"
                         + "attrib=" + "\n" + "value=";
                }
            }

        }

        private void Gph_element_identification_generators(int xm, int ym)
        {
            int x1, x2, y1, y2, inside;
            for (int i1 = 0; i1 < generators_no; i1++) // scanare obiecte de tip "generators"
            {
                if ((generators[i1, generators_PROP_x0] != "") && (generators[i1, generators_PROP_y0] != ""))
                {
                    x1 = int.Parse(generators[i1, generators_PROP_x0]);
                    y1 = int.Parse(generators[i1, generators_PROP_y0]);
                    inside = inside_rect(x1, y1, x1 + object_dx, y1 + line_dy, xm, ym);
                    if (inside == 1)
                    {
                        generators[i1, generators_PROP_gph_selected] = "1";
                        richTextBox_console2.Text = "generator=#" + i1.ToString() + "\nname=" + generators[i1, generators_PROP_name]
                            + "\nBus=" + generators[i1, generators_PROP_bus] + "\nPn=" + generators[i1, generators_PROP_Pn]
                            + "\nPn= " + generators[i1, generators_PROP_Pn] + "\nQn= " + generators[i1, generators_PROP_Qn]
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
                                SimpleGph_channels1[channel_free1, h1 - LPs_scenarios_multi_LF_start - 1] = double.Parse(generators_values_set[i1, generators_PROP_P, h1]);
                            // salvare in canalul grafic 2
                            if (generators_values_set[i1, generators_PROP_U1, h1] != "")
                                SimpleGph_channels2[channel_free2, h1 - LPs_scenarios_multi_LF_start - 1] = double.Parse(generators_values_set[i1, generators_PROP_U1, h1]);
                        }
                        channel_name[channel_free1] = generators[i1, generators_PROP_name];
                        channel_free1++; if (channel_free1 >= SimpleGph_channels_MAX) channel_free1 = 0;
                        channel_free2++; if (channel_free2 >= SimpleGph_channels_MAX) channel_free2 = 0;
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
                    inside = inside_rect(x1, y1, x1 + 20, y1 + 11, xm, ym);
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

        private void Gph_element_identification_measurements(int xm, int ym)
        {
            int x1, x2, y1, y2, inside;

            for (int i1 = 0; i1 < measurements_no; i1++) // scanare obiecte de tip "interracts"
            {
                if ((measurements[i1, interracts_PROP_x0] != "") && (measurements[i1, interracts_PROP_y0] != ""))
                {
                    string s1 = "";
                    x1 = int.Parse(measurements[i1, measurements_PROP_x0]);
                    y1 = int.Parse(measurements[i1, measurements_PROP_y0]);
                    inside = inside_rect(x1, y1, x1 + 20, y1 + 95, xm, ym);
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

        private void Graph_phasors_identification(int xm, int ym)
        {
            int x1, x2, y1, y2, inside, inside_enrage;

            for (int i1 = 0; i1 < graph_phasors_no; i1++) // scanare obiecte de tip "interracts"
            {
                if ((graph_phasors[i1, graph_phasors_PROP_x0] != "") && (graph_phasors[i1, graph_phasors_PROP_y0] != ""))
                {
                    string s1 = "";
                    x1 = int.Parse(graph_phasors[i1, graph_phasors_PROP_x0]);
                    y1 = int.Parse(graph_phasors[i1, graph_phasors_PROP_y0]);
                    inside = inside_rect(x1, y1, x1 + gph_phasors_legend_dx, y1 + gph_phasors_legend_dx, xm, ym);
                    inside_enrage = inside_rect(x1+50, y1, x1 + 110, y1 + 20, xm, ym);
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

        private void Gph_element_identification_nodes(int xm, int ym)
        {
            int x1, x2, y1, y2, inside;

            for (int i1 = 0; i1 < nodes_no; i1++) // scanare obiecte de tip "nodes"
            {
                if ((nodes[i1, nodes_PROP_x0] != "") && (nodes[i1, nodes_PROP_y0] != ""))
                {
                    x1 = int.Parse(nodes[i1, nodes_PROP_x0]);
                    y1 = int.Parse(nodes[i1, nodes_PROP_y0]);
                    inside = inside_rect(x1, y1, x1 + object_dx, y1 + 12, xm, ym);
                    if (inside == 1)
                    {
                        nodes[i1, nodes_PROP_gph_selected] = "1";
                        richTextBox_console2.Text = "node=#" + i1.ToString() + "\nname=" + nodes[i1, nodes_PROP_name]
                             + "\nbus_name=" + nodes[i1, nodes_PROP_bus_name]
                            + "\nBus=" + nodes[i1, nodes_PROP_bus] + "\nU_source=" 
                            + nodes[i1, nodes_PROP_U_source_object] 
                            + "\nU_source_no=" + nodes[i1, nodes_PROP_U_source_object_number]
                            +"\nU_source_name=" + nodes[i1, nodes_PROP_U_source_object_name];
                        richTextBox_console2.Text += "\nU1=" + nodes[i1, nodes_PROP_U1];
                        richTextBox_console2.Text += "\nU2=" + nodes[i1, nodes_PROP_U2];
                        richTextBox_console2.Text += "\nU3=" + nodes[i1, nodes_PROP_U3];
                        richTextBox_console2.Text += "\nConnected obj=" + nodes[i1, nodes_PROP_number_of_connected_objects];
                        richTextBox_console2.Text += "\nConnect=" + nodes[i1, nodes_PROP_list_of_connected_objects];
                        richTextBox_console2.Text += "\nPolylines=" + nodes[i1, nodes_PROP_plylines];

                        richTextBox_console_cd.Text = "object=node\n" + "name=" + nodes[i1, nodes_PROP_bus] + "\n"
                            + "attrib=" + "Polylines\n" + "value=";
                    }
                    else nodes[i1, nodes_PROP_gph_selected] = "0";
                }
            }

        }

        private void Gph_element_identification_labels(int xm, int ym)
        {
            int x1, x2, y1, y2, inside;

            for (int i1 = 0; i1 < labels_no; i1++) // scanare obiecte de tip "label"
            {
                if ((labels[i1, labels_PROP_x0] != "") && (labels[i1, labels_PROP_y0] != ""))
                {
                    x1 = int.Parse(labels[i1, labels_PROP_x0]);
                    y1 = int.Parse(labels[i1, labels_PROP_y0]);
                    inside = inside_rect(x1, y1, x1 + object_dx, y1 + 12, xm, ym);
                    if (inside == 1)
                    {
                        labels[i1, labels_PROP_gph_selected] = "1";
                        richTextBox_console2.Text = "label=#" + i1.ToString() + "\nname=" + labels[i1, labels_PROP_name]
                            + "\naction=" + labels[i1, labels_PROP_action];// + "\nU_source=" + nodes[i1, nodes_PROP_U_source_object];

                        if (labels[i1, labels_PROP_action] == "Grid_sumary")
                        {
                            calculate_grid_data1_string = "";
                            calculate_grid_data1();
                            richTextBox_console2.Text += calculate_grid_data1_string;
                        }
                    }
                    else labels[i1, labels_PROP_gph_selected] = "0";
                }
            }

        }

        private void Gph_element_identification_graph_pies(int xm, int ym)
        {
            int x1, x2, y1, y2, inside;
            int graph_pies_dx = 120;
            int graph_pies_dy = 60;

            for (int i1 = 0; i1 < graph_pies_no; i1++) // scanare obiecte de tip "label"
            {
                if ((graph_pies[i1, graph_pies_PROP_x0] != "") && (graph_pies[i1, graph_pies_PROP_y0] != ""))
                {
                    x1 = int.Parse(graph_pies[i1, graph_pies_PROP_x0]);
                    y1 = int.Parse(graph_pies[i1, graph_pies_PROP_y0]);
                    inside = inside_rect(x1, y1, x1 + graph_pies_dx, y1 + graph_pies_dy, xm, ym);
                    if (inside == 1)
                    {
                        graph_pies[i1, graph_pies_PROP_gph_selected] = "1";
                        richTextBox_console2.Text = "graph_pies=#" + i1.ToString() + "\nname=" + graph_pies[i1, graph_pies_PROP_name]
                            + "\ncommand=" + graph_pies[i1, graph_pies_PROP_command]
                            + "\nobj=" + graph_pies[i1, graph_pies_PROP_obj]
                            + "\nnumber=" + graph_pies[i1, graph_pies_PROP_number]
                            + "\nx0=" + graph_pies[i1, graph_pies_PROP_x0]
                            + "\ny0=" + graph_pies[i1, graph_pies_PROP_y0]
                            ;
                    }
                    else graph_pies[i1, graph_pies_PROP_gph_selected] = "0";
                }
            }

        }

        private void Gph_element_identification_smart_meters(int xm, int ym)
        {
            int x1, x2, y1, y2, inside;
            int smart_meters_dx = 200;
            int smart_meters_dy = 200;

            for (int i1 = 0; i1 < smart_meters_no; i1++) // scanare obiecte de tip "label"
            {
                if ((smart_meters[i1, smart_meters_PROP_x0] != "") && (smart_meters[i1, smart_meters_PROP_y0] != ""))
                {
                    x1 = int.Parse(smart_meters[i1, smart_meters_PROP_x0]);
                    y1 = int.Parse(smart_meters[i1, smart_meters_PROP_y0]);
                    inside = inside_rect(x1, y1, x1 + smart_meters_dx, y1 + smart_meters_dy, xm, ym);
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

        private void Gph_element_identification(int xm, int ym)
        {
            // In urma clicarii pe un obiect, atributele acestuia sunt afisate in consola 2


            Gph_element_identification_trafos(xm, ym);

            Gph_element_identification_loads(xm, ym);

            Gph_element_identification_lines(xm, ym);

            Gph_element_identification_generators(xm, ym);

            Gph_element_identification_measurements(xm, ym);

            Graph_phasors_identification(xm, ym);

            Gph_element_identification_nodes(xm, ym);

            Gph_element_identification_graph_pies(xm, ym);

            Gph_element_identification_smart_meters(xm, ym);

            Gph_element_identification_labels(xm, ym);

            Scan_SimpleGph(xm, ym);

            Scan_console_Training(xm, ym);
        }

        double P_bilant_grid_before = 0, lossess_proc_before = 0;
        string calculate_grid_data1_string = "";

        private void calculate_grid_data1()
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

    }
}