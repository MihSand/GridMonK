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
                    elem_terminal = monitors[mon_pos, monitors_PROP_terminal];
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
                                            if (elem_terminal == "1")
                                            { // capatul terminalului 1
                                                if (pos == 2) trafos[elem_pos, trafos_PROP_P1] = s;
                                                if (pos == 3) trafos[elem_pos, trafos_PROP_Q1] = s;
                                                if (pos == 4) trafos[elem_pos, trafos_PROP_P2] = s;
                                                if (pos == 5) trafos[elem_pos, trafos_PROP_Q2] = s;
                                                if (pos == 6) trafos[elem_pos, trafos_PROP_P3] = s;
                                                if (pos == 7) trafos[elem_pos, trafos_PROP_Q3] = s;
                                                if (pos == 8) trafos[elem_pos, trafos_PROP_P4] = s;
                                                if (pos == 9) trafos[elem_pos, trafos_PROP_Q4] = s;
                                            }
                                            if (elem_terminal == "2")
                                            { // capatul terminalului 1
                                                if (pos == 2) trafos[elem_pos, trafos_PROP_P1_t2] = s;
                                                if (pos == 3) trafos[elem_pos, trafos_PROP_Q1_t2] = s;
                                                if (pos == 4) trafos[elem_pos, trafos_PROP_P2_t2] = s;
                                                if (pos == 5) trafos[elem_pos, trafos_PROP_Q2_t2] = s;
                                                if (pos == 6) trafos[elem_pos, trafos_PROP_P3_t2] = s;
                                                if (pos == 7) trafos[elem_pos, trafos_PROP_Q3_t2] = s;
                                                if (pos == 8) trafos[elem_pos, trafos_PROP_P4_t2] = s;
                                                if (pos == 9) trafos[elem_pos, trafos_PROP_Q4_t2] = s;
                                            }
                                    }
                                    if (elem_terminal == "1")
                                    {
                                        if (pos == 2) trafos_values_set[elem_pos, trafos_PROP_P1, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 3) trafos_values_set[elem_pos, trafos_PROP_Q1, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 4) trafos_values_set[elem_pos, trafos_PROP_P2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 5) trafos_values_set[elem_pos, trafos_PROP_Q2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 6) trafos_values_set[elem_pos, trafos_PROP_P3, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 7) trafos_values_set[elem_pos, trafos_PROP_Q3, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 8) trafos_values_set[elem_pos, trafos_PROP_P4, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 9) trafos_values_set[elem_pos, trafos_PROP_Q4, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    }
                                    if (elem_terminal == "2")
                                    {
                                        if (pos == 2) trafos_values_set[elem_pos, trafos_PROP_P1_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 3) trafos_values_set[elem_pos, trafos_PROP_Q1_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 4) trafos_values_set[elem_pos, trafos_PROP_P2_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 5) trafos_values_set[elem_pos, trafos_PROP_Q2_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 6) trafos_values_set[elem_pos, trafos_PROP_P3_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 7) trafos_values_set[elem_pos, trafos_PROP_Q3_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 8) trafos_values_set[elem_pos, trafos_PROP_P4_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                        if (pos == 9) trafos_values_set[elem_pos, trafos_PROP_Q4_t2, nr_line + LPs_scenarios_multi_LF_start] = s;
                                    }
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