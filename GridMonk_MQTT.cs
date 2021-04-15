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

        //        MqttClient client1, client2; // Two MQTT clients are defined by default
        //        string MqttClient1_str_in = "", MqttClient2_str_in = "";
        //        byte code;
        //        int MQTT_broker_broker_crt=1;
        MqttClient client1, client2; // Two MQTT clients are defined by default
        string MqttClient1_str_in = "", MqttClient2_str_in = "";
        byte code;
        int MQTT_broker_broker_crt = 1;

        private void MQTT_subscribe(string topic_to_subscribe, int MQTT_broker)
        {
            // Only 2 MQTT brokers can be used in this version (number is given by the variable MQTT_broker)
            ushort msgId;

            if (MQTT_broker == 1)
                msgId = client1.Subscribe(new string[] { topic_to_subscribe },  // textBox1.Text = numele topicii pe caer subscriem
                new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

            if (MQTT_broker == 2)
                msgId = client2.Subscribe(new string[] { topic_to_subscribe },  // textBox1.Text = numele topicii pe caer subscriem
             new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

        }
        
        string topic_to_publish = "hello_message";
        string topic_to_publish_message = "Hello world";

        private void MQTT_publish(string topic_tb_publish, string topic_tb_publish_message, int MQTT_broker)
        {
            //int list_len = alist.Count;
            ushort msgId;
            
            if (MQTT_broker == 1)
                msgId = client1.Publish(topic_tb_publish, // topic
                                          Encoding.UTF8.GetBytes(topic_tb_publish_message), // message body
                                          MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, // QoS level
                                          false); // retained
            if (MQTT_broker == 2)
                msgId = client2.Publish(topic_tb_publish, // topic
                                          Encoding.UTF8.GetBytes(topic_tb_publish_message), // message body
                                          MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, // QoS level
                                          false); // retained
             
        }

        void Client_MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
        {
            // this is an (optional) echo of what I have published
            Debug.WriteLine("MessageId = " + e.MessageId + " Published = " + e.IsPublished);
        }

        string[] Input_node_name = new string[_GridMonK_Max_Nodes_input_mapping];
        int[] GridMonK_node_number = new int[_GridMonK_Max_Nodes_input_mapping];
        string[] GridMonK_node_name = new string[_GridMonK_Max_Nodes_input_mapping];

        string[] Input_line_node1_name = new string[_GridMonK_Max_Lines_input_mapping];
        string[] Input_line_node2_name = new string[_GridMonK_Max_Lines_input_mapping];
        int[] GridMonK_line_number = new int[_GridMonK_Max_Lines_input_mapping];
        string[] GridMonK_line_name = new string[_GridMonK_Max_Lines_input_mapping];

        volatile string message_received = "";
        double received_lines_q1, received_nodes_u3;
        String received_date;
        void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            // send event to each process which may need to receive data
            MQTT_Received_Data_Wisegrid(e);
            MQTT_Received_Data_S4G(e);
            MQTT_Received_Data_SCADA_FEP(e);

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





    }
}