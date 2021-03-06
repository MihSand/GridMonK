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

        const int trafos_MAX_MAX = 10; // 50, 10
        const int generators_MAX_MAX = 50; //200; 50
        const int monitors_MAX_MAX = 400; //1000; 100

        Pen p1Black = new Pen(Color.Black);
        Pen p1Black3 = new Pen(Color.Black, 3);
        Pen p2LightGray = new Pen(Color.LightGray);
        Pen p3DarkGray = new Pen(Color.DarkGray);
        Pen p4LightBlue2 = new Pen(Color.LightBlue, 2);
        Pen p4_Line = new Pen(Color.Blue, 4);
        Pen p4Blue3 = new Pen(Color.Blue, 3);
        Pen p5DarkBlue = new Pen(Color.DarkBlue);
        Pen p5DarkBlue2 = new Pen(Color.DarkBlue, 2);
        Pen p5DarkBlue3 = new Pen(Color.DarkBlue, 3);
        Pen p6Red2 = new Pen(Color.Red, 2);
        Pen p6Red4 = new Pen(Color.Red, 4);
        Pen p7Green = new Pen(Color.Green);
        Pen p7Green3 = new Pen(Color.Green,3);
        Pen p7LightGreen = new Pen(Color.LightGreen);
        Pen p8Magenta = new Pen(Color.Magenta);
        Pen p9Lime = new Pen(Color.Lime);
        Pen p10Maroon = new Pen(Color.Maroon, 2);
        Pen p11Orange = new Pen(Color.Orange, 2);
        Pen px = new Pen(Color.Black, 1);

        Font Font0s = new System.Drawing.Font("Arial", 6);
        Font Font0 = new System.Drawing.Font("Arial", 7);
        Font Font0bold = new System.Drawing.Font("Arial", 7, FontStyle.Bold);
        Font Font1 = new System.Drawing.Font("Arial", 8);
        Font Font1bold = new System.Drawing.Font("Arial", 8, FontStyle.Bold);
        Font Font2 = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
        Font Font3 = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
        Font Font12 = new System.Drawing.Font("Arial", 12, FontStyle.Regular);
        Font Font12b = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
        Font Font11 = new System.Drawing.Font("Arial", 11, FontStyle.Regular);
        Font Font11b = new System.Drawing.Font("Arial", 11, FontStyle.Bold);
        Font Font_crt = new System.Drawing.Font("Arial", 8);

        SolidBrush b0Black = new SolidBrush(Color.Black);
        SolidBrush b1White = new SolidBrush(Color.White);
        SolidBrush b2Blue = new SolidBrush(Color.Blue);
        SolidBrush b2AliceBlue = new SolidBrush(Color.AliceBlue);
        SolidBrush b3DarkGray = new SolidBrush(Color.DarkGray);
        SolidBrush b3LightGray = new SolidBrush(Color.LightGray);
        SolidBrush b4LightBlue = new SolidBrush(Color.LightBlue);
        SolidBrush b5DarkBlue = new SolidBrush(Color.DarkBlue);
        SolidBrush b6Red = new SolidBrush(Color.Red);
        SolidBrush b6LightPink = new SolidBrush(Color.LightPink);
        SolidBrush b7Green = new SolidBrush(Color.Green);
        SolidBrush b7LightGreen = new SolidBrush(Color.LightGreen);
        SolidBrush b7DarkGreen = new SolidBrush(Color.DarkGreen);
        SolidBrush b7LightGreen2 = new SolidBrush(Color.SpringGreen);
        SolidBrush b8Magenta = new SolidBrush(Color.Magenta);
        SolidBrush b8LightCoral = new SolidBrush(Color.LightCoral);
        SolidBrush b9Lime = new SolidBrush(Color.Lime);
        SolidBrush b10Maroon = new SolidBrush(Color.Maroon);
        SolidBrush b10LightSalmon = new SolidBrush(Color.LightSalmon);
        SolidBrush b11Orange = new SolidBrush(Color.Orange);
        SolidBrush b11LightSalmon = new SolidBrush(Color.LightSalmon);
        SolidBrush b13Yellow = new SolidBrush(Color.Yellow);
        SolidBrush b13LightYellow = new SolidBrush(Color.LightYellow);
        SolidBrush b14Aqua = new SolidBrush(Color.Aqua);
        SolidBrush b14Cyan = new SolidBrush(Color.Cyan);
        SolidBrush SolidBrush_crt = new SolidBrush(Color.Black);// b12;

        string GUI_Language = "English";

        // Scenario_series = 0 .. 3 = Scenarios series space used for "Undo" (depth = 4)
        // Scenario_series = 4 .. 7 = Scenarios series space used for "mem" (up to 4 memorised grid situations)
        // Scenario_series = 8 correspond to Work-Space zone results after invoking OpenDSS
        // Scenario_series = 0 .. 8 = first 9 scenarios series space: used for "Undo" and "Mem" function
        // Scenario_series = 9 correspond to RBM information (Regim baza mediu, Base Grid)
        // Scenario_series = 10 correspond to RBM values (Regim baza mediu, Base Grid)
        // Scenario series =  11 .. 109 = 99 regimuri de forecast, in care sa incapa cel putin 96 intervale (la 15 minute) 
        // Scenario series = 111 .. 209 = 99 regimuri "forecast corrected", in care sa incapa cel putin 96 intervale (la 15 minute)
        // Scenario series = 211 .. 219 = 9 scenarii de rezerva (TBD)
        // Scenario series = 221 .. 820 = 600 regimuri de analiza in jurul unui punct din retea, de ex. 50 valori pentru 12 unghiuri (pas de 30 grd) 
        const int LPs_scenarios_multi_LF_start = 9; // where start OpenDSS records for the multi-LF forecast mode (1+24 up to 1+96 LPs)
        const int LPs_scenarios_multi_LF_labels = 9; // where "base grid" scenario labels (U, I, P, Q etc.) are memorized;
        const int LPs_scenarios_base_grid_values = 10; // where "base grid" scenario is memorized

        // zone for the forecast scenarios based on LFs corresponding to load and generation profiles (24 up to 96)
        // Next value points where "forecast" scenarios start, 24/96 intervals of 1h / 15m are memorized, calculated by OpenDSS
        const int LPs_scenarios_LF_forecasts_start = 11;
        //const int LPs_scenarios_LF_forecasts_length_MAX = 99; // total number of possible LF scenarios of "forecast" scenarios

        // total number of possible LF scenarios of "forecast" scenarios, allowing an LP scenario for at least each minute of a day (1440 minutes)
        //const int LPs_scenarios_LF_forecasts_length_MAX = 100;
        const int LPs_scenarios_LF_forecasts_length_MAX = 3005; // more than one hour
        //const int LPs_scenarios_LF_forecasts_length_MAX = 11005; // more than one hour
        //const int LPs_scenarios_LF_forecasts_length_MAX = 5000;
        //const int LPs_scenarios_LF_forecasts_length_MAX = 9000; 

        // zone for grid power flows analysis of "forecast corrected" LFs (24 up to 96)
        // Next value points to the place where "forecast corrected" scenarios start, 24/96 intervals of 1h / 15m are memorized, calculated by OpenDSS
        const int LPs_scenarios_LF_forecasts_corrected_start = LPs_scenarios_LF_forecasts_start + LPs_scenarios_LF_forecasts_length_MAX; //111; 
        // number of scenarios of "forecast corrected", allowing at least 96 intervals (one day, intervals of 15 minutes = 96)
        const int LPs_scenarios_LF_forecasts_corrected_length_MAX = 99; 

        // zone with suplimentary scenarios, to be allocated later
        // where suplimentary scenarios start
        const int LPs_scenarios_supl_start = LPs_scenarios_LF_forecasts_corrected_start + LPs_scenarios_LF_forecasts_corrected_length_MAX; //211;
        const int LPs_scenarios_supl_length_MAX = 9; // number of suplimentary scenarios

        // zone for grid power flows analysis, based on many LFs (e.g. up to 600 different LFs)
        // where "forecast corrected" scenarios start, 24/96 intervals of 1h / 15m are memorized, calculated by OpenDSS
        const int LPs_scenarios_LF_analysis_start = LPs_scenarios_supl_start + LPs_scenarios_supl_length_MAX;// 221; 
        const int LPs_scenarios_LF_analysis_length_MAX = 600; // number of scenarios of "forecast corrected"

        // free zone start, length is decided by the value of historical_values_depth_MAX 
        // where "freezone" start, up to historical_values_depth_MAX
        const int LPs_scenarios_freezone_start = LPs_scenarios_LF_analysis_start + LPs_scenarios_LF_analysis_length_MAX; // 821; 

        // max number, which is the used number + 10
        const int historical_values_depth_MAX = LPs_scenarios_freezone_start + 10; //1000;

        int Grid_is_calculated = 0; // 0=no  1=yes  ; used to make some calculations only if the grid is calculated by invoking OpenDSS

        double double_NULL = 2.71314e-20; // a number near zero but which cannot be practically obtained from input or calculated data

        const int scenarios_prop_MAX = 20;
        string[,] scenarios = new string[scenarios_prop_MAX, historical_values_depth_MAX]; // 50 x loads (attached to nodes), 50 x properties
        const int scenarios_PROP_name = 0;
        const int scenarios_PROP_type = 1;
        const int scenarios_PROP_date = 2;
        const int scenarios_PROP_description = 3;
        const int scenarios_PROP_violations = 4;
        const int scenarios_PROP_recorded_commands = 5;

        const int loads_MAX = 250; const int loads_prop_MAX = 75;
        //const int loads_values_set_MAX = 40;
        //const int loads_values_depth_MAX = 500;
//        string[,,] loads_values_set = new string[loads_MAX, loads_values_set_MAX, loads_values_depth_MAX];
        string[,,] loads_values_set = new string[loads_MAX, loads_prop_MAX, historical_values_depth_MAX];
        const int loads_values_start = 9;
        //int loads_values_set_no = 0;
        string[,] loads = new string[loads_MAX, loads_prop_MAX]; // 50 x loads (attached to nodes), 50 x properties
        double[,,,] loads_npolys = new double[loads_MAX, 10, 2, 2]; // poli-linii [nr.load, segment, connection, x/y=0/1
        const int loads_PROP_name = 0;
        const int loads_PROP_bus = 1;
        const int loads_PROP_phases = 3;
        const int loads_PROP_voltage = 4;
        const int loads_PROP_Pn = 5;
        const int loads_PROP_Qn = 6;
        const int loads_PROP_model = 7;
        const int loads_PROP_PF = 8;
        const int loads_PROP_status = 9;
        const int loads_PROP_daily = 10;
        const int loads_PROP_conn = 11;
        const int loads_PROP_Vminpu = 12;
        const int loads_PROP_Vmaxpu = 13;
        const int loads_PROP_U1 = 14;
        const int loads_PROP_U2 = 15;
        const int loads_PROP_U3 = 16;
        const int loads_PROP_U4 = 17;
        const int loads_PROP_U1fi = 18;
        const int loads_PROP_U2fi = 19;
        const int loads_PROP_U3fi = 20;
        const int loads_PROP_U4fi = 21;
        const int loads_PROP_I1 = 22;
        const int loads_PROP_I2 = 23;
        const int loads_PROP_I3 = 24;
        const int loads_PROP_I4 = 25;
        const int loads_PROP_I1fi = 26;
        const int loads_PROP_I2fi = 27;
        const int loads_PROP_I3fi = 28;
        const int loads_PROP_I4fi = 29;
        const int loads_PROP_P1 = 30;
        const int loads_PROP_P2 = 31;
        const int loads_PROP_P3 = 32;
        const int loads_PROP_Q1 = 33;
        const int loads_PROP_Q2 = 34;
        const int loads_PROP_Q3 = 35;
        const int loads_PROP_x0 = 36;
        const int loads_PROP_y0 = 37;
        const int loads_PROP_P = 38;
        const int loads_PROP_Q = 39;
        const int loads_PROP_brk = 40;
        const int loads_PROP_sim_storage = 41; // selection of a storage device, to be modeled as load with +/-P, +/-Q
        const int loads_PROP_sim_storage_attr = 42; // E_stor#12;P_StorMax#2;SOC#0.75;EffCh#0.95;EffDisch#0.96;
        const int loads_PROP_sim_type = 43; // load simulating one of the following types: {EV, Bat, Capacitor, Inductor, STATCOM }
        const int loads_PROP_sim_type_attr = 44; // E_stor#12;P_StorMax#2;SOC#0.75;EffCh#0.95;EffDisch#0.96;Model#Tesla_M3;IMG=EV3_TM3_V01.jpg;
        const int loads_PROP_onoff = 45; // can be "enable" or anything else; if "enable", it shows breakers for on and off
        const int loads_PROP_duty = 46;
        const int loads_PROP_pin1_x = 47;
        const int loads_PROP_pin1_y = 48;
        const int loads_PROP_Font1 = 49;
        const int loads_PROP_kWmax = 50;
        const int loads_PROP_node_auto_draw = 51;
        const int loads_PROP_Font1_Mask = 52;
        // Information below: For a certain voltage level, user can choose a Microgrid name, e.g "A" or "B" or "MyMG"; 
        // The pool for balance calculations is based on voltage level + Microgrid name
        const int loads_PROP_user_name = 53;
        const int loads_PROP_MicroGrid1 = 54;
        const int loads_PROP_MicroGrid2 = 55;
        const int loads_PROP_MicroGrid3 = 56;
        //
        const int loads_PROP_Prosumer_SaaS = 57; // The Storage as a Service (SaaS) asked for the prosumer
        const int loads_PROP_Pn_buffered = 58;
        const int loads_PROP_Prosumer_SaaS_pair = 59; // The SaaS pair number of the prosumer
        //
        const int loads_PROP_bus_poz = 60; // pozition on the connection bus
        const int loads_PROP_npoly1_xy = 61; // polyline data
        const int loads_PROP_pin1_x0 = 62;
        const int loads_PROP_pin1_y0 = 63;
        //
        const int loads_PROP_gph_Draw_Highlighted = 64; // The drawing is high lighred if this attribute is "1"
        const int loads_PROP_gph_Draw_Highlighted_color = 65; // The highlighet color may be custom; if it is "", then the color is the normal color of the border
        const int loads_PROP_gph_Draw_Highlighted_blinking = 66; // The highlighet color is blinking if it is "1"
        const int loads_PROP_gph_DrawType = 67;
        const int loads_PROP_gph_selected = 68;
        const int loads_PROP_gph_direction = 69; // N,W,S,E (modul de desenare; N=cu borna sus, W=cu borna in satnga
        int loads_no = 0;

        const int loadshapes_MAX = 250; const int loadshapes_prop_MAX = 10;
        string[,] loadshapes = new string[loadshapes_MAX, loadshapes_prop_MAX]; // 50 x loads (attached to nodes), 20 x properties
        const int loadshapes_PROP_name = 0;
        const int loadshapes_PROP_npts = 1;
        const int loadshapes_PROP_interval = 2;
        const int loadshapes_PROP_csvfile = 3;
        const int loadshapes_PROP_mult = 4;
        const int loadshapes_PROP_PQCSVFile = 5;
        const int loadshapes_PROP_calculate_Hour2Min = 6;
        const int loadshapes_PROP_replace_PQCSVFile = 7;
        int loadshapes_no = 0;

        const int linecodes_MAX = 100; const int linecodes_prop_MAX = 20;
        string[,] linecodes = new string[linecodes_MAX, linecodes_prop_MAX]; // 50 x loads (attached to nodes), 20 x properties
        const int linecodes_PROP_name = 0;
        const int linecodes_PROP_nphases = 1;
        const int linecodes_PROP_R1 = 2;
        const int linecodes_PROP_X1 = 3;
        const int linecodes_PROP_C1 = 4;
        const int linecodes_PROP_units = 5;
        const int linecodes_PROP_Imax = 6;
        const int linecodes_PROP_Umax = 7;
        int linecodes_no = 0;

        const int HIL_FrontEnd_MAX = 10;  // cel mult 10 legaturi HIL u procesul
        const int HIL_FrontEnd_prop_MAX = 25; // 
        string[,] HIL_FrontEnd = new string[HIL_FrontEnd_MAX, HIL_FrontEnd_prop_MAX]; // 10 x HIL_FrontEnd, 10 x properties
        const int HIL_FrontEnd_PROP_Name = 0;
        const int HIL_FrontEnd_PROP_ComType = 1; // Serial, IP
        const int HIL_FrontEnd_PROP_ComAddr = 2; // If IP, it gives the IP address; If Serial, it gives the name of the Com port (e.g. COM6) 
        const int HIL_FrontEnd_PROP_ComPort = 3; // If IP, it gives the IP port; If RS485, it gives the address of the device
        const int HIL_FrontEnd_PROP_Baudrate = 4; // Applicable only for Serial
        const int HIL_FrontEnd_PROP_ProtocolType = 5; // Several protocols can be implemented: UI_Generators etc.
        const int HIL_FrontEnd_PROP_ProtocolVer = 6; // In case the version is important, this is a filed to descrive the version
        const int HIL_FrontEnd_PROP_X0 = 7; // Position X0 of the drawing
        const int HIL_FrontEnd_PROP_Y0 = 8; // Position Y0 of the drawing
        const int HIL_FrontEnd_PROP_Visible = 9; // If the drawing is visible
        const int HIL_FrontEnd_PROP_ObjType = 12; // The type of object in GridMonK realtime database: line, load, generator
        const int HIL_FrontEnd_PROP_ObjNo = 13;  // The object number in GridMonK realtime database
        const int HIL_FrontEnd_PROP_Terminal = 14; // If the object has more than one terminal, it can be written here (for lines, trafos, nodes etc.)
        const int HIL_FrontEnd_PROP_Phases = 15; // If it is targeted one phase or the 3-pahse measurement (1 or 3)
        const int HIL_FrontEnd_PROP_Phase_Number = 16; // the phase number. It can be 1,2,3 if HIL_FrontEnd_PROP_Phases=1, or it is neglected otherwise
        const int HIL_FrontEnd_PROP_ScaleMaxU = 17; // Scaling the U sent data, based on a maximum value of the externalk HIL generator
        const int HIL_FrontEnd_PROP_ScaleMaxI = 18; // Scaling the I sent data, based on a maximum value of the externalk HIL generator

        const int HIL_FrontEnd_PROP_gph_selected = 24; // Selectare = 1

        int HIL_FrontEnd_no = 0;
        int HIL_FrontEnd_dX = 40;
        int HIL_FrontEnd_dY = 26;

        //const int lines__scenarios_series_start = 9; // where a series of scenario start, e.g. 24 intervals fo one hour calculated by OpenDSS

        const int lines_MAX = 250;  // cel mult 200 linii in retea
        const int lines_prop_MAX = 110;
        //const int lines_values_set_MAX = 40;
        //const int lines_values_depth_MAX = 500;
        //string[,,] lines_values_set = new string[lines_MAX, lines_values_set_MAX, lines_values_depth_MAX];
        string[,,] lines_values_set = new string[lines_MAX, lines_prop_MAX, historical_values_depth_MAX];
        //const int lines_values_start = 9; // LPs_scenarios_series_start
        int lines_values_set_no = 0;


        //const int lines_scenarios_MAX = 200; // numarul de situatii diferite (scenarii) de retea
        //string[,,] lines_scenarios = new string[lines_MAX, lines_prop_MAX, lines_scenarios_MAX]; //seturi de scenarii de stari linie
        string[,] lines = new string[lines_MAX, lines_prop_MAX]; // 100 x lines (connecting two nodes), 50 x properties
        double[,] lines_double = new double[lines_MAX, lines_prop_MAX]; // 100 x lines (connecting two nodes), 50 x properties
        double[,,,] lines_npolys = new double[lines_MAX, 10, 2, 2]; // poli-linii [nr.gen, segment, connection, x/y=0/1
        const int lines_PROP_name = 0;
        const int lines_PROP_bus1 = 1;
        const int lines_PROP_bus2 = 2;
        const int lines_PROP_phases = 3;
        const int lines_PROP_length = 4;
        const int lines_PROP_units = 5;
        const int lines_PROP_linecode = 6;
        const int lines_PROP_draw_xy = 7;
        const int lines_PROP_x0 = 8;
        const int lines_PROP_y0 = 9;
        const int lines_PROP_P1 = 10;
        const int lines_PROP_P2 = 11;
        const int lines_PROP_P3 = 12;
        const int lines_PROP_P = 13;
        const int lines_PROP_Q1 = 14;
        const int lines_PROP_Q2 = 15;
        const int lines_PROP_Q3 = 16;
        const int lines_PROP_Q = 17;
        const int lines_PROP_S = 18;
        const int lines_PROP_U1 = 19;
        const int lines_PROP_U2 = 20;
        const int lines_PROP_U3 = 21;
        const int lines_PROP_U1fi = 22;
        const int lines_PROP_U2fi = 23;
        const int lines_PROP_U3fi = 24;
        const int lines_PROP_I1 = 25;
        const int lines_PROP_I2 = 26;
        const int lines_PROP_I3 = 27;
        const int lines_PROP_I1fi = 28;
        const int lines_PROP_I2fi = 29;
        const int lines_PROP_I3fi = 30;
        const int lines_PROP_plyline_name = 31;
        const int lines_PROP_plyline_xys = 32;
        const int lines_PROP_brk1 = 33;
        const int lines_PROP_brk2 = 34;
        const int lines_PROP_R1 = 35;
        const int lines_PROP_X1 = 36;
        const int lines_PROP_C1 = 37;
        const int lines_PROP_delta_Ufi1 = 47; // unghiul intre tensiunile la cele 2 cape ale liniei, faza 1
        const int lines_PROP_delta_Ufi2 = 48; // unghiul intre tensiunile la cele 2 cape ale liniei, faza 2
        const int lines_PROP_delta_Ufi3 = 49; // unghiul intre tensiunile la cele 2 cape ale liniei, faza 3
        const int lines_PROP_Imax = 50;
        const int lines_PROP_Umax = 51;
        const int lines_PROP_onoff = 52;
        const int lines_PROP_sign1 = 53;
        const int lines_PROP_sign2 = 54;
        const int lines_PROP_P1_t2 = 55; // t2 = Terminalul 2 al liniei
        const int lines_PROP_P2_t2 = 56;
        const int lines_PROP_P3_t2 = 57;
        const int lines_PROP_P_t2 = 58;
        const int lines_PROP_Q1_t2 = 59;
        const int lines_PROP_Q2_t2 = 60;
        const int lines_PROP_Q3_t2 = 61;
        const int lines_PROP_Q_t2 = 62;
        const int lines_PROP_S_t2 = 63;
        const int lines_PROP_U1_t2 = 64;
        const int lines_PROP_U2_t2 = 65;
        const int lines_PROP_U3_t2 = 66;
        const int lines_PROP_U1fi_t2 = 67;
        const int lines_PROP_U2fi_t2 = 68;
        const int lines_PROP_U3fi_t2 = 69;
        const int lines_PROP_I1_t2 = 70;
        const int lines_PROP_I2_t2 = 71;
        const int lines_PROP_I3_t2 = 72;
        const int lines_PROP_I1fi_t2 = 73;
        const int lines_PROP_I2fi_t2 = 74;
        const int lines_PROP_I3fi_t2 = 75;

        const int lines_PROP_delta_P = 76;
        const int lines_PROP_delta_Q = 77;
        const int lines_PROP_delta_U1 = 78;
        const int lines_PROP_delta_U1fi = 79;

        const int lines_PROP_pin1_x = 80;
        const int lines_PROP_pin1_y = 81;
        const int lines_PROP_pin2_x = 82;
        const int lines_PROP_pin2_y = 83;
        const int lines_PROP_Font_P = 84;
        const int lines_PROP_Font1_Mask = 85;
        const int lines_PROP_Font_Q = 86;
        const int lines_PROP_Font_U = 87;
        const int lines_PROP_HidePinsNo = 88;

        const int lines_PROP_voltage = 89;  // Nominal voltage level, useful for making balancing calculations
        const int lines_PROP_bus1poz = 90; // to which position in bus 1 is connected
        const int lines_PROP_bus2poz = 91; // to which position in bus 2 is connected
        // Information below: For a certain voltage level, user can choose a Microgrid name, e.g "A" or "B" or "MyMG"; 
        // The pool for balance calculations is based on voltage level + Microgrid name
        const int lines_PROP_MicroGrid1 = 92;
        const int lines_PROP_MicroGrid2 = 93;
        const int lines_PROP_MicroGrid3 = 94;
        const int lines_PROP_ConnectionType = 95;
        const int lines_PROP_npoly1_xy = 96; // GridMonK: line connection on connection 1 of the line
        const int lines_PROP_npoly2_xy = 97;
        const int lines_PROP_pin1_x0 = 98;
        const int lines_PROP_pin1_y0 = 99;
        const int lines_PROP_pin2_x0 = 100;
        const int lines_PROP_pin2_y0 = 101;
        const int lines_PROP_OutUI = 102;
        //
        const int lines_PROP_gph_DrawType = 107;
        const int lines_PROP_gph_selected = 108;
        const int lines_PROP_gph_direction = 109; // N,W,S,E
        int lines_no = 0;
        const int line_Terminal_1 = 0;
        const int line_Terminal_2 = 1;


        //const int trafos_values_set_MAX = 40;
        //const int trafos_values_depth_MAX = 500;
        //string[,,] trafos_values_set = new string[lines_MAX, lines_values_set_MAX, trafos_values_depth_MAX];
        string[,,] trafos_values_set = new string[lines_MAX, trafos_prop_MAX, historical_values_depth_MAX];
        const int trafos_values_start = 9;
        //int trafos_values_set_no = 0;

        const int trafos_MAX = trafos_MAX_MAX; // Large=50
        const int trafos_prop_MAX = 80;
        const int trafos__scenarios_series_start = 9; // where a series of scenario start, e.g. 24 intervals fo one hour calculated by OpenDSS
        //const int trafos_scenarios_MAX = 200; // numarul de situatii diferite (scenarii) de retea
        //string[,,] trafos_values_set = new string[trafos_MAX, trafos_prop_MAX, trafos_scenarios_MAX]; //seturi de scenarii de stari linie
        string[,] trafos = new string[trafos_MAX, trafos_prop_MAX]; // 40 x transformers (connecting two nodes), 50 x properties
        double[,] trafos_double = new double[trafos_MAX, trafos_prop_MAX]; // "doubl evariable, if can be calculated
        double[,,,] trafos_npolys = new double[trafos_MAX, 10, 2, 2]; // poli-linii [nr.trafo, segment, connection, x/y=0/1
        const int trafos_PROP_name = 0;
        const int trafos_PROP_bus1 = 1;
        const int trafos_PROP_bus2 = 2;
        const int trafos_PROP_windings = 3;
        const int trafos_PROP_kVs = 4;
        const int trafos_PROP_kVAs = 5;
        const int trafos_PROP_noloadloss = 6;
        const int trafos_PROP_loadloss = 7;
        const int trafos_PROP_imag = 8;
        const int trafos_PROP_xhl = 9;
        const int trafos_PROP_wdg = 10;
        const int trafos_PROP_tap = 11;
        const int trafos_PROP_maxtap = 12;
        const int trafos_PROP_mintap = 13;
        const int trafos_PROP_conns = 14;
        const int trafos_PROP_busses = 15; // cumulated description of bus 1 and 2
        const int trafos_PROP_x0 = 17;
        const int trafos_PROP_y0 = 18;
        const int trafos_PROP_plyline_name = 19;
        const int trafos_PROP_plyline_xys = 20;
        const int trafos_PROP_P1 = 21;
        const int trafos_PROP_P2 = 22;
        const int trafos_PROP_P3 = 23;
        const int trafos_PROP_P4 = 24;
        const int trafos_PROP_P = 25;
        const int trafos_PROP_Q1 = 26;
        const int trafos_PROP_Q2 = 27;
        const int trafos_PROP_Q3 = 28;
        const int trafos_PROP_Q4 = 29;
        const int trafos_PROP_Q = 30;
        const int trafos_PROP_S = 31;
        const int trafos_PROP_I1 = 32;
        const int trafos_PROP_I2 = 33;
        const int trafos_PROP_I3 = 34;
        const int trafos_PROP_I4 = 35;
        const int trafos_PROP_P1_t2 = 36;
        const int trafos_PROP_P2_t2 = 37;
        const int trafos_PROP_P3_t2 = 38;
        const int trafos_PROP_P4_t2 = 39;
        const int trafos_PROP_P_t2 = 40;

        const int trafos_PROP_Q1_t2 = 41;
        const int trafos_PROP_Q2_t2 = 42;
        const int trafos_PROP_Q3_t2 = 43;
        const int trafos_PROP_Q4_t2 = 44;
        const int trafos_PROP_Q_t2 = 45;

        const int trafos_PROP_S_t2 = 46;

        const int trafos_PROP_brk1 = 47;
        const int trafos_PROP_brk2 = 48;
        const int trafos_PROP_pin1_x = 49;
        const int trafos_PROP_pin1_y = 50;
        const int trafos_PROP_pin2_x = 51;
        const int trafos_PROP_pin2_y = 52;
        const int trafos_PROP_U_Prm_nom = 53;   // primary voltage, information can be extracted from "trafos_PROP_kVs"
        const int trafos_PROP_U_Sec1_nom = 54;   // secondary voltage winding 1, information can be extracted from "trafos_PROP_kVs"
        const int trafos_PROP_U_Sec2_nom = 55;   // secondary voltage winding 2 (if exist), information can be extracted from "trafos_PROP_kVs"
        const int trafos_PROP_kVAs_sec1 = 56; // kVA for winding 1 (primary), considred as sec1 
        const int trafos_PROP_kVAs_sec2 = 57; // kVA for winding 2 (secondary), consudred as sec2
        // Information below: For a certain voltage level, user can choose a Microgrid name, e.g "A" or "B" or "MyMG"; 
        // The pool for balance calculations is based on voltage level + Microgrid name
        const int trafos_PROP_MicroGridPrm = 58;  
        const int trafos_PROP_MicroGridSec1 = 59;
        const int trafos_PROP_MicroGridSec2 = 60;
        //
        const int trafos_PROP_pin1_x0 = 61;
        const int trafos_PROP_pin1_y0 = 62;
        const int trafos_PROP_pin1_x2 = 63;
        const int trafos_PROP_pin1_y2 = 64;
        const int trafos_PROP_node_auto_draw = 65;
        const int trafos_PROP_bus_poz = 66;
        const int trafos_PROP_npoly1_xy = 67;
        //
        const int trafos_PROP_gph_DrawType = 77;
        const int trafos_PROP_gph_selected = 78;
        const int trafos_PROP_gph_direction = 79; // N,W,S,E
        int trafos_no = 0;

        const int generators_MAX = generators_MAX_MAX; //200;
        const int generators_prop_MAX = 70; // total number of properties associated to each generator
        string[,] generators = new string[generators_MAX, generators_prop_MAX]; // 50 x generators (attached to nodes), 40 x properties
        double[,] generators_double = new double[generators_MAX, generators_prop_MAX]; // varianta double a valorilor, where possible
        double[,,,] generators_npolys = new double[generators_MAX, 10, 2, 2]; // poli-linii [nr.gen, segment, connection, x/y=0/1
        string[,,] generators_values_set = new string[generators_MAX, generators_prop_MAX, historical_values_depth_MAX]; //seturi de scenarii de stari linie
        // HashMap<string, string> generator[generators_MAX];
        // generator.get("name")
        // generator.set(column[0], column[1]);
        const int generators_PROP_name = 0;
        const int generators_PROP_bus = 1; // OpenDSS input for describing the topology: the name of the bus where the generator is connected
        const int generators_PROP_phases = 3;
        const int generators_PROP_voltage = 4;
        const int generators_PROP_Pn = 5; // OpenDSS input for the proposed active power P of the generator. After LF calculation, the real power is stored as "P"
        const int generators_PROP_Qn = 6; // OpenDSS input for the proposed reactive power Q of the generator. After LF calculation, the real power is stored as "Q"
        const int generators_PROP_model = 7;
        const int generators_PROP_PF = 8; // OpenDSS input for the proposed power power PF of the generator. If defined, Qn prevails over PF
        const int generators_PROP_status = 9;
        const int generators_PROP_daily = 10;
        const int generators_PROP_x0 = 11; // GridMonK: x0 position of the generator drawing in the UI
        const int generators_PROP_y0 = 12; // GridMonK: x0 position of the generator drawing in the UI
        const int generators_PROP_U1 = 13; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_U2 = 14; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_U3 = 15; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_U4 = 16; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_I1 = 17; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_I2 = 18; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_I3 = 19; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_I4 = 20; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_P1 = 21; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_P2 = 22; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_P3 = 23; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_P = 24; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_Q1 = 25; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_Q2 = 26; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_Q3 = 27; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_Q = 28; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_U1fi = 29; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_U2fi = 30; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_U3fi = 31; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_U4fi = 32; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_I1fi = 33; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_I2fi = 34; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_I3fi = 35; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_I4fi = 36; // GridMonK: result obtained by OpenDSS
        const int generators_PROP_Preal = 37; // GridMonK: result obtained by OpenDSS: the real active power P after LF calculation
        const int generators_PROP_Qreal = 38; // GridMonK: result obtained by OpenDSS: the real reactive power Q after LF calculation
        const int generators_PROP_S = 39;
        const int generators_PROP_duty = 40;
        const int generators_PROP_Vminpu = 41;
        const int generators_PROP_Vmaxpu = 42;
        const int generators_PROP_Pn_max = 43;
        const int generators_PROP_Qn_max = 44;
        const int generators_PROP_conn = 45;
        const int generators_PROP_pin1_x = 46; // GridMonK: 
        const int generators_PROP_pin1_y = 47; // GridMonK: 
        const int generators_PROP_gph_DrawType = 48; // GridMonK: 
        const int generators_PROP_brk = 49; // GridMonK: The status of a generator. It can be "on" or "off". If the styring is empty, the breaker status is not defined
        const int generators_PROP_node_auto_draw = 50; // points to which node connector to draw line
        const int generators_PROP_gen_type = 51;
        const int generators_PROP_user_name = 52;
        // Information below: For a certain voltage level, user can choose a Microgrid name, e.g "A" or "B" or "MyMG"; 
        // The pool for balance calculations is based on voltage level + Microgrid name
        const int generators_PROP_MicroGrid1 = 53;
        const int generators_PROP_MicroGrid2 = 54;
        const int generators_PROP_MicroGrid3 = 55;
        // description of connections of a generator to a substation busbar system (one or two busbars)
        const int generators_PROP_bus_poz = 56;
        const int generators_PROP_bus2_poz = 57;
        // additional descriptions for the generator
        const int generators_PROP_kVA = 60; // OpenDSS: kVA rating of electrical machine. Defaults to 1.2* kW if not specified. Applied to machine or inverter definition for Dynamics mode solutions
        const int generators_PROP_Max_kvar = 61; // OpenDSS: Maximum kvar limit for Model = 3. Defaults to twice the specified load kvar. Always reset this if you change PF or kvar properties.
        const int generators_PROP_Min_kvar = 62; // OpenDSS: Minimum kvar limit for Model = 3. Enter a negative number if generator can absorb vars. Defaults to negative of Maxkvar. Always reset this if you change PF or kvar properties.
        const int generators_PROP_Xd = 63; // OpenDSS: Per unit synchronous reactance of machine. Presently used only for Thevinen impedance for power flow calcs of user models (model=6). Typically use a value from 0.4 to 1.0. Default is 1.0

        const int generators_PROP_Pvfactor = 64; // OpenDSS: Convergence deceleration factor for P-V generator model (Model=3). Default is 0.1. If the circuit converges easily, you may want to use a higher number such as 1.0. Use a lower number if solution diverges.
        const int generators_PROP_Debugtrace = 65;
        const int generators_PROP_npoly1_xy = 66; // GridMonK: line connection on connection 1 of the line
        const int generators_PROP_npoly2_xy = 67;

        const int generators_PROP_gph_selected = 68;
        const int generators_PROP_gph_direction = 69;
        int generators_no = 0;

        const int vsources_MAX = 20; const int vsources_prop_MAX = 20;
        string[,] vsources = new string[vsources_MAX, vsources_prop_MAX]; // 50 x generators (attached to nodes), 40 x properties
        string[,,] vsources_values_set = new string[vsources_MAX, vsources_prop_MAX, historical_values_depth_MAX]; //seturi de scenarii de stari linie
        const int vsources_PROP_name = 0;
        const int vsources_PROP_pu = 1;
        const int vsources_PROP_phase = 2;
        const int vsources_PROP_angle = 3;
        const int vsources_PROP_BasekV = 4;
        const int vsources_PROP_baseMVA = 5;
        const int vsources_PROP_Mvasc3 = 6;
        const int vsources_PROP_Mvasc1 = 7;
        const int vsources_PROP_isc3 = 8;
        const int vsources_PROP_isc1 = 9;
        const int vsources_PROP_R1 = 10;
        const int vsources_PROP_X1 = 11;
        const int vsources_PROP_gph_selected = 18;
        const int vsources_PROP_gph_direction = 19;
        int vsources_no = 0;

        const int monitors_MAX = monitors_MAX_MAX; //1000;
        const int monitors_prop_MAX = 10;
        string[,] monitors = new string[monitors_MAX, monitors_prop_MAX]; // 50 x generators (attached to nodes), 20 x properties
        const int monitors_PROP_name = 0;
        const int monitors_PROP_element = 1;
        const int monitors_PROP_subelement = 2;
        const int monitors_PROP_terminal = 3;
        const int monitors_PROP_mode = 4;
        const int monitors_PROP_ppolar = 5;
        int monitors_no = 0;

        const int exports_MAX = 1000; const int exports_prop_MAX = 5;
        string[,] exports = new string[exports_MAX, exports_prop_MAX]; // 50 x generators (attached to nodes), 25 x properties
        const int exports_PROP_action = 0;
        const int exports_PROP_param = 1;
        int exports_no = 0;

        struct Draw_item_attrib
        {
            public string Attrib_text;
            public string Visible; // "0" or "1"
            public string phases; // "1x1x"  1=se afiseaza, orice alta valoare se neglijeaza
            public int x0;
            public int y0;
            public int Font;
            public string Color;
        };
        struct Draw_busbar_attrib
        {
            public string Attrib_text;
            public string Enable; // "0" or "1"
            public string Direction; // "N" or other
            public int Bays_number;
            public int[] coord_x, coord_y;
            public int Bay_size;
        };

        const int nodes_MAX = 250; const int nodes_prop_MAX = 60;
        string[,] nodes = new string[nodes_MAX, nodes_prop_MAX]; // 100 x nodes (attached to objects), 30 x properties
        Draw_item_attrib[] nodes_Draw_U_proc = new Draw_item_attrib[nodes_MAX];
        Draw_busbar_attrib[] nodes_Draw_busbar = new Draw_busbar_attrib[nodes_MAX];
        const int nodes_PROP_name = 0;
        const int nodes_PROP_bus = 1;
        const int nodes_PROP_x0 = 2;
        const int nodes_PROP_y0 = 3;
        const int nodes_PROP_number_of_connected_objects = 4;
        const int nodes_PROP_list_of_connected_objects = 5; // Format: object_type.object_name.connector_name
        const int nodes_PROP_P_balance = 6;
        const int nodes_PROP_Q_balance = 7;
        const int nodes_PROP_U1 = 8;
        const int nodes_PROP_U1fi = 9;
        const int nodes_PROP_U2 = 10;
        const int nodes_PROP_U2fi = 11;
        const int nodes_PROP_U3 = 12;
        const int nodes_PROP_U3fi = 13;
        const int nodes_PROP_U4 = 14;
        const int nodes_PROP_U4fi = 15;
        const int nodes_PROP_U_source_object = 16; // line, generator, load etc.
        const int nodes_PROP_U_source_object_number = 17; // numer of line, generator etc.
        const int nodes_PROP_U_source_object_name = 18; // name of the source
        const int nodes_PROP_U_source_object_avail_U_meas = 19; //
        const int nodes_PROP_U_source_object_terminal = 20; // some obejcts have more than one terminal, e.g. lines have 2 terminals: "1" and "2"
        const int nodes_PROP_voltage = 21;
        const int nodes_PROP_arrow = 22;
        const int nodes_PROP_bus_name = 23;
        const int nodes_PROP_draw_U1 = 24;
        const int nodes_PROP_draw_U1fi = 25;
        const int nodes_PROP_draw_type = 26;
        const int nodes_PROP_pin1_x0 = 27;
        const int nodes_PROP_pin1_y0 = 28;
        const int nodes_PROP_pin1_x1 = 29;
        const int nodes_PROP_pin1_y1 = 30;
        const int nodes_PROP_pin1_x2 = 31;
        const int nodes_PROP_pin1_y2 = 32;
        const int nodes_PROP_pin2_x = 33;
        const int nodes_PROP_pin2_y = 34;
        const int nodes_PROP_bus_name_x = 35;
        const int nodes_PROP_bus_name_y = 36;
        const int nodes_PROP_Font1 = 37;
        const int nodes_PROP_U_x = 38;
        const int nodes_PROP_U_y = 39;
        const int nodes_PROP_x1 = 40;
        const int nodes_PROP_y1 = 41;
        const int nodes_PROP_con1from = 42;
        const int nodes_PROP_x2 = 43;
        const int nodes_PROP_y2 = 44;
        const int nodes_PROP_con2from = 45;
        const int nodes_PROP_Draw_U1proc = 46;

        const int nodes_PROP_gph_selected = 58;
        const int nodes_PROP_plylines = 59;
        int nodes_no = 0;

        const int nodes_metadata_MAX = 250; const int nodes_metadata_prop_MAX = 25;
        string[,] nodes_metadata = new string[nodes_MAX, nodes_metadata_prop_MAX]; // 100 x nodes_metadata, 20 x properties
        Draw_item_attrib[] nodes_metadata_Draw_U_proc = new Draw_item_attrib[nodes_MAX];
        Draw_busbar_attrib[] nodes_metadata_Draw_busbar = new Draw_busbar_attrib[nodes_MAX];
        const int nodes_metadata_PROP_name = 0;
        const int nodes_metadata_PROP_bus = 1;
        const int nodes_metadata_PROP_x0 = 2;
        const int nodes_metadata_PROP_y0 = 3;
        const int nodes_metadata_PROP_x1 = 4;
        const int nodes_metadata_PROP_y1 = 5;
        const int nodes_metadata_PROP_x2 = 6;
        const int nodes_metadata_PROP_y2 = 7;
        const int nodes_metadata_PROP_arrow = 8;
        const int nodes_metadata_PROP_bus_name = 9;
        const int nodes_metadata_PROP_draw_U1 = 10;
        const int nodes_metadata_PROP_draw_U1fi = 11;
        const int nodes_metadata_PROP_draw_type = 12;
        const int nodes_metadata_PROP_bus_name_x = 13;
        const int nodes_metadata_PROP_bus_name_y = 14;
        const int nodes_metadata_PROP_Font1 = 15;
        const int nodes_metadata_PROP_U_x = 16;
        const int nodes_metadata_PROP_U_y = 17;
        const int nodes_metadata_PROP_con1from = 18;
        const int nodes_metadata_PROP_con2from = 19;
        const int nodes_metadata_PROP_gph_selected = 24;
        int nodes_metadata_no = 0;

        const int labels_MAX = 100; const int labels_prop_MAX = 20;
        string[,] labels = new string[labels_MAX, labels_prop_MAX]; // 100 x labels, 20 x properties
        const int labels_PROP_name = 0;
        const int labels_PROP_text = 1;
        const int labels_PROP_font = 2;
        const int labels_PROP_color = 3;
        const int labels_PROP_x0 = 4;
        const int labels_PROP_y0 = 5;
        const int labels_PROP_action = 6;
        const int labels_PROP_gph_selected = 19;
        int labels_no = 0;

        const int interracts_MAX = 100; const int interracts_prop_MAX = 25;
        string[,] interracts = new string[interracts_MAX, interracts_prop_MAX]; // 1000 x polylines (connecting two nodes), 10 x properties
        const int interracts_PROP_name = 0;
        const int interracts_PROP_type = 1;
        const int interracts_PROP_text = 2;
        const int interracts_PROP_command = 3;
        const int interracts_PROP_x0 = 4;
        const int interracts_PROP_y0 = 5;
        const int interracts_PROP_dx = 6;
        const int interracts_PROP_dy = 7;
        const int interracts_PROP_appearence = 8;
        const int interracts_PROP_gph_selected = 18;
        const int interracts_PROP_gph_direction = 19;
        int interracts_no = 0;

        const int measurements_MAX = 100; const int measurements_prop_MAX = 25;
        string[,] measurements = new string[measurements_MAX, measurements_prop_MAX]; // 1000 x polylines (connecting two nodes), 10 x properties
        const int measurements_PROP_name = 0;
        const int measurements_PROP_type = 1;
        const int measurements_PROP_text = 2;
        const int measurements_PROP_command = 3;
        const int measurements_PROP_x0 = 4;
        const int measurements_PROP_y0 = 5;
        const int measurements_PROP_dx = 6;
        const int measurements_PROP_dy = 7;
        const int measurements_PROP_magnif = 8;
        const int measurements_PROP_appearence = 9;
        const int measurements_PROP_gph_selected = 18;
        const int measurements_PROP_gph_direction = 19;
        int measurements_no = 0;

        const int graph_phasors_MAX = 25; const int graph_phasors_prop_MAX = 20;
        string[,] graph_phasors = new string[graph_phasors_MAX, graph_phasors_prop_MAX];
        const int graph_phasors_PROP_name = 0;
        const int graph_phasors_PROP_gph_text = 2;
        const int graph_phasors_PROP_command = 3;
        const int graph_phasors_PROP_x0 = 4;
        const int graph_phasors_PROP_y0 = 5;
        const int graph_phasors_PROP_magnif = 6;
        const int graph_phasors_PROP_legend_dx = 7;
        const int graph_phasors_PROP_transparency = 16;
        const int graph_phasors_PROP_enlarge = 17;
        const int graph_phasors_PROP_gph_selected = 18;
        const int graph_phasors_PROP_gph_direction = 19;
        int graph_phasors_no = 0;

        const int graphs_MAX = 10; const int graphs_prop_MAX = 30;
        string[,] graphs = new string[graphs_MAX, graphs_prop_MAX];
        const int graphs_PROP_name = 0;
        const int graphs_PROP_gph_title = 2;
        const int graphs_PROP_command = 3;
        const int graphs_PROP_x0 = 4;
        const int graphs_PROP_y0 = 5;
        const int graphs_PROP_magnif = 6;
        const int graphs_PROP_legend_dx = 7;
        const int graphs_PROP_Y_min = 8; // max value on Y axis
        const int graphs_PROP_Y_max = 9; // min value on Y axis
        const int graphs_PROP_Samples_max = 10;
        const int graphs_PROP_Samples_X_width = 11;
        const int graphs_PROP_Graph_type = 12;
        const int graphs_PROP_dX_legend = 13;
        const int graphs_PROP_graph_dY = 14;

        const int graphs_PROP_graph_shaddow = 25; // if it is "1", a shadow of the object is also displayed, usefu for dragging the object

        const int graphs_PROP_transparency = 26;
        const int graphs_PROP_enlarge = 27;
        const int graphs_PROP_gph_selected = 28;
        const int graphs_PROP_gph_direction = 29;
        int graphs_no = 0;

        const int graph_sankeys_MAX = 10; const int graph_sankeys_prop_MAX = 20;
        string[,] graph_sankeys = new string[graph_sankeys_MAX, graph_sankeys_prop_MAX];
        const int graph_sankeys_PROP_name = 0;
        const int graph_sankeys_PROP_gph_text = 2;
        const int graph_sankeys_PROP_command = 3;
        const int graph_sankeys_PROP_x0 = 4;
        const int graph_sankeys_PROP_y0 = 5;
        const int graph_sankeys_PROP_magnif = 6;
        const int graph_sankeys_PROP_legend_dx = 7;
        const int graph_sankeys_PROP_transparency = 16;
        const int graph_sankeys_PROP_enlarge = 17;
        const int graph_sankeys_PROP_gph_selected = 18;
        const int graph_sankeys_PROP_gph_direction = 19;
        int graph_sankeys_no = 0;

        const int graph_pies_MAX = 100; const int graph_pies_prop_MAX = 25;
        string[,] graph_pies = new string[graph_pies_MAX, graph_pies_prop_MAX];
        double[,] graph_pies_double = new double[graph_pies_MAX, graph_pies_prop_MAX];
        const int graph_pies_PROP_name = 0;
        const int graph_pies_PROP_command = 3;
        const int graph_pies_PROP_x0 = 4;
        const int graph_pies_PROP_y0 = 5;
        const int graph_pies_PROP_obj = 6;
        const int graph_pies_PROP_number = 7;
        const int graph_pies_PROP_max = 8; // maximum value, used for scaling
        const int graph_pies_PROP_min = 9; // minimum value, used for scaling
        const int graph_pies_PROP_max_norm = 10;
        const int graph_pies_PROP_min_norm = 11;
        const int graph_pies_PROP_meas_type = 12;
        const int graph_pies_PROP_visible = 13;
        const int graph_pies_PROP_bus = 14;
        const int graph_pies_PROP_gph_selected = 18;
        const int graph_pies_PROP_gph_direction = 19; // at the moment it is implemented only "N"
        const int graph_pies_PROP_size = 20;
        int graph_pies_no = 0;

        const int graph_smallgph_MAX = 20; const int graph_smallgph_prop_MAX = 20;
        string[,] graph_smallgph = new string[graph_smallgph_MAX, graph_smallgph_prop_MAX];
        const int graph_smallgph_PROP_name = 0;
        const int graph_smallgph_PROP_command = 3;
        const int graph_smallgph_PROP_x0 = 4;
        const int graph_smallgph_PROP_y0 = 5;
        const int graph_smallgph_PROP_obj = 6;
        const int graph_smallgph_PROP_U_fact = 7;
        const int graph_smallgph_PROP_P_fact = 8;
        const int graph_smallgph_PROP_P_type = 9;
        const int graph_smallgph_PROP_gph_direction = 19; // at the moment it is implemented only "N"
        int graph_smallgph_no = 0;

        const int prosumers_MAX = 10; const int prosumers_prop_MAX = 30;
        string[,] prosumers = new string[prosumers_MAX, prosumers_prop_MAX];
        const int prosumers_PROP_name = 0;
        const int prosumers_PROP_text = 1;
        const int prosumers_PROP_x0 = 2;
        const int prosumers_PROP_y0 = 3;
        const int prosumers_PROP_obj = 4;
        const int prosumers_PROP_load_number = 5;
        const int prosumers_PROP_phases = 6;
        const int prosumers_PROP_bus = 7;
        const int prosumers_PROP_voltage = 8;
        const int prosumers_PROP_P_PCC = 9;
        const int prosumers_PROP_Q_PCC = 10;
        const int prosumers_PROP_P_BAT = 11;
        const int prosumers_PROP_SoC = 12;
        const int prosumers_PROP_P_PV = 13;
        const int prosumers_PROP_ER_Mode = 14;
        const int prosumers_PROP_P_scal_factor = 15;
        const int prosumers_PROP_Q_scal_factor = 16;
        const int prosumers_PROP_P_source2grid = 17; // which power is mirrored into the grid: PCC, PV, CONS
        const int prosumers_PROP_Type = 18; // main, pair
        const int prosumers_PROP_load_pair_number = 19;
        int prosumers_no = 0;

        const int smart_meters_MAX = 100; const int smart_meters_prop_MAX = 80;
        string[,] smart_meters = new string[smart_meters_MAX, smart_meters_prop_MAX];
        const int smart_meters_PROP_name = 0;
        const int smart_meters_PROP_text = 1;
        const int smart_meters_PROP_x0 = 2;
        const int smart_meters_PROP_y0 = 3;
        const int smart_meters_PROP_obj = 4;
        const int smart_meters_PROP_number = 5;
        const int smart_meters_PROP_Ap = 6; // Total active energy plus (+)
        const int smart_meters_PROP_Am = 7; // Total active energy minus (-)
        const int smart_meters_PROP_Rp = 8; // Total reactive energy plus (+)
        const int smart_meters_PROP_Rm = 9; // Total reactive energy minus (-)
        const int smart_meters_PROP_U1 = 10;
        const int smart_meters_PROP_U2 = 11;
        const int smart_meters_PROP_U3 = 12;
        const int smart_meters_PROP_I1 = 13;
        const int smart_meters_PROP_I2 = 14;
        const int smart_meters_PROP_I3 = 15;
        const int smart_meters_PROP_P = 16;
        const int smart_meters_PROP_P_prm = 17;
        const int smart_meters_PROP_P1 = 18;
        const int smart_meters_PROP_P2 = 19;
        const int smart_meters_PROP_P3 = 20;
        const int smart_meters_PROP_P1_prm = 21;
        const int smart_meters_PROP_P2_prm = 22;
        const int smart_meters_PROP_P3_prm = 23;
        const int smart_meters_PROP_Q = 24;
        const int smart_meters_PROP_Q_prm = 25;
        const int smart_meters_PROP_Q1 = 26;
        const int smart_meters_PROP_Q2 = 27;
        const int smart_meters_PROP_Q3 = 28;
        const int smart_meters_PROP_Q1_prm = 29;
        const int smart_meters_PROP_Q2_prm = 30;
        const int smart_meters_PROP_Q3_prm = 31;
        const int smart_meters_PROP_K1 = 32;
        const int smart_meters_PROP_K2 = 33;
        const int smart_meters_PROP_K3 = 34;
        const int smart_meters_PROP_fi_U1_U1 = 35;
        const int smart_meters_PROP_fi_U2_U1 = 36;
        const int smart_meters_PROP_fi_U3_U1 = 37;
        const int smart_meters_PROP_fi_I1_U1 = 38;
        const int smart_meters_PROP_fi_I2_U1 = 39;
        const int smart_meters_PROP_fi_I3_U1 = 40;
        const int smart_meters_PROP_f = 41;
        const int smart_meters_PROP_SMM_time = 42;
        const int smart_meters_PROP_meas_type = 43;
        const int smart_meters_PROP_visible = 54;
        const int smart_meters_PROP_gph_selected = 78;
        const int smart_meters_PROP_gph_direction = 79;
        int smart_meters_no = 0;

        const int PMUs_MAX = 100; const int PMUs_prop_MAX = 40;
        string[,] PMUs = new string[PMUs_MAX, PMUs_prop_MAX];
        const int PMUs_PROP_name = 0;
        const int PMUs_PROP_text = 1;
        const int PMUs_PROP_x0 = 2;
        const int PMUs_PROP_y0 = 3;
        const int PMUs_PROP_obj = 4;
        const int PMUs_PROP_number = 5;
        const int PMUs_PROP_U1 = 6;
        const int PMUs_PROP_U1fi = 7;
        const int PMUs_PROP_U2 = 8;
        const int PMUs_PROP_U2fi = 9;
        const int PMUs_PROP_U3 = 10;
        const int PMUs_PROP_U3fi = 11;
        const int PMUs_PROP_U4 = 12;
        const int PMUs_PROP_U4fi = 13;
        const int PMUs_PROP_I1 = 14;
        const int PMUs_PROP_I1fi = 15;
        const int PMUs_PROP_I2 = 16;
        const int PMUs_PROP_I2fi = 17;
        const int PMUs_PROP_I3 = 18;
        const int PMUs_PROP_I3fi = 19;
        const int PMUs_PROP_I4 = 20;
        const int PMUs_PROP_I4fi = 21;
        const int PMUs_PROP_f = 22;
        const int PMUs_PROP_rocof = 23;
        const int PMUs_PROP_send_MQTT = 24;
        const int PMUs_PROP_send_MQTT_topic = 25;
        const int PMUs_PROP_send_MQTT_channel = 26;
        const int PMUs_PROP_meas_type = 36;
        const int PMUs_PROP_visible = 37;
        const int PMUs_PROP_gph_selected = 38;
        const int PMUs_PROP_gph_direction = 39;
        int PMUs_no = 0;

        const int polylines1_MAX = 1000; const int polylines1_prop_MAX = 10;
        string[,] polylines1 = new string[polylines1_MAX, polylines1_prop_MAX]; // 1000 x polylines (connecting two nodes), 10 x properties
        const int polylines1_PROP_name = 0;
        const int polylines1_PROP_bus1 = 1;
        const int polylines1_PROP_bus2 = 2;
        const int polylines1_PROP_polylines1_xys = 3;
        int polylines1_no = 0;

        const int polylines2node_MAX = 500; const int polylines2node_prop_MAX = 10;
        string[,] polylines2node = new string[polylines2node_MAX, polylines2node_prop_MAX]; // 1000 x polylines (connecting two nodes), 10 x properties
        const int polylines2node_PROP_name = 0;
        const int polylines2node_PROP_bus1 = 1;
        const int polylines2node_PROP_bus2 = 2;
        const int polylines2node_PROP_npolylines_xys = 3;
        int polylines2node_no = 0;

        const int Operations_MAX = 500; const int Operations_prop_MAX = 20;
        string[,] Operations = new string[Operations_MAX, Operations_prop_MAX]; // 
        const int Operations_PROP_object = 0;
        const int Operations_PROP_name = 1;
        const int Operations_PROP_attrib = 2;
        const int Operations_PROP_value = 3;
        const int Operations_PROP_value_old = 4; // Old value, before the operation took place
        const int Operations_PROP_date = 5;
        const int Operations_PROP_time = 6;
        const int Operations_PROP_TimePeriod = 7;
        const int Operations_PROP_TimePeriodUnit = 8;
        const int Operations_PROP_reason = 9;
        const int Operations_PROP_detail1_bus1 = 10; // specific detail: for "line" is Node1 from "Node_Connection": ["Node1", "Node2"]; for "load" and "generator" is Node1
        const int Operations_PROP_detail2_bus2 = 11; // specific detail: for "line" is Node2 from "Node_Connection": ["Node1", "Node2"]
        const int Operations_PROP_detail3_item_no = 12; // specific detail: for "line" is "Line_Number", for "load" is "Load_number" etc.
        const int Operations_PROP_detail4_type = 13; // type of object; for "load" can be storage, for "line" can be "MV line", for "generator" can be "PV"
        const int Operations_PROP_user_name = 14;
        int Operations_no = 0;

        const int GridCongestions_MAX = 500;
        const int GridCongestions_prop_MAX = 30;

        string[,,] GridCongestions = new string[2, GridCongestions_MAX, GridCongestions_prop_MAX]; // 
        const int GridCongestions_PROP_object = 0;
        const int GridCongestions_PROP_name = 1;
        const int GridCongestions_PROP_attrib = 2;
        const int GridCongestions_PROP_value = 3;
        const int GridCongestions_PROP_value_proc = 4;
        const int GridCongestions_PROP_value_lim = 5;
        const int GridCongestions_PROP_value_lim2_down = 6;
        const int GridCongestions_PROP_value_lim1_down = 7;
        const int GridCongestions_PROP_value_nominal = 8;
        const int GridCongestions_PROP_value_lim1_up = 9;
        const int GridCongestions_PROP_value_lim2_up = 10;
        const int GridCongestions_PROP_value_phase1 = 11;
        const int GridCongestions_PROP_value_phase2 = 12;
        const int GridCongestions_PROP_value_phase3 = 13;
        const int GridCongestions_PROP_value_P1 = 14;
        const int GridCongestions_PROP_value_P2 = 15;
        const int GridCongestions_PROP_value_P3 = 16;
        const int GridCongestions_PROP_value_Q1 = 17;
        const int GridCongestions_PROP_value_Q2 = 18;
        const int GridCongestions_PROP_value_Q3 = 19;
        const int GridCongestions_PROP_node1 = 20;
        const int GridCongestions_PROP_node2 = 21;
        int[] GridCongestions_no = new int[2];

        // Load profiles (LPs)
        const int LP_depth_MAX = 4000; const int LP_prop_MAX = 10;
        string[,] LPs = new string[LP_depth_MAX, LP_prop_MAX]; // 
        const int LP_PROP_object = 0;
        const int LP_PROP_name = 1;
        const int LP_PROP_attrib = 2;
        const int LP_PROP_value = 3;
        int LP_pointer = 0;


        // "global graphics information" = Global_Gph_Info
        const int Global_Gph_Info__types_MAX = 10; // 10 different global information, to be possible to switch during GridMink operation
        const int Global_Gph_Info_prop_MAX = 10; // 10 properties for each type
        string[,] Global_Gph_Info = new string[Global_Gph_Info__types_MAX, Global_Gph_Info_prop_MAX]; // 1000 x polylines (connecting two nodes), 10 x properties
        const int Global_Gph_Info_PROP_name = 0;
        const int Global_Gph_Info_PROP_background_image = 1;
        const int Global_Gph_Info_PROP_background_image_type = 2; // strech, tile, normal_center
        const int Global_Gph_Info_PROP_lines_background_color = 3;
        const int Global_Gph_Info_PROP_loads_background_color = 4;
        const int Global_Gph_Info_PROP_generators_background_color = 5;
        const int Global_Gph_Info_PROP_trafos_background_color = 6;
        int Global_Gph_Info_no = 0;

        // global variables in openDSS
        string OpenDSS_datapath = "";
        string OpenDSS_DefaultBaseFrequency = "";
        string OpenDSS_controlmode = "";
        string OpenDSS_mode = "";
        string OpenDSS_Circuit = ""; // this one is not global, but in simple circuits there is only one declaration
        string OpenDSS_load_flows_number = "";
        int OpenDSS_load_flows_number_int = 1;

        string OpenDSS_other_exports = "";
        double Global_PVs_factor = 1.0;
        double Global_loads_factor = 1.0;

        // variables related to "solve"
        string OpenDSS_solve_mode = "";
        string OpenDSS_solve_number = "";
        string OpenDSS_solve_stepsize = "";

        // Grid-Monk file to be used by openDSS when invoked by Grid-Monk
        string GridMonk2OpenDSS_grid_file = "";

        // variabile globale GridMonK
        string _GridMonK_nodes_wires_connection = ""; // if the wires are  based on manual polylines (default), automatic, or mixed
        string _GridMonK_GUI_refresh = "Refresh";

        // number of grid clusters (voltage levets + microgrids) to annalyze boundaries data
        const int _GridMonK_max_nr_grid_clusters = 9;

        // frecventa retelei
        double grid_frequency = 50.00;

        // Timeframe for snapshoots
        string Timeframe_crt_str = "Base";
        string Scenario_timeframe_length = "1h";
        int Timeframe_crt = -1;

        // MQTT connections
        string MQTT_Connect = "";
        string MQTT_broker_std1 = "127.0.0.1";
        string MQTT_broker_std1_subscribe_topic = "Basic_Topic_client1";
        string MQTT_broker_user1 = "";
        string MQTT_broker_password1 = "";

        string MQTT_broker_std2 = "127.0.0.1";
        string MQTT_broker_std2_subscribe_topic = "#";
        string MQTT_broker_user2 = "";
        string MQTT_broker_password2 = "";

        // Received MQTT data in JSON format (seen as external SCADA system data) need to be decoded 
        // and information need to be copied (mapped) into GridMonK database
        const int _GridMonK_Max_Nodes_input_mapping = 100; // defines the maximum mapping rules for reading and mapping external nodes data from MQTT messages
        const int _GridMonK_Max_Lines_input_mapping = 100; // defines the maximum mapping rules for reading and mapping external lines data from MQTT messages

        int GridMonK_Congestion_Forecast_mode = 0; // no CFM
        int GridMonK_realtime_data_mode = 0; // no realtime data
    }
}