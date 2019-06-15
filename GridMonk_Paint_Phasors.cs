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

namespace GridMonC
{
    public partial class GridMonk : Form
    {

        const int gph_phasors_number_MAX = 36;
        int gph_phasors_legend_dx = 70;
        public struct Phasor
        {
            public string[] ch; // each 
            public string[] obj;
            public string[] name;
            public string[] measurement;
            public string[] bus;
            public string[] prev;
            public string[] max;
            public string[] color;
            public string[] arrow;
            public string[] meas_type; // phasor measurement type. Can be U, I, F (if it is force as vector)
            public string[] text; // custom text associated to the phasor
            public string[] draw_label; // If=1, the phasor will be dispayed with its name and all indemtificators
            public string[] draw_label_text; // For the draw_lable, instead od default indetifiatcor, user can define a user-text
        }

        Phasor[] gph_phasors_properties = new Phasor[graph_phasors_MAX];
        double Angle_real_time = 0;

        private void gph_phasors_alloc()
        {
            for (int p1 = 0; p1 < graph_phasors_MAX; p1++)
            {
                gph_phasors_properties[p1].ch = new string[gph_phasors_number_MAX];
                gph_phasors_properties[p1].obj = new string[gph_phasors_number_MAX];
                gph_phasors_properties[p1].name = new string[gph_phasors_number_MAX];
                gph_phasors_properties[p1].measurement = new string[gph_phasors_number_MAX];
                gph_phasors_properties[p1].bus = new string[gph_phasors_number_MAX];

                gph_phasors_properties[p1].prev = new string[gph_phasors_number_MAX];
                gph_phasors_properties[p1].max = new string[gph_phasors_number_MAX];
                gph_phasors_properties[p1].color = new string[gph_phasors_number_MAX];

                gph_phasors_properties[p1].arrow = new string[gph_phasors_number_MAX];
                gph_phasors_properties[p1].meas_type = new string[gph_phasors_number_MAX];
                gph_phasors_properties[p1].text = new string[gph_phasors_number_MAX];
                gph_phasors_properties[p1].draw_label = new string[gph_phasors_number_MAX];
                gph_phasors_properties[p1].draw_label_text = new string[gph_phasors_number_MAX];
            }
        }

        private void gph_phasors_ini()
        {
            for (int p1 = 0; p1 < graph_phasors_MAX; p1++)
            {
                for (int j = 0; j < gph_phasors_number_MAX; j++) gph_phasors_properties[p1].ch[j] = "";
                for (int j = 0; j < gph_phasors_number_MAX; j++) gph_phasors_properties[p1].obj[j] = "";
                for (int j = 0; j < gph_phasors_number_MAX; j++) gph_phasors_properties[p1].name[j] = "";
                for (int j = 0; j < gph_phasors_number_MAX; j++) gph_phasors_properties[p1].measurement[j] = "";
                for (int j = 0; j < gph_phasors_number_MAX; j++) gph_phasors_properties[p1].bus[j] = "";

                for (int j = 0; j < gph_phasors_number_MAX; j++) gph_phasors_properties[p1].prev[j] = "";
                for (int j = 0; j < gph_phasors_number_MAX; j++) gph_phasors_properties[p1].max[j] = "";
                for (int j = 0; j < gph_phasors_number_MAX; j++) gph_phasors_properties[p1].color[j] = "";

                for (int j = 0; j < gph_phasors_number_MAX; j++) gph_phasors_properties[p1].arrow[j] = "";
                for (int j = 0; j < gph_phasors_number_MAX; j++) gph_phasors_properties[p1].meas_type[j] = "";
                for (int j = 0; j < gph_phasors_number_MAX; j++) gph_phasors_properties[p1].text[j] = "";
                for (int j = 0; j < gph_phasors_number_MAX; j++) gph_phasors_properties[p1].draw_label[j] = "";
                for (int j = 0; j < gph_phasors_number_MAX; j++) gph_phasors_properties[p1].draw_label_text[j] = "";
            }
        }

        private void Paint_phasors(object sender, PaintEventArgs e, int gph_object_no) {

            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);

            string s1 = "", s2 = "";
            double magnif_real = 1;
            if (graph_phasors[gph_object_no, graph_phasors_PROP_legend_dx] != "")
            {
                gph_phasors_legend_dx = int.Parse(graph_phasors[gph_object_no, graph_phasors_PROP_legend_dx]);
            }
            if (graph_phasors[gph_object_no, graph_phasors_PROP_magnif] != "")
            {
                magnif_real = double.Parse(graph_phasors[gph_object_no, graph_phasors_PROP_magnif]);
            }

            int radius = 77; // raza pentru Vnom + 10%
            int radius_magnified = (int)(1.0 * radius * magnif_real); // raza pentru Vnom + 10%, afectata de magnificare: radius_magnified = radius * magnif_real;
            int radius_n = 70; // raza pentru valoarea nominala
            int radius_n_magnified = (int)(1.0 * radius_n * magnif_real);
            int radius_low = 63; // raza pentru Vnom - 10%
            int radius_low_magnified = (int)(1.0 * radius_low * magnif_real);
            int radius_low2 = 56; // raza pentru Vnom - 20% =  80% Vnom
            int radius_low2_magnified = (int)(1.0 * radius_low2 * magnif_real);
            int radius_currents = 40; // raza pentru marimea 2 (curenti), cu val. maxima = 60% din celelalte valori
            int radius_currents_magnified = (int)(1.0 * radius_currents * magnif_real);
            int object_no = -1, prop_module = -1, prop_angle = -1, sign_I = 1;
            int type_UI =0; // 0 = U, 1 = I

            int x0 = -X0_shift + object_x0 + radius_magnified;
            int y0 = -Y0_shift + object_y0 + radius_magnified;

            if (graph_phasors[gph_object_no, graph_phasors_PROP_enlarge] == "1")
            {
                if (graph_phasors[gph_object_no, graph_phasors_PROP_transparency] == "1")
                    g.FillRectangle(b1White, -X0_shift + object_x0, -Y0_shift + object_y0, radius_magnified * 2 + gph_phasors_legend_dx, radius_magnified * 2);
                //g.DrawString("Phasors:", Font1, b2Blue, object_x0 + 1, object_y0 + 10 * 0);
                g.DrawString("ReducE", Font3, b0Black, -X0_shift + object_x0 + 55, -Y0_shift + object_y0 + 10 * 0);
            }
            if ((graph_phasors[gph_object_no, graph_phasors_PROP_enlarge] == "0")|| (graph_phasors[gph_object_no, graph_phasors_PROP_enlarge] == ""))
            { 
                if(graph_phasors[gph_object_no, graph_phasors_PROP_transparency] == "1")
                    g.FillRectangle(b1White, -X0_shift + object_x0, -Y0_shift + object_y0, 110, 20);
                //g.DrawString(graph_phasors[gph_object_no, graph_phasors_PROP_name], Font1, b2Blue, object_x0 + 1, object_y0 + 10 * 0);
                g.DrawString("EnlargeE", Font3, b0Black, -X0_shift + object_x0 + 55, -Y0_shift + object_y0 + 10 * 0);
                g.DrawString(graph_phasors[gph_object_no, graph_phasors_PROP_gph_text], Font3, b2Blue, -X0_shift + object_x0 + 1, -Y0_shift + object_y0 + 10 * 0);
                return;
            }
            g.DrawString(graph_phasors[gph_object_no, graph_phasors_PROP_gph_text], Font3, b2Blue, -X0_shift + object_x0 + 1, -Y0_shift + object_y0 + 10 * 0);

            g.DrawEllipse(p2LightGray, gph_phasors_legend_dx + x0 - radius_magnified, y0 - radius_magnified, radius_magnified * 2, radius_magnified * 2); // Max circle
            g.DrawEllipse(p3DarkGray, gph_phasors_legend_dx + x0 - radius_n_magnified, y0 - radius_n_magnified, radius_n_magnified * 2, radius_n_magnified * 2); // Max circle
            g.DrawEllipse(p2LightGray, gph_phasors_legend_dx + x0 - radius_low_magnified, y0 - radius_low_magnified, radius_low_magnified * 2, radius_low_magnified * 2); // Max circle
            g.DrawEllipse(p2LightGray, gph_phasors_legend_dx + x0 - radius_low2_magnified, y0 - radius_low2_magnified, radius_low2_magnified * 2, radius_low2_magnified * 2); // Max circle
            g.DrawEllipse(p5DarkBlue, gph_phasors_legend_dx + x0 - 2, y0 - 2, 4, 4); // Circle center

            g.DrawEllipse(p2LightGray, gph_phasors_legend_dx + x0 - radius_currents_magnified, y0 - radius_currents_magnified, radius_currents_magnified * 2, radius_currents_magnified * 2); // Max circle

            g.DrawLine(p2LightGray, gph_phasors_legend_dx + x0, y0 - radius_magnified, gph_phasors_legend_dx + x0, y0 + radius_magnified);
            g.DrawLine(p2LightGray, gph_phasors_legend_dx + x0 - radius_magnified, y0, gph_phasors_legend_dx + x0 + radius_magnified, y0);

            string[] Phasor_name = new string[gph_phasors_number_MAX];
            double[] Phasor_Angles = new double[gph_phasors_number_MAX];
            double[] Phasor_Module = new double[gph_phasors_number_MAX];
            double[] Phasor_Module_val_real = new double[gph_phasors_number_MAX];
            double[] Phasor_Module_val_real_angle = new double[gph_phasors_number_MAX];

            double Unom = 230000; // V
            double Inom = 5000; // A
            int[] Phasor_prev = new int[gph_phasors_number_MAX];
            for (int i1 = 0; i1 < gph_phasors_number_MAX; i1++) Phasor_prev[i1] = -1;

            for (int j = 0; j < gph_phasors_number_MAX; j++)
            {
                if (gph_phasors_properties[gph_object_no].prev[j] != "")
                    Phasor_prev[j] = int.Parse(gph_phasors_properties[gph_object_no].prev[j]);
                if (gph_phasors_properties[gph_object_no].obj[j] == "line") // we have a line
                    if (gph_phasors_properties[gph_object_no].name[j] != "")
                        for (int l1 = 0; l1 < lines_no; l1++)
                            if (gph_phasors_properties[gph_object_no].name[j] == lines[l1, lines_PROP_name].ToLower()) object_no = l1;
                if (gph_phasors_properties[gph_object_no].obj[j] == "load") // we have a load
                    if (gph_phasors_properties[gph_object_no].name[j] != "")
                        for (int l1 = 0; l1 < loads_no; l1++)
                            if (gph_phasors_properties[gph_object_no].name[j] == loads[l1, loads_PROP_name].ToLower()) object_no = l1;

                if ((gph_phasors_properties[gph_object_no].measurement[j] != "") && (gph_phasors_properties[gph_object_no].bus[j] != ""))
                {
                    if (gph_phasors_properties[gph_object_no].measurement[j] == "u1")
                    {
                        type_UI = 0;
                        if (gph_phasors_properties[gph_object_no].bus[j] == "1") {
                            if (gph_phasors_properties[gph_object_no].obj[j] == "line") { prop_module = lines_PROP_U1; prop_angle = lines_PROP_U1fi; }
                            if (gph_phasors_properties[gph_object_no].obj[j] == "load") { prop_module = loads_PROP_U1; prop_angle = loads_PROP_U1fi; }
                        }
                        if (gph_phasors_properties[gph_object_no].bus[j] == "2") { prop_module = lines_PROP_U1_t2; prop_angle = lines_PROP_U1fi_t2; }
                    }
                    if (gph_phasors_properties[gph_object_no].measurement[j] == "u2")
                    {
                        type_UI = 0;
                        if (gph_phasors_properties[gph_object_no].bus[j] == "1")
                        {
                            if (gph_phasors_properties[gph_object_no].obj[j] == "line") { prop_module = lines_PROP_U2; prop_angle = lines_PROP_U2fi; }
                            if (gph_phasors_properties[gph_object_no].obj[j] == "load") { prop_module = loads_PROP_U2; prop_angle = loads_PROP_U2fi; }
                        }
                        //if (gph_phasors_properties[gph_object_no].bus[j] == "1")  { prop_module = lines_PROP_U2; prop_angle = lines_PROP_U2fi; }
                        if (gph_phasors_properties[gph_object_no].bus[j] == "2")  { prop_module = lines_PROP_U2_t2; prop_angle = lines_PROP_U2fi_t2; }
                    }
                    if (gph_phasors_properties[gph_object_no].measurement[j] == "u3")
                    {
                        type_UI = 0;
                        if (gph_phasors_properties[gph_object_no].bus[j] == "1")
                        {
                            if (gph_phasors_properties[gph_object_no].obj[j] == "line") { prop_module = lines_PROP_U3; prop_angle = lines_PROP_U3fi; }
                            if (gph_phasors_properties[gph_object_no].obj[j] == "load") { prop_module = loads_PROP_U3; prop_angle = loads_PROP_U3fi; }
                        }
                        //if (gph_phasors_properties[gph_object_no].bus[j] == "1") { prop_module = lines_PROP_U3; prop_angle = lines_PROP_U3fi; }
                        if (gph_phasors_properties[gph_object_no].bus[j] == "2") { prop_module = lines_PROP_U3_t2; prop_angle = lines_PROP_U3fi_t2; }
                    }
                    if (gph_phasors_properties[gph_object_no].measurement[j] == "i1")
                    {
                        type_UI = 1; // tip curent
                        if (gph_phasors_properties[gph_object_no].bus[j] == "1")
                        {
                            sign_I = 1;
                            if (gph_phasors_properties[gph_object_no].obj[j] == "line") { prop_module = lines_PROP_I1; prop_angle = lines_PROP_I1fi; }
                            if (gph_phasors_properties[gph_object_no].obj[j] == "load") { prop_module = loads_PROP_I1; prop_angle = loads_PROP_I1fi; }
                        }
                        //if (gph_phasors_properties[gph_object_no].bus[j] == "1") { prop_module = lines_PROP_I1; prop_angle = lines_PROP_I1fi; }
                        if (gph_phasors_properties[gph_object_no].bus[j] == "2") { sign_I = -1; prop_module = lines_PROP_I1_t2; prop_angle = lines_PROP_I1fi_t2; }
                    }
                    if (gph_phasors_properties[gph_object_no].measurement[j] == "i2")
                    {
                        type_UI = 1; // tip curent
                        if (gph_phasors_properties[gph_object_no].bus[j] == "1")
                        {
                            sign_I = 1;
                            if (gph_phasors_properties[gph_object_no].obj[j] == "line") { prop_module = lines_PROP_I2; prop_angle = lines_PROP_I2fi; }
                            if (gph_phasors_properties[gph_object_no].obj[j] == "load") { prop_module = loads_PROP_I2; prop_angle = loads_PROP_I2fi; }
                        }
                        //if (gph_phasors_properties[gph_object_no].bus[j] == "1") { prop_module = lines_PROP_I2; prop_angle = lines_PROP_I2fi; }
                        if (gph_phasors_properties[gph_object_no].bus[j] == "2") { sign_I = -1; prop_module = lines_PROP_I2_t2; prop_angle = lines_PROP_I2fi_t2; }
                    }
                    if (gph_phasors_properties[gph_object_no].measurement[j] == "i3")
                    {
                        type_UI = 1; // tip curent
                        if (gph_phasors_properties[gph_object_no].bus[j] == "1")
                        {
                            sign_I = 1;
                            if (gph_phasors_properties[gph_object_no].obj[j] == "line") { prop_module = lines_PROP_I3; prop_angle = lines_PROP_I3fi; }
                            if (gph_phasors_properties[gph_object_no].obj[j] == "load") { prop_module = loads_PROP_I3; prop_angle = loads_PROP_I3fi; }
                        }
                        //if (gph_phasors_properties[gph_object_no].bus[j] == "1") { prop_module = lines_PROP_I3; prop_angle = lines_PROP_I3fi; }
                        if (gph_phasors_properties[gph_object_no].bus[j] == "2") { sign_I = -1; prop_module = lines_PROP_I3_t2; prop_angle = lines_PROP_I3fi_t2; }
                    }

                    if ((prop_angle != -1) && (object_no != -1))
                    {
                        if (gph_phasors_properties[gph_object_no].obj[j] == "line") if (lines[object_no, prop_angle] != "")
                            {
                                if (type_UI == 0) // Fazor tip U
                                    Phasor_Angles[j] = double.Parse(lines[object_no, prop_angle]) + 90; 
                                if(type_UI==1) // Fazor tip I
                                {
                                    if (gph_phasors_properties[gph_object_no].bus[j] == "1") // ???? De verificat daca e bine
                                        Phasor_Angles[j] = (180 - double.Parse(lines[object_no, prop_angle])) + 90;
                                    if (gph_phasors_properties[gph_object_no].bus[j] == "2")
                                        Phasor_Angles[j] = - (180 - double.Parse(lines[object_no, prop_angle])) + 90;
                                }
                            }
                        if (gph_phasors_properties[gph_object_no].obj[j] == "load") if (loads[object_no, prop_angle] != "")
                                Phasor_Angles[j] = double.Parse(loads[object_no, prop_angle]) + 90;
                    }
                    if ((prop_module != -1) && (object_no != -1))
                    {
                        double radius1=100;
                        if (type_UI == 0) radius1 = radius_n;
                        if (type_UI == 1) radius1 = radius_currents;
                        if (gph_phasors_properties[gph_object_no].obj[j] == "line") if (lines[object_no, prop_module] != "")
                        { 
                            Phasor_Module[j] = double.Parse(lines[object_no, prop_module]) / double.Parse(gph_phasors_properties[gph_object_no].max[j]) * radius1;
                            Phasor_Module_val_real[j] = double.Parse(lines[object_no, prop_module]);
                        }
                        if (gph_phasors_properties[gph_object_no].obj[j] == "load") if (loads[object_no, prop_module] != "")
                        { 
                            Phasor_Module[j] = double.Parse(loads[object_no, prop_module]) / double.Parse(gph_phasors_properties[gph_object_no].max[j]) * radius1;
                            Phasor_Module_val_real[j] = double.Parse(loads[object_no, prop_module]);
                        }
                    }
                }
            }
            int channel;
            int x1, y1;
            int dx0, dy0;
            int[] x1_prev = new int[gph_phasors_number_MAX]; 
            int[] y1_prev = new int[gph_phasors_number_MAX];
            for (int i1 = 0; i1 < 3; i1++)
            {
                x1_prev[i1] = 0;
                y1_prev[i1] = 0;
            }

            int module = 0;
            Pen[] px = new Pen[6];
            px[0] = new Pen(Color.Black, 1);
            px[1] = new Pen(Color.Blue, 1);
            px[2] = new Pen(Color.Brown, 1);
            px[3] = new Pen(Color.Red, 3);
            px[4] = new Pen(Color.Green, 3);
            px[5] = new Pen(Color.LightBlue, 3);
            SolidBrush[] bx = new SolidBrush[gph_phasors_number_MAX];
            bx[0] = new SolidBrush(Color.Black);
            bx[1] = new SolidBrush(Color.Blue);
            bx[2] = new SolidBrush(Color.Brown);
            bx[3] = new SolidBrush(Color.Red);
            bx[4] = new SolidBrush(Color.Green);
            bx[5] = new SolidBrush(Color.LightBlue);

            Pen pcrt = new Pen(Color.Black, 1);
            SolidBrush bcrt = new SolidBrush(Color.Black);

            for (int ph = 0; ph < 18; ph++)
            {
                module = ph / 3;
                pcrt = px[module];
                x1 = (int)(Phasor_Module[ph] * magnif_real * Math.Cos(-(Phasor_Angles[ph]+ Angle_real_time) / 180 * Math.PI));
                y1 = (int)(Phasor_Module[ph] * magnif_real * Math.Sin(-(Phasor_Angles[ph] + Angle_real_time) / 180 * Math.PI));
                if (Phasor_prev[ph] != -1)
                {
                    dx0 = x1_prev[Phasor_prev[ph]];
                    dy0 = y1_prev[Phasor_prev[ph]];
                }
                else
                {
                    dx0 = 0;
                    dy0 = 0;
                }

                // display legend
                int legend_dispay_number = gph_phasors_number_MAX;
                int legend_dispay_set = 1;
                if (magnif_real < 1.4) legend_dispay_number = 12;
                if (magnif_real >= 1.4) legend_dispay_number = 18;
                if (magnif_real >= 3) legend_dispay_number = gph_phasors_number_MAX;
                // Display legend with phashors sources
                //g.DrawString("Phasors:", Font1, b2Blue, object_x0 + 1, object_y0 + 10 * 0);
                g.DrawString("  SET 1 ", Font1, b2Blue, -X0_shift + object_x0 + 1, -Y0_shift + object_y0 + 10 * 1 +5);
                for (int j = 0; j < legend_dispay_number; j++)
                {
                    module = j / 3;
                    bcrt = bx[module];
                    s1 = gph_phasors_properties[gph_object_no].name[j]
                        + "." + gph_phasors_properties[gph_object_no].measurement[j] + "." + gph_phasors_properties[gph_object_no].bus[j];
                    g.DrawString(s1, Font1, bcrt, -X0_shift + object_x0 + 1, -Y0_shift + object_y0 + 10 * (j + 2) + 5);
                }

                if (gph_phasors_properties[gph_object_no].draw_label[ph] == "1")
                {
                    s2 = "";
                    if (gph_phasors_properties[gph_object_no].meas_type[ph] == "u") s2 = "V";
                    if (gph_phasors_properties[gph_object_no].meas_type[ph] == "i") s2 = "A";
                    if (gph_phasors_properties[gph_object_no].meas_type[ph] == "fi") s2 = "⁰";
                    g.DrawEllipse(pcrt, gph_phasors_legend_dx + x0 + dx0 + x1-2, y0 + dy0 + y1-2, 4, 4);
                    if (gph_phasors_properties[gph_object_no].draw_label_text[ph] == "") { 
                        s1 = gph_phasors_properties[gph_object_no].name[ph] + "." + gph_phasors_properties[gph_object_no].measurement[ph] + "." +
                            gph_phasors_properties[gph_object_no].bus[ph] + "=" + Phasor_Module_val_real[ph] + s2;
                        s2 = Phasor_Module_val_real[ph] + s2;
                    }
                    else s1 = gph_phasors_properties[gph_object_no].draw_label_text[ph];
                    g.DrawString(s1, Font1, b2Blue, gph_phasors_legend_dx + x0 + dx0 + x1 + 1, y0 + dy0 + y1 + 1);
                }
                //if()
                g.DrawLine(pcrt, gph_phasors_legend_dx + x0 + dx0, y0 + dy0, gph_phasors_legend_dx + x0 +dx0 + x1, y0 +dy0 + y1);
                // se memoreaza punctul final al fazouluim care poaet fi de plecare pentru un alt fazor
                x1_prev[ph] = dx0 + x1;
                y1_prev[ph] = dy0 + y1;
            }
        }
    }
}