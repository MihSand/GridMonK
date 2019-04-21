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
        string[] Congestions = new string[historical_values_depth_MAX];
        // Dispatch Operations during a timeframe
        string[] Dispatch_operations = new string[historical_values_depth_MAX];
        string[] Congestions_old = new string[historical_values_depth_MAX];
        string Congestions_temp = "";
        string typeof_24hours_set = "";

        static int console_Training_X0 = 180;
        static int console_Training_Y0 = 670;
        static int console_Training_delta_X0 = 300;
        static int console_Training_delta_Y0 = 120;
        // Clipping the graphics of scheme zone with this rectangle
        Point[] polyPoints_clip_console_Training_zone = {
                new Point(console_Training_X0, console_Training_Y0-8),
                new Point(console_Training_X0+console_Training_delta_X0, console_Training_Y0-8),
                new Point(console_Training_X0+console_Training_delta_X0, console_Training_Y0+console_Training_delta_Y0+5),
                new Point(console_Training_X0, console_Training_Y0+console_Training_delta_Y0+5)};
        string timeframe_crt_Text = "Work Space";

        string voltage_violations_on_each_phase = "false";
        string current_violations_on_each_phase = "false";

        // calculates nodes violations in the current grid view (RBM)
        void assess_nodes_violations()
        {
            //  
            double U_nom = 230;
            double U_zero_max = 5;
            double U1=0, U2=0, U3 = 0;
            for (int n1 = 0; n1 < nodes_no; n1++)
            {
                try
                {
                    U1 = double.Parse(nodes[n1, nodes_PROP_U1]);
                    U2 = double.Parse(nodes[n1, nodes_PROP_U2]);
                    U3 = double.Parse(nodes[n1, nodes_PROP_U2]);

                    // find automatically the voltage level, to be between Un-70% and Un+70%
                    if ((U1 < 230.94  * 1.7) && (U1 > 230.94 / 1.7)) { U_nom = 230.94; U_zero_max = 5; }   // 400 V
                    if ((U1 < 11547.0 * 1.7) && (U1 > 11547.0 / 1.7)) { U_nom = 11547; U_zero_max = 100; } // 20 kV / sqrt(3)
                    if ((U1 < 230940 * 1.7) && (U1 > 230940 / 1.7)) { U_nom = 230940; U_zero_max = 1000; } // 400 kV

                    // Check the boundaries Un+10%
                    if (U1 > U_nom * 1.1)
                        Congestions_temp += "U1[Node=" + nodes[n1, nodes_PROP_bus] + "]=" + nodes[n1, nodes_PROP_U1] +"V" + " > Un+10% (OVER-Voltage)\n";
                    if(voltage_violations_on_each_phase == "true") { 
                        if (U2 > U_nom * 1.1)
                            Congestions_temp += "U2[Node=" + nodes[n1, nodes_PROP_bus] + "]=" + nodes[n1, nodes_PROP_U2] + "V" + " > Un+10% (OVER-Voltage)\n";
                        if (U3 > U_nom * 1.1)
                            Congestions_temp += "U3[Node=" + nodes[n1, nodes_PROP_bus] + "]=" + nodes[n1, nodes_PROP_U3] + "V" + " > Un+10% (OVER-Voltage)\n";
                    }

                    // Check the boundaries Un-10%
                    if ((U1 < U_nom * 0.9) && (U1 > U_zero_max))
                        Congestions_temp += "U1[Node=" + nodes[n1, nodes_PROP_bus] + "] =" + nodes[n1, nodes_PROP_U1] + "V" + " < 230V-10% (UNDER-Voltage)\n";
                    if (voltage_violations_on_each_phase == "true")
                    {
                        if ((U2 < U_nom * 0.9) && (U2 > U_zero_max))
                            Congestions_temp += "U2[Node=" + nodes[n1, nodes_PROP_bus] + "] =" + nodes[n1, nodes_PROP_U2] + "V" + " < 230V-10% (UNDER-Voltage)\n";
                        if ((U3 < U_nom * 0.9) && (U3 > U_zero_max))
                            Congestions_temp += "U3[Node=" + nodes[n1, nodes_PROP_bus] + "] =" + nodes[n1, nodes_PROP_U3] + "V" + " < 230V-10% (UNDER-Voltage)\n";
                    }
                } catch { }
            }
        }

        // calculates lines violations in the current grid view (RBM)
        void assess_lines_violations()
        {
            double I1, I2, I3;
            for (int l1 = 0; l1 < lines_no; l1++)
            {
                try {
                    I1 = double.Parse(lines[l1, lines_PROP_I1]);
                    I2 = double.Parse(lines[l1, lines_PROP_I2]);
                    I3 = double.Parse(lines[l1, lines_PROP_I3]);
                    if (I1 > double.Parse(lines[l1, lines_PROP_Imax]))
                        Congestions_temp += "I1[Line=" + lines[l1, lines_PROP_name] + "] =" + lines[l1, lines_PROP_I1] + "A" + " > Imax =" + lines[l1, lines_PROP_Imax] + "A (OVER-Current)\n";
                    if (current_violations_on_each_phase == "true")
                    {
                        if (I2 > double.Parse(lines[l1, lines_PROP_Imax]))
                            Congestions_temp += "I2[Line=" + lines[l1, lines_PROP_name] + "] =" + lines[l1, lines_PROP_I2] + "A" + " > Imax =" + lines[l1, lines_PROP_Imax] + "A (OVER-Current)\n";
                        if (I3 > double.Parse(lines[l1, lines_PROP_Imax]))
                            Congestions_temp += "I3[Line=" + lines[l1, lines_PROP_name] + "] =" + lines[l1, lines_PROP_I3] + "A" + " > Imax =" + lines[l1, lines_PROP_Imax] + "A (OVER-Current)\n";
                    }
                }
                catch { }
            }
        }

        // Copy in RBM the grid values from a certain timeframe
        private void Copy_Training_Set()
        {
            // copy values in the current Workspace
            for (int i1 = 0; i1 < loads_no; i1++) for (int j1 = 0; j1 < loads_prop_MAX; j1++)
                    if (loads_values_set[i1, j1, Timeframe_crt + 11] != "") loads[i1, j1] = loads_values_set[i1, j1, Timeframe_crt + 11];
            for (int i1 = 0; i1 < lines_no; i1++) for (int j1 = 0; j1 < lines_prop_MAX; j1++)
                    if (lines_values_set[i1, j1, Timeframe_crt + 11] != "") lines[i1, j1] = lines_values_set[i1, j1, Timeframe_crt + 11];
            for (int i1 = 0; i1 < trafos_no; i1++) for (int j1 = 0; j1 < trafos_prop_MAX; j1++)
                    if (trafos_values_set[i1, j1, Timeframe_crt + 11] != "") trafos[i1, j1] = trafos_values_set[i1, j1, Timeframe_crt + 11];
            for (int i1 = 0; i1 < generators_no; i1++) for (int j1 = 0; j1 < generators_prop_MAX; j1++)
                    if (generators_values_set[i1, j1, Timeframe_crt + 11] != "") generators[i1, j1] = generators_values_set[i1, j1, Timeframe_crt + 11];
            /*
            for (int i1 = 0; i1 < linecodes_MAX; i1++) for (int j1 = 0; j1 < linecodes_prop_MAX; j1++) linecodes[i1, j1] = "";
            for (int i1 = 0; i1 < loadshapes_MAX; i1++) for (int j1 = 0; j1 < loadshapes_prop_MAX; j1++) loadshapes[i1, j1] = "";
            for (int i1 = 0; i1 < monitors_MAX; i1++) for (int j1 = 0; j1 < monitors_prop_MAX; j1++) monitors[i1, j1] = "";
            for (int i1 = 0; i1 < exports_MAX; i1++) for (int j1 = 0; j1 < exports_prop_MAX; j1++) exports[i1, j1] = "";
            for (int i1 = 0; i1 < nodes_MAX; i1++) for (int j1 = 0; j1 < nodes_prop_MAX; j1++) nodes[i1, j1] = "";
            for (int i1 = 0; i1 < nodes_metadata_MAX; i1++) for (int j1 = 0; j1 < nodes_metadata_prop_MAX; j1++) nodes_metadata[i1, j1] = "";
            for (int i1 = 0; i1 < labels_MAX; i1++) for (int j1 = 0; j1 < labels_prop_MAX; j1++) labels[i1, j1] = "";
            for (int i1 = 0; i1 < interracts_MAX; i1++) for (int j1 = 0; j1 < interracts_prop_MAX; j1++) interracts[i1, j1] = "";
            for (int i1 = 0; i1 < measurements_MAX; i1++) for (int j1 = 0; j1 < measurements_prop_MAX; j1++) measurements[i1, j1] = "";
            for (int i1 = 0; i1 < graph_phasors_MAX; i1++) for (int j1 = 0; j1 < graph_phasors_prop_MAX; j1++) graph_phasors[i1, j1] = "";
            for (int i1 = 0; i1 < polylines1_MAX; i1++) for (int j1 = 0; j1 < polylines1_prop_MAX; j1++) polylines1[i1, j1] = "";
            for (int i1 = 0; i1 < polylines2node_MAX; i1++) for (int j1 = 0; j1 < polylines2node_prop_MAX; j1++) polylines2node[i1, j1] = "";
            */
            //gph_phasors_ini();
            //for (int i1 = 0; i1 < lines_MAX; i1++) for (int j1 = 0; j1 < lines_values_set_MAX; j1++)
            //        for (int k1 = 0; k1 < lines_values_depth_MAX; k1++) lines_values_set[i1, j1, k1] = "";
            calculate_values_from_results();
            assess_nodes_properties(); // se realoca tensiuni la noduri etc.

        }

        private void Console_Training_forecast_Next()
        {
            // move to next training timeframe
            Timeframe_crt++;
            if (Timeframe_crt > 23) Timeframe_crt = 23;
            typeof_24hours_set = "forecast";
        }
        private void Console_Training_forecast_corrected_Next()
        {
            // move to next training timeframe
            Timeframe_crt++;
            if (Timeframe_crt > 23) Timeframe_crt = 23;
            typeof_24hours_set = "forecast_corrected";
        }

        private void Console_Training_forecast_Prev()
        {
            // move to previous training timeframe
            Timeframe_crt--;
            if (Timeframe_crt <= -1) Timeframe_crt = -1;
            typeof_24hours_set = "forecast";
        }
        private void Console_Training_forecast_corrected_Prev()
        {
            // move to previous training timeframe
            Timeframe_crt--;
            if (Timeframe_crt <= -1) Timeframe_crt = -1;
            typeof_24hours_set = "forecast_corrected";
        }

        private void Console_Training_Cmd_Calc()
        {
            // in this routine the OpenDSS LF is requested to provide only one LF
            DateTime t1 = DateTime.Now;
            string st1 = ">> Grid compute\n";
            int t1s = t1.Second, t1ms = t1.Millisecond;
            st1 += "T(ini):" + t1.Year.ToString() + "." + t1.Month.ToString("00") + "." + t1.Day.ToString("00")
                + " " + t1.Hour.ToString("00") + ":" + t1.Minute.ToString("00") + ":" + t1.Second.ToString("00")
                 + "." + t1.Millisecond.ToString("000") + "\n";
            string richTextBox_console_answers_str = st1;

            Generate_Load_PQ_circular_scan_file(1.0);

            generate_output_dss("multi_LP_Scan_1440", "U_stability");  // producere fisier de iesire compatibil dss.

            OpenDSS_invoke("multi_LP_Scan_1440"); // se lanseaza OpenDSS

            return;

            richTextBox_console_answers_str += ">> Generate output (";
            generate_output_dss("multi_LP_RMB_and_24h", "One_LP"); // se salveaza statusul curent al retelei

            t1 = DateTime.Now;
            int t2s = t1.Second, t2ms = t1.Millisecond; // dupa generate_output_dss()
            int dt2_msec = t2s * 1000 + t2ms - t1s * 1000 - t1ms;
            if (dt2_msec < 0) dt2_msec = dt2_msec + 60000;
            richTextBox_console_answers_str += dt2_msec.ToString() + " ms)\n";

            richTextBox_console_answers_str += ">> Invoke OpenDSS (";
            OpenDSS_invoke("multi_LP_RMB_and_24h"); // se lanseaza OpenDSS
            t1 = DateTime.Now;
            int t3s = t1.Second, t3ms = t1.Millisecond;
            int dt3_msec = t3s * 1000 + t3ms - t2s * 1000 - t2ms;
            if (dt3_msec < 0) dt3_msec = dt3_msec + 60000;
            richTextBox_console_answers_str += dt3_msec.ToString() + " ms)\n";

            richTextBox_console_answers_str += ">> Process results (";
            read_OpenDSS_results("multi_LP_RMB_and_24h"); // se citesc rezultatele din fiserele de iesire ale OpenDSS
            t1 = DateTime.Now;
            int t4s = t1.Second, t4ms = t1.Millisecond;
            int dt4_msec = t4s * 1000 + t4ms - t3s * 1000 - t3ms;
            if (dt4_msec < 0) dt4_msec = dt4_msec + 60000;
            richTextBox_console_answers_str += dt4_msec.ToString() + " ms)\n";

            Grid_is_calculated = 1; // some further functions can work only if the grid is calculated (Compute is requested)

            assess_nodes_properties(); // in urma valorilor rezultat, asociate la obiecte, se fac calcule legate de noduri
            add_nodes_properties_from_paramaterisation_nodes_metadata(); // proprietati de noduri din parametrzarea initiala; nu se cheama aceasta component ataunci cand se parcurg cele 24 ore

            calculate_values_from_results(); // in urma valorilor rezultat, asociate la obiecte, se fac calcule legate de noduri
            Timeframe_crt = -1;

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

            // reseteaza continutul graficelor
            for (int i1 = 0; i1 < SimpleGph_channels_MAX; i1++)
                for (int j1 = 0; j1 < SimpleGph_channels_depth_MAX; j1++)
                {
                    SimpleGph_channels1[i1, j1] = 0;
                    SimpleGph_channels2[i1, j1] = 0;
                }
            channel_free1 = 0;
            channel_free2 = 0;
            for (int i1 = 0; i1 < SimpleGph_channels_MAX; i1++) channel_name[i1] = "";

            Refresh(); // se redeseneaza GUI

        }

        // Scan the buttons of the Training console
        private void Scan_console_Training(int xm, int ym)
        {
            int x1=0, y1 = 0, inside=0;
            // Prepare coordinates for command "Right"
            x1 = console_Training_X0 + 2;
            y1 = console_Training_Y0;
            inside = inside_rect(x1, y1, x1 + 35, y1 + 30, xm, ym);
            if (inside == 1) Console_Training_forecast_Next(); // Execute command "Right"

            // Prepare coordinates for command "Left"
            //g.DrawImage(Image_left, console_Training_X0 + 32 + 192 + 37, console_Training_Y0 + 10, 35, 20); // 169 x 148, raport 1.142
            x1 = console_Training_X0 + 32 + 192 + 37;
            y1 = console_Training_Y0 +10;
            inside = inside_rect(x1, y1, x1 + 35, y1 + 20, xm, ym);
            if (inside == 1) Console_Training_forecast_Prev(); // Execute command "Left"

            // Prepare coordinates for command "Right" forecast corrected
            x1 = console_Training_X0 + 2;
            y1 = console_Training_Y0 + 100;
            inside = inside_rect(x1, y1, x1 + 35, y1 + 20, xm, ym);
            if (inside == 1) Console_Training_forecast_corrected_Next(); // Execute command "Right"
            // Prepare coordinates for command "Left" forecast corrected
            //g.DrawImage(Image_left, console_Training_X0 + 32 + 192 + 37, console_Training_Y0 + 100, 35, 20); // 169 x 148, raport 1.142
            x1 = console_Training_X0 + 32+192+37;
            y1 = console_Training_Y0 + 100;
            inside = inside_rect(x1, y1, x1 + 35, y1 + 20, xm, ym);
            if (inside == 1) Console_Training_forecast_corrected_Prev(); // Execute command "Right"

            // Cmd+Calc
            //e.Graphics.DrawImage(Image_Cmd_Calc, console_Training_X0 + 190, console_Training_Y0 + 40, 80, 25); // 169 x 148, raport 1.142
            x1 = console_Training_X0 + 190;
            y1 = console_Training_Y0 + 40;
            inside = inside_rect(x1, y1, x1 + 80, y1 + 25, xm, ym);
            if (inside == 1) Console_Training_Cmd_Calc(); // Execute command "Cmd_Calc"

            Copy_Training_Set();
        }

        Image Image_right, Image_left, Image_down, Image_Work_space, Image_regim_baza, Image_Input_grid, Image_Cmd_Calc;
        Image Image_Save_solved, Image_Back, Image_Undo, Image_Redo, Image_Config;

        private void Console_Training_Ini() { 
            Image_right = Image.FromFile(OpenDSS_Path + @"/" + @"arrow_right1.jpg"); // 222 x 154, 58 x 40
            Image_left = Image.FromFile(OpenDSS_Path + @"/" + @"arrow_left1b.jpg"); // 222 x 154, 58 x 40
            Image_down = Image.FromFile(OpenDSS_Path + @"/" + @"arrow_down.jpg"); // 222 x 154, 58 x 40
            Image_Work_space = Image.FromFile(OpenDSS_Path + @"/" + @"work_space1.jpg"); // 222 x 154, 58 x 40
            Image_regim_baza = Image.FromFile(OpenDSS_Path + @"/" + @"regim_baza1.jpg"); // 222 x 154, 58 x 40
            Image_Input_grid = Image.FromFile(OpenDSS_Path + @"/" + @"input_grid1.jpg"); // 222 x 154, 58 x 40
            Image_Cmd_Calc = Image.FromFile(OpenDSS_Path + @"/" + @"cmd+calc2.jpg"); // 222 x 154, 58 x 40
            Image_Save_solved = Image.FromFile(OpenDSS_Path + @"/" + @"Save_solved1.jpg"); // 222 x 154, 58 x 40
            Image_Back = Image.FromFile(OpenDSS_Path + @"/" + @"back1.jpg"); // 222 x 154, 58 x 40
            Image_Undo = Image.FromFile(OpenDSS_Path + @"/" + @"undo1.jpg"); // 222 x 154, 58 x 40
            Image_Redo = Image.FromFile(OpenDSS_Path + @"/" + @"redo1.jpg"); // 222 x 154, 58 x 40
            Image_Config = Image.FromFile(OpenDSS_Path + @"/" + @"config2.jpg"); // 222 x 154, 58 x 40
        }

        private void Paint_console_Training(object sender, PaintEventArgs e)
        {
            string s1 = "", s2 = "";


            Graphics g = e.Graphics;
            // Clipping the plygones start lines
            GraphicsPath path_clip1 = new GraphicsPath();
            path_clip1.AddPolygon(polyPoints_clip_console_Training_zone);
            Region region1 = new Region(path_clip1);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region1, CombineMode.Replace);

            int t2 = Timeframe_crt + 1;
            //Timeframe_crt_str = Timeframe_crt.ToString();
            timeframe_crt_Text = "H[ " + Timeframe_crt.ToString("00") + ":00-" + t2.ToString("00") + ":00 ]";
            //if(Timeframe_crt != -1) g.DrawString(timeframe_crt_Text, Font2, b0Black, console_Training_X0 + 70, console_Training_Y0 - 7);

            Congestions_temp = "Violations ";
            if(Grid_is_calculated==1) if (Timeframe_crt != -1)
            {
                Congestions_temp += " for Timeframe: "+ timeframe_crt_Text + "\n";
                assess_nodes_violations();
                assess_lines_violations();
                Congestions[Timeframe_crt] = Congestions_temp;
            }

            if (GUI_Language == "Romanian") s1 = "Consola";
            if (GUI_Language == "English") s1 = "Training";
            if (GUI_Language == "Romanian") s2 = "Comenzi"; // Manevre
            if (GUI_Language == "English") s2 = "Console";
            g.DrawString(s1, Font11b, b0Black, console_Training_X0 + 1, console_Training_Y0 +35);
            g.DrawString(s2, Font11b, b0Black, console_Training_X0 + 1, console_Training_Y0 + 47);
            // Config
            g.DrawImage(Image_Config, console_Training_X0 + 12, console_Training_Y0 + 65, 30, 30); // 169 x 148, raport 1.142
            // Right+Left forecast
            g.DrawImage(Image_right, console_Training_X0 + 2, console_Training_Y0+10, 30, 20); // 169 x 148, raport 1.142
            g.DrawImage(Image_left, console_Training_X0 + 32+192+37, console_Training_Y0+10, 30, 20); // 169 x 148, raport 1.142
            // Right+Left forecast corrected
            g.DrawImage(Image_right, console_Training_X0 + 2, console_Training_Y0 + 100, 30, 20); // 169 x 148, raport 1.142
            g.DrawImage(Image_left, console_Training_X0 + 32 + 192 + 37, console_Training_Y0 + 100, 30, 20); // 169 x 148, raport 1.142

            // 24 hours forecast / prognosis
            if (GUI_Language == "Romanian") s1 = "Prognoza 24h : RMB"; // regim mediu de baza
            if (GUI_Language == "English") s1 = "24h Forecast : BAF"; // Basic average flow
            if (Timeframe_crt == -1) g.DrawString(s1, Font1, b0Black, console_Training_X0 + 65, console_Training_Y0 - 4);
            if (GUI_Language == "Romanian") s1 = "Prognoza 24h : "; // regim mediu de baza
            if (GUI_Language == "English") s1 = "24h Forecast : "; // Basic average flow
            if (Timeframe_crt != -1) g.DrawString(s1 + "#" + Timeframe_crt.ToString() +"=" + timeframe_crt_Text, Font1, b0Black, console_Training_X0 + 65, console_Training_Y0 - 4);
            e.Graphics.FillRectangle(b13Yellow, console_Training_X0 + 68, console_Training_Y0 + 15, 24 * 8, 10); // 24 * 8 = 192
            for (int i1 = 0; i1 < 24; i1++) e.Graphics.DrawLine(p3DarkGray, console_Training_X0 + 68 + (i1) * 8, console_Training_Y0 + 15,
                console_Training_X0 + 68 + (i1) * 8, console_Training_Y0 + 15 + 12);
            for (int i1 = 0; i1 < 7; i1++) e.Graphics.DrawLine(p1Black, console_Training_X0 + 68 + (i1*4) * 8, console_Training_Y0 + 13,
                console_Training_X0 + 68 + (i1*4) * 8, console_Training_Y0 + 13 + 20);
            if(typeof_24hours_set == "forecast") {
                if (Timeframe_crt != -1) e.Graphics.FillRectangle(b2Blue, console_Training_X0 + 68 + (Timeframe_crt)*8, console_Training_Y0 + 13, 8, 16);
                if (Timeframe_crt == -1)
                {
                    e.Graphics.FillRectangle(b2Blue, console_Training_X0 + 42 + (Timeframe_crt) * 8, console_Training_Y0 + 10, 30, 25);
                    g.DrawString("RMB", Font1bold, b1White, console_Training_X0 + 42 + (Timeframe_crt) * 8, console_Training_Y0 + 15);
                }
            }

            // 24 hours forecast corrected
            if (GUI_Language == "Romanian") s1 = "Prognoza 24h corectata: RMB"; // regim mediu de baza
            if (GUI_Language == "English") s1 = "24h Forecast corrected: BAF"; // Basic average flow
            if (Timeframe_crt == -1) g.DrawString(s1, Font1, b0Black, console_Training_X0 + 55, console_Training_Y0 + 95);
            if (GUI_Language == "Romanian") s1 = "Prognoza 24h corectata: "; // regim mediu de baza
            if (GUI_Language == "English") s1 = "24h Forecast corrected: "; // Basic average flow
            if (Timeframe_crt != -1) g.DrawString(s1 + "#"+Timeframe_crt.ToString() + "=" + timeframe_crt_Text, Font1, b0Black, console_Training_X0 + 35, console_Training_Y0 + 95);
            e.Graphics.FillRectangle(b7LightGreen, console_Training_X0 + 68, console_Training_Y0 + 110, 24 * 8, 10); //b7LightGreen
            for (int i1 = 0; i1 < 24; i1++) e.Graphics.DrawLine(p3DarkGray, console_Training_X0 + 68 + (i1) * 8, console_Training_Y0 + 110,
                console_Training_X0 + 68 + (i1) * 8, console_Training_Y0 + 110 + 12);
            for (int i1 = 0; i1 < 7; i1++) e.Graphics.DrawLine(p1Black, console_Training_X0 + 68 + (i1 * 4) * 8, console_Training_Y0 + 110,
                console_Training_X0 + 68 + (i1 * 4) * 8, console_Training_Y0 + 110 + 20);
            if (typeof_24hours_set == "forecast_corrected") {
                if (Timeframe_crt != -1) e.Graphics.FillRectangle(b11Orange, console_Training_X0 + 68 + (Timeframe_crt) * 8, console_Training_Y0 + 110, 8, 16);
                if (Timeframe_crt == -1)
                {
                    e.Graphics.FillRectangle(b11Orange, console_Training_X0 + 42 + (Timeframe_crt) * 8, console_Training_Y0 + 107, 30, 25);
                    g.DrawString("RMB", Font1bold, b0Black, console_Training_X0 + 42 + (Timeframe_crt) * 8, console_Training_Y0 + 110);
                }
            }

            //e.Graphics.DrawImage(Image_down, console_Training_X0 + 120, console_Training_Y0 + 32, 35, 24); // 169 x 148, raport 1.142
            // Work space
            if (typeof_24hours_set == "forecast") px = p4Blue3;
            if (typeof_24hours_set == "forecast_corrected") px = p7Green3;
            e.Graphics.FillRectangle(b7LightGreen, console_Training_X0 + 103, console_Training_Y0 + 45, 110, 25); // 169 x 148, raport 1.142
            e.Graphics.DrawRectangle(px, console_Training_X0 + 103, console_Training_Y0 + 45, 110, 25); // 169 x 148, raport 1.142
            if ((typeof_24hours_set == "forecast") && (Timeframe_crt == -1))
                g.DrawString("Fc.Rmb.md", Font2, b0Black, console_Training_X0 + 103, console_Training_Y0 + 48);
            if ((typeof_24hours_set == "forecast_corrected") && (Timeframe_crt == -1))
                g.DrawString("FcCor.Rmb.md", Font11b, b0Black, console_Training_X0 + 103, console_Training_Y0 + 48);
            if ((typeof_24hours_set == "forecast") && (Timeframe_crt != -1))
                g.DrawString("Fc.#"+ Timeframe_crt.ToString() +".md", Font2, b0Black, console_Training_X0 + 103, console_Training_Y0 + 48);
            if ((typeof_24hours_set == "forecast_corrected") && (Timeframe_crt != -1))
                g.DrawString("FcCor.#" + Timeframe_crt.ToString() + ".md", Font11b, b0Black, console_Training_X0 + 103, console_Training_Y0 + 48);
            // Cmd+Calc
            e.Graphics.DrawImage(Image_Cmd_Calc, console_Training_X0 + 218, console_Training_Y0 + 40, 77, 25); // 169 x 148, raport 1.142
            //e.Graphics.FillRectangle(b7LightGreen, console_Training_X0 + 190, console_Training_Y0 + 40, 80, 25); // 169 x 148, raport 1.142
            // Save_Solved
            e.Graphics.DrawImage(Image_Save_solved, console_Training_X0 + 218, console_Training_Y0 + 70, 77, 25); // 169 x 148, raport 1.142
            // Image down
            //e.Graphics.DrawImage(Image_down, console_Training_X0 + 100, console_Training_Y0 + 70, 35, 24); // 169 x 148, raport 1.142
            e.Graphics.DrawImage(Image_down, console_Training_X0 + 130, console_Training_Y0 + 75, 35, 24); // 169 x 148, raport 1.142
            // Image Undo
            //e.Graphics.DrawImage(Image_Back, console_Training_X0 + 145, console_Training_Y0 + 70, 35, 24); // 169 x 148, raport 1.142
            e.Graphics.DrawImage(Image_Undo, console_Training_X0 + 67, console_Training_Y0 + 40, 33, 24); // 169 x 148, raport 1.142
            // Image Redo
            e.Graphics.DrawImage(Image_Redo, console_Training_X0 + 67, console_Training_Y0 + 70, 33, 24); // 169 x 148, raport 1.142
            // regim_baza
            //e.Graphics.DrawImage(Image_regim_baza, console_Training_X0 + 10, console_Training_Y0 + 40, 80, 25); // 169 x 148, raport 1.142
            //if(Timeframe_crt == -1) e.Graphics.DrawRectangle(p4Blue3, console_Training_X0 + 10, console_Training_Y0 + 40, 80, 25); // 169 x 148, raport 1.142
            // Input_grid
            //e.Graphics.DrawImage(Image_Input_grid, console_Training_X0 + 10, console_Training_Y0 + 70, 80, 25); // 169 x 148, raport 1.142
            //e.Graphics.DrawRectangle(p4Blue3, console_Training_X0 + 10, console_Training_Y0 + 70, 80, 25); // 169 x 148, raport 1.142


            // Chenar al zonei
            e.Graphics.DrawRectangle(p5DarkBlue, console_Training_X0, console_Training_Y0 - 7, console_Training_delta_X0-1, console_Training_delta_Y0 + 11);
            // 460, 660

            if(Timeframe_crt != -1) if(Congestions[Timeframe_crt] != Congestions_old[Timeframe_crt]) { 
                richTextBox_events.Text = Congestions[Timeframe_crt];
                    Congestions_old[Timeframe_crt] = Congestions[Timeframe_crt];
            }
        }

    }
}