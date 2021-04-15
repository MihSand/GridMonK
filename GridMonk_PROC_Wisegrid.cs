/*
 * Grid-MonK is an open source softwaer application intended to annalize power grids, esepcially microgrids
 * The basic variant works by invoking the OpenDSS open-source application, with input files prepared by Grid_MonK
 * and export output files read by OpenDSS and used for further calulations and for interracting with a grid specialist.
 * Grid-Monk can be used and modified by anybody, the only condition is to keep these comments unchanged in the upper
 * part of the used or modified application
 * There is no guarrantee given for any functionality or for any
 * influence on the computer(s) running this applications or on other applications which run on the computer(s)
 * Initiator of the Grid-Monk application: Mihai Sanduleac, University Politehnica of Bucharest, Romania
 * This module has been developed for H2020 project WiseGrid
 * Contributors: Mihai Sanduleac, Catalin Chimirel
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
using System.IO;
using System.Diagnostics;

namespace GridMonC
{
    public partial class GridMonk : Form
    {
        public class FcP2DCM
        {
            [JsonProperty("Date")]
            public String date { get; set; }
            
            public class Node
            {
                [JsonProperty("Node_Number")]
                public String nodeNumber { get; set; }
                
                [JsonProperty("Name")]
                public String name { get; set; }

                [DefaultValue(0.0)]
                [JsonProperty("U1", DefaultValueHandling = DefaultValueHandling.Populate)]
                public double U1 { get; set; }

                [DefaultValue(0.0)]
                [JsonProperty("U2", DefaultValueHandling = DefaultValueHandling.Populate)]
                public double U2 { get; set; }

                [DefaultValue(0.0)]
                [JsonProperty("U3", DefaultValueHandling = DefaultValueHandling.Populate)]
                public double U3 { get; set; }

                [DefaultValue(0.0)]
                [JsonProperty("P1", DefaultValueHandling = DefaultValueHandling.Populate)]
                public double P1 { get; set; }

                [DefaultValue(0.0)]
                [JsonProperty("P2", DefaultValueHandling = DefaultValueHandling.Populate)]
                public double P2 { get; set; }

                [DefaultValue(0.0)]
                [JsonProperty("P3", DefaultValueHandling = DefaultValueHandling.Populate)]
                public double P3 { get; set; }

                [DefaultValue(0.0)]
                [JsonProperty("Q1", DefaultValueHandling = DefaultValueHandling.Populate)]
                public double Q1 { get; set; }

                [DefaultValue(0.0)]
                [JsonProperty("Q2", DefaultValueHandling = DefaultValueHandling.Populate)]
                public double Q2 { get; set; }

                [DefaultValue(0.0)]
                [JsonProperty("Q3", DefaultValueHandling = DefaultValueHandling.Populate)]
                public double Q3 { get; set; }
            }

            public class Line
            {
                [JsonProperty("Line_Number")]
                public String lineNumber { get; set; }
                
                [JsonProperty("Node_Connection")]
                public List<String> nodeConnection { get; set; }

                [JsonProperty("Name")]
                public String name { get; set; }

                [DefaultValue(0.0)]
                [JsonProperty("P1", DefaultValueHandling = DefaultValueHandling.Populate)]
                public double P1 { get; set; }

                [DefaultValue(0.0)]
                [JsonProperty("P2", DefaultValueHandling = DefaultValueHandling.Populate)]
                public double P2 { get; set; }

                [DefaultValue(0.0)]
                [JsonProperty("P3", DefaultValueHandling = DefaultValueHandling.Populate)]
                public double P3 { get; set; }

                [DefaultValue(0.0)]
                [JsonProperty("Q1", DefaultValueHandling = DefaultValueHandling.Populate)]
                public double Q1 { get; set; }

                [DefaultValue(0.0)]
                [JsonProperty("Q2", DefaultValueHandling = DefaultValueHandling.Populate)]
                public double Q2 { get; set; }

                [DefaultValue(0.0)]
                [JsonProperty("Q3", DefaultValueHandling = DefaultValueHandling.Populate)]
                public double Q3 { get; set; }
            }

            [JsonProperty("Nodes")]
            public List<Node> nodes { get; set; }

            [JsonProperty("Lines")]
            public List<Line> lines { get; set; }
        }
        
        void MQTT_Received_Data_Wisegrid(MqttMsgPublishEventArgs e)
        {
            //Input_node_name[0] = "B110-125"; GridMonK_node_number[0] = 0;
            //Input_node_name[1] = "B110-030"; GridMonK_node_number[1] = 13;

            Debug.WriteLine("Received = " + Encoding.UTF8.GetString(e.Message) + " on topic " + e.Topic);
            message_received = Encoding.UTF8.GetString(e.Message);
            if(e.Topic == "pqg-dcm")
            {
                var serializer = new JsonSerializer();
                FcP2DCM res = serializer.Deserialize<FcP2DCM>(new JsonTextReader(new StringReader(Encoding.UTF8.GetString(e.Message))));
                //serializer.Populate(new JsonTextReader(new StringReader(Encoding.UTF8.GetString(e.Message))), res);

                received_date = res.date;

                double P, Q;
                string name = "";

                // Readout of lines data
                int nr_lines = res.lines.Count;
                string line_node1, line_node2;
                int nr_nodeCons = res.lines[0].nodeConnection.Count;
                int line_dest = 0;
                for (int l1=0; l1<nr_lines; l1++)
                {
                    received_lines_q1 = res.lines[1].Q1;
                    line_node1 = res.lines[l1].nodeConnection[0];
                    line_node2 = res.lines[l1].nodeConnection[1];
                    for (int i = 0; i < _GridMonK_Max_Nodes_input_mapping; i++)
                        if ((line_node1 == Input_line_node1_name[i]) && (line_node2 == Input_line_node2_name[i]))
                        {
                            line_dest = GridMonK_line_number[i];
                            lines[line_dest, lines_PROP_P1] = res.lines[l1].P1.ToString();
                            lines[line_dest, lines_PROP_P2] = res.lines[l1].P2.ToString();
                            lines[line_dest, lines_PROP_P3] = res.lines[l1].P3.ToString();
                            lines[line_dest, lines_PROP_Q1] = res.lines[l1].Q1.ToString();
                            lines[line_dest, lines_PROP_Q2] = res.lines[l1].Q2.ToString();
                            lines[line_dest, lines_PROP_Q3] = res.lines[l1].Q3.ToString();
                            P = res.lines[l1].P1 + res.lines[l1].P2 + res.lines[l1].P3;
                            Q = res.lines[l1].Q1 + res.lines[l1].Q2 + res.lines[l1].Q3;
                            lines[line_dest, lines_PROP_P] = P.ToString();
                            lines[line_dest, lines_PROP_Q] = Q.ToString();
                        }
                    //String fstNodeCon = res.lines[0].nodeConnection[0];

                }

                // Readout of nodes data
                int nr_nodes = res.nodes.Count;
                int load_dest = 0;
                for (int n1 = 0; n1 < nr_nodes; n1++)
                {
                    name = res.nodes[n1].name;
                    for (int i = 0; i < _GridMonK_Max_Nodes_input_mapping; i++)
                        if (name == Input_node_name[i])
                        {
                            load_dest = GridMonK_node_number[i];
                            //received_nodes_u3 = res.nodes[0].U3;
                            loads[load_dest, loads_PROP_U1] = res.nodes[n1].U1.ToString();
                            loads[load_dest, loads_PROP_U2] = res.nodes[n1].U2.ToString();
                            loads[load_dest, loads_PROP_U3] = res.nodes[n1].U3.ToString();

                            loads[load_dest, loads_PROP_P1] = res.nodes[n1].P1.ToString();
                            loads[load_dest, loads_PROP_P2] = res.nodes[n1].P2.ToString();
                            loads[load_dest, loads_PROP_P3] = res.nodes[n1].P3.ToString();

                            loads[load_dest, loads_PROP_Q1] = res.nodes[n1].Q1.ToString();
                            loads[load_dest, loads_PROP_Q2] = res.nodes[n1].Q2.ToString();
                            loads[load_dest, loads_PROP_Q3] = res.nodes[n1].Q3.ToString();

                            P = res.nodes[n1].P1 + res.nodes[n1].P2 + res.nodes[n1].P3;
                            Q = res.nodes[n1].Q1 + res.nodes[n1].Q2 + res.nodes[n1].Q3;
                            loads[load_dest, loads_PROP_P] = P.ToString();
                            loads[load_dest, loads_PROP_Q] = Q.ToString();
                        }
                }
            }
            //textBox_remove.Text = message_received;
            // Codul acesta e necesar deoarece ne aflam pe un alt thread de executie decat cel original in care s-au creat elementele de GUI
            /*
            if (this.richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke((MethodInvoker)delegate ()
                {
                    richTextBox1.AppendText("Topic(" + e.Topic + "): " + Encoding.UTF8.GetString(e.Message) + "\n");
                });
            }
            else
            {
                richTextBox1.AppendText("Topic(" + e.Topic + "): " + Encoding.UTF8.GetString(e.Message) + "\n");
            }
            */
        }

        int MQTT_Message_V2_enable = 0; // of 0, it is disabled; if 1, it is enabled
        string MQTT_broker1_Message_type = "V1"; // can be V1 or V2
        public void Read_Wisegrid_config_file()
        {
            try
            {
                string Wisegrid_config_file = "PROC_Wisegrid_config_file.txt";
                string[] lines = System.IO.File.ReadAllLines(Wisegrid_config_file);
                Write_GridMonK_log("PROC_Wisegrid_config_file readout [" + Wisegrid_config_file + "]");
                foreach (string line in lines)
                {
                    if (line[0] != '#')
                    {
                        char[] delimiterChars = { '=', '[', ';' };
                        string[] line1 = line.Split(delimiterChars);
                        if (line1[0] == "MQTT_Message_V2_enable")
                        {
                            MQTT_Message_V2_enable = int.Parse(line1[1]);
                            Write_GridMonK_log("MQTT_Message_V2_enable=" + MQTT_Message_V2_enable.ToString());
                        }
                        if (line1[0] == "MQTT_broker1_Message_type")
                        {
                            MQTT_broker1_Message_type = line1[1];
                            Write_GridMonK_log("MQTT_broker1_Message_type=" + MQTT_broker1_Message_type);
                        }
                    }
                }
            }
            catch
            {
                Write_GridMonK_log("Wisegrid_config=Error1");
                Console.WriteLine("Wisegrid_config-Err1");
            }
        }


    private void PROC_ini_Wisegrid()
        {
            // read S4G config file "PROC_Wisegrid_config_file.txt"
            Read_Wisegrid_config_file();
        }

        private void timer3_PROC_Wisegrid(object sender, EventArgs e)
        {

        }

        string Operations_export_JSON = "Operations_export_JSON.txt";
        string Operations_export_JSON_V2 = "Operations_export_JSON_V2.txt";

        void GridCongestions_screening(int when)
        {
            // when=0 is for initial congestions, when=1 is for final congetsions
            if (when > 1) when = 1; if (when < 0) when = 0;
            double U1 = 0, U2 = 0, U3 = 0;
            GridCongestions_no[when] = 0;

            for (int nr_node = 0; nr_node < nodes_no; nr_node++)
                if (nodes[nr_node, nodes_PROP_U1] != "") // check only on phase 1
                    if (nodes[nr_node, nodes_PROP_voltage] != "")
                    {
                        int congestion = 0; // congestion becomes 1 if there is a volateg violation
                        double Nominal_voltage = double.Parse(nodes[nr_node, nodes_PROP_voltage]);
                        double limit_d = 0.95, limit_u = 1.05; // these limits are for medium and high voltage
                        if (Nominal_voltage < 10) { limit_d = 0.9; limit_u = 1.1; } // This is for low voltage
                        double Vmin = 1000.0 * Nominal_voltage * limit_d / 1.73205;
                        double Vmax = 1000.0 * Nominal_voltage * limit_u / 1.73205;
                        U1 = double.Parse(nodes[nr_node, nodes_PROP_U1]);
                        U2 = double.Parse(nodes[nr_node, nodes_PROP_U2]);
                        U3 = double.Parse(nodes[nr_node, nodes_PROP_U3]);
                        double Vproc = U1 / Nominal_voltage / 1000 * 1.73205 * 100;
                        double V_nominal = Nominal_voltage * 1000 / 1.73205;
                        if (U1 < Vmin)
                        {
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_object] = "node";
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_name] = nodes[nr_node, nodes_PROP_name];
                            //GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_attrib] = "<Umin";
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_attrib] = "LOW";
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value] = U1.ToString("########0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_proc] = Vproc.ToString("##0.00");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_lim] = Vmin.ToString("##0.0");
                            //GridCongestions[when, GridCongestions_no[when], Operations_PROP_user_name] = nodes[nr_node, Operations_PROP_user_name];
                            congestion = 1;
                            //GridCongestions_no[when]++;
                        }
                        if (U1 > Vmax)
                        {
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_object] = "node";
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_name] = nodes[nr_node, nodes_PROP_name];
                            //GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_attrib] = ">Umax";
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_attrib] = "HIGH";
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value] = U1.ToString();
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_proc] = Vproc.ToString("##0.00");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_lim] = Vmax.ToString("##0.0");
                            congestion = 1;
                            //GridCongestions_no[when]++;
                        }
                        if (congestion == 1)
                        {
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_lim2_down] = Vmin.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_lim1_down] = Vmin.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_nominal] = V_nominal.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_lim2_up] = Vmax.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_lim1_up] = Vmax.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_phase1] = U1.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_phase2] = U2.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_phase3] = U3.ToString("#0.0");
                            GridCongestions_no[when]++;
                        }
                    }

            for (int nr_line = 0; nr_line < lines_no; nr_line++)
                if (lines[nr_line, lines_PROP_I1] != "")  // check only current on phase 1
                    if (lines[nr_line, lines_PROP_Imax] != "")
                    {
                        int congestion = 0; // congestion becomes 1 if there is a volateg violation
                        double Nominal_current = double.Parse(lines[nr_line, lines_PROP_Imax]);
                        double I1 = 0, I2 = 0, I3 = 0; double P1 = 0, P2 = 0, P3 = 0; double Q1 = 0, Q2 = 0, Q3 = 0;
                        try { 
                            I1 = double.Parse(lines[nr_line, lines_PROP_I1]);
                            I2 = double.Parse(lines[nr_line, lines_PROP_I2]);
                            I3 = double.Parse(lines[nr_line, lines_PROP_I3]);
                            P1 = double.Parse(lines[nr_line, lines_PROP_P1]);
                            P2 = double.Parse(lines[nr_line, lines_PROP_P2]);
                            P3 = double.Parse(lines[nr_line, lines_PROP_P3]);
                            Q1 = double.Parse(lines[nr_line, lines_PROP_Q1]);
                            Q2 = double.Parse(lines[nr_line, lines_PROP_Q2]);
                            Q3 = double.Parse(lines[nr_line, lines_PROP_Q3]);
                        } catch { }
                        
                        double Iproc = I1 / Nominal_current * 100;
                        if (I1 > Nominal_current)
                        {
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_object] = "line";
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_name] = lines[nr_line, lines_PROP_name];
                            //GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_attrib] = ">Imax";
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_attrib] = "HIGH";
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value] = I1.ToString("########0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_proc] = Iproc.ToString("##0.00");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_lim] = Nominal_current.ToString("##0.0");
                            congestion = 1;
                        }
                        if (congestion == 1)
                        {
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_lim1_up] = Nominal_current.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_lim2_up] = (Nominal_current * 1.1).ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_phase1] = I1.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_phase2] = I2.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_phase3] = I3.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_P1] = P1.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_P2] = P2.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_P3] = P3.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_Q1] = Q1.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_Q2] = Q2.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_value_Q3] = Q3.ToString("#0.0");
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_node1] = lines[nr_line, lines_PROP_bus1];
                            GridCongestions[when, GridCongestions_no[when], GridCongestions_PROP_node2] = lines[nr_line, lines_PROP_bus2];
                            GridCongestions_no[when]++;
                        }
                    }

        }

        private void Export_JSON_Wisegrid()
        {
            // Initial developement of this export functionality is for WiseGrid project 
            /* // model of JSON export data
{
"Message_type": "DCM-CkP",
"Congestions_ini": {
"Date": "2018.08.14 10:00",
	"Nodes": [
		{
			"Node_Number": "N308",
			"Name": "N308",
			"U_Limits":  [195.0, 207.0, 230.0, 253.0, 255.0],
			"U1_Assessment": "HIGH",  "U1": 254.4,
			"U2_Assessment": "OK",  "U2": 245.3,
			"U3_Assessment": "HIGH_HIGH", "U3": 255.9
		} ],
	"Lines": [
		{
			"Line_Number": "N011-N127",
			"Node_Connection": ["N011", "N127"],
			"Name": "N011-N127",
			"I_Limits":  [200.0, 300.0],
			"I1_Assessment": "HIGH", "I1": 259.4, "P1": 421.3,  "Q1": -17.2,
			"I2_Assessment": "HIGH", "I2": 259.4, "P2": 421.3, "Q2": -17.2,
			"I3_Assessment": "HIGH", "I3": 259.4, "P3": 421.3, "Q3": -17.2
		} ]
},
"Congestions_final": {
"Date": "2018.08.14 10:00",
	"Nodes": [],
	"Lines": []
}
"Mitigation": {
"Date": "2018.08.14 10:00",

"Loads": [
{
"Node_Number": "1",
"Name": "MV_supply_point_01",
"Type": "Load",
"Q": ["0",  "-140.0"],
"Message": "Parameter Q changed",
"Message_CFM": "Q is requested to increase node voltage"
},
{
"Node_Number": "1",
"Name": "Load_N107",
"Type": "Load",
"P": ["120",  "85.0"],
"Message": "Parameter P changed",
"Message_CFM": "Demand response campain is needed"
},
{
"Node_Number": "N103",
"Name": "Storage_01",
"Type": "Storage",
"P": ["0.0",  "50.0"],
"Message": "Parameter P changed",
"Message_CFM": "P is requested from battery"
},

"Lines": [
{
"Line_Number": "1",
"Node_Connection": ["N201", "N303"],
"Name": "N201-N303",
"Brk1": ["ON", "OFF"],
"Message": "Parameter Brk1 changed",
"Message_CFM": "Line has been disconnected"
},
{
"Line_Number": "7",
"Node_Connection": ["N012", "N021"],
"Name": "N01-N021",
"Brk1": ["ON", "OFF"],
"Message": "Parameter Brk1 changed",
"Message_CFM": "Line has been disconnected"
}
]
}
}
             */

            GridCongestions_screening(1); // request Grid congestions list

            int i1_node = 0, i1_line = 0;
            string s1_conj_ini_V1 = ""; // first format version of MQTT message
            string s1_conj_ini_V2 = ""; // second format version of MQTT message
            // Initial congestions [0
            int first_line_congestion = 0;
            s1_conj_ini_V2 += "\t\t\"Nodes\" : [\n";

            for (int i1 = 0; i1 < GridCongestions_no[0]; i1++)
            {
                if (GridCongestions[0, i1, GridCongestions_PROP_object] == "node")
                {
                    s1_conj_ini_V1 +=
                        "{ \"Node#\": \"" + i1_node.ToString("000") + "\", "
                        + " \"Node\": \"" + GridCongestions[0, i1, GridCongestions_PROP_name] + "\", "
                        + "\"Status\": \"" + GridCongestions[0, i1, GridCongestions_PROP_attrib] + "\", "
                        + "\"Value\": " + GridCongestions[0, i1, GridCongestions_PROP_value] + ", "
                        + "\"[%]\": " + GridCongestions[0, i1, GridCongestions_PROP_value_proc] + ", "
                        + "\"Lim\": " + GridCongestions[0, i1, GridCongestions_PROP_value_lim] + ", "
                        + "\"User_Name\": \"" + GridCongestions[0, i1, Operations_PROP_user_name] + "\" }";
                    if (i1 < GridCongestions_no[0] - 1) s1_conj_ini_V1 += ",\n"; else s1_conj_ini_V1 += "\n";

                    // second format version of MQTT message, made in a different string
                    s1_conj_ini_V2 +=
                        "\t\t{\n\t\t\t\"Node_Number\": \"" + i1_node.ToString("000") + "\", "
                        + "\n\t\t\t\"Name\": \"" + GridCongestions[0, i1, GridCongestions_PROP_name] + "\",\n"
                        + "\t\t\t\"U_Limits\": [\n"
                        + "\t\t\t\t" + GridCongestions[0, i1, GridCongestions_PROP_value_lim2_down] + "\n"
                        + "\t\t\t\t" + GridCongestions[0, i1, GridCongestions_PROP_value_lim1_down] + "\n"
                        + "\t\t\t\t" + GridCongestions[0, i1, GridCongestions_PROP_value_nominal] + "\n"
                        + "\t\t\t\t" + GridCongestions[0, i1, GridCongestions_PROP_value_lim1_up] + "\n"
                        + "\t\t\t\t" + GridCongestions[0, i1, GridCongestions_PROP_value_lim2_up] + "\n"
                        //+ "\t\t\t\t207.0\n"
                        //+ "\t\t\t\t230.0\n"
                        //+ "\t\t\t\t253.0\n"
                        //+ "\t\t\t\t255.0\n"
                        + "\t\t\t],\n"
                        + "\t\t\t\"U1_Assessment\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_attrib] + "\",\n"
                        + "\t\t\t\"U1\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_value_phase1] + "\",\n"
                        + "\t\t\t\"U2_Assessment\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_attrib] + "\",\n"
                        + "\t\t\t\"U2\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_value_phase2] + "\",\n"
                        + "\t\t\t\"U3_Assessment\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_attrib] + "\",\n"
                        + "\t\t\t\"U3\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_value_phase3] + "\",\n"
                        + "\t\t}"
                        ;

                    if (i1 < GridCongestions_no[0] - 1) s1_conj_ini_V2 += ",\n"; else s1_conj_ini_V2 += "\n";

                    i1_node++;
                }

                if (GridCongestions[0, i1, GridCongestions_PROP_object] == "line")
                {
                    if(first_line_congestion==0)
                    {
                        s1_conj_ini_V2 += "\t\t],\n";
                        s1_conj_ini_V2 += "\t\t\"Lines\" : [\n";
                        first_line_congestion = 1;
                    }
                    s1_conj_ini_V1 +=
                        "{ \"Line#\": \"" + i1_line.ToString("000") + "\", "
                        + " \"Line\": \"" + GridCongestions[0, i1, GridCongestions_PROP_name] + "\", "
                        + "\"Status\": \"" + GridCongestions[0, i1, GridCongestions_PROP_attrib] + "\", "
                        + "\"Value\": " + GridCongestions[0, i1, GridCongestions_PROP_value] + ", "
                        + "\"[%]\": " + GridCongestions[0, i1, GridCongestions_PROP_value_proc] + ", "
                        + "\"Lim\": " + GridCongestions[0, i1, GridCongestions_PROP_value_lim] + ", "
                        + "\"User_Name\": \"" + GridCongestions[0, i1, Operations_PROP_user_name] + "\" }";
                    if (i1 < GridCongestions_no[0] - 1) s1_conj_ini_V1 += ",\n"; else s1_conj_ini_V1 += "\n";

                    // second format version of MQTT message, made in a different string
                    s1_conj_ini_V2 +=
                        //"\t\t{\n\t\t\t\"Line_Number\": \"" + i1_line.ToString("000") + "\", "
                        "\t\t{\n\t\t\t\"Line_Number\": \"" + GridCongestions[0, i1, GridCongestions_PROP_name] + "\", "
                        + "\n\t\t\t\"Node_Connection\": ["
                        + "\n\t\t\t\t\"" + GridCongestions[0, i1, GridCongestions_PROP_node1] + "\", "
                        + "\n\t\t\t\t\"" + GridCongestions[0, i1, GridCongestions_PROP_node2] + "\"\n\t\t\t],"
                        + "\n\t\t\t\"Name\": \"" + GridCongestions[0, i1, GridCongestions_PROP_name] + "\",\n"
                        + "\t\t\t\"I_Limits\": [\n"
                        + "\t\t\t\t" + GridCongestions[0, i1, GridCongestions_PROP_value_lim1_up] + "\n"
                        + "\t\t\t\t" + GridCongestions[0, i1, GridCongestions_PROP_value_lim2_up] + "\n"
                        //+ "\t\t\t\t300.0\n"
                        + "\t\t\t],\n"
                        + "\t\t\t\"I1_Assessment\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_attrib] + "\",\n"
                        + "\t\t\t\"I1\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_value_phase1] + "\",\n"
                        + "\t\t\t\"P1\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_value_P1] + "\",\n"
                        + "\t\t\t\"Q1\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_value_Q1] + "\",\n"
                        + "\t\t\t\"I2_Assessment\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_attrib] + "\",\n"
                        + "\t\t\t\"I2\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_value_phase2] + "\",\n"
                        + "\t\t\t\"P2\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_value_P2] + "\",\n"
                        + "\t\t\t\"Q2\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_value_Q2] + "\",\n"
                        + "\t\t\t\"I3_Assessment\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_attrib] + "\",\n"
                        + "\t\t\t\"I3\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_value_phase3] + "\",\n"
                        + "\t\t\t\"P3\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_value_P3] + "\",\n"
                        + "\t\t\t\"Q3\" : \"" + GridCongestions[0, i1, GridCongestions_PROP_value_Q3] + "\",\n"
                        + "\t\t}"
                        ;

                    if (i1 < GridCongestions_no[0] - 1) s1_conj_ini_V2 += ",\n"; else s1_conj_ini_V2 += "\n";

                    i1_line++;
                }
            }
            s1_conj_ini_V2 += "\t\t]\n";

            string s1_conj_final = "";
            i1_node = 0; i1_line = 0;
            // Final congestions [1
            for (int i1 = 0; i1 < GridCongestions_no[1]; i1++)
            {
                if (GridCongestions[1, i1, GridCongestions_PROP_object] == "node")
                {
                    s1_conj_final +=
                        "{ \"Node#\": \"" + i1_node.ToString("000") + "\", "
                        + " \"Node\": \"" + GridCongestions[1, i1, GridCongestions_PROP_name] + "\", "
                        + "\"Status\": \"" + GridCongestions[1, i1, GridCongestions_PROP_attrib] + "\", "
                        + "\"Value\": " + GridCongestions[1, i1, GridCongestions_PROP_value] + ", "
                        + "\"[%]\": " + GridCongestions[1, i1, GridCongestions_PROP_value_proc] + ", "
                        + "\"Lim\": " + GridCongestions[1, i1, GridCongestions_PROP_value_lim] + ", "
                        + "\"User_Name\": \"" + GridCongestions[1, i1, Operations_PROP_user_name] + "\" }";
                    if (i1 < GridCongestions_no[1] - 1) s1_conj_final += ",\n"; else s1_conj_final += "\n";
                    i1_node++;
                }
                if (GridCongestions[1, i1, GridCongestions_PROP_object] == "line")
                {
                    s1_conj_final +=
                        "{ \"Line#\": \"" + i1_line.ToString("000") + "\", "
                        + " \"Line\": \"" + GridCongestions[1, i1, GridCongestions_PROP_name] + "\", "
                        + "\"Status\": \"" + GridCongestions[1, i1, GridCongestions_PROP_attrib] + "\", "
                        + "\"Value\": " + GridCongestions[1, i1, GridCongestions_PROP_value] + ", "
                        + "\"[%]\": " + GridCongestions[1, i1, GridCongestions_PROP_value_proc] + ", "
                        + "\"Lim\": " + GridCongestions[1, i1, GridCongestions_PROP_value_lim] + ", "
                        + "\"User_Name\": \"" + GridCongestions[1, i1, Operations_PROP_user_name] + "\" }";
                    if (i1 < GridCongestions_no[1] - 1) s1_conj_final += ",\n"; else s1_conj_final += "\n";
                    i1_line++;
                }
            }

//  {
//    "_id" : "2020.04.17 10:00", 
//    "Congestions" : {
//        "Date" : "2020.04.17 10:00", 
//        "Nodes" : [
//            {

            string date_of_tomorrow = "";
            DateTime today = DateTime.Now;
            DateTime tomorrow = today.AddDays(1);
            date_of_tomorrow += "\"" + tomorrow.Year.ToString("0000") + "." + tomorrow.Month.ToString("00") + "."
                + tomorrow.Day.ToString("00") + " " + "14:00\",\n";
            string s1 = "";

            string s2 = "{\n";
            string s2_V2 = "{\n";

            s2 += "\"Message_type\": \"DCM - CkP\",\n";
            s2 += "\"Congestions_ini\": {\n";
            s2 += "\"Date\": " + date_of_tomorrow;
            s2 += "\"GridCongestionsIni_no\": " + GridCongestions_no[0].ToString() + ",\n";
            s2 += "\"List_ini\": [\n";
            s2 += s1_conj_ini_V1;
            s2 += "]\n";

            s2_V2 += "\t\"_id\": " + date_of_tomorrow;
            s2_V2 += "\t\"Congestions\": {\n";
            s2_V2 += "\t\t\"Date\": " + date_of_tomorrow;
            //s2_V2 += "\t\t\"Nodes\" : [\n";
            s2_V2 += s1_conj_ini_V2;
            s2_V2 += "\t},\n";

            //s2 += "\"Lines\": [\n";
            //s2 += "]\n";
            s2 += "},\n";
            s2 += "\"Congestions_final\": {\n";
            //s2 += "\"Date\": \"2020.04.11 14:00\",\n";
            s2 += "\"Date\": " + date_of_tomorrow;
            s2 += "\"GridCongestionsFinal_no\": " + GridCongestions_no[1].ToString() + ",\n";
            s2 += "\"List_final\": [\n";
            s2 += s1_conj_final;
            //s2 += "],\n";
            //s2 += "\"Lines\": [\n";
            s2 += "]\n";
            s2 += "},\n";
            s2 += "\"Mitigation\": {\n";

            s2_V2 += "\t\"Message_type\" : \"DCM-CkP\",\n"; 

            s2_V2 += "\t\"Mitigation\": {\n";
            s2_V2 += "\t\t\"Date\": " + date_of_tomorrow;
            //"Date" : "2020.04.21 10:00", 

            // construction of the JSON file
            // Make list of "load" operations
            string s2_mitigations = "";
            bool first_record = true;

            s2 += "\"Loads\": [\n";
            s2_mitigations += "\t\t\"Loads\": [\n";
            for (int i1 = 0; i1 < Operations_no; i1++)
            {
                if (Operations[i1, Operations_PROP_object] == "load")
                {
                    if (first_record == false) { s2_mitigations += ",\n"; }
                    if (first_record == true) first_record = false;
                    s2_mitigations += "\t\t{\n";
                    //s2_mitigations += "\"Load#\": \"" + Operations[i1, Operations_PROP_detail3_item_no] + "\",\n";
                    //s2_mitigations += "\"Node_Connection\": [\"" + Operations[i1, Operations_PROP_detail1_bus1] + "\"],\n";
                    s2_mitigations += "\t\t\t\"Node_Number\": \"" + Operations[i1, Operations_PROP_detail1_bus1] + "\",\n";
                    s2_mitigations += "\t\t\t\"Name\": \"" + Operations[i1, Operations_PROP_name] + "\",\n";
                    //s2_mitigations += "\"User_Name\": \"" + Operations[i1, Operations_PROP_user_name] + "\",\n";
                    if(Operations[i1, Operations_PROP_detail4_type] == "") s2_mitigations += "\t\t\t\"Type\": \"" + "Load" + "\",\n";
                    else s2_mitigations += "\t\t\t\"Type\": \"" + Operations[i1, Operations_PROP_detail4_type] + "\",\n";

                    if (Operations[i1, Operations_PROP_attrib] == "Pn")
                    {
                        s2_mitigations += "\t\t\t\"P\": [\"" + Operations[i1, Operations_PROP_value_old] + "\", \"" + Operations[i1, Operations_PROP_value] + "\"],\n";
                        s2_mitigations += "\t\t\t\"Message\": \"" + "Parameter P changed" + "\",\n";
                    }
                    if (Operations[i1, Operations_PROP_attrib] == "Qn")
                    {
                        s2_mitigations += "\t\t\t\"Q\": [\"" + Operations[i1, Operations_PROP_value_old] + "\", \"" + Operations[i1, Operations_PROP_value] + "\"],\n";
                        s2_mitigations += "\t\t\t\"Message\": \"" + "Parameter Q changed" + "\",\n";
                    }
                    if (Operations[i1, Operations_PROP_attrib] == "PF")
                    {
                        s2_mitigations += "\t\t\t\"PF\": [\"" + Operations[i1, Operations_PROP_value_old] + "\", \"" + Operations[i1, Operations_PROP_value] + "\"],\n";
                        s2_mitigations += "\t\t\t\"Message\": \"" + "Parameter PF changed" + "\",\n";
                    }
                    if (Operations[i1, Operations_PROP_attrib] == "brk")
                    {
                        s2_mitigations += "\t\t\t\"brk\": [\"" + Operations[i1, Operations_PROP_value_old] + "\", \"" + Operations[i1, Operations_PROP_value] + "\"],\n";
                        s2_mitigations += "\t\t\t\"Message\": \"" + "Parameter brk changed" + "\",\n";
                    }

                    if (Operations[i1, Operations_PROP_attrib] == "Pn")
                    {
                        if (Operations[i1, Operations_PROP_detail4_type] == "storage")
                            s2_mitigations += "\t\t\t\"Message_CFM\": \"" + "P is requested from battery" + "\"\n";
                        else if (Operations[i1, Operations_PROP_detail4_type] == "EV")
                            s2_mitigations += "\t\t\t\"Message_CFM\": \"" + "P is needed for EV charge" + "\"\n";
                        else
                            s2_mitigations += "\t\t\t\"Message_CFM\": \"" + "Demand response campain is needed" + "\"\n";
                    }

                    if (Operations[i1, Operations_PROP_attrib] == "Qn")
                    {
                        s2_mitigations += "\t\t\t\"Message_CFM\": \"" + "Q is requested to change node voltage" + "\"\n";
                    }
                    if (Operations[i1, Operations_PROP_attrib] == "PF")
                    {
                        s2_mitigations += "\t\t\t\"Message_CFM\": \"" + "PF value is requested to change" + "\"\n";
                    }
                    if (Operations[i1, Operations_PROP_attrib] == "brk")
                    {
                        string s3 = "";
                        if (Operations[i1, Operations_PROP_detail4_type] == "storage") s3 = "Storage";
                        else if (Operations[i1, Operations_PROP_detail4_type] == "EV") s3 = "EV";
                        if (Operations[i1, Operations_PROP_value] == "off") s2_mitigations += "\t\t\t\"Message_CFM\": \"" + s3 + " has been disconnected" + "\"\n";
                        if (Operations[i1, Operations_PROP_value] == "on") s2_mitigations += "\t\t\t\"Message_CFM\": \"" + s3 + " has been connected" + "\"\n";
                    }
                    s2_mitigations += "\t\t\t,\"id\" : 5.0\n";
                    s2_mitigations += "\t\t}\n";

                }
            }
            s2_mitigations += "\t\t],\n";
            // Make list of "line" operations
            s2_mitigations += "\t\t\"Lines\": [\n";
            first_record = true;
            for (int i1 = 0; i1 < Operations_no; i1++)
            {
                if (Operations[i1, Operations_PROP_object] == "line")
                {
                    if (first_record == false) { s2_mitigations += ",\n"; }
                    if (first_record == true) first_record = false;
                    s2_mitigations += "{\n";
                    s2_mitigations += "\"Line#\": \"" + Operations[i1, Operations_PROP_detail3_item_no] + "\",\n";
                    s2_mitigations += "\"Node_Connection\": [\"" + Operations[i1, Operations_PROP_detail1_bus1] + "\", \"" + Operations[i1, Operations_PROP_detail2_bus2] + "\"],\n";
                    s2_mitigations += "\"Name\": \"" + Operations[i1, Operations_PROP_name] + "\",\n";
                    s2_mitigations += "\"User_Name\": \"" + Operations[i1, Operations_PROP_user_name] + "\",\n";
                    s2_mitigations += "\"Type\": \"" + Operations[i1, Operations_PROP_detail4_type] + "\",\n";
                    if (Operations[i1, Operations_PROP_attrib] == "brk1")
                    {
                        s2_mitigations += "\"Brk1\": [\"" + Operations[i1, Operations_PROP_value_old] + "\", \"" + Operations[i1, Operations_PROP_value] + "\"],\n";
                        s2_mitigations += "\"Message\": \"" + "Parameter Brk1 changed" + "\",\n";
                    }
                    if (Operations[i1, Operations_PROP_attrib] == "brk2")
                    {
                        s2_mitigations += "\"Brk2\": [\"" + Operations[i1, Operations_PROP_value_old] + "\", \"" + Operations[i1, Operations_PROP_value] + "\"],\n";
                        s2_mitigations += "\"Message\": \"" + "Parameter Brk2 changed" + "\",\n";
                    }
                    if (Operations[i1, Operations_PROP_value] == "off") s2_mitigations += "\"Message_CFM\": \"" + "Line has been disconnected" + "\"\n";
                    if (Operations[i1, Operations_PROP_value] == "on") s2_mitigations += "\"Message_CFM\": \"" + "Line has been connected" + "\"\n";
                    s2_mitigations += "}\n";

                }
            }
            s2_mitigations += "\t\t],\n";

            // Make list of "genrator" operations
            s2_mitigations += "\t\t\"Generators\": [\n";
            first_record = true;
            for (int i1 = 0; i1 < Operations_no; i1++)
            {
                if (Operations[i1, Operations_PROP_object] == "generator")
                {
                    if (first_record == false) { s2_mitigations += ",\n"; }
                    if (first_record == true) first_record = false;
                    s2_mitigations += "{\n";
                    s2_mitigations += "\"Generator#\": \"" + Operations[i1, Operations_PROP_detail3_item_no] + "\",\n";
                    s2_mitigations += "\"Node_Connection\": [\"" + Operations[i1, Operations_PROP_detail1_bus1] + "\"],\n";
                    //s2 += "\"Node_Number\": \"" + Operations[i1, Operations_PROP_detail_bus1] + "\",\n";
                    s2_mitigations += "\"Name\": \"" + Operations[i1, Operations_PROP_name] + "\",\n";
                    s2_mitigations += "\"User_Name\": \"" + Operations[i1, Operations_PROP_user_name] + "\",\n";
                    s2_mitigations += "\"Type\": \"" + Operations[i1, Operations_PROP_detail4_type] + "\",\n";
                    if (Operations[i1, Operations_PROP_attrib] == "Pn")
                    {
                        s2_mitigations += "\"P\": [\"" + Operations[i1, Operations_PROP_value_old] + "\", \"" + Operations[i1, Operations_PROP_value] + "\"],\n";
                        s2_mitigations += "\"Message\": \"" + "Parameter P changed" + "\",\n";
                        s2_mitigations += "\"Message_CFM\": \"" + "P is changed" + "\"\n";
                    }
                    if (Operations[i1, Operations_PROP_attrib] == "Qn")
                    {
                        s2_mitigations += "\"Q\": [\"" + Operations[i1, Operations_PROP_value_old] + "\", \"" + Operations[i1, Operations_PROP_value] + "\"],\n";
                        s2_mitigations += "\"Message\": \"" + "Parameter Q changed" + "\",\n";
                        s2_mitigations += "\"Message_CFM\": \"" + "Q is changed" + "\"\n";
                    }
                    if (Operations[i1, Operations_PROP_attrib] == "PF")
                    {
                        s2_mitigations += "\"PF\": [\"" + Operations[i1, Operations_PROP_value_old] + "\", \"" + Operations[i1, Operations_PROP_value] + "\"],\n";
                        s2_mitigations += "\"Message\": \"" + "Parameter PF changed" + "\",\n";
                        s2_mitigations += "\"Message_CFM\": \"" + "PF is changed" + "\"\n";
                    }
                    s2_mitigations += "}\n";

                }
            }
            s2 += s2_mitigations + "]\n";
            s2_V2 += s2_mitigations + "\t\t],\n";
            // Make list of "genrator" operations
            s2_V2 += "\t\t\"Trafos\": [\n\t\t]\n";
            s2_V2 += "\t}\n";
            s2_V2 += "}\n";

            // construction of the text file
            for (int i1 = 0; i1 < Operations_no; i1++)
            {
                s1 += "object=" + Operations[i1, Operations_PROP_object] + "\t";
                s1 += "name=" + Operations[i1, Operations_PROP_name] + "\t";
                s1 += "attrib=" + Operations[i1, Operations_PROP_attrib] + "\t";
                s1 += "value=" + Operations[i1, Operations_PROP_value] + "\t";
                s1 += "date=" + Operations[i1, Operations_PROP_date] + "\t";
                s1 += "time=" + Operations[i1, Operations_PROP_time] + "\t";
                s1 += "TP=" + Operations[i1, Operations_PROP_TimePeriod] + "\t";
                s1 += "TPU=" + Operations[i1, Operations_PROP_TimePeriodUnit] + "\t";
                s1 += "reason=" + Operations[i1, Operations_PROP_reason] + "\t";
                s1 += "\n";

            }

            string Operations_export_file = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + Operations_export_JSON;
            File.WriteAllText(Operations_export_file, s1);

            s2 += "}\n}";

            string Operations_export_JSON_file = Grid_Projects_Path + @"\" + GridMonk_Project + @"\" + Operations_export_JSON + ".json";
            Write_GridMonK_log("Export_JSON_to_file=" + Operations_export_JSON_file);
            File.WriteAllText(Operations_export_JSON_file, s2);
            Write_GridMonK_log("Export_JSON[file]=Done");

            string Operations_export_JSON_file_V2 = Grid_Projects_Path + @"\" + GridMonk_Project + @"\" + Operations_export_JSON_V2 + ".json";
            File.WriteAllText(Operations_export_JSON_file_V2, s2_V2);


            // Export JSON as MQTT message
            topic1 = "Congestion_Forecast/DCM-CkP";
            payload1 = s2;
            if (MQTT_Connect.ToLower() == "wisegrid")
                if ((topic1 != "") && (payload1 != ""))
                {
                    int payload_len = payload1.Length;
                    MQTT_publish(topic1, payload1, 1);
                    Write_GridMonK_log("Export_JSON_to_MQTT_broker1=done;topic=" + topic1 + ";payload_length=" + payload_len.ToString());
                    topic1 = ""; payload1 = ""; // the strings become back empty, such that only a new command will generate 
                }
        }




    }
}