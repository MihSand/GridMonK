/*
 * Grid-MonK is an open source softwaer application intended to annalize power grids, esepcially microgrids
 * The basic variant works by invoking the OpenDSS open-source application, with input files prepared by Grid_MonK
 * and export output files read by OpenDSS and used for further calulations and for interracting with a grid specialist.
 * Grid-Monk can be used and modified by anybody, the only condition is to keep these comments unchanged in the upper
 * part of the used or modified application
 * There is no guarrantee given for any functionality or for any
 * influence on the computer(s) running this applications or on other applications which run on the computer(s)
 * Initiator of the Grid-Monk application: Mihai Sanduleac, University Politehnica of Bucharest, Romania
 * Contributors: 
 *    Special thanks for suggesting improved functionalities: Prof. Mircea Eremia, 
 *    Prof. Associate Lucian Toma, Dr. Catalin Chimirel
 * Basic functionality for a GUI to handle grid objects and to invoke OpenDSS for displaying results and interracting with grid objects
 *   is developed under University Politehnica of Bucharest, Department of Electrical Power Systems, started in September 2018
 * Functionality of next day congestion management dispatch annaysis developed under H2020 project WiseGrid, 
 *      specific developments started in November 2018
 * Functionality related to PMU data simulation and microgrid PMU data sent to a virtual PMU assessment tool is 
 *   developed under H2020 project NRG5, specific developments started in December 2018
 * Functionality related to connection to simulated prosumers developed in the application UnirCon_EMS is  
 *   developed under H2020 project Storage4Grid, specific developments started in December 2018
 * Develpments under free will or for developing different special functionalities needed for research projects are welcome
 */
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
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Diagnostics;

namespace GridMonC
{
    public partial class GridMonk : Form
    {

        class Poly
        {
            public String name;
            // [ [0, 0], [1, 2], [2, 2] ]
            // [0, 0]
            // [ {"x": 0, "y": 0}, {"x": 1, "y" : 2} ]
            public List<List<double>> lines; // lines[i][0]. lines[i][1]
            public Pen penStyle;
            public Poly(String _name)
            {
                name = _name;
                lines = new List<List<double>>();
                penStyle = new Pen(Color.Black);
            }

            public String mkLines()
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;
                StringBuilder sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, this.lines);
                }
                return sb.ToString();
            }

            public void readLines(String lns)
            {
                try
                {
                    var serializer = new JsonSerializer();
                    serializer.Populate(new JsonTextReader(new StringReader(lns)), this.lines);
                }
                catch { } // da eroare daca nu exisat nici o polilinie
            }

            public String mkString()
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;
                StringBuilder sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, this.lines);
                }
                return "!!polyline=" + name + " !!poly_xy=" + sb.ToString();
            }
        }

        List<Poly> polylines;
        bool drawingPolyline = false;

        string Grid_Monk_Conf = "Grid_Monk_Conf.txt";
        string OpenDSS_Path = "";
        string Grid_Projects_Path = "";
        string GridMonk_Project = "Grid_Ex01";
        string OpenDSS_file = "";
        public void read_config_file()
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(Grid_Monk_Conf);
                foreach (string line in lines)
                {
                    if (line[0] != '#')
                    {
                        char[] delimiterChars = { '=', '[', ';' };
                        string[] line1 = line.Split(delimiterChars);
                        if (line1[0] == "OpenDSS_Path") OpenDSS_Path = line1[1];
                        if (line1[0] == "Grid_Projects_Path") Grid_Projects_Path = line1[1];
                        if (line1[0] == "GridMonk_Project") GridMonk_Project = line1[1];
                        if (line1[0] == "GUI_Language") GUI_Language = line1[1];

                        if (line1[0] == "MQTT_broker_std1") MQTT_broker_std1 = line1[1];
                        if (line1[0] == "MQTT_broker_std1_subscribe_topic") MQTT_broker_std1_subscribe_topic = line1[1];

                        if (line1[0] == "MQTT_broker_std2") MQTT_broker_std2 = line1[1];
                        if (line1[0] == "MQTT_broker_std2_subscribe_topic") MQTT_broker_std2_subscribe_topic = line1[1];
                    }
                    /*if (line1[0] == "Communication") {
                        if (line1[1] == "COM_port") textBox_com_port.Text = line1[2];
                        if (line1[1] == "COM_info") comboBox_com_info.Text = line1[2];
                        if (line1[1] == "COM_baudrate") comboBox_baudrate.Text = line1[2];
                        if (line1[1] == "Access_level") comboBox_Access_level.Text = line1[2];
                        if (line1[1] == "Password") textBox_password1.Text = line1[2];
                        if (line1[1] == "SLAM_delay1") textBox_SLAM_timeout1.Text = line1[2];
                        if (line1[1] == "SLAM_delay2") textBox_SLAM_timeout2.Text = line1[2];
                    }*/
                }
            }
            catch
            {
                //textBox_path_files.Text = @"C:\";
            }
            OpenDSS_file = GridMonk_Project + ".dss";
            richTextBox_console2.Text = ">> GridMonK config. file:\n" + Grid_Monk_Conf + "\n";
            richTextBox_console2.Text += ">> Path OpenDSS:\n" + OpenDSS_Path +"\n";
            richTextBox_console2.Text += ">> Path Grid:\n" + Grid_Projects_Path + "\n";
            richTextBox_console2.Text += ">> Project:\n" + GridMonk_Project + "\n";

            richTextBox_console2.Text += ">> Grid contexts MAX no: " + historical_values_depth_MAX.ToString() + "\n";
            richTextBox_console2.Text += ">> Grid MAX lines: " + lines_MAX.ToString() + "\n";
            richTextBox_console2.Text += ">> Grid MAX loads: " + loads_MAX.ToString() + "\n";
            richTextBox_console2.Text += ">> Grid MAX generators: " + generators_MAX.ToString() + "\n";
            richTextBox_console2.Text += ">> Grid MAX trafos: " + trafos_MAX.ToString() + "\n";
            richTextBox_console2.Text += ">> Grid MAX nodes: " + nodes_MAX.ToString() + "\n";
            richTextBox_console2.Text += ">> Measurements MAX Smart Meters: " + smart_meters_MAX.ToString() + "\n";

            //button_right_wnd.Image = Image.FromFile("arrow_right1.jpg");
            // Align the image and text on the button.
            //button_right_wnd.ImageAlign = ContentAlignment.MiddleCenter;//.MiddleRight;
            // open source images from https://www.flaticon.com
            button_right_wnd.BackgroundImage = Image.FromFile("arrow_right1.jpg");
            button_right_wnd.BackgroundImageLayout = ImageLayout.Stretch;
            button_left_wnd.BackgroundImage = Image.FromFile("arrow_left1.jpg");
            button_left_wnd.BackgroundImageLayout = ImageLayout.Stretch;
            button_up_wnd.BackgroundImage = Image.FromFile("arrow_up.jpg");
            button_up_wnd.BackgroundImageLayout = ImageLayout.Stretch;
            button_down_wnd.BackgroundImage = Image.FromFile("arrow_down.jpg");
            button_down_wnd.BackgroundImageLayout = ImageLayout.Stretch;
            button_zoom_in.BackgroundImage = Image.FromFile("zoom_in.jpg");
            button_zoom_in.BackgroundImageLayout = ImageLayout.Stretch;
            button_zoom_out.BackgroundImage = Image.FromFile("zoom_out.jpg");
            button_zoom_out.BackgroundImageLayout = ImageLayout.Stretch;
            button_center.BackgroundImage = Image.FromFile("Button_center.jpg");
            button_center.BackgroundImageLayout = ImageLayout.Stretch;
        }

        public GridMonk()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            //this..MaximizeBox();
            polylines = new List<Poly>();

            read_config_file();
            Console_Training_Ini();
            // prepare MQTT connectors
            try
            {
                client1 = new MqttClient(MQTT_broker_std1);
                code = client1.Connect(Guid.NewGuid().ToString());

                client1.MqttMsgPublished += client_MqttMsgPublished;
                client1.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

                MQTT_subscribe("Basic_Topic_client1", 1);
            }
            catch
            {
                Console.WriteLine("Error initialising MQTT Client 1");
            }
            try
            {
                client2 = new MqttClient(MQTT_broker_std1);
                code = client2.Connect(Guid.NewGuid().ToString());

                client2.MqttMsgPublished += client_MqttMsgPublished;
                client2.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

                MQTT_subscribe("#", 2);
            }
            catch
            {
                Console.WriteLine("Error initialising MQTT Client 2");
            }

            for (int i1 = 0; i1 < lines_MAX; i1++) for (int j1 = 0; j1 < lines_prop_MAX; j1++)
                    for (int k1 = 0; k1 < historical_values_depth_MAX; k1++) lines_values_set[i1, j1, k1] = "";
            for (int i1 = 0; i1 < historical_values_depth_MAX; i1++) { Congestions[i1] = ""; Congestions_old[i1] = ""; }
            for (int i1 = 0; i1 < graph_smallgph_MAX; i1++) for (int j1 = 0; j1 < graph_smallgph_prop_MAX; j1++) graph_smallgph[i1, j1] = "";
            for (int i1 = 0; i1 < scenarios_prop_MAX; i1++)
                for (int j1 = 0; j1 < historical_values_depth_MAX; j1++)
                {
                    scenarios[i1, j1] = "";
                }

            gph_phasors_alloc(); // space allocation for gph_phasors
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime t1 = DateTime.Now;
            textBox_DateTime.Text = t1.Year.ToString() + "-" + "-" + t1.Month.ToString() + "-" + t1.Day.ToString()
                + "    " + t1.Hour.ToString() + ":" + t1.Minute.ToString() + ":" + t1.Second.ToString();
            Angle_real_time += (grid_frequency - 50) * 50;
            if(Angle_real_time>360) Angle_real_time += -360; // this angle is for simulating PMU phasors rotation
            //textBox_timeframe_crt.Text = Timeframe_crt_str;

            // read MQTT data 
            MQTT_broker_broker_crt = 1;
            MQTT_publish("Time", "a" + t1.ToLongDateString(), MQTT_broker_broker_crt);
            MqttClient2_str_in = message_received;
            textBox_remove.Text = message_received;

            Refresh();
        }

        private void timer2_MQTT_pub_Tick(object sender, EventArgs e)
        {
            // data sent as JSON messages
            string s1 = "230", s2 = "0.56", s3 = "50.03", s4 = "1.37";
            // {"SMX1": { "U1": "230", "U1fi"="0.56", "f"="50.03", "rocof"= "1.37" } }
            
            for (int i1 = 0; i1 < PMUs_no; i1++) {
                var jobj = new JObject();
                var smx = new JObject();
                //smx.Add("U1", s1);
                //smx.Add("U1fi", s2);
                //smx.Add("f", s3);
                //smx.Add("rocof", s4);

                smx.Add("Obj", PMUs[i1, PMUs_PROP_obj]);
                smx.Add("Obj_no", PMUs[i1, PMUs_PROP_number]);
                smx.Add("U1", PMUs[i1, PMUs_PROP_U1]);
                smx.Add("U1fi", PMUs[i1, PMUs_PROP_U1fi]);
                smx.Add("U2", PMUs[i1, PMUs_PROP_U2]);
                smx.Add("U2fi", PMUs[i1, PMUs_PROP_U2fi]);
                smx.Add("U3", PMUs[i1, PMUs_PROP_U3]);
                smx.Add("U3fi", PMUs[i1, PMUs_PROP_U3fi]);
                smx.Add("f", s3);
                smx.Add("rocof", s4);
                jobj.Add("PMU", smx);
                string serialized = JsonConvert.SerializeObject(jobj);
                MQTT_publish("PMU"+i1.ToString("00"), serialized, 1);
            }

        }


        int object_dx = 95, object_dxtot = 100, line_dy = 95, line_dytot = 100;
        int object_dx_EV = 48, object_dx_EVtot = 50, object_dy_EV = 78, object_dysmall_EV = 51; // dimensiuni pentru obiecte de tip EV
        int object_x0 = 0, object_y0 = 0;

        int obj_number = 0;

        int crtnr = 0;
        private void button_Start_Click(object sender, EventArgs e)
        {
            drawingPolyline = true;
            polylines.Add(new Poly("n" + crtnr.ToString()));
            crtnr++;
        }

        private void button_Stop_Click(object sender, EventArgs e)
        {
            drawingPolyline = false;
        }

        //int x_mouse_crt = 0, y_mouse_crt = 0;
        private int inside_rect(int x1, int y1, int x2, int y2, int px, int py)
        { // if [px, py] are inside the rectangle [x1,y1,x2,y2], it returns 1, else 0
            int res = 0;
            if ((x1 <= px) && (x2 >= px) && (y1 <= py) && (y2 >= py)) res = 1;
            return res;
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            textBox_mouse_xy.Text = " (" + e.X.ToString() + ", " + e.Y.ToString() + ")";
            Gph_element_identification(e.X, e.Y);

            if (drawingPolyline)
            {
                int last = polylines.Count() - 1;
                List<List<double>> crt = polylines[last].lines;
                //polylines.Remove(crt);
                List<double> n = new List<double>();
                n.Add(e.X);
                n.Add(e.Y);
                crt.Add(n);
                Refresh();
                //textBox_mouse_xy.Text = crt.Count().ToString();
                textBox_mouse_xy.Text = " (" + e.X.ToString() + ", " + e.Y.ToString() + ")";
            }
        }

        private void button_Next_timeframe_Click(object sender, EventArgs e)
        {

        }

        private void button_Prev_timeframe_Click(object sender, EventArgs e)
        {
            // move to previous timeframe
            //int Timeframe_crt = 1;
            if(Timeframe_crt_str == "Base") Timeframe_crt = 1;
            else Timeframe_crt--;
            if (Timeframe_crt < 1) Timeframe_crt = 1;
            Timeframe_crt_str = Timeframe_crt.ToString();
            textBox_S_max_U_stability.Text = Timeframe_crt_str;

        }

        private void gridSummaryReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            calculate_grid_data1();
            Refresh();
        }

        private void button_left_wnd_Click(object sender, EventArgs e)
        {
            //   <<==
            X0_shift = X0_shift - Electrical_scheme_zone_delta_X0 / 4;
            if (X0_shift < -Electrical_scheme_zone_delta_X0 * 3 / 4) X0_shift = -Electrical_scheme_zone_delta_X0 * 3 / 4;
        }

        private void button_right_wnd_Click(object sender, EventArgs e)
        {
            //   ==>>
            X0_shift = X0_shift + Electrical_scheme_zone_delta_X0 / 4;
            if (X0_shift > Electrical_scheme_zone_delta_X0 * 3 / 4) X0_shift = Electrical_scheme_zone_delta_X0 * 3 / 4;
        }

        private void button_up_wnd_Click(object sender, EventArgs e)
        {
            //   ▲ Up
            Y0_shift = Y0_shift - Electrical_scheme_zone_delta_Y0 / 4;
            if (Y0_shift < -Electrical_scheme_zone_delta_Y0 * 3 / 4) Y0_shift = -Electrical_scheme_zone_delta_Y0 * 3 / 4;

        }

        private void button_down_wnd_Click(object sender, EventArgs e)
        {
            //   ▼ Down
            Y0_shift = Y0_shift + Electrical_scheme_zone_delta_Y0 / 4;
            if (Y0_shift > Electrical_scheme_zone_delta_Y0 * 3 / 4) Y0_shift = Electrical_scheme_zone_delta_Y0 * 3 / 4;

        }


        private void button_remove_Click(object sender, EventArgs e)
        {
            String name = textBox_remove.Text;
            //polylines.Remove(polylines.Find(x => x.name == name));
            textBox_mouse_xy.Text = polylines.LastIndexOf(polylines.Find(x => x.name == name)).ToString();
            richTextBox_console2.Text += polylines.Find(x => x.name == name).mkString() + "\n";
            Refresh();
        }

        private void button_Compute_Click(object sender, EventArgs e)
        {
            DateTime t1 = DateTime.Now;
            string st1 = ">> Grid compute\n";
            int t1s = t1.Second, t1ms = t1.Millisecond;
            st1 += "T(ini):" + t1.Year.ToString() + "." + t1.Month.ToString("00") + "." + t1.Day.ToString("00")
                + " " + t1.Hour.ToString("00") + ":" + t1.Minute.ToString("00") + ":" + t1.Second.ToString("00")
                 + "." + t1.Millisecond.ToString("000") + "\n";
            string richTextBox_console_answers_str = st1;

            richTextBox_console_answers_str += ">> Generate output (";
            generate_output_dss("multi_LP_RMB_and_24h", "Forecast"); // se salveaza statusul curent al retelei

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
                            +"." + t2.Millisecond.ToString("000") + "\n";
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

            typeof_24hours_set = "forecast";

            Refresh(); // se redeseneaza GUI
        }

        /// Invoke OpenDSS
        void OpenDSS_invoke(string OpenDSS_file)
        {
            // STDERR_OpenDSS.Txt
            System.IO.File.Delete(Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + "STDERR_OpenDSS.Txt");

            //GridMonk2OpenDSS_grid_file = Grid_Projects_Path + @"\" + GridMonk_Project + @"\" + "output1.dss";
            GridMonk2OpenDSS_grid_file = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + OpenDSS_file + ".dss";

            // se ruleaza OpenDSS
            ProcessStartInfo startInfo = new ProcessStartInfo(); // E:\App\VStudio\GridMonK\GM_Projects
            startInfo.FileName = OpenDSS_Path+ @"/OpenDSS.exe";
            if(checkBox_file_dss.Checked==true) // se foloseste fisierul initial
                startInfo.Arguments = @"E:/App/VStudio/GridMonK/PowerGrid/Grid_test_01_Monc.dss /nogui";
            else // se foloseste fisierul ce descrie starea curenta a retelei, rezultaet in urma interactiunii GUI
                startInfo.Arguments = GridMonk2OpenDSS_grid_file + " /nogui";
            Process prc = Process.Start(startInfo);
            prc.WaitForExit(5000);
            
            if(!prc.HasExited)
            {
                // return -1; sau altceva => s-a blocat
            }
            System.Threading.Thread.Sleep(100); // se asteapta 100 milisecunde;

            //System.Threading.Thread.Sleep(1500); // se asteapta 1.5 secunde; timpul ar putea sa creasca sau se va implementa o alta varianta 

        }

        void calculate_values_from_results()
        {
            // se calculeaza marimi ce se pot deduce din cele citite 
            double P3f = 0;
            double Q3f = 0;
            double S3f = 0;

            if (Grid_is_calculated == 0) return;

            // loads
            for (int i1 = 0; i1 < loads_no; i1++)
            {
                if ((loads[i1, loads_PROP_P1] != "") && (loads[i1, loads_PROP_P2] != "") && (loads[i1, loads_PROP_P3] != ""))
                {
                    P3f = double.Parse(loads[i1, loads_PROP_P1]) + double.Parse(loads[i1, loads_PROP_P2]) + +double.Parse(loads[i1, loads_PROP_P3]);
                    loads[i1, loads_PROP_P] = P3f.ToString("#####0.000");
                }
                if ((loads[i1, loads_PROP_Q1] != "") && (loads[i1, loads_PROP_Q2] != "") && (loads[i1, loads_PROP_Q3] != ""))
                {
                    Q3f = double.Parse(loads[i1, loads_PROP_Q1]) + double.Parse(loads[i1, loads_PROP_Q2]) + +double.Parse(loads[i1, loads_PROP_Q3]);
                    loads[i1, loads_PROP_Q] = Q3f.ToString("#####0.000");
                }

            }
            for (int i1 = 0; i1 < loads_MAX; i1++)
                for (int j1 = LPs_scenarios_multi_LF_start + 1; j1 < historical_values_depth_MAX- LPs_scenarios_multi_LF_start - 1; j1++)
                {
                    if ((loads_values_set[i1, loads_PROP_P1, j1] != "") && (loads_values_set[i1, loads_PROP_P2, j1] != "") && (loads_values_set[i1, loads_PROP_P3, j1] != ""))
                    {
                        P3f = double.Parse(loads_values_set[i1, loads_PROP_P1, j1]) + double.Parse(loads_values_set[i1, loads_PROP_P2, j1]) + double.Parse(loads_values_set[i1, loads_PROP_P3, j1]);
                        loads_values_set[i1, loads_PROP_P, j1] = P3f.ToString("#####0.0");
                    }
                    if ((loads_values_set[i1, loads_PROP_Q1, j1] != "") && (loads_values_set[i1, loads_PROP_Q2, j1] != "") && (loads_values_set[i1, loads_PROP_Q3, j1] != ""))
                    {
                        Q3f = double.Parse(loads_values_set[i1, loads_PROP_Q1, j1]) + double.Parse(loads_values_set[i1, loads_PROP_Q2, j1]) + double.Parse(loads_values_set[i1, loads_PROP_Q3, j1]);
                        loads_values_set[i1, loads_PROP_Q, j1] = Q3f.ToString("#####0.0");
                    }
                }

            // lines
            for (int i1 = 0; i1 < lines_no; i1++)
            {
                if ((lines[i1, lines_PROP_P1] != "") && (lines[i1, lines_PROP_P2] != "") && (lines[i1, lines_PROP_P3] != ""))
                {
                    P3f = double.Parse(lines[i1, lines_PROP_P1]) + double.Parse(lines[i1, lines_PROP_P2]) + +double.Parse(lines[i1, lines_PROP_P3]);
                    lines[i1, lines_PROP_P] = P3f.ToString("#####0.0");
                }
                if ((lines[i1, lines_PROP_Q1] != "") && (lines[i1, lines_PROP_Q2] != "") && (lines[i1, lines_PROP_Q3] != ""))
                {
                    Q3f = double.Parse(lines[i1, lines_PROP_Q1]) + double.Parse(lines[i1, lines_PROP_Q2]) + +double.Parse(lines[i1, lines_PROP_Q3]);
                    lines[i1, lines_PROP_Q] = Q3f.ToString("#####0.0");
                }
                if ((lines[i1, lines_PROP_P] != "") && (lines[i1, lines_PROP_Q] != ""))
                {
                    S3f = Math.Sqrt(P3f*P3f + Q3f*Q3f);
                    lines[i1, lines_PROP_S] = S3f.ToString("#####0.0");
                }

                if ((lines[i1, lines_PROP_P1_t2] != "") && (lines[i1, lines_PROP_P2_t2] != "") && (lines[i1, lines_PROP_P3_t2] != ""))
                {
                    P3f = double.Parse(lines[i1, lines_PROP_P1_t2]) + double.Parse(lines[i1, lines_PROP_P2_t2]) + +double.Parse(lines[i1, lines_PROP_P3_t2]);
                    lines[i1, lines_PROP_P_t2] = P3f.ToString("#####0.0");
                }
                if ((lines[i1, lines_PROP_Q1_t2] != "") && (lines[i1, lines_PROP_Q2_t2] != "") && (lines[i1, lines_PROP_Q3_t2] != ""))
                {
                    Q3f = double.Parse(lines[i1, lines_PROP_Q1_t2]) + double.Parse(lines[i1, lines_PROP_Q2_t2]) + +double.Parse(lines[i1, lines_PROP_Q3_t2]);
                    lines[i1, lines_PROP_Q_t2] = Q3f.ToString("#####0.0");
                }
                if ((lines[i1, lines_PROP_P_t2] != "") && (lines[i1, lines_PROP_Q_t2] != ""))
                {
                    S3f = Math.Sqrt(P3f * P3f + Q3f * Q3f);
                    lines[i1, lines_PROP_S_t2] = S3f.ToString("#####0.0");
                }

                int found_linecode = -1;
                for (int j1 = 0; j1 < linecodes_no; j1++)
                {
                    if (linecodes[j1, linecodes_PROP_name] == lines[i1, lines_PROP_linecode])
                    {
                        found_linecode = j1; j1 = linecodes_no;
                    }
                }
                if (found_linecode != -1) { 
                    double R1 = 0;
                    if (linecodes[found_linecode, linecodes_PROP_R1] != "")
                    R1 = double.Parse(linecodes[found_linecode, linecodes_PROP_R1]) * double.Parse(lines[i1, lines_PROP_length]);
                    lines[i1, lines_PROP_R1] = R1.ToString("###0.000");
                    double X1 = 0;
                    if (linecodes[found_linecode, linecodes_PROP_X1] != "")
                    X1 = double.Parse(linecodes[found_linecode, linecodes_PROP_X1]) * double.Parse(lines[i1, lines_PROP_length]);
                    lines[i1, lines_PROP_X1] = X1.ToString("###0.000");
                    //double Imax = 0;
                    if (linecodes[found_linecode, linecodes_PROP_Imax] != "")
                        lines[i1, lines_PROP_Imax] = linecodes[found_linecode, linecodes_PROP_Imax];
                    //double Umax = 0;
                    if (linecodes[found_linecode, linecodes_PROP_Umax] != "")
                        lines[i1, lines_PROP_Umax] = linecodes[found_linecode, linecodes_PROP_Umax];
                }
            }
            for (int i1 = 0; i1 < lines_MAX; i1++)
                for (int j1 = LPs_scenarios_multi_LF_start + 1; j1 < historical_values_depth_MAX - LPs_scenarios_multi_LF_start - 1; j1++)
                {
                    if ((lines_values_set[i1, lines_PROP_P1, j1] !="") && (lines_values_set[i1, lines_PROP_P2, j1] != "") && (lines_values_set[i1, lines_PROP_P3, j1] != ""))
                    {
                        P3f = double.Parse(lines_values_set[i1, lines_PROP_P1, j1]) + double.Parse(lines_values_set[i1, lines_PROP_P2, j1]) + +double.Parse(lines_values_set[i1, lines_PROP_P3, j1]);
                        lines_values_set[i1, lines_PROP_P, j1] = P3f.ToString("#####0.0");
                    }
                    if ((lines_values_set[i1, lines_PROP_P1_t2, j1] != "") && (lines_values_set[i1, lines_PROP_P2_t2, j1] != "") && (lines_values_set[i1, lines_PROP_P3_t2, j1] != ""))
                    {
                        P3f = double.Parse(lines_values_set[i1, lines_PROP_P1_t2, j1]) + double.Parse(lines_values_set[i1, lines_PROP_P2_t2, j1]) + +double.Parse(lines_values_set[i1, lines_PROP_P3_t2, j1]);
                        lines_values_set[i1, lines_PROP_P_t2, j1] = P3f.ToString("#####0.0");
                    }
                    if ((lines_values_set[i1, lines_PROP_Q1, j1] != "") && (lines_values_set[i1, lines_PROP_Q2, j1] != "") && (lines_values_set[i1, lines_PROP_Q3, j1] != ""))
                    {
                        Q3f = double.Parse(lines_values_set[i1, lines_PROP_Q1, j1]) + double.Parse(lines_values_set[i1, lines_PROP_Q2, j1]) + +double.Parse(lines_values_set[i1, lines_PROP_Q3, j1]);
                        lines_values_set[i1, lines_PROP_Q, j1] = Q3f.ToString("#####0.0");
                    }
                    if ((lines_values_set[i1, lines_PROP_Q1_t2, j1] != "") && (lines_values_set[i1, lines_PROP_Q2_t2, j1] != "") && (lines_values_set[i1, lines_PROP_Q3_t2, j1] != ""))
                    {
                        Q3f = double.Parse(lines_values_set[i1, lines_PROP_Q1_t2, j1]) + double.Parse(lines_values_set[i1, lines_PROP_Q2_t2, j1]) + +double.Parse(lines_values_set[i1, lines_PROP_Q3_t2, j1]);
                        lines_values_set[i1, lines_PROP_Q_t2, j1] = Q3f.ToString("#####0.0");
                    }
                    if ((lines_values_set[i1, lines_PROP_P, j1] != "") && (lines_values_set[i1, lines_PROP_Q, j1] != ""))
                    {
                        S3f = double.Parse(lines_values_set[i1, lines_PROP_P, j1])* double.Parse(lines_values_set[i1, lines_PROP_P, j1])
                            + double.Parse(lines_values_set[i1, lines_PROP_Q, j1])*double.Parse(lines_values_set[i1, lines_PROP_Q, j1]);
                        lines_values_set[i1, lines_PROP_S, j1] = Q3f.ToString("#####0.0");
                    }
                }

            // generators
            for (int i1 = 0; i1 < generators_no; i1++)
            {
                if ((generators[i1, generators_PROP_P1] != "") && (generators[i1, generators_PROP_P2] != "") && (generators[i1, generators_PROP_P3] != ""))
                {
                    P3f = double.Parse(generators[i1, generators_PROP_P1]) + double.Parse(generators[i1, generators_PROP_P2]) + +double.Parse(generators[i1, generators_PROP_P3]);
                    generators[i1, generators_PROP_P] = P3f.ToString("#####0.0");
                }
                if ((generators[i1, generators_PROP_Q1] != "") && (generators[i1, generators_PROP_Q2] != "") && (generators[i1, generators_PROP_Q3] != ""))
                {
                    Q3f = double.Parse(generators[i1, generators_PROP_Q1]) + double.Parse(generators[i1, generators_PROP_Q2]) + +double.Parse(generators[i1, generators_PROP_Q3]);
                    generators[i1, generators_PROP_Q] = Q3f.ToString("#####0.0");
                }
                if ((generators[i1, generators_PROP_P] != "") && (generators[i1, generators_PROP_Q] != ""))
                {
                    S3f = Math.Sqrt(P3f * P3f + Q3f * Q3f);
                    generators[i1, generators_PROP_S] = S3f.ToString("#####0.0");
                }
            }
            for (int i1 = 0; i1 < generators_MAX; i1++)
                for (int j1 = LPs_scenarios_multi_LF_start + 1; j1 < historical_values_depth_MAX - LPs_scenarios_multi_LF_start - 1; j1++)
                {
                    if ((generators_values_set[i1, generators_PROP_P1, j1] != "") && (generators_values_set[i1, generators_PROP_P2, j1] != "") && (generators_values_set[i1, generators_PROP_P3, j1] != ""))
                    {
                        P3f = double.Parse(generators_values_set[i1, generators_PROP_P1, j1]) + double.Parse(generators_values_set[i1, generators_PROP_P2, j1]) + +double.Parse(generators_values_set[i1, generators_PROP_P3, j1]);
                        generators_values_set[i1, generators_PROP_P, j1] = P3f.ToString("#####0.0");
                    }
                    if ((generators_values_set[i1, generators_PROP_Q1, j1] != "") && (generators_values_set[i1, generators_PROP_Q2, j1] != "") && (generators_values_set[i1, generators_PROP_Q3, j1] != ""))
                    {
                        Q3f = double.Parse(generators_values_set[i1, generators_PROP_Q1, j1]) + double.Parse(generators_values_set[i1, generators_PROP_Q2, j1]) + +double.Parse(generators_values_set[i1, generators_PROP_Q3, j1]);
                        generators_values_set[i1, generators_PROP_Q, j1] = Q3f.ToString("#####0.0");
                    }
                }

            // trafos
            for (int i1 = 0; i1 < trafos_no; i1++)
            {
                if ((trafos[i1, trafos_PROP_P1] != "") && (trafos[i1, trafos_PROP_P2] != "") && (trafos[i1, trafos_PROP_P3] != ""))
                {
                    P3f = double.Parse(trafos[i1, trafos_PROP_P1]) + double.Parse(trafos[i1, trafos_PROP_P2]) + +double.Parse(trafos[i1, trafos_PROP_P3]);
                    trafos[i1, trafos_PROP_P] = P3f.ToString("#####0.0");
                }
                if ((trafos[i1, trafos_PROP_Q1] != "") && (trafos[i1, trafos_PROP_Q2] != "") && (trafos[i1, trafos_PROP_Q3] != ""))
                {
                    Q3f = double.Parse(trafos[i1, trafos_PROP_Q1]) + double.Parse(trafos[i1, trafos_PROP_Q2]) + +double.Parse(trafos[i1, trafos_PROP_Q3]);
                    trafos[i1, trafos_PROP_Q] = Q3f.ToString("#####0.0");
                }
                if ((trafos[i1, trafos_PROP_P] != "") && (trafos[i1, trafos_PROP_Q] != ""))
                {
                    S3f = Math.Sqrt(P3f * P3f + Q3f * Q3f);
                    trafos[i1, trafos_PROP_S] = S3f.ToString("#####0.0");
                }
            }
            for (int i1 = 0; i1 < trafos_MAX; i1++)
                for (int j1 = LPs_scenarios_multi_LF_start + 1; j1 < historical_values_depth_MAX - LPs_scenarios_multi_LF_start - 1; j1++)
                {
                    if ((trafos_values_set[i1, trafos_PROP_P1, j1] != "") && (trafos_values_set[i1, trafos_PROP_P2, j1] != "") && (trafos_values_set[i1, trafos_PROP_P3, j1] != ""))
                    {
                        P3f = double.Parse(trafos_values_set[i1, trafos_PROP_P1, j1]) + double.Parse(trafos_values_set[i1, trafos_PROP_P2, j1]) + +double.Parse(trafos_values_set[i1, trafos_PROP_P3, j1]);
                        trafos_values_set[i1, trafos_PROP_P, j1] = P3f.ToString("#####0.0");
                    }
                    if ((trafos_values_set[i1, trafos_PROP_Q1, j1] != "") && (trafos_values_set[i1, trafos_PROP_Q2, j1] != "") && (trafos_values_set[i1, trafos_PROP_Q3, j1] != ""))
                    {
                        Q3f = double.Parse(trafos_values_set[i1, trafos_PROP_Q1, j1]) + double.Parse(trafos_values_set[i1, trafos_PROP_Q2, j1]) + +double.Parse(trafos_values_set[i1, trafos_PROP_Q3, j1]);
                        trafos_values_set[i1, trafos_PROP_Q, j1] = Q3f.ToString("#####0.0");
                    }
                    if ((trafos_values_set[i1, trafos_PROP_P, j1] != "") && (trafos_values_set[i1, trafos_PROP_Q, j1] != ""))
                    {
                        S3f = double.Parse(trafos_values_set[i1, trafos_PROP_P, j1]) * double.Parse(trafos_values_set[i1, trafos_PROP_P, j1])
                            + double.Parse(trafos_values_set[i1, trafos_PROP_Q, j1]) * double.Parse(trafos_values_set[i1, trafos_PROP_Q, j1]);
                        trafos_values_set[i1, trafos_PROP_S, j1] = Q3f.ToString("#####0.0");
                    }
                }
        }

        void assess_nodes_properties()
        {
            //  
            // se aloca tensiuni la noduri, prin citirea tensiunii unui "load", "generator" sau "line"  conectat la acel nod
            // (in ac. ordine si prioritate de alocare)
            for (int n1 = 0; n1 < nodes_no; n1++)
            {
                int object_no = -1;
                if(nodes[n1, nodes_PROP_U_source_object] == "load") { 
                    if(nodes[n1, nodes_PROP_U_source_object_number] != "") {
                        object_no = int.Parse(nodes[n1, nodes_PROP_U_source_object_number]);
                        nodes[n1, nodes_PROP_U1] = loads[object_no, loads_PROP_U1];
                        nodes[n1, nodes_PROP_U2] = loads[object_no, loads_PROP_U2];
                        nodes[n1, nodes_PROP_U3] = loads[object_no, loads_PROP_U3];
                        nodes[n1, nodes_PROP_U4] = loads[object_no, loads_PROP_U4];

                        nodes[n1, nodes_PROP_U1fi] = loads[object_no, loads_PROP_U1fi];
                        nodes[n1, nodes_PROP_U2fi] = loads[object_no, loads_PROP_U2fi];
                        nodes[n1, nodes_PROP_U3fi] = loads[object_no, loads_PROP_U3fi];
                        nodes[n1, nodes_PROP_U4fi] = loads[object_no, loads_PROP_U4fi];

                        nodes[n1, nodes_PROP_voltage] = loads[object_no, loads_PROP_voltage];
                        if(loads[object_no, loads_PROP_brk]=="on")
                        nodes[n1, nodes_PROP_U_source_object_avail_U_meas] = "1"; 
                        else nodes[n1, nodes_PROP_U_source_object_avail_U_meas] = "0";
                    }
                }
                else if (nodes[n1, nodes_PROP_U_source_object] == "generator") { 
                    if (nodes[n1, nodes_PROP_U_source_object_number] != "")
                    {
                        object_no = int.Parse(nodes[n1, nodes_PROP_U_source_object_number]);
                        nodes[n1, nodes_PROP_U1] = generators[object_no, generators_PROP_U1];
                        nodes[n1, nodes_PROP_U2] = generators[object_no, generators_PROP_U2];
                        nodes[n1, nodes_PROP_U3] = generators[object_no, generators_PROP_U3];
                        nodes[n1, nodes_PROP_U4] = generators[object_no, generators_PROP_U4];

                        nodes[n1, nodes_PROP_U1fi] = generators[object_no, generators_PROP_U1fi];
                        nodes[n1, nodes_PROP_U2fi] = generators[object_no, generators_PROP_U2fi];
                        nodes[n1, nodes_PROP_U3fi] = generators[object_no, generators_PROP_U3fi];
                        nodes[n1, nodes_PROP_U4fi] = generators[object_no, generators_PROP_U4fi];

                        nodes[n1, nodes_PROP_voltage] = generators[object_no, loads_PROP_voltage];
                        if (generators[object_no, loads_PROP_brk] == "on")
                            nodes[n1, nodes_PROP_U_source_object_avail_U_meas] = "1";
                        else nodes[n1, nodes_PROP_U_source_object_avail_U_meas] = "0";
                    }
                }
                else if (nodes[n1, nodes_PROP_U_source_object] == "line")
                {
                    if (nodes[n1, nodes_PROP_U_source_object_number] != "")
                    {
                        object_no = int.Parse(nodes[n1, nodes_PROP_U_source_object_number]);
                        nodes[n1, nodes_PROP_U1] = lines[object_no, lines_PROP_U1];
                        nodes[n1, nodes_PROP_U2] = lines[object_no, lines_PROP_U2];
                        nodes[n1, nodes_PROP_U3] = lines[object_no, lines_PROP_U3];
                        //nodes[n1, nodes_PROP_U4] = lines[object_no, lines_PROP_U4];

                        nodes[n1, nodes_PROP_U1fi] = lines[object_no, lines_PROP_U1fi];
                        nodes[n1, nodes_PROP_U2fi] = lines[object_no, lines_PROP_U2fi];
                        nodes[n1, nodes_PROP_U3fi] = lines[object_no, lines_PROP_U3fi];
                        //nodes[n1, nodes_PROP_U4fi] = lines[object_no, lines_PROP_U4fi];

                        nodes[n1, nodes_PROP_voltage] = generators[object_no, loads_PROP_voltage];
                        if (generators[object_no, loads_PROP_brk] == "on")
                            nodes[n1, nodes_PROP_U_source_object_avail_U_meas] = "1";
                        else nodes[n1, nodes_PROP_U_source_object_avail_U_meas] = "0";
                    }
                }
            }
/*            for (int n1 = 0; n1 < nodes_no; n1++)
            {
                // se aloca proprietati ndourile, rezultate din declaraii de tip nodes_metadata
                for (int m1 = 0; m1 < nodes_metadata_no; m1++)
                {
                    if (nodes_metadata[m1, nodes_metadata_PROP_bus] == nodes[n1, nodes_PROP_bus]) {
                        nodes[n1, nodes_PROP_name] = nodes_metadata[m1, nodes_metadata_PROP_name];
                        nodes[n1, nodes_PROP_x0] = nodes_metadata[m1, nodes_metadata_PROP_x0];
                        nodes[n1, nodes_PROP_y0] = nodes_metadata[m1, nodes_metadata_PROP_y0];
                        nodes[n1, nodes_PROP_arrow] = nodes_metadata[m1, nodes_metadata_PROP_arrow];
                        nodes[n1, nodes_PROP_bus_name] = nodes_metadata[m1, nodes_metadata_PROP_bus_name];
                        nodes[n1, nodes_PROP_draw_U1] = nodes_metadata[m1, nodes_metadata_PROP_draw_U1];
                        nodes[n1, nodes_PROP_draw_U1fi] = nodes_metadata[m1, nodes_metadata_PROP_draw_U1fi];
                    }
                }
            }
            */
        }

        void add_nodes_properties_from_paramaterisation_nodes_metadata()
        {
            //  Add nodes properties from nodes metadata associated with these nodes
            for (int n1 = 0; n1 < nodes_no; n1++)
            {
                // se aloca proprietati ndourile, rezultate din declaraii de tip nodes_metadata
                for (int m1 = 0; m1 < nodes_metadata_no; m1++)
                {
                    if (nodes_metadata[m1, nodes_metadata_PROP_bus] == nodes[n1, nodes_PROP_bus])
                    {
                        nodes[n1, nodes_PROP_name] = nodes_metadata[m1, nodes_metadata_PROP_name];
                        nodes[n1, nodes_PROP_x0] = nodes_metadata[m1, nodes_metadata_PROP_x0];
                        nodes[n1, nodes_PROP_y0] = nodes_metadata[m1, nodes_metadata_PROP_y0];
                        nodes[n1, nodes_PROP_arrow] = nodes_metadata[m1, nodes_metadata_PROP_arrow];
                        nodes[n1, nodes_PROP_bus_name] = nodes_metadata[m1, nodes_metadata_PROP_bus_name];
                        nodes[n1, nodes_PROP_bus_name_x] = nodes_metadata[m1, nodes_metadata_PROP_bus_name_x];
                        nodes[n1, nodes_PROP_bus_name_y] = nodes_metadata[m1, nodes_metadata_PROP_bus_name_y];
                        nodes[n1, nodes_PROP_draw_U1] = nodes_metadata[m1, nodes_metadata_PROP_draw_U1];
                        nodes[n1, nodes_PROP_draw_U1fi] = nodes_metadata[m1, nodes_metadata_PROP_draw_U1fi];
                        nodes[n1, nodes_PROP_U_x] = nodes_metadata[m1, nodes_metadata_PROP_U_x];
                        nodes[n1, nodes_PROP_U_y] = nodes_metadata[m1, nodes_metadata_PROP_U_y];
                        nodes[n1, nodes_PROP_draw_type] = nodes_metadata[m1, nodes_metadata_PROP_draw_type];
                        nodes[n1, nodes_PROP_Font1] = nodes_metadata[m1, nodes_metadata_PROP_Font1];
                        // nodes_properties_calculation(n1);
                    }
                }
            }

        }

        // This routine reads all results after OpenDSS run, and places the read values in the appropriate variables of the objects
        void read_OpenDSS_results(string load_flow_type)
        {
            // load_flow_type = "multi_LP_RMB_and_24h"
            // load_flow_type = "one_LP_training"
            // load_flow_type = "multi_LP_Scan_1440"
            string File_to_be_scanned = "";
            string File_to_be_scanned_prefix = GridMonk_Project; // default it is the GridMink project
            string file_export = "";
            string[] file_text;

            if (load_flow_type == "multi_LP_RMB_and_24h") File_to_be_scanned_prefix = GridMonk_Project;
            if (load_flow_type == "multi_LP_Scan_1440") File_to_be_scanned_prefix = "U_stability";

            for (int e1 = 0; e1 < exports_no; e1++) // se scaneaza fiecare fisier de export
            {
                // GridMonk2OpenDSS_grid_file = Grid_Projects_Path + @"\" + GridMonk_Project + @"\" + "output1.dss";
                File_to_be_scanned = Grid_Projects_Path + @"/" + GridMonk_Project + @"/";
                //File_to_be_scanned += GridMonk_Project + "_Mon_";
                File_to_be_scanned += File_to_be_scanned_prefix + "_Mon_";
                File_to_be_scanned += exports[e1, exports_PROP_param] +".csv";

                file_export = "Grid_test_01" + "_Mon_" + exports[e1, exports_PROP_param] + ".csv";
                string mon1 = ""; int mon_pos = -1;

                for (int m1 = 0; m1 < monitors_no; m1++)
                    if (monitors[m1, monitors_PROP_name] == exports[e1, exports_PROP_param]) mon_pos = m1;
                mon1 = monitors[mon_pos, monitors_PROP_name];
                string elemtype = ""; string elem_name = ""; int elem_pos = -1; string elem_terminal = "";

                // Pentru fisierul de export dat, se cauta tipul de obiect, al catelea obiect si ce tip de date se cere (UI sau PQ)

                // daca export-ul este legat de linii, pregateste citirea rezultatelor legate de linii
                if (monitors[mon_pos, monitors_PROP_element].Contains("line."))
                {
                    elemtype = "line";
                    elem_name = monitors[mon_pos, monitors_PROP_element].Remove(0, 5);
                    for (int l1 = 0; l1 < lines_no; l1++)
                        if (lines[l1, monitors_PROP_name] == elem_name) elem_pos = l1;
                    elem_terminal = monitors[mon_pos, monitors_PROP_terminal];
                }

                //  daca export-ul este legat de loads, pregateste citirea rezultatelor legate de sarcini (loads)
                if (monitors[mon_pos, monitors_PROP_element].Contains("load."))
                {
                    elemtype = "load";
                    elem_name = monitors[mon_pos, monitors_PROP_element].Remove(0, 5);
                    for (int l1 = 0; l1 < loads_no; l1++)
                        if (loads[l1, loads_PROP_name] == elem_name) elem_pos = l1;
                }

                //  daca export-ul este legat de generatoare, pregateste citirea rezultatelor legate de generatoare
                if (monitors[mon_pos, monitors_PROP_element].Contains("generator."))
                {
                    elemtype = "generator";
                    elem_name = monitors[mon_pos, monitors_PROP_element].Remove(0, 10);
                    for (int l1 = 0; l1 < generators_no; l1++)
                        if (generators[l1, generators_PROP_name] == elem_name) elem_pos = l1;
                }

                //  daca export-ul este legat de trafos, pregateste citirea rezultatelor legate de transformatoare
                if (monitors[mon_pos, monitors_PROP_element].Contains("transformer."))
                {
                    elemtype = "transformer";
                    elem_name = monitors[mon_pos, monitors_PROP_element].Remove(0, 12);
                    for (int l1 = 0; l1 < trafos_no; l1++)
                        if (trafos[l1, monitors_PROP_name] == elem_name) elem_pos = l1;
                }

                // exemplu de fisier
                //File_to_be_scanned = @"E:\App\VStudio\GridMonC\PowerGrid\" + "Grid_test_01_Mon_n10_ui.csv";


                int nr_line = 0;
                // Use a tab to indent each line of the file.
                char[] delimiterChars1 = { '\t', ',' };
                char[] delimiterChars2 = { '\t', '[' };
                try
                {
                    file_text = File.ReadAllLines(File_to_be_scanned);
                    //if(File_to_be_scanned== "E:\\App\\VStudio\\GridMonK\\PowerGrid\\Grid_test_01_Mon_ld_11_pq.csv")
                    //    nr_line = 0;
                    richTextBox_console2.Text += "File: " + file_export + "\n";
                    foreach (string line in file_text)
                    {
                        string[] line1 = line.Split(delimiterChars1);
                        //if (nr_line == 1) // primul interval de timp
                        //{
                            int pos = 0;
                            foreach (string s in line1)
                            {
                                if (elemtype == "line")
                                {
                                if (monitors[mon_pos, monitors_PROP_mode] == "1")  // modul P1,2,3, Q1,2,3
                                {
                                    if (load_flow_type == "multi_LP_RMB_and_24h") // Only for 
                                    if (nr_line == 1) // this lines contains RMB LF
                                    {
                                        if (elem_terminal == "1")
                                        {
                                            if (pos == 2) lines[elem_pos, lines_PROP_P1] = s;
                                            if (pos == 4) lines[elem_pos, lines_PROP_P2] = s;
                                            if (pos == 6) lines[elem_pos, lines_PROP_P3] = s;
                                            if (pos == 3) lines[elem_pos, lines_PROP_Q1] = s;
                                            if (pos == 5) lines[elem_pos, lines_PROP_Q2] = s;
                                            if (pos == 7) lines[elem_pos, lines_PROP_Q3] = s;
                                        }
                                        else
                                        {
                                            if (pos == 2) lines[elem_pos, lines_PROP_P1_t2] = s;
                                            if (pos == 4) lines[elem_pos, lines_PROP_P2_t2] = s;
                                            if (pos == 6) lines[elem_pos, lines_PROP_P3_t2] = s;
                                            if (pos == 3) lines[elem_pos, lines_PROP_Q1_t2] = s;
                                            if (pos == 5) lines[elem_pos, lines_PROP_Q2_t2] = s;
                                            if (pos == 7) lines[elem_pos, lines_PROP_Q3_t2] = s;
                                        }
                                    }
                                    if (elem_terminal == "1")
                                    {
                                        // elemen_pos = al cata linie; pos = care interval de timp; urmeaza variabila
                                        if (pos == 2) lines_values_set[elem_pos, lines_PROP_P1, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 3) lines_values_set[elem_pos, lines_PROP_Q1, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 4) lines_values_set[elem_pos, lines_PROP_P2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 5) lines_values_set[elem_pos, lines_PROP_Q2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 6) lines_values_set[elem_pos, lines_PROP_P3, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 7) lines_values_set[elem_pos, lines_PROP_Q3, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    }
                                    else
                                    {
                                        if (pos == 2) lines_values_set[elem_pos, lines_PROP_P1_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 3) lines_values_set[elem_pos, lines_PROP_Q1_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 4) lines_values_set[elem_pos, lines_PROP_P2_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 5) lines_values_set[elem_pos, lines_PROP_Q2_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 6) lines_values_set[elem_pos, lines_PROP_P3_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 7) lines_values_set[elem_pos, lines_PROP_Q3_t2, nr_line + LPs_scenarios_multi_LF_start] = s;

                                    }
                                }
                                if (monitors[mon_pos, monitors_PROP_mode] == "0")  // modul U 1,2,3, I 1,2,3
                                    {
                                        if (nr_line == 1)
                                        {
                                        if (elem_terminal == "1") { // capatul terminalului 1
                                            if (pos == 2) lines[elem_pos, lines_PROP_U1] = s;
                                            if (pos == 3) lines[elem_pos, lines_PROP_U1fi] = s;
                                            if (pos == 4) lines[elem_pos, lines_PROP_U2] = s;
                                            if (pos == 5) lines[elem_pos, lines_PROP_U2fi] = s;
                                            if (pos == 6) lines[elem_pos, lines_PROP_U3] = s;
                                            if (pos == 7) lines[elem_pos, lines_PROP_U3fi] = s;
                                            if (pos == 8) lines[elem_pos, lines_PROP_I1] = s;
                                            if (pos == 9) lines[elem_pos, lines_PROP_I1fi] = s;
                                            if (pos == 10) lines[elem_pos, lines_PROP_I2] = s;
                                            if (pos == 11) lines[elem_pos, lines_PROP_I2fi] = s;
                                            if (pos == 12) lines[elem_pos, lines_PROP_I3] = s;
                                            if (pos == 13) lines[elem_pos, lines_PROP_I3fi] = s;
                                        } else // celalalt capat al liniei, adica terminal=2
                                        {
                                            if (pos == 2) lines[elem_pos, lines_PROP_U1_t2] = s;
                                            if (pos == 3) lines[elem_pos, lines_PROP_U1fi_t2] = s;
                                            if (pos == 4) lines[elem_pos, lines_PROP_U2_t2] = s;
                                            if (pos == 5) lines[elem_pos, lines_PROP_U2fi_t2] = s;
                                            if (pos == 6) lines[elem_pos, lines_PROP_U3_t2] = s;
                                            if (pos == 7) lines[elem_pos, lines_PROP_U3fi_t2] = s;
                                            if (pos == 8) lines[elem_pos, lines_PROP_I1_t2] = s;
                                            if (pos == 9) lines[elem_pos, lines_PROP_I1fi_t2] = s;
                                            if (pos == 10) lines[elem_pos, lines_PROP_I2_t2] = s;
                                            if (pos == 11) lines[elem_pos, lines_PROP_I2fi_t2] = s;
                                            if (pos == 12) lines[elem_pos, lines_PROP_I3_t2] = s;
                                            if (pos == 13) lines[elem_pos, lines_PROP_I3fi_t2] = s;

                                        }
                                    }
                                        // now we store 24 hours data
                                    if (elem_terminal == "1")
                                    { 
                                        if (pos == 2) lines_values_set[elem_pos, lines_PROP_U1, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 3) lines_values_set[elem_pos, lines_PROP_U1fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 4) lines_values_set[elem_pos, lines_PROP_U2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 5) lines_values_set[elem_pos, lines_PROP_U2fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 6) lines_values_set[elem_pos, lines_PROP_U3, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 7) lines_values_set[elem_pos, lines_PROP_U3fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 8) lines_values_set[elem_pos, lines_PROP_I1, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 9) lines_values_set[elem_pos, lines_PROP_I1fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 10) lines_values_set[elem_pos, lines_PROP_I2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 11) lines_values_set[elem_pos, lines_PROP_I2fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 12) lines_values_set[elem_pos, lines_PROP_I3, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 13) lines_values_set[elem_pos, lines_PROP_I3fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    }
                                    if (elem_terminal == "2")
                                    {
                                        if (pos == 2) lines_values_set[elem_pos, lines_PROP_U1_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 3) lines_values_set[elem_pos, lines_PROP_U1fi_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 4) lines_values_set[elem_pos, lines_PROP_U2_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 5) lines_values_set[elem_pos, lines_PROP_U2fi_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 6) lines_values_set[elem_pos, lines_PROP_U3_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 7) lines_values_set[elem_pos, lines_PROP_U3fi_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 8) lines_values_set[elem_pos, lines_PROP_I1_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 9) lines_values_set[elem_pos, lines_PROP_I1fi_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 10) lines_values_set[elem_pos, lines_PROP_I2_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 11) lines_values_set[elem_pos, lines_PROP_I2fi_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 12) lines_values_set[elem_pos, lines_PROP_I3_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 13) lines_values_set[elem_pos, lines_PROP_I3fi_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    }
                                }
                            }
                            if (elemtype == "transformer")
                             {
                                    if (monitors[mon_pos, monitors_PROP_mode] == "1")  // modul P1,2,3, Q1,2,3
                                    {
                                        if (nr_line == 1)
                                        {
                                            if (pos == 2) trafos[elem_pos, trafos_PROP_P1] = s;
                                            if (pos == 3) trafos[elem_pos, trafos_PROP_Q1] = s;
                                            if (pos == 4) trafos[elem_pos, trafos_PROP_P2] = s;
                                            if (pos == 5) trafos[elem_pos, trafos_PROP_Q2] = s;
                                            if (pos == 6) trafos[elem_pos, trafos_PROP_P3] = s;
                                            if (pos == 7) trafos[elem_pos, trafos_PROP_Q3] = s;
                                            if (pos == 8) trafos[elem_pos, trafos_PROP_P4] = s;
                                            if (pos == 9) trafos[elem_pos, trafos_PROP_Q4] = s;
                                        }
                                    if (pos == 2) trafos_values_set[elem_pos, trafos_PROP_P1, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 3) trafos_values_set[elem_pos, trafos_PROP_Q1, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 4) trafos_values_set[elem_pos, trafos_PROP_P2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 5) trafos_values_set[elem_pos, trafos_PROP_Q2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 6) trafos_values_set[elem_pos, trafos_PROP_P3, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 7) trafos_values_set[elem_pos, trafos_PROP_Q3, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 8) trafos_values_set[elem_pos, trafos_PROP_P4, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 9) trafos_values_set[elem_pos, trafos_PROP_Q4, nr_line + LPs_scenarios_multi_LF_start] = s;

                                }
                            }
                            if (elemtype == "generator")
                            {
                                    if (monitors[mon_pos, monitors_PROP_mode] == "0") // fisierul contine tensiuni pe fiecare faza
                                    {
                                        if (nr_line == 1)
                                        {
                                        if (pos == 2) generators[elem_pos, generators_PROP_U1] = s;
                                        if (pos == 3) generators[elem_pos, generators_PROP_U1fi] = s;
                                        if (pos == 4) generators[elem_pos, generators_PROP_U2] = s;
                                        if (pos == 5) generators[elem_pos, generators_PROP_U2fi] = s;
                                        if (pos == 6) generators[elem_pos, generators_PROP_U3] = s;
                                        if (pos == 7) generators[elem_pos, generators_PROP_U3fi] = s;
                                        if (pos == 8) generators[elem_pos, generators_PROP_I1] = s;
                                        if (pos == 9) generators[elem_pos, generators_PROP_I1fi] = s;
                                        if (pos == 10) generators[elem_pos, generators_PROP_I2] = s;
                                        if (pos == 11) generators[elem_pos, generators_PROP_I2fi] = s;
                                        if (pos == 12) generators[elem_pos, generators_PROP_I3] = s;
                                        if (pos == 13) generators[elem_pos, generators_PROP_I3fi] = s;
                                        }
                                    }
                                    if (monitors[mon_pos, monitors_PROP_mode] == "1") // Module results P1,2,3, Q1,2,3
                                    {
                                        if (nr_line == 1) // first line=0 in the csv file has the header, line 1 has first set of results
                                        {
                                            if (pos == 2) generators[elem_pos, generators_PROP_P1] = s;
                                            if (pos == 3) generators[elem_pos, generators_PROP_Q1] = s;
                                            if (pos == 4) generators[elem_pos, generators_PROP_P2] = s;
                                            if (pos == 5) generators[elem_pos, generators_PROP_Q2] = s;
                                            if (pos == 6) generators[elem_pos, generators_PROP_P3] = s;
                                            if (pos == 7) generators[elem_pos, generators_PROP_Q3] = s;
                                        }
                                        if (pos == 2) generators_values_set[elem_pos, generators_PROP_P1, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 3) generators_values_set[elem_pos, generators_PROP_Q1, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 4) generators_values_set[elem_pos, generators_PROP_P2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 5) generators_values_set[elem_pos, generators_PROP_Q2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 6) generators_values_set[elem_pos, generators_PROP_P3, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 7) generators_values_set[elem_pos, generators_PROP_Q3, nr_line + LPs_scenarios_multi_LF_start] = s;
                                }
                                if (monitors[mon_pos, monitors_PROP_mode] == "0") // modul U1,2,3,4 I1,2,3,4
                                {
                                    if (nr_line == 1) // the first line, corresponding to the snapshoot
                                    {
                                        if (pos == 2) generators[elem_pos, generators_PROP_U1] = s;
                                        if (pos == 3) generators[elem_pos, generators_PROP_U1fi] = s;
                                        if (pos == 4) generators[elem_pos, generators_PROP_U2] = s;
                                        if (pos == 5) generators[elem_pos, generators_PROP_U2fi] = s;
                                        if (pos == 6) generators[elem_pos, generators_PROP_U3] = s;
                                        if (pos == 7) generators[elem_pos, generators_PROP_U3fi] = s;
                                        if (pos == 8) generators[elem_pos, generators_PROP_U4] = s;
                                        if (pos == 9) generators[elem_pos, generators_PROP_U4fi] = s;
                                        if (pos == 10) generators[elem_pos, generators_PROP_I1] = s;
                                        if (pos == 11) generators[elem_pos, generators_PROP_I1fi] = s;
                                        if (pos == 12) generators[elem_pos, generators_PROP_I2] = s;
                                        if (pos == 13) generators[elem_pos, generators_PROP_I2fi] = s;
                                        if (pos == 14) generators[elem_pos, generators_PROP_I3] = s;
                                        if (pos == 15) generators[elem_pos, generators_PROP_I3fi] = s;
                                        if (pos == 16) generators[elem_pos, generators_PROP_I4] = s;
                                        if (pos == 17) generators[elem_pos, generators_PROP_I4fi] = s;
                                    }
                                    if (pos == 2) generators_values_set[elem_pos, generators_PROP_U1, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 3) generators_values_set[elem_pos, generators_PROP_U1fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 4) generators_values_set[elem_pos, generators_PROP_U2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 5) generators_values_set[elem_pos, generators_PROP_U2fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 6) generators_values_set[elem_pos, generators_PROP_U3, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 7) generators_values_set[elem_pos, generators_PROP_U3fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 8) generators_values_set[elem_pos, generators_PROP_U4, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 9) generators_values_set[elem_pos, generators_PROP_U4fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 10) generators_values_set[elem_pos, generators_PROP_I1, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 11) generators_values_set[elem_pos, generators_PROP_I1fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 12) generators_values_set[elem_pos, generators_PROP_I2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 13) generators_values_set[elem_pos, generators_PROP_I2fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 14) generators_values_set[elem_pos, generators_PROP_I3, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 15) generators_values_set[elem_pos, generators_PROP_I3fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 16) generators_values_set[elem_pos, generators_PROP_I4, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 17) generators_values_set[elem_pos, generators_PROP_I4fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                }
                            }
                            if (elemtype == "load")
                            {
                                    // int loads_attrib_pos = -1;
                                    if (monitors[mon_pos, monitors_PROP_mode] == "0") // fisierul contine tensiuni pe fiecare faza
                                    {
                                        if (nr_line == 1)
                                        {
                                            if (pos == 2) loads[elem_pos, loads_PROP_U1] = s;
                                            if (pos == 3) loads[elem_pos, loads_PROP_U1fi] = s;
                                            if (pos == 4) loads[elem_pos, loads_PROP_U2] = s;
                                            if (pos == 5) loads[elem_pos, loads_PROP_U2fi] = s;
                                            if (pos == 6) loads[elem_pos, loads_PROP_U3] = s;
                                            if (pos == 7) loads[elem_pos, loads_PROP_U3fi] = s;
                                            if (pos == 8) loads[elem_pos, loads_PROP_U4] = s;
                                            if (pos == 9) loads[elem_pos, loads_PROP_U4fi] = s;
                                            if (pos == 10) loads[elem_pos, loads_PROP_I1] = s;
                                            if (pos == 11) loads[elem_pos, loads_PROP_I1fi] = s;
                                            if (pos == 12) loads[elem_pos, loads_PROP_I2] = s;
                                            if (pos == 13) loads[elem_pos, loads_PROP_I2fi] = s;
                                            if (pos == 14) loads[elem_pos, loads_PROP_I3] = s;
                                            if (pos == 15) loads[elem_pos, loads_PROP_I3fi] = s;
                                            if (pos == 16) loads[elem_pos, loads_PROP_I4] = s;
                                            if (pos == 17) loads[elem_pos, loads_PROP_I4fi] = s;
                                        }
                                    if (pos == 2) loads_values_set[elem_pos, loads_PROP_U1, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 3) loads_values_set[elem_pos, loads_PROP_U1fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 4) loads_values_set[elem_pos, loads_PROP_U2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 5) loads_values_set[elem_pos, loads_PROP_U2fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 6) loads_values_set[elem_pos, loads_PROP_U3, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 7) loads_values_set[elem_pos, loads_PROP_U3fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 8) loads_values_set[elem_pos, loads_PROP_U4, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 9) loads_values_set[elem_pos, loads_PROP_U4fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 10) loads_values_set[elem_pos, loads_PROP_I1, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 11) loads_values_set[elem_pos, loads_PROP_I1fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 12) loads_values_set[elem_pos, loads_PROP_I2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 13) loads_values_set[elem_pos, loads_PROP_I2fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 14) loads_values_set[elem_pos, loads_PROP_I3, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 15) loads_values_set[elem_pos, loads_PROP_I3fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 16) loads_values_set[elem_pos, loads_PROP_I4, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 17) loads_values_set[elem_pos, loads_PROP_I4fi, nr_line + LPs_scenarios_multi_LF_start] = s;
                                }
                                if (monitors[mon_pos, monitors_PROP_mode] == "1") // fisierul contine puteri P si Q pe fiecare faza
                                 {
                                        if (nr_line == 1)
                                        {
                                            if (pos == 2) loads[elem_pos, loads_PROP_P1] = s;
                                            if (pos == 4) loads[elem_pos, loads_PROP_P2] = s;
                                            if (pos == 6) loads[elem_pos, loads_PROP_P3] = s;
                                            if (pos == 3) loads[elem_pos, loads_PROP_Q1] = s;
                                            if (pos == 5) loads[elem_pos, loads_PROP_Q2] = s;
                                            if (pos == 7) loads[elem_pos, loads_PROP_Q3] = s;
                                        }
                                    if (pos == 2) loads_values_set[elem_pos, loads_PROP_P1, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 3) loads_values_set[elem_pos, loads_PROP_Q1, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 4) loads_values_set[elem_pos, loads_PROP_P2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 5) loads_values_set[elem_pos, loads_PROP_Q2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 6) loads_values_set[elem_pos, loads_PROP_P3, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    if (pos == 7) loads_values_set[elem_pos, loads_PROP_Q3, nr_line + LPs_scenarios_multi_LF_start] = s;
                                }
                            }
                                pos++;
                            }

                        nr_line++;
                        lines_values_set_no = nr_line;
                    }
                }
                catch { // we have errors in reading the file
                    richTextBox_console2.Text += "File I/O: " + file_export + "\n";
                }
            }
        }

    }
}