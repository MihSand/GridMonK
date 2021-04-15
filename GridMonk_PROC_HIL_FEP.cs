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

        private void PROC_ini_HIL_FEP()
        {
            // read HIL_FEP config file "PROC_HIL_FEP_config_file.txt"

            // 
        }

        int buffer_receive_ptr = 0;
        byte[] buffer_send = new byte[100];
        byte[] buffer_receive = new byte[250];
        private void Read_UI_settings()
        {
            int seri_bytes = 0;
            string converted;
            buffer_receive_ptr = 0;
            // first send command
            buffer_send[0] = (byte)'^'; // ask for all UI settings
            serialPort1.Write(buffer_send, 0, 1);
            // wait for answer
            int w1 = 0;
            while ((serialPort1.BytesToRead == 0) && (w1 < 40))
            {
                System.Threading.Thread.Sleep(2);
                w1++;
            }
            if (w1 > 39) richTextBox_events.Text += "STM32 does not answer !\n";

            while ((serialPort1.BytesToRead != 0) && (buffer_receive_ptr < 100)) // condition to have max 100 bytes
            {
                seri_bytes = serialPort1.ReadByte();
                buffer_receive[buffer_receive_ptr] = (byte)seri_bytes;
                buffer_receive_ptr++;
                if (buffer_receive_ptr > 249) buffer_receive_ptr = 249;
                //System.Threading.Thread.Sleep(1);
            }
            converted = Encoding.UTF8.GetString(buffer_receive, 0, buffer_receive_ptr);
            richTextBox_events.Text += converted + "\n";
        }

        private void HIL_FEP_action1()
        {
            if (first_serial_action == 0)
            {
                serialPort1.PortName = HIL_FrontEnd[0, HIL_FrontEnd_PROP_ComAddr]; // "COM6";
                serialPort1.BaudRate = 115200;
                try { 
                    serialPort1.Open();
                    serialPort1.DiscardInBuffer();
                    first_serial_action = 1;
                } catch
                {
                    richTextBox_events.Text = "Serial port \"" + serialPort1.PortName + "\" could not be opened";
                }
            }
            // read I, I, and U_fi, I_fi from the LF results
            double U1 = 0, refmu = 0;
            if (lines[9, lines_PROP_U1] != "")
            {
                U1 = double.Parse(lines[9, lines_PROP_U1]);
                refmu = U1 / 230 / 2;
                if (refmu > 0.6) refmu = 0.6;
            }
            double I1 = 0, refmi = 0;
            if (lines[9, lines_PROP_I1] != "")
            {
                I1 = double.Parse(lines[9, lines_PROP_I1]);
                refmi = I1 / 400;
                if (refmi > 0.5) refmi = 0.5;
            }
            double U1fi=0, refau = 0; ;
            if (lines[9, lines_PROP_U1fi] != "")
            {
                U1fi = double.Parse(lines[9, lines_PROP_U1fi]);
                refau = U1fi;
                if (refau < 0) refau = 360 - refau;
                refau = refau * 3.1415926 / 180;
                if (refau > Math.PI) refau = Math.PI;
            }
            double I1fi=0, refai = 0;
            if (lines[9, lines_PROP_I1fi] != "")
            {
                I1fi = double.Parse(lines[9, lines_PROP_I1fi]);
                refai = I1fi;
                if (refai < 0) refai = 360 - refai;
                refai = refai * 3.1415926 / 180;
                if (refau > Math.PI) refau = Math.PI;
            }
            string port = lines[9, lines_PROP_OutUI];

            // Initial readout of the UI generators
            //Read_UI_settings();
            //System.Threading.Thread.Sleep(100);
            // Transmission of HIL_FEP
            string str1 = "";
            int s_length = 0;
            byte[] sb;

            if (first_serial_action == 1) { // Only if the serial port has been succesfully opened
                str1 = "@refmu=0.15" + "\n";
                //str1 = "@refmu="+refmu.ToString("#0.000") + "\n";
                richTextBox_events.Text = "[U1  =" + U1.ToString() + "]-> " + str1;
                s_length = str1.Length;
                sb = Encoding.ASCII.GetBytes(str1);
                 serialPort1.Write(sb, 0, s_length);
                System.Threading.Thread.Sleep(200);
                //Read_UI_settings();

                str1 = "@refau=0" + "\n";
                //str1 = "@refau=" + refau.ToString("#0.000") + "\n";
                richTextBox_events.Text += "[U1fi=" + U1fi.ToString() + "]-> " + str1;
                s_length = str1.Length;
                sb = Encoding.ASCII.GetBytes(str1);
                serialPort1.Write(sb, 0, s_length);
                System.Threading.Thread.Sleep(200);
                //Read_UI_settings();

                str1 = "@refmi=0.20" + "\n";
                //str1 = "@refmi=" + refmi.ToString("#0.000") + "\n";
                richTextBox_events.Text += "[I1=  " + I1.ToString() + "]-> " + str1;
                s_length = str1.Length;
                sb = Encoding.ASCII.GetBytes(str1);
                serialPort1.Write(sb, 0, s_length);
                System.Threading.Thread.Sleep(200);
                //Read_UI_settings();

                str1 = "@refai=0" + "\n";
                //str1 = "@refai=" + refai.ToString("#0.000") + "\n";
                richTextBox_events.Text += "[I1fi=" + I1fi.ToString() + "]-> " + str1;
                s_length = str1.Length;
                sb = Encoding.ASCII.GetBytes(str1);
                serialPort1.Write(sb, 0, s_length);
                System.Threading.Thread.Sleep(200);

                Read_UI_settings(); // REad back the setting, to test if they have been well received
                // TBD the comparison
            }
        }

        int first_serial_action = 0;
        private void timer3_PROC_HIL_FEP(object sender, EventArgs e)
        {
            // This request will be made each 1 second, from main timer3.
            //HIL_FEP_action1();
        }

    }
}