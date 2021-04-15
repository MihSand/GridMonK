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

            public String MkLines()
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

            public void ReadLines(String lns)
            {
                try
                {
                    var serializer = new JsonSerializer();
                    serializer.Populate(new JsonTextReader(new StringReader(lns)), this.lines);
                }
                catch { Console.WriteLine("ReadLines-Err1"); } // da eroare daca nu exisat nici o polilinie
            }

            public String MkString()
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
        string GridMonk_SCADA_information = "SCADA_links.txt";
        string Console_Training_log = "Training_log.txt";
        string OpenDSS_file = "";

        public void Read_config_file()
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(Grid_Monk_Conf);
                Write_GridMonK_log("Grid_Monk_Conf readout ["+ Grid_Monk_Conf + "]");
                foreach (string line in lines)
                {
                    if (line != "") if (line[0] != '#')
                    {
                        char[] delimiterChars = { '=', '[', ';' };
                        string[] line1 = line.Split(delimiterChars);
                        if (line1[0] == "OpenDSS_Path")
                        {
                            OpenDSS_Path = line1[1];
                            Write_GridMonK_log("OpenDSS_Path [" + OpenDSS_Path + "]");
                        }
                        if (line1[0] == "Grid_Projects_Path")
                        {
                            Grid_Projects_Path = line1[1];
                            Write_GridMonK_log("Grid_Projects_Path [" + Grid_Projects_Path + "]");
                        }
                        if (line1[0] == "GridMonk_Project")
                        {
                            GridMonk_Project = line1[1];
                            Write_GridMonK_log("GridMonk_Project [" + GridMonk_Project + "]");
                        }
                        if (line1[0] == "GUI_Language")
                        {
                            GUI_Language = line1[1];
                            Write_GridMonK_log("GUI_Language [" + GUI_Language + "]");
                        }

                        // MQTT links can be connected to a specific layout for each project, currently S4G and Wisegrid
                        // The variable "MQTT_Connect" can have at the moment the following possible values: "WiseGrid" or "S4G"
                        if (line1[0] == "MQTT_Connect")
                        {
                            MQTT_Connect = line1[1];
                            Write_GridMonK_log("MQTT_Connect [" + MQTT_Connect + "]");
                        }

                        if (line1[0] == "MQTT_broker_std1")
                        {
                            MQTT_broker_std1 = line1[1];
                            Write_GridMonK_log("MQTT_broker_std1 [" + MQTT_broker_std1 + "]");
                        }
                        if (line1[0] == "MQTT_broker_user1")
                        {
                            MQTT_broker_user1 = line1[1];
                            Write_GridMonK_log("MQTT_broker_user1 [" + MQTT_broker_user1 + "]");
                        }
                        if (line1[0] == "MQTT_broker_password1")
                        {
                            MQTT_broker_password1 = line1[1];
                            Write_GridMonK_log("MQTT_broker_password1 [" + MQTT_broker_password1 + "]");
                        }
                        if (line1[0] == "MQTT_broker_std1_subscribe_topic")
                        {
                            MQTT_broker_std1_subscribe_topic = line1[1];
                            Write_GridMonK_log("MQTT_broker_std1_subscribe_topic [" + MQTT_broker_std1_subscribe_topic + "]");
                        }

                        if (line1[0] == "MQTT_broker_std2")
                        {
                            MQTT_broker_std2 = line1[1];
                            Write_GridMonK_log("MQTT_broker_std2 [" + MQTT_broker_std2 + "]");
                        }
                        if (line1[0] == "MQTT_broker_user2")
                        {
                            MQTT_broker_user2 = line1[1];
                            Write_GridMonK_log("MQTT_broker_user2 [" + MQTT_broker_user2 + "]");
                        }
                        if (line1[0] == "MQTT_broker_password2")
                        {
                            MQTT_broker_password2 = line1[1];
                            Write_GridMonK_log("MQTT_broker_password2 [" + MQTT_broker_password2 + "]");
                        }
                        if (line1[0] == "MQTT_broker_std2_subscribe_topic")
                        {
                            MQTT_broker_std2_subscribe_topic = line1[1];
                            Write_GridMonK_log("MQTT_broker_std2_subscribe_topic [" + MQTT_broker_std2_subscribe_topic + "]");
                        }

                        if (line1[0] == "GridMonk_SCADA_information")
                        {
                            GridMonk_SCADA_information = line1[1];
                            Write_GridMonK_log("GridMonk_SCADA_information [" + GridMonk_SCADA_information + "]");
                        }
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
                Write_GridMonK_log("Grid_Monk_Conf=Error1");
                Console.WriteLine("Read_config_file-Err1");
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


            string jpg_files = "";
            try
            {
                Write_GridMonK_log("Read jpg files");
                button_right_end.BackgroundImage = Image.FromFile("arrow_right2.jpg"); jpg_files += "arrow_right2.jpg;";
                button_right_end.BackgroundImageLayout = ImageLayout.Stretch; 
                button_right_wnd.BackgroundImage = Image.FromFile("arrow_right1.jpg"); jpg_files += "arrow_right1.jpg;";
                button_right_wnd.BackgroundImageLayout = ImageLayout.Stretch; 
                button_left_wnd.BackgroundImage = Image.FromFile("arrow_left1.jpg"); jpg_files += "arrow_left1.jpg;";
                button_left_wnd.BackgroundImageLayout = ImageLayout.Stretch; 
                button_up_wnd.BackgroundImage = Image.FromFile("arrow_up.jpg"); jpg_files += "arrow_up.jpg;";
                button_up_wnd.BackgroundImageLayout = ImageLayout.Stretch; 
                button_down_wnd.BackgroundImage = Image.FromFile("arrow_down.jpg"); jpg_files += "arrow_down.jpg;";
                button_down_wnd.BackgroundImageLayout = ImageLayout.Stretch; 
                button_zoom_in.BackgroundImage = Image.FromFile("zoom_in.jpg"); jpg_files += "zoom_in.jpg;";
                button_zoom_in.BackgroundImageLayout = ImageLayout.Stretch; 
                button_zoom_out.BackgroundImage = Image.FromFile("zoom_out.jpg"); jpg_files += "zoom_out.jpg;";
                button_zoom_out.BackgroundImageLayout = ImageLayout.Stretch;
                button_center.BackgroundImage = Image.FromFile("Button_center.jpg"); jpg_files += "Button_center.jpg;";
                button_center.BackgroundImageLayout = ImageLayout.Stretch;
                Write_GridMonK_log("Read jpg files complete [" + jpg_files + "]");
            }
            catch
            {
                Write_GridMonK_log("Error in reading jpg files, could be read = [" + jpg_files +"]");
                Console.WriteLine("Error in reading jpg files-Err2");
            }

            // initialze PROC modules
            Write_GridMonK_log("Grid_Monk_Conf=initialze_PROC_modules");
            try
            {
                PROC_ini_S4G();
            }
            catch
            {
                Write_GridMonK_log("PROC_ini_S4G=Error");
                Console.WriteLine("PROC_ini_S4G-Err3");
            }
            Write_GridMonK_log("Grid_Monk_Conf=initialze_PROC_modules=S4G=Finished");
            try
            {
                PROC_ini_Wisegrid();
            }
            catch
            {
                Write_GridMonK_log("PROC_ini_Wisegrid=Error");
            }
            Write_GridMonK_log("Grid_Monk_Conf=initialze_PROC_modules=WSG=Finished");
            try
            {
                PROC_ini_SCADA_FEP();
            }
            catch
            {
                Write_GridMonK_log("PROC_ini_SCADA_FEP=Error");
            }
            Write_GridMonK_log("Grid_Monk_Conf=initialze_PROC_modules=SCADA=Finished");
        }

        int SCADA_nodes_number = 0;
        int SCADA_lines_number = 0;
        public void Read_SCADA_file()
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + GridMonk_SCADA_information);
                SCADA_nodes_number = 0;
                SCADA_lines_number = 0;
                foreach (string line in lines)
                {
                    if (line[0] != '#')
                    {
                        char[] delimiterChars1 = { '\t', '&', ' ' };
                        string[] line1 = line.Split(delimiterChars1);
                        char[] delimiterChars2 = { '=' };
                        string[] line2 = line1[0].Split(delimiterChars2);
                        if ((line2[0] == "object") && (line2[1] == "node"))
                        {
                            char[] delimiterChars3 = { ';' };
                            string[] line3 = line1[1].Split(delimiterChars3);
                            foreach (string line4 in line3)
                            {
                                string[] line5 = line4.Split(delimiterChars2);
                                if (line5[0] == "Input_node_name") Input_node_name[SCADA_nodes_number] = line5[1];
                                if (line5[0] == "GridMonK_node_number") GridMonK_node_number[SCADA_nodes_number] = int.Parse(line5[1]);
                                if (line5[0] == "GridMonK_node_name") GridMonK_node_name[SCADA_nodes_number] = line5[1];

                            }
                            SCADA_nodes_number++;
                        }

                        if ((line2[0] == "object") && (line2[1] == "line"))
                        {
                            char[] delimiterChars3 = { ';' };
                            string[] line3 = line1[1].Split(delimiterChars3);
                            foreach (string line4 in line3)
                            {
                                string[] line5 = line4.Split(delimiterChars2);
                                if (line5[0] == "Input_node_name1") Input_line_node1_name[SCADA_lines_number] = line5[1];
                                if (line5[0] == "Input_node_name2") Input_line_node2_name[SCADA_lines_number] = line5[1];
                                if (line5[0] == "GridMonK_line_number") GridMonK_line_number[SCADA_lines_number] = int.Parse(line5[1]);
                                if (line5[0] == "GridMonK_line_name") GridMonK_line_name[SCADA_lines_number] = line5[1];

                            }
                            SCADA_lines_number++;
                        }
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
                Console.WriteLine("Read_SCADA_file-Err1");
            }
        }

        public void Write_GridMonK_log(string text1)
        {
            string s1 = "";
            DateTime t1 = DateTime.Now;
            s1 = t1.Year.ToString() + "." + t1.Month.ToString() + "." + t1.Day.ToString()
                + "\t" + t1.Hour.ToString("00") + ":" + t1.Minute.ToString("00") + ":" + t1.Second.ToString("00") + ":" + t1.Millisecond.ToString("000");
            string file_LP_recording = t1.Year.ToString() + "." + t1.Month.ToString() + "." + t1.Day.ToString() + "." + "GridMonK_log.txt";
            s1 += "\t" + text1 + "\n";
            File.AppendAllText(file_LP_recording, s1);
        }

        public GridMonk()
        {
            Write_GridMonK_log("############################");
            Write_GridMonK_log("#GridMonk=" + "Start");
            InitializeComponent();
            Write_GridMonK_log("InitializeComponent = Done");
            this.DoubleBuffered = true;
            //this..MaximizeBox();
            polylines = new List<Poly>();

            Read_config_file();
            Write_GridMonK_log("read_config_file = Done");
            //read_SCADA_file();
            Console_Training_Ini();

            // prepare MQTT connectors
            // MQTT Client 1
            Write_GridMonK_log("Start Initialize MQTT link 1");
            try
            {
                client1 = new MqttClient(MQTT_broker_std1);
                if((MQTT_broker_user1=="") && (MQTT_broker_password1=="")) { 
                    code = client1.Connect(Guid.NewGuid().ToString());
                    Write_GridMonK_log("MQTT_broker1=Connected, NO username and password; Code="+code.ToString());
                }
                else { 
                    code = client1.Connect(Guid.NewGuid().ToString(), MQTT_broker_user1, MQTT_broker_password1);
                    Write_GridMonK_log("MQTT_broker1=Connected, WITH username and password; Code=" + code.ToString());
                }

                client1.MqttMsgPublished += Client_MqttMsgPublished;
                client1.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

                MQTT_subscribe("#", 1);
                //MQTT_subscribe(MQTT_broker_std1_subscribe_topic, 1);
            }
            catch
            {
                Console.WriteLine("Error initialising MQTT Client 1");
                Write_GridMonK_log("Error initialising MQTT Client 1");
            }
            // MQTT Client 2
            Write_GridMonK_log("Start Initialize MQTT link 2");
            try
            {
                client2 = new MqttClient(MQTT_broker_std1);
                if ((MQTT_broker_user2 == "") && (MQTT_broker_password2 == ""))
                {
                    code = client2.Connect(Guid.NewGuid().ToString());
                    Write_GridMonK_log("MQTT_broker2=Connected, NO username and password; Code=" + code.ToString());
                }
                else
                {
                    code = client2.Connect(Guid.NewGuid().ToString(), MQTT_broker_user2, MQTT_broker_password2);
                    Write_GridMonK_log("MQTT_broker2=Connected, WITH username and password; Code=" + code.ToString());
                }

                client2.MqttMsgPublished += Client_MqttMsgPublished;
                client2.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

                MQTT_subscribe("#", 2);
                Console.WriteLine("MQTT Client 2 is listening");
            }
            catch
            {
                Console.WriteLine("Error initialising MQTT Client 2");
                Write_GridMonK_log("Error initialising MQTT Client 2");
            }

            Write_GridMonK_log("Initialize variables");
            for (int i1 = 0; i1 < lines_MAX; i1++) for (int j1 = 0; j1 < lines_prop_MAX; j1++)
                    for (int k1 = 0; k1 < historical_values_depth_MAX; k1++) lines_values_set[i1, j1, k1] = "";
            for (int i1 = 0; i1 < historical_values_depth_MAX; i1++) { Congestions[i1] = ""; Congestions_old[i1] = ""; }
            for (int i1 = 0; i1 < graph_smallgph_MAX; i1++) for (int j1 = 0; j1 < graph_smallgph_prop_MAX; j1++) graph_smallgph[i1, j1] = "";
            for (int i1 = 0; i1 < scenarios_prop_MAX; i1++)
                for (int j1 = 0; j1 < historical_values_depth_MAX; j1++)
                {
                    scenarios[i1, j1] = "";
                }

            Gph_phasors_alloc(); // space allocation for gph_phasors
            Write_GridMonK_log("Done initialization");
        }

        int even_second = 0;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            DateTime t1 = DateTime.Now;
            textBox_DateTime.Text = t1.Year.ToString() + "-" + "-" + t1.Month.ToString() + "-" + t1.Day.ToString()
                + "    " + t1.Hour.ToString() + ":" + t1.Minute.ToString() + ":" + t1.Second.ToString();
            if ((t1.Second % 2) == 0) even_second = 1; else even_second = 0;
            Angle_real_time += (grid_frequency - 50) * 50;
            if(Angle_real_time>360) Angle_real_time += -360; // this angle is for simulating PMU phasors rotation
            //textBox_timeframe_crt.Text = Timeframe_crt_str;

            // read MQTT data 
            MQTT_broker_broker_crt = 1;
            //MQTT_publish("Time", "a" + t1.ToLongDateString(), MQTT_broker_broker_crt);
            MqttClient2_str_in = message_received;
            //textBox_remove.Text = message_received;

            // LP_recording();

            Refresh();
        }

        private void Timer2_MQTT_pub_Tick(object sender, EventArgs e)
        {
            // data sent as JSON messages
            string s3 = "50.03", s4 = "1.37";
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
        int object_x0 = 0, object_y0 = 0; // variables which point the coordonates of the currently painted object
        int object_x1 = 0, object_y1 = 0; // used for nodes (or other objects) having multi-positions
        int object_x2 = 0, object_y2 = 0; // used for nodes (or other objects) having multi-positions

        int obj_number = 0;

        int crtnr = 0;
        /*private void Button_Start_Click(object sender, EventArgs e)
        {
            drawingPolyline = true;
            polylines.Add(new Poly("n" + crtnr.ToString()));
            crtnr++;
        }*/
        /*private void Button_Stop_Click(object sender, EventArgs e)
        {
            drawingPolyline = false;
        }*/

        //int x_mouse_crt = 0, y_mouse_crt = 0;
        private int Inside_rect(int x1, int y1, int x2, int y2, int px, int py)
        { // if [px, py] are inside the rectangle [x1,y1,x2,y2], it returns 1, else 0
            int res = 0;
            if ((x1 <= px) && (x2 >= px) && (y1 <= py) && (y2 >= py)) res = 1;
            return res;
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            textBox_mouse_xy.Text = " (" + e.X.ToString() + ", " + e.Y.ToString() + ")";
            int draw_properties = 1;
            Gph_element_identification(e.X, e.Y, draw_properties);

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

        private void Button_Next_timeframe_Click(object sender, EventArgs e)
        {

        }

        private void Button_Prev_timeframe_Click(object sender, EventArgs e)
        {
            // move to previous timeframe
            //int Timeframe_crt = 1;
            if(Timeframe_crt_str == "Base") Timeframe_crt = 1;
            else Timeframe_crt--;
            if (Timeframe_crt < 1) Timeframe_crt = 1;
            Timeframe_crt_str = Timeframe_crt.ToString();
            textBox_S_max_U_stability.Text = Timeframe_crt_str;

        }

        private void GridSummaryReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Calculate_grid_data1();
            richTextBox_console2.Text = calculate_grid_data1_string;
            Refresh();
        }

        private void Button_left_wnd_Click(object sender, EventArgs e)
        {
            //   <<==
            X0_shift = X0_shift - Electrical_scheme_zone_delta_X0 / 4;
            if (X0_shift < -Electrical_scheme_zone_delta_X0 * 3 / 4) X0_shift = -Electrical_scheme_zone_delta_X0 * 3 / 4;
        }

        private void Button_right_wnd_Click(object sender, EventArgs e)
        {
            //   ==>>
            X0_shift = X0_shift + Electrical_scheme_zone_delta_X0 / 4;
            if (X0_shift > Electrical_scheme_zone_delta_X0 * (3+4+4+4+4) / 4) X0_shift = Electrical_scheme_zone_delta_X0 * (3+4+4+4+4) / 4;
        }

        private void Button_up_wnd_Click(object sender, EventArgs e)
        {
            //   ▲ Up
            Y0_shift = Y0_shift - Electrical_scheme_zone_delta_Y0 / 4;
            if (Y0_shift < -Electrical_scheme_zone_delta_Y0 * 3 / 4) Y0_shift = -Electrical_scheme_zone_delta_Y0 * 3 / 4;

        }

        private void Button_down_wnd_Click(object sender, EventArgs e)
        {
            //   ▼ Down
            Y0_shift = Y0_shift + Electrical_scheme_zone_delta_Y0 / 4;
            if (Y0_shift > Electrical_scheme_zone_delta_Y0 * 3 / 4) Y0_shift = Electrical_scheme_zone_delta_Y0 * 3 / 4;

        }

        private void Button_center_Click(object sender, EventArgs e)
        {
            X0_shift = 0; Y0_shift = 0;
        }

        private void Button_zoom_out_Click(object sender, EventArgs e)
        {
            zoom = zoom / 2;
        }

        private void Button_zoom_in_Click(object sender, EventArgs e)
        {
            zoom = zoom * 2;

        }
        private void ConfigInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void BasicToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void Button_Forecast_Analysis_Click(object sender, EventArgs e)
        {
            if (GridMonK_Congestion_Forecast_mode == 0) GridMonK_Congestion_Forecast_mode = 1; // we switch to forecast mode
            else GridMonK_Congestion_Forecast_mode = 0;
            if (GridMonK_Congestion_Forecast_mode == 1)
            {
                button_Forecast_Analysis.ForeColor = Color.Red;
            }
            else button_Forecast_Analysis.ForeColor = Color.Black;
        }

        //private void Button_Load_file_Click(object sender, EventArgs e)
        //{

        //}

        private void Button_export_JSON_Click(object sender, EventArgs e)
        {
            Export_JSON_Wisegrid();
        }

        private void Button_right_end_Click(object sender, EventArgs e)
        {
            X0_shift = Electrical_scheme_zone_delta_X0 * 17 / 4;
        }

        private void Button_export_JSON2_Click(object sender, EventArgs e)
        {
            Button_export_JSON_Click(sender, e);
        }

        private void GridMonk_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                client1.Disconnect();
                client2.Disconnect();
            }
            catch
                {
                    Console.WriteLine("Error disconnecting MQTT Client 1");
                }

        }

        bool _mousePressed = false;
        int _mousePressed_x = 0; // memorare pozitie X mouse cand se face click
        int _mousePressed_y = 0; // memorare pozitie Y mouse cand se face click
        int dx_mousePressed_x = 0;
        int dy_mousePressed_y = 0;
        private void GridMonk_MouseMove(object sender, MouseEventArgs e)
        {
            //richTextBox_console1.Text = " (" + e.X.ToString() + ", " + e.Y.ToString() + ")";
            //Gph_element_identified = "";
            //Gph_element_identification(e.X, e.Y, 0); // 0: no draw_param, 1: draw_param

            dx_mousePressed_x = e.X - _mousePressed_x;
            dy_mousePressed_y = e.Y - _mousePressed_y;
            if ((_mousePressed == true) && (Gph_element_identified == "graph"))
            {
                //graphs[0, graphs_PROP_graph_shaddow] = "1";
            }
        }

        private void GridMonk_MouseDown(object sender, MouseEventArgs e)
        {
            //graphs[0, graphs_PROP_graph_shaddow] = "1"; // shadow of graphs is ON
            _mousePressed = true;
            Gph_element_identified = "";
            Gph_element_identification(e.X, e.Y, 0); // 0: no draw_param, 1: draw_param
            _mousePressed_x = e.X;
            _mousePressed_y = e.Y;
        }

        private void GridMonk_MouseUp(object sender, MouseEventArgs e)
        {
            // cand mouse-ul revine
            _mousePressed = false;

            dx_mousePressed_x = e.X - _mousePressed_x;
            if (Math.Abs(dx_mousePressed_x) < 3) dx_mousePressed_x = 0; // desensibilizare pentru mici drag-uri
            dy_mousePressed_y = e.Y - _mousePressed_y;
            if (Math.Abs(dy_mousePressed_y) < 3) dy_mousePressed_y = 0; // desensibilizare pentru mici drag-uri

            /*
            graphs[0, graphs_PROP_graph_shaddow] = "0"; // shadow if graph is OFF
            
            // the new position of x and y are memorised in the graph object
            int x_gph = int.Parse(graphs[0, graphs_PROP_x0]) + dx_mousePressed_x;
            if (dx_mousePressed_x != 0) graphs[0, graphs_PROP_x0] = x_gph.ToString();

            int y_gph = int.Parse(graphs[0, graphs_PROP_y0]) + dy_mousePressed_y;
            if (dy_mousePressed_y != 0) graphs[0, graphs_PROP_y0] = y_gph.ToString();
            */
        }

        int Profile_no = 0;
        string GridMonk_RunScript_Out_file1 = "";
        string GridMonk_RunScript_Out_file2 = "";
        string GridMonk_RunScript_Out_scaled_file = "";
        string GridMonK_Script_Data_log_file = "GridMonK_Script_Data_log.txt";
        const int Profile_no_MAX = 500;
        const int Profile_Variable_MAX = 100;
        string[,] Script_Data = new string[Profile_no_MAX, Profile_Variable_MAX]; // 100 situatii (profile), max 16 variabile 
        string s_Out_file2 = "";

        private void ScriptResults_Out()
        {
            // Here are written the results for a certain script data
            DateTime t1 = DateTime.Now;
            string s1 = "", sx = "", s_line0 = "";
            string Timestamp1 = "";

            richTextBox_console_cd.Text = "Profile_no=" + Profile_no.ToString();

            Timestamp1 = ">DateTime=" + t1.Year.ToString("##00") + "-" + t1.Month.ToString("00") + "-" + t1.Day.ToString("00") + "_";
            Timestamp1 += t1.Hour.ToString("00") + "-" + t1.Minute.ToString("00") + "-" + t1.Second.ToString("00");

            // General data
            s1 = Timestamp1 + ";#" + Profile_no.ToString("000") +"\t";
            s_Out_file2 = s1;
            s_line0 = s1;
            s1 += "\nGenData" + "\t" + "Global_PVs_factor=" + Global_PVs_factor.ToString() + "\t";
            s1 += "Global_loads_factor=" + Global_loads_factor.ToString() + "\t";
            // Generator
            s1 += "\nGenerators\t" + generators[0, generators_PROP_Pn] + "\t" + generators[1, generators_PROP_Pn] + "\t";

            // Results as they are
            s1 += "\nNodes(U,fi)" + "\t" + nodes_no.ToString() + "\t";
            for (int i1 = 0; i1 < nodes_no; i1++)
            {
                sx = nodes[i1, nodes_PROP_U1] + "\t" + nodes[i1, nodes_PROP_U1fi] + "\t";
                s1 += sx;
                s_Out_file2 += sx;
                if (Profile_no == 0) s_line0 += "node." + i1.ToString() + "." + "U1" + "\t" + "node." + i1.ToString() + "." + "fi1" + "\t";
            }
            s1 += "\nGener(P,Q)" + "\t" + generators_no.ToString() + "\t";
            for (int i1 = 0; i1 < generators_no; i1++)
            {
                sx = generators[i1, generators_PROP_P] + "\t" + generators[i1, generators_PROP_Q] + "\t";
                s1 += sx;
                s_Out_file2 += sx;
                if (Profile_no == 0) s_line0 += "generator." + i1.ToString() + "." + "P" + "\t" + "generator." + i1.ToString() + "." + "Q" + "\t";
            }
            s1 += "\nLines(P,Q)" + "\t" + lines_no.ToString() + "\t";
            for (int i1 = 0; i1 < lines_no; i1++)
            {
                sx = lines[i1, lines_PROP_P] + "\t" + lines[i1, lines_PROP_Q] + "\t";
                s1 += sx;
                s_Out_file2 += sx;
                if (Profile_no == 0) s_line0 += "line." + i1.ToString() + "." + "P" + "\t" + "line." + i1.ToString() + "." + "Q" + "\t";
            }
            s1 += "\nLines(_I_)" + "\t" + lines_no.ToString() + "\t";
            for (int i1 = 0; i1 < lines_no; i1++)
            {
                sx = lines[i1, lines_PROP_I1] + "\t";
                s1 += sx;
                s_Out_file2 += sx;
                if (Profile_no == 0) s_line0 += "line." + i1.ToString() + "." + "I" + "\t";
            }
            s1 += "\nloads(P,Q)" + "\t" + loads_no.ToString() + "\t";
            for (int i1 = 0; i1 < loads_no; i1++)
            {
                sx = loads[i1, loads_PROP_P] + "\t" + loads[i1, loads_PROP_Q] + "\t";
                s1 += sx;
                s_Out_file2 += sx;
                if (Profile_no == 0) s_line0 += "load." + i1.ToString() + "." + "P" + "\t" + "load." + i1.ToString() + "." + "Q" + "\t";
            }
            s1 += "\nLnBrk(0,1)" + "\t" + lines_no.ToString() + "\t";
            for (int i1 = 0; i1 < lines_no; i1++)
            {
                if (lines[i1, lines_PROP_brk1] == "on") sx = "1" + "\t";
                else sx = "0" + "\t";
                s1 += sx;
                s_Out_file2 += sx;
                if (Profile_no == 0) s_line0 += "line." + i1.ToString() + "." + "brk1" + "\t";
            }
            s1 += "\n";
            s_Out_file2 += "\n";
            s_line0 += "\n";

            // GridMonk2OpenDSS_grid_file = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + "RunScript_Out.csv";
            // File.WriteAllText(GridMonk2OpenDSS_grid_file, s1);
            File.AppendAllText(GridMonk_RunScript_Out_file1, s1);
            if (Profile_no == 0) File.AppendAllText(GridMonk_RunScript_Out_file2, s_line0);
            File.AppendAllText(GridMonk_RunScript_Out_file2, s_Out_file2);

            // Scaled Results for ANN
            s1 = "";
            if (Profile_no == 0) {
                // se scrie cap de tabel
                s1 = Timestamp1 + ";#" + Profile_no.ToString("000") + "\t";
                for (int i1 = 0; i1 < 70; i1++) s1 += i1.ToString("000000") + "\t";
                s1 += "\n";
            }
            s1 += Timestamp1 + ";#" + Profile_no.ToString("000") + "\t";
            double scaled, value;
            // Nodes(U,fi)
            double min1 = 119500, max1 = 146100, min2 = -20, max2 = 20;
            for (int i1 = 0; i1 < nodes_no; i1++)
            {
                value = Double.Parse(nodes[i1, nodes_PROP_U1]);
                scaled = (value - min1) / (max1 - min1);
                s1 += scaled.ToString("#0.0000") + "\t";
                value = Double.Parse(nodes[i1, nodes_PROP_U1fi]);
                scaled = (value - min2) / (max2 - min2);
                s1 += scaled.ToString("#0.0000") + "\t";
            }
            // Gener(P,Q)
            min1 = -200000; max1 = 200000; min2 = -200000; max2 = 200000;
            for (int i1 = 0; i1 < generators_no; i1++)
            {
                value = Double.Parse(generators[i1, generators_PROP_P]);
                scaled = (value - min1) / (max1 - min1);
                s1 += scaled.ToString("#0.0000") + "\t";
                value = Double.Parse(generators[i1, generators_PROP_Q]);
                scaled = (value - min2) / (max2 - min2);
                s1 += scaled.ToString("#0.0000") + "\t";
            }
            // Lines(P,Q)
            min1 = -200000; max1 = 200000; min2 = -200000; max2 = 200000;
            for (int i1 = 0; i1 < lines_no; i1++)
            {
                value = Double.Parse(lines[i1, lines_PROP_P]);
                scaled = (value - min1) / (max1 - min1);
                s1 += scaled.ToString("#0.0000") + "\t";
                value = Double.Parse(lines[i1, lines_PROP_Q]);
                scaled = (value - min2) / (max2 - min2);
                s1 += scaled.ToString("#0.0000") + "\t";
            }
            // Lines(_I_)
            min1 = -400; max1 = 400;
            for (int i1 = 0; i1 < lines_no; i1++)
            {
                value = Double.Parse(lines[i1, lines_PROP_I1]);
                scaled = (value - min1) / (max1 - min1);
                s1 += scaled.ToString("#0.0000") + "\t";
            }
            // Loads(P,Q)
            min1 = -200000; max1 = 200000; min2 = -200000; max2 = 200000;
            for (int i1 = 0; i1 < loads_no; i1++)
            {
                value = Double.Parse(loads[i1, loads_PROP_P]);
                scaled = (value - min1) / (max1 - min1);
                s1 += scaled.ToString("#0.0000") + "\t";
                value = Double.Parse(loads[i1, loads_PROP_Q]);
                scaled = (value - min2) / (max2 - min2);
                s1 += scaled.ToString("#0.0000") + "\t";
            }
            // Lines(Brk1)
            min1 = 0; max1 = 1;
            for (int i1 = 0; i1 < lines_no; i1++)
            {
                if (lines[i1, lines_PROP_brk1] == "on")
                    s1 += "1.0000" + "\t";
                else s1 += "0.0000" + "\t";
            }
            s1 += "\n";

            //GridMonk_RunScript_Out_scaled_file = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + "RunScript_Out_scaled.csv";
            File.AppendAllText(GridMonk_RunScript_Out_scaled_file, s1);

            // memorare in "GridMonK_Script_Data_log.txt"
            for(int i1=0; i1< nodes_no; i1++)
                Script_Data[Profile_no, i1] = nodes[i1, nodes_PROP_U1];

            Profile_no++;

        }

        int RunScript_enabled = 0; // only if it is 1 the script is enabled; default it is disabled
        int button_RunScript_first_time = 1;
        private void button_RunScript_Click(object sender, EventArgs e)
        {
            // This function will run a script with different load-flow characteristics.

            if (RunScript_enabled == 0) return; // if the script is not enabled, it will return without any change
            // Initialisation
            // Profile_no = 0;
            for (int i1 = 0; i1 < Profile_no_MAX; i1++)
                for (int j1 = 0; j1 < Profile_Variable_MAX; j1++)
                    Script_Data[i1, j1] = "";

            string s1 = "", Timestamp1 = "";
            if(button_RunScript_first_time == 1) { 

                // Initialize the result files
                GridMonk_RunScript_Out_file1 = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + "RunScript_Out_Type1.log";
                GridMonk_RunScript_Out_file2 = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + "RunScript_Out_Type2.log";
                File.WriteAllText(GridMonk_RunScript_Out_file1, s1);
                File.WriteAllText(GridMonk_RunScript_Out_file2, s_Out_file2);

                GridMonk_RunScript_Out_scaled_file = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + "RunScript_Out_scaled.log";
                File.WriteAllText(GridMonk_RunScript_Out_scaled_file, s1);

                button_RunScript_first_time = 0;
            }

            // Running the script (hardwired or from file "GridMonk_script.txt"
            Global_loads_factor = 1;
            double gen_memo1 = Double.Parse(generators[0, generators_PROP_Pn]);
            double gen_memo2 = Double.Parse(generators[1, generators_PROP_Pn]);
            for (int brks=0; brks<=5; brks++) {
                if (brks == 0) // toate liniile conectate
                {
                    lines[3, lines_PROP_brk1] = "on";
                }
                if (brks == 1) // contingenta linia 3
                {
                    // se deconecteaza linia 3
                    lines[3, lines_PROP_brk1] = "off";
                    lines[3, lines_PROP_bus1] = lines[3, lines_PROP_bus1] + "@" + lines[3, lines_PROP_name];
                }
                if (brks == 2) // contingenta linia 1
                {
                    // se reconecteaza brk1 pe linia 3
                    lines[3, lines_PROP_bus1] = lines[3, lines_PROP_bus1].Replace("@" + lines[3, lines_PROP_name], "");
                    lines[3, lines_PROP_brk1] = "on";
                    // se deconecteaza linia 1
                    lines[1, lines_PROP_brk1] = "off";
                    lines[1, lines_PROP_bus1] = lines[1, lines_PROP_bus1] + "@" + lines[1, lines_PROP_name];
                }
                if (brks == 3) // contingenta linia 5
                {
                    // se reconecteaza brk1 pe linia 1
                    lines[1, lines_PROP_bus1] = lines[1, lines_PROP_bus1].Replace("@" + lines[1, lines_PROP_name], "");
                    lines[1, lines_PROP_brk1] = "on";
                    // se deconecteaza linia 5
                    lines[5, lines_PROP_brk1] = "off";
                    lines[5, lines_PROP_bus1] = lines[5, lines_PROP_bus1] + "@" + lines[5, lines_PROP_name];
                }
                if (brks == 4) // contingenta linia 4
                {
                    // se reconecteaza brk1 pe linia 5
                    lines[5, lines_PROP_bus1] = lines[5, lines_PROP_bus1].Replace("@" + lines[5, lines_PROP_name], "");
                    lines[5, lines_PROP_brk1] = "on";
                    // se deconecteaza linia 4
                    lines[4, lines_PROP_brk1] = "off";
                    lines[4, lines_PROP_bus1] = lines[4, lines_PROP_bus1] + "@" + lines[4, lines_PROP_name];
                }
                if (brks == 4) // contingenta linia 2
                {
                    // se reconecteaza brk1 pe linia 4
                    lines[4, lines_PROP_bus1] = lines[4, lines_PROP_bus1].Replace("@" + lines[4, lines_PROP_name], "");
                    lines[4, lines_PROP_brk1] = "on";
                    // se deconecteaza linia 2
                    lines[2, lines_PROP_brk1] = "off";
                    lines[2, lines_PROP_bus1] = lines[2, lines_PROP_bus1] + "@" + lines[2, lines_PROP_name];
                }
                if (brks == 5) // contingenta linia 0
                {
                    // se reconecteaza brk1 pe linia 2
                    lines[2, lines_PROP_bus1] = lines[2, lines_PROP_bus1].Replace("@" + lines[2, lines_PROP_name], "");
                    lines[2, lines_PROP_brk1] = "on";
                    // se deconecteaza linia 0
                    lines[0, lines_PROP_brk1] = "off";
                    lines[0, lines_PROP_bus1] = lines[0, lines_PROP_bus1] + "@" + lines[0, lines_PROP_name];
                }
                for (int i1=-2; i1<=2; i1++) {

                Global_loads_factor = 1.0 + 0.1 * i1;

                double gen_RT;
                for (int j1 = -2; j1 <= 2; j1++)
                {
                    gen_RT = gen_memo1 * (1 + 0.1 * j1);
                    generators[0, generators_PROP_Pn] = gen_RT.ToString(); 
                    Button_Compute_Click(sender, e);
                    // write results from scripting
                    ScriptResults_Out();
                }
                generators[0, generators_PROP_Pn] = gen_memo1.ToString(); // aducere a valorii Gen[0] la valoarea initiala

                for (int j1 = -2; j1 <= 2; j1++)
                {
                    gen_RT = gen_memo2 * (1 + 0.1 * j1);
                    generators[1, generators_PROP_Pn] = gen_RT.ToString();
                    Button_Compute_Click(sender, e);
                    // write results from scripting
                    ScriptResults_Out();
                }
                generators[1, generators_PROP_Pn] = gen_memo2.ToString(); // aducere a valorii Gen[0] la valoarea initiala
            }
                // revenire a ultimei linii
            lines[0, lines_PROP_bus1] = lines[0, lines_PROP_bus1].Replace("@" + lines[0, lines_PROP_name], "");
            lines[0, lines_PROP_brk1] = "on";
            }

            GridMonK_Script_Data_log_file = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + "GridMonK_Script_Data.log";
            s1 = "";
            for (int j1 = 0; j1 < Profile_Variable_MAX; j1++) s1 += "Val_" + j1.ToString("00") + "\t";
                s1 += "\n";
            for (int i1=0; i1<Profile_no; i1++) { 
                for(int j1=0; j1< Profile_Variable_MAX; j1++)
                {
                    s1 += Script_Data[i1, j1] + "\t";
                }
                s1 += "\n";
            }
            File.WriteAllText(GridMonK_Script_Data_log_file, s1);

        }

        private void button_Custom1_Click(object sender, EventArgs e)
        {
            HIL_FEP_action1();
        }

        private void Button_realtime_data_Click(object sender, EventArgs e)
        {
            if (GridMonK_realtime_data_mode == 0) GridMonK_realtime_data_mode = 1;
            else GridMonK_realtime_data_mode = 0;
            if (GridMonK_realtime_data_mode == 1)
            {
                button_realtime_data.ForeColor = Color.Red;
            }
            else button_realtime_data.ForeColor = Color.Black;

        }

        private void Timer3_PROC_Tick(object sender, EventArgs e)
        {
            //timer3_PROC_Wisegrid(sender, e);
            //timer3_PROC_S4G(sender, e);
            {
                timer3_PROC_Wisegrid(sender, e);
                if (activate_S4G_HIL.ToLower() == "yes") //validation check for the HIL module
                {
                    Timer3_PROC_S4G(sender, e);
                }
            }
        }

        private void ReloadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _GridMonK_GUI_refresh = "Stop";
            Read_config_file();
            Console_Training_Ini();
            Load_project();
            _GridMonK_GUI_refresh = "Refresh";

        }

        private void Button_remove_Click(object sender, EventArgs e)
        {
        }
        /*private void Button_remove_Click(object sender, EventArgs e)
        {
            String name = textBox_remove.Text;
            //polylines.Remove(polylines.Find(x => x.name == name));
            textBox_mouse_xy.Text = polylines.LastIndexOf(polylines.Find(x => x.name == name)).ToString();
            richTextBox_console2.Text += polylines.Find(x => x.name == name).MkString() + "\n";
            Refresh();
        }*/

        private void Button_Compute_Click(object sender, EventArgs e)
        {
            Write_GridMonK_log("#Command=" + "Compute=Started");
            DateTime t1 = DateTime.Now;
            string st1 = ">> Grid compute\n";
            string T_ini = "T(ini):" + t1.Year.ToString() + "." + t1.Month.ToString("00") + "." + t1.Day.ToString("00")
                + " " + t1.Hour.ToString("00") + ":" + t1.Minute.ToString("00") + ":" + t1.Second.ToString("00")
                 + "." + t1.Millisecond.ToString("000");
            int t1s = t1.Second, t1ms = t1.Millisecond;
            st1 += T_ini + "\n";
            string richTextBox_console_answers_str = st1;

            Write_GridMonK_log("Start Compute at: " + T_ini);

            richTextBox_console_answers_str += ">> Generate output (";
            Generate_output_dss("multi_LP_RMB_and_24h", "Forecast"); // se salveaza statusul curent al retelei

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

            Assess_nodes_properties(); // in urma valorilor rezultat, asociate la obiecte, se fac calcule legate de noduri
            Add_nodes_properties_from_paramaterisation_nodes_metadata(); // proprietati de noduri din parametrzarea initiala; nu se cheama aceasta component ataunci cand se parcurg cele 24 ore

            Calculate_values_from_results(); // in urma valorilor rezultat, asociate la obiecte, se fac calcule legate de noduri
            Timeframe_crt = -1;

            DateTime t2;
            t2 = DateTime.Now;
            string T_final = "T(end):" + t2.Year.ToString() + "." + t2.Month.ToString("00") + "." + t2.Day.ToString("00")
                            + " " + t2.Hour.ToString("00") + ":" + t2.Minute.ToString("00") + ":" + t2.Second.ToString("00")
                            + "." + t2.Millisecond.ToString("000");
            richTextBox_console_answers_str += T_final + "\n";
            Write_GridMonK_log("Stop Compute at: " + T_final);
            
            // calculate delta T
            int dt_msec = t2.Second * 1000 + t2.Millisecond - t1s * 1000 - t1ms;
            if (dt_msec < 0) dt_msec = dt_msec + 60000;
            richTextBox_console_answers_str += "Total time (msec)= " + dt_msec.ToString();
            richTextBox_console_answers.Text = richTextBox_console_answers_str;
            Write_GridMonK_log("Compute time (msec): " + dt_msec.ToString());

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

            if (GridMonK_Congestion_Forecast_mode == 1)
                for (int interval = 0; interval < 24; interval++)
                {
                    Generate_output_dss("GridMonk_Project_" + interval.ToString("00"), "Snapshoot");  // producere fisier de iesire compatibil dss.
                }

            Write_GridMonK_log("#Command=" + "Compute=Finished");

            if(new_Load_command==1) { 
                GridCongestions_screening(0); // request Grid congestions list
                new_Load_command = 0;
            }

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

        void Calculate_values_from_results()
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
                    if (lines[i1, lines_PROP_R1] == "") {
                        // if there is not yet a value in the variable, it is copied form linecode
                        // however, if it was changed with the console, it is kept as it is.
                        lines[i1, lines_PROP_R1] = R1.ToString("###0.000");
                        lines_double[i1, lines_PROP_R1] = R1; // we store als the R1 value as a "double" number
                    }
                    double X1 = 0;
                    if (linecodes[found_linecode, linecodes_PROP_X1] != "")
                        X1 = double.Parse(linecodes[found_linecode, linecodes_PROP_X1]) * double.Parse(lines[i1, lines_PROP_length]);
                    if(lines[i1, lines_PROP_X1] == "") {
                        // if there is not yet a value in the variable, it is copied form linecode
                        // however, if it was changed with the console, it is kept as it is.
                        lines[i1, lines_PROP_X1] = X1.ToString("###0.000");
                        lines_double[i1, lines_PROP_X1] = X1; // we store also the X1 value as a "double" number
                    }
                    //double Imax = 0;
                    if (lines[i1, lines_PROP_Imax] == "") 
                        // Only if there is no Imax defined for the current line we take the Imax from linecode
                        // Otherwise, it remains the Imax define din the line, which is prevailing to Imax from  linecode
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
            Calculate_grid_data_scenarios_array();
        }

        private void Detect_voltages_for_nodes()
        {
            //nodes_no = 0;
            int found_node = 0;
            // Scan loads
            for (int ld1 = 0; ld1 < loads_no; ld1++)
            {
                if(ld1==4) {
                    ld1 = 4;
                }
                found_node = 0;
                for (int n1 = 0; n1 < nodes_no; n1++)
                    if (loads[ld1, loads_PROP_bus] == nodes[n1, nodes_PROP_bus])
                    {
                        found_node = 1; //n1 = nodes_no;
                        if (loads[ld1, loads_PROP_U1] != "") // check if the load has a calculated voltage, meaning non zero string
                        {
                            // se asociaza tensiunile in fiecare nod la care este conectata o sarcina
                            nodes[n1, nodes_PROP_U_source_object] = "load";
                            nodes[n1, nodes_PROP_U_source_object_number] = ld1.ToString();
                            nodes[n1, nodes_PROP_U_source_object_name] = loads[ld1, loads_PROP_name];
                        }
                    }
                //if (found_node == 0) // este un nod nou gasit in lista de "loads", care trebuie inregistrat
                //{
                //    nodes_no++;
                //}
            }

            // scan lines for the first terminal
            for (int l1 = 0; l1 < lines_no; l1++)
            {
                // scan the first bus of the current line
                found_node = 0;
                for (int n1 = 0; n1 < nodes_no; n1++)
                {
                    if (lines[l1, lines_PROP_bus1] == nodes[n1, nodes_PROP_bus])
                    {
                        if (lines[l1, lines_PROP_U1] != "") // check if the line has a calculated voltage, meaning non zero string
                        {
                            //if (nodes[nodes_no, nodes_PROP_U_source_object] == "")
                            {
                                nodes[n1, nodes_PROP_U_source_object] = "line";
                                nodes[n1, nodes_PROP_U_source_object_number] = l1.ToString();
                                nodes[n1, nodes_PROP_U_source_object_terminal] = "1";
                                nodes[n1, nodes_PROP_U_source_object_name] = lines[l1, lines_PROP_name];// + ".1";
                                nodes[n1, nodes_PROP_voltage] = lines[l1, lines_PROP_voltage];
                            }
                        }
                        found_node = 1;
                    }
                }
                //if (found_node == 0)
                //{
                //    nodes_no++;
                //}

                // scan the second bus of the current line
                found_node = 0;
                for (int n1 = 0; n1 < nodes_no; n1++)
                {
                    if (lines[l1, lines_PROP_bus2] == nodes[n1, nodes_PROP_bus])
                    {
                        if (lines[l1, lines_PROP_U1_t2] != "") // check if the line has a calculated voltage, meaning non zero string
                        {

                            //if (nodes[nodes_no, nodes_PROP_U_source_object] == "")
                            {
                                nodes[n1, nodes_PROP_U_source_object] = "line";
                                nodes[n1, nodes_PROP_U_source_object_number] = l1.ToString();
                                nodes[n1, nodes_PROP_U_source_object_terminal] = "2";
                                nodes[n1, nodes_PROP_U_source_object_name] = lines[l1, lines_PROP_name];// + ".2";
                                nodes[n1, nodes_PROP_voltage] = lines[l1, lines_PROP_voltage];
                            }
                        }
                        found_node = 1;
                    }
                }
                //if (found_node == 0)
                //{
                //    nodes_no++;
                //}

            }
            /*
            // scan trafos
            for (int t1 = 0; t1 < trafos_no; t1++)
            {
                found_node = 0;
                for (int n1 = 0; n1 < nodes_no; n1++)
                    if (trafos[t1, trafos_PROP_bus1] == nodes[n1, nodes_PROP_bus])
                    {
                        //if (trafos[t1, trafos_PROP_U1] != "") // check if the line has a calculated voltage, meaning non zero string
                        if (nodes[nodes_no, nodes_PROP_U_source_object] == "")
                        {
                            nodes[nodes_no, nodes_PROP_U_source_object] = "trafo";
                            nodes[nodes_no, nodes_PROP_U_source_object_number] = t1.ToString();
                            nodes[nodes_no, nodes_PROP_U_source_object_name] = trafos[t1, trafos_PROP_name];
                        }
                        found_node = 1;
                    }
                if (found_node == 0)
                {
                    nodes_no++;
                }
            }
            */
            // Scan generators
            for (int g1 = 0; g1 < generators_no; g1++)
            {
                found_node = 0;
                for (int n1 = 0; n1 < nodes_no; n1++)
                    if (generators[g1, generators_PROP_bus] == nodes[n1, nodes_PROP_bus])
                    {
                        //if (nodes[nodes_no, nodes_PROP_U_source_object] == "")
                        {
                            nodes[n1, nodes_PROP_U_source_object] = "generator";
                            nodes[n1, nodes_PROP_U_source_object_number] = g1.ToString();
                            nodes[n1, nodes_PROP_U_source_object_name] = generators[g1, generators_PROP_name];
                        }
                        found_node = 1;
                    }
                if (found_node == 0)
                {
                    nodes_no++;
                }
            }

        }


        void Assess_nodes_properties()
        {
            Detect_voltages_for_nodes();
            //  
            // se aloca tensiuni la noduri, prin citirea tensiunii unui "load", "generator" sau "line"  conectat la acel nod
            // (in ac. ordine si prioritate de alocare)
            int found_voltage = 0;
            for (int n1 = 0; n1 < nodes_no; n1++)
            {
                if(n1 == 9)
                {
                    n1 = 9;
                }
                found_voltage = 0;
                int object_no = -1;
                if (nodes[n1, nodes_PROP_U_source_object] == "load") { 
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

                        if(nodes[n1, nodes_PROP_U1] != "") found_voltage = 1;
                    }
                }
                if (nodes[n1, nodes_PROP_U_source_object] == "generator") { 
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

                        if (nodes[n1, nodes_PROP_U1] != "") found_voltage = 1;
                    }
                }
                if (nodes[n1, nodes_PROP_U_source_object] == "line")
                {
                    if (nodes[n1, nodes_PROP_U_source_object_number] != "")
                    { // lines[l1, lines_PROP_bus2]
                        object_no = int.Parse(nodes[n1, nodes_PROP_U_source_object_number]);
                        //if(nodes[nodes_no, nodes_PROP_U_source_object_terminal] == "1") 
                        if(nodes[n1, nodes_PROP_U_source_object_terminal] == "1")
                        { 
                            nodes[n1, nodes_PROP_U1] = lines[object_no, lines_PROP_U1];
                            nodes[n1, nodes_PROP_U2] = lines[object_no, lines_PROP_U2];
                            nodes[n1, nodes_PROP_U3] = lines[object_no, lines_PROP_U3];
                            //nodes[n1, nodes_PROP_U4] = lines[object_no, lines_PROP_U4];

                            nodes[n1, nodes_PROP_U1fi] = lines[object_no, lines_PROP_U1fi];
                            nodes[n1, nodes_PROP_U2fi] = lines[object_no, lines_PROP_U2fi];
                            nodes[n1, nodes_PROP_U3fi] = lines[object_no, lines_PROP_U3fi];
                            //nodes[n1, nodes_PROP_U4fi] = lines[object_no, lines_PROP_U4fi];
                        }
                        if (nodes[n1, nodes_PROP_U_source_object_terminal] == "2")
                        {
                            nodes[n1, nodes_PROP_U1] = lines[object_no, lines_PROP_U1_t2];
                            nodes[n1, nodes_PROP_U2] = lines[object_no, lines_PROP_U2_t2];
                            nodes[n1, nodes_PROP_U3] = lines[object_no, lines_PROP_U3_t2];
                            //nodes[n1, nodes_PROP_U4] = lines[object_no, lines_PROP_U4];

                            nodes[n1, nodes_PROP_U1fi] = lines[object_no, lines_PROP_U1fi_t2];
                            nodes[n1, nodes_PROP_U2fi] = lines[object_no, lines_PROP_U2fi_t2];
                            nodes[n1, nodes_PROP_U3fi] = lines[object_no, lines_PROP_U3fi_t2];
                            //nodes[n1, nodes_PROP_U4fi] = lines[object_no, lines_PROP_U4fi];
                        }

                        nodes[n1, nodes_PROP_voltage] = lines[object_no, lines_PROP_voltage];
                        if (lines[object_no, lines_PROP_brk1] == "on")
                            nodes[n1, nodes_PROP_U_source_object_avail_U_meas] = "1";
                        else nodes[n1, nodes_PROP_U_source_object_avail_U_meas] = "0";

                        if (nodes[n1, nodes_PROP_U1] != "") found_voltage = 1;
                    }
                }
            }
        }

    }
}