/*
 * Grid-MonK is an open source softwaer application intended to annalize power grids, esepcially microgrids
 * The basic variant works by invoking the OpenDSS open-source application, with input files prepared by Grid_MonK
 * and export output files read by OpenDSS and used for further calulations and for interracting with a grid specialist.
 * Grid-Monk can be used and modified by anybody, the only condition is to keep these comments unchanged in the upper
 * part of the used or modified application
 * There is no guarrantee given for any functionality or for any
 * influence on the computer(s) running this applications or on other applications which run on the computer(s)
 * Initiator of the Grid-Monk application: Mihai Sanduleac, University Politehnica of Bucharest, Romania
 * This module is initially developed to implement Hardware in the loop (HIL) functionality in Storage4Grid H2020 project for the Romanian use-case
 * Contributors: Mihai Sanduleac, Andrei Tudose, Vlad Sanduleac
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
        //Client mosqsub/17072-RPI-TEST0 received PUBLISH (d0, q0, r0, m0, '/LESSAg/SMX/LESSAg_ER_Data', ... (61 bytes))
        // {"ER_mode":"0.0","P_BAT_real":"0.0","PPv":"0.0","SoC":"-123.27959""P_CONS":"0"}
        // {"ER_mode":"3.0","PBat":"0.0","PBat_set":"80.0","PPv":"0.09655807","SoC":"20.0","UPv":"1.1810849","Ubat":"91.700745"}
        // {"ER_mode":"3.0","Ibat":"0.0","PBat":"0.0","PBat_set":"80.0","PGrid":"-52.073196","PPv":"0.12612382","QGrid":"-66.43855","SoC":"20.0","UPv":"1.6599782","Ubat":"91.65485"}
        // {"ER_PGrid":"-55.0563","ER_QGrid":"-70.068085","ER_UGrid":"265.18634","ER_mode":"3.0","Ibat":"0.0","PBat":"0.0","PBat_set":"80.0","PPv":"0.15351124","SMXDateTime":"11/26/2019 20:08:50","SoC":"20.0","UPv":"2.4245253","Ubat":"91.708405"}
        // {"ER_PGrid":"-51.937653","ER_QGrid":"-73.473145","ER_UGrid":"265.6218","ER_mode":"3.0","Ibat":"0.0","PBat":"0.0","PBat_set":"80.0","PPv":"0.13027498","SMXDateTime":"11/26/2019 20:15:15","SoC":"20.0","U1_PCC":"223.9","UPv":"1.4696689","Ubat":"91.679245"}
        // {"ER_PGrid":"-52.553516","ER_QGrid":"-71.67967","ER_UGrid":"262.57367","ER_mode":"3.0","Ibat":"0.0","PBat":"0.0","PBat_set":"80.0","PPv":"0.22794218","SMXDateTime":"11/26/2019 20:21:15","SoC":"20.0","U1_PCC":"224.0","UPv":"2.2500453","Ubat":"91.66137"}
        // {"ER_PGrid":"-54.8513","ER_QGrid":"-70.34186","ER_UGrid":"254.8808","ER_mode":"3.0","IBat":"0.0","PBat":"0.0","PBat_set":"80.0","PPv":"0.19908737","SMXDateTime":"11/26/2019 20:30:30","SoC":"20.0","U1_PCC":"224.3","U2_PCC":"222.0","U3_PCC":"225.70000000000002","UBat":"91.70719","UPv":"2.0962303"}
        // {"ER_PGrid":"-56.307663","ER_QGrid":"-70.771324","ER_UGrid":"262.7188","ER_mode":"3.0","IBat":"0.0",
        //    "P1_PCC":"232","P2_PCC":"0","P3_PCC":"94","PBat":"0.0","PBat_set":"80.0","PPv":"0.15338075","Q1_PCC":"182","Q2_PCC":"0","Q3_PCC":"76",
        //    "SMXDateTime":"11/26/2019 20:37:10","SoC":"20.0","U1_PCC":"224.10000000000002","U2_PCC":"222.3","U3_PCC":"225.9",
        //    "UBat":"91.68706","UPv":"1.8634341"}
        // {"Am_PCC":"18556.100000000002","Ap_PCC":"7165997.100000001","ER_PGrid":"-53.064434","ER_QGrid":"-72.39636","ER_UGrid":"250.81664",
        //    "ER_mode":"3.0","IBat":"0.0","P1_PCC":"233","P2_PCC":"0","P3_PCC":"633","PBat":"0.0","PBat_set":"80.0","PPv":"0.14650382",
        //    "Q1_PCC":"182","Q2_PCC":"0","Q3_PCC":"13","Rm_PCC":"3404277.4000000004","Rp_PCC":"11365.2","SMXDateTime":"11/26/2019 20:43:35",
        //    "SoC":"20.0","U1_PCC":"224.10000000000002","U2_PCC":"222.3","U3_PCC":"224.5","UBat":"91.70137","UPv":"1.7798858"}
        // {"Am_PCC":"18556.100000000002","Ap_PCC":"7166062.800000001","ER_PGrid":"-59.999847","ER_QGrid":"-68.20898","ER_UGrid":"261.55762",
        //     "ER_mode":"3.0","IBat":"0.0","K1_PCC":"0.78","K2_PCC":"2.0","K3_PCC":"0.77","P1_PCC":"232","P2_PCC":"0","P3_PCC":"94",
        //     "PBat":"0.0","PBat_set":"80.0","PPv":"0.16463186","P_PCC":"327","Q1_PCC":"183","Q2_PCC":"0","Q3_PCC":"77","Q_PCC":"259",
        //     "Rm_PCC":"3404307.3000000003","Rp_PCC":"11365.2","SMXDateTime":"11/26/2019 20:52:25","SoC":"20.0",
        //     "U1_PCC":"224.70000000000002","U2_PCC":"222.8","U3_PCC":"226.5","UBat":"91.69916","UPv":"1.8572586","f_PCC":"49.900000000000006"}
        // {"Am_PCC":"18556.100000000002","Ap_PCC":"7166117.600000001","ER_PGrid":"-51.833862","ER_QGrid":"-73.50863","ER_UGrid":"278.68515",
        //     "ER_mode":"3.0","IBat":"0.0","K1_PCC":"0.78","K2_PCC":"2.0","K3_PCC":"0.77","P1_PCC":"233","P2_PCC":"0","P3_PCC":"94",
        //     "PBat":"0.0","PBat_set":"80.0","PPv":"0.08411516","P_PCC":"328","Q1_PCC":"183","Q2_PCC":"0","Q3_PCC":"76","Q_PCC":"258",
        //     "Rm_PCC":"3404350.5","Rp_PCC":"11365.2","SMM_Time_PCC":"11/26/2019 21:59:12","SMXCpuLoad_PCC":"4.03","SMXCpuTemp_PCC":"51.5",
        //     "SMXDateTime_PCC":"11/26/2019 21:02:25","SoC":"20.0","U1_PCC":"225.20000000000002","U2_PCC":"223.0","U3_PCC":"226.8",
        //     "UBat":"91.67081","UPv":"1.1070814","f_PCC":"49.900000000000006"}
        /*
            {"Am_PCC":"18558.9","Ap_PCC":"7216156.800000001","ER_PGrid":"-75.08131","ER_QGrid":"-72.28873","ER_UGrid":"270.99228",
            "ER_mode":"3.0","IBat":"-0.26651138","K1_PCC":"0.79","K2_PCC":"2.0","K3_PCC":"0.99",
            "LESSAg_ER_Mode_cd":"[{\"v\":3.0,\"u\":\"\", \"t\":0, \"n\":\"\"}]","LESSAg_ER_PBat_cd":"[{\"v\":-25.0,\"u\":\"\", \"t\":0, \"n\":\"\"}]",
            "LESSAg_ER_QGrid_cd":"[{\"v\":0.0,\"u\":\"\", \"t\":0, \"n\":\"\"}]","P1_PCC":"233","P2_PCC":"0","P3_PCC":"191","PBat":"-25.18127",
            "PBat_set":"-25.0","PPv":"0.11794956","P_PCC":"424","Q1_PCC":"177","Q2_PCC":"0","Q3_PCC":"14","Q_PCC":"163","Rm_PCC":"3430897.6",
            "Rp_PCC":"11373.2","SMM_Time_PCC":"12/01/2019 19:35:55","SMXCpuLoad_PCC":"4.40","SMXCpuTemp_PCC":"52.599998474121094",
            "SMXDateTime_PCC":"12/01/2019 18:39:10","SoC":"44.355858","U1_PCC":"222.5","U2_PCC":"220.70000000000002","U3_PCC":"225.0",
            "UBat":"94.48479","UPv":"1.5523927","f_PCC":"50.0"}


         */



        public class LESSAg_ER_Data
        {
            [DefaultValue("0.0")]
            [JsonProperty("ER_PGrid", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String ER_PGrid_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("ER_QGrid", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String ER_QGrid_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("ER_UGrid", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String ER_UGrid_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("ER_mode", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String ER_mode { get; set; } 

            [DefaultValue("0.0")]
            [JsonProperty("PBat", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String P_BAT_real_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("UBat", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String U_BAT_real_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("IBat", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String I_BAT_real_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("PBat_set", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String P_BAT_setpoint_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("U_DC220", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String U_DC220_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("PPv", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String P_PV_MQTT { get; set; }
            [DefaultValue("0.0")]
            [JsonProperty("UPv", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String U_PV_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("SoC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String SoC_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("LESSAg_ER_PBat_cd", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String LESSAg_ER_PBat_cd { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("LESSAg_ER_QGrid_cd", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String LESSAg_ER_QGrid_cd { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("LESSAg_ER_Mode_cd", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String LESSAg_ER_Mode_cd { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("U1_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String U1_PCC_MQTT { get; set; }
            [DefaultValue("0.0")]
            [JsonProperty("U2_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String U2_PCC_MQTT { get; set; }
            [DefaultValue("0.0")]
            [JsonProperty("U3_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String U3_PCC_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("P_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String P_PCC_MQTT { get; set; }
            [DefaultValue("0.0")]
            [JsonProperty("P1_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String P1_PCC_MQTT { get; set; }
            [DefaultValue("0.0")]
            [JsonProperty("P2_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String P2_PCC_MQTT { get; set; }
            [DefaultValue("0.0")]
            [JsonProperty("P3_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String P3_PCC_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("Q_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String Q_PCC_MQTT { get; set; }
            [DefaultValue("0.0")]
            [JsonProperty("Q1_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String Q1_PCC_MQTT { get; set; }
            [DefaultValue("0.0")]
            [JsonProperty("Q2_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String Q2_PCC_MQTT { get; set; }
            [DefaultValue("0.0")]
            [JsonProperty("Q3_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String Q3_PCC_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("Ap_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String Ap_PCC_MQTT { get; set; }
            [DefaultValue("0.0")]
            [JsonProperty("Am_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String Am_PCC_MQTT { get; set; }
            [DefaultValue("0.0")]
            [JsonProperty("Rp_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String Rp_PCC_MQTT { get; set; }
            [DefaultValue("0.0")]
            [JsonProperty("Rm_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String Rm_PCC_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("K1_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String K1_PCC_MQTT { get; set; }
            [DefaultValue("0.0")]
            [JsonProperty("K2_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String K2_PCC_MQTT { get; set; }
            [DefaultValue("0.0")]
            [JsonProperty("K3_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String K3_PCC_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("f_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String f_PCC_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("SMXDateTime_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String SMXDateTime_PCC_MQTT { get; set; }
            
            [DefaultValue("0.0")]
            [JsonProperty("SMM_Time_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String SMM_Time_PCC_MQTT { get; set; }

            [DefaultValue("0.0")]
            [JsonProperty("SMXCpuTemp_PCC", DefaultValueHandling = DefaultValueHandling.Populate)]
            public String SMXCpuTemp_PCC_MQTT { get; set; }
            

            //[DefaultValue("0.0")]
            //[JsonProperty("P_CONS_MQTT", DefaultValueHandling = DefaultValueHandling.Populate)]
            //public String P_CONS_MQTT { get; set; }
        }
        // {"ER_mode":"3.0","PBat":"79.41042","PPv":"12.247213","SoC":"43.433002"}
        string er_mode;
        double P_BAT_real_MQTT = 0;
        double P_BAT_real_MQTT_Ap = 0, P_BAT_real_MQTT_Am=0;
        double P_PV_MQTT = 0, U_PV_MQTT = 0, SoC_MQTT = 0, P_CONS_MQTT = 0;
        double P_BAT_setpoint_MQTT = 0, U_BAT_real_MQTT = 0, I_BAT_MQTT = 0;
        double U_DC220_MQTT = 0;
        double ER_PGrid_MQTT = 0, ER_QGrid_MQTT = 0;
        string LESSAg_ER_PBat_cd = "";
        string LESSAg_ER_QGrid_cd = "";
        string LESSAg_ER_Mode_cd = "";
        double U1_PCC_MQTT = 0, U2_PCC_MQTT = 0, U3_PCC_MQTT = 0;
        double P_PCC_MQTT = 0, P1_PCC_MQTT = 0, P2_PCC_MQTT = 0, P3_PCC_MQTT = 0;
        double Q_PCC_MQTT = 0, Q1_PCC_MQTT = 0, Q2_PCC_MQTT = 0, Q3_PCC_MQTT = 0;
        double K1_PCC_MQTT = 0, K2_PCC_MQTT = 0, K3_PCC_MQTT = 0;
        double f_PCC_MQTT = 0;
        double Ap_PCC_MQTT = 0, Am_PCC_MQTT = 0, Rp_PCC_MQTT = 0, Rm_PCC_MQTT = 0;
        double P_PCC_LSG = 0; // fost P_PCC, reprezinta P_PCC calculate de LESSAg
        double P_PV_after_curtail = 0; 
        double P_PCC_real_values = 0; // calculated from P_CONS, P_PV and P_Storage in realtime
        string SMXDateTime_PCC_MQTT = "", SMM_Time_PCC_MQTT = "";
        double SMXCpuTemp_PCC_MQTT = 0;

        int MQTT_Received_messages = 0;
        int MQTT_Received_and_filtered_messages = 0;

        // LESSAg/ER/ER_Mode = [{"v":3.0,"u":"", "t":0, "n":""}]
        // LESSAg/ER/PBat = [{"v":40.0,"u":"", "t":0, "n":""}]
        // LESSAg/ER/QGrid = [{"v":0.0,"u":"", "t":0, "n":""}]
        string topic1 = "";
        string payload1 = "";
        //string topic1 = "LESSAg/ER/PBat/Cd";
        //string payload1 = "[{\"v\":40.0,\"u\":\"\", \"t\":0, \"n\":\"\"}]";

        // Variables of S4G module which are exposed to external cgange through GridMonK "interract" which can manipulate "_ProcessVar" varibles
        const int S4G_EXT_var_max_number = 20, S4G_EXT_var_max_prop = 4;
        string[,] S4G_var = new string[S4G_EXT_var_max_number, S4G_EXT_var_max_prop];
        // allocation of variables meaning
        const int S4G_var_PROP_P_bat_setp = 0;
        const int S4G_var_PROP_Q_grid_setp = 1;
        const int S4G_var_PROP_ER_mode_setp = 2;
        const int S4F_var_PROP_Cons_scaling_factor = 4;
        const int S4F_var_PROP_PV_scaling_factor = 5;
        const int S4F_var_PROP_Storage_scaling_factor = 6;
        const int S4F_var_PROP_P_Cons_SeTpoint = 7;
        const int S4F_var_PROP_P_factor = 8; // P_factor when copied in the  load into the simulated grid
        const int S4F_var_PROP_P_source = 9; // P_factor when copied in the  load into the simulated grid


        void MQTT_Published_Data_S4G()
        {
            if (MQTT_Connect == "S4G")
            if ((topic1 != "") && (payload1 != ""))
            {
                MQTT_publish(topic1, payload1, 1);
                topic1 = ""; payload1 = ""; // the strings become back empty, such that only a new command will generate 
            }
        }

        void MQTT_Received_Data_S4G(MqttMsgPublishEventArgs e)
        {
            Debug.WriteLine("Received = " + Encoding.UTF8.GetString(e.Message) + " on topic " + e.Topic);
            message_received = Encoding.UTF8.GetString(e.Message);
            MQTT_Received_messages++;
            if (e.Topic == "/LESSAg/SMX/LESSAg_ER_Data")
            {
                var serializer = new JsonSerializer();
                LESSAg_ER_Data res = serializer.Deserialize<LESSAg_ER_Data>(new JsonTextReader(new StringReader(Encoding.UTF8.GetString(e.Message))));

                MQTT_Received_and_filtered_messages++;

                er_mode = res.ER_mode;
                P_BAT_real_MQTT = Convert.ToDouble(res.P_BAT_real_MQTT);    // P_BAT masurat de ER
                P_BAT_setpoint_MQTT = Convert.ToDouble(res.P_BAT_setpoint_MQTT);
                U_BAT_real_MQTT = Convert.ToDouble(res.U_BAT_real_MQTT);
                I_BAT_MQTT = Convert.ToDouble(res.I_BAT_real_MQTT);
                P_PV_MQTT = Convert.ToDouble(res.P_PV_MQTT);
                U_PV_MQTT = Convert.ToDouble(res.U_PV_MQTT);
                SoC_MQTT = Convert.ToDouble(res.SoC_MQTT);          // SoC calculat de ER
                U_DC220_MQTT = Convert.ToDouble(res.U_DC220_MQTT); // tensiune bara 220 V DC
                LESSAg_ER_PBat_cd = res.LESSAg_ER_PBat_cd;
                LESSAg_ER_QGrid_cd = res.LESSAg_ER_QGrid_cd;
                LESSAg_ER_Mode_cd = res.LESSAg_ER_Mode_cd;
                // ER exchanged power with the grid
                ER_PGrid_MQTT = Convert.ToDouble(res.ER_PGrid_MQTT);
                ER_QGrid_MQTT = Convert.ToDouble(res.ER_QGrid_MQTT);
                
                U1_PCC_MQTT = Convert.ToDouble(res.U1_PCC_MQTT);
                U2_PCC_MQTT = Convert.ToDouble(res.U2_PCC_MQTT);
                U3_PCC_MQTT = Convert.ToDouble(res.U3_PCC_MQTT);

                P_PCC_MQTT = Convert.ToDouble(res.P_PCC_MQTT);
                P1_PCC_MQTT = Convert.ToDouble(res.P1_PCC_MQTT);
                P2_PCC_MQTT = Convert.ToDouble(res.P2_PCC_MQTT);
                P3_PCC_MQTT = Convert.ToDouble(res.P3_PCC_MQTT);

                Q_PCC_MQTT = Convert.ToDouble(res.Q_PCC_MQTT);
                Q1_PCC_MQTT = Convert.ToDouble(res.Q1_PCC_MQTT);
                Q2_PCC_MQTT = Convert.ToDouble(res.Q2_PCC_MQTT);
                Q3_PCC_MQTT = Convert.ToDouble(res.Q3_PCC_MQTT);

                Ap_PCC_MQTT = Convert.ToDouble(res.Ap_PCC_MQTT);
                Am_PCC_MQTT = Convert.ToDouble(res.Am_PCC_MQTT);
                Rp_PCC_MQTT = Convert.ToDouble(res.Rp_PCC_MQTT);
                Rm_PCC_MQTT = Convert.ToDouble(res.Rm_PCC_MQTT);

                K1_PCC_MQTT = Convert.ToDouble(res.K1_PCC_MQTT);
                K2_PCC_MQTT = Convert.ToDouble(res.K2_PCC_MQTT);
                K3_PCC_MQTT = Convert.ToDouble(res.K3_PCC_MQTT);

                f_PCC_MQTT = Convert.ToDouble(res.f_PCC_MQTT);

                SMXDateTime_PCC_MQTT = res.SMXDateTime_PCC_MQTT;
                SMM_Time_PCC_MQTT = res.SMM_Time_PCC_MQTT;

                SMXCpuTemp_PCC_MQTT = Convert.ToDouble(res.SMXCpuTemp_PCC_MQTT);
                

            }
        }
        
        const int mode_DiskFile = 0; //data comes from a file located on disk
        const int mode_LOOP = 1; //data comes from a software loop, as follows: input(t)=output(t-1)
        const int mode_MQTTmessage = 2; //data comes from a real process via MQTT messages


        string activate_S4G_HIL = "no";
        string PROC_S4G_data_path = "";
        string LESSAg_project_name = "";
        // Data on files related to conspmtion LPs
        string[] P_CONS_profile_file_name = new string[Profiles_MAX_number];
        string[] P_CONS_profile_date = new string[Profiles_MAX_number];
        string[] P_CONS_profile_info = new string[Profiles_MAX_number];

        string P_CONS_profile_forecast_24h__file_name = "";
        double P_CONS_profile_forecast_factor = 1;
        double P_CONS_profile_forecast_time_period = 1;


        string P_PV_profile_meteo_foreacst_24h_file_name = "";
        double P_PV_profile_meteo_forecast_24h_factor = 1;
        double PV_forecast_time_period = 1;

        string Battery_forecast_24h_file_name = "";

        const int Profiles_MAX_number = 12; // more than 86400 points, which correspond to one day, each 1 second
        const int Profiles_MAX_length = 99000; // more than 86400 points, which correspond to one day, each 1 second

        string sim_type = "fullday";
        int sim_time = -1;//default value =0, means the algorithm is going to run until the end of day
        const int simulation_until_end_of_day = -1;
        double[,] P_CONS_profile = new double[Profiles_MAX_number, Profiles_MAX_length]; // Profile for consumption
        int P_cons_time_period = 1;

        double[] P_PV_meteo_profile = new double[Profiles_MAX_length]; // Profile for PV meteo (no curtailment)
        int P_PV_time_period = 1;
        string P_PV_profile_meteo_file_name = "";
        string P_SaaS_profile_file_name = "P_SaaS_profile_01.txt"; 

        double[] P_SaaS_profile = new double[Profiles_MAX_length]; // Profile for Storage as a Service (SaaS)
        int[] P_SaaS_profile_partner = new int[Profiles_MAX_length]; // Partner for the Storage as a Service (SaaS)
        

        string EMS_strategy = "";
        double P_BAT_max_inverters;
        double P_Cons_SeTpoint = 500;
        double Battery_charging_efficiency = 1;
        double Battery_discharging_efficiency = 1;
        string battery_limitation = "";
        int battery_mode = 0;
        double E_bat_nominal = 0;
        double E_bat_max = 0;
        double E_bat_min = 0;
        double SoC_init = 0;
        int T_integration=0;
        int nr_tot_logs = 0;

        //Meter variables
        double Meter_E_BAT_real_charging = 0; // Wh
        double Meter_E_BAT_real_discharging = 0; // Wh
        double P_PCC_Ap = 0; // meter index A plus
        double P_PCC_Am = 0; // meter index A minus
        double P_CONS_Ap = 0;
        double P_PV_meteo_Am = 0;
        double P_PV_after_curtail_Am = 0;
        double P_PV_curtail_Am = 0; // PV energy curtailed

        double PV_time_period = 1;
        double PV_scaling_factor = 1;
        int P_PV_profile_meteo_shift = 0; // in hours
        double bat_scaling_factor = 1;
        double bat_time_period = 1;
        double Cons_time_period = 1;
        double Cons_scaling_factor = 1;

        int process_time_period = 1;

        int P_cons_mode;
        int P_PV_meteo_mode;
        int P_PV_after_curtail_mode, P_bat_mode, SoC_mode, P_PV_curtail_mode;
        // P_CONS and P_PV_METEO and (forecasts!!!!) can be in one of the following modes:
        //0 - data comes from a file located on disk (pre-recorded data)
        //1 - data comes from a "software loop" (the input value at current time(t) is the output value at time t-1)
        //2 - data comes as MQTT messages from a real process (hardware in the loop). In this mode, output values at time t may differ from input values at time t+1


        double[] P_CONS_Profile_forecast_24h = new double[MEM_MAX_length];//!!!!!!!!!!!!!!!!!!!!1
        double[] P_PV_profile_meteo_forecast_24h = new double[MEM_MAX_length];//!!!!!!!!!!!!!!!!!!!!!!!
        int PCC_injection_allowed = 0; //Default=0: we are not allowed to be in injection mode towards the system. P_PCC is always positive. PV curtailment is applied in order to realise this

        private void S4G_EXT_Functions(int function_number)
        {
            if(function_number==0)
            {
                Read_SaaS_file();
            }
        }

        private void S4G_EXT_Function_GPH()
        {
            for (int h1 = 0; h1 < 24; h1++)
            {
                // salvare in canalul grafic 1
                SimpleGph_channels1[0, h1+1] = P_CONS_profile[Profiles_crt_number,Realtime_interval_pointer + h1];
                // salvare in canalul grafic 2
                SimpleGph_channels1[1, h1+1] = P_SaaS_profile[Realtime_interval_pointer + h1];
                SimpleGph_channels1[2, h1 + 1] = P_PV_meteo_profile[Realtime_interval_pointer + h1] * PV_scaling_factor;
                if(Realtime_interval_pointer>24)
                    SimpleGph_channels1[3, h1 + 1] = P_PV_meteo_profile[Realtime_interval_pointer + h1-24] * PV_scaling_factor;
            }
            channel_name[0] = "P_CONS";
            channel_name[1] = "P_SaaS";
            channel_name[2] = "P_PV_nxt";
            channel_name[3] = "P_PV_his";
        }

        int prosumer_pair_old = 0;
        private void Paint_Prosumer(object sender, PaintEventArgs e)
        {
            string s1 = "";
            int x0 = 0;
            int y0 = 0;
            if (prosumers_no == 0) return;
            if (((prosumers[0, prosumers_PROP_x0]) != null) && ((prosumers[0, prosumers_PROP_x0]) != "")) x0 = int.Parse(prosumers[0, prosumers_PROP_x0]);
            if (((prosumers[0, prosumers_PROP_y0]) != null) && ((prosumers[0, prosumers_PROP_y0]) != "")) y0 = int.Parse(prosumers[0, prosumers_PROP_y0]);
            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);

            string prosumer_name = prosumers[0, prosumers_PROP_name];
            g.DrawString("Name = " + prosumer_name, Font1, b5DarkBlue, x0 - 400, y0 + 18 * 1);
            string prosumer_load_no = prosumers[0, prosumers_PROP_load_number];
            g.DrawString("Load # = "+ prosumer_load_no, Font1, b5DarkBlue, x0 - 400, y0 + 18 * 2);
            //string prosumer_load_no = prosumers[0, prosumers_PROP_load_number];
            g.DrawString("Load Name = " + loads[2, loads_PROP_name], Font1, b5DarkBlue, x0 - 400, y0 + 18 * 3);
            string prosumer_P_scal_factor = prosumers[0, prosumers_PROP_P_scal_factor];
            g.DrawString("P_factor = " + prosumer_P_scal_factor, Font1, b5DarkBlue, x0 - 400, y0 + 18 * 4);
            string prosumer_P_source = prosumers[0, prosumers_PROP_P_source2grid]; // PCC, PV, CONS
            g.DrawString("P_source = " + prosumer_P_source, Font1, b5DarkBlue, x0 - 400, y0 + 18 * 5);

            if (P_cons_mode == 0) s1 = "Disk"; else if (P_cons_mode == 1) s1 = "SwLoop"; else if (P_cons_mode == 2) s1 = "MQTT"; else s1 = "?";
            s1 = "P_CONS_mode= " + s1;
            g.DrawString(s1, Font1, b5DarkBlue, x0 - 280, y0 + 18*1);
            if (P_PV_meteo_mode == 0) s1 = "Disk"; else if (P_PV_meteo_mode == 1) s1 = "SwLoop"; else if (P_PV_meteo_mode == 2) s1 = "MQTT"; else s1 = "?";
            s1 = "P_PV_mode= " + s1;
            g.DrawString(s1, Font1, b5DarkBlue, x0 - 280, y0 + 18 * 2);
            if (P_bat_mode == 0) s1 = "Disk"; else if (P_bat_mode == 1) s1 = "SwLoop"; else if (P_bat_mode == 2) s1 = "MQTT"; else s1 = "?";
            s1 = "P_bat_mode= " + s1;
            g.DrawString(s1, Font1, b5DarkBlue, x0 - 280, y0 + 18 * 3);
            s1 = "U_DC220= " + U_DC220_MQTT.ToString("###0.000");
            g.DrawString(s1, Font1, b5DarkBlue, x0 - 280, y0 + 18 * 4);

            if (LSG_P_BAT_cd_setpoint > 0) s1 = "Charging_> " + LSG_P_BAT_cd_setpoint.ToString("###0.0");
            else if (LSG_P_BAT_cd_setpoint < 0) s1 = "DisCharg< " + LSG_P_BAT_cd_setpoint.ToString("###0.0");
            else if (LSG_P_BAT_cd_setpoint == 0) s1 = "___Bat_Idle___";
            g.DrawString(s1, Font1, b5DarkBlue, x0 - 280, y0 + 18 * 5);

            if (P_BAT_real_MQTT < 0) s1 = "ChargingR_> " + P_BAT_real_MQTT.ToString("###0.0");
            else if (P_BAT_real_MQTT > 0) s1 = "DisChargR< " + P_BAT_real_MQTT.ToString("###0.0");
            else if (P_BAT_real_MQTT == 0) s1 = "___Bat_Idle_R_";
            g.DrawString(s1, Font1, b5DarkBlue, x0 - 280, y0 + 18 * 6);

            s1 = "PV_Am=" + P_PV_meteo_Am.ToString("###0.000");
            g.DrawString(s1, Font1, b5DarkBlue, x0 - 280, y0 + 18 * 7);
            s1 = "Bat_Ap=" + P_BAT_real_MQTT_Ap.ToString("###0.000");
            g.DrawString(s1, Font1, b5DarkBlue, x0 - 280, y0 + 18 * 8);
            s1 = "Bat_Am=" + P_BAT_real_MQTT_Am.ToString("###0.000");
            g.DrawString(s1, Font1, b5DarkBlue, x0 - 280, y0 + 18 * 9);
            s1 = "Pcons_Ap=" + P_CONS_Ap.ToString("####0.000");
            g.DrawString(s1, Font1, b5DarkBlue, x0 - 280, y0 + 18 * 10);


            //DateTime t1 = DateTime.Now;
            //s1 = "T=" + t1.Year.ToString() + "." +t1.Month.ToString("00") +"." + t1.Day.ToString("00") + " " + t1.ToLongTimeString();
            //g.DrawString(s1, Font1, b5DarkBlue, x0 - 320, y0 + 160);

            //s1 += "Time=" + t1.ToLongDateString() + " " + t1.ToLongTimeString() + "\n";

            s1 = "S4G_HIL_cnt_crt= " + S4G_HIL_counter.ToString();
            //g.DrawString(s1, Font1, b5DarkBlue, x0 - 160, y0 + 20);
            g.DrawString(s1, Font1, b5DarkBlue, x0 + 10, y0 + 18 * 10);
            //s1 = "S4G_HIL_cnt_in_day= " + ((sim_start_time / T_integration) + S4G_HIL_counter).ToString();
            s1 = "S4G_HIL_cnt_in_day= " + (sim_start_time+ S4G_HIL_counter).ToString("0000");
            //g.DrawString(s1, Font1, b5DarkBlue, x0 - 160, y0 + 40);
            g.DrawString(s1, Font1, b5DarkBlue, x0 + 10, y0 + 18 * 11);

            /// MEM_P_CONS_in[S4G_HIL_counter]      // LSG_P_CONS
            s1 = "LSG_P_CONS= " + LSG_P_CONS.ToString("#####0.000");
            g.DrawString(s1, Font1, b0Black, x0 - 160, y0 + 18 * 1);
            s1 = "Cons_scaling_factor= " + Cons_scaling_factor.ToString("###0.000");
            g.DrawString(s1, Font1, b0Black, x0 - 160, y0 + 18 * 2);

            s1 = "LSG_P_BAT_cd_stp= " + LSG_P_BAT_cd_setpoint.ToString("#####0.000");
            g.DrawString(s1, Font1, b7DarkGreen, x0 - 160, y0 + 18 * 3);
            s1 = "LSG_P_BAT_cd_in= " + LSG_P_BAT_in.ToString("#####0.000");
            g.DrawString(s1, Font1, b7DarkGreen, x0 - 160, y0 + 18 * 4);
            double p_bat = - P_BAT_real_MQTT * bat_scaling_factor;
            s1 = "LSG_P_BAT_real=" + p_bat.ToString("####0.000");
            g.DrawString(s1, Font1, b7DarkGreen, x0 - 160, y0 + 18 * 5);

            s1 = "bat_scaling_factor= " + bat_scaling_factor.ToString("#####0.00");
            g.DrawString(s1, Font1, b7DarkGreen, x0 - 160, y0 + 18 * 6);

            s1 = "PV_scaling_factor= " + PV_scaling_factor.ToString("#####0.00");
            g.DrawString(s1, Font1, b6Red, x0 - 160, y0 + 18 * 7);
            double p_pv_real = LSG_P_PV_meteo * PV_scaling_factor;
            //s1 = "LSG_P_PV_meteo= " + LSG_P_PV_meteo.ToString("#####0.00");
            s1 = "LSG_P_PV= " + p_pv_real.ToString("#####0.00");
            g.DrawString(s1, Font1, b6Red, x0 - 160, y0 + 18 * 8);
            p_pv_real = - P_PV_MQTT * PV_scaling_factor;
            s1 = "LSG_P_PV_real= " + p_pv_real.ToString("#####0.00");
            g.DrawString(s1, Font1, b6Red, x0 - 160, y0 + 18 * 9);
            if (S4G_HIL_counter != 0)
            {
                s1 = "P_PV_curtail_in[hc]= " + MEM_P_PV_curtail_in[S4G_HIL_counter].ToString("#####0.00");
                g.DrawString(s1, Font1, b6Red, x0 - 160, y0 + 18 * 10);
            }
            s1 = "LSG_SoC= " + (LSG_SoC*100).ToString("###0.0000");
            g.DrawString(s1, Font1, b5DarkBlue, x0 - 160, y0 + 18 * 11);
            s1 = "P_PCC_LSG= " + P_PCC_LSG.ToString("#####0.000");
            g.DrawString(s1, Font1, b5DarkBlue, x0 - 160, y0 + 18 * 12);
            s1 = "P_PCC_real= " + P_PCC_real_values.ToString("#####0.000");
            g.DrawString(s1, Font1, b5DarkBlue, x0 - 160, y0 + 18 * 13);
            s1 = "Pcons_SeTpoint= " + P_Cons_SeTpoint.ToString("#####0.000");
            g.DrawString(s1, Font1, b5DarkBlue, x0 - 160, y0 + 18 * 14);
            if (S4G_HIL_counter != 0)
            {
                s1 = "P_SaaS_profile[rt]= " + P_SaaS_profile[Realtime_interval_pointer].ToString("#####0.000");
                g.DrawString(s1, Font1, b5DarkBlue, x0 - 160, y0 + 18 * 15);
            }


            // ****************** Data from SMX
            s1 = "SoC= " + SoC_MQTT.ToString("#####0.000");
            g.DrawString(s1, Font1, b5DarkBlue, x0 + 10, y0 + 18*1);
            s1 = "P_BAT= " + P_BAT_real_MQTT.ToString(); // aceasta eset puterea data efectiv de ER, care este primita prin mesaje MQTT
            g.DrawString(s1, Font1, b7DarkGreen, x0 + 10, y0 + 18 * 2);
            s1 = "P_BAT_setp= " + P_BAT_setpoint_MQTT.ToString();
            g.DrawString(s1, Font1, b7DarkGreen, x0 + 10, y0 + 18 * 3);
            s1 = "U_BAT= " + U_BAT_real_MQTT.ToString("####0.00");
            g.DrawString(s1, Font1, b7DarkGreen, x0 + 10, y0 + 18 * 4);
            s1 = "I_BAT= " + I_BAT_MQTT.ToString();
            g.DrawString(s1, Font1, b7DarkGreen, x0 + 10, y0 + 18 * 5);
            s1 = "U_DC220= " + U_DC220_MQTT.ToString("##0.00");
            g.DrawString(s1, Font1, b5DarkBlue, x0 + 10, y0 + 18 * 6);
            s1 = "P_PV= " + P_PV_MQTT.ToString("####0.00");
            g.DrawString(s1, Font1, b6Red, x0 + 10, y0 + 18 * 7);
            s1 = "U_PV= " + U_PV_MQTT.ToString("####0.00");
            g.DrawString(s1, Font1, b6Red, x0 + 10, y0 + 18 * 8);

            //g.DrawString(s1, Font1, b5DarkBlue, x0 + 10, y0 + 18 * 9);

            //s1 = "P_ER_grid=" + ER_PGrid_MQTT.ToString("####0.000");
            //g.DrawString(s1, Font1, b5DarkBlue, x0 + 10, y0 + 18 * 10);
            //s1 = "Q_ER_grid=" + ER_QGrid_MQTT.ToString("####0.000");
            //g.DrawString(s1, Font1, b5DarkBlue, x0 + 10, y0 + 18 * 11);

            s1 = "ER_MODE=" + er_mode;
            g.DrawString(s1, Font1, b5DarkBlue, x0 + 10, y0 + 210);

            s1 = "RCV1= " + MQTT_Received_messages.ToString();
            g.DrawString(s1, Font1, b5DarkBlue, x0 + 10, y0 + 230);
            s1 = "RCV2= " + MQTT_Received_and_filtered_messages.ToString();
            g.DrawString(s1, Font1, b5DarkBlue, x0 + 10, y0 + 245);

            s1 = "Pbat_cd=" + LESSAg_ER_PBat_cd;
            g.DrawString(s1, Font1, b5DarkBlue, x0 + 10, y0 + 260);
            s1 = "Qgrd_cd=" + LESSAg_ER_QGrid_cd;
            g.DrawString(s1, Font1, b5DarkBlue, x0 + 10, y0 + 275);
            s1 = "ER_md_cd=" + LESSAg_ER_Mode_cd;
            g.DrawString(s1, Font1, b5DarkBlue, x0 + 10, y0 + 290);

            int zone3 = 0;
            if (zone3 == 1)
            {
                s1 = "U1_PCC= " + U1_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 130, y0 + 20);
                s1 = "U2_PCC= " + U2_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 130, y0 + 40);
                s1 = "U3_PCC= " + U3_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 130, y0 + 60);

                s1 = "P_PCC_MQTT= " + P_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 130, y0 + 90);
                s1 = "P1_PCC= " + P1_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 130, y0 + 110);
                s1 = "P2_PCC= " + P2_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 130, y0 + 130);
                s1 = "P3_PCC= " + P3_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 130, y0 + 150);

                s1 = "Q_PCC= " + Q_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 130, y0 + 180);
                s1 = "Q1_PCC= " + Q1_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 130, y0 + 200);
                s1 = "Q2_PCC= " + Q2_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 130, y0 + 220);
                s1 = "Q3_PCC= " + Q3_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 130, y0 + 240);

                s1 = "Ap_PCC= " + Ap_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 230, y0 + 20);
                s1 = "Am_PCC= " + Am_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 230, y0 + 40);
                s1 = "Rp_PCC= " + Rp_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 230, y0 + 60);
                s1 = "Rm_PCC= " + Rm_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 230, y0 + 80);

                s1 = "K1_PCC= " + K1_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 230, y0 + 110);
                s1 = "K2_PCC= " + K2_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 230, y0 + 130);
                s1 = "K3_PCC= " + K3_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 230, y0 + 150);

                s1 = "f_PCC= " + f_PCC_MQTT.ToString();
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 230, y0 + 180);

                s1 = "Date1= " + SMXDateTime_PCC_MQTT;
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 230, y0 + 210);
                s1 = "Date2= " + SMM_Time_PCC_MQTT;
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 230, y0 + 230);
                s1 = "Temp= " + SMXCpuTemp_PCC_MQTT;
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 230, y0 + 250);
                s1 = "T_PC=" + current_DateTime;
                g.DrawString(s1, Font1, b5DarkBlue, x0 + 230, y0 + 270);
            }

            // Inject HIL value in the simulated grid
            double P_scal_factor = double.Parse(prosumer_P_scal_factor);
            double P_node = P_PCC_real_values* P_scal_factor; // default value if no other condition is met

            if (prosumers[0, prosumers_PROP_P_source2grid] =="PCC") P_node = P_PCC_real_values * P_scal_factor;
            if (prosumers[0, prosumers_PROP_P_source2grid] == "PV") P_node = LSG_P_PV_meteo * P_scal_factor;
            if (prosumers[0, prosumers_PROP_P_source2grid] == "CONS") P_node = LSG_P_CONS * P_scal_factor;

            // change behavior in the grid
            int prosumer_main = 2;
            loads[prosumer_main, loads_PROP_Pn] = P_node.ToString("##0.0");
            loads[prosumer_main, loads_PROP_sim_storage] = "Prosumer";
            double SaaS = P_SaaS_profile[Realtime_interval_pointer];
            loads[prosumer_main, loads_PROP_Prosumer_SaaS] = SaaS.ToString("#####0.000");
            if ((SaaS > 1) || (SaaS < -1)) {
                loads[prosumer_main, loads_PROP_gph_Draw_Highlighted] = "1"; // the main prosumer will be highlighted
            }
            else loads[prosumer_main, loads_PROP_gph_Draw_Highlighted] = "";

            // reset ex prosumers
            loads[prosumer_pair_old, loads_PROP_sim_storage] = "";
            loads[prosumer_pair_old, loads_PROP_Prosumer_SaaS] = "0";
            double Load_pair = P_scal_factor * (double.Parse(loads[prosumer_pair_old, loads_PROP_Pn_buffered]));
            loads[prosumer_pair_old, loads_PROP_Pn] = Load_pair.ToString("##0.0");

            try
            { 
            // The pair of the prosumer, which using/taken the SaaS service
                int prosumer_pair = 1; // default is 1
                if (P_SaaS_profile_partner[Realtime_interval_pointer] != 0)
                    prosumer_pair = P_SaaS_profile_partner[Realtime_interval_pointer];
                loads[prosumer_pair, loads_PROP_sim_storage] = "Prosumer";
                if ((SaaS > 1) || (SaaS < -1)) // If we have a SaaS, the pair will be highlighted
                {
                    loads[prosumer_pair, loads_PROP_gph_Draw_Highlighted] = "1"; // the pair will be highlighted
                }
                else loads[prosumer_pair, loads_PROP_gph_Draw_Highlighted] = "";
                SaaS = -SaaS;
                loads[prosumer_pair, loads_PROP_Prosumer_SaaS] = SaaS.ToString("#####0.000");
                Load_pair = P_scal_factor * (double.Parse(loads[prosumer_pair, loads_PROP_Pn_buffered]) + SaaS);
                loads[prosumer_pair, loads_PROP_Pn] = Load_pair.ToString("##0.0");
                prosumer_pair_old = prosumer_pair;
            } catch { }
        }

        public void Read_SaaS_file()
        {
            int nr_lines = 0;
            string[] lines;
            try
            {
                lines = System.IO.File.ReadAllLines(PROC_S4G_data_path + P_SaaS_profile_file_name);
                foreach (string line in lines)
                {
                    // Use a tab to indent each line of the file.
                    char[] delimiterChars = { '\t', '[' };
                    string[] line1 = line.Split(delimiterChars);
                    int pos = 0;
                    foreach (string s in line1)
                    {
                        //if ((pos == 2) && (nr_lines != 0)) {
                        if ((pos == 2) && (nr_lines != 0))
                        {
                            P_SaaS_profile[nr_lines - 1] = double.Parse(s);
                            P_SaaS_profile[nr_lines - 1] = P_SaaS_profile[nr_lines - 1];
                        }
                        if ((pos == 3) && (nr_lines != 0))
                        {
                            P_SaaS_profile_partner[nr_lines - 1] = int.Parse(s);
                        }
                        pos++;
                    }
                    nr_lines++;
                }
            }
            catch
            {
                Write_GridMonK_log("PROC_ini_S4G=Error4=Missing P_SaaS_profile_file");
            }
            S4G_EXT_Function_GPH();
        }

        public void Battery_command()
        {
            // Simulation of the battery behavior
            if (LSG_E_bat >= E_bat_max) if (LSG_P_BAT_cd_setpoint > 0) LSG_P_BAT = 0;
            double E_plus_max; // Maximum energy quantity possible to be introduced in the battery in the next timey step, in charging mode
            double E_minus_max; // Maximum energy quantity possible to be extracted from the battery in the next timey step, in discharging mode

            if (LSG_P_BAT_cd_setpoint >= 0)
            {
                // We check that the new setpoint, if positive (consumption/chraging mode), will not exceed the total energy of the battery
                E_plus_max = LSG_P_BAT_cd_setpoint * 1 / 3600; 

                if (LSG_E_bat < E_bat_max - E_plus_max)
                {
                    battery_mode = 1; // charging mode
                } // simulare ER, partea de baterie
                else
                {
                    LSG_P_BAT_cd_setpoint = 0;
                    battery_mode = 0; // idle mode
                }
                if (LSG_P_BAT_cd_setpoint == 0) battery_mode = 0;
            }
            else
            {
                // LSG_P_BAT_cd_setpoint<0, meaning we have an order to discharge/produce energy from battery
                // We check that the new setpoint, if negative (produce/discharge mode), will not go below minimum energy accepted for the the battery
                E_minus_max = LSG_P_BAT_cd_setpoint * 1 / 3600;

                if (LSG_E_bat + E_minus_max >= E_bat_min)
                {
                    battery_mode = 2; // discharging/production mode
                }
                else
                {
                    LSG_P_BAT_cd_setpoint = 0;
                    battery_mode = 0; // idle mode
                }
            }

            battery_limitation = "";
            if (LSG_P_BAT_cd_setpoint >= 0)
            {
                if (LSG_P_BAT_cd_setpoint > P_BAT_max_inverters)
                {
                    LSG_P_BAT_cd_setpoint = P_BAT_max_inverters;
                    battery_limitation = " (Lim+)";
                }
            }
            else
            {
                if (LSG_P_BAT_cd_setpoint < -P_BAT_max_inverters)
                {
                    LSG_P_BAT_cd_setpoint = -P_BAT_max_inverters;
                    battery_limitation = " (Lim-)";
                }
            }
            // Battery energy is obtained by integration, considering also effciency for chargind and discharging
            if (LSG_P_BAT_cd_setpoint >= 0) LSG_E_bat = LSG_E_bat + (LSG_P_BAT_cd_setpoint + P_SaaS_profile[Realtime_interval_pointer]) * T_integration / 3600 * Battery_charging_efficiency;
            else LSG_E_bat = LSG_E_bat + (LSG_P_BAT_cd_setpoint + P_SaaS_profile[Realtime_interval_pointer]) * T_integration / 3600 * Battery_discharging_efficiency;

            LSG_SoC = LSG_E_bat / E_bat_max;
            //P_PCC_LSG = - LSG_P_PV_meteo - LSG_P_BAT -P_CONS; // simulare retea inetrna prosumer

        }

        int sim_start_time; // variable which points the starting minute (when GridMonk starts), used to select the right data from files when in RealTime
        double[] Battery_forecast_24h = new double[99600];

        int P_CONS_profile_number = 0;
        private void PROC_ini_S4G()
        {
            // read S4G config file "PROC_S4G_config_file.txt"
            // read consumption_file, scaling factor, time-period for a Pcons record (1s, 5s, 60s) 
            // PV_production_file, scaling factor, scaling factor, time-period for a PPv record (1s, 5s, 60s) 
            //
            for (int i1 = 0; i1 < S4G_EXT_var_max_number; i1++) for (int j1 = 0; j1 < S4G_EXT_var_max_prop; j1++) S4G_var[i1, j1] = "";
            for (int i1=0; i1< Profiles_MAX_number; i1++)
            {
                P_CONS_profile_file_name[i1] = "";
                P_CONS_profile_date[i1] = "";
                P_CONS_profile_info[i1] = "";
            }

            string[] lines;
            try { 
            lines = System.IO.File.ReadAllLines("PROC_S4G_config_file.txt");
            foreach (string line in lines)
            {
                if (line[0] != '#')
                {
                    char[] delimiters_Level1 = { ';' };
                    string[] cmds = line.Split(delimiters_Level1);
                    foreach (string cmd in cmds)
                    {
                        char[] delimiters_Level2 = { '=', '[', ']' }; 
                        string[] line1 = cmd.Split(delimiters_Level2); 
                        if (line1[0].ToLower() == "activate_s4g_hil") activate_S4G_HIL = line1[1];
                        if (line1[0].ToLower() == "proc_s4g_data_path") PROC_S4G_data_path = line1[1];
                        if (line1[0].ToLower() == "lessag_project_name") LESSAg_project_name = line1[1];
                        if (line1[0].ToLower() == "ems_strategy") EMS_strategy = line1[1];
                        if (line1[0].ToLower() == "sim_type") sim_type = line1[1];
                        if (line1[0].ToLower() == "sim_time") sim_time = Convert.ToInt16(line1[1]);

                            if (line1[0].ToLower() == "p_cons_profile_number")
                            {
                                try { P_CONS_profile_number = int.Parse(line1[1]); } catch { }
                                if (P_CONS_profile_number > 5) P_CONS_profile_number = 5;
                                if (P_CONS_profile_number < 0) P_CONS_profile_number = 0;
                            }
                            if (line1[0].ToLower() == "p_cons_profile_file_name")
                                P_CONS_profile_file_name[P_CONS_profile_number] = line1[1];
                            if (line1[0].ToLower() == "p_cons_profile_date") P_CONS_profile_date[P_CONS_profile_number] = line1[1];
                            if (line1[0].ToLower() == "p_cons_profile_info") P_CONS_profile_info[P_CONS_profile_number] = line1[1];

                            if (line1[0].ToLower() == "p_pv_profile_meteo_file_name") P_PV_profile_meteo_file_name = line1[1];
                        if (line1[0].ToLower() == "p_pv_profile_meteo_shift") P_PV_profile_meteo_shift = Convert.ToInt16(line1[1]);

                        if (line1[0].ToLower() == "p_cons_profile_forecast_24h__file_name") P_CONS_profile_forecast_24h__file_name = line1[1];
                        if (line1[0].ToLower() == "p_cons_profile_forecast_factor") P_CONS_profile_forecast_factor = double.Parse(line1[1]); 
                        if (line1[0].ToLower() == "p_cons_profile_forecast_time_period") P_CONS_profile_forecast_time_period = Convert.ToDouble(line1[1]);

                        if (line1[0].ToLower() == "p_pv_profile_meteo_foreacst_24h_file_name") P_PV_profile_meteo_foreacst_24h_file_name = line1[1];
                        if (line1[0].ToLower() == "p_pv_profile_meteo_forecast_24h_factor") P_PV_profile_meteo_forecast_24h_factor = double.Parse(line1[1]); 
                        if (line1[0].ToLower() == "pv_forecast_time_period") PV_forecast_time_period = Convert.ToDouble(line1[1]);

                        if (line1[0].ToLower() == "battery_forecast_24h_file_name") Battery_forecast_24h_file_name = line1[1];

                        if (line1[0].ToLower() == "e_bat_nominal") E_bat_nominal = Convert.ToDouble(line1[1]);
                        if (line1[0].ToLower() == "e_bat_max") E_bat_max = Convert.ToDouble(line1[1]);
                        if (line1[0].ToLower() == "e_bat_min") E_bat_min = Convert.ToDouble(line1[1]);
                        if (line1[0].ToLower() == "soc_init") SoC_init = Convert.ToDouble(line1[1]);
                        if (line1[0].ToLower() == "t_integration") T_integration = Convert.ToInt16(line1[1]); // this is  process time period
                        if (line1[0].ToLower() == "p_bat_max_inverters") P_BAT_max_inverters = Convert.ToDouble(line1[1]);
                        if (line1[0].ToLower() == "battery_discharging_efficiency") Battery_discharging_efficiency = Convert.ToDouble(line1[1]);
                        if (line1[0].ToLower() == "battery_charging_efficiency") Battery_charging_efficiency = Convert.ToDouble(line1[1]);

                        if (line1[0].ToLower() == "bat_scaling_factor") bat_scaling_factor = Convert.ToDouble(line1[1]);
                        if (line1[0].ToLower() == "bat_time_period") bat_time_period = Convert.ToDouble(line1[1]);

                        if (line1[0].ToLower() == "cons_scaling_factor") Cons_scaling_factor = Convert.ToDouble(line1[1]);
                        if (line1[0].ToLower() == "cons_time_period") Cons_time_period = Convert.ToDouble(line1[1]);
                        if (line1[0].ToLower() == "p_cons_setpoint") P_Cons_SeTpoint = Convert.ToDouble(line1[1]);
                            

                        if (line1[0].ToLower() == "pv_scaling_factor") PV_scaling_factor = Convert.ToDouble(line1[1]);
                        if (line1[0].ToLower() == "pv_time_period") PV_time_period = Convert.ToDouble(line1[1]);

                        if (line1[0].ToLower() == "pcc_injection_allowed") PCC_injection_allowed = Convert.ToInt16(line1[1]);
                        if (line1[0].ToLower() == "p_cons_mode") P_cons_mode = Convert.ToInt16(line1[1]);
                        if (line1[0].ToLower() == "p_pv_meteo_mode") P_PV_meteo_mode = Convert.ToInt16(line1[1]);
                        if (line1[0].ToLower() == "p_pv_after_curtail_mode") P_PV_after_curtail_mode = Convert.ToInt16(line1[1]);
                        if (line1[0].ToLower() == "p_pv_curtail_mode") P_PV_curtail_mode = Convert.ToInt16(line1[1]);
                        if (line1[0].ToLower() == "p_bat_mode") P_bat_mode = Convert.ToInt16(line1[1]);
                        if (line1[0].ToLower() == "soc_mode") SoC_mode = Convert.ToInt16(line1[1]);
                        if (line1[0].ToLower() == "process_time_period") process_time_period = Convert.ToInt16(line1[1]); //!!!!!!!!!!!!!!!!!!!!!!!!! de lucrat aici....
                    }
                }
            }
            }
            catch
            {
                Write_GridMonK_log("PROC_ini_S4G=Error1");
            }
            if (sim_type.ToLower() == "realtime")
            {
                //timer3_PROC.Interval = T_integration * 1000;
                timer3_PROC.Interval = 1000; // interrupt each 1 second
            }

            int nr_lines = 0;
            P_CONS_profile_number = 0;
            if (P_cons_mode == 0)
            {
                // ****** read the entire "P_CONS_profile" file
                try {
                string filename1 = PROC_S4G_data_path + P_CONS_profile_file_name[P_CONS_profile_number];
                lines = System.IO.File.ReadAllLines(filename1);
                foreach (string line in lines)
                {
                    // Use a tab to indent each line of the file.
                    char[] delimiterChars = { '\t', '[' };
                    string[] line1 = line.Split(delimiterChars);
                    int pos = 0;
                    foreach (string s in line1)
                    {
                        //if ((pos == 2) && (nr_lines != 0)) {
                        if ((pos == 2) && (nr_lines != 0))
                        {
                                // Profiles_crt_number
                                P_CONS_profile[0, nr_lines - 1] = double.Parse(s);
                            P_CONS_profile[0, nr_lines - 1] = P_CONS_profile[0, nr_lines - 1];
                        }
                        pos++;
                    }
                    nr_lines++;
                }
                } 
                catch
                {
                    Write_GridMonK_log("PROC_ini_S4G=Error2");
                }
            }
            Profiles_crt_number = 0;
            // ****** read the entire "P_PV_profile_meteo" file
            nr_lines = 0;
            if (P_PV_meteo_mode == 0)
            {
                try { 
                lines = System.IO.File.ReadAllLines(PROC_S4G_data_path + P_PV_profile_meteo_file_name);
                foreach (string line in lines)
                {
                    // Use a tab to indent each line of the file.
                    char[] delimiterChars = { '\t', '[' };
                    string[] line1 = line.Split(delimiterChars);
                    int pos = 0;
                    foreach (string s in line1)
                    {
                        //if ((pos == 2) && (nr_lines != 0)) {
                        if ((pos == 2) && (nr_lines != 0))
                        {
                            P_PV_meteo_profile[nr_lines - 1] = double.Parse(s);
                            P_PV_meteo_profile[nr_lines - 1] = P_PV_meteo_profile[nr_lines - 1];
                        }
                        pos++;
                    }
                    nr_lines++;
                }
                //Shifting the meteo profile if the data was measured on a different time zone
                int pv_tot_logs = Convert.ToInt16(86400 / PV_time_period);
                int shift_profile = Convert.ToInt16(P_PV_profile_meteo_shift * 3600 / PV_time_period);
                for (int i1 = 0; i1 < pv_tot_logs - shift_profile; i1++)
                    P_PV_meteo_profile[pv_tot_logs - i1] = P_PV_meteo_profile[pv_tot_logs - shift_profile - i1];
                }
                catch
                {
                    Write_GridMonK_log("PROC_ini_S4G=Error3");
                }
            }
            // ****** read the entire "P_CONS_profile_forecast_24h" file
            nr_lines = 0;
            if (P_CONS_profile_forecast_24h__file_name != "") 
            {
                lines = System.IO.File.ReadAllLines(PROC_S4G_data_path + P_CONS_profile_forecast_24h__file_name);
                foreach (string line in lines)
                {
                    char[] delimiterChars = { '\t', '[' };
                    string[] line1 = line.Split(delimiterChars);
                    int pos = 0;
                    foreach (string s in line1)
                    {
                        if (pos == 2 && (nr_lines != 0))
                        {
                            P_CONS_Profile_forecast_24h[nr_lines-1] = double.Parse(s) * P_CONS_profile_forecast_factor;
                        }
                        pos++;
                    }
                    nr_lines++;
                }
            }
            // ****** read the entire "P_SaaS_profile" file
            Read_SaaS_file();
            /*
            nr_lines = 0;
                try
                {
                    lines = System.IO.File.ReadAllLines(PROC_S4G_data_path + P_SaaS_profile_file_name);
                    foreach (string line in lines)
                    {
                        // Use a tab to indent each line of the file.
                        char[] delimiterChars = { '\t', '[' };
                        string[] line1 = line.Split(delimiterChars);
                        int pos = 0;
                        foreach (string s in line1)
                        {
                            //if ((pos == 2) && (nr_lines != 0)) {
                            if ((pos == 2) && (nr_lines != 0))
                            {
                                P_SaaS_profile[nr_lines - 1] = double.Parse(s);
                                P_SaaS_profile[nr_lines - 1] = P_SaaS_profile[nr_lines - 1];
                            }
                            pos++;
                        }
                        nr_lines++;
                    }
                }
                catch
                {
                    write_GridMonK_log("PROC_ini_S4G=Error4=Missing P_SaaS_profile_file");
                }
                */
            // ****** read the entire "P_PV_profile_meteo_foreacst_24" file in a string variable
            nr_lines = 0;
            if (P_PV_profile_meteo_foreacst_24h_file_name != "")
            {
                lines = System.IO.File.ReadAllLines(PROC_S4G_data_path + P_PV_profile_meteo_foreacst_24h_file_name);
                foreach (string line in lines)
                {
                    char[] delimiterChars = { '\t', '[' };
                    string[] line1 = line.Split(delimiterChars);
                    int pos = 0;
                    foreach (string s in line1)
                    {
                        if (pos == 2 && (nr_lines != 0))
                        {
                            P_PV_profile_meteo_forecast_24h[nr_lines-1] = double.Parse(s) * P_PV_profile_meteo_forecast_24h_factor;
                        }
                        pos++;
                    }
                    nr_lines++;
                }
                //Shifting the meteo profile if the data was measured on a different time zone
                int pv_tot_logs = Convert.ToInt16(86400 / PV_time_period);
                int shift_profile = Convert.ToInt16(P_PV_profile_meteo_shift * 3600 / PV_time_period);
                for (int i1 = 0; i1 < pv_tot_logs - shift_profile; i1++)
                    P_PV_profile_meteo_forecast_24h[pv_tot_logs - i1] = P_PV_profile_meteo_forecast_24h[pv_tot_logs - shift_profile - i1];
            }
            // ****** read the entire "P_PV_profile_meteo_foreacst_24" file in a string variable
            nr_lines = 0;
            if (Battery_forecast_24h_file_name != "")
            {
                lines = System.IO.File.ReadAllLines(PROC_S4G_data_path + Battery_forecast_24h_file_name);
                foreach (string line in lines)
                {
                    char[] delimiterChars = { '\t', '[' };
                    string[] line1 = line.Split(delimiterChars);
                    int pos = 0;
                    foreach (string s in line1)
                    {
                        if (pos == 2 && (nr_lines != 0)) //pos=????????????????????????????????????
                        {
                            Battery_forecast_24h[nr_lines - 1] = double.Parse(s);
                        }
                        pos++;
                    }
                    nr_lines++;
                }
            }
           

            //Pre process schedules (as in Unircon)
            //
            if (EMS_strategy == "Battery_forecast_optimized")
            {
            }
            if (EMS_strategy == "UniRCon")
            {
            }
            if (EMS_strategy == "PV_forecast_driven")//?????????????????
            {
                int bat_tot_logs = Convert.ToInt16(86400 / bat_time_period);
                for(int i = 0; i < bat_tot_logs; i++)
                {
                    double aux = bat_time_period / PV_forecast_time_period * i;
                    Battery_forecast_24h[i] = - P_PV_profile_meteo_forecast_24h[Convert.ToInt16(Math.Truncate(aux))];
                }

            }
            if (EMS_strategy== "PV_CONS_forecast_driven")
            {
                int bat_tot_logs = Convert.ToInt16(86400 / bat_time_period);
                //bat_time_period
                for (int i = 0; i < bat_tot_logs; i++)
                {
                    double aux_pv = bat_time_period / PV_forecast_time_period * i;
                    double aux_cons= bat_time_period / P_CONS_profile_forecast_time_period * i;
                    Battery_forecast_24h[i] = -P_PV_profile_meteo_forecast_24h[Convert.ToInt16(Math.Truncate(aux_pv))] - P_CONS_Profile_forecast_24h[Convert.ToInt16(Math.Truncate(aux_cons))];
                }
            }
                
            S4G_start_time = DateTime.Now;
        }

        DateTime S4G_start_time;   
        DateTime t_S4G;
        DateTime t_S4G_1;
        int S4G_HIL_LESSAg_dt;
        int S4G_HIL_dt;
        TimeSpan S4G_HIL_timer;
        int S4G_HIL_counter = 0; // contor care se initializeaza cu zero la pornirea programului

        // contor care memoreaza intervalul curfent din zi, pentru a putea pointa pe curbe de sarcina, productie, SaaS preincarcate pentru o zi
        // numarul depinde de perioada de integrare T_integration; daca T_integration=60, S4G_interval_counter=minutul curent din zi
        int Realtime_interval_pointer = 0; // int S4G_interval_counter = 0;

        string[,] PROC_log_S4G = new string[MEM_MAX_length, 16];
        const int counter_pos = 0;
        const int datetime_pos = 1;
        const int time_elapsed_pos = 2;
        const int p_cons_pos = 3;
        const int p_pv_meteo_pos = 4;
        const int p_pv_after_curtail_pos = 5;
        const int p_bat_real_pos = 6;
        const int SoC_pos = 7;
        const int p_pcc_pos = 8;
        const int p_bat_setpoit_pos = 9;

        // ****************************************************************************************
        // ********************************** LESSAg **********************************************
        // ****************************************************************************************
        // Variables used by LESSAg algorithm
        double LSG_SoC = 0;     // Previous name was "SoC"
        double LSG_P_PV_meteo = 0;
        double LSG_P_PV_curtail = 0;
        double LSG_P_BAT_cd_setpoint;
        //double LSG_P_bat_in = 0;
        double LSG_P_BAT; // inlocuim P_BAT_real cu LSG_P_BAT, baloare ade calcul
        double LSG_P_CONS;
        double LSG_E_bat = 0;
        double LSG_P_BAT_in = 0; // the input value of the battery power P_BAT, as seem by LESSAg
        // different data
        int Profiles_crt_number = 0;
        // Memorised data from LESSAg data
        const int MEM_MAX_length = 100000; // maximum memorised records for a certain measurement
        double[] MEM_P_bat_in = new double[MEM_MAX_length];
        double[] MEM_P_bat_out = new double[MEM_MAX_length];
        double[] MEM_P_PV_curtail_in = new double[MEM_MAX_length];
        double[] MEM_P_PV_curtail_out = new double[MEM_MAX_length];
        double[] MEM_P_PV_meteo_in = new double[MEM_MAX_length];
        double[] MEM_P_PV_meteo_out = new double[MEM_MAX_length];
        double[] MEM_SoC_in = new double[MEM_MAX_length];
        double[] MEM_SoC_out = new double[MEM_MAX_length];
        double[] MEM_P_CONS_in = new double[MEM_MAX_length];
        double[] MEM_P_CONS_out = new double[MEM_MAX_length];
        // LESSAg
        private void LESSAg_algo()
        {
            //LSG_P_bat_in = MEM_P_bat_in[S4G_HIL_counter];

            LSG_E_bat = LSG_SoC * E_bat_max;

            LSG_P_CONS = MEM_P_CONS_in[S4G_HIL_counter];    // Read P_CONS load profile from memorized P_CONS using HIL counter

            LSG_P_PV_meteo = MEM_P_PV_meteo_in[S4G_HIL_counter];

            LSG_P_PV_curtail = MEM_P_PV_curtail_in[S4G_HIL_counter];


            P_PV_after_curtail = LSG_P_PV_meteo; // - LSG_P_PV_curtail;

            // P_PV in fisiere pe disc trebuei sa foe cu semn minus
            P_PCC_LSG = LSG_P_CONS + P_PV_after_curtail + LSG_P_BAT_cd_setpoint; // Se calc cu LSG_P_BAT_cd_setpoint anterior

            if (EMS_strategy == "Battery_forecast_optimized")
            {
                LSG_P_BAT_cd_setpoint = Battery_forecast_24h[Realtime_interval_pointer] * bat_scaling_factor;
                Battery_command();
            }
            if (EMS_strategy == "PV_forecast_driven")
            {
                LSG_P_BAT_cd_setpoint = Battery_forecast_24h[Realtime_interval_pointer] * bat_scaling_factor;
                Battery_command();
            }
            if (EMS_strategy == "PV_CONS_forecast_driven")
            {
                LSG_P_BAT_cd_setpoint = Battery_forecast_24h[Realtime_interval_pointer] * bat_scaling_factor;
                Battery_command();
            }
            if (EMS_strategy == "UniRCon")
            {
                //if (P_PCC_LSG < 0)
                if (LSG_P_CONS + P_PV_after_curtail < 0)
                {
                    // We are in injection (production) mode towards DSO, so we need to take AP measures
                    // we try as first measure to increase battery consumption
                    // LSG_P_BAT_cd_setpoint = LSG_P_BAT - P_PCC_LSG;
                    LSG_P_BAT_cd_setpoint = -(LSG_P_CONS + P_PV_after_curtail); // **** Am schimbat linia de mai sus ****
                }
                else
                {
                    // We are in consumption mode from DSO point of view
                        LSG_P_BAT_cd_setpoint = 0;
                }
                // give an order to the battery to take the new setpoint; however, if battery full, new setpoint might be not considered
                Battery_command();

                LSG_P_BAT_cd_setpoint += P_SaaS_profile[Realtime_interval_pointer]; // Add the SaaS

                // calculate again P_PCC_LSG with the LSG_P_BAT after battery constraints have been considerede in Battery_command
                P_PCC_LSG = LSG_P_CONS + P_PV_after_curtail + LSG_P_BAT_cd_setpoint;
                if (PCC_injection_allowed == 0)
                {
                    if (P_PCC_LSG < 0) LSG_P_PV_curtail = LSG_P_PV_curtail + P_PCC_LSG; //else LSG_P_PV_curtail = 0;
                }
                P_PV_after_curtail = LSG_P_PV_meteo - LSG_P_PV_curtail;
                // we recalculate P_PCC_LSG
                P_PCC_LSG = LSG_P_CONS + P_PV_after_curtail + LSG_P_BAT_cd_setpoint;// + P_SaaS_profile[Realtime_interval_pointer];

            }
            // ************* Integrations (meters) ************* //
            // Metering (energy index calculated by integrating powers)
            if (LSG_P_BAT_cd_setpoint >= 0) Meter_E_BAT_real_charging = Meter_E_BAT_real_charging + LSG_P_BAT_cd_setpoint * T_integration / 3600;
            else Meter_E_BAT_real_discharging = Meter_E_BAT_real_discharging - LSG_P_BAT_cd_setpoint * T_integration / 3600;

            if (P_PCC_LSG >= 0) P_PCC_Ap = P_PCC_Ap + P_PCC_LSG * T_integration / 3600;
            else P_PCC_Am = P_PCC_Am - P_PCC_LSG * T_integration / 3600;

            P_PV_meteo_Am = P_PV_meteo_Am - LSG_P_PV_meteo * T_integration / 3600;
            P_PV_after_curtail_Am = P_PV_after_curtail_Am - P_PV_after_curtail * T_integration / 3600;
            P_PV_curtail_Am = P_PV_curtail_Am - LSG_P_PV_curtail * T_integration / 3600;

            P_CONS_Ap = P_CONS_Ap + LSG_P_CONS * T_integration / 3600;

            if(P_BAT_real_MQTT>=0) P_BAT_real_MQTT_Ap = P_BAT_real_MQTT_Ap + P_BAT_real_MQTT * T_integration / 3600;
            else P_BAT_real_MQTT_Am = P_BAT_real_MQTT_Am + P_BAT_real_MQTT * T_integration / 3600;
            //P_BAT_real_MQTT
            // ************* Integrations (meters) finished ************* //

            MEM_P_bat_out[S4G_HIL_counter] = LSG_P_BAT_cd_setpoint;
            MEM_P_PV_curtail_out[S4G_HIL_counter] = LSG_P_PV_curtail;
            MEM_P_CONS_out[S4G_HIL_counter] = LSG_P_CONS;
            MEM_SoC_out[S4G_HIL_counter] = LSG_SoC;
            //P_PV_meteo_out[S4G_HIL_counter] = P_PV_meteo_in[S4G_HIL_counter];
            MEM_P_PV_meteo_out[S4G_HIL_counter] = LSG_P_PV_meteo;

            int t_S4G_s_old = t_S4G.Second;
            int t_S4G_ms_old = t_S4G.Millisecond;
            t_S4G = DateTime.Now;

            S4G_HIL_dt = t_S4G.Second * 1000 + t_S4G.Millisecond - t_S4G_s_old * 1000 - t_S4G_ms_old;
            //S4G_HIL_LESSAg_dt = t_S4G.Second * 1000 + t_S4G.Millisecond - t_S4G_1.Second * 1000 - t_S4G_1.Millisecond;
            S4G_HIL_LESSAg_dt = Convert.ToInt32((t_S4G - t_S4G_1).TotalMilliseconds); // time spent in the current LESSAg loop
            S4G_HIL_timer = t_S4G - S4G_start_time; // time passed from the starting point

            // make a line of records on disk, for 60 seconds records
            LP_recording("60");

            S4G_var[0, 0] = (-LSG_P_BAT_cd_setpoint).ToString("####0.0"); // The varaible which can induce MQTT order is set; after MQTT will be produced, the variablke is consumed (set to nul)

            LSG_P_BAT_in = LSG_P_BAT_cd_setpoint; // Put as input for next iteration

            nr_tot_logs = 86400 / T_integration;

            // ************************************************************************************************* //
            // Memorisation of important data in a "PROC_log_S4G" matrix, to be saved at the end of the process
            PROC_log_S4G[S4G_HIL_counter, counter_pos] = (S4G_HIL_counter).ToString();//????????? poate de lucrat aici
            if (sim_type.ToLower() == "fullday")
            {
                TimeSpan time = TimeSpan.FromSeconds(S4G_HIL_counter * T_integration); // time elapsed, in seconds, after S4G_counter iterations
                PROC_log_S4G[S4G_HIL_counter, datetime_pos] = time.ToString(@"hh\:mm\:ss\:fff");
            }
            else if (sim_type.ToLower() == "realtime")
            {
                DateTime time = DateTime.Now;
                PROC_log_S4G[S4G_HIL_counter, datetime_pos] = time.ToString(@"hh\:mm\:ss\:fff");
            }
            PROC_log_S4G[S4G_HIL_counter, time_elapsed_pos] = S4G_HIL_dt.ToString("###0.0");
            PROC_log_S4G[S4G_HIL_counter, p_cons_pos] = MEM_P_CONS_out[S4G_HIL_counter].ToString("####0.0");
            PROC_log_S4G[S4G_HIL_counter, p_pv_meteo_pos] = MEM_P_PV_meteo_out[S4G_HIL_counter].ToString("####0.0");
            PROC_log_S4G[S4G_HIL_counter, p_pv_after_curtail_pos] = P_PV_after_curtail.ToString("####0.0");
            PROC_log_S4G[S4G_HIL_counter, p_bat_real_pos] = MEM_P_bat_out[S4G_HIL_counter].ToString("####0.0");
            PROC_log_S4G[S4G_HIL_counter, SoC_pos] = MEM_SoC_out[S4G_HIL_counter].ToString("####0.0##");
            PROC_log_S4G[S4G_HIL_counter, p_pcc_pos] = P_PCC_LSG.ToString("####0.0");
            PROC_log_S4G[S4G_HIL_counter, p_bat_setpoit_pos] = LSG_P_BAT_cd_setpoint.ToString("####0.0");
            PROC_log_S4G[S4G_HIL_counter, 10] = S4G_HIL_LESSAg_dt.ToString("####0.0");
            // ************************************************************************************************* //

            S4G_HIL_counter = S4G_HIL_counter + 1;

        }

        bool LESSAg_loop_done = false;
        // ************************** Starts every second, makes LESSAg algorithm every 60 seconds ***********************
        private void Timer3_PROC_S4G(object sender, EventArgs e)
        {
            // This is the main process of LESSAg
            t_S4G_1 = DateTime.Now;
            int hour = DateTime.Now.Hour;
            if (hour >= 24) hour = hour - 24;
            Realtime_interval_pointer = (hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second) / T_integration;
             //determine the starting point of the simulation
            if (S4G_HIL_counter==0)
			 { 
				if (sim_type.ToLower() == "realtime")
				{
                    
					sim_start_time = DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second; //the current second at the start of simulation
					sim_start_time = (sim_start_time / T_integration); // process begins at the current time
					// if (sim_type.ToLower() == "realtime")
					//
					//
					//
					//timer3_PROC.Interval=  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! durata refresh este de aprox 350 ms. Trebuie tinut cont de asta
					// posibil ca timer.interval= T_integration?! (avut grija ca interval este in milisecunde si T_integ in sec )               
				}
				else if (sim_type.ToLower() == "fullday")
				{
					sim_start_time = 0; //process starts at 00:00:00
				}	
			 }


            if (DateTime.Now.Second > 30) LESSAg_loop_done = false; // 

            if ((DateTime.Now.Second < 5) && (LESSAg_loop_done == false)) // start of LESSAg loop (1 minute)
            {
                LESSAg_loop_done = true;

                if (SoC_mode == mode_LOOP) // 1
                {
                    if (S4G_HIL_counter == 0) MEM_SoC_in[S4G_HIL_counter] = SoC_init;
                    else MEM_SoC_in[S4G_HIL_counter] = MEM_SoC_out[S4G_HIL_counter - 1];
                }
                else if (SoC_mode == mode_MQTTmessage)  // 2 = prin MQTT 
                {
                    MEM_SoC_in[S4G_HIL_counter] = SoC_MQTT;
                }
                LSG_SoC = MEM_SoC_in[S4G_HIL_counter];

                if (P_cons_mode == mode_DiskFile)
                    MEM_P_CONS_in[S4G_HIL_counter] = P_CONS_profile[Profiles_crt_number, Realtime_interval_pointer] * Cons_scaling_factor;
                else if (P_cons_mode == mode_MQTTmessage) MEM_P_CONS_in[S4G_HIL_counter] = P_CONS_MQTT * Cons_scaling_factor;// In this case P_CONS comes from realtime load measurmeents

                if (P_PV_meteo_mode == mode_DiskFile)
                    MEM_P_PV_meteo_in[S4G_HIL_counter] = P_PV_meteo_profile[Realtime_interval_pointer] * PV_scaling_factor; // P_PV_metero should come with positive value ??
                else if (P_PV_meteo_mode == mode_MQTTmessage) MEM_P_PV_meteo_in[S4G_HIL_counter] = -P_PV_MQTT * PV_scaling_factor; //?????????????????????????? cum sa fac scalarea 

                if (P_PV_curtail_mode == mode_LOOP)
                {
                    if (S4G_HIL_counter == 0) MEM_P_PV_curtail_in[S4G_HIL_counter] = 0;
                    else MEM_P_PV_curtail_in[S4G_HIL_counter] = MEM_P_PV_curtail_out[S4G_HIL_counter - 1];
                }
                else if (P_PV_curtail_mode == mode_MQTTmessage) MEM_P_PV_curtail_in[S4G_HIL_counter] = P_PV_MQTT * PV_scaling_factor;//???????????????? Pt timpul real ar trebuie sa citim P_PV_after_curtail

                if (P_bat_mode == mode_LOOP)
                {
                    //if (S4G_HIL_counter == 0) MEM_P_bat_in[S4G_HIL_counter] = 0;
                    //else MEM_P_bat_in[S4G_HIL_counter] = MEM_P_bat_out[S4G_HIL_counter - 1];
                    if (S4G_HIL_counter == 0) LSG_P_BAT_in = 0;
                    else LSG_P_BAT_in = LSG_P_BAT_cd_setpoint; // valoarea de comanda din loop-ul anterior
                }
                // else if (P_bat_mode == mode_MQTTmessage) P_bat_in[S4G_HIL_counter] = -P_BAT_real_MQTT * bat_scaling_factor;
                //else if (P_bat_mode == mode_MQTTmessage)
                //    MEM_P_bat_in[S4G_HIL_counter] = (-P_BAT_real_MQTT * bat_scaling_factor + P_SaaS_profile[Realtime_interval_pointer]);

                //LSG_P_BAT = MEM_P_bat_in[S4G_HIL_counter]; // ************** ?????????????????????????????? ******************
                LSG_P_BAT_cd_setpoint = LSG_P_BAT_in; // ************** ?????????????????????????????? ******************

                // ******************************* Invoke LESSAg (Local Energy Storage System Agent)

                LESSAg_algo(); // request for running the storage algorithm

                // We process ER_mode, PBat, PPv, SoC, Pcons

                // We write in the PROC log file all important data, on a line: Date-Time, ER_mode, PBat, PPv, SoC

                // *************************** LESSAg algorithm finished **************************
            } // end of 1 minute loop

            // Publish new commands in MQTT/JSON format
            if (S4G_var[S4G_var_PROP_P_bat_setp, 0] != "")
            {
                topic1 = "HIL/LESSAg/SMX/PBat";
                double Pbat_CD = double.Parse(S4G_var[S4G_var_PROP_P_bat_setp, 0]) / bat_scaling_factor;    // battery is reduced by this factor
                payload1 = "[{\"v\":" + Pbat_CD.ToString("###0.0") + ",\"u\":\"\", \"t\":0, \"n\":\"\"}]";
                S4G_var[S4G_var_PROP_P_bat_setp, 1] = S4G_var[S4G_var_PROP_P_bat_setp, 0];
                S4G_var[S4G_var_PROP_P_bat_setp, 0] = "";
                MQTT_Published_Data_S4G(); // **************** LESSAg order *********************
            }
            // Publish new commands in MQTT/JSON format
            if (S4G_var[S4G_var_PROP_Q_grid_setp, 0] != "")
            {
                topic1 = "HIL/LESSAg/SMX/QGrid";
                payload1 = "[{\"v\":" + S4G_var[S4G_var_PROP_Q_grid_setp, 0] + ",\"u\":\"\", \"t\":0, \"n\":\"\"}]";
                S4G_var[S4G_var_PROP_Q_grid_setp, 1] = S4G_var[S4G_var_PROP_Q_grid_setp, 0];
                S4G_var[S4G_var_PROP_Q_grid_setp, 0] = "";
                MQTT_Published_Data_S4G(); // **************** LESSAg order *********************
            }
            // Publish new commands in MQTT/JSON format
            if (S4G_var[S4G_var_PROP_ER_mode_setp, 0] != "")
            {
                topic1 = "HIL/LESSAg/SMX/ER_Mode";
                payload1 = "[{\"v\":" + S4G_var[S4G_var_PROP_ER_mode_setp, 0] + ",\"u\":\"\", \"t\":0, \"n\":\"\"}]";
                S4G_var[S4G_var_PROP_ER_mode_setp, 1] = S4G_var[S4G_var_PROP_ER_mode_setp, 0];
                S4G_var[S4G_var_PROP_ER_mode_setp, 0] = "";
                MQTT_Published_Data_S4G(); // **************** LESSAg order *********************
            }

            // Change variables of S4G based on commands comming from the GUI
            // const int S4F_var_PROP_Cons_scaling_factor = 4;
            if (S4G_var[S4F_var_PROP_Cons_scaling_factor, 0] != "")
            {
                try
                {
                    Cons_scaling_factor = double.Parse(S4G_var[S4F_var_PROP_Cons_scaling_factor, 0]);
                    Write_GridMonK_log("Cons_scaling_factor=Changed="+ Cons_scaling_factor.ToString("0000.000"));
                }
                catch { }
                S4G_var[S4F_var_PROP_Cons_scaling_factor, 1] = S4G_var[S4F_var_PROP_Cons_scaling_factor, 0];
                S4G_var[S4F_var_PROP_Cons_scaling_factor, 0] = "";
            }
            // const int S4F_var_PROP_PV_scaling_factor = 5;
            if (S4G_var[S4F_var_PROP_PV_scaling_factor, 0] != "")
            {
                try
                {
                    PV_scaling_factor = double.Parse(S4G_var[S4F_var_PROP_PV_scaling_factor, 0]);
                    Write_GridMonK_log("PV_scaling_factor=Changed=" + PV_scaling_factor.ToString("0000.000"));
                }
                catch { }
                S4G_var[S4F_var_PROP_PV_scaling_factor, 1] = S4G_var[S4F_var_PROP_PV_scaling_factor, 0];
                S4G_var[S4F_var_PROP_PV_scaling_factor, 0] = "";
            }
            // const int S4F_var_PROP_Storage_scaling_factor = 6;
            if (S4G_var[S4F_var_PROP_Storage_scaling_factor, 0] != "")
            {
                try { 
                    bat_scaling_factor = double.Parse(S4G_var[S4F_var_PROP_Storage_scaling_factor, 0]);
                } catch { }
                S4G_var[S4F_var_PROP_Storage_scaling_factor, 1] = S4G_var[S4F_var_PROP_Storage_scaling_factor, 0];
                S4G_var[S4F_var_PROP_Storage_scaling_factor, 0] = "";
            }
            // const int S4F_var_PROP_P_Cons_SeTpoint = 7;
            if (S4G_var[S4F_var_PROP_P_Cons_SeTpoint, 0] != "")
            {
                try
                {
                    P_Cons_SeTpoint = double.Parse(S4G_var[S4F_var_PROP_P_Cons_SeTpoint, 0]);
                }
                catch { }
                S4G_var[S4F_var_PROP_P_Cons_SeTpoint, 1] = S4G_var[S4F_var_PROP_P_Cons_SeTpoint, 0];
                S4G_var[S4F_var_PROP_P_Cons_SeTpoint, 0] = "";
            }
            // const int S4F_var_PROP_P_factor = 8;
            if (S4G_var[S4F_var_PROP_P_factor, 0] != "")
            {
                try
                {
                    prosumers[0, prosumers_PROP_P_scal_factor] = S4G_var[S4F_var_PROP_P_factor, 0];
                }
                catch { }
                S4G_var[S4F_var_PROP_P_factor, 1] = S4G_var[S4F_var_PROP_P_factor, 0];
                S4G_var[S4F_var_PROP_P_factor, 0] = "";
            }
            // const int S4F_var_PROP_P_source = 9;
            if (S4G_var[S4F_var_PROP_P_source, 0] != "")
            {
                try
                {
                    if((S4G_var[S4F_var_PROP_P_source, 0]=="PCC") || (S4G_var[S4F_var_PROP_P_source, 0] == "PV") || (S4G_var[S4F_var_PROP_P_source, 0] == "CONS")) { 
                        prosumers[0, prosumers_PROP_P_source2grid] = S4G_var[S4F_var_PROP_P_source, 0];
                    }
                    else prosumers[0, prosumers_PROP_P_source2grid] = "PCC"; // this is the default value if wrong text is introduced
                }
                catch { }
                S4G_var[S4F_var_PROP_P_source, 1] = S4G_var[S4F_var_PROP_P_source, 0];
                S4G_var[S4F_var_PROP_P_source, 0] = "";
            }


            //poate fi imbunatatita conditia!!!!!!!!!!!!!!!!
            if ((sim_time == simulation_until_end_of_day) && (S4G_HIL_counter == (nr_tot_logs - sim_start_time)))
            { 
                // timer3_PROC.Stop();
                richTextBox_console1.Text += "LESSAg Simulation completed.";
                Generate_report_files();
                //System.Environment.Exit(1);
            }
            else if (S4G_HIL_counter == sim_time/T_integration)
            {
                // timer3_PROC.Stop();
                richTextBox_console1.Text += "LESSAg Simulation completed.";
                Generate_report_files();
                //System.Environment.Exit(1);
            }
            // *************************** LESSAg algorithm stop **************************

            // Make load-flow with new dat

            //loads[5, loads_PROP_Pn] = (P_PCC_LSG/10).ToString(); //RP incepe da aiurea de la ~110 kW pt nodul 5 
            //Grid_compute();
            //De facut continutul lui button_compute_click o functie, pt a putea fi rulata din exteriorul butonului

            if (P_PV_meteo_mode == mode_MQTTmessage) P_PCC_real_values = -(P_PV_MQTT * PV_scaling_factor);
            else if (P_PV_meteo_mode == mode_DiskFile) P_PCC_real_values = (P_PV_meteo_profile[Realtime_interval_pointer] * PV_scaling_factor);

            if (P_bat_mode == mode_MQTTmessage) P_PCC_real_values += -(P_BAT_real_MQTT * bat_scaling_factor);
            else if (P_bat_mode == mode_LOOP) P_PCC_real_values += (LSG_P_BAT_in);


            P_PCC_real_values += LSG_P_CONS * Cons_scaling_factor; // + P_SaaS_profile[Realtime_interval_pointer];

            if (activate_S4G_HIL == "yes") S4G_EXT_Function_GPH();
            LP_recording("01");
        }

        long LP_no_rec = 0;
        string LP_postfix = "";
        string current_DateTime = "";
        void LP_recording(string time1)
        {
            if (P_cons_mode == mode_DiskFile) LP_postfix = ".Cd";
                else if (P_cons_mode == mode_MQTTmessage) LP_postfix = ".Cr";
                    else LP_postfix = ".Cx";
            if (P_PV_meteo_mode == mode_DiskFile) LP_postfix += "Pd";
                else if (P_PV_meteo_mode == mode_MQTTmessage) LP_postfix += "Pr";
                    else LP_postfix += "Px";
            LP_postfix += "AABB";

            DateTime t1 = DateTime.Now;
            string st = t1.Year.ToString() + "." + t1.Month.ToString("00") + "." + t1.Day.ToString("00");
            current_DateTime = st + "-" + t1.Hour.ToString("00") + ":" + t1.Minute.ToString("00") + ":" + t1.Second.ToString("00") + "." + t1.Millisecond.ToString("000") + "\t";
            string file_LP_recording = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + st + ".LP"+time1+ LP_postfix + ".txt";
            string s1 = "";
            if (((LP_no_rec == 0) && (time1=="01")) || ((S4G_HIL_counter==0) && (time1=="60")))
            {
                s1 += "Date-Time\t" +
                    "PBat_cd\tPBat_cd_val\tQgrd_cd\tER_md_cd\tER_MODE\t" +
                    "PBat_setp\tPBat_real\t" +
                    "U_BATreal\tI_BATreal\tSoC\t" +
                    "P_PV\tU_PV\t" + "ER_Pgrid\tER_Qgrid\t" +
                    "P2_PCCsign\tQ2_PCC\tK2_PCC\t" +
                    "U2_PCC\tAp_PCC\tAm_PCC\tRp_PCC\tRm_PCC\t" +
                    "f_PCC\tDate1\tDate2\t" +

                    "LSG_P_CONS\tP_BAT_cd_LSG\tP_BAT_in_LSG\t" +
                    "Bat_K\tPV_K\tCons_K\t" +
                    "LSG_P_PV_meteo\tP_PV_curtail_in\tSoC_LSG\tP_PCC_LSG\tP_Cons_SeTpoint\t" +

                    "P_PCC_real\tP_SaaS\t" +

                    "RCV1\tRCV2\tTemp_UC\t" +
                    "RP1_PCC\tP3_PCC\tPtot_PCC\t" +
                    "U1_PCC_MQTT\tU3_PCC_MQTT\t" +
                    "K1_PCC_MQTT\tK3_PCC_MQTT\t" +

                    "U_DC220_MQTT\t" +
                    "P_BAT_real_MQTT_Ap\tP_BAT_real_MQTT_Am\tP_PV_meteo_Am\tP_CONS_Ap\t" +
                    
                    "\n";
                LP_no_rec++;
            }
            s1 += st + " " + t1.Hour.ToString() + ":" + t1.Minute.ToString() + ":" + t1.Second.ToString()
                 + "." + t1.Millisecond.ToString("000") + "\t";
            s1 += LESSAg_ER_PBat_cd + "\t";                 // 1
            s1 += S4G_var[0, 1].ToString() + "\t";          // 2
            s1 += LESSAg_ER_QGrid_cd + "\t";                 // 1
            s1 += LESSAg_ER_Mode_cd + "\t";                 // 1
            s1 += er_mode + "\t";                 // 1
            s1 += P_BAT_setpoint_MQTT.ToString() + "\t";    // 3
            s1 += P_BAT_real_MQTT.ToString() + "\t";        // 4
            s1 += U_BAT_real_MQTT.ToString() + "\t";        // 4
            s1 += I_BAT_MQTT.ToString() + "\t";        // 4
            s1 += SoC_MQTT.ToString() + "\t";        // 4

            s1 += P_PV_MQTT.ToString() + "\t";              // 5
            s1 += U_PV_MQTT.ToString() + "\t";              // 6
            s1 += ER_PGrid_MQTT.ToString() + "\t";          // 7
            s1 += ER_QGrid_MQTT.ToString() + "\t";          // 8

            double P2_real = P2_PCC_MQTT;                   // 9
            if (K2_PCC_MQTT < 0) P2_real = -P2_real;
            s1 += P2_real.ToString() + "\t";

            s1 += Q2_PCC_MQTT.ToString() + "\t";            // 10
            s1 += K2_PCC_MQTT.ToString() + "\t";            // 11
            s1 += U2_PCC_MQTT.ToString() + "\t";            // 11
            s1 += Ap_PCC_MQTT.ToString() + "\t";            // 11
            s1 += Am_PCC_MQTT.ToString() + "\t";            // 11
            s1 += Rp_PCC_MQTT.ToString() + "\t";            // 11
            s1 += Rm_PCC_MQTT.ToString() + "\t";            // 11
            s1 += f_PCC_MQTT.ToString() + "\t";            // 11
            s1 += SMXDateTime_PCC_MQTT + "\t";            // 11
            s1 += SMM_Time_PCC_MQTT + "\t";            // 11

            // LESSAg data
            //"Pcons_LSG\tP_BAT_cd_LSG\tP_BAT_in_LSG\t" +
            s1 += LSG_P_CONS + "\t";            // 11
            s1 += LSG_P_BAT_cd_setpoint + "\t";            // 11
            if (S4G_HIL_counter > 0) s1 += MEM_P_bat_in[S4G_HIL_counter - 1] + "\t"; else s1 += "0\t";
            //"Bat_K\tPV_K\tCons_K\t" +
            s1 += bat_scaling_factor + "\t";            // 11
            s1 += PV_scaling_factor + "\t";            // 11
            s1 += Cons_scaling_factor + "\t";            // 11
            //"LSG_P_PV_meteo\tP_PV_curtail_in\tSoC_LSG\tP_PCC_LSG\t" +
            s1 += LSG_P_PV_meteo.ToString("#####0.000") + "\t";            // 11
            s1 += MEM_P_PV_curtail_in[S4G_HIL_counter].ToString("#####0.000") + "\t";            // 11
            s1 += LSG_SoC.ToString("#####0.000") + "\t";            // 11
            s1 += P_PCC_LSG.ToString("#####0.000") + "\t";            // 11
            s1 += P_Cons_SeTpoint.ToString("#####0.000") + "\t";            // 11

            s1 += P_PCC_real_values.ToString("#####0.000") + "\t";            // 11
            if (S4G_HIL_counter > 0) s1 += P_SaaS_profile[Realtime_interval_pointer] + "\t"; else s1 += "0\t";

            s1 += MQTT_Received_messages.ToString() + "\t";            // 11
            s1 += MQTT_Received_and_filtered_messages.ToString() + "\t";            // 11
            s1 += SMXCpuTemp_PCC_MQTT + "\t";            // 11

            double P1_real = P1_PCC_MQTT;                   // 9
            if (K1_PCC_MQTT < 0) P1_real = -P1_real;
            s1 += P1_real.ToString() + "\t";
            double P3_real = P3_PCC_MQTT;                   // 9
            if (K3_PCC_MQTT < 0) P3_real = -P3_real;
            s1 += P3_real.ToString() + "\t";
            s1 += P_PCC_MQTT.ToString() + "\t";
            s1 += U1_PCC_MQTT.ToString() + "\t";
            s1 += U3_PCC_MQTT.ToString() + "\t";
            s1 += K1_PCC_MQTT.ToString() + "\t";
            s1 += K3_PCC_MQTT.ToString() + "\t";

            s1 += U_DC220_MQTT.ToString() + "\t";
            s1 += P_BAT_real_MQTT_Ap.ToString("####0.000") + "\t";
            s1 += P_BAT_real_MQTT_Am.ToString("####0.000") + "\t";
            s1 += P_PV_meteo_Am.ToString("####0.000") + "\t";
            s1 += P_CONS_Ap.ToString("####0.000") + "\t";
            

            s1 += "\n";

            try { 
                File.AppendAllText(file_LP_recording, s1);
            } 
            catch { }

        }

        public void Generate_report_files()
        {
            string s1 = "";
            DateTime t1 = DateTime.Now;
            //richTextBox_Console.Text += "Time=" + t1.ToLongDateString() + " " + t1.ToLongTimeString() + "\n";

            s1 += "Time=" + t1.ToLongDateString() + " " + t1.ToLongTimeString() + "\n";

            s1 += "*****Configuration****=Conf\n";
            s1 += "LESSAg_project_name=" + LESSAg_project_name + "\n";
            s1 += "PROC_S4G_data_path=" + PROC_S4G_data_path + "\n";
            s1 += "P_CONS_profile_file_name=" + P_CONS_profile_file_name + "\n";
            s1 += "P_CONS_profile_factor=" + Cons_scaling_factor.ToString() + "\n";
            //s1 += "P_CONS_profile_forecast_24h__file_name=" + P_CONS_profile_forecast_24h__file_name + "\n";
            //s1 += "P_CONS_profile_forecast_factor=" + P_CONS_profile_forecast_factor.ToString() + "\n";
            s1 += "P_PV_profile_meteo_file_name=" + P_PV_profile_meteo_file_name + "\n";
            s1 += "P_PV_profile_meteo_factor=" + PV_scaling_factor.ToString() + "\n";
            s1 += "P_PV_profile_meteo_shift=" + P_PV_profile_meteo_shift.ToString() +" h" +"\n";
            //s1 += "P_PV_profile_meteo_foreacst_24h_file_name=" + P_PV_profile_meteo_foreacst_24h_file_name + "\n";
            //s1 += "P_PV_profile_meteo_forecast_24h_factor=" + P_PV_profile_meteo_forecast_24h_factor.ToString() + "\n";

            s1 += "*****Parameters******=param\n";
            s1 += "E_bat_nominal=" + E_bat_nominal.ToString("#0.000") + "\n";
            s1 += "E_bat_min=" + E_bat_min.ToString("#0.000") + "\n";
            s1 += "E_bat_max=" + E_bat_max.ToString("#0.000") + "\n";
            s1 += "P_BAT_max_inverters=" + P_BAT_max_inverters.ToString("#0.000") + "\n";
            s1 += "Battery_charging_efficiency=" + Battery_charging_efficiency.ToString("#0.000") + "\n";
            s1 += "Battery_discharging_efficiency=" + Battery_discharging_efficiency.ToString("#0.000") + "\n";

            s1 += "*****EMS parameters****=EMS\n";
            s1 += "EMS_strategy=" + EMS_strategy + "\n";
            s1 += "P_Cons_SeTpoint=" + P_Cons_SeTpoint.ToString() + "\n";
            //s1 += "\n";

            s1 += "*****Initialisations******=Ini\n";
            s1 += "E_bat_ini=" + (SoC_init*E_bat_nominal).ToString("#0.000") + "\n";
            s1 += "SoC_init=" + SoC_init.ToString("#0.000") + "\n";

            s1 += "*****Test conditions******\n";
            s1 += "Simulation type=" + sim_type + "\n";
            s1 += "simulation time=" + (S4G_HIL_counter * T_integration).ToString() + " s\n";
            s1 += "PCC_injection_allowed=" + PCC_injection_allowed.ToString() + "\n";
            s1 += "P_CONS_mode=" + P_cons_mode.ToString() + "\n";
            s1 += "P_PV_meteo_mode=" + P_PV_meteo_mode.ToString() + "\n";
            s1 += "P_PV_curtail_mode=" + P_PV_curtail_mode.ToString() + "\n";
            s1 += "P_bat_mode=" + P_bat_mode.ToString() + "\n";
            s1 += "SoC_mode=" + SoC_mode.ToString() + "\n";
            s1 += "T_integration=" + T_integration.ToString() + "\n";

            s1 += "*****Results******=Res\n";
            s1 += "P_PCC_Ap=" + P_PCC_Ap.ToString("#0.000") + "\n";
            s1 += "P_PCC_Am=" + P_PCC_Am.ToString("#0.000") + "\n";
            s1 += "P_PV_meteo_Am=" + P_PV_meteo_Am.ToString("#0.000") + "\n";
            s1 += "P_PV_curtail_Am=" + P_PV_curtail_Am.ToString("#0.000") + "\n";
            s1 += "P_PV_after_curtail_Am=" + P_PV_after_curtail_Am.ToString("#0.000") + "\n";
            //s1 += "PV_curt_val=" + PV_curt_val.ToString() + "\n";
            s1 += "P_CONS_Ap=" + P_CONS_Ap.ToString("#0.000") + "\n";
            s1 += "LSG_E_bat=" + LSG_E_bat.ToString("#0.000") + "\n";
            s1 += "Meter_E_BAT_real_charging=" + Meter_E_BAT_real_charging.ToString("#0.000") + "\n";
            s1 += "Meter_E_BAT_real_discharging=" + Meter_E_BAT_real_discharging.ToString("#0.000") + "\n";

            double d1 = 0;
            d1 = P_PCC_Ap / P_CONS_Ap * 100;
            s1 += "Epcc_plus_on_Econs[%]=" + d1.ToString("#0.000") + "\n";
            d1 = P_PCC_Am / P_CONS_Ap * 100;
            s1 += "Epcc_minus_on_Econs[%]=" + d1.ToString("#0.000") + "\n";
            d1 = -P_PV_meteo_Am / P_CONS_Ap * 100;
            s1 += "K_auto_cons[%]=" + d1.ToString("#0.000") + "\n";

            if (P_PV_meteo_Am != 0) d1 = (P_PV_meteo_Am - P_PV_after_curtail_Am) / P_PV_meteo_Am * 100; else d1 = 0;
            s1 += "PV_curt_val[%]=" + d1.ToString("#0.000") + "\n";

            System.IO.File.WriteAllText(PROC_S4G_data_path + LESSAg_project_name +  "-summary.txt", s1);

            string export = "Counter\tDate_Time(de modificat pentru timp real!!)\ttime_elapsed\tP_cons\tP_PV_meteo\tP_PV_after_curtail\tP_bat\tSoC\tP_PCC\tP_bat_cd_setpoint\tLESSAg_time [ms]\n";
            for (int i = 0; i < nr_tot_logs; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    export += PROC_log_S4G[i, j] + "\t";
                }
                export += "\n";
            }
            try { 
                System.IO.File.WriteAllText(PROC_S4G_data_path + LESSAg_project_name  + "-Results_Matrix.txt", export);
            }
            catch
            {
                Write_GridMonK_log("#generate_report_files=" + "Error1");
            }
        }




    }
}