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

        private void Paint_PMU(object sender, PaintEventArgs e)
        {
            string s1 = "";
            string Associated_obj = "";
            string Associated_obj_number = "";

            //int smart_meter_dx = 260, smart_meter_dy = 200;

            // Example of received JSON
            //{"1-1-1-8-0-255":{"-2":"4001708.0","-5":"02/10/2019 12:43:54"},"1-1-131-7-0-255":{"-2":"184","-5":"02/10/2019 12:43:54","-9":"7.32"},"1-1-14-7-0-255":{"-2":"50.0","-5":"02/10/2019 12:43:54"},"1-1-151-7-0-255":{"-2":"72","-5":"02/10/2019 12:43:54","-9":"2.88"},"1-1-16-7-0-255":{"-2":"1664","-5":"02/10/2019 12:43:54","-9":"66.52"},"1-1-171-7-0-255":{"-2":"18","-5":"02/10/2019 12:43:54","-9":"0.72"},"1-1-191-7-0-255":{"-2":"93","-5":"02/10/2019 12:43:54","-9":"3.72"},"1-1-2-8-0-255":{"-2":"908509.4","-5":"02/10/2019 12:43:54"},"1-1-3-8-0-255":{"-2":"1460923.5","-5":"02/10/2019 12:43:54"},"1-1-31-7-0-255":{"-2":"1.8800000000000001","-5":"02/10/2019 12:43:55","-9":"75.2"},"1-1-32-7-0-255":{"-2":"236.20000000000002","-5":"02/10/2019 12:43:54"},"1-1-33-7-0-255":{"-2":"0.98","-5":"02/10/2019 12:43:55"},"1-1-36-7-0-255":{"-2":"438","-5":"02/10/2019 12:43:54","-9":"17.52"},"1-1-4-8-0-255":{"-2":"392.6","-5":"02/10/2019 12:43:54"},"1-1-51-7-0-255":{"-2":"2.56","-5":"02/10/2019 12:43:55","-9":"102.4"},"1-1-52-7-0-255":{"-2":"235.3","-5":"02/10/2019 12:43:54"},"1-1-53-7-0-255":{"-2":"0.99","-5":"02/10/2019 12:43:55"},"1-1-56-7-0-255":{"-2":"603","-5":"02/10/2019 12:43:54","-9":"24.12"},"1-1-71-7-0-255":{"-2":"2.68","-5":"02/10/2019 12:43:55","-9":"107.2"},"1-1-72-7-0-255":{"-2":"235.10000000000002","-5":"02/10/2019 12:43:54"},"1-1-73-7-0-255":{"-2":"0.98","-5":"02/10/2019 12:43:55"},"1-1-76-7-0-255":{"-2":"622","-5":"02/10/2019 12:43:54","-9":"24.88"}}
            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);

            // Paint interracts
            for (int i1 = 0; i1 < PMUs_no; i1++)
            {
                //int default_xy = 1;
                if ((PMUs[i1, PMUs_PROP_x0] == "") || (PMUs[i1, PMUs_PROP_y0] == ""))
                {
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    //default_xy = 1;
                }
                else
                {
                    object_x0 = -X0_shift + int.Parse(PMUs[i1, PMUs_PROP_x0]);
                    object_y0 = -Y0_shift + int.Parse(PMUs[i1, PMUs_PROP_y0]);
                    //default_xy = 0;
                }

                Associated_obj = PMUs[i1, PMUs_PROP_obj];
                Associated_obj_number = PMUs[i1, PMUs_PROP_number];

                s1 = PMUs[i1, PMUs_PROP_text];

                //g.FillRectangle(b1White, object_x0, object_y0, smart_meter_dx, smart_meter_dy);
                //g.DrawRectangle(p1Black, object_x0, object_y0, smart_meter_dx, smart_meter_dy);
                //if (PMUs[i1, PMUs_PROP_text] != "")
                //    g.DrawString(PMUs[i1, PMUs_PROP_text], Font1, b0Black, object_x0, object_y0 + 5);

                try
                {
                    //SMX smx = new SMX();
                    //var serializer = new JsonSerializer();
                    //serializer.Populate(new JsonTextReader(new StringReader(MqttClient2_str_in)), smx);
                    //s1 = smx.Ap.value;
                    //var jobj = (JObject)serializer.Deserialize(new JsonTextReader(new StringReader(MqttClient2_str_in)));
                    //if (jobj.ContainsKey("SMX") && ((JObject)jobj["SMX"]).ContainsKey("LD01"))
                    //{ jobj = (JObject)jobj["SMX"]["LD01"]; }
                    //if (Associated_obj == "load") {   }

                    int number = int.Parse(PMUs[i1, PMUs_PROP_number]);
                    PMUs[i1, PMUs_PROP_U1] = loads[number, loads_PROP_U1];
                    PMUs[i1, PMUs_PROP_U1fi] = loads[number, loads_PROP_U1fi];
                    PMUs[i1, PMUs_PROP_U2] = loads[number, loads_PROP_U2];
                    PMUs[i1, PMUs_PROP_U2fi] = loads[number, loads_PROP_U2fi];
                    PMUs[i1, PMUs_PROP_U3] = loads[number, loads_PROP_U3];
                    PMUs[i1, PMUs_PROP_U3fi] = loads[number, loads_PROP_U3fi];
                }
                catch {  }
            } 
        }

    }
}