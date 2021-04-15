/*
 * Grid-MonK is an open source softwaer application intended to annalize power grids, esepcially microgrids
 * The basic variant works by invoking the OpenDSS open-source application, with input files prepared by Grid_MonK
 * and export output files read by OpenDSS and used for further calulations and for interracting with a grid specialist.
 * Grid-Monk can be used and modified by anybody, the only condition is to keep these comments unchanged in the upper
 * part of the used or modified application
 * There is no guarrantee given for any functionality or for any
 * influence on the computer(s) running this applications or on other applications which run on the computer(s)
 * Initiator of the Grid-Monk application: Mihai Sanduleac, University Politehnica of Bucharest, Romania
 * This module develops a standard SCADA interface with external acquisition modules which communicate through MQTT and a GridMonK standardized JSON messaging
 * Contributors: Mihai Sanduleac
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
        //Client mosqsub/17072-RPI-TEST0 received PUBLISH (d0, q0, r0, m0, '/SCADA', ... (xx bytes))
        // {"NodeName":"N21","U_node":"231.7","I_cons":"0.0","P_cons":"0.0","Q_cons":"-10.3"}

        public class SCADA_FEP_Data
        {
            [DefaultValue("Node_NULL")]
            [JsonProperty("NodeName", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String NodeName { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("U_node", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String U_node { get; set; } 

            [DefaultValue("0.0")]
            [JsonProperty("I_cons", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String I_cons { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("P_cons", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String P_cons { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("Q_cons", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String Q_cons { get; set; }

        }

        public class SMX_FEP_Data
        {
            [DefaultValue("00000")]
            [JsonProperty("MeterName", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String MeterName { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("U_node", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String U_node { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("I_cons", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String I_cons { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("P_cons", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String P_cons { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("Q_cons", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String Q_cons { get; set; }

        }

        void MQTT_Received_Data_SCADA_FEP(MqttMsgPublishEventArgs e)
        {

            string NodeName, MeterName, U_node, I_cons, P_cons, Q_cons;

            Debug.WriteLine("Received = " + Encoding.UTF8.GetString(e.Message) + " on topic " + e.Topic);
            message_received = Encoding.UTF8.GetString(e.Message);
            // verify the received topic and process the information
            if(e.Topic == "/SCADA")
            {
                var serializer = new JsonSerializer();
                SCADA_FEP_Data res = serializer.Deserialize<SCADA_FEP_Data>(new JsonTextReader(new StringReader(Encoding.UTF8.GetString(e.Message))));

                NodeName = res.NodeName;
                U_node = res.U_node; I_cons = res.I_cons; P_cons = res.P_cons; Q_cons = res.Q_cons;
            }
            if (e.Topic == "/SMX")
            {
                var serializer = new JsonSerializer();
                SMX_FEP_Data res = serializer.Deserialize<SMX_FEP_Data>(new JsonTextReader(new StringReader(Encoding.UTF8.GetString(e.Message))));

                MeterName = res.MeterName;
                U_node = res.U_node; I_cons = res.I_cons; P_cons = res.P_cons; Q_cons = res.Q_cons;
            }
        }

        private void PROC_ini_SCADA_FEP()
        {
            // read SCADA_FEP config file "PROC_SCADA_FEP_config_file.txt"

            // 
        }

        private void timer3_PROC_SCADA_FEP(object sender, EventArgs e)
        {
            // Main loop of SCADA_FEP

            // Publish new commands in MQTT/JSON format

            // Make load-flow with new data
        }

    }
}