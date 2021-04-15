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
using Newtonsoft;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
//using OpenDSSengine;

namespace GridMonC
{
    // used added "OpenDSSengine.dll"
    // in order to work, it needs:
    // Regsiter:
    // make it compatible with: >TlbImp.exe OpenDSSengine.dll /out:OpenDSSengine1.dll
    // Project -> Add Reference -> Browse
    public partial class GridMonk : Form
    {
        string OpenDSS_Circuit_name = "";
        string OpenDSS_Circuit_basekV = "";
        string OpenDSS_Circuit_bus1 = "";
        string OpenDSS_Circuit_pu = "";
        string OpenDSS_Circuit_angle = "";
        string OpenDSS_Circuit_phases = "";
        string OpenDSS_Circuit_Vminpu = "";
        string OpenDSS_Circuit_Vmaxpu = "";
        string OpenDSS_Circuit_MVASC3 = "";
        string OpenDSS_Circuit_MVASC1 = "";
        string OpenDSS_Circuit_R1 = "";
        string OpenDSS_Circuit_X1 = "";
        string Open_DSS_Edit_string = "";

        int new_Load_command = 0;
        private void Button_Load_file_Click(object sender, EventArgs e)
        {
            Write_GridMonK_log("#Command=" + "Load_file=Start");
            Write_GridMonK_log("GridMonk_Project=" + GridMonk_Project);
            Load_project();
            Read_SCADA_file();
            Operations_no = 0; // Index of operations is set to zero
            Write_GridMonK_log("#Command=" + "Load_file=Finished");
            new_Load_command = 1;
            // Initialize
        }

        private void Load_project()
        {
            richTextBox_events.Text = ""; // la o lansare noua se reseteaza acest text
            Timeframe_crt = -1; // The timeframe is set to -1, which correspond to RMB

            loads_no = 0;
            lines_no = 0;           lines_values_set_no = 0;
            generators_no = 0;
            vsources_no = 0;
            exports_no = 0;
            monitors_no = 0;
            linecodes_no = 0;
            loadshapes_no = 0;
            interracts_no = 0;
            measurements_no = 0;
            graph_phasors_no = -1; // permite mai multe randuri despre acelasi grafic
            graph_sankeys_no = -1; // permite mai multe randuri despre acelasi grafic
            graph_pies_no = 0;
            graph_smallgph_no = 0;
            graphs_no = 0;
            prosumers_no = 0;
            smart_meters_no = 0;
            PMUs_no = 0;
            nodes_no = 0;
            nodes_metadata_no = 0;
            labels_no = 0;
            trafos_no = 0;
            polylines1_no = 0;
            polylines2node_no = 0;
            HIL_FrontEnd_no = 0;
            Angle_real_time = 0;

            OpenDSS_other_exports = "";

            for (int i1 = 0; i1 < loads_MAX; i1++) for (int j1 = 0; j1 < loads_prop_MAX; j1++) loads[i1, j1] = "";
            for (int i1 = 0; i1 < loads_MAX; i1++)
                for (int sgm = 0; sgm < 10; sgm++)
                    for (int con = 0; con < 2; con++)
                    {
                        loads_npolys[i1, sgm, con, 0] = 0; // coordinate x
                        loads_npolys[i1, sgm, con, 1] = 0; // coordinate y
                    }
            for (int i1 = 0; i1 < HIL_FrontEnd_MAX; i1++) for (int j1 = 0; j1 < HIL_FrontEnd_prop_MAX; j1++)
                {
                    HIL_FrontEnd[i1, j1] = ""; // Initialize HIL_FrontEnd objects 
                }
            for (int i1 = 0; i1 < lines_MAX; i1++) for (int j1 = 0; j1 < lines_prop_MAX; j1++)
                {
                    lines[i1, j1] = "";
                    // initialize with a number near zero but which cannot be practically obtained from input or calculated data
                    lines_double[i1, j1] = double_NULL; 
                }
            for (int i1 = 0; i1 < lines_MAX; i1++)
                for (int sgm = 0; sgm < 10; sgm++)
                    for (int con = 0; con < 2; con++)
                    {
                        lines_npolys[i1, sgm, con, 0] = 0; // coordinate x
                        lines_npolys[i1, sgm, con, 1] = 0; // coordinate y
                    }
            for (int i1 = 0; i1 < trafos_MAX; i1++) for (int j1 = 0; j1 < trafos_prop_MAX; j1++)
                {
                trafos[i1, j1] = "";
                    // initialize with a number near zero but which cannot be practically obtained from input or calculated data
                trafos_double[i1, j1] = double_NULL;
            }
            for (int i1 = 0; i1 < trafos_MAX; i1++)
                for (int sgm = 0; sgm < 10; sgm++)
                    for (int con = 0; con < 2; con++)
                    {
                        trafos_npolys[i1, sgm, con, 0] = 0; // coordinate x
                        trafos_npolys[i1, sgm, con, 1] = 0; // coordinate y
                    }

            for (int i1 = 0; i1 < linecodes_MAX; i1++) for (int j1 = 0; j1 < linecodes_prop_MAX; j1++) linecodes[i1, j1] = "";
            for (int i1 = 0; i1 < loadshapes_MAX; i1++) for (int j1 = 0; j1 < loadshapes_prop_MAX; j1++) loadshapes[i1, j1] = "";

            for (int i1 = 0; i1 < generators_MAX; i1++) for (int j1 = 0; j1 < generators_prop_MAX; j1++)
                {
                    generators[i1, j1] = "";
                    // initialize with a number near zero but which cannot be practically obtained from input or calculated data
                    generators_double[i1, j1] = double_NULL;
                }
            for (int i1 = 0; i1 < generators_MAX; i1++)
                for (int sgm = 0; sgm < 10; sgm++)
                    for (int con = 0; con < 2; con++)
                    {
                        generators_npolys[i1, sgm, con, 0] = 0; // coordinate x
                        generators_npolys[i1, sgm, con, 1] = 0; // coordinate y
                    }

            for (int i1 = 0; i1 < vsources_MAX; i1++) for (int j1 = 0; j1 < vsources_prop_MAX; j1++) vsources[i1, j1] = "";
            for (int i1 = 0; i1 < monitors_MAX; i1++) for (int j1 = 0; j1 < monitors_prop_MAX; j1++) monitors[i1, j1] = "";
            for (int i1 = 0; i1 < exports_MAX; i1++) for (int j1 = 0; j1 < exports_prop_MAX; j1++) exports[i1, j1] = "";
            for (int i1 = 0; i1 < nodes_MAX; i1++) for (int j1 = 0; j1 < nodes_prop_MAX; j1++) nodes[i1, j1] = "";
            for (int i1 = 0; i1 < nodes_metadata_MAX; i1++)
            {
                for (int j1 = 0; j1 < nodes_metadata_prop_MAX; j1++) nodes_metadata[i1, j1] = "";
                // initialize the structure of "nodes_Draw_U_proc"
                nodes_Draw_U_proc[i1].Attrib_text = "{visible:0,x0:0,y0:0,font:10,color:\"black\"}";
                nodes_Draw_U_proc[i1].Visible = "0"; // initial it is invisible
                nodes_Draw_U_proc[i1].x0 = 0; nodes_Draw_U_proc[i1].y0 = 0; // initial x0 and y0 are zero
                nodes_Draw_U_proc[i1].Font = 8; // intial font is 8
                nodes_Draw_U_proc[i1].Color = "Black";
                // initialize the structure of "nodes_Draw_busbar"
                nodes_Draw_busbar[i1].Attrib_text = "{enable:0,direction:N,bays:3,bay_size:10}";
                nodes_Draw_busbar[i1].Enable = "0"; // initial it is not enabled
                nodes_Draw_busbar[i1].Direction = "N";
                nodes_Draw_busbar[i1].Bays_number = 3; // intial number is 3
                nodes_Draw_busbar[i1].Bay_size = 10; // initail step between bays is 10 points
                nodes_Draw_busbar[i1].coord_x = new int[20]; // maximum 20 bays for a busbar
                for (int x1 = 0; x1 < 20; x1++) nodes_Draw_busbar[i1].coord_x[x1] = 0;
                nodes_Draw_busbar[i1].coord_y = new int[20]; // maximum 20 bays for a busbar
                for (int y1 = 0; y1 < 20; y1++) nodes_Draw_busbar[i1].coord_y[y1] = 0;
            }
            for (int i1 = 0; i1 < labels_MAX; i1++) for (int j1 = 0; j1 < labels_prop_MAX; j1++) labels[i1, j1] = "";
            for (int i1 = 0; i1 < interracts_MAX; i1++) for (int j1 = 0; j1 < interracts_prop_MAX; j1++) interracts[i1, j1] = "";
            for (int i1 = 0; i1 < measurements_MAX; i1++) for (int j1 = 0; j1 < measurements_prop_MAX; j1++) measurements[i1, j1] = "";
            for (int i1 = 0; i1 < graph_phasors_MAX; i1++) for (int j1 = 0; j1 < graph_phasors_prop_MAX; j1++) graph_phasors[i1, j1] = "";
            for (int i1 = 0; i1 < graph_sankeys_MAX; i1++) for (int j1 = 0; j1 < graph_sankeys_prop_MAX; j1++) graph_sankeys[i1, j1] = "";
            for (int i1 = 0; i1 < graph_pies_MAX; i1++)
            {
                for (int j1 = 0; j1 < graph_pies_prop_MAX; j1++) graph_pies[i1, j1] = "";
                graph_pies[i1, graph_pies_PROP_size] = "1.0"; // deafult dimmention is 1
                graph_pies_double[i1, graph_pies_PROP_size] = 1.0; // deafult dimmention is 1
            }
            for (int i1 = 0; i1 < graph_smallgph_MAX; i1++) for (int j1 = 0; j1 < graph_smallgph_prop_MAX; j1++) graph_smallgph[i1, j1] = "";
            for (int i1 = 0; i1 < prosumers_MAX; i1++) for (int j1 = 0; j1 < prosumers_prop_MAX; j1++) prosumers[i1, j1] = "";
            for (int i1 = 0; i1 < smart_meters_MAX; i1++) for (int j1 = 0; j1 < smart_meters_prop_MAX; j1++) smart_meters[i1, j1] = "";
            for (int i1 = 0; i1 < PMUs_MAX; i1++) for (int j1 = 0; j1 < PMUs_prop_MAX; j1++) PMUs[i1, j1] = "";
            for (int i1 = 0; i1 < polylines1_MAX; i1++) for (int j1 = 0; j1 < polylines1_prop_MAX; j1++) polylines1[i1, j1] = "";
            for (int i1 = 0; i1 < polylines2node_MAX; i1++) for (int j1 = 0; j1 < polylines2node_prop_MAX; j1++) polylines2node[i1, j1] = "";
            for (int i1 = 0; i1 < graphs_MAX; i1++) for (int j1 = 0; j1 < graphs_prop_MAX; j1++) prosumers[i1, j1] = "";
            Gph_phasors_ini(); // these objects need more initialisations
            Gph_sankeys_ini(); // these objects need more initialisations
            for (int i1 = 0; i1 < Global_Gph_Info__types_MAX; i1++) for (int j1 = 0; j1 < Global_Gph_Info_prop_MAX; j1++) Global_Gph_Info[i1, j1] = "";
            // initialize all historical data sets
            for (int i1 = 0; i1 < loads_MAX; i1++) for (int j1 = 0; j1 < loads_prop_MAX; j1++)
                    for (int k1 = 0; k1 < historical_values_depth_MAX; k1++) loads_values_set[i1, j1, k1] = "";
            for (int i1 = 0; i1 < lines_MAX; i1++) for (int j1 = 0; j1 < lines_prop_MAX; j1++)
                    for (int k1 = 0; k1 < historical_values_depth_MAX; k1++) lines_values_set[i1, j1, k1] = "";
            for (int i1 = 0; i1 < lines_MAX; i1++) lines[i1, lines_PROP_Imax] = "40000"; // we put 40kA as max value for a line, which cannot be attended
            for (int i1 = 0; i1 < trafos_MAX; i1++) for (int j1 = 0; j1 < trafos_prop_MAX; j1++)
                    for (int k1 = 0; k1 < historical_values_depth_MAX; k1++) trafos_values_set[i1, j1, k1] = "";
            for (int i1 = 0; i1 < generators_MAX; i1++) for (int j1 = 0; j1 < generators_prop_MAX; j1++)
                    for (int k1 = 0; k1 < historical_values_depth_MAX; k1++) generators_values_set[i1, j1, k1] = "";
            //Initailize GridMonk global variables
            _GridMonK_nodes_wires_connection = "";

            string File_to_be_scanned = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + GridMonk_Project + ".dss";
            //string[] file_text = System.IO.File.ReadAllLines(@"E:\App\VStudio\GridMonK\PowerGrid\" + File_to_be_scanned);
            string[] file_text = System.IO.File.ReadAllLines(File_to_be_scanned);
            foreach (string line in file_text)
            {
                char[] delimiterChars1 = { '\t', ' ' };
                string[] line1 = line.Split(delimiterChars1);
                string[] column_in, column_cd, cdx, cd2;

                if ((line1[0].ToLower() == "new") && (line1[1].Contains("Circuit.")))
                {
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        if (column.Contains("Circuit.")) OpenDSS_Circuit_name = column.Remove(0, 8);
                        if (column_in[0] == "basekV") OpenDSS_Circuit_basekV = column_in[1];
                        if (column_in[0] == "bus1") OpenDSS_Circuit_bus1 = column_in[1];
                        if (column_in[0] == "pu") OpenDSS_Circuit_pu = column_in[1];
                        if (column_in[0] == "angle") OpenDSS_Circuit_angle = column_in[1];
                        if (column_in[0] == "phases") OpenDSS_Circuit_phases = column_in[1];
                        if (column_in[0].ToLower() == "vminpu") OpenDSS_Circuit_Vminpu = column_in[1];
                        if (column_in[0].ToLower() == "vmaxpu") OpenDSS_Circuit_Vmaxpu = column_in[1];
                        if (column_in[0].ToLower() == "mvasc3") OpenDSS_Circuit_MVASC3 = column_in[1];
                        if (column_in[0].ToLower() == "mvasc1") OpenDSS_Circuit_MVASC1 = column_in[1];
                        if (column_in[0].ToLower() == "r1") OpenDSS_Circuit_R1 = column_in[1];
                        if (column_in[0].ToLower() == "x1") OpenDSS_Circuit_X1 = column_in[1];

                    }
                }

                // Vsources
                if ((line1[0].ToLower() == "new") && (line1[1].Contains("Vsource.")))
                {
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        if (column.Contains("Vsource.")) vsources[vsources_no, vsources_PROP_name] = column.Remove(0, 8);
                        if (column_in[0].ToLower() == "basekv") vsources[vsources_no, vsources_PROP_BasekV] = column_in[1];
                        if (column_in[0].ToLower() == "pu") vsources[vsources_no, vsources_PROP_pu] = column_in[1];
                        if (column_in[0].ToLower() == "angle") vsources[vsources_no, vsources_PROP_angle] = column_in[1];
                        if (column_in[0].ToLower() == "phase") vsources[vsources_no, vsources_PROP_phase] = column_in[1];
                        if (column_in[0].ToLower() == "basemva") vsources[vsources_no, vsources_PROP_baseMVA] = column_in[1];
                        if (column_in[0].ToLower() == "mvasc3") vsources[vsources_no, vsources_PROP_Mvasc3] = column_in[1];
                        if (column_in[0].ToLower() == "mvasc1") vsources[vsources_no, vsources_PROP_Mvasc1] = column_in[1];
                        if (column_in[0].ToLower() == "isc3") vsources[vsources_no, vsources_PROP_isc3] = column_in[1];
                        if (column_in[0].ToLower() == "isc1") vsources[vsources_no, vsources_PROP_isc1] = column_in[1];
                        if (column_in[0].ToLower() == "r1") vsources[vsources_no, vsources_PROP_R1] = column_in[1];
                        if (column_in[0].ToLower() == "x1") vsources[vsources_no, vsources_PROP_X1] = column_in[1];

                    }
                    vsources_no++;
                }
                // lines
                if ((line1[0].ToLower() == "new") && (line1[1].Contains("load."))) // we have a new load declared
                {
                    loads[loads_no, loads_PROP_gph_direction] = "N"; // default is with connections and drawing in the north
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        if (column.Contains("load.")) loads[loads_no, loads_PROP_name] = column.Remove(0, 5);
                        if (column_in[0] == "bus1") loads[loads_no, loads_PROP_bus] = column_in[1];
                        if (column_in[0] == "phases") loads[loads_no, loads_PROP_phases] = column_in[1];
                        if (column.ToLower().Contains("kv=")) loads[loads_no, loads_PROP_voltage] = column.Remove(0, 3);
                        if (column.Contains("kW="))
                        {
                            loads[loads_no, loads_PROP_Pn] = column.Remove(0, 3);
                            loads[loads_no, loads_PROP_Pn_buffered] = loads[loads_no, loads_PROP_Pn];
                        }
                        //if (column.Contains("kvar=")) loads[loads_no, loads_PROP_Qn] = column.Remove(0, 5);
                        if (column_in[0].ToLower() == "kvar") loads[loads_no, loads_PROP_Qn] = column_in[1];
                        if (column_in[0].ToLower() == "pf") loads[loads_no, loads_PROP_PF] = column_in[1];
                        //if (column.ToLower().Contains("pf=")) loads[loads_no, loads_PROP_PF] = column.Remove(0, 3);
                        if (column.ToLower().Contains("model=")) loads[loads_no, loads_PROP_model] = column.Remove(0, 6);
                        //if (column_in[0].ToLower() == "model") loads[loads_no, loads_PROP_model] = column_in[1];
                        if (column_in[0].ToLower() == "conn") loads[loads_no, loads_PROP_conn] = column_in[1];
                        //if (column.Contains("conn=")) loads[loads_no, loads_PROP_conn] = column.Remove(0, 5);
                        //if (column.Contains("daily=")) loads[loads_no, loads_PROP_daily] = column.Remove(0, 6);
                        if (column_in[0].ToLower() == "daily") loads[loads_no, loads_PROP_daily] = column_in[1];
                        //if (column.Contains("Duty=")) loads[loads_no, loads_PROP_duty] = column.Remove(0, 6);
                        if (column_in[0] == "Duty") loads[loads_no, loads_PROP_duty] = column_in[1];
                        if (column.Contains("status=")) loads[loads_no, loads_PROP_status] = column.Remove(0, 7);
                        if (column.Contains("Vminpu=")) loads[loads_no, loads_PROP_Vminpu] = column.Remove(0, 7);
                        if (column.Contains("Vmaxpu=")) loads[loads_no, loads_PROP_Vmaxpu] = column.Remove(0, 7);
                        if (column_in[0].ToLower() == "!!npoly1_xy")
                            loads[loads_no, loads_PROP_npoly1_xy] = column_in[1];
                        if (column_in[0].ToLower() == "!!user_name")
                            loads[loads_no, loads_PROP_user_name] = column_in[1];
                        if (column_in[0] == "!!x0") loads[loads_no, loads_PROP_x0] = column_in[1];
                        if (column_in[0] == "!!y0")
                            loads[loads_no, loads_PROP_y0] = column_in[1];
                        if (column_in[0] == "!!brk") loads[loads_no, loads_PROP_brk] = column_in[1];
                        if (column_in[0] == "!!onoff")
                            loads[loads_no, loads_PROP_onoff] = column_in[1];
                        if (column_in[0].ToLower() == "!!bus_poz") loads[loads_no, loads_PROP_bus_poz] = column_in[1];
                        if (column_in[0].ToLower() == "!!bus1poz") loads[loads_no, loads_PROP_bus_poz] = column_in[1];
                        if (column_in[0] == "!!Font1") loads[loads_no, loads_PROP_Font1] = column_in[1];
                        if (column_in[0] == "!!Font1_Mask") loads[loads_no, loads_PROP_Font1_Mask] = column_in[1];
                        if (column_in[0] == "!!DrawType") loads[loads_no, loads_PROP_gph_DrawType] = column_in[1];
                        if (column.Contains("!!gph_direction=")) loads[loads_no, loads_PROP_gph_direction] = column.Remove(0, 16);
                        if (column_in[0].ToLower() == "!!sim_storage") loads[loads_no, loads_PROP_sim_storage] = column_in[1];
                        if (column_in[0].ToLower() == "!!sim_storage_attr") loads[loads_no, loads_PROP_sim_storage_attr] = column_in[1];
                        if (column_in[0].ToLower() == "!!sim_type") loads[loads_no, loads_PROP_sim_type] = column_in[1];
                        if (column_in[0].ToLower() == "!!sim_type_attr") loads[loads_no, loads_PROP_sim_type_attr] = column_in[1];
                        if (column_in[0].ToLower() == "!!microgrid") loads[loads_no, loads_PROP_MicroGrid1] = column_in[1];
                        if (column_in[0].ToLower() == "!!node_auto_draw") loads[loads_no, loads_PROP_node_auto_draw] = column_in[1];
                        if (column_in[0].ToLower() == "!!kwmax") loads[loads_no, loads_PROP_kWmax] = column_in[1];
                    }
                    if (loads[loads_no, loads_PROP_brk] == "") loads[loads_no, loads_PROP_brk] = "on";
                    if (loads[loads_no, loads_PROP_brk] == "off") loads[loads_no, loads_PROP_bus] += "@" + loads[loads_no, loads_PROP_name];

                    if ((loads[loads_no, loads_PROP_x0] != "") && (loads[loads_no, loads_PROP_y0] != ""))
                        // we can calculate pins position
                    Loads_properties_calculation(loads_no);

                    loads_no++;
                }

                if ((line1[0].ToLower() == "new") && (line1[1].Contains("loadshape."))) // we have a new line declared
                {
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        if (column.Contains("loadshape.")) loadshapes[loadshapes_no, loadshapes_PROP_name] = column.Remove(0, 10);
                        if (column_in[0] == "npts") loadshapes[loadshapes_no, loadshapes_PROP_npts] = column_in[1];
                        if (column_in[0] == "interval") loadshapes[loadshapes_no, loadshapes_PROP_interval] = column_in[1];
                        if (column_in[0] == "csvfile") loadshapes[loadshapes_no, loadshapes_PROP_csvfile] = column_in[1];
                        if (column_in[0] == "PQCSVFile") loadshapes[loadshapes_no, loadshapes_PROP_PQCSVFile] = column_in[1];
                        if (column_in[0] == "mult") loadshapes[loadshapes_no, loadshapes_PROP_mult] = column_in[1];
                        if (column_in[0] == "!!calculate_Hour2Min") loadshapes[loadshapes_no, loadshapes_PROP_calculate_Hour2Min] = column_in[1];
                        if (column_in[0] == "!!replace_PQCSVFile") loadshapes[loadshapes_no, loadshapes_PROP_replace_PQCSVFile] = column_in[1];
                    }
                    loadshapes_no++;
                }
                // analyze "linecode" objects
                if ((line1[0].ToLower() == "new") && (line1[1].Contains("linecode."))) // we have a new line declared
                {
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        if (column.Contains("linecode.")) linecodes[linecodes_no, linecodes_PROP_name] = column.Remove(0, 9);
                        //if (column_in[0] == "linecode") linecodes[linecodes_no, linecodes_PROP_name] = column_in[1];
                        if (column_in[0].ToLower() == "nphases") linecodes[linecodes_no, linecodes_PROP_nphases] = column_in[1];
                        if (column_in[0].ToLower() == "r1") linecodes[linecodes_no, linecodes_PROP_R1] = column_in[1];
                        if (column_in[0].ToLower() == "x1") linecodes[linecodes_no, linecodes_PROP_X1] = column_in[1];
                        if (column_in[0].ToLower() == "c1") linecodes[linecodes_no, linecodes_PROP_C1] = column_in[1];
                        if (column_in[0].ToLower() == "units") linecodes[linecodes_no, linecodes_PROP_units] = column_in[1];
                        if (column_in[0] == "!!Imax")
                            linecodes[linecodes_no, linecodes_PROP_Imax] = column_in[1];
                        if (column_in[0] == "!!Umax") linecodes[linecodes_no, linecodes_PROP_Umax] = column_in[1];
                    }
                    linecodes_no++;
                }
                // analyze "transformer" objects
                if ((line1[0].ToLower() == "new") && (line1[1].Contains("transformer."))) // we have a new line declared
                {
                    trafos[trafos_no, trafos_PROP_gph_direction] = "N"; // default is with connections and drawing in the north
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        if (column.Contains("transformer.")) trafos[trafos_no, trafos_PROP_name] = column.Remove(0, 12);
                        if (column_in[0] == "windings") trafos[trafos_no, trafos_PROP_windings] = column_in[1];
                        if (column.Contains("buses=")) trafos[trafos_no, trafos_PROP_busses] = column.Remove(0, 6);
                        if (column.Contains("conns=")) trafos[trafos_no, trafos_PROP_conns] = column.Remove(0, 6);
                        if (column.Contains("kVs=")) trafos[trafos_no, trafos_PROP_kVs] = column.Remove(0, 4);
                        if (column.Contains("kVAs="))
                        {
                            trafos[trafos_no, trafos_PROP_kVAs] = column.Remove(0, 5);
                            column_in = trafos[trafos_no, trafos_PROP_kVAs].Split('(',',', ')');
                            trafos[trafos_no, trafos_PROP_kVAs_sec1] = column_in[1];
                            trafos[trafos_no, trafos_PROP_kVAs_sec2] = column_in[2];
                            try
                            {
                                trafos_double[trafos_no, trafos_PROP_kVAs_sec1] = double.Parse(trafos[trafos_no, trafos_PROP_kVAs_sec1]);
                                trafos_double[trafos_no, trafos_PROP_kVAs_sec2] = double.Parse(trafos[trafos_no, trafos_PROP_kVAs_sec2]);
                            }
                            catch { }
                        }
                        if (column.Contains("%noloadloss=")) trafos[trafos_no, trafos_PROP_noloadloss] = column.Remove(0, 12);
                        if (column.Contains("%loadloss=")) trafos[trafos_no, trafos_PROP_loadloss] = column.Remove(0, 10);
                        if (column.Contains("%imag=")) trafos[trafos_no, trafos_PROP_imag] = column.Remove(0, 6);
                        if (column_in[0] == "xhl") trafos[trafos_no, trafos_PROP_xhl] = column_in[1];
                        if (column_in[0] == "wdg") trafos[trafos_no, trafos_PROP_wdg] = column_in[1];
                        if (column_in[0] == "tap") trafos[trafos_no, trafos_PROP_tap] = column_in[1];
                        if (column_in[0] == "maxtap") trafos[trafos_no, trafos_PROP_maxtap] = column_in[1];
                        if (column_in[0] == "mintap") trafos[trafos_no, trafos_PROP_mintap] = column_in[1];
                        if (column_in[0].ToLower() == "!!npoly1_xy")
                            trafos[trafos_no, trafos_PROP_npoly1_xy] = column_in[1];
                        if (column_in[0].ToLower() == "!!bus_poz") trafos[trafos_no, trafos_PROP_bus_poz] = column_in[1];
                        if (column_in[0].ToLower() == "!!bus1poz") trafos[trafos_no, trafos_PROP_bus_poz] = column_in[1];
                        if (column_in[0] == "!!brk1") trafos[trafos_no, trafos_PROP_brk1] = column_in[1];
                        if (column_in[0] == "!!brk2") trafos[trafos_no, trafos_PROP_brk2] = column_in[1];
                        if (column.Contains("!!x0=")) trafos[trafos_no, trafos_PROP_x0] = column.Remove(0, 5);
                        if (column.Contains("!!y0=")) trafos[trafos_no, trafos_PROP_y0] = column.Remove(0, 5);
                        if (column.Contains("!!gph_direction=")) trafos[trafos_no, trafos_PROP_gph_direction] = column.Remove(0, 16);
                        //  =0.4
                        if (column_in[0].ToLower() == "!!kvsprm") trafos[trafos_no, trafos_PROP_U_Prm_nom] = column_in[1];
                        if (column_in[0].ToLower() == "!!kvssec1") trafos[trafos_no, trafos_PROP_U_Sec1_nom] = column_in[1];
                        if (column_in[0].ToLower() == "!!kvssec2") trafos[trafos_no, trafos_PROP_U_Sec2_nom] = column_in[1];
                        if (column_in[0].ToLower() == "!!microgridprm") trafos[trafos_no, trafos_PROP_MicroGridPrm] = column_in[1];
                        if (column_in[0].ToLower() == "!!microgridsec1") trafos[trafos_no, trafos_PROP_MicroGridSec1] = column_in[1];
                        if (column_in[0].ToLower() == "!!microgridsec2") trafos[trafos_no, trafos_PROP_MicroGridSec2] = column_in[1];
                    }
                    if (trafos[trafos_no, trafos_PROP_busses] != "")
                    { // Example of busses description: (N2,N3)
                        char[] delimiterCharsTr = { '(', ',', ')' };
                        string[] lineTr = trafos[trafos_no, trafos_PROP_busses].Split(delimiterCharsTr);
                        trafos[trafos_no, trafos_PROP_bus1] = lineTr[1];
                        trafos[trafos_no, trafos_PROP_bus2] = lineTr[2];
                        if (trafos[trafos_no, trafos_PROP_brk1] == "") trafos[trafos_no, trafos_PROP_brk1] = "on";
                        if (trafos[trafos_no, trafos_PROP_brk2] == "") trafos[trafos_no, trafos_PROP_brk2] = "on";
                        if (trafos[trafos_no, trafos_PROP_brk1] == "off")
                            trafos[trafos_no, trafos_PROP_bus1] += "@" + trafos[trafos_no, trafos_PROP_name];
                        if (trafos[trafos_no, trafos_PROP_brk2] == "off")
                            trafos[trafos_no, trafos_PROP_bus2] += "@" + trafos[trafos_no, trafos_PROP_name];
                        trafos[trafos_no, trafos_PROP_busses] = 
                            "(" + trafos[trafos_no, trafos_PROP_bus1] + "," + trafos[trafos_no, trafos_PROP_bus2] + ")";
                    }

                    if ((trafos[trafos_no, trafos_PROP_x0] != "") && (trafos[trafos_no, trafos_PROP_y0] != ""))
                        // we can calculate pins position
                        Trafos_properties_calculation(trafos_no);

                    trafos_no++;
                }
                // analyze "line" objects
                if ((line1[0].ToLower() == "new") && (line1[1].Contains("line."))) // we have a new line declared
                {
                    lines[lines_no, lines_PROP_gph_direction] = "N"; // default is with connections and drawing in the north
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        if (column.Contains("line.")) lines[lines_no, lines_PROP_name] = column.Remove(0, 5);
                        if (column_in[0].ToLower() == "bus1") lines[lines_no, lines_PROP_bus1] = column_in[1];
                        if (column_in[0].ToLower() == "bus2") lines[lines_no, lines_PROP_bus2] = column_in[1];
                        if (column_in[0].ToLower() == "phases") lines[lines_no, lines_PROP_phases] = column_in[1];
                        if (column_in[0].ToLower() == "length") lines[lines_no, lines_PROP_length] = column_in[1];
                        if (column_in[0].ToLower() == "units") lines[lines_no, lines_PROP_units] = column_in[1];
                        if (column_in[0].ToLower() == "linecode") lines[lines_no, lines_PROP_linecode] = column_in[1];
                        if (column_in[0].ToLower() == "!!bus1poz") lines[lines_no, lines_PROP_bus1poz] = column_in[1];
                        if (column_in[0].ToLower() == "!!bus2poz") lines[lines_no, lines_PROP_bus2poz] = column_in[1];
                        if (column_in[0] == "!!x0") lines[lines_no, lines_PROP_x0] = column_in[1];
                        if (column_in[0] == "!!y0") lines[lines_no, lines_PROP_y0] = column_in[1];
                        if (column_in[0] == "!!poly_xy") lines[lines_no, lines_PROP_plyline_xys] = column_in[1];
                        if (column_in[0] == "!!DrawType") lines[lines_no, lines_PROP_gph_DrawType] = column_in[1];
                        if (column_in[0].ToLower() == "!!gph_direction") lines[lines_no, lines_PROP_gph_direction] = column_in[1];
                        if (column_in[0] == "!!brk1") lines[lines_no, lines_PROP_brk1] = column_in[1];
                        if (column_in[0] == "!!brk2") lines[lines_no, lines_PROP_brk2] = column_in[1];
                        if (column_in[0] == "!!onoff") lines[lines_no, lines_PROP_onoff] = column_in[1];
                        if (column_in[0] == "!!sign1") lines[lines_no, lines_PROP_sign1] = column_in[1];
                        if (column_in[0] == "!!sign2") lines[lines_no, lines_PROP_sign2] = column_in[1];
                        if (column_in[0] == "!!Imax") lines[lines_no, lines_PROP_Imax] = column_in[1];
                        if (column_in[0] == "!!Umax") lines[lines_no, lines_PROP_Umax] = column_in[1];
                        if (column_in[0].ToLower() == "!!outui") lines[lines_no, lines_PROP_OutUI] = column_in[1];
                        if (column_in[0].ToLower() == "!!connectiontype")
                            lines[lines_no, lines_PROP_ConnectionType] = column_in[1];

                        if (column_in[0].ToLower() == "!!font_p") lines[lines_no, lines_PROP_Font_P] = column_in[1]; // Font 1 se refera la fonuti diferite de default pentru unele din marimile tiparite
                        if (column_in[0] == "!!Font1_Mask") lines[lines_no, lines_PROP_Font1_Mask] = column_in[1];
                        if (column_in[0].ToLower() == "!!font_q") lines[lines_no, lines_PROP_Font_Q] = column_in[1];
                        if (column_in[0].ToLower() == "!!font_u") lines[lines_no, lines_PROP_Font_U] = column_in[1];
                        if (column_in[0] == "!!HidePinsNo") lines[lines_no, lines_PROP_HidePinsNo] = column_in[1];
                        if (column_in[0].ToLower() == "!!microgrid1") lines[lines_no, lines_PROP_MicroGrid1] = column_in[1];
                        if (column_in[0].ToLower() == "!!microgrid2") lines[lines_no, lines_PROP_MicroGrid2] = column_in[1];
                        if (column_in[0].ToLower() == "!!kv") lines[lines_no, lines_PROP_voltage] = column_in[1];
                        if (column_in[0].ToLower() == "!!npoly1_xy")
                            lines[lines_no, lines_PROP_npoly1_xy] = column_in[1];
                        if (column_in[0].ToLower() == "!!npoly2_xy")
                            lines[lines_no, lines_PROP_npoly2_xy] = column_in[1];
                    }
                    if (lines[lines_no, lines_PROP_brk1] == "") lines[lines_no, lines_PROP_brk1] = "on";
                    if (lines[lines_no, lines_PROP_brk2] == "") lines[lines_no, lines_PROP_brk2] = "on";
                    if (lines[lines_no, lines_PROP_brk1] == "off") lines[lines_no, lines_PROP_bus1] += "@" + lines[lines_no, lines_PROP_name];
                    if (lines[lines_no, lines_PROP_brk2] == "off") lines[lines_no, lines_PROP_bus2] += "@" + lines[lines_no, lines_PROP_name];

                    if ((lines[lines_no, lines_PROP_x0] != "") && (lines[lines_no, lines_PROP_y0] != "")) 
                        // we can calculate pins position
                        Lines_properties_calculation(lines_no);

                    lines_no++;
                }
                // analyze "polyline1" objects
                if ((line1[0].ToLower() == "!!new") && (line1[1].Contains("polyline1."))) // we have a new polyline declared
                { // polylines1
                    foreach (string column in line1)
                    {
                        if (column.Contains("polyline1.")) polylines1[polylines1_no, polylines1_PROP_name] = column.Remove(0, 10);
                        if (column.Contains("!!poly_xy=")) polylines1[polylines1_no, polylines1_PROP_polylines1_xys] = column.Remove(0, 10);
                    }
                    if (polylines1[polylines1_no, polylines1_PROP_name] != null && polylines1[polylines1_no, polylines1_PROP_polylines1_xys] != "")
                    {
                        Poly np = new Poly(polylines1[polylines1_no, polylines1_PROP_name]);
                        np.ReadLines(polylines1[polylines1_no, polylines1_PROP_polylines1_xys]);
                        polylines.Add(np);
                        crtnr++;
                    }
                    polylines1_no++;
                }
                // analyze "polylines2node" objects
                if ((line1[0].ToLower() == "!!new") && (line1[1].Contains("polylines2node="))) // we have a new polyline declared
                { // polylines2nodes
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        //if (column.Contains("loadshape.")) loadshapes[loadshapes_no, loadshapes_PROP_name] = column.Remove(0, 10);
                        if (column_in[0] == "polylines2node") polylines2node[polylines2node_no, polylines2node_PROP_name] = column_in[1];
                        if (column_in[0] == "!!npoly_xy") polylines2node[polylines2node_no, polylines2node_PROP_npolylines_xys] = column_in[1];
                        //if (column.Contains("polyline1.")) polylines1[polylines1_no, polylines1_PROP_name] = column.Remove(0, 10);
                        //if (column.Contains("!!poly_xy=")) polylines1[polylines1_no, polylines1_PROP_polylines1_xys] = column.Remove(0, 10);
                    }
                    if (polylines2node[polylines2node_no, polylines2node_PROP_name] != null && polylines2node[polylines2node_no, polylines2node_PROP_npolylines_xys] != "")
                    {
                        List<List<List<double>>> polys;
                        polys = new List<List<List<double>>>();
                        var serializer = new JsonSerializer();
                        serializer.Populate(new JsonTextReader(new StringReader(polylines2node[polylines2node_no, polylines2node_PROP_npolylines_xys])), polys);
                        int idx = 0;
                        foreach (List<List<Double>> lns in polys)
                        {
                            Poly np = new Poly(polylines2node[polylines2node_no, polylines2node_PROP_name] + "." + idx.ToString());
                            np.lines = lns;
                            polylines.Add(np);
                            crtnr++;
                            idx++;
                        }
                    }
                    polylines2node_no++;
                }
                // analyze "generator" objects
                if ((line1[0].ToLower() == "new") && (line1[1].Contains("generator."))) // we have a new generator declared
                {
                    generators[generators_no, generators_PROP_gph_direction] = "N"; // default is with connections and drawing in the north
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        if (column.Contains("generator.")) generators[generators_no, generators_PROP_name] = column.Remove(0, 10);
                        if (column.Contains("bus1=")) generators[generators_no, generators_PROP_bus] = column.Remove(0, 5);
                        if (column.Contains("phases=")) generators[generators_no, generators_PROP_phases] = column.Remove(0, 7);
                        if (column_in[0].ToLower() == "kv")
                        {
                            generators[generators_no, generators_PROP_voltage] = column_in[1];
                            try { generators_double[generators_no, generators_PROP_voltage] = double.Parse(column_in[1]); } catch { }
                        }
                        if (column_in[0].ToLower() == "kw") generators[generators_no, generators_PROP_Pn] = column_in[1];
                        if (column_in[0].ToLower() == "kvar") generators[generators_no, generators_PROP_Qn] = column_in[1];
                        if (column_in[0].ToLower() == "pf") generators[generators_no, generators_PROP_PF] = column_in[1];
                        if (column_in[0].ToLower() == "conn") generators[generators_no, generators_PROP_conn] = column_in[1];
                        if (column_in[0].ToLower() == "model") generators[generators_no, generators_PROP_model] = column_in[1];
                        if (column_in[0].ToLower() == "daily") generators[generators_no, generators_PROP_daily] = column_in[1];
                        if (column_in[0].ToLower() == "duty") generators[generators_no, generators_PROP_duty] = column_in[1];
                        if (column_in[0].ToLower() == "vminpu") generators[generators_no, generators_PROP_Vminpu] = column_in[1];
                        if (column_in[0].ToLower() == "vmaxpu") generators[generators_no, generators_PROP_Vmaxpu] = column_in[1];
                        if (column_in[0].ToLower() == "status") generators[generators_no, generators_PROP_status] = column_in[1];
                        if (column_in[0].ToLower() == "gentype") generators[generators_no, generators_PROP_gen_type] = column_in[1];
                        if (column_in[0].ToLower() == "kva") generators[generators_no, generators_PROP_kVA] = column_in[1];
                        if (column_in[0].ToLower() == "maxkvar") generators[generators_no, generators_PROP_Max_kvar] = column_in[1];
                        if (column_in[0].ToLower() == "minkvar") generators[generators_no, generators_PROP_Min_kvar] = column_in[1];
                        if (column_in[0].ToLower() == "xd") generators[generators_no, generators_PROP_Xd] = column_in[1];
                        if (column_in[0].ToLower() == "debugtrace") generators[generators_no, generators_PROP_Debugtrace] = column_in[1];
                        if (column_in[0].ToLower() == "pvfactor") generators[generators_no, generators_PROP_Pvfactor] = column_in[1];
                        if (column_in[0].ToLower() == "!!user_name") generators[generators_no, generators_PROP_user_name] = column_in[1];

                        if (column_in[0].ToLower() == "!!kw_max") generators[generators_no, generators_PROP_Pn_max] = column_in[1];
                        if (column_in[0].ToLower() == "!!kvar_max") generators[generators_no, generators_PROP_Qn_max] = column_in[1];
                        if (column_in[0] == "!!bus_poz") generators[generators_no, generators_PROP_bus_poz] = column_in[1];
                        if (column_in[0] == "!!bus1poz") generators[generators_no, generators_PROP_bus_poz] = column_in[1]; // varinat for bus_poz
                        if (column_in[0] == "!!x0") generators[generators_no, generators_PROP_x0] = column_in[1];
                        if (column_in[0] == "!!y0") generators[generators_no, generators_PROP_y0] = column_in[1];
                        if (column_in[0].ToLower() == "!!microgrid") generators[generators_no, generators_PROP_MicroGrid1] = column_in[1];
                        if (column_in[0] == "!!DrawType") generators[generators_no, generators_PROP_gph_DrawType] = column_in[1];
                        if (column.Contains("!!gph_direction=")) generators[generators_no, generators_PROP_gph_direction] = column.Remove(0, 16);
                        if (column_in[0].ToLower() == "!!node_auto_draw") generators[generators_no, generators_PROP_node_auto_draw] = column_in[1];
                    }

                    if (generators[generators_no, generators_PROP_brk] == "") generators[generators_no, generators_PROP_brk] = "on";
                    if ((generators[generators_no, generators_PROP_x0] != "") && (generators[generators_no, generators_PROP_y0] != ""))
                        // we can calculate pins position
                        Generators_properties_calculation(generators_no);

                    generators_no++;
                }
                // analyze "monitor" objects
                if ((line1[0].ToLower() == "new") && (line1[1].Contains("monitor."))) // we have a new generator declared
                {
                    foreach (string column in line1)
                    {
                        if (column.Contains("monitor.")) monitors[monitors_no, monitors_PROP_name] = column.Remove(0, 8);
                        if (column.Contains("element=")) monitors[monitors_no, monitors_PROP_element] = column.Remove(0, 8);
                        if (column.Contains("terminal=")) monitors[monitors_no, monitors_PROP_terminal] = column.Remove(0, 9);
                        if (column.Contains("mode=")) monitors[monitors_no, monitors_PROP_mode] = column.Remove(0, 5);
                        if (column.Contains("ppolar=")) monitors[monitors_no, monitors_PROP_ppolar] = column.Remove(0, 7);
                    }
                    monitors_no++;
                }
                // analyze "interract" objects
                if ((line1[0].ToLower() == "!!new") && (line1[1].Contains("interract."))) // we have a new generator declared
                {
                    foreach (string column in line1)
                    {
                        if (column.Contains("interract.")) interracts[interracts_no, interracts_PROP_name] = column.Remove(0, 10);
                        column_in = column.Split('=');
                        if (column_in[0] == "!!type") interracts[interracts_no, interracts_PROP_type] = column_in[1];
                        if (column_in[0] == "!!text") interracts[interracts_no, interracts_PROP_text] = column_in[1];
                        if (column_in[0] == "!!command") interracts[interracts_no, interracts_PROP_command] = column_in[1];
                        if (column_in[0] == "!!x0") interracts[interracts_no, interracts_PROP_x0] = column_in[1];
                        if (column_in[0] == "!!y0") interracts[interracts_no, interracts_PROP_y0] = column_in[1];
                        if (column_in[0] == "!!dx") interracts[interracts_no, interracts_PROP_dx] = column_in[1];
                        if (column_in[0] == "!!dy") interracts[interracts_no, interracts_PROP_dy] = column_in[1];
                    }
                    interracts_no++;
                }

                // analyze "nodes_metadata" objects
                if ((line1[0].ToLower() == "!!new") && (line1[1].Contains("nodes_metadata."))) // we have a new generator declared
                {
                    foreach (string column in line1)
                    {
                        if (column.Contains("nodes_metadata."))
                            nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_name] = column.Remove(0, 15);
                        column_in = column.Split('=');
                        if (column_in[0].ToLower() == "!!bus_name") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_bus_name] = column_in[1];
                        if (column_in[0].ToLower() == "!!bus_name_x") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_bus_name_x] = column_in[1];
                        if (column_in[0].ToLower() == "!!bus_name_y") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_bus_name_y] = column_in[1];
                        if (column_in[0].ToLower() == "!!bus") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_bus] = column_in[1];
                        if (column_in[0].ToLower() == "!!x0")
                        {
                            nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_x0] = column_in[1];
                            nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_x1] = nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_x0];
                            nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_x2] = nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_x0];
                        }
                        if (column_in[0].ToLower() == "!!y0")
                        {
                            nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_y0] = column_in[1];
                            nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_y1] = nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_y0];
                            nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_y2] = nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_y0];
                        }
                        if (column_in[0].ToLower() == "!!draw_uproc")
                        {
                            nodes_metadata_Draw_U_proc[nodes_metadata_no].Attrib_text = column_in[1];
                        }
                        if (column_in[0].ToLower() == "!!busbar")
                        {
                            nodes_metadata_Draw_busbar[nodes_metadata_no].Attrib_text = column_in[1];
                        }
                        if (column_in[0].ToLower() == "!!x1") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_x1] = column_in[1];
                        if (column_in[0].ToLower() == "!!y1") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_y1] = column_in[1];
                        if (column_in[0].ToLower() == "!!con1from") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_con1from] = column_in[1];
                        if (column_in[0].ToLower() == "!!x2") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_x2] = column_in[1];
                        if (column_in[0].ToLower() == "!!y2") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_y2] = column_in[1];
                        if (column_in[0].ToLower() == "!!con2from") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_con2from] = column_in[1];
                        if (column_in[0].ToLower() == "!!u_x") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_U_x] = column_in[1];
                        if (column_in[0].ToLower() == "!!u_y") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_U_y] = column_in[1];
                        if (column_in[0].ToLower() == "!!draw_u1") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_draw_U1] = column_in[1];
                        if (column_in[0].ToLower() == "!!draw_u1fi") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_draw_U1fi] = column_in[1];
                        if (column_in[0].ToLower() == "!!arrow") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_arrow] = column_in[1];
                        if (column_in[0].ToLower() == "!!draw_type") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_draw_type] = column_in[1];
                        if (column_in[0].ToLower() == "!!font1") nodes_metadata[nodes_metadata_no, nodes_metadata_PROP_Font1] = column_in[1];

                    }
                    nodes_metadata_no++;
                }

                // analyze "label" objects
                if ((line1[0].ToLower() == "!!new") && (line1[1].Contains("label."))) // we have a new generator declared
                {
                    foreach (string column in line1)
                    {
                        if (column.Contains("label."))
                            labels[labels_no, labels_PROP_name] = column.Remove(0, 6);
                        column_in = column.Split('=');
                        if (column_in[0] == "!!text") labels[labels_no, labels_PROP_text] = column_in[1];
                        if (column_in[0] == "!!font") labels[labels_no, labels_PROP_font] = column_in[1];
                        if (column_in[0] == "!!color") labels[labels_no, labels_PROP_color] = column_in[1];
                        if (column_in[0] == "!!x0") labels[labels_no, labels_PROP_x0] = column_in[1];
                        if (column_in[0] == "!!y0") labels[labels_no, labels_PROP_y0] = column_in[1];
                        if (column_in[0] == "!!font") labels[labels_no, labels_PROP_font] = column_in[1];
                        if (column_in[0] == "!!action") labels[labels_no, labels_PROP_action] = column_in[1];
                    }
                    labels_no++;
                }

                // analyze "measurement" objects
                if ((line1[0].ToLower() == "!!new") && (line1[1].Contains("measurement."))) // we have a new generator declared
                {
                    foreach (string column in line1)
                    {
                        if (column.Contains("measurement."))
                            measurements[measurements_no, measurements_PROP_name] = column.Remove(0, 12);
                        column_in = column.Split('=');
                        if (column_in[0] == "!!type") measurements[measurements_no, measurements_PROP_type] = column_in[1];
                        if (column_in[0] == "!!text") measurements[measurements_no, measurements_PROP_text] = column_in[1];
                        if (column_in[0] == "!!command") measurements[measurements_no, measurements_PROP_command] = column_in[1];
                        if (column_in[0] == "!!x0") measurements[measurements_no, measurements_PROP_x0] = column_in[1];
                        if (column_in[0] == "!!y0") measurements[measurements_no, measurements_PROP_y0] = column_in[1];
                        if (column_in[0] == "!!dx") measurements[measurements_no, measurements_PROP_dx] = column_in[1];
                        if (column_in[0] == "!!dy") measurements[measurements_no, measurements_PROP_dy] = column_in[1];
                        if (column_in[0] == "!!magnif") measurements[measurements_no, measurements_PROP_magnif] = column_in[1];
                    }
                    int measurement_exist = 0;
                    for (int m1 = 0; m1 < measurements_no; m1++)
                        if (measurements[m1, measurements_PROP_name] == measurements[measurements_no, measurements_PROP_name])
                            measurement_exist = 1;
                    if(measurement_exist == 0) measurements_no++;
                }
                
                // analyze "graph_phasors" objects
                if ((line1[0] == "!!new") && (line1[1].Contains("graph_phasors."))) // we have a new generator declared
                {
                    if (graph_phasors_no == -1) graph_phasors_no++;
                    else
                    {
                        int graph_phasors_exist = 0;
                        for (int m1 = 0; m1 <= graph_phasors_no; m1++)
                            if (graph_phasors[m1, graph_phasors_PROP_name] == line1[1].Remove(0, 14))
                                graph_phasors_exist = 1;
                        if (graph_phasors_exist == 0) graph_phasors_no++;
                    }
                    graph_phasors[graph_phasors_no, graph_phasors_PROP_name] = line1[1].Remove(0, 14);
                    int channel_crt = 0;
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        if (column_in[0] == "!!gph_text") graph_phasors[graph_phasors_no, graph_phasors_PROP_gph_text] = column_in[1];
                        if (column_in[0] == "!!command") graph_phasors[graph_phasors_no, graph_phasors_PROP_command] = column_in[1];
                        if (column_in[0] == "!!command+")
                        {
                            graph_phasors[graph_phasors_no, graph_phasors_PROP_command] += column_in[1];
                            column_cd = column.Split(';');
                            foreach (string cd_string in column_cd)
                            {
                                cdx = cd_string.Split('=');
                                if (cdx[0] == "!!command+")
                                {
                                    cd2 = cdx[1].Split('.');
                                    channel_crt = int.Parse(cd2[1]);
                                }
                                gph_phasors_properties[graph_phasors_no].ch[channel_crt] = channel_crt.ToString();
                                if (cdx[0].ToLower() == "obj") gph_phasors_properties[graph_phasors_no].obj[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "name") gph_phasors_properties[graph_phasors_no].name[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "measurement") gph_phasors_properties[graph_phasors_no].measurement[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "bus") gph_phasors_properties[graph_phasors_no].bus[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "prev") gph_phasors_properties[graph_phasors_no].prev[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "max") gph_phasors_properties[graph_phasors_no].max[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "color") gph_phasors_properties[graph_phasors_no].color[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "arrow") gph_phasors_properties[graph_phasors_no].arrow[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "meas_type") gph_phasors_properties[graph_phasors_no].meas_type[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "draw_label") gph_phasors_properties[graph_phasors_no].draw_label[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "draw_label_text") gph_phasors_properties[graph_phasors_no].draw_label_text[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "text") gph_phasors_properties[graph_phasors_no].text[channel_crt] = cdx[1].ToLower();
                            }
                        }
                        if (column_in[0] == "!!x0") graph_phasors[graph_phasors_no, graph_phasors_PROP_x0] = column_in[1];
                        if (column_in[0] == "!!y0") graph_phasors[graph_phasors_no, graph_phasors_PROP_y0] = column_in[1];
                        if (column_in[0] == "!!legend_dx") graph_phasors[graph_phasors_no, graph_phasors_PROP_legend_dx] = column_in[1];
                        if (column_in[0] == "!!magnif") graph_phasors[graph_phasors_no, graph_phasors_PROP_magnif] = column_in[1];
                        if (column_in[0] == "!!transparency") graph_phasors[graph_phasors_no, graph_phasors_PROP_transparency] = column_in[1];
                        if (column_in[0] == "!!enlarge") graph_phasors[graph_phasors_no, graph_phasors_PROP_enlarge] = column_in[1];
                    }
                }

                // analyze "graph_phasors" objects
                if ((line1[0].ToLower() == "!!new") && (line1[1].Contains("graph_sankeys."))) // we have a new generator declared
                {
                    if (graph_sankeys_no == -1) graph_sankeys_no++;
                    else
                    {
                        int graph_sankeys_exist = 0;
                        for (int m1 = 0; m1 <= graph_sankeys_no; m1++)
                            if (graph_sankeys[m1, graph_sankeys_PROP_name] == line1[1].Remove(0, 14))
                                graph_sankeys_exist = 1;
                        if (graph_sankeys_exist == 0) graph_sankeys_no++;
                    }
                    graph_sankeys[graph_sankeys_no, graph_sankeys_PROP_name] = line1[1].Remove(0, 14);
                    int channel_crt = 0;
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        if (column_in[0] == "!!gph_text") graph_sankeys[graph_sankeys_no, graph_sankeys_PROP_gph_text] = column_in[1];
                        if (column_in[0] == "!!command") graph_sankeys[graph_sankeys_no, graph_sankeys_PROP_command] = column_in[1];
                        if (column_in[0] == "!!command+")
                        {
                            graph_phasors[graph_phasors_no, graph_phasors_PROP_command] += column_in[1];
                            column_cd = column.Split(';');
                            foreach (string cd_string in column_cd)
                            {
                                /*
                                cdx = cd_string.Split('=');
                                if (cdx[0] == "!!command+")
                                {
                                    cd2 = cdx[1].Split('.');
                                    channel_crt = int.Parse(cd2[1]);
                                }
                                gph_phasors_properties[graph_phasors_no].ch[channel_crt] = channel_crt.ToString();
                                if (cdx[0].ToLower() == "obj") gph_phasors_properties[graph_phasors_no].obj[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "name") gph_phasors_properties[graph_phasors_no].name[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "measurement") gph_phasors_properties[graph_phasors_no].measurement[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "bus") gph_phasors_properties[graph_phasors_no].bus[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "prev") gph_phasors_properties[graph_phasors_no].prev[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "max") gph_phasors_properties[graph_phasors_no].max[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "color") gph_phasors_properties[graph_phasors_no].color[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "arrow") gph_phasors_properties[graph_phasors_no].arrow[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "meas_type") gph_phasors_properties[graph_phasors_no].meas_type[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "draw_label") gph_phasors_properties[graph_phasors_no].draw_label[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "draw_label_text") gph_phasors_properties[graph_phasors_no].draw_label_text[channel_crt] = cdx[1].ToLower();
                                if (cdx[0].ToLower() == "text") gph_phasors_properties[graph_phasors_no].text[channel_crt] = cdx[1].ToLower();
                                */
                            }
                        }
                        if (column_in[0] == "!!x0") graph_sankeys[graph_sankeys_no, graph_sankeys_PROP_x0] = column_in[1];
                        if (column_in[0] == "!!y0") graph_sankeys[graph_sankeys_no, graph_sankeys_PROP_y0] = column_in[1];
                        //if (column_in[0] == "!!legend_dx") graph_phasors[graph_phasors_no, graph_phasors_PROP_legend_dx] = column_in[1];
                        //if (column_in[0] == "!!magnif") graph_phasors[graph_phasors_no, graph_phasors_PROP_magnif] = column_in[1];
                        //if (column_in[0] == "!!transparency") graph_phasors[graph_phasors_no, graph_phasors_PROP_transparency] = column_in[1];
                        //if (column_in[0] == "!!enlarge") graph_phasors[graph_phasors_no, graph_phasors_PROP_enlarge] = column_in[1];
                    }
                }

                // analyze "Global_Gph_Info" objects
                if ((line1[0].ToLower() == "!!new") && (line1[1] == "!!Global_Gph_Info")) // we have a new pie declared
                {
                    Global_Gph_Info_no = 0; // by default it is targeted the first Global_Gph_Info
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        try
                        {   // this instruction may change the default Global_Gph_Info_no
                            if (column_in[0].ToLower() == "!!Global_Gph_Info_no") Global_Gph_Info_no = int.Parse(column_in[1]);
                            if (Global_Gph_Info_no > Global_Gph_Info__types_MAX) Global_Gph_Info_no = Global_Gph_Info__types_MAX;
                            if (Global_Gph_Info_no < 0) Global_Gph_Info_no = 0;
                        }
                        catch { Console.WriteLine("Load_project-Err1"); }
                        if (column_in[0].ToLower() == "!!background_image") Global_Gph_Info[Global_Gph_Info_no, Global_Gph_Info_PROP_background_image] = column_in[1];
                        if (column_in[0].ToLower() == "!!lines_background_color") Global_Gph_Info[Global_Gph_Info_no, Global_Gph_Info_PROP_lines_background_color] = column_in[1];
                        if (column_in[0].ToLower() == "!!loads_background_color")
                            Global_Gph_Info[Global_Gph_Info_no, Global_Gph_Info_PROP_loads_background_color] = column_in[1];
                        if (column_in[0].ToLower() == "!!generators_background_color")
                            Global_Gph_Info[Global_Gph_Info_no, Global_Gph_Info_PROP_generators_background_color] = column_in[1];
                        if (column_in[0].ToLower() == "!!trafos_background_color")
                            Global_Gph_Info[Global_Gph_Info_no, Global_Gph_Info_PROP_trafos_background_color] = column_in[1];
                    }
                    //Global_Gph_Info_no++;
                }


                // analyze "graph_pies" objects
                if ((line1[0].ToLower() == "!!new") && (line1[1].Contains("graph_pies."))) // we have a new pie declared
                {
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        if (column_in[0].ToLower() == "!!x0") graph_pies[graph_pies_no, graph_pies_PROP_x0] = column_in[1];
                        if (column_in[0].ToLower() == "!!y0") graph_pies[graph_pies_no, graph_pies_PROP_y0] = column_in[1];
                        if (column_in[0].ToLower() == "!!obj") graph_pies[graph_pies_no, graph_pies_PROP_obj] = column_in[1];
                        if (column_in[0].ToLower() == "!!number") graph_pies[graph_pies_no, graph_pies_PROP_number] = column_in[1];
                        if (column_in[0].ToLower() == "!!bus") graph_pies[graph_pies_no, graph_pies_PROP_bus] = column_in[1];
                        if (column_in[0].ToLower() == "!!max") graph_pies[graph_pies_no, graph_pies_PROP_max] = column_in[1];
                        if (column_in[0].ToLower() == "!!min") graph_pies[graph_pies_no, graph_pies_PROP_min] = column_in[1];
                        if (column_in[0].ToLower() == "!!max_norm") graph_pies[graph_pies_no, graph_pies_PROP_max_norm] = column_in[1];
                        if (column_in[0].ToLower() == "!!min_norm") graph_pies[graph_pies_no, graph_pies_PROP_min_norm] = column_in[1];
                        if (column_in[0].ToLower() == "!!meas_type") graph_pies[graph_pies_no, graph_pies_PROP_meas_type] = column_in[1];
                        if (column_in[0].ToLower() == "!!size")
                        {
                            graph_pies[graph_pies_no, graph_pies_PROP_size] = column_in[1];
                            try { graph_pies_double[graph_pies_no, graph_pies_PROP_size] = double.Parse(column_in[1]); } catch { }
                        }
                        //if (column_in[0] == "!!command") graph_phasors[graph_phasors_no, graph_phasors_PROP_command] = column_in[1];
                    }
                    graph_pies_no++;
                }

                // analyze "graphs" objects
                if ((line1[0].ToLower() == "!!new") && (line1[1].Contains("graphs."))) // we have a new pie declared
                {
                    foreach (string column in line1)
                    {
                        if (column.Contains("graphs."))
                            graphs[graphs_no, graphs_PROP_name] = column.Remove(0, 7);
                        column_in = column.Split('=');
                        if (column_in[0].ToLower() == "!!x0") graphs[graphs_no, graphs_PROP_x0] = column_in[1];
                        if (column_in[0].ToLower() == "!!y0") graphs[graphs_no, graphs_PROP_y0] = column_in[1];
                        if (column_in[0].ToLower() == "!!title") graphs[graphs_no, graphs_PROP_gph_title] = column_in[1];
                        if (column_in[0].ToLower() == "!!y_max") graphs[graphs_no, graphs_PROP_Y_max] = column_in[1];
                        if (column_in[0].ToLower() == "!!y_min") graphs[graphs_no, graphs_PROP_Y_min] = column_in[1];
                        if (column_in[0].ToLower() == "!!samples_max") graphs[graphs_no, graphs_PROP_Samples_max] = column_in[1];
                        if (column_in[0].ToLower() == "!!samples_x_width") graphs[graphs_no, graphs_PROP_Samples_X_width] = column_in[1];
                        if (column_in[0].ToLower() == "!!graph_type") graphs[graphs_no, graphs_PROP_Graph_type] = column_in[1];
                        if (column_in[0].ToLower() == "!!graph_dx_legend") graphs[graphs_no, graphs_PROP_dX_legend] = column_in[1];
                        if (column_in[0].ToLower() == "!!graph_dy") graphs[graphs_no, graphs_PROP_graph_dY] = column_in[1];
                    }
                    graphs_no++;
                }

                // analyze "prosumers" objects
                if ((line1[0].ToLower() == "!!new") && (line1[1].Contains("prosumer."))) // we have a new pie declared
                {
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        if (column_in[0].ToLower() == "!!x0") prosumers[prosumers_no, prosumers_PROP_x0] = column_in[1];
                        if (column_in[0].ToLower() == "!!y0") prosumers[prosumers_no, prosumers_PROP_y0] = column_in[1];
                        if (column_in[0].ToLower() == "!!name") prosumers[prosumers_no, prosumers_PROP_name] = column_in[1];
                        if (column_in[0].ToLower() == "!!text") prosumers[prosumers_no, prosumers_PROP_text] = column_in[1];
                        if (column_in[0].ToLower() == "!!obj") prosumers[prosumers_no, prosumers_PROP_obj] = column_in[1];
                        if (column_in[0].ToLower() == "!!load_number") prosumers[prosumers_no, prosumers_PROP_load_number] = column_in[1];
                        if (column_in[0].ToLower() == "!!p_scal_factor")
                            prosumers[prosumers_no, prosumers_PROP_P_scal_factor] = column_in[1];
                        if (column_in[0].ToLower() == "!!q_scal_factor") prosumers[prosumers_no, prosumers_PROP_Q_scal_factor] = column_in[1];
                        if (column_in[0].ToLower() == "!!draw_highlighted_color")
                            prosumers[prosumers_no, loads_PROP_gph_Draw_Highlighted_color] = column_in[1];
                        if (column_in[0].ToLower() == "!!draw_highlighted_blinking")
                            prosumers[prosumers_no, loads_PROP_gph_Draw_Highlighted_blinking] = column_in[1];

                        if (column_in[0].ToLower() == "!!p_source2grid") prosumers[prosumers_no, prosumers_PROP_P_source2grid] = column_in[1];
                        if (column_in[0].ToLower() == "!!saas_pair_number") prosumers[prosumers_no, prosumers_PROP_load_pair_number] = column_in[1];
                        if (column_in[0].ToLower() == "!!type") prosumers[prosumers_no, prosumers_PROP_Type] = column_in[1];
                    }
                    prosumers_no++;
                }

                // analyze "smart_meter" objects
                if ((line1[0].ToLower() == "!!new") && (line1[1].Contains("smart_meter."))) // we have a new pie declared
                {
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        if (column_in[0].ToLower() == "!!x0") smart_meters[smart_meters_no, smart_meters_PROP_x0] = column_in[1];
                        if (column_in[0].ToLower() == "!!y0") smart_meters[smart_meters_no, smart_meters_PROP_y0] = column_in[1];
                        if (column_in[0].ToLower() == "!!text") smart_meters[smart_meters_no, smart_meters_PROP_text] = column_in[1];
                        if (column_in[0].ToLower() == "!!obj") smart_meters[smart_meters_no, smart_meters_PROP_obj] = column_in[1];
                        if (column_in[0].ToLower() == "!!number") smart_meters[smart_meters_no, smart_meters_PROP_number] = column_in[1];
                        if (column_in[0].ToLower() == "!!meas_type") smart_meters[smart_meters_no, smart_meters_PROP_meas_type] = column_in[1];
                        //if (column_in[0] == "!!command") graph_phasors[graph_phasors_no, graph_phasors_PROP_command] = column_in[1];
                    }
                    smart_meters_no++;
                }

                // analyze "pie" objects
                if ((line1[0].ToLower() == "!!new") && (line1[1].Contains("smallgph."))) // we have a new pie declared
                {
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        if (column_in[0].ToLower() == "!!x0") graph_smallgph[graph_smallgph_no, graph_smallgph_PROP_x0] = column_in[1];
                        if (column_in[0].ToLower() == "!!y0") graph_smallgph[graph_smallgph_no, graph_smallgph_PROP_y0] = column_in[1];
                        if (column_in[0].ToLower() == "!!u_fact") graph_smallgph[graph_smallgph_no, graph_smallgph_PROP_U_fact] = column_in[1];
                        if (column_in[0].ToLower() == "!!p_fact") graph_smallgph[graph_smallgph_no, graph_smallgph_PROP_P_fact] = column_in[1];
                        if (column_in[0].ToLower() == "!!p_type") graph_smallgph[graph_smallgph_no, graph_smallgph_PROP_P_type] = column_in[1];
                        if (column_in[0].ToLower() == "!!obj") graph_smallgph[graph_smallgph_no, smart_meters_PROP_obj] = column_in[1];
                        if (column_in[0].ToLower() == "!!number") graph_smallgph[graph_smallgph_no, smart_meters_PROP_number] = column_in[1];
                        //if (column_in[0] == "!!command") graph_phasors[graph_phasors_no, graph_phasors_PROP_command] = column_in[1];
                    }
                    graph_smallgph_no++;
                }

                // analyze "pmu" objects
                if ((line1[0].ToLower() == "!!new") && (line1[1].Contains("pmu."))) // we have a new pie declared
                {
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        if (column_in[0].ToLower() == "!!x0") PMUs[PMUs_no, PMUs_PROP_x0] = column_in[1];
                        if (column_in[0].ToLower() == "!!y0") PMUs[PMUs_no, PMUs_PROP_y0] = column_in[1];
                        if (column_in[0].ToLower() == "!!text") PMUs[PMUs_no, PMUs_PROP_text] = column_in[1];
                        if (column_in[0].ToLower() == "!!obj") PMUs[PMUs_no, PMUs_PROP_obj] = column_in[1];
                        if (column_in[0].ToLower() == "!!number") PMUs[PMUs_no, PMUs_PROP_number] = column_in[1];
                        if (column_in[0].ToLower() == "!!meas_type") PMUs[PMUs_no, PMUs_PROP_meas_type] = column_in[1];
                    }
                    PMUs_no++;
                }

                // analyze "monitors" objects
                if ((line1[0].ToLower() == "export") && (line1[1] == "monitors")) // we have a new export declared
                {
                    exports[exports_no, exports_PROP_action] = "monitors";
                    exports[exports_no, exports_PROP_param] = line1[2];
                    exports_no++;
                }
                if (((line1[0] == "export")|| (line1[0] == "Export")) && (line1[1] == "YPrims")) OpenDSS_other_exports += line + "\n";
                if ((line1[0] == "export") && (line1[1] == "NodeNames")) OpenDSS_other_exports += line + "\n";
                if ((line1[0] == "export") && (line1[1] == "Result")) OpenDSS_other_exports += line + "\n";
                if ((line1[0] == "export") && (line1[1] == "Losses")) OpenDSS_other_exports += line + "\n";

                if ((line1[0] == "export") || (line1[0] == "Export")) {
                    if (line1[1] == "Voltages") OpenDSS_other_exports += line + "\n";
                    if (line1[1] == "Currents") OpenDSS_other_exports += line + "\n";
                    if (line1[1] == "P_ByPhase") OpenDSS_other_exports += line + "\n";
                }

                // Global variables
                //if ((line1[0].ToLower() == "set") && (line1[1].Contains("datapath="))) // we have a new load declared
                //{ OpenDSS_datapath = line1[1].Remove(0, 9); }
                if ((line1[0] == "set") && (line1[1].Contains("DefaultBaseFrequency="))) // we have a new load declared
                { OpenDSS_DefaultBaseFrequency = line1[1].Replace("DefaultBaseFrequency=", ""); }
                if ((line1[0] == "set") && (line1[1].Contains("controlmode="))) // we have a new load declared
                { OpenDSS_controlmode = line1[1].Replace("controlmode=", ""); }
                if ((line1[0] == "set") && (line1[1].Contains("mode="))) // we have a new load declared
                { OpenDSS_mode = line1[1].Replace("mode=", ""); }
                if ((line1[0].ToLower() == "edit") && (line1[1].Contains("Vsource.Source"))) // we have a new load declared
                {
                    Open_DSS_Edit_string = line;
                }
                if (line1[0].ToLower() == "solve") // we have a new load declared
                {
                    foreach (string column in line1)
                    {
                        column_in = column.Split('=');
                        if (column_in[0].ToLower() == "mode") OpenDSS_solve_mode = column_in[1];
                        if (column_in[0].ToLower() == "number") OpenDSS_solve_number = column_in[1];
                        if (column_in[0].ToLower() == "stepsize") OpenDSS_solve_stepsize = column_in[1];
                    }
                }

                if (line1[0].ToLower() == "!!new")
                    if(line1[1].ToLower()== "!!global_info") // we have ...
                    {
                        column_in = line1[2].Split('=');
                        if (column_in[0].ToLower() == "!!nodes_wires_connection") _GridMonK_nodes_wires_connection = column_in[1];
                    }

                if ((line1[0] == "!!set") && (line1[1].Contains("grid_frequency="))) // we set the system frequency
                {
                    try
                    {
                        grid_frequency = double.Parse(line1[1].Remove(0, 15));
                        if (grid_frequency > 55) grid_frequency = 55;
                        if (grid_frequency < 45) grid_frequency = 45;
                    }
                    catch { grid_frequency = 50.000; }
                }
                if ((line1[0] == "!!set") && (line1[1].Contains("Global_loads_factor="))) // we set the system frequency
                {
                    try
                    {
                        Global_loads_factor = double.Parse(line1[1].Remove(0, 20));
                        if (Global_loads_factor > 10) Global_loads_factor = 10;
                        if (Global_loads_factor < 0.1) Global_loads_factor = 0.1;
                    }
                    catch { Global_loads_factor = 1.0; }
                }
                if ((line1[0] == "!!set") && (line1[1].Contains("Global_PVs_factor="))) // we set the system frequency
                {
                    try
                    {
                        Global_PVs_factor = double.Parse(line1[1].Remove(0, 18));
                        if (Global_PVs_factor > 10) Global_PVs_factor = 10;
                        if (Global_PVs_factor < 0.1) Global_PVs_factor = 0.1;
                    }
                    catch { Global_PVs_factor = 1.0; }
                }

                if (line1[0].ToLower() == "!!HIL_FrontEnd".ToLower()) // it is defined a HIL Front End connection
                {
                    try
                    {
                        // {visible:1,x:0,y:0,font:10,color:"white"}
                        string sw1 = line1[1].Replace("{", "");
                        sw1 = sw1.Replace("}", "");
                        sw1 = sw1.Replace(", ", ","); // if we have coma with space, put only coma
                        sw1 = sw1.Replace(", ", ","); // if we have coma with space, put only coma
                        sw1 = sw1.Replace(", ", ","); // if we have coma with space, put only coma
                        char[] delimChars1 = { ',' , ';'};
                        char[] delimChars2 = { ':', '=' };
                        string[] line1a = sw1.Split(delimChars1);
                        string[] line3;
                        foreach (string line2 in line1a)
                        {
                            line3 = line2.Split(delimChars2);
                            if (line3[0].ToLower() == "Name".ToLower()) HIL_FrontEnd[HIL_FrontEnd_no, HIL_FrontEnd_PROP_Name] = line3[1];
                            if (line3[0].ToLower() == "ComType".ToLower()) HIL_FrontEnd[HIL_FrontEnd_no, HIL_FrontEnd_PROP_ComType] = line3[1];
                            if (line3[0].ToLower() == "ComAddr".ToLower()) HIL_FrontEnd[HIL_FrontEnd_no, HIL_FrontEnd_PROP_ComAddr] = line3[1];
                            if (line3[0].ToLower() == "ComPort".ToLower()) HIL_FrontEnd[HIL_FrontEnd_no, HIL_FrontEnd_PROP_ComPort] = line3[1];
                            if (line3[0].ToLower() == "Baudrate".ToLower()) HIL_FrontEnd[HIL_FrontEnd_no, HIL_FrontEnd_PROP_Baudrate] = line3[1];
                            if (line3[0].ToLower() == "ProtocolType".ToLower()) HIL_FrontEnd[HIL_FrontEnd_no, HIL_FrontEnd_PROP_ProtocolType] = line3[1];
                            if (line3[0].ToLower() == "ProtocolVer".ToLower()) HIL_FrontEnd[HIL_FrontEnd_no, HIL_FrontEnd_PROP_ProtocolVer] = line3[1];
                            if (line3[0].ToLower() == "ObjType".ToLower()) HIL_FrontEnd[HIL_FrontEnd_no, HIL_FrontEnd_PROP_ObjType] = line3[1];
                            if (line3[0].ToLower() == "ObjNo".ToLower()) HIL_FrontEnd[HIL_FrontEnd_no, HIL_FrontEnd_PROP_ObjNo] = line3[1];
                            if (line3[0].ToLower() == "Terminal".ToLower()) HIL_FrontEnd[HIL_FrontEnd_no, HIL_FrontEnd_PROP_Terminal] = line3[1];
                            if (line3[0].ToLower() == "ScaleMaxU".ToLower()) HIL_FrontEnd[HIL_FrontEnd_no, HIL_FrontEnd_PROP_ScaleMaxU] = line3[1];
                            if (line3[0].ToLower() == "ScaleMaxI".ToLower()) HIL_FrontEnd[HIL_FrontEnd_no, HIL_FrontEnd_PROP_ScaleMaxI] = line3[1];
                            if (line3[0].ToLower() == "X0".ToLower()) HIL_FrontEnd[HIL_FrontEnd_no, HIL_FrontEnd_PROP_X0] = line3[1];
                            if (line3[0].ToLower() == "Y0".ToLower()) HIL_FrontEnd[HIL_FrontEnd_no, HIL_FrontEnd_PROP_Y0] = line3[1];
                            if (line3[0].ToLower() == "Visible".ToLower()) HIL_FrontEnd[HIL_FrontEnd_no, HIL_FrontEnd_PROP_Visible] = line3[1];
                        }
                        HIL_FrontEnd_no++;
                    }
                    catch
                    {
                    }
                }

                if ((line1[0] == "!!set") && (line1[1].Contains("Scenario_timeframe_length="))) // we have a new load declared
                    Scenario_timeframe_length = line1[1].Remove(0, 26);
            }

            graph_phasors_no++; // daca era -1 va deveni zero, adica nu s-a gasit nici un astfel de obiect
            graph_sankeys_no++; // daca era -1 va deveni zero, adica nu s-a gasit nici un astfel de obiect

            Generate_nodes_list(); // lista de noduri este generata prin analiza obiectelor citite

            Add_nodes_properties_from_paramaterisation_nodes_metadata();

            Generate_output_dss("multi_LP_RMB_and_24h", "Forecast");  // producere fisier de iesire compatibil dss.

            double[] loadshapes24p = new double[24]; // 24 hours active power P
            double[] loadshapes24q = new double[24]; // 24 hours reactive power Q
            double[] loadshapes1440p = new double[24];
            double[] loadshapes1440q = new double[24];
            for (int l1 = 0; l1 < loadshapes_no; l1++)
            {
                if (loadshapes[l1, loadshapes_PROP_calculate_Hour2Min] != "")
                {
                    string loadshape_file = Grid_Projects_Path + @"/" + GridMonk_Project + @"/"
                        + loadshapes[l1, loadshapes_PROP_PQCSVFile];
                    string[] loadshape_file_text = System.IO.File.ReadAllLines(loadshape_file);
                    int hour = 0;
                    foreach (string line in loadshape_file_text)
                    {
                        char[] delimiterChars1 = { '\t', ',', ' ' };
                        string[] line1 = line.Split(delimiterChars1);
                        loadshapes24p[hour] = double.Parse(line1[0]);
                        loadshapes24q[hour] = double.Parse(line1[1]);
                        hour++;
                    }
                    string loadshape_out = "";
                    for(int h1=0; h1<24; h1++)
                    {
                        for(int m1=0; m1<60; m1++)
                        {
                            loadshape_out += loadshapes24p[h1].ToString() + "," + loadshapes24q[h1].ToString() +"\n";
                        }
                    }
                    string loadshape_file_out = Grid_Projects_Path + @"/" + GridMonk_Project + @"/"
                        + loadshapes[l1, loadshapes_PROP_calculate_Hour2Min];
                    File.WriteAllText(loadshape_file_out, loadshape_out);
                }
            }

            // Generare raport obiecte citite, afisate in consola nr. 1
            Generate_console1_report();

        }

        private void Generate_console1_report()
        {
            // se genereaza raportul cu obiectele citite, care se afiseaza in console 1
            richTextBox_console1.Text = "*** Number of linecodes=" + linecodes_no.ToString() + " ***\n";
            for (int i1 = 0; i1 < linecodes_no; i1++)
                richTextBox_console1.Text += linecodes[i1, linecodes_PROP_name] + "\n";

            richTextBox_console1.Text += "\n*** Number of lines=" + lines_no.ToString() + " ***\n";
            for (int i1 = 0; i1 < lines_no; i1++)
                richTextBox_console1.Text += lines[i1, lines_PROP_name] + "\t" + lines[i1, lines_PROP_bus1] + "\t" + lines[i1, lines_PROP_bus2] + "\n";

            richTextBox_console1.Text += "\n*** Number of loads=" + loads_no.ToString() + " ***\n";
            for (int i1 = 0; i1 < loads_no; i1++)
                richTextBox_console1.Text += loads[i1, loads_PROP_name] + "\t" + loads[i1, loads_PROP_bus] + "\n";

            richTextBox_console1.Text += "\n*** Number of loadshapes=" + loadshapes_no.ToString() + " ***\n";
            for (int i1 = 0; i1 < loadshapes_no; i1++)
                richTextBox_console1.Text += loadshapes[i1, loadshapes_PROP_name] + "\n";

            richTextBox_console1.Text += "\n*** Number of generators=" + generators_no.ToString() + " ***\n";
            for (int i1 = 0; i1 < generators_no; i1++)
                richTextBox_console1.Text += generators[i1, generators_PROP_name] + "\t" + generators[i1, generators_PROP_bus] + "\n";

            richTextBox_console1.Text += "\n*** Number of monitors=" + monitors_no.ToString() + " ***\n";
            for (int i1 = 0; i1 < monitors_no; i1++)
                richTextBox_console1.Text += monitors[i1, monitors_PROP_name] + "\t" + monitors[i1, monitors_PROP_element]
                     + "\t" + monitors[i1, monitors_PROP_terminal] + "\t" + monitors[i1, monitors_PROP_mode]
                      + monitors[i1, monitors_PROP_ppolar] + "\n";

            richTextBox_console1.Text += "\n*** Number of exports=" + exports_no.ToString() + " ***\n";
            for (int i1 = 0; i1 < exports_no; i1++)
                richTextBox_console1.Text += exports[i1, exports_PROP_action] + "\t" + exports[i1, exports_PROP_param] + "\n";

            richTextBox_console1.Text += "\n*** Number of polylines1=" + polylines1_no.ToString() + " ***\n";
            for (int i1 = 0; i1 < polylines1_no; i1++)
                richTextBox_console1.Text += polylines1[i1, polylines1_PROP_name] + "\n" + polylines1[i1, polylines1_PROP_polylines1_xys] + "\n";

            richTextBox_console1.Text += "\n*** Number of measurements=" + measurements_no.ToString() + " ***\n";
            for (int i1 = 0; i1 < measurements_no; i1++)
                richTextBox_console1.Text += measurements[i1, measurements_PROP_name] + "\n" + "\n";

        }
        // 
        private void Generate_nodes_list()
        {
            // Scan the objects to extract the list of "nodes" 
            // (OpenDSS does not declare nodes as objects, but refers them in other objects
            // const int nodes_PROP_number_of_connected_objects = 4;
            // const int nodes_PROP_list_of_connected_objects = 5; // Format: object_type.object_name.connector_name
            nodes_no = 0;
            int found_node = 0;
            // Scan loads
            for (int ld1 = 0; ld1 < loads_no; ld1++)
            {
                found_node = 0;
                for (int n1 = 0; n1 < nodes_no; n1++)
                    if (loads[ld1, loads_PROP_bus] == nodes[n1, nodes_PROP_bus])
                    {
                        found_node = 1; //n1 = nodes_no;
                        nodes[n1, nodes_PROP_list_of_connected_objects] +=
                            "load." + loads[ld1, loads_PROP_name] + "." + loads[ld1, loads_PROP_bus] + ",";
                    }
                if (found_node == 0) // este un nod nou gasit in lista de "loads", care trebuie inregistrat
                {
                    if (loads[ld1, loads_PROP_U1] != "") // check if the load has a calculated voltage, meaning non zero string
                    {
                        // se asociaza tensiunile in fiecare nod la care este conectata o sarcina
                        nodes[nodes_no, nodes_PROP_U_source_object] = "load";
                        nodes[nodes_no, nodes_PROP_U_source_object_number] = ld1.ToString();
                        nodes[nodes_no, nodes_PROP_U_source_object_name] = loads[ld1, loads_PROP_name];
                    }
                    nodes[nodes_no, nodes_PROP_bus] = loads[ld1, loads_PROP_bus];
                    nodes[nodes_no, nodes_PROP_name] = loads[ld1, loads_PROP_name];
                    nodes[nodes_no, nodes_PROP_list_of_connected_objects] +=
                        "load." + loads[ld1, loads_PROP_name] + "." + loads[ld1, loads_PROP_bus] + ",";
                    nodes_no++;
                }
            }

            // scan lines for the first terminal
            for (int l1 = 0; l1 < lines_no; l1++)
            {
                // scan the first bus of the current line
                found_node = 0;
                for (int n1 = 0; n1 < nodes_no; n1++) {
                    if (lines[l1, lines_PROP_bus1] == nodes[n1, nodes_PROP_bus])
                    {
                        found_node = 1;
                        //n1 = nodes_no;
                        nodes[n1, nodes_PROP_list_of_connected_objects] +=
                            "line." + lines[l1, lines_PROP_name] + "." + lines[l1, lines_PROP_bus1] + ",";
                        nodes[nodes_no, nodes_PROP_U_source_object_terminal] = "1";
                    }
                }
                if (found_node == 0)
                {
                    if(lines[l1, lines_PROP_U1] != "") // check if the line has a calculated voltage, meaning non zero string
                    {
                        if (nodes[nodes_no, nodes_PROP_U_source_object] == "")
                        {
                            nodes[nodes_no, nodes_PROP_U_source_object] = "line";
                            nodes[nodes_no, nodes_PROP_U_source_object_number] = l1.ToString();
                            nodes[nodes_no, nodes_PROP_U_source_object_name] = lines[l1, lines_PROP_name];
                        }
                    }
                    nodes[nodes_no, nodes_PROP_bus] = lines[l1, lines_PROP_bus1];
                    nodes[nodes_no, nodes_PROP_name] = lines[l1, lines_PROP_name];
                    nodes[nodes_no, nodes_PROP_U_source_object_terminal] = "1";
                    nodes[nodes_no, nodes_PROP_list_of_connected_objects] +=
                        "line." + lines[l1, lines_PROP_name] + "." + lines[l1, lines_PROP_bus1] + ",";
                    nodes_no++;
                }
                
                // scan the second bus of the current line
                found_node = 0;
                for (int n1 = 0; n1 < nodes_no; n1++)
                {
                    if (lines[l1, lines_PROP_bus2] == nodes[n1, nodes_PROP_bus])
                    {
                        found_node = 1;
                        //n1 = nodes_no;
                        nodes[n1, nodes_PROP_list_of_connected_objects] +=
                            "line." + lines[l1, lines_PROP_name] + "." + lines[l1, lines_PROP_bus2] + ",";
                        nodes[nodes_no, nodes_PROP_U_source_object_terminal] = "2";
                    }
                }
                if (found_node == 0)
                {
                    if (lines[l1, lines_PROP_U1_t2] != "") // check if the line has a calculated voltage, meaning non zero string
                    {

                        if (nodes[nodes_no, nodes_PROP_U_source_object] == "")
                        {
                            nodes[nodes_no, nodes_PROP_U_source_object] = "line";
                            nodes[nodes_no, nodes_PROP_U_source_object_number] = l1.ToString();
                            nodes[nodes_no, nodes_PROP_U_source_object_name] = lines[l1, lines_PROP_name];
                        }
                    }
                    nodes[nodes_no, nodes_PROP_bus] = lines[l1, lines_PROP_bus2];
                    nodes[nodes_no, nodes_PROP_name] = lines[l1, lines_PROP_name];
                    nodes[nodes_no, nodes_PROP_U_source_object_terminal] = "2";
                    nodes[nodes_no, nodes_PROP_list_of_connected_objects] +=
                        "line." + lines[l1, lines_PROP_name] + "." + lines[l1, lines_PROP_bus2] + ",";
                    nodes_no++;
                }
                
            }
            // scan trafos
            for (int t1 = 0; t1 < trafos_no; t1++)
            {
                found_node = 0;
                for (int n1 = 0; n1 < nodes_no; n1++)
                    if (trafos[t1, trafos_PROP_bus1] == nodes[n1, nodes_PROP_bus])
                    {
                        found_node = 1;
                        //n1 = nodes_no;
                        nodes[n1, nodes_PROP_list_of_connected_objects] +=
                            "trafo." + trafos[t1, trafos_PROP_name] + "." + trafos[t1, trafos_PROP_bus1] + ",";
                    }
                if (found_node == 0)
                {
                    nodes[nodes_no, nodes_PROP_bus] = trafos[t1, trafos_PROP_bus1];
                    nodes[nodes_no, nodes_PROP_name] = trafos[t1, trafos_PROP_name];
                    nodes[nodes_no, nodes_PROP_list_of_connected_objects] +=
                        "trafo." + trafos[t1, trafos_PROP_name] + "." + trafos[t1, trafos_PROP_bus1] + ",";
                    if (nodes[nodes_no, nodes_PROP_U_source_object] == "")
                    {
                        nodes[nodes_no, nodes_PROP_U_source_object] = "trafo";
                        nodes[nodes_no, nodes_PROP_U_source_object_number] = t1.ToString();
                        nodes[nodes_no, nodes_PROP_U_source_object_name] = trafos[t1, trafos_PROP_name];
                    }
                    nodes_no++;
                }
            }
            // Scan generators
            for (int g1 = 0; g1 < generators_no; g1++)
            {
                found_node = 0;
                for (int n1 = 0; n1 < nodes_no; n1++)
                    if (generators[g1, generators_PROP_bus] == nodes[n1, nodes_PROP_bus])
                    {
                        found_node = 1;
                        //n1 = nodes_no;
                        nodes[n1, nodes_PROP_list_of_connected_objects] +=
                            "generator." + generators[g1, generators_PROP_name] + "." + generators[g1, generators_PROP_bus] + ",";
                    }
                if (found_node == 0)
                {
                    nodes[nodes_no, nodes_PROP_bus] = generators[g1, loads_PROP_bus];
                    nodes[nodes_no, nodes_PROP_name] = generators[g1, loads_PROP_name];
                    nodes[nodes_no, nodes_PROP_list_of_connected_objects] +=
                        "generator." + generators[g1, loads_PROP_name] + "." + generators[g1, loads_PROP_bus] + ",";
                    if (nodes[nodes_no, nodes_PROP_U_source_object] == "")
                    {
                        nodes[nodes_no, nodes_PROP_U_source_object] = "generator";
                        nodes[nodes_no, nodes_PROP_U_source_object_number] = g1.ToString();
                        nodes[nodes_no, nodes_PROP_U_source_object_name] = generators[g1, generators_PROP_name];
                    }
                    nodes_no++;
                }
            }

            //nodes_no = 0; //int found_node = 0;
            //}
            // Allocate polylines to nodes
            for (int p1 = 0; p1 < polylines2node_no; p1++)
            {
                string poly_node_name = polylines2node[p1, polylines2node_PROP_name];
                for (int n1 = 0; n1 < nodes_no; n1++)
                {
                    if (nodes[n1, nodes_PROP_bus] == poly_node_name)
                        nodes[n1, nodes_PROP_plylines] = polylines2node[p1, polylines2node_PROP_npolylines_xys];
                }

            }

            Assess_nodes_properties();
        }

        void Add_nodes_properties_from_paramaterisation_nodes_metadata()
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
                        nodes[n1, nodes_PROP_x1] = nodes_metadata[m1, nodes_metadata_PROP_x1];
                        nodes[n1, nodes_PROP_y1] = nodes_metadata[m1, nodes_metadata_PROP_y1];
                        nodes[n1, nodes_PROP_con1from] = nodes_metadata[m1, nodes_metadata_PROP_con1from];
                        nodes[n1, nodes_PROP_x2] = nodes_metadata[m1, nodes_metadata_PROP_x2];
                        nodes[n1, nodes_PROP_y2] = nodes_metadata[m1, nodes_metadata_PROP_y2];
                        nodes[n1, nodes_PROP_con2from] = nodes_metadata[m1, nodes_metadata_PROP_con2from];
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
                        // attributes for U%, in a JSON format
                        nodes_Draw_U_proc[n1].Attrib_text = nodes_metadata_Draw_U_proc[m1].Attrib_text;
                        try
                        {
                            // {visible:1,x:0,y:0,font:10,color:"white"}
                            string sw1 = nodes_Draw_U_proc[n1].Attrib_text.Replace("{", "");
                            sw1 = sw1.Replace("}", "");
                            char[] delimiterChars1 = { ',' };
                            char[] delimiterChars2 = { ':' };
                            string[] line1 = sw1.Split(delimiterChars1);
                            string[] line3;
                            foreach (string line2 in line1)
                            {
                                line3 = line2.Split(delimiterChars2);
                                if (line3[0].ToLower() == "visible") nodes_Draw_U_proc[n1].Visible = line3[1];
                                if (line3[0].ToLower() == "x0") nodes_Draw_U_proc[n1].x0 = int.Parse(line3[1]);
                                if (line3[0].ToLower() == "y0") nodes_Draw_U_proc[n1].y0 = int.Parse(line3[1]);
                                if (line3[0].ToLower() == "font") nodes_Draw_U_proc[n1].Font = int.Parse(line3[1]);
                                if (line3[0].ToLower() == "color") nodes_Draw_U_proc[n1].Color = line3[1].Replace("\"","");

                            }

                        } catch { Console.WriteLine("Add_nodes_properties_from_paramaterisation_nodes_metadata-Err1"); }
                        // attributes for nodes busbar, in a JSON format
                        nodes_Draw_busbar[n1].Attrib_text = nodes_metadata_Draw_busbar[m1].Attrib_text;
                        try
                        {
                            // {enable:1,direction:N,bays:3,bay_size:10}
                            string sw1 = nodes_Draw_busbar[n1].Attrib_text.Replace("{", "");
                            sw1 = sw1.Replace("}", "");
                            char[] delimiterChars1 = { ',' };
                            char[] delimiterChars2 = { ':' };
                            string[] line1 = sw1.Split(delimiterChars1);
                            string[] line3;
                            foreach (string line2 in line1)
                            {
                                line3 = line2.Split(delimiterChars2);
                                if (line3[0].ToLower() == "enable") nodes_Draw_busbar[n1].Enable = line3[1];
                                if (line3[0].ToLower() == "direction") nodes_Draw_busbar[n1].Direction = line3[1];
                                if (line3[0].ToLower() == "bays") nodes_Draw_busbar[n1].Bays_number = int.Parse(line3[1]);
                                if (line3[0].ToLower() == "bay_size") nodes_Draw_busbar[n1].Bay_size = int.Parse(line3[1]);
                            }
                            for(int b1=0; b1< nodes_Draw_busbar[n1].Bays_number; b1++)
                            {
                                if(nodes_Draw_busbar[n1].Direction == "N")
                                {
                                    // busbar to the North ("N")
                                    nodes_Draw_busbar[n1].coord_x[b1] = 0; // x
                                    nodes_Draw_busbar[n1].coord_y[b1] =
                                        -(nodes_Draw_busbar[n1].Bay_size / 2 + b1 * nodes_Draw_busbar[n1].Bay_size); // y
                                }
                                if (nodes_Draw_busbar[n1].Direction == "E")
                                {
                                    // busbar to the East ("E")
                                    nodes_Draw_busbar[n1].coord_y[b1] = 0; // y = constant
                                    nodes_Draw_busbar[n1].coord_x[b1] =
                                        +(nodes_Draw_busbar[n1].Bay_size / 2 + b1 * nodes_Draw_busbar[n1].Bay_size); // y
                                }
                            }
                        }
                        catch { Console.WriteLine("Add_nodes_properties_from_paramaterisation_nodes_metadata-Err1"); }

                        // nodes_properties_calculation(n1);
                    }
                }
            }

        }


        private void Generate_output_dss(string OpenDSS_file, string type)
        {
            string type_of_file = type;
            // pregatirea fisierului de iesire, in format dss
            string s1 = "!!type_of_file=" + type_of_file + "\n";
            s1 += "!!GMK_version=0.99-2018.02.27\n";
            s1 += "!\n";
            //s1 += "set datapath=" + OpenDSS_datapath + "\n";
            s1 += "set datapath=" + OpenDSS_datapath + "\n";
            s1 += "Set DefaultBaseFrequency=" + OpenDSS_DefaultBaseFrequency + "\n";
            s1 += "!\n";

            /*if (column.Contains("Circuit.")) OpenDSS_Circuit_name = column.Remove(0, 8);
            if (column_in[0] == "basekV") OpenDSS_Circuit_basekV = column_in[1];
            if (column_in[0] == "pu") OpenDSS_Circuit_pu = column_in[1];
            if (column_in[0] == "angle") OpenDSS_Circuit_angle = column_in[1];
            if (column_in[0] == "phases") OpenDSS_Circuit_phases = column_in[1];
            if (column_in[0] == "Vminpu") OpenDSS_Circuit_Vminpu = column_in[1];
            if (column_in[0] == "Vmaxpu") OpenDSS_Circuit_Vmaxpu = column_in[1];
            */

            string Circuit_name = GridMonk_Project;
            if (type_of_file == "U_stability") Circuit_name = "U_stability";

            //s1 += "new " + "Circuit." + GridMonk_Project + " basekV=" + OpenDSS_Circuit_basekV + " pu=" + OpenDSS_Circuit_pu
            s1 += "new " + "Circuit." + Circuit_name;
            if (OpenDSS_Circuit_bus1 != "") s1 += " bus1=" + OpenDSS_Circuit_bus1;
            if (OpenDSS_Circuit_basekV != "") s1 += " basekV=" + OpenDSS_Circuit_basekV;
            if (OpenDSS_Circuit_pu != "") s1 += " pu=" + OpenDSS_Circuit_pu;
            if (OpenDSS_Circuit_angle != "") s1 += " angle=" + OpenDSS_Circuit_angle;
            if (OpenDSS_Circuit_phases != "") s1 += " phases=" + OpenDSS_Circuit_phases;
            if (OpenDSS_Circuit_Vminpu != "") s1 += " Vminpu=" + OpenDSS_Circuit_Vminpu;
            if (OpenDSS_Circuit_Vmaxpu != "") s1 += " Vmaxpu=" + OpenDSS_Circuit_Vmaxpu;
            if (OpenDSS_Circuit_MVASC1 != "") s1 += " MVASC1=" + OpenDSS_Circuit_MVASC1;
            if (OpenDSS_Circuit_MVASC3 != "") s1 += " MVASC3=" + OpenDSS_Circuit_MVASC3;
            if (OpenDSS_Circuit_R1 != "") s1 += " R1=" + OpenDSS_Circuit_R1;
            if (OpenDSS_Circuit_X1 != "") s1 += " X1=" + OpenDSS_Circuit_X1;
            if (Open_DSS_Edit_string != "") s1 += "\n" + Open_DSS_Edit_string + "\n";

            s1 += "\n";

            for (int g1 = 0; g1 < vsources_no; g1++)
            {
                s1 += "new Vsource.";
                s1 += vsources[g1, vsources_PROP_name];
                s1 += " BasekV=" + vsources[g1, vsources_PROP_BasekV];
                s1 += " pu=" + vsources[g1, vsources_PROP_pu];
                s1 += " angle=" + vsources[g1, vsources_PROP_angle];
                s1 += " phase=" + vsources[g1, vsources_PROP_phase];
                if (vsources[g1, vsources_PROP_baseMVA] != "") s1 += " baseMVA=" + vsources[g1, vsources_PROP_baseMVA];
                if(vsources[g1, vsources_PROP_Mvasc3] != "") s1 += " Mvasc3=" + vsources[g1, vsources_PROP_Mvasc3];
                if (vsources[g1, vsources_PROP_Mvasc1] != "") s1 += " Mvasc1=" + vsources[g1, vsources_PROP_Mvasc1];
                if (vsources[g1, vsources_PROP_isc3] != "") s1 += " isc3=" + vsources[g1, vsources_PROP_isc3];
                if (vsources[g1, vsources_PROP_isc1] != "") s1 += " isc1=" + vsources[g1, vsources_PROP_isc1];
                if (vsources[g1, vsources_PROP_R1] != "") s1 += " R1=" + vsources[g1, vsources_PROP_R1];
                if (vsources[g1, vsources_PROP_X1] != "") s1 += " X1=" + vsources[g1, vsources_PROP_X1];
                s1 += "\n";
            }
            s1 += "!\n";
            // Raportare polylines:
            //for (int g1 = 0; g1 < polylines1_no; g1++)
            //{
            //        s1 += " " + polylines[0].mkString();
            //}
            for (int g1 = 0; g1 < linecodes_no; g1++)
            {
                s1 += "new linecode.";
                s1 += linecodes[g1, linecodes_PROP_name];
                s1 += " nphases=" + linecodes[g1, linecodes_PROP_nphases];
                s1 += " R1=" + linecodes[g1, linecodes_PROP_R1];
                s1 += " X1=" + linecodes[g1, linecodes_PROP_X1];
                if (linecodes[g1, linecodes_PROP_C1] != "") s1 += " C1=" + linecodes[g1, linecodes_PROP_C1];
                s1 += " units=" + linecodes[g1, linecodes_PROP_units];
                s1 += "\n";
            }
            s1 += "!\n";
            for (int ld1 = 0; ld1 < loadshapes_no; ld1++)
            {
                s1 += "new loadshape.";
                s1 += loadshapes[ld1, loadshapes_PROP_name];
                s1 += " npts=" + loadshapes[ld1, loadshapes_PROP_npts];
                if (loadshapes[ld1, loadshapes_PROP_interval].Contains("#"))
                    s1 += " interval=" + loadshapes[ld1, loadshapes_PROP_interval].Replace("#", " ");
                else s1 += " interval=" + loadshapes[ld1, loadshapes_PROP_interval];
                if (loadshapes[ld1, loadshapes_PROP_csvfile] != "") s1 += " csvfile=" + loadshapes[ld1, loadshapes_PROP_csvfile];

                if (loadshapes[ld1, loadshapes_PROP_PQCSVFile] != "")
                {
                    if(loadshapes[ld1, loadshapes_PROP_replace_PQCSVFile]=="")
                        s1 += " PQCSVFile=" + loadshapes[ld1, loadshapes_PROP_PQCSVFile];
                    else
                        s1 += " PQCSVFile=" + loadshapes[ld1, loadshapes_PROP_replace_PQCSVFile];
                }
                //s1 += " mult=" + loadshapes[ld1, loadshapes_PROP_mult];
                s1 += "\n";
            }
            s1 += "!\n";
            for (int l1 = 0; l1 < trafos_no; l1++)
            {
                s1 += "new transformer." + trafos[l1, trafos_PROP_name];
                s1 += " windings=" + trafos[l1, trafos_PROP_windings];
                s1 += " buses=" + trafos[l1, trafos_PROP_busses];
                s1 += " conns=" + trafos[l1, trafos_PROP_conns];
                s1 += " kVs=" + trafos[l1, trafos_PROP_kVs];
                s1 += " kVAs=" + trafos[l1, trafos_PROP_kVAs];
                s1 += " %noloadloss=" + trafos[l1, trafos_PROP_noloadloss];
                s1 += " %loadloss=" + trafos[l1, trafos_PROP_loadloss];
                s1 += " %imag=" + trafos[l1, trafos_PROP_imag];
                s1 += " xhl=" + trafos[l1, trafos_PROP_xhl];
                s1 += " wdg=" + trafos[l1, trafos_PROP_wdg];
                s1 += " tap=" + trafos[l1, trafos_PROP_tap];
                s1 += " maxtap=" + trafos[l1, trafos_PROP_maxtap];
                s1 += " mintap=" + trafos[l1, trafos_PROP_mintap];
                if (trafos[l1, trafos_PROP_x0] != "") s1 += " !!x0=" + trafos[l1, trafos_PROP_x0];
                if (trafos[l1, trafos_PROP_y0] != "") s1 += " !!y0=" + trafos[l1, trafos_PROP_y0];
                if (trafos[l1, trafos_PROP_gph_direction] != "") s1 += " !!gph_direction=" + trafos[l1, trafos_PROP_gph_direction];
                s1 += "\n";
            }
            s1 += "!\n";
            for (int l1 = 0; l1 < lines_no; l1++)
            {
                s1 += "new line.";
                s1 += lines[l1, lines_PROP_name];
                s1 += " bus1=" + lines[l1, lines_PROP_bus1];
                s1 += " bus2=" + lines[l1, lines_PROP_bus2];
                s1 += " length=" + lines[l1, lines_PROP_length];
                s1 += " phases=" + lines[l1, lines_PROP_phases];
                s1 += " units=" + lines[l1, lines_PROP_units];
                s1 += " linecode=" + lines[l1, lines_PROP_linecode];
                if (lines[l1, lines_PROP_x0] != "") s1 += " !!x0=" + lines[l1, lines_PROP_x0];
                if (lines[l1, lines_PROP_y0] != "") s1 += " !!y0=" + lines[l1, lines_PROP_y0];
                if (lines[l1, lines_PROP_Imax] != "") s1 += " !!Imax=" + lines[l1, lines_PROP_Imax];
                if (lines[l1, lines_PROP_Umax] != "") s1 += " !!Umax=" + lines[l1, lines_PROP_Umax];
                if (lines[l1, lines_PROP_gph_direction] != "") s1 += " !!gph_direction=" + lines[l1, lines_PROP_gph_direction];
                if (lines[l1, lines_PROP_brk1] != "") s1 += " !!brk1=" + lines[l1, lines_PROP_brk1];
                if (lines[l1, lines_PROP_brk2] != "") s1 += " !!brk2=" + lines[l1, lines_PROP_brk2];
                if (lines[l1, lines_PROP_voltage] != "") s1 += " !!kV=" + lines[l1, lines_PROP_voltage];
                if (lines[l1, lines_PROP_MicroGrid1] != "") s1 += " !!MicroGrid1=" + lines[l1, lines_PROP_MicroGrid1];
                if (lines[l1, lines_PROP_MicroGrid2] != "") s1 += " !!MicroGrid2=" + lines[l1, lines_PROP_MicroGrid2];
                s1 += "\n";
            }
            s1 += "!\n";
            //s1 += "new transformer.T1 windings=2 buses=(N2, N3) conns=(delta, wye) kVs=(20, 0.4) kVAs=(630, 630) %noloadloss=0.025 %loadloss=0.095 %imag=0 xhl=2.5 wdg=1 tap=1 maxtap=1.05 mintap=0.85" + "\n"; // very rigid implementation, will be changed later
            //s1 += "!\n";

            for (int ld1 = 0; ld1 < loads_no; ld1++)
            {
                s1 += "new load.";
                s1 += loads[ld1, loads_PROP_name];
                s1 += " bus1=" + loads[ld1, loads_PROP_bus];
                s1 += " phases=" + loads[ld1, loads_PROP_phases];
                s1 += " kV=" + loads[ld1, loads_PROP_voltage];
                
                // Considering Global_loads_factor
                double Pn_factorized, Qn_factorized;
                if(loads[ld1, loads_PROP_sim_type] == "") // if we have classic load, meaning not storage or EV we can apply the lad factor
                //    if (loads[ld1, loads_PROP_sim_storage] == "")
                        Pn_factorized = double.Parse(loads[ld1, loads_PROP_Pn]) * Global_loads_factor;
                else Pn_factorized = double.Parse(loads[ld1, loads_PROP_Pn]); // daca e de tip storage nu se aplica Global_loads_factor
                //s1 += " kW=" + loads[ld1, loads_PROP_Pn];

                //S_max_consumption
                if (type_of_file == "Forecast") s1 += " kW=" + Pn_factorized.ToString("##0.00");
                if (type_of_file == "U_stability") s1 += " kW=" + S_max_consumption;

                if (type_of_file == "Forecast") if (loads[ld1, loads_PROP_Qn] != "")
                    {
                        if (loads[ld1, loads_PROP_sim_storage] == "")
                            Qn_factorized = double.Parse(loads[ld1, loads_PROP_Qn]) * Global_loads_factor;
                        else Qn_factorized = double.Parse(loads[ld1, loads_PROP_Qn]); // daca e de tip storage nu se aplica Global_loads_factor
                        s1 += " kvar=" + Qn_factorized.ToString("##0.00");
                    }
                if (type_of_file == "Forecast") if (loads[ld1, loads_PROP_PF] != "") s1 += " pf=" + loads[ld1, loads_PROP_PF];
                if (type_of_file == "U_stability") s1 += " kvar=" + S_max_consumption;

                s1 += " model=" + loads[ld1, loads_PROP_model];
                if (loads[ld1, loads_PROP_conn] != "") s1 += " conn=" + loads[ld1, loads_PROP_conn];

                if (loads[ld1, loads_PROP_daily] != "") { 
                    if (type_of_file == "Forecast") s1 += " daily=" + loads[ld1, loads_PROP_daily];

                    if (type_of_file == "U_stability")
                    {
                        if (textBox_U_stability_Load_no_value == ld1) s1 += " daily=" + "Load_PQ_circular_scan";
                        else s1 += " daily=" + "Load_PQ_constant_1500";
                    }
                }
                if (loads[ld1, loads_PROP_duty] != "") s1 += " duty=" + loads[ld1, loads_PROP_duty];
                if (loads[ld1, loads_PROP_status] != "") s1 += " status=" + loads[ld1, loads_PROP_status];
                if (loads[ld1, loads_PROP_Vminpu] != "") s1 += " Vminpu=" + loads[ld1, loads_PROP_Vminpu];
                if (loads[ld1, loads_PROP_Vmaxpu] != "") s1 += " Vmaxpu=" + loads[ld1, loads_PROP_Vmaxpu];
                if (loads[ld1, loads_PROP_brk] != "") s1 += " !!brk=" + loads[ld1, loads_PROP_brk];
                if (loads[ld1, loads_PROP_x0] != "") s1 += " !!x0=" + loads[ld1, loads_PROP_x0];
                if (loads[ld1, loads_PROP_y0] != "") s1 += " !!y0=" + loads[ld1, loads_PROP_y0];
                if (loads[ld1, loads_PROP_MicroGrid1] != "") s1 += " !!MicroGrid=" + loads[ld1, loads_PROP_MicroGrid1];
                if (loads[ld1, loads_PROP_gph_direction] != "") s1 += " !!gph_direction=" + loads[ld1, loads_PROP_gph_direction];
                s1 += "\n";
            }
            s1 += "!\n";
            for (int g1 = 0; g1 < generators_no; g1++)
            {
                if (generators[g1, generators_PROP_brk] == "off") {
                    s1 += "new load.";
                    s1 += generators[g1, generators_PROP_name]+"_discharge";
                    s1 += " bus1=" + generators[g1, generators_PROP_bus];
                    s1 += " phases=" + generators[g1, generators_PROP_phases];
                    s1 += " kV=" + generators[g1, generators_PROP_voltage];
                    s1 += " kw=0";
                    s1 += " kvar=0";
                    s1 += " model=1";
                    s1 += " conn=wye";
                    s1 += "\n";
                }
                s1 += "new generator.";
                s1 += generators[g1, generators_PROP_name];
                if (generators[g1, generators_PROP_brk] == "off") s1 += " bus1=" + "Source01";
                else s1 += " bus1=" + generators[g1, generators_PROP_bus];
                s1 += " phases=" + generators[g1, generators_PROP_phases];
                s1 += " kV=" + generators[g1, generators_PROP_voltage];
                
                // Considering Global_PVs_factor

                double Pn_factorized = 0;
                if (generators[g1, generators_PROP_Pn] != "")
                    Pn_factorized = double.Parse(generators[g1, generators_PROP_Pn]) * Global_PVs_factor;
                // if the generator's breaker is opne, the power P of the generator will be set to zero, otherwise the OpenDSS loadflow will not converge
                if (generators[g1, generators_PROP_brk] == "off") Pn_factorized = 0;
                s1 += " kW=" + Pn_factorized.ToString("000.00");

                double Qn_factorized = 0;
                if (generators[g1, generators_PROP_Qn] != "")
                    Qn_factorized = double.Parse(generators[g1, generators_PROP_Qn]) * Global_PVs_factor;
                // if the generator's breaker is opne, the power Q of the generator will be set to zero, otherwise the OpenDSS loadflow will not converge
                if (generators[g1, generators_PROP_brk] == "off") Qn_factorized = 0;
                if (generators[g1, generators_PROP_Qn] != "") s1 += " kvar=" + Qn_factorized.ToString("000.00");

                if (generators[g1, generators_PROP_PF] != "") s1 += " pf=" + generators[g1, generators_PROP_PF];
                if (generators[g1, generators_PROP_conn] != "") s1 += " conn=" + generators[g1, generators_PROP_conn];
                if (generators[g1, generators_PROP_model] != "") s1 += " model=" + generators[g1, generators_PROP_model];
                // new attributes added
                if (generators[g1, generators_PROP_kVA] != "") s1 += " kva=" + generators[g1, generators_PROP_kVA];
                if (generators[g1, generators_PROP_Max_kvar] != "") s1 += " maxkvar=" + generators[g1, generators_PROP_Max_kvar];
                if (generators[g1, generators_PROP_Min_kvar] != "") s1 += " minkvar=" + generators[g1, generators_PROP_Min_kvar];
                if (generators[g1, generators_PROP_Xd] != "") s1 += " xd=" + generators[g1, generators_PROP_Xd];
                if (generators[g1, generators_PROP_Debugtrace] != "") s1 += " debugtrace=" + generators[g1, generators_PROP_Debugtrace];
                if (generators[g1, generators_PROP_Pvfactor] != "") s1 += " pvfactor=" + generators[g1, generators_PROP_Pvfactor];

                if (generators[g1, generators_PROP_Vminpu] != "") s1 += " Vminpu=" + generators[g1, generators_PROP_Vminpu];
                if (generators[g1, generators_PROP_Vmaxpu] != "") s1 += " Vmaxpu=" + generators[g1, generators_PROP_Vmaxpu];
                if (generators[g1, generators_PROP_status] != "") s1 += " status=" + generators[g1, generators_PROP_status];
                if (generators[g1, generators_PROP_daily] != "") s1 += " daily=" + generators[g1, generators_PROP_daily];
                if (generators[g1, generators_PROP_duty] != "") s1 += " Duty=" + generators[g1, generators_PROP_duty];
                if (generators[g1, generators_PROP_x0] != "") s1 += " !!x0=" + generators[g1, generators_PROP_x0];
                if (generators[g1, generators_PROP_y0] != "") s1 += " !!y0=" + generators[g1, generators_PROP_y0];
                if (generators[g1, generators_PROP_MicroGrid1] != "") s1 += " !!MicroGrid=" + generators[g1, generators_PROP_MicroGrid1];
                if (generators[g1, generators_PROP_gph_direction] != "") s1 += " !!gph_direction=" + generators[g1, generators_PROP_gph_direction];
                s1 += "\n";
            }
            s1 += "!\n";
            for (int n1 = 0; n1 < nodes_metadata_no; n1++)
            {
                s1 += "!!new nodes_metadata.";
                s1 += nodes_metadata[n1, nodes_metadata_PROP_name];
                s1 += " !!bus_name=" + nodes_metadata[n1, nodes_metadata_PROP_bus_name];
                s1 += " !!bus=" + nodes_metadata[n1, nodes_metadata_PROP_bus];
                s1 += " !!x0=" + nodes_metadata[n1, nodes_metadata_PROP_x0];
                s1 += " !!y0=" + nodes_metadata[n1, nodes_metadata_PROP_y0];
                s1 += " !!x1=" + nodes_metadata[n1, nodes_metadata_PROP_x1];
                s1 += " !!y1=" + nodes_metadata[n1, nodes_metadata_PROP_y1];
                s1 += " !!con1from=" + nodes_metadata[n1, nodes_metadata_PROP_con1from];
                s1 += " !!x2=" + nodes_metadata[n1, nodes_metadata_PROP_x2];
                s1 += " !!y2=" + nodes_metadata[n1, nodes_metadata_PROP_y2];
                s1 += " !!con2from=" + nodes_metadata[n1, nodes_metadata_PROP_con2from];
                s1 += " !!draw_U1=" + nodes_metadata[n1, nodes_metadata_PROP_draw_U1];
                s1 += " !!draw_U1fi=" + nodes_metadata[n1, nodes_metadata_PROP_draw_U1fi];
                s1 += " !!arrow=" + nodes_metadata[n1, nodes_metadata_PROP_arrow];
                s1 += "\n";
            }
            s1 += "!\n";
            for (int n1 = 0; n1 < labels_no; n1++)
            {
                s1 += "!!new labels.";
                s1 += labels[n1, labels_PROP_name];
                s1 += " !!text=" + labels[n1, labels_PROP_text];
                s1 += " !!font=" + labels[n1, labels_PROP_font];
                s1 += " !!color=" + labels[n1, labels_PROP_color];
                s1 += " !!x0=" + labels[n1, labels_PROP_x0];
                s1 += " !!y0=" + labels[n1, labels_PROP_y0];
                s1 += "\n";
            }
            s1 += "!\n";
            s1 += "set controlmode=" + OpenDSS_controlmode + "\n";
            //s1 += "set mode=" + OpenDSS_mode + " stepsize=15m number=26" + "\n";
            s1 += "set mode=" + OpenDSS_mode + "\n";
            s1 += "!\n";
            for (int m1 = 0; m1 < monitors_no; m1++)
            {
                s1 += "new monitor.";
                s1 += monitors[m1, monitors_PROP_name];
                s1 += " element=" + monitors[m1, monitors_PROP_element];
                s1 += " terminal=" + monitors[m1, monitors_PROP_terminal];
                s1 += " mode=" + monitors[m1, monitors_PROP_mode];
                s1 += " ppolar=" + monitors[m1, monitors_PROP_ppolar];
                s1 += "\n";
            }
            s1 += "!\n";
            s1 += "// ******* Order to make all calculations:" + "\n";
            //s1 += "solve" + "\n";
            if (type_of_file == "Forecast")
            {
                s1 += "solve ";
                if (OpenDSS_solve_mode != "") s1 += "mode=" + OpenDSS_solve_mode;
                if (OpenDSS_solve_number != "") s1 += " number=" + OpenDSS_solve_number;
                if (OpenDSS_solve_stepsize != "") s1 += " stepsize=" + OpenDSS_solve_stepsize;
                s1 += "\n";
            }
            if (type_of_file == "U_stability") s1 += "solve mode=daily number=1500 stepsize=1s" + "\n";
            if (type_of_file == "One_LP") s1 += "solve mode=daily number=1 stepsize=1s" + "\n";
            s1 += "!\n";
            for (int e1 = 0; e1 < exports_no; e1++)
            {
                s1 += "export " + exports[e1, exports_PROP_action] + " " + exports[e1, exports_PROP_param] + "\n";
            }

            s1 += "!\n";

            for (int n1 = 0; n1 < nodes_no; n1++)
            { // salvare in fisierul output1.dss a tuturor nodurilor
                // In OpenDSS, nodurile nu sunt definite ca obiecte, ci se deduc prin anailza obiectelor
                // obiecte posibile (implementate on Grid_MonC): lines, loads, generators, trafos
                s1 += "!!nodes ";
                s1 += " number=" + n1.ToString();
                s1 += " name=Node#" + n1.ToString("0000");//nodes[n1, nodes_PROP_name];
                s1 += " bus=" + nodes[n1, nodes_PROP_bus];
                s1 += " U_source_object=" + nodes[n1, nodes_PROP_U_source_object];
                s1 += " U_source_object_number=" + nodes[n1, nodes_PROP_U_source_object_number];
                s1 += " list=" + nodes[n1, nodes_PROP_list_of_connected_objects];
                // nodes[nodes_no, nodes_PROP_list_of_connected_objects] 
                if (nodes[n1, nodes_PROP_x0] != "") s1 += " x0=" + nodes[n1, nodes_PROP_x0];
                if (nodes[n1, nodes_PROP_y0] != "") s1 += " y0=" + nodes[n1, nodes_PROP_y0];
                if (nodes[n1, nodes_PROP_plylines] != "") s1 += " polylines=" + nodes[n1, nodes_PROP_plylines];
                s1 += "\n";
            }
            s1 += "!\n";

            for (int n1 = 0; n1 < polylines1_no; n1++)
            { // salvare in fisierul output1.dss a tuturor legaturilor de tip "polyline", din matricea "polylines1"
                s1 += "!!new polyline1.";
                s1 += polylines1[n1, polylines1_PROP_name];
                s1 += " !!poly_xy=" + polylines1[n1, polylines1_PROP_polylines1_xys];
                s1 += "\n";
            }
            s1 += "!\n";
            for (int n1 = 0; n1 < polylines2node_no; n1++)
            { // salvare in fisierul output1.dss a tuturor legaturilor de tip "polyline", din matricea "polylines1"
                s1 += "!!new polylines2node.";
                s1 += polylines2node[n1, polylines2node_PROP_name];
                s1 += " !!npoly_xy=" + polylines2node[n1, polylines2node_PROP_npolylines_xys];
                s1 += "\n";
            }
            s1 += "!\n";

            s1 += OpenDSS_other_exports;

            //GridMonk2OpenDSS_grid_file = Grid_Projects_Path + @"\" + GridMonk_Project + @"\" + "output1.dss";
            GridMonk2OpenDSS_grid_file = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + OpenDSS_file + ".dss";
            File.WriteAllText(GridMonk2OpenDSS_grid_file, s1);

            Refresh();
        }

    }
}