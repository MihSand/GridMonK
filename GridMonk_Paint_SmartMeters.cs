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
using Newtonsoft.Json.Linq;
using System.IO;
using System.Diagnostics;

namespace GridMonC
{
    public partial class GridMonk : Form
    {

        public class SMX
        {
            public class SMXProperty
            {
                [JsonProperty("-2")]
                public String value { get; set; }

                [JsonProperty("-5")]
                public String datetime { get; set; }
            }
            [JsonProperty("1-1-1-8-0-255")]
            public SMXProperty Ap { get; set; }

            [JsonProperty("1-1-131-7-0-255")]
            public SMXProperty Rp { get; set; }
        }

        int Smart_Meter_Pie_x0 = 0;
        int Smart_Meter_Pie_y0 = 0;
        string Smart_Meter_Pie_type = "";
        double Smart_Meter_Pie_val1 = 0;
        double Smart_Meter_Pie_val2 = 0;
        double Smart_Meter_Pie_val3 = 0;
        double Smart_Meter_Pie_value_max = 100;
        double Smart_Meter_Pie_value_min = -100;
        private void Paint_Smart_Meter_Pie(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            int value_max = (int)Smart_Meter_Pie_value_max; // value max, at right part fo the pie 
            int value_min = (int)Smart_Meter_Pie_value_min; // value min, at left part fo the pie
            //int value_max_norm = 100; // normal value max, at right part fo the pie 
            //int value_min_norm = -100; // normal value min, at left part fo the pie
            int obj_number = -1;
            int value_center = 0;
            string pie_meas_type = Smart_Meter_Pie_type;

            int value_1 = 0, value_2 = 0, value_3 = 0;
            double value_1d = 0, value_2d = 0, value_3d = 0;

            if (pie_meas_type == "PQ")
            {
                //if (lines[obj_number, lines_PROP_P] != "")
                {
                    value_1d = -Smart_Meter_Pie_val1;
                    value_1 = (int)value_1d;
                };
                //if (lines[obj_number, lines_PROP_Q] != "")
                {
                    value_2d = -Smart_Meter_Pie_val2;
                    value_2 = (int)value_2d;
                };
            }
            if (pie_meas_type == "3U")
            {
                //if (loads[obj_number, loads_PROP_U1] != "")
                {
                    value_1d = Smart_Meter_Pie_val1;
                    value_1 = (int)value_1d;//.Parse(lines[16, lines_PROP_P]);
                };
                //if (loads[obj_number, loads_PROP_U2] != "")
                {
                    value_2d = Smart_Meter_Pie_val2;
                    value_2 = (int)value_2d;
                };
                //if (loads[obj_number, loads_PROP_U3] != "")
                {
                    value_3d = Smart_Meter_Pie_val3;
                    value_3 = (int)value_2d;
                };
            }

            if (value_1 > value_max) value_1 = value_max;
            if (value_1 < value_min) value_1 = value_min;
            if (value_2 > value_max) value_2 = value_max;
            if (value_2 < value_min) value_2 = value_min;
            value_center = (int)(1.0 * (value_max + value_min) / 2);

            int x0 = -X0_shift + Smart_Meter_Pie_x0; //1220;
            int y0 = -Y0_shift + Smart_Meter_Pie_y0; // 400;
            int diam_max = 120;
            int diam_max_y = 120;
            int text_type = 1;

            if (pie_meas_type == "PQ")
            {
                e.Graphics.FillPie(b13LightYellow, x0, y0, diam_max, diam_max_y, -180, 180);
                e.Graphics.FillPie(b2Blue, x0, y0,
                    diam_max, diam_max_y, -90 - (int)(1.0 * 90 * value_1 / value_max), (int)(1.0 * 90 * value_1 / value_max)); // measurement 1
                e.Graphics.FillPie(b7LightGreen, x0 + 8, y0 + 8,
                    diam_max_y - 16, diam_max - 16, -90 - (int)(1.0 * 90 * value_2 / value_max), (int)(1.0 * 90 * value_2 / value_max));  // measurement 2
                e.Graphics.FillPie(b1White, x0 + 20, y0 + 20, diam_max - 40, diam_max - 40, -180, 180);
                e.Graphics.DrawArc(p1Black, x0, y0, diam_max, diam_max, -180, 180);
                e.Graphics.DrawArc(p1Black, x0 + 20, y0 + 20, diam_max - 40, diam_max - 40, -180, 180);
                e.Graphics.DrawLine(p1Black, x0 + diam_max / 2, y0, x0 + diam_max / 2, y0 + 20);
                e.Graphics.DrawLine(p1Black, x0, y0 + diam_max / 2, x0 + 20, y0 + diam_max / 2);
                e.Graphics.DrawLine(p1Black, x0 + diam_max, y0 + diam_max / 2, x0 + diam_max - 20, y0 + diam_max / 2);
                if (text_type == 1)
                {
                    g.DrawString("P=" + value_1d.ToString("#0.00") + "k", Font1, b5DarkBlue, x0 - 2, y0 + diam_max / 2 + 2); // 
                    g.DrawString("Q=" + value_2d.ToString("#0.00") + "k", Font1, b7Green, x0 + diam_max / 2, y0 + diam_max / 2 + 2);
                }
                if (text_type == 2)
                {
                    g.DrawString("P=" + value_1d.ToString("#0.00"), Font3, b5DarkBlue, x0 + 10, y0 + diam_max / 2); // 
                    g.DrawString("Q=" + value_2d.ToString("#0.00"), Font3, b7Green, x0 + 10, y0 + diam_max / 2 + 12);
                }
                g.DrawString("-" + value_max.ToString("###0"), Font1, b5DarkBlue, x0 + 23, y0 + diam_max / 2 - 12);
                g.DrawString(value_max.ToString("###0"), Font1, b5DarkBlue, x0 + diam_max - 50, y0 + diam_max / 2 - 12);
                g.DrawString("0", Font1, b5DarkBlue, x0 + diam_max / 2 - 5, y0 + 20);
                //g.DrawString("Ln=" + lines[obj_number, lines_PROP_name], Font1, b5DarkBlue, x0 + diam_max / 2 - 30, y0 - 15);
            }

            if (pie_meas_type == "3U")
            {
                SolidBrush sb1 = new SolidBrush(Color.Azure), sb2 = new SolidBrush(Color.Aquamarine), sb3 = new SolidBrush(Color.Aqua);
                e.Graphics.FillPie(sb3, x0, y0, diam_max, diam_max_y, -180, 180); // b4LightBlue, b13LightYellow, b9Lime, b3LightGray
                                                                                  //value_1 = 207;
                                                                                  //value_2 = 230;
                                                                                  //value_3 = 253;
                                                                                  //e.Graphics.FillRectangle(sb, x0-20, y0, x0,y0+10);
                                                                                  //e.Graphics.FillRectangle(sb, x0 - 20, y0+10, x0, y0 + 20);
                e.Graphics.FillPie(b6Red, x0, y0, diam_max, diam_max_y, -180,
                    (int)(2.0 * 90 * (value_1 - value_min) / (value_max - value_min))); // measurement 1
                e.Graphics.FillPie(b13Yellow, x0 + 6, y0 + 6, diam_max - 12, diam_max_y - 12, -180,
                    (int)(2.0 * 90 * (value_2 - value_min) / (value_max - value_min))); // measurement 1
                e.Graphics.FillPie(b2Blue, x0 + 12, y0 + 12, diam_max - 24, diam_max_y - 24, -180,
                    (int)(2.0 * 90 * (value_3 - value_min) / (value_max - value_min))); // measurement 1
                e.Graphics.FillPie(b1White, x0 + 18, y0 + 18, diam_max - 36, diam_max - 36, -180, 180);
                e.Graphics.DrawArc(p1Black, x0, y0, diam_max, diam_max, -180, 180);
                e.Graphics.DrawArc(p1Black, x0 + 18, y0 + 18, diam_max - 36, diam_max - 36, -180, 180);
                e.Graphics.DrawLine(p1Black, x0 + diam_max / 2, y0, x0 + diam_max / 2, y0 + 20);
                e.Graphics.DrawLine(p1Black, x0, y0 + diam_max / 2, x0 + 20, y0 + diam_max / 2);
                e.Graphics.DrawLine(p1Black, x0 + diam_max, y0 + diam_max / 2, x0 + diam_max - 20, y0 + diam_max / 2);

                g.DrawString("U1=" + value_1d.ToString("#0.00"), Font1, b6Red, x0 - 2, y0 + diam_max / 2 + 2); // 
                g.DrawString("U2=" + value_2d.ToString("#0.00"), Font1, b11Orange, x0 + diam_max / 2, y0 + diam_max / 2 + 2);
                g.DrawString("U3=" + value_3d.ToString("#0.00"), Font1, b5DarkBlue, x0 + diam_max / 3, y0 + diam_max / 2 + 12);

                g.DrawString("" + value_min.ToString("###0"), Font1, b5DarkBlue, x0 + 23, y0 + diam_max / 2 - 12);
                g.DrawString(value_max.ToString("###0"), Font1, b5DarkBlue, x0 + diam_max - 50, y0 + diam_max / 2 - 12);
                //s1 = 
                g.DrawString(value_center.ToString(), Font1, b5DarkBlue, x0 + diam_max / 2 - 12, y0 + 20);
                //g.DrawString("Ld=" + loads[obj_number, loads_PROP_name], Font1, b5DarkBlue, x0 + diam_max / 2 - 30, y0 - 15);
            }
        }

        private void Paint_Smart_Meter(object sender, PaintEventArgs e)
        {
            string s1 = "";
            string Associated_obj = "";
            string Associated_obj_number = "";

            int smart_meter_dx = 260, smart_meter_dy = 200;

            // Example of received JSON
            //{"1-1-1-8-0-255":{"-2":"4001708.0","-5":"02/10/2019 12:43:54"},"1-1-131-7-0-255":{"-2":"184","-5":"02/10/2019 12:43:54","-9":"7.32"},"1-1-14-7-0-255":{"-2":"50.0","-5":"02/10/2019 12:43:54"},"1-1-151-7-0-255":{"-2":"72","-5":"02/10/2019 12:43:54","-9":"2.88"},"1-1-16-7-0-255":{"-2":"1664","-5":"02/10/2019 12:43:54","-9":"66.52"},"1-1-171-7-0-255":{"-2":"18","-5":"02/10/2019 12:43:54","-9":"0.72"},"1-1-191-7-0-255":{"-2":"93","-5":"02/10/2019 12:43:54","-9":"3.72"},"1-1-2-8-0-255":{"-2":"908509.4","-5":"02/10/2019 12:43:54"},"1-1-3-8-0-255":{"-2":"1460923.5","-5":"02/10/2019 12:43:54"},"1-1-31-7-0-255":{"-2":"1.8800000000000001","-5":"02/10/2019 12:43:55","-9":"75.2"},"1-1-32-7-0-255":{"-2":"236.20000000000002","-5":"02/10/2019 12:43:54"},"1-1-33-7-0-255":{"-2":"0.98","-5":"02/10/2019 12:43:55"},"1-1-36-7-0-255":{"-2":"438","-5":"02/10/2019 12:43:54","-9":"17.52"},"1-1-4-8-0-255":{"-2":"392.6","-5":"02/10/2019 12:43:54"},"1-1-51-7-0-255":{"-2":"2.56","-5":"02/10/2019 12:43:55","-9":"102.4"},"1-1-52-7-0-255":{"-2":"235.3","-5":"02/10/2019 12:43:54"},"1-1-53-7-0-255":{"-2":"0.99","-5":"02/10/2019 12:43:55"},"1-1-56-7-0-255":{"-2":"603","-5":"02/10/2019 12:43:54","-9":"24.12"},"1-1-71-7-0-255":{"-2":"2.68","-5":"02/10/2019 12:43:55","-9":"107.2"},"1-1-72-7-0-255":{"-2":"235.10000000000002","-5":"02/10/2019 12:43:54"},"1-1-73-7-0-255":{"-2":"0.98","-5":"02/10/2019 12:43:55"},"1-1-76-7-0-255":{"-2":"622","-5":"02/10/2019 12:43:54","-9":"24.88"}}
            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);

            // Paint interracts
            for (int i1 = 0; i1 < smart_meters_no; i1++)
            {
                int default_xy = 1;
                if ((smart_meters[i1, smart_meters_PROP_x0] == "") || (smart_meters[i1, smart_meters_PROP_y0] == ""))
                {
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    default_xy = 1;
                }
                else
                {
                    object_x0 = -X0_shift + int.Parse(smart_meters[i1, smart_meters_PROP_x0]);
                    object_y0 = -Y0_shift + int.Parse(smart_meters[i1, smart_meters_PROP_y0]);
                    default_xy = 0;
                }

                Associated_obj = smart_meters[i1, smart_meters_PROP_obj];
                Associated_obj_number = smart_meters[i1, smart_meters_PROP_number];

                s1 = smart_meters[i1, smart_meters_PROP_text];

                g.FillRectangle(b1White, object_x0, object_y0, smart_meter_dx, smart_meter_dy);
                g.DrawRectangle(p1Black, object_x0, object_y0, smart_meter_dx, smart_meter_dy);
                if (smart_meters[i1, smart_meters_PROP_text] != "")
                    g.DrawString(smart_meters[i1, smart_meters_PROP_text], Font1, b0Black, object_x0, object_y0 + 5);

                try
                {
                    SMX smx = new SMX();

                    var serializer = new JsonSerializer();
                    serializer.Populate(new JsonTextReader(new StringReader(MqttClient2_str_in)), smx);
                    //s1 = serializer.ToString();
                    s1 = smx.Ap.value;

                    var jobj = (JObject)serializer.Deserialize(new JsonTextReader(new StringReader(MqttClient2_str_in)));
                    if (jobj.ContainsKey("SMX") && ((JObject)jobj["SMX"]).ContainsKey("LD01"))
                    {
                        jobj = (JObject)jobj["SMX"]["LD01"];
                    }

                    if (Associated_obj == "load")
                    {
                        //var Ap = (JObject) jobj["1-1-1-8-0-255"];
                        smart_meters[i1, smart_meters_PROP_Ap] = (String)jobj["1-1-1-8-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_Am] = (String)jobj["1-1-2-8-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_Rp] = (String)jobj["1-1-3-8-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_Rm] = (String)jobj["1-1-4-8-0-255"]["-2"];

                        smart_meters[i1, smart_meters_PROP_U1] = (String)jobj["1-1-32-7-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_U2] = (String)jobj["1-1-52-7-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_U3] = (String)jobj["1-1-72-7-0-255"]["-2"];

                        //"SMX/LD01/1-1-16-7-0-255/-2": { "decimals" : 3, "label": "P" },
                        //"SMX/LD01/1-1-16-7-0-255/-9": { "decimals" : 2, "label": "P_prm" },
                        smart_meters[i1, smart_meters_PROP_P] = (String)jobj["1-1-16-7-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_P_prm] = (String)jobj["1-1-16-7-0-255"]["-9"];
                        smart_meters[i1, smart_meters_PROP_P1] = (String)jobj["1-1-36-7-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_P2] = (String)jobj["1-1-56-7-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_P3] = (String)jobj["1-1-76-7-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_P1_prm] = (String)jobj["1-1-36-7-0-255"]["-9"];
                        smart_meters[i1, smart_meters_PROP_P2_prm] = (String)jobj["1-1-56-7-0-255"]["-9"];
                        smart_meters[i1, smart_meters_PROP_P3_prm] = (String)jobj["1-1-76-7-0-255"]["-9"];

                        smart_meters[i1, smart_meters_PROP_Q] = (String)jobj["1-1-131-7-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_Q_prm] = (String)jobj["1-1-131-7-0-255"]["-9"];
                        smart_meters[i1, smart_meters_PROP_Q1] = (String)jobj["1-1-151-7-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_Q2] = (String)jobj["1-1-171-7-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_Q3] = (String)jobj["1-1-191-7-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_Q1_prm] = (String)jobj["1-1-151-7-0-255"]["-9"];
                        smart_meters[i1, smart_meters_PROP_Q2_prm] = (String)jobj["1-1-171-7-0-255"]["-9"];
                        smart_meters[i1, smart_meters_PROP_Q3_prm] = (String)jobj["1-1-191-7-0-255"]["-9"];

                        smart_meters[i1, smart_meters_PROP_I1] = (String)jobj["1-1-31-7-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_I2] = (String)jobj["1-1-51-7-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_I3] = (String)jobj["1-1-71-7-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_K1] = (String)jobj["1-1-33-7-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_K2] = (String)jobj["1-1-53-7-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_K3] = (String)jobj["1-1-73-7-0-255"]["-2"];

                        //smart_meters[i1, smart_meters_PROP_fi_U1_U1] = (String)jobj["1-1-81-7-0-255"]["-2"];
                        //smart_meters[i1, smart_meters_PROP_fi_U2_U1] = (String)jobj["1-1-81-7-1-255"]["-2"];
                        //smart_meters[i1, smart_meters_PROP_fi_U3_U1] = (String)jobj["1-1-81-7-2-255"]["-2"];
                        //smart_meters[i1, smart_meters_PROP_fi_I1_U1] = (String)jobj["1-1-81-7-4-255"]["-2"];
                        //smart_meters[i1, smart_meters_PROP_fi_I2_U1] = (String)jobj["1-1-81-7-5-255"]["-2"];
                        //smart_meters[i1, smart_meters_PROP_fi_I3_U1] = (String)jobj["1-1-81-7-6-255"]["-2"];
                        //"SMX/LD01/1-1-14-7-0-255/-2": { "decimals" : 2, "label": "f" },
                        s1 = (String)jobj["1-1-14-7-0-255"]["-2"];
                        smart_meters[i1, smart_meters_PROP_f] = s1;
                        //"SMX/LD01/0-0-1-0-0-255/-2": "SMM_Time",
                        s1 = (String)jobj["1-1-1-8-0-255"]["-5"];
                        smart_meters[i1, smart_meters_PROP_SMM_time] = s1;
                        //"SysCpuTemp": { "decimals" : 3, "label": "SysCpuTemp" }
                        //smart_meters[i1, smart_meters_PROP_SysCpuTemp] = (String)jobj["SysCpuTemp"]["-2"];

                        Paint_Smart_Meter_Pie(sender, e);
                    }

                }
                catch
                {
                    //Debug.Print(ee.ToString());
                }
                /*
	"SMX/LD01/1-1-36-7-0-255/-9": {"decimals" : 2, "label": "P1_prm" },	"SMX/LD01/1-1-56-7-0-255/-9": {"decimals" : 2, "label": "P2_prm" },	"SMX/LD01/1-1-76-7-0-255/-9": {"decimals" : 2, "label": "P3_prm" },
	"SMX/LD01/1-1-151-7-0-255/-9": {"decimals" : 2, "label": "Q1_prm" },"SMX/LD01/1-1-171-7-0-255/-9": {"decimals" : 2, "label": "Q2_prm" },"SMX/LD01/1-1-191-7-0-255/-9": {"decimals" : 2, "label": "Q3_prm" },
	"SMX/LD01/1-1-16-7-0-255/-2": {"decimals" : 3, "label": "P" },	"SMX/LD01/1-1-16-7-0-255/-9": {"decimals" : 2, "label": "P_prm" },
	"SMX/LD01/1-1-131-7-0-255/-2": {"decimals" : 3, "label": "Q" },	"SMX/LD01/1-1-131-7-0-255/-9": {"decimals" : 2, "label": "Q_prm" },
	"SMX/LD01/1-1-14-7-0-255/-2": {"decimals" : 2, "label": "f"	},
    "SysDateTime": "SMX_SysDateTime",
	"SysCpuLoad": "SysCpuLoad",
	"SysMemoryLoad": {"decimals" : 3, "label": "SysMemoryLoad" }
 	"Module/MQTTClient/MQTTClient1/NumPayloadsPub": "NumPayloadsPub1",
	"Module/MQTTClient/MQTTClient1/NumBytesPubAfterRecon": "NumBytesPubAfterRecon1",
	"SysDateTime": "SMX_SysDateTime",
	"SysCpuLoad": "SysCpuLoad",
	"SysMemoryLoad": {"decimals" : 3, "label": "SysMemoryLoad" },
	"SysCpuTemp": {"decimals" : 3, "label": "SysCpuTemp" }
                 */
                int start_values = 80;
                /*
                // draw smart meter information
                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_f]).ToString("######0.00");
                g.DrawString("f= " + s1 + "Hz", Font1, b0Black, object_x0 + 5, object_y0 + start_values + 20);

                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_P1]).ToString("######0.00");
                g.DrawString("P1= " + s1, Font1, b0Black, object_x0 + 5, object_y0 + start_values + 30);
                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_P2]).ToString("######0.00");
                g.DrawString("P2= " + s1, Font1, b0Black, object_x0 + 80, object_y0 + start_values + 30);
                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_P3]).ToString("######0.00");
                g.DrawString("P3= " + s1, Font1, b0Black, object_x0 + 160, object_y0 + start_values + 30);

                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_Q1]).ToString("######0.00");
                g.DrawString("Q1= " + s1, Font1, b0Black, object_x0 + 5, object_y0 + start_values + 40);
                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_Q2]).ToString("######0.00");
                g.DrawString("Q2= " + s1, Font1, b0Black, object_x0 + 80, object_y0 + start_values + 40);
                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_Q3]).ToString("######0.00");
                g.DrawString("Q3= " + s1, Font1, b0Black, object_x0 + 160, object_y0 + start_values + 40);

                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_I1]).ToString("######0.00");
                g.DrawString("I1= " + s1, Font1, b0Black, object_x0 + 5, object_y0 + start_values + 50);
                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_I2]).ToString("######0.00");
                g.DrawString("I2= " + s1, Font1, b0Black, object_x0 + 80, object_y0 + start_values + 50);
                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_I3]).ToString("######0.00");
                g.DrawString("I3= " + s1, Font1, b0Black, object_x0 + 160, object_y0 + start_values + 50);

                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_K1]).ToString("######0.00");
                g.DrawString("K1= " + s1, Font1, b0Black, object_x0 + 5, object_y0 + start_values + 60);
                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_K2]).ToString("######0.00");
                g.DrawString("K2= " + s1, Font1, b0Black, object_x0 + 80, object_y0 + start_values + 60);
                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_K3]).ToString("######0.00");
                g.DrawString("K3= " + s1, Font1, b0Black, object_x0 + 160, object_y0 + start_values + 60);

                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_Ap]).ToString("######0.00");
                g.DrawString("Ap= " + s1, Font1, b0Black, object_x0 + 5, object_y0 + start_values + 80);
                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_Am]).ToString("######0.00");
                g.DrawString("Am= " + s1, Font1, b0Black, object_x0 + 120, object_y0 + start_values + 80);
                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_Rp]).ToString("######0.00");
                g.DrawString("Rp= " + s1, Font1, b0Black, object_x0 + 5, object_y0 + start_values + 90);
                s1 = double.Parse(smart_meters[i1, smart_meters_PROP_Rm]).ToString("######0.00");
                g.DrawString("Rm= " + s1, Font1, b0Black, object_x0 + 120, object_y0 + start_values + 90);

                s1 = smart_meters[i1, smart_meters_PROP_SMM_time];
                g.DrawString("SMM time = " + s1, Font1, b0Black, object_x0 + 5, object_y0 + start_values + 100);

                Smart_Meter_Pie_x0 = object_x0 +5;
                Smart_Meter_Pie_y0 = object_y0+20;
                Smart_Meter_Pie_type = "PQ";
                Smart_Meter_Pie_val1 = double.Parse(smart_meters[i1, smart_meters_PROP_P]); // P
                Smart_Meter_Pie_val2 = double.Parse(smart_meters[i1, smart_meters_PROP_Q]); // Q
                Smart_Meter_Pie_value_max = 2000;
                Smart_Meter_Pie_value_min = -2000;
                Paint_Smart_Meter_Pie(sender, e);

                Smart_Meter_Pie_x0 = object_x0 + 135;
                Smart_Meter_Pie_y0 = object_y0 + 20;
                Smart_Meter_Pie_type = "3U";
                Smart_Meter_Pie_val1 = double.Parse(smart_meters[i1, smart_meters_PROP_U1]); // 
                Smart_Meter_Pie_val2 = double.Parse(smart_meters[i1, smart_meters_PROP_U2]); // 
                Smart_Meter_Pie_val3 = double.Parse(smart_meters[i1, smart_meters_PROP_U3]); // 
                Smart_Meter_Pie_value_max = 253;
                Smart_Meter_Pie_value_min = 207;
                Paint_Smart_Meter_Pie(sender, e);
                */
            } // end painting smart meters
        }

    }
}