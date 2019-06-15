/*
 * Grid-MonK is an open source software application intended to annalize power grids, esepcially microgrids
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
            richTextBox_console2.Text = calculate_grid_data1_string;
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
            if (X0_shift > Electrical_scheme_zone_delta_X0 * (3+4) / 4) X0_shift = Electrical_scheme_zone_delta_X0 * (3+4) / 4;
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

        private void configInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void basicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Basic reports of the project database, obtained from the GridMonK menu
            string s1 = "";
            for (int n1 = 0; n1 < nodes_no; n1++)
            {
                s1 += "Node=" + n1.ToString("000") +"\t";
                s1 += "Bus=" + nodes[n1, nodes_PROP_bus] + "\t";
                s1 += "Connected_objects=" + nodes[n1, nodes_PROP_list_of_connected_objects] + "\t";
                s1 += "\n";
            }

            GridMonk2OpenDSS_grid_file = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + "GridMonK_Project_DB_Basic.txt";
            File.WriteAllText(GridMonk2OpenDSS_grid_file, s1);

        }

        private void reloadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _GridMonK_GUI_refresh = "Stop";
            read_config_file();
            Console_Training_Ini();
            Load_project();
            _GridMonK_GUI_refresh = "Refresh";

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

            // Delete previous results
            var dir = new DirectoryInfo(Grid_Projects_Path + @"/" + GridMonk_Project + @"/");
            foreach (var file in dir.EnumerateFiles("*.csv"))
            {
                file.Delete();
            }
            //System.IO.File.Delete(Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + "*.csv");

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

            // calculate synthetic data (lossess, totalk powers etc.) for each scenario
            calculate_grid_data_scenarios_array();
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

    }
}