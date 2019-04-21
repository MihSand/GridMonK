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
        int expand_obj_gph = 1;
        int nr_obj_parked_ox = 8;
        int x0_unordered = 10, y0_unordered = 670;

        // Drawing area for the Electrical_scheme
        static int Electrical_scheme_zone_X0_start = 180; // era 185
        static int Electrical_scheme_zone_Y0_start = 50;
        static int Electrical_scheme_zone_delta_X0 = 1350; //1384 - 185 = 1348;
        static int Electrical_scheme_zone_delta_Y0 = 610; // 610 - 50
        // Graphical variables
        static int El_Sch_X0_shift = 0;
        int X0_shift = 0; // which is the X coordinate from where is displayed the Electrical scheme
        int Y0_shift = 0;

        // Clipping the graphics of scheme zone with this resctangle
        Point[] polyPoints_clip_scheme_zone = {
                new Point(Electrical_scheme_zone_X0_start, Electrical_scheme_zone_Y0_start),
                new Point(Electrical_scheme_zone_X0_start + Electrical_scheme_zone_delta_X0,
                    Electrical_scheme_zone_Y0_start),
                new Point(Electrical_scheme_zone_X0_start + Electrical_scheme_zone_delta_X0,
                    Electrical_scheme_zone_Y0_start + Electrical_scheme_zone_delta_Y0),
                new Point(Electrical_scheme_zone_X0_start, Electrical_scheme_zone_Y0_start + Electrical_scheme_zone_delta_Y0)};


        /****************** Desenare obiecte de tip line si alte functii ale acestui obiect ******************/
        private void lines_properties_calculation(int nr_line)
        {
            if ((lines[nr_line, lines_PROP_x0] != "") && (lines[nr_line, lines_PROP_y0] != "")) {
                // calculate pins position inside the line representation on the grid picture
                int x0 = int.Parse(lines[nr_line, lines_PROP_x0]);
                int y0 = int.Parse(lines[nr_line, lines_PROP_y0]);
                int pin1_x = 0, pin1_y = 0, pin2_x = 0, pin2_y = 0;
                if ((lines[nr_line, lines_PROP_gph_direction]=="N") && (lines[nr_line, lines_PROP_gph_DrawType] == ""))
                {
                    pin1_x = x0 + 2; pin1_y = y0 + 4;
                    lines[nr_line, lines_PROP_pin1_x] = pin1_x.ToString();
                    lines[nr_line, lines_PROP_pin1_y] = pin1_y.ToString();
                    pin2_x = x0 + object_dx - 4; pin2_y = y0 + 4;
                    lines[nr_line, lines_PROP_pin2_x] = pin2_x.ToString();
                    lines[nr_line, lines_PROP_pin2_y] = pin2_y.ToString();
                }
                if ((lines[nr_line, lines_PROP_gph_direction] == "N") && (lines[nr_line, lines_PROP_gph_DrawType] == "L2S0"))
                {
                    pin1_x = x0 + 2; pin1_y = y0 + 6;
                    lines[nr_line, lines_PROP_pin1_x] = pin1_x.ToString();
                    lines[nr_line, lines_PROP_pin1_y] = pin1_y.ToString();
                    pin2_x = x0 + object_dx*2 - 2; pin2_y = y0 + 6;
                    lines[nr_line, lines_PROP_pin2_x] = pin2_x.ToString();
                    lines[nr_line, lines_PROP_pin2_y] = pin2_y.ToString();

                }
            }
        }

        private void Paint_lines(object sender, PaintEventArgs e)
        {
            string s1 = "", s1a = "", s2 = "", s3 = "";
            double Umax, Imax;
            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);

            Pen p_crt = new Pen(Color.DarkBlue);
            //int X0_shift = 0;
            //int Y0_shift = 0;

            // Desenare obiecte de tip linie electrica
            for (int i1 = 0; i1 < lines_no; i1++)
            {
                int extend_x = 1;
                // lines_PROP_gph_DrawType = "L3S0" inseamna Latime 3x si Stil=0 (stil de desenare)
                if (lines[i1, lines_PROP_gph_DrawType] == "L3S0") extend_x = 3; // linia va fi desenata de 3 ori mai lunga pe OX
                if (lines[i1, lines_PROP_gph_DrawType] == "L2S0") extend_x = 2; // linia va fi desenata de 2 ori mai lunga pe OX
                //else extend_x = 1;

                string gph_direction = lines[i1, lines_PROP_gph_direction];

                int default_xy = 1;
                if ((lines[i1, lines_PROP_x0] == "") || (lines[i1, lines_PROP_y0] == ""))
                { // liniile nu au coordonate x0 si y0, se vor parca jos, la rand
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    // Brush brush = new SolidBrush(Color.FromArgb(alpha, red, green, blue))
                    Brush brush1 = new SolidBrush(Color.FromArgb(127, 255, 255, 255)); // b1White
                    g.FillRectangle(brush1, object_x0, object_y0, object_dx*2/3, line_dy / 3);
                    default_xy = 1;
                }
                else
                {
                    object_x0 = -X0_shift + int.Parse(lines[i1, lines_PROP_x0]);
                    object_y0 = -Y0_shift + int.Parse(lines[i1, lines_PROP_y0]);
                    Brush brush1 = new SolidBrush(Color.FromArgb(127, 255, 255, 255)); // b1White
                    if ((gph_direction == "N") && (extend_x == 1)) g.FillRectangle(brush1, object_x0, object_y0, object_dx * extend_x, line_dy);
                    if ((gph_direction == "N") && (extend_x == 2)) g.FillRectangle(brush1, object_x0, object_y0, object_dx * extend_x, line_dy - 40);
                    if ((gph_direction == "N") && (extend_x == 3)) g.FillRectangle(brush1, object_x0, object_y0, object_dx*extend_x, line_dy - 30);
                    if (gph_direction == "W") g.FillRectangle(brush1, object_x0, object_y0, object_dx * extend_x, line_dy);
                    default_xy = 0;
                }


                // draw the line symbol
                Pen p1Black4arrow = new Pen(Color.Black, 3);
                p1Black4arrow.EndCap = LineCap.ArrowAnchor;
                Pen p1Green4arrow = new Pen(Color.Green, 3);
                p1Green4arrow.EndCap = LineCap.ArrowAnchor;

                if (gph_direction == "N")
                {   // s-a implementat si grafic linie cu latime 3x
                    if (extend_x == 1)
                    { // Desenare simbol linie
                        p_crt = p5DarkBlue;
                        if ((lines[i1, lines_PROP_Imax] != "") && (lines[i1, lines_PROP_I1] != ""))
                        {
                            Imax = double.Parse(lines[i1, lines_PROP_Imax]);
                            if (Imax < double.Parse(lines[i1, lines_PROP_I1])) p_crt = p6Red2;
                        }
                        g.DrawRectangle(p_crt, object_x0 + 22 + (extend_x - 1) * 10, object_y0 + 0, object_dx * extend_x - 44 - (extend_x - 1) * 20, 11);
                        g.DrawString("1", Font1, b0Black, object_x0 + 22 + (extend_x - 1) * 10, object_y0 + 1);
                        g.DrawString("2", Font0, b0Black, object_x0 + object_dx * extend_x - 33 - (extend_x - 1) * 10, object_y0 + 1);
                        g.DrawLine(p5DarkBlue, object_x0, object_y0 + 6, object_x0 + 22 + 10 * (extend_x - 1), object_y0 + 6);
                        g.DrawLine(p5DarkBlue, object_x0 + object_dx * extend_x - 22 - 10 * (extend_x - 1), object_y0 + 6, object_x0 + object_dx * extend_x, object_y0 + 6);
                    }
                    if (extend_x == 2)
                    { // Desenare simbol linie
                        p_crt = p5DarkBlue;
                        if ((lines[i1, lines_PROP_Imax] != "")&& (lines[i1, lines_PROP_I1] != ""))
                        {
                            Imax = double.Parse(lines[i1, lines_PROP_Imax]);
                            if(Imax < double.Parse(lines[i1, lines_PROP_I1])) p_crt = p6Red2;
                        }
                        g.DrawRectangle(p_crt, object_x0 + 16, object_y0 - 5, object_dx * extend_x - 33, 19);
                        if(lines[i1, lines_PROP_HidePinsNo] == "") { 
                            g.DrawString("1", Font1, b0Black, object_x0 + 21, object_y0 - 1);
                            g.DrawString("2", Font0, b0Black, object_x0 + object_dx * extend_x - 35, object_y0 + 0);
                        }
                        g.DrawLine(p_crt, object_x0, object_y0 + 6, object_x0 + 16, object_y0 + 6);
                        g.DrawLine(p_crt, object_x0 + object_dx * extend_x - 17, object_y0 + 6, object_x0 + object_dx * extend_x, object_y0 + 6);
                    }
                    if (extend_x == 3)
                    { // Desenare simbol linie
                        p_crt = p5DarkBlue;
                        if ((lines[i1, lines_PROP_Imax] != "") && (lines[i1, lines_PROP_I1] != ""))
                        {
                            Imax = double.Parse(lines[i1, lines_PROP_Imax]);
                            if (Imax < double.Parse(lines[i1, lines_PROP_I1])) p_crt = p6Red2;
                        }
                        g.DrawRectangle(p_crt, object_x0 + 22, object_y0 - 1, object_dx * extend_x - 48, 13);
                        g.DrawString("1", Font1, b0Black, object_x0 + 21, object_y0 - 1);
                        g.DrawString("2", Font0, b0Black, object_x0 + object_dx * extend_x - 35, object_y0 + 0);
                        g.DrawLine(p_crt, object_x0, object_y0 + 6, object_x0 + 22, object_y0 + 6);
                        g.DrawLine(p_crt, object_x0 + object_dx * extend_x - 25, object_y0 + 6, object_x0 + object_dx * extend_x, object_y0 + 6);
                    }
                    g.DrawEllipse(p5DarkBlue, object_x0, object_y0 + 4, 4, 4); // Pin_Bus1
                    g.DrawEllipse(p5DarkBlue, object_x0 + object_dx * extend_x - 4, object_y0 + 4, 4, 4); // Pin_Bus2
                    if (lines[i1, lines_PROP_brk1] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                    g.FillRectangle(SolidBrush_crt, object_x0 + 6, object_y0 + 2, 8, 8); // Brk1
                    if (lines[i1, lines_PROP_brk2] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                    g.FillRectangle(SolidBrush_crt, object_x0 + object_dx*extend_x - 14, object_y0 + 2, 8, 8); // Brk2
                }
                if (gph_direction == "W")
                {
                    g.DrawString("1", Font1, b0Black, object_x0 + 1, object_y0 + 22);
                    g.DrawString("2", Font0, b0Black, object_x0 + 1, object_y0 + 58);
                    g.DrawRectangle(p5DarkBlue, object_x0 + 0, object_y0 + 22, 11, line_dy - 46); // Desenare simbol linie
                    g.DrawLine(p5DarkBlue, object_x0 + 6, object_y0, object_x0 + 6, object_y0 + 22);
                    g.DrawLine(p5DarkBlue, object_x0 + 6, object_y0 + line_dy - 22, object_x0 + 6, object_y0 + line_dy);
                    g.DrawEllipse(p5DarkBlue, object_x0 + 4, object_y0, 4, 4); // Pin_Bus1
                    if (lines[i1, lines_PROP_brk1] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                    //g.FillRectangle(b12, object_x0 + 10, object_y0 + 2, 8, 8); // Brk1
                    g.FillRectangle(SolidBrush_crt, object_x0 + 2, object_y0 + 10, 8, 8); // Brk1
                    g.DrawEllipse(p5DarkBlue, object_x0 + 4, object_y0 + line_dy - 4, 4, 4); // Pin_Bus2
                    if (lines[i1, lines_PROP_brk2] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                    g.FillRectangle(SolidBrush_crt, object_x0 + 2, object_y0 + line_dy - 18, 8, 8); // Brk2
                }

                if (((extend_x == 3)|| (extend_x == 2)) && (gph_direction == "N")) { 
                    if(lines[i1, lines_PROP_onoff]=="enable") { 
                        // butoane de comenzi pentru brk1
                        g.FillRectangle(b8LightCoral, object_x0 + 3, object_y0 + 14, 17, 12);
                        g.DrawString("on", Font1, b0Black, object_x0 + 3, object_y0 + 14);
                        g.FillRectangle(b14Aqua, object_x0 + 3 + 17 + 2, object_y0 + 14, 17, 12);
                        g.DrawString("off", Font1, b0Black, object_x0 + 3 + 1 + 17, object_y0 + 14);
                        // butoane de comenzi pentru brk2
                        g.FillRectangle(b8LightCoral, object_x0 + object_dx * extend_x -39, object_y0 + 14, 17, 12);
                        g.DrawString("on", Font1, b0Black, object_x0 + object_dx * extend_x - 39, object_y0 + 14);
                        g.FillRectangle(b14Aqua, object_x0 + object_dx * extend_x - 20, object_y0 + 14, 17, 12);
                        g.DrawString("off", Font1, b0Black, object_x0 + object_dx * extend_x - 20, object_y0 + 14);
                    }
                    if (lines[i1, lines_PROP_sign1] == "enable")
                    {
                        // butoane de semnalizare protectii pentru brk1
                        g.FillRectangle(b13LightYellow, object_x0 + 3, object_y0 + 26, 17, 12);
                        g.DrawString("s1", Font1, b0Black, object_x0 + 3, object_y0 + 26);
                        // butoane de semnalizare protectii pentru brk2
                        g.FillRectangle(b13LightYellow, object_x0 + object_dx * extend_x - 39, object_y0 + 26, 17, 12);
                        g.DrawString("s1", Font1, b0Black, object_x0 + object_dx * extend_x - 39, object_y0 + 26);
                    }
                    if (lines[i1, lines_PROP_sign2] == "enable")
                    {
                        // butoane de semnalizare protectii pentru brk1
                        g.FillRectangle(b13LightYellow, object_x0 + 3 + 17 + 2, object_y0 + 26, 17, 12);
                        g.DrawString("s2", Font1, b0Black, object_x0 + 3 + 1 + 17, object_y0 + 26);
                        // butoane de semnalizare protectii pentru brk2
                        g.FillRectangle(b13LightYellow, object_x0 + object_dx * extend_x - 20, object_y0 + 26, 17, 12);
                        g.DrawString("s2", Font1, b0Black, object_x0 + object_dx * extend_x - 20, object_y0 + 26);
                    }
                }

                // display parameters
                int indent = 0;
                if (gph_direction == "N") indent = 0; if (gph_direction == "W") indent = 11;

                s1 = "Ln=" + lines[i1, lines_PROP_name];
                if (lines[i1, lines_PROP_gph_selected] == "1") { Font_crt = Font1bold; SolidBrush_crt = b6Red; }
                    else { Font_crt = Font1; SolidBrush_crt = b0Black; }
                if (gph_direction == "N") {
                    if (extend_x == 1) g.DrawString(s1, Font_crt, SolidBrush_crt, object_x0 + 2 + indent + object_dx / 2 * (extend_x - 1), object_y0 + 11);
                    if (extend_x == 2) g.DrawString(s1, Font_crt, SolidBrush_crt, object_x0 + object_dx * 2 / 2 - 25, object_y0 + 13);
                    if (extend_x == 3) g.DrawString(s1, Font_crt, SolidBrush_crt, object_x0 + object_dx * 3 / 2 - 20, object_y0 + 13);
                }
                if (gph_direction == "W") g.DrawString(s1, Font_crt, SolidBrush_crt, object_x0 + 2 + indent, object_y0 + 11);

                string bus_display = "";
                bus_display = lines[i1, lines_PROP_bus1];
                if (lines[i1, lines_PROP_bus1].Contains("@")) bus_display = lines[i1, lines_PROP_bus1].Remove(lines[i1, lines_PROP_bus1].IndexOf('@'));
                if (bus_display.Length > 4) s1 = "B1=" + bus_display.Remove(4); else s1 = "B1=" + bus_display;
                if (extend_x == 1) g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent, object_y0 + 22);
                if ((extend_x == 2) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 - 10 + (extend_x - 1) * 10, object_y0 + 13);
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 22 + (extend_x - 1) * 10, object_y0 + 13);

                bus_display = lines[i1, lines_PROP_bus2];
                if (lines[i1, lines_PROP_bus2].Contains("@")) bus_display = lines[i1, lines_PROP_bus2].Remove(lines[i1, lines_PROP_bus2].IndexOf('@'));
                if(bus_display.Length>4) s1 = "B2=" + bus_display.Remove(4); else s1 = "B2=" + bus_display;
                if (extend_x == 1) g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent + 47, object_y0 + 22);
                if ((extend_x == 2) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + object_dx * extend_x - 25 - (extend_x - 1) * 20, object_y0 + 13);
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + object_dx * extend_x - 44 - (extend_x - 1) * 20, object_y0 + 13);

                if(lines[i1, lines_PROP_length] != "")
                s1 = "L=" + double.Parse(lines[i1, lines_PROP_length]).ToString("##0.000") + "" + lines[i1, lines_PROP_units];
                if (extend_x == 1) g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent, object_y0 + 32);
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + 38);


                s1 = "Cod=" + lines[i1, lines_PROP_linecode];
                if (extend_x == 1) g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent, object_y0 + 42);
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 42, object_y0 + 25);

                s1 = "Imax=" + lines[i1, lines_PROP_Imax];
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 127, object_y0 + 25);

                s1 = "Umax=" + lines[i1, lines_PROP_Umax];
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 185, object_y0 + 25);

                s1 = "R=" + lines[i1, lines_PROP_R1];
                if ((extend_x == 2) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 75, object_y0 + 28);
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 65, object_y0 + 38);

                s1 = "X=" + lines[i1, lines_PROP_X1];
                if ((extend_x == 2) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 75, object_y0 + 40);
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 120, object_y0 + 38);

                s1 = "G=";// + lines[i1, lines_PROP_C1];
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 175, object_y0 + 38);

                s1 = "B=";// + lines[i1, lines_PROP_C1];
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 230, object_y0 + 38);

                double P3f = 0, P3f_kilo = 0, P3f_t2 = 0, P3f_t2_kilo = 0, delta_P3f = 0, delta_P3f_kilo = 0;
                if (lines[i1, lines_PROP_P] != "") {
                    P3f = double.Parse(lines[i1, lines_PROP_P]); P3f_kilo = P3f / 1000;
                    if (lines[i1, lines_PROP_P_t2] != "") P3f_t2 = double.Parse(lines[i1, lines_PROP_P_t2]); P3f_t2_kilo = P3f_t2 / 1000;
                    delta_P3f = P3f + P3f_t2; delta_P3f_kilo = delta_P3f / 1000;
                    // Draw arrow showing the active power direction. If powers are under 0.01 kW or 0.01 kvar, arrows are not shown
                    if ((P3f > 0.01) && (lines[i1, lines_PROP_P] != "") && (gph_direction == "N")) { 
                        if (extend_x == 1) g.DrawLine(p1Black4arrow, object_x0 + 33 + object_dx / 2 * (extend_x - 1), object_y0 + 3, object_x0 + 57 + object_dx / 2 * (extend_x - 1), object_y0 + 3);
                        if (extend_x == 2) g.DrawLine(p1Black4arrow, object_x0 + 85, object_y0 + 3, object_x0 + 100, object_y0 + 3);
                        if (extend_x == 3) g.DrawLine(p1Black4arrow, object_x0 + 150, object_y0 + 3, object_x0 + 182, object_y0 + 3);
                    }
                    if ((P3f < 0.01) && (lines[i1, lines_PROP_P] != "") && (gph_direction == "N")) {
                        if (extend_x == 1) g.DrawLine(p1Black4arrow, object_x0 + 57 + object_dx / 2 * (extend_x - 1), object_y0 + 3, object_x0 + 33 + object_dx / 2 * (extend_x - 1), object_y0 + 3);
                        if (extend_x == 2) g.DrawLine(p1Black4arrow, object_x0 + 100, object_y0 + 3, object_x0 + 85, object_y0 + 3);
                        if (extend_x == 3) g.DrawLine(p1Black4arrow, object_x0 + 182, object_y0 + 3, object_x0 + 150, object_y0 + 3);
                    }
                    if ((P3f < 0.01) && (lines[i1, lines_PROP_P] != "") && (gph_direction == "W"))
                        g.DrawLine(p1Black4arrow, object_x0 + 3, object_y0 + 57, object_x0 + 3, object_y0 + 37);
                    if ((P3f > 0.01) && (lines[i1, lines_PROP_P] != "") && (gph_direction == "W"))
                        g.DrawLine(p1Black4arrow, object_x0 + 3, object_y0 + 33, object_x0 + 3, object_y0 + 57);
                    if (Math.Abs(P3f) >= 10000) s1 = "P=" + P3f_kilo.ToString("##0.0") + "M"; else s1 = "P=" + P3f.ToString("####0.0") + "k";
                    if (Math.Abs(P3f_t2) >= 10000) s2 = "P=" + P3f_t2_kilo.ToString("##0.0") + "M"; else s2 = "P=" + P3f_t2.ToString("####0.0") + "k";
                    if (Math.Abs(delta_P3f) >= 10000) s3 = "ΔP=" + delta_P3f_kilo.ToString("##0.0") + "M"; else s3 = "ΔP=" + delta_P3f.ToString("####0.00") + "k";
                    //s1 = "P=" + lines[i1, lines_PROP_P];
                    if (extend_x == 1) g.DrawString(s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 52);
                    if ((extend_x == 2) && (gph_direction == "N"))
                    {
                        int dx1 = 0;
                        Font_crt = Font12b;
                        if (lines[i1, lines_PROP_HidePinsNo] == "1") dx1 = -13;
                            g.DrawString(s1, Font_crt, b2Blue, object_x0 + 30 + dx1, object_y0 - 4);
                        if (lines[i1, lines_PROP_P_t2] != "") g.DrawString(s2, Font_crt, b2Blue, object_x0 + 100, object_y0 - 4);
                        //g.DrawString(s3, Font1, b2Blue, object_x0 + 91, object_y0 - 1);
                    }
                    if ((extend_x == 3) && (gph_direction == "N"))
                    {
                        g.DrawString(s1, Font1, b2Blue, object_x0 + 30, object_y0 - 1);
                        if (lines[i1, lines_PROP_P_t2] != "") g.DrawString(s2, Font1, b2Blue, object_x0 + 188, object_y0 - 1);
                        g.DrawString(s3, Font1, b2Blue, object_x0 + 91, object_y0 - 1);
                    }
                }

                double Q3f = 0, Q3f_kilo=0, Q3f_t2 = 0, Q3f_t2_kilo = 0, delta_Q3f = 0, delta_Q3f_kilo = 0;
                if (lines[i1, lines_PROP_Q] != "")
                {
                    Q3f = double.Parse(lines[i1, lines_PROP_Q]); Q3f_kilo = Q3f / 1000;
                    if (lines[i1, lines_PROP_Q_t2] != "")  Q3f_t2 = double.Parse(lines[i1, lines_PROP_Q_t2]); Q3f_t2_kilo = Q3f_t2 / 1000;
                    delta_Q3f = Q3f + Q3f_t2; delta_Q3f_kilo = delta_Q3f / 1000;
                    // Draw arrow showing the reactive power direction
                    if ((Q3f > 0) && (lines[i1, lines_PROP_P] != "") && (gph_direction == "N")) {
                        if (extend_x == 1) g.DrawLine(p1Green4arrow, object_x0 + 39 + object_dx / 2 * (extend_x - 1), object_y0 + 7, object_x0 + 51 + object_dx / 2 * (extend_x - 1), object_y0 + 7);
                        if (extend_x == 3) g.DrawLine(p1Green4arrow, object_x0 + 158, object_y0 + 7, object_x0 + 175, object_y0 + 7);
                    }
                    if ((Q3f < 0) && (lines[i1, lines_PROP_Q] != "") && (gph_direction == "N")) {
                        if (extend_x == 1) g.DrawLine(p1Green4arrow, object_x0 + 51 + object_dx / 2 * (extend_x - 1), object_y0 + 7, object_x0 + 39 + object_dx / 2 * (extend_x - 1), object_y0 + 7);
                        if (extend_x == 3) g.DrawLine(p1Green4arrow, object_x0 + 174, object_y0 + 7, object_x0 + 156, object_y0 + 7);
                    }
                    if(Math.Abs(Q3f) >= 10000) s1 = "Q=" + Q3f_kilo.ToString("##0.0") +"M"; else s1 = "Q=" + Q3f.ToString("####0.0") + "k";
                    if (Math.Abs(Q3f_t2) >= 10000) s2 = "Q=" + Q3f_t2_kilo.ToString("##0.0") + "M"; else s2 = "Q=" + Q3f_t2.ToString("####0.0") + "k";
                    if (Math.Abs(delta_Q3f) >= 10000) s3 = "ΔQ=" + delta_Q3f_kilo.ToString("##0.0") + "M"; else s3 = "ΔQ=" + delta_Q3f.ToString("####0.00") + "k";
                    //s1 = "Q=" + lines[i1, lines_PROP_Q];
                    if (extend_x == 1) g.DrawString(s1, Font1, b2Blue, object_x0 + 50 + indent, object_y0 + 52);
                    if ((extend_x == 2) && (gph_direction == "N"))
                    {
                        Font_crt = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
                        g.DrawString(s1, Font_crt, b2Blue, object_x0 + 30, object_y0 - 20);
                        if (lines[i1, lines_PROP_Q_t2] != "") g.DrawString(s2, Font_crt, b2Blue, object_x0 + 100, object_y0 - 20);
                        //g.DrawString(s3, Font1, b2Blue, object_x0 + 91, object_y0 - 14);
                    }
                    if ((extend_x == 3) && (gph_direction == "N"))
                    {
                        g.DrawString(s1, Font1, b2Blue, object_x0 + 30, object_y0 - 14);
                        if (lines[i1, lines_PROP_Q_t2] != "") g.DrawString(s2, Font1, b2Blue, object_x0 + 188, object_y0 - 14);
                        g.DrawString(s3, Font1, b2Blue, object_x0 + 91, object_y0 - 14);
                    }
                }

                double S3f = 0, S3f_kilo = 0, S3f_t2 = 0, S3f_t2_kilo = 0;
                if (lines[i1, lines_PROP_S] != "")
                {
                    S3f = double.Parse(lines[i1, lines_PROP_S]); S3f_kilo = S3f / 1000;
                    if (lines[i1, lines_PROP_S_t2] != "") S3f_t2 = double.Parse(lines[i1, lines_PROP_S_t2]); S3f_t2_kilo = S3f_t2 / 1000;
                    if (Math.Abs(S3f) >= 10000) s1 = "S=" + S3f_kilo.ToString("##0.0") + "M"; else s1 = "S=" + S3f.ToString("####0") + "k";
                    if (Math.Abs(S3f_t2) >= 10000) s2 = "S=" + S3f_t2_kilo.ToString("##0.0") + "M"; else s2 = "S=" + S3f_t2.ToString("####0.0") + "k";
                    // s1 = "S=" + double.Parse(lines[i1, lines_PROP_S]).ToString("#0.0");
                    if (extend_x == 1) g.DrawString(s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 62);
                    if ((extend_x == 2) && (gph_direction == "N"))
                    {
                        // Afisare S [kVA] in ambele capete ale liniei
                        //g.DrawString(s1, Font1, b2Blue, object_x0 + 30, object_y0 - 25);
                        //if (lines[i1, lines_PROP_S_t2] != "") g.DrawString(s2, Font1, b2Blue, object_x0 + 188, object_y0 - 25);
                    }
                    if ((extend_x == 3) && (gph_direction == "N"))
                    {
                        g.DrawString(s1, Font1, b2Blue, object_x0 + 30, object_y0 - 25);
                        if (lines[i1, lines_PROP_S_t2] != "") g.DrawString(s2, Font1, b2Blue, object_x0 + 188, object_y0 - 25);
                    }
                }
                if (lines[i1, lines_PROP_I1] != "")
                {
                    if (extend_x == 1)
                    {
                        s1 = "I1=" + double.Parse(lines[i1, lines_PROP_I1]).ToString("#0.0");
                        g.DrawString(s1, Font1, b2Blue, object_x0 + 50 + indent, object_y0 + 62);
                    }
                    if ((extend_x == 3) && (gph_direction == "N")) {
                        s1 = "I1=" + double.Parse(lines[i1, lines_PROP_I1]).ToString("#0.000");
                        if (lines[i1, lines_PROP_I1_t2] != "") s2 = "I1=" + double.Parse(lines[i1, lines_PROP_I1_t2]).ToString("#0.000");
                        else s2 = "";
                        g.DrawString(s1, Font1, b2Blue, object_x0 + 75, object_y0 + 50);
                        g.DrawString(s2, Font1, b2Blue, object_x0 + 145, object_y0 + 50);
                    }
                }
                if (lines[i1, lines_PROP_I1fi] != "")
                {
                    s1 = "I1ϕ=" + double.Parse(lines[i1, lines_PROP_I1fi]).ToString("#0.000");
                    if (lines[i1, lines_PROP_I1fi_t2] != "") s2 = "I1ϕ=" + double.Parse(lines[i1, lines_PROP_I1fi_t2]).ToString("#0.000");
                    else s2 = "";
                    if ((extend_x == 3) && (gph_direction == "N"))
                    {
                        g.DrawString(s1, Font1, b2Blue, object_x0 + 75, object_y0 + 62);
                        g.DrawString(s2, Font1, b2Blue, object_x0 + 145, object_y0 + 62);
                    }
                }
                double U = 0, U_t2 = 0;
                if (lines[i1, lines_PROP_U1] != "")
                {
                    U = double.Parse(lines[i1, lines_PROP_U1]);
                    if (lines[i1, lines_PROP_U1_t2] != "") U_t2 = double.Parse(lines[i1, lines_PROP_U1_t2]);
                    if (U < 1000) { s1 = "U1 =" + U.ToString("#00.0"); s2 = "U1 =" + U_t2.ToString("#00.0"); }
                    else
                    {
                        U = U / 1000; // se va afisa in kV
                        U_t2 = U_t2 / 1000; // se va afisa in kV
                        s1 = "U1 =" + U.ToString("#00.00") + "k";
                        s2 = "U1 =" + U_t2.ToString("#00.00") + "k";
                    }
                    if (extend_x == 1)
                    {
                        if(gph_direction == "N") g.DrawString(s1+s1a, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 72);
                        if(gph_direction == "W") {
                            g.DrawString(s1, Font1, b2Blue, object_x0 - 2 + indent, object_y0 + 0);
                            g.DrawString("1", Font0s, b2Blue, object_x0 + 12 + indent, object_y0 + 5);
                            //g.DrawString(s1a, Font1, b2Blue, object_x0 + 15 + indent, object_y0 + 0);
                            g.DrawString(s2, Font1, b2Blue, object_x0 - 2 + indent, object_y0 + 72);
                            g.DrawString("2", Font0s, b2Blue, object_x0 + 12 + indent, object_y0 + 77);
                        }
                    }
                    if ((extend_x == 2) && (gph_direction == "N"))
                    {
                        Font_crt = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
                        g.DrawString(s1 + s1a, Font_crt, b2Blue, object_x0 + 4, object_y0 + 28);
                        if (lines[i1, lines_PROP_U1_t2] != "") g.DrawString(s2, Font_crt, b2Blue, object_x0 + 118, object_y0 + 28);
                    }
                    if ((extend_x == 3) && (gph_direction == "N"))
                    {
                        g.DrawString(s1+s1a, Font1, b2Blue, object_x0 + 4, object_y0 + 50);
                        if (lines[i1, lines_PROP_U1_t2] != "") g.DrawString(s2, Font1, b2Blue, object_x0 + 215, object_y0 + 50); //145
                    }
                }
                if (lines[i1, lines_PROP_U1fi] != "")
                {
                    s1 = "" + double.Parse(lines[i1, lines_PROP_U1fi]).ToString("#0.0");
                    if (lines[i1, lines_PROP_U1fi_t2] != "") s2 = "" + double.Parse(lines[i1, lines_PROP_U1fi_t2]).ToString("#0.0");
                    else s2 = "";
                    if (extend_x == 1)
                    {
                        s1 = "" + double.Parse(lines[i1, lines_PROP_U1fi]).ToString("#0.0");
                        if (gph_direction == "N") g.DrawString(s1, Font1, b2Blue, object_x0 + 60 + indent, object_y0 + 72);
                        if (gph_direction == "W")
                        {
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 57 + indent, object_y0 + 0);
                            g.DrawString(s2, Font1, b2Blue, object_x0 + 57 + indent, object_y0 + 72);
                        }
                    }
                    if ((extend_x == 2) && (gph_direction == "N"))
                    {
                        s1 = "U1ϕ=" +double.Parse(lines[i1, lines_PROP_U1fi]).ToString("#0.000");
                        if (lines[i1, lines_PROP_U1fi_t2] != "") s2 = "U1ϕ=" + double.Parse(lines[i1, lines_PROP_U1fi_t2]).ToString("#0.000");
                        else s2 = "";
                        g.DrawString(s1, Font1, b2Blue, object_x0 + 4, object_y0 + 40);
                        g.DrawString(s2, Font1, b2Blue, object_x0 + 118, object_y0 + 40);
                    }
                    if ((extend_x == 3) && (gph_direction == "N"))
                    {
                        s1 = "U1ϕ=" + double.Parse(lines[i1, lines_PROP_U1fi]).ToString("#0.000");
                        if (lines[i1, lines_PROP_U1fi_t2] != "") s2 = "U1ϕ=" + double.Parse(lines[i1, lines_PROP_U1fi_t2]).ToString("#0.000");
                        else s2 = "";
                        if (extend_x == 1)
                        {
                            g.DrawString("U1ϕ=" + s1, Font1, b2Blue, object_x0 + 4, object_y0 + 61);
                        }
                        if ((extend_x == 3) && (gph_direction == "N"))
                        {
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 4, object_y0 + 62);
                            g.DrawString(s2, Font1, b2Blue, object_x0 + 215, object_y0 + 62);//145
                        }
                    }
                }
                if ((lines[i1, lines_PROP_U1] != "") && (lines[i1, lines_PROP_U1fi] != "") && (lines[i1, lines_PROP_U1_t2] != "") && (lines[i1, lines_PROP_U1fi_t2] != ""))
                {
                    double U1r_t1, U1i_t1, U1r_t2, U1i_t2, dU1r, dU1i, dU1, dU1_kilo, dUfi1;
                    U1r_t1 = double.Parse(lines[i1, lines_PROP_U1])*Math.Cos(double.Parse(lines[i1, lines_PROP_U1fi])/180);
                    U1i_t1 = double.Parse(lines[i1, lines_PROP_U1]) * Math.Sin(double.Parse(lines[i1, lines_PROP_U1fi])/180);
                    U1r_t2 = double.Parse(lines[i1, lines_PROP_U1_t2]) * Math.Cos(double.Parse(lines[i1, lines_PROP_U1fi_t2])/180);
                    U1i_t2 = double.Parse(lines[i1, lines_PROP_U1_t2]) * Math.Sin(double.Parse(lines[i1, lines_PROP_U1fi_t2])/180);
                    dU1r = U1r_t1 - U1r_t2; // real part of delta U1
                    dU1i = U1i_t1 - U1i_t2; // imag part of delta U1
                    dU1 = Math.Sqrt(dU1r * dU1r + dU1i * dU1i); dU1_kilo = dU1 / 1000;
                    if (dU1i != 0)
                    {
                        dUfi1 = Math.Atan(dU1r / dU1i)*180;
                        if (dU1r < 0) dUfi1 += 180;
                        if (dUfi1 > 180) dUfi1 = dUfi1 - 360;
                    }
                    else
                    {
                        if (dU1r >= 0) dUfi1 = 0; else dUfi1 = -180.0;
                    }
                    lines[i1, lines_PROP_delta_U1] = dU1.ToString("##0.000");
                    lines[i1, lines_PROP_delta_U1fi] = dUfi1.ToString("##0.000");
                    if (Math.Abs(dU1) >= 1000) s1 = "ΔU1= " + dU1_kilo.ToString("##0.000") + "k";
                    else s1 = "ΔU1= " + dU1.ToString("##0.00");
                    //g.DrawString(s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 82);
                    if ((gph_direction == "N") && (extend_x == 3)) g.DrawString(s1, Font1, b2Blue, object_x0 + 101, object_y0 - 25);
                    if ((gph_direction == "W")&& (extend_x == 1)) g.DrawString(s1, Font1, b2Blue, object_x0 - 2 + indent, object_y0 +83);
                    if ((gph_direction == "N") && (extend_x == 1)) g.DrawString(s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 82);
                }

                if (default_xy == 1) obj_number++;
            }

        }

        /****************** Desenare obiecte de tip trafo ******************/
        private void Paint_trafos(object sender, PaintEventArgs e)
        {
            string s1 = "";
            //int obj_x = 0, obj_y = 0;

            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);

            // Desenare obiecte de tip sarcina (load)
            for (int i1 = 0; i1 < trafos_no; i1++)
            {
                int default_xy = 1;
                if ((trafos[i1, trafos_PROP_x0] == "") || (trafos[i1, trafos_PROP_y0] == ""))
                {
                    // nu exista coordonate anterioare
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    default_xy = 1;
                }
                else
                {   // Exista coordonate stabilite anterior
                    object_x0 = - X0_shift + int.Parse(trafos[i1, trafos_PROP_x0]);
                    object_y0 = - Y0_shift + int.Parse(trafos[i1, trafos_PROP_y0]);
                    default_xy = 0;
                }

                g.FillRectangle(b4LightBlue, object_x0, object_y0, object_dx + 20, line_dy);

                s1 = "P: "; g.DrawString(s1, Font1, b2Blue, object_x0 + 31, object_y0 + 2);
                s1 = "Q: "; g.DrawString(s1, Font1, b2Blue, object_x0 + 31, object_y0 + 12);
                s1 = "T: " + trafos[i1, trafos_PROP_name];
                if (trafos[i1, trafos_PROP_gph_selected] == "1") g.DrawString(s1, Font1bold, b6Red, object_x0 + 31, object_y0 + 22);
                else g.DrawString(s1, Font1, b0Black, object_x0 + 31, object_y0 + 22);
                if (trafos[i1, trafos_PROP_conns] == "(delta,wye)") s1 = "Bs=" + trafos[i1, trafos_PROP_busses] + "dw";
                g.DrawString(s1, Font1, b0Black, object_x0 + 31, object_y0 + 32);
                s1 = "kVs=" + trafos[i1, trafos_PROP_kVs]; g.DrawString(s1, Font1, b0Black, object_x0 + 31, object_y0 + 42);
                s1 = "kVA=" + trafos[i1, trafos_PROP_kVAs]; g.DrawString(s1, Font1, b0Black, object_x0 + 31, object_y0 + 52);
                //if(trafos[i1, trafos_PROP_conns]=="(delta,wye)")
                //s1 = "conns=" + "(dw)"; g.DrawString(s1, Font1, b0Black, object_x0 + 20, object_y0 + 42);
                s1 = "tap=" + trafos[i1, trafos_PROP_tap]; g.DrawString(s1, Font1, b0Black, object_x0 + 31, object_y0 + 62);

                double P3ph = 0;
                if ((trafos[i1, trafos_PROP_P1] != "") && (trafos[i1, trafos_PROP_P2] != "") && (trafos[i1, trafos_PROP_P3] != ""))
                    P3ph = double.Parse(trafos[i1, trafos_PROP_P1]) + double.Parse(trafos[i1, trafos_PROP_P2]) + double.Parse(trafos[i1, trafos_PROP_P3]);
                s1 = "P: " + P3ph.ToString("####0.0");
                trafos[i1, trafos_PROP_P] = P3ph.ToString("####0");
                g.DrawString(s1, Font1, b2Blue, object_x0 + 20, object_y0 + 72);

                double Q3ph = 0;
                if ((trafos[i1, trafos_PROP_Q1] != "") && (trafos[i1, trafos_PROP_Q2] != "") && (trafos[i1, trafos_PROP_Q3] != ""))
                    Q3ph = double.Parse(trafos[i1, trafos_PROP_Q1]) + double.Parse(trafos[i1, trafos_PROP_Q2]) + double.Parse(trafos[i1, trafos_PROP_Q3]);
                s1 = "Q: " + Q3ph.ToString("####0.0");
                trafos[i1, trafos_PROP_Q] = Q3ph.ToString("####0");
                g.DrawString(s1, Font1, b2Blue, object_x0 + 20, object_y0 + 82);

                double S3ph = Math.Sqrt(P3ph * P3ph + Q3ph * Q3ph);
                s1 = "S: " + S3ph.ToString("####0");
                trafos[i1, trafos_PROP_S] = S3ph.ToString("####0");
                g.DrawString(s1, Font1, b2Blue, object_x0 + 76, object_y0 + 82);

                g.DrawEllipse(p5DarkBlue, object_x0 + 11, object_y0, 6, 6); // Pin_Bus1
                g.DrawLine(p5DarkBlue, object_x0 + 14, object_y0 + 6, object_x0 + 14, object_y0 + 27);
                g.DrawEllipse(p5DarkBlue, object_x0 + 11, object_y0 + line_dy - 6, 6, 6); // Pin_Bus2
                g.DrawLine(p5DarkBlue, object_x0 + 14, object_y0 + line_dy - 6, object_x0 + 14, object_y0 + line_dy - 31);
                g.FillRectangle(b2Blue, object_x0 + 11, object_y0 + 10, 8, 8); // Brk1
                g.FillRectangle(b2Blue, object_x0 + 11, object_y0 + line_dy - 18, 8, 8); // Brk2

                g.DrawEllipse(p5DarkBlue2, object_x0, object_y0 + line_dy / 2 - 20, 28, 28); // Pin_Bus1
                g.DrawEllipse(p5DarkBlue2, object_x0, object_y0 + line_dy / 2 - 10, 28, 28); // Pin_Bus1

                if (default_xy == 1) obj_number++;
            }
        }

        /****************** Desenare obiecte de tip load si alte functii ale acestui obiect ******************/
        private void loads_properties_calculation(int nr_load)
        {
            if ((loads[nr_load, loads_PROP_x0] != "") && (loads[nr_load, loads_PROP_y0] != ""))
            {
                // calculate pins position inside the line representation on the grid picture
                int x0 = int.Parse(loads[nr_load, loads_PROP_x0]);
                int y0 = int.Parse(loads[nr_load, loads_PROP_y0]);
                int pin1_x = 0, pin1_y = 0;
                if (loads[nr_load, loads_PROP_gph_direction] == "N")
                {
                    pin1_x = x0 + object_dx / 2; pin1_y = y0 + 2;
                    loads[nr_load, loads_PROP_pin1_x] = pin1_x.ToString();
                    loads[nr_load, loads_PROP_pin1_y] = pin1_y.ToString();
                }
            }
        }

        /****************** Desenare obiecte de tip loads ******************/
        private void Paint_loads(object sender, PaintEventArgs e)
        {
            string s1 = "", s2 = "";
            double P3f = 0, P3f_mega = 0;
            double Q3f = 0, Q3f_mega = 0;
            //int obj_x = 0, obj_y = 0;

            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            //Point[] polyPoints_clip_scheme_zone = {new Point(100, 100), new Point(500, 100), new Point(500, 500), new Point(100, 500)};
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            // Construct a region based on the path.
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);
            // Clipping the plygones end lines

            // Desenare obiecte de tip sarcina (load)
            for (int i1 = 0; i1 < loads_no; i1++)
            {
                int default_xy = 1;
                if ((loads[i1, loads_PROP_x0] == "") || (loads[i1, loads_PROP_y0] == ""))
                {
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    default_xy = 1;
                }
                else
                {
                    object_x0 = -X0_shift + int.Parse(loads[i1, loads_PROP_x0]);
                    object_y0 = -Y0_shift + int.Parse(loads[i1, loads_PROP_y0]);
                    default_xy = 0;
                }

                // desenare dreptunghi general
                if ((loads[i1, loads_PROP_sim_storage] == "") && (loads[i1, loads_PROP_sim_type] == ""))
                    g.FillRectangle(b13LightYellow, object_x0, object_y0, object_dx, line_dy);
                if (loads[i1, loads_PROP_sim_storage] == "stor")
                { // avem storage
                    g.FillRectangle(b7LightGreen, object_x0, object_y0, object_dx * 1 / 2, line_dy);
                    g.FillRectangle(b13LightYellow, object_x0 + object_dx * 1 / 2, object_y0, object_dx * 1 / 2, line_dy);

                }
                if (loads[i1, loads_PROP_sim_type] == "EV")
                { // avem EV

                }

                if (loads[i1, loads_PROP_sim_type] == "") // avem load clasic, nu stoarge sau EV
                {
                    int y0res = 0;
                    string gph_direction = loads[i1, loads_PROP_gph_direction];
                    // draw the line symbol
                    if (gph_direction == "N")
                    {
                            g.DrawLine(p1Black3, object_x0 + 20, object_y0 + 3 + 16, object_x0 + object_dx - 20, object_y0 + 3 + 16); // Bus of the load
                            g.DrawLine(p1Black, object_x0 + object_dx / 2, object_y0 + 6, object_x0 + object_dx / 2, object_y0 + 20); // Bus of the load

                            if (loads[i1, loads_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                            g.FillRectangle(SolidBrush_crt, object_x0 + object_dx / 2 - 4, object_y0 + 8, 9, 8); // Brk1

                            g.DrawEllipse(p5DarkBlue, object_x0 + object_dx / 2 - 3, object_y0, 6, 6); // Pin_Bus1
                            y0res = 20;
                    }
                    if (gph_direction == "W")
                    {
                            //g.DrawLine(p1Black3, object_x0 + 3 + 16, object_y0, object_x0 + 3 + 16, object_y0 + line_dy);
                            g.DrawLine(p1Black3, object_x0 + 3 + 16, object_y0 + 22 + 10, object_x0 + 3 + 16, object_y0 + line_dy - 42);
                            g.DrawLine(p1Black, object_x0 + 12, object_y0 + +line_dy / 2 - 5, object_x0 + 20, object_y0 + line_dy / 2 - 5); // Bus of the load

                            if (loads[i1, loads_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                            g.FillRectangle(SolidBrush_crt, object_x0 + 8, object_y0 + line_dy / 2 - 3 - 5, 8, 8); // Brk1

                            //g.DrawEllipse(p5DarkBlue, object_x0 + 16, object_y0, 6, 6); // Pin_Bus1
                            //g.DrawEllipse(p5DarkBlue, object_x0 + 16, object_y0 + line_dy - 6, 6, 6); // Pin_Bus1
                            g.DrawEllipse(p5DarkBlue, object_x0, object_y0 + line_dy / 2 - 3 - 5, 6, 6); // Pin_Bus1
                            y0res = 1;
                    }
                    if (loads[i1, loads_PROP_sim_type] == "") s1 = "Ld=";
                    else s1 = loads[i1, loads_PROP_sim_type] + "=";
                    s1 += loads[i1, loads_PROP_name];
                    if (loads[i1, loads_PROP_gph_selected] == "1") g.DrawString(s1, Font1bold, b6Red, object_x0 + 1, object_y0 + y0res);
                    else g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + y0res);
                    if (loads[i1, loads_PROP_onoff] == "enable") { // este posibil sa fie facute manevre on si off cu breakerul
                        if (gph_direction == "N") {
                            // b8LightCoral b14Aqua
                            g.FillRectangle(b8LightCoral, object_x0 + 3, object_y0 + 3, 17, 12);
                            g.DrawString("on", Font1, b0Black, object_x0 + 3, object_y0 + 3);
                            g.FillRectangle(b14Aqua, object_x0 + 3 + 17 + 2, object_y0 + 3, 17, 12);
                            g.DrawString("off", Font1, b0Black, object_x0 + 3 + 1 + 17, object_y0 + 3);
                        }
                        if (gph_direction == "W")
                        {
                            g.FillRectangle(b8LightCoral, object_x0 + 3 + 21, object_y0 - 10 + line_dy / 2, 17, 12);
                            g.DrawString("on", Font1, b0Black, object_x0 + 3 + 21, object_y0 - 10 + line_dy / 2);
                            g.FillRectangle(b14Aqua, object_x0 + 3 + 21 + 2 + 17, object_y0 - 10 + line_dy / 2, 17, 12);
                            g.DrawString("off", Font1, b0Black, object_x0 + 3 + 1 + 21 + 17, object_y0 - 10 + line_dy / 2);
                        }
                    }

                    //loads[loads_no, loads_PROP_gph_DrawType]
                    s1 = "Bus=" + loads[i1, loads_PROP_bus];
                    if (loads[i1, loads_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + y0res + 10);
                    double dval = 0, dval_mega = 0, force_mega = 0;
                    // Display Pn
                    if(loads[i1, loads_PROP_Pn] != "")
                        dval = double.Parse(loads[i1, loads_PROP_Pn]); dval_mega = dval / 1000;
                    if ((dval>=100)||(dval<=-100)) s1 = "Pn=" + dval.ToString("##0");
                    else s1 = "Pn=" + dval.ToString("##0.0");
                    if (Math.Abs(dval) >= 10000) { s1 = "Pn=" + dval_mega.ToString("##0") + "M"; force_mega = 1; }
                    if(loads[i1, loads_PROP_gph_DrawType]=="") g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + y0res + 20);
                    if (loads[i1, loads_PROP_gph_DrawType] == "Ld1S1") g.DrawString(s1, Font3, b0Black, object_x0 + 1, object_y0 + y0res + 10);

                    // Display Qn or PF 
                    if (loads[i1, loads_PROP_Qn] != "")
                    {
                        dval = double.Parse(loads[i1, loads_PROP_Qn]); dval_mega = dval / 1000;
                        //s1 = "Qn=" + double.Parse(loads[i1, loads_PROP_Qn]).ToString("##0.0");
                        if ((Math.Abs(dval) >= 10000) || (force_mega==1)) {
                            s1 = "Qn=" + dval_mega.ToString("##0") + "M"; force_mega = 1;
                        }
                        else s1 = "Qn=" + double.Parse(loads[i1, loads_PROP_Qn]).ToString("##0.0");
                        if (loads[i1, loads_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, b0Black, object_x0 + 50, object_y0 + y0res + 20);
                        if (loads[i1, loads_PROP_gph_DrawType] == "Ld1S1") g.DrawString(s1, Font3, b0Black, object_x0 + 1, object_y0 + y0res + 23);
                    }
                    else if (loads[i1, loads_PROP_PF] != "")
                    {
                        s1 = "PF=" + loads[i1, loads_PROP_PF];
                        if (loads[i1, loads_PROP_PF] != "") g.DrawString(s1, Font1, b0Black, object_x0 + 55, object_y0 + y0res + 20);
                    }

                    if (loads[i1, loads_PROP_P] != "")
                    {
                        force_mega = 0;
                        P3f = double.Parse(loads[i1, loads_PROP_P]); P3f_mega = P3f / 1000;
                        //if(i1==11)
                        //    s1 = "P=" + P3f.ToString("#####0.0");
                        if (Math.Abs(P3f) >= 10000)
                        {
                            s1 = "P=" + P3f_mega.ToString("#####0") + "M";
                            force_mega = 1;
                        }
                        else s1 = "P=" + P3f.ToString("#####0.0");
                        if (gph_direction == "N")
                        {
                            if (loads[i1, loads_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, b2Blue, object_x0 + 1, object_y0 + 52);
                            if (loads[i1, loads_PROP_gph_DrawType] == "Ld1S1") g.DrawString(s1, Font3, b2Blue, object_x0 + 1, object_y0 + y0res + 36);
                        }
                        if (gph_direction == "W") g.DrawString(s1, Font1, b2Blue, object_x0 + 1, object_y0 + y0res + 50);
                    }
                    if (loads[i1, loads_PROP_Q] != "")
                    {
                        Q3f = double.Parse(loads[i1, loads_PROP_Q]); Q3f_mega = Q3f / 1000;
                        if ((Math.Abs(Q3f) >= 10000) || (force_mega==1))
                        {
                            s1 = "Q=" + Q3f_mega.ToString("#####0") + "M";
                            force_mega = 1;
                        }
                        else s1 = "Q=" + Q3f.ToString("#####0.0");
                        if (gph_direction == "N")
                        {
                            if (loads[i1, loads_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, b2Blue, object_x0 + 50, object_y0 + 52);
                            if (loads[i1, loads_PROP_gph_DrawType] == "Ld1S1") g.DrawString(s1, Font3, b2Blue, object_x0 + 1, object_y0 + y0res + 49);
                        }
                        if (gph_direction == "W") g.DrawString(s1, Font1, b2Blue, object_x0 + 50, object_y0 + y0res + 50);
                    }

                    double u = 0, u_kilo = 0;
                    double fi = 0;
                    if ((loads[i1, loads_PROP_U1] != "") && (loads[i1, loads_PROP_U2] != "") && (loads[i1, loads_PROP_U3] != "")
                        && (loads[i1, loads_PROP_U1fi] != ""))
                    {
                        u = double.Parse(loads[i1, loads_PROP_U1]); u_kilo = u/1000;  s1 = "U1=";
                        if (u >= 1000) s1 += u_kilo.ToString("#00.00")+"k"; else s1 += u.ToString("#00.0");
                        fi = double.Parse(loads[i1, loads_PROP_U1fi]);
                        if (u >= 1000) s1 += "  "; else s1 += "   ";
                        if (loads[i1, loads_PROP_gph_DrawType] == "") { if (fi > 0) s1 += "+"; s1 += fi.ToString("#00.0"); }
                        s2 = fi.ToString("#0.00");
                        Font_crt = new System.Drawing.Font("Arial", 8);
                        if (loads[i1, loads_PROP_Font1] !="") Font_crt = new System.Drawing.Font("Arial", 10, FontStyle.Bold); ;
                        if (loads[i1, loads_PROP_gph_DrawType] == "") g.DrawString(s1, Font_crt, b2Blue, object_x0 + 0, object_y0 + 62);
                        if (loads[i1, loads_PROP_gph_DrawType] == "Ld1S1")
                        {
                            g.DrawString(s1, Font3, b2Blue, object_x0 + 0, object_y0 + y0res + 62);
                            g.DrawString(s2, Font1, b2Blue, object_x0 + 64, object_y0 + y0res + 64);
                        }
                        //g.DrawString(s1, Font1, b2Blue, object_x0 + 55, object_y0 + 72);

                        u = double.Parse(loads[i1, loads_PROP_U2]); u_kilo = u/1000; s1 = "U2=";
                        if (u >= 1000) s1 += u_kilo.ToString("#00.00") + "k"; else s1 += u.ToString("#00.0");
                        fi = double.Parse(loads[i1, loads_PROP_U2fi]);
                        if (u >= 1000) s1 += "  "; else s1 += "   ";
                        if (fi > 0) s1 += "+";
                        if (u >= 1000) s1 += fi.ToString("#00"); else s1 += fi.ToString("#00.0");
                        if (loads[i1, loads_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 72);

                        u = double.Parse(loads[i1, loads_PROP_U3]); u_kilo = u/1000; s1 = "U3=";
                        if (u >= 1000) s1 += u_kilo.ToString("#00.00") + "k"; else s1 += u.ToString("#00.0");
                        fi = double.Parse(loads[i1, loads_PROP_U3fi]);
                        if (u >= 1000) s1 += "  "; else s1 += "   ";
                        if (fi > 0) s1 += "+";
                        if (u >= 1000) s1 += fi.ToString("#00"); else s1 += fi.ToString("#00.0");
                        if (loads[i1, loads_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 82);

                    }

                    if (loads[i1, loads_PROP_U1fi] != "")
                    {
                    }
                }
                // Paint storage 
                if (loads[i1, loads_PROP_sim_type] == "storage") //  avem storage
                {
                    int y0res = 0;
                    string gph_direction = loads[i1, loads_PROP_gph_direction];
                    // draw the line symbol
                    if (gph_direction == "N")
                    {
                        //g.DrawLine(p1Black3, object_x0, object_y0 + 3 + 16, object_x0 + object_dx, object_y0 + 3 + 16); // Bus of the load
                        g.DrawLine(p1Black3, object_x0 + 20, object_y0 + 3 + 16, object_x0 + object_dx - 20, object_y0 + 3 + 16); // Bus of the load
                        g.DrawLine(p1Black, object_x0 + object_dx / 2, object_y0 + 6, object_x0 + object_dx / 2, object_y0 + 20); // Bus of the load

                        if (loads[i1, loads_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                        g.FillRectangle(SolidBrush_crt, object_x0 + object_dx / 2 - 4, object_y0 + 8, 9, 8); // Brk1

                        //g.DrawEllipse(p5DarkBlue, object_x0, object_y0 + 16, 6, 6); // Pin_Bus1
                        g.DrawEllipse(p5DarkBlue, object_x0 + object_dx / 2 - 3, object_y0, 6, 6); // Pin_Bus1
                                                                                                   //g.DrawEllipse(p5DarkBlue, object_x0 + object_dx - 6, object_y0 + 16, 6, 6); // Pin_Bus1
                        y0res = 21;
                    }
                    if (gph_direction == "W")
                    {
                        //g.DrawLine(p1Black3, object_x0 + 3 + 16, object_y0, object_x0 + 3 + 16, object_y0 + line_dy);
                        g.DrawLine(p1Black3, object_x0 + 3 + 16, object_y0 + 22 + 10, object_x0 + 3 + 16, object_y0 + line_dy - 42);
                        g.DrawLine(p1Black, object_x0 + 12, object_y0 + +line_dy / 2 - 5, object_x0 + 20, object_y0 + line_dy / 2 - 5); // Bus of the load

                        if (loads[i1, loads_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                        g.FillRectangle(SolidBrush_crt, object_x0 + 8, object_y0 + line_dy / 2 - 3 - 5, 8, 8); // Brk1

                        //g.DrawEllipse(p5DarkBlue, object_x0 + 16, object_y0, 6, 6); // Pin_Bus1
                        //g.DrawEllipse(p5DarkBlue, object_x0 + 16, object_y0 + line_dy - 6, 6, 6); // Pin_Bus1
                        g.DrawEllipse(p5DarkBlue, object_x0, object_y0 + line_dy / 2 - 3 - 5, 6, 6); // Pin_Bus1
                        y0res = 1;
                    }
                    if (loads[i1, loads_PROP_sim_type] == "") s1 = "Ld=";
                    else s1 = loads[i1, loads_PROP_sim_type] + "=";
                    s1 += loads[i1, loads_PROP_name];
                    if (loads[i1, loads_PROP_gph_selected] == "1") g.DrawString(s1, Font1bold, b6Red, object_x0 + 1, object_y0 + y0res);
                    else g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + y0res);
                    if (loads[i1, loads_PROP_onoff] == "enable")
                    { // este posibil sa fie facute manevre on si off cu breakerul
                        if (gph_direction == "N")
                        {
                            // b8LightCoral b14Aqua
                            g.FillRectangle(b8LightCoral, object_x0 + 3, object_y0 + 3, 17, 12);
                            g.DrawString("on", Font1, b0Black, object_x0 + 3, object_y0 + 3);
                            g.FillRectangle(b14Aqua, object_x0 + 3 + 17 + 2, object_y0 + 3, 17, 12);
                            g.DrawString("off", Font1, b0Black, object_x0 + 3 + 1 + 17, object_y0 + 3);
                        }
                        if (gph_direction == "W")
                        {
                            g.FillRectangle(b8LightCoral, object_x0 + 3 + 21, object_y0 - 10 + line_dy / 2, 17, 12);
                            g.DrawString("on", Font1, b0Black, object_x0 + 3 + 21, object_y0 - 10 + line_dy / 2);
                            g.FillRectangle(b14Aqua, object_x0 + 3 + 21 + 2 + 17, object_y0 - 10 + line_dy / 2, 17, 12);
                            g.DrawString("off", Font1, b0Black, object_x0 + 3 + 1 + 21 + 17, object_y0 - 10 + line_dy / 2);
                        }
                    }

                    //s1 = "Ld=" + loads[i1, loads_PROP_name]; g.DrawString(s1, Font1, b0Black, object_x0 + 10, object_y0 + 12);
                    s1 = "Bus=" + loads[i1, loads_PROP_bus]; g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + y0res + 10);
                    double dval = 0;
                    dval = double.Parse(loads[i1, loads_PROP_Pn]);
                    if ((dval >= 100) || (dval <= -100)) s1 = "Pn=" + dval.ToString("##0");
                    else s1 = "Pn=" + dval.ToString("##0.0");
                    g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + y0res + 20);
                    if (loads[i1, loads_PROP_Qn] != "")
                    {
                        s1 = "Qn=" + double.Parse(loads[i1, loads_PROP_Qn]).ToString("##0.0");
                        g.DrawString(s1, Font1, b0Black, object_x0 + 50, object_y0 + y0res + 20);
                    }
                    else if (loads[i1, loads_PROP_PF] != "")
                    {
                        s1 = "PF=" + loads[i1, loads_PROP_PF];
                        if (loads[i1, loads_PROP_PF] != "") g.DrawString(s1, Font1, b0Black, object_x0 + 50, object_y0 + y0res + 20);
                    }

                    if (loads[i1, loads_PROP_P] != "")
                    {
                        P3f = double.Parse(loads[i1, loads_PROP_P]);
                        s1 = "P=" + P3f.ToString("#####0.0");
                        if (gph_direction == "N") g.DrawString(s1, Font1, b2Blue, object_x0 + 1, object_y0 + 52);
                        if (gph_direction == "W") g.DrawString(s1, Font1, b2Blue, object_x0 + 1, object_y0 + y0res + 50);
                    }
                    if (loads[i1, loads_PROP_Q] != "")
                    {
                        Q3f = double.Parse(loads[i1, loads_PROP_Q]);
                        s1 = "Q=" + Q3f.ToString("#####0.0");
                        if (gph_direction == "N") g.DrawString(s1, Font1, b2Blue, object_x0 + 50, object_y0 + 52);
                        if (gph_direction == "W") g.DrawString(s1, Font1, b2Blue, object_x0 + 50, object_y0 + y0res + 50);
                    }

                    double u = 0;
                    double fi = 0;
                    if ((loads[i1, loads_PROP_U1] != "") && (loads[i1, loads_PROP_U1] != "") && (loads[i1, loads_PROP_U1] != "")
                        && (loads[i1, loads_PROP_U1fi] != ""))
                    {
                        u = double.Parse(loads[i1, loads_PROP_U1]); s1 = "U1=";
                        if (u >= 1000) s1 += u.ToString("#00"); else s1 += u.ToString("#00.0");
                        fi = double.Parse(loads[i1, loads_PROP_U1fi]);
                        if (u >= 1000) s1 += "  "; else s1 += "   ";
                        if (fi > 0) s1 += "+";
                        s1 += fi.ToString("#00.0");
                        g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 62);
                        //g.DrawString(s1, Font1, b2Blue, object_x0 + 55, object_y0 + 72);

                        u = double.Parse(loads[i1, loads_PROP_U2]); s1 = "U2=";
                        if (u >= 1000) s1 += u.ToString("#00"); else s1 += u.ToString("#00.0");
                        fi = double.Parse(loads[i1, loads_PROP_U2fi]);
                        if (u >= 1000) s1 += "  "; else s1 += "   ";
                        if (fi > 0) s1 += "+";
                        if (u >= 1000) s1 += fi.ToString("#00"); else s1 += fi.ToString("#00.0");
                        g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 72);

                        u = double.Parse(loads[i1, loads_PROP_U3]); s1 = "U3=";
                        if (u >= 1000) s1 += u.ToString("#00"); else s1 += u.ToString("#00.0");
                        fi = double.Parse(loads[i1, loads_PROP_U3fi]);
                        if (u >= 1000) s1 += "  "; else s1 += "   ";
                        if (fi > 0) s1 += "+";
                        if (u >= 1000) s1 += fi.ToString("#00"); else s1 += fi.ToString("#00.0");
                        g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 82);

                    }

                    if (loads[i1, loads_PROP_U1fi] != "")
                    {
                    }
                }

                if (loads[i1, loads_PROP_sim_type] == "EV")
                {
                    int selected = 0;
                    if (loads[i1, loads_PROP_gph_selected] == "1") selected = 1; else selected = 0;

                    Image newImage1 = Image.FromFile(Grid_Projects_Path +@"/"+ GridMonk_Project + @"/EV1.jpg"); // 190 x 148, 40 x 31 sau 51 x 40
                    Image newImage2 = Image.FromFile(Grid_Projects_Path + @"/" + GridMonk_Project + @"/EV2.jpg"); // 169 x 154, 40 x 37 sau 44 x 40
                    Image newImage3 = Image.FromFile(Grid_Projects_Path + @"/" + GridMonk_Project + @"/EV3_TM3_V01.jpg"); // 222 x 154, 58 x 40
                    /*
                    Image newImage1 = Image.FromFile(@"E:\App\VStudio\GridMonK\PowerGrid\EV1.jpg"); // 190 x 148, 40 x 31 sau 51 x 40
                    Image newImage2 = Image.FromFile(@"E:\App\VStudio\GridMonK\PowerGrid\EV2.jpg"); // 169 x 154, 40 x 37 sau 44 x 40
                    Image newImage3 = Image.FromFile(@"E:\App\VStudio\GridMonK\PowerGrid\EV3_TM3_V01.jpg"); // 222 x 154, 58 x 40
                    */
                    if (loads[i1, loads_PROP_gph_direction] == "N") {
                        // b6LightPink, b13Yellow
                        if (selected == 1) g.FillRectangle(b6LightPink, object_x0, object_y0, object_dx_EV, object_dy_EV);
                        else g.FillRectangle(b6LightPink, object_x0, object_y0, object_dx_EV, object_dysmall_EV);

                        if (loads[i1, loads_PROP_sim_type_attr] == "EV1") e.Graphics.DrawImage(newImage1, object_x0 + 1, object_y0 + 1, 35, 19);
                        if (loads[i1, loads_PROP_sim_type_attr] == "EV2") e.Graphics.DrawImage(newImage2, object_x0 + 1, object_y0 + 1, 35, 19);
                        if (loads[i1, loads_PROP_sim_type_attr] == "EV3") e.Graphics.DrawImage(newImage3, object_x0 + 1, object_y0 + 1, 35, 19);

                        g.DrawLine(p1Black3, object_x0 + object_dx_EV - 9, object_y0 + 3 + 16, object_x0 + object_dx_EV - 1, object_y0 + 3 + 16); // Bus of the load
                        g.DrawLine(p1Black, object_x0 + object_dx_EV - 5, object_y0 + 6, object_x0 + object_dx_EV - 5, object_y0 + 20); // Bus of the load
                        if (loads[i1, loads_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                        g.FillRectangle(SolidBrush_crt, object_x0 + object_dx_EV - 9, object_y0 + 8, 9, 8); // Brk1
                        g.DrawEllipse(p5DarkBlue, object_x0 + object_dx_EV - 8, object_y0, 6, 6); // Pin_Bus1 
                    }
                    if (loads[i1, loads_PROP_gph_direction] == "S")
                    {
                        // b6LightPink, b13Yellow
                        if (selected == 1) g.FillRectangle(b6LightPink, object_x0, object_y0 - (object_dy_EV - object_dysmall_EV), object_dx_EV, object_dy_EV);
                        else g.FillRectangle(b6LightPink, object_x0, object_y0, object_dx_EV, object_dysmall_EV);

                        if (loads[i1, loads_PROP_sim_type_attr] == "EV1") e.Graphics.DrawImage(newImage1, object_x0 + 1, object_y0 + object_dysmall_EV - 21, 35, 19);
                        if (loads[i1, loads_PROP_sim_type_attr] == "EV2") e.Graphics.DrawImage(newImage2, object_x0 + 1, object_y0 + object_dysmall_EV - 21, 35, 19);
                        if (loads[i1, loads_PROP_sim_type_attr] == "EV3") e.Graphics.DrawImage(newImage3, object_x0 + 1, object_y0 + object_dysmall_EV - 21, 35, 19);

                        g.DrawLine(p1Black3, object_x0 + object_dx_EV - 9, object_y0 + object_dysmall_EV - 20, object_x0 + object_dx_EV - 1, object_y0 + object_dysmall_EV - 20); // Bus of the load
                        g.DrawLine(p1Black, object_x0 + object_dx_EV - 5, object_y0 + object_dysmall_EV - 6, object_x0 + object_dx_EV - 5, object_y0 + object_dysmall_EV - 20); // Bus of the load
                        if (loads[i1, loads_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                        g.FillRectangle(SolidBrush_crt, object_x0 + object_dx_EV - 9, object_y0 + object_dysmall_EV - 16, 9, 8); // Brk1
                        g.DrawEllipse(p5DarkBlue, object_x0 + object_dx_EV - 8, object_y0 + object_dysmall_EV - 7, 6, 6); // Pin_Bus1 
                    }

                    if (loads[i1, loads_PROP_name] != "")
                    {
                        s1 = "" + loads[i1, loads_PROP_name];
                        if (loads[i1, loads_PROP_gph_selected] == "1")
                        {
                            if (loads[i1, loads_PROP_gph_direction] == "N") g.DrawString(s1, Font1bold, b6Red, object_x0 + 1, object_y0 + 19);
                            if (loads[i1, loads_PROP_gph_direction] == "S") g.DrawString(s1, Font1bold, b6Red, object_x0 + 1, object_y0 + 19 - 46);
                        }
                        else {
                            if (loads[i1, loads_PROP_gph_direction] == "N") g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + 19);
                            if (loads[i1, loads_PROP_gph_direction] == "S") g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + 19 - 20);
                        }
                        //g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + 25);
                    }
                    if (selected == 1) if (loads[i1, loads_PROP_bus] != "")
                        {
                            string bus_display = loads[i1, loads_PROP_bus];
                            if (loads[i1, loads_PROP_bus].Contains("@")) bus_display = loads[i1, loads_PROP_bus].Remove(loads[i1, loads_PROP_bus].IndexOf('@'));
                            s1 = "Bs:" + bus_display;
                            if (loads[i1, loads_PROP_gph_direction] == "N") g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + 29);
                            if (loads[i1, loads_PROP_gph_direction] == "S") {
                                if (selected == 1) g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + 19 - 35);
                                else g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + 1);
                            }
                        }
                    if (selected == 1) if (loads[i1, loads_PROP_Pn] != "")
                        {
                            s1 = "Pn=" + loads[i1, loads_PROP_Pn];
                            if (loads[i1, loads_PROP_gph_direction] == "N") g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + 39);
                            if (loads[i1, loads_PROP_gph_direction] == "S")
                            {
                                if (selected == 1) g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + 19 - 25);
                                else g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + 11);
                            }
                        }
                    if (loads[i1, loads_PROP_P] != "")
                    {
                        P3f = double.Parse(loads[i1, loads_PROP_P]);
                        s1 = "P=" + P3f.ToString("#####0.0");
                        if (loads[i1, loads_PROP_gph_direction] == "N") g.DrawString(s1, Font1, b2Blue, object_x0 + 1, object_y0 + 29 + selected * 20);
                        else g.DrawString(s1, Font1, b2Blue, object_x0 + 1, object_y0 + 19 - 15 + (1 - selected) * 4);
                    }
                    if (loads[i1, loads_PROP_Q] != "")
                    {
                        Q3f = double.Parse(loads[i1, loads_PROP_Q]);
                        s1 = "Q=" + Q3f.ToString("#####0.0");
                        if (loads[i1, loads_PROP_gph_direction] == "N") g.DrawString(s1, Font1, b2Blue, object_x0 + 1, object_y0 + 39 + selected * 20);
                        else g.DrawString(s1, Font1, b2Blue, object_x0 + 1, object_y0 + 19 - 5 + (1 - selected) * 4);
                    }

                }

                if (default_xy == 1) obj_number++;
            }
        }

        /****************** Desenare obiecte de tip load si alte functii ale acestui obiect ******************/
        private void generators_properties_calculation(int nr_generator)
        {
            if ((generators[nr_generator, generators_PROP_x0] != "") && (generators[nr_generator, generators_PROP_y0] != ""))
            {
                // calculate pins position inside the line representation on the grid picture
                int x0 = int.Parse(generators[nr_generator, generators_PROP_x0]);
                int y0 = int.Parse(generators[nr_generator, generators_PROP_y0]);
                int pin1_x = 0, pin1_y = 0;
                if (generators[nr_generator, generators_PROP_gph_direction] == "N")
                {
                    pin1_x = x0 + object_dx / 2; pin1_y = y0 + 2;
                    generators[nr_generator, generators_PROP_pin1_x] = pin1_x.ToString();
                    generators[nr_generator, generators_PROP_pin1_y] = pin1_y.ToString();
                }
            }
        }

        /****************** Desenare obiecte de tip generators**************/
        private void Paint_generators(object sender, PaintEventArgs e)
        {
            string s1 = "", s2 = "";
            int obj_x = 0, obj_y = 0;

            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);

            // Paint generators
            for (int i1 = 0; i1 < generators_no; i1++)
            {
                int y0res = 0;
                int default_xy = 1;
                if ((generators[i1, generators_PROP_x0] == "") || (generators[i1, generators_PROP_y0] == ""))
                {
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    default_xy = 1;
                }
                else
                {
                    object_x0 = -X0_shift + int.Parse(generators[i1, generators_PROP_x0]);
                    object_y0 = -Y0_shift + int.Parse(generators[i1, generators_PROP_y0]);
                    default_xy = 0;
                }

                string gph_direction = generators[i1, generators_PROP_gph_direction];
                // draw the line symbol
                g.FillRectangle(b7LightGreen, object_x0, object_y0, object_dx, line_dy);

                if (gph_direction == "N")
                {
                    g.DrawLine(p1Black3, object_x0 + 20, object_y0 + 3 + 14, object_x0 + object_dx - 20, object_y0 + 3 + 14); // Bus of the generator
                    g.DrawLine(p1Black, object_x0 + object_dx / 2, object_y0 + 6, object_x0 + object_dx / 2, object_y0 + 18); // Bus of the load

                    if (generators[i1, generators_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                    g.FillRectangle(SolidBrush_crt, object_x0 + object_dx / 2 - 4, object_y0 + 7, 9, 8); // Brk1

                    g.DrawEllipse(p5DarkBlue, object_x0 + object_dx / 2 - 3, object_y0, 6, 6); // Pin_Bus1
                    y0res = 20;

                    //g.DrawLine(p1Black3, object_x0, object_y0 + 3, object_x0 + object_dx, object_y0 + 3);
                    //g.DrawEllipse(p5DarkBlue, object_x0, object_y0, 6, 6); // Pin_Bus1
                    //g.DrawEllipse(p5DarkBlue, object_x0 + object_dx / 2 - 3, object_y0, 6, 6); // Pin_Bus1
                    //g.DrawEllipse(p5DarkBlue, object_x0 + object_dx - 6, object_y0, 6, 6); // Pin_Bus1
                }
                if (gph_direction == "S")
                {
                    g.DrawLine(p1Black3, object_x0 + 20, object_y0 + line_dy - 18, object_x0 + object_dx - 20, object_y0 + line_dy - 18); // Bus of the generator
                    g.DrawLine(p1Black, object_x0 + object_dx / 2, object_y0 + line_dy - 6, object_x0 + object_dx / 2, object_y0 + line_dy - 18); // Bus of the load

                    if (generators[i1, generators_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                    g.FillRectangle(SolidBrush_crt, object_x0 + object_dx / 2 - 4, object_y0 + line_dy - 15, 9, 8); // Brk1

                    g.DrawEllipse(p5DarkBlue, object_x0 + object_dx / 2 - 3, object_y0 + line_dy - 6, 6, 6); // Pin_Bus1

                    //g.DrawLine(p1Black3, object_x0, object_y0 + line_dy - 3, object_x0 + object_dx, object_y0 + line_dy - 3);
                    //g.DrawEllipse(p5DarkBlue, object_x0, object_y0 + line_dy - 6, 6, 6); // Pin_Bus1
                    //g.DrawEllipse(p5DarkBlue, object_x0 + object_dx / 2 - 3, object_y0 + line_dy - 6, 6, 6); // Pin_Bus1
                    //g.DrawEllipse(p5DarkBlue, object_x0 + object_dx - 6, object_y0 + line_dy - 6, 6, 6); // Pin_Bus1
                    y0res = -1;
                }

                s1 = "Ge=" + generators[i1, generators_PROP_name];
                if (generators[i1, generators_PROP_gph_selected] == "1") g.DrawString(s1, Font1bold, b6Red, object_x0 + 15, object_y0 + y0res-2);
                else g.DrawString(s1, Font1, b0Black, object_x0 + 15, object_y0 + y0res-2);
                //s1 = "Bus=" + generators[i1, generators_PROP_bus]; g.DrawString(s1, Font1, b0Black, object_x0 + 6, object_y0 + 22);
                //s1 = "Ph=" + generators[i1, generators_PROP_phases]; g.DrawString(s1, Font1, b0Black, object_x0 + 6, object_y0 + 32);
                //s1 = "U=" + generators[i1, generators_PROP_voltage]; g.DrawString(s1, Font1, b0Black, object_x0 + 50, object_y0 + 32);

                double P3f = 0, P3f_mega = 0, force_mega = 0;
                if (generators[i1, generators_PROP_Pn] != "") {
                    P3f = double.Parse(generators[i1, generators_PROP_Pn]); P3f_mega = P3f / 1000;
                    if (Math.Abs(P3f) >= 10000)
                    {
                        s1 = "Pn=" + P3f_mega.ToString("#####0.0") + "M";
                        force_mega = 1;
                    }
                    else s1 = "Pn=" + P3f.ToString("#####0.00");
                    //g.DrawString(s1, Font1, b0Black, object_x0 + 6, object_y0 + 42);
                    if (generators[i1, generators_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + y0res + 20);
                    if (generators[i1, generators_PROP_gph_DrawType] == "G1S1") g.DrawString(s1, Font3, b0Black, object_x0 + 1, object_y0 + y0res + 9);
                }

                s1 = "Qn=" + generators[i1, generators_PROP_Qn];
                if (generators[i1, generators_PROP_Qn] != "")
                {
                    //g.DrawString(s1, Font1, b0Black, object_x0 + 6, object_y0 + 52);
                    if (generators[i1, generators_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, b0Black, object_x0 + 50, object_y0 + y0res + 20);
                    if (generators[i1, generators_PROP_gph_DrawType] == "G1S1") g.DrawString(s1, Font3, b0Black, object_x0 + 1, object_y0 + y0res + 22);
                }
                else
                {
                    s1 = "PF=" + generators[i1, generators_PROP_PF];
                    if (generators[i1, generators_PROP_PF] != "")
                    {
                        //g.DrawString(s1, Font1, b0Black, object_x0 + 6, object_y0 + 52);
                        if (generators[i1, generators_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, b0Black, object_x0 + 50, object_y0 + y0res + 20);
                        if (generators[i1, generators_PROP_gph_DrawType] == "G1S1") g.DrawString(s1, Font3, b0Black, object_x0 + 1, object_y0 + y0res + 22);
                    }
                }

                if (generators[i1, generators_PROP_P] != "")
                {
                    P3f = double.Parse(generators[i1, generators_PROP_P]); P3f_mega = P3f / 1000;
                    if (Math.Abs(P3f) >= 10000)
                    {
                        s1 = "P=" + P3f_mega.ToString("#####0.0") + "M";
                        force_mega = 1;
                    }
                    else s1 = "P=" + P3f.ToString("#####0.00");
                    //g.DrawString(s1, Font1, b2Blue, object_x0 + 6, object_y0 + 62);
                    if (generators[i1, generators_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, b2Blue, object_x0 + 1, object_y0 + 52);
                    if (generators[i1, generators_PROP_gph_DrawType] == "G1S1") g.DrawString(s1, Font3, b2Blue, object_x0 + 1, object_y0 + y0res + 35);
                }

                double Q3f = 0, Q3f_mega = 0;
                if (generators[i1, generators_PROP_Q] != "")
                {
                    Q3f = double.Parse(generators[i1, generators_PROP_Q]);
                    if ((Math.Abs(Q3f) >= 10000) || (force_mega == 1))
                    {
                        s1 = "Q=" + Q3f_mega.ToString("#####0.0") + "M";
                    }
                    else s1 = "Q=" + Q3f.ToString("#####0.00");

                    //g.DrawString(s1, Font1, b2Blue, object_x0 + 6, object_y0 + 72);
                    if (generators[i1, generators_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, b2Blue, object_x0 + 50, object_y0 + 52);
                    if (generators[i1, generators_PROP_gph_DrawType] == "G1S1") g.DrawString(s1, Font3, b2Blue, object_x0 + 1, object_y0 + y0res + 48);
                }

                double u = 0, u_kilo = 0;
                double fi = 0;
                if ((generators[i1, generators_PROP_U1] != "") && (generators[i1, generators_PROP_U2] != "") && (generators[i1, generators_PROP_U3] != "")
                    && (generators[i1, generators_PROP_U1fi] != ""))
                {
                    u = double.Parse(generators[i1, generators_PROP_U1]); u_kilo = u / 1000; s1 = "U1=";
                    if (u >= 1000) s1 += u_kilo.ToString("#00.00") + "k"; else s1 += u.ToString("#00.0");
                    fi = double.Parse(generators[i1, generators_PROP_U1fi]);
                    if (u >= 1000) s1 += "  "; else s1 += "   ";
                    if (generators[i1, generators_PROP_gph_DrawType] == "") { if (fi > 0) s1 += "+"; s1 += fi.ToString("#00.0"); }
                    s2 = fi.ToString("#0.00");
                    Font_crt = new System.Drawing.Font("Arial", 8);
                    if (generators[i1, generators_PROP_gph_DrawType] == "") g.DrawString(s1, Font_crt, b2Blue, object_x0 + 0, object_y0 + 62);
                    if (generators[i1, generators_PROP_gph_DrawType] == "G1S1")
                    {
                        g.DrawString(s1, Font3, b2Blue, object_x0 + 0, object_y0 + y0res + 61);
                        g.DrawString(s2, Font1, b2Blue, object_x0 + 64, object_y0 + y0res + 63);
                    }
                }

                    if (default_xy == 1) obj_number++;
            } // End painting generators

        }

        /****************** Desenare obiecte de tip interract **************/
        private void Paint_interracts(object sender, PaintEventArgs e)
        {
            string s1 = "";

            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);

            // Paint interracts
            for (int i1 = 0; i1 < interracts_no; i1++)
            {
                int default_xy = 1;
                if ((interracts[i1, nodes_PROP_x0] == "") || (interracts[i1, nodes_PROP_y0] == ""))
                {
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    default_xy = 1;
                }
                else
                {
                    object_x0 = -X0_shift + int.Parse(interracts[i1, interracts_PROP_x0]);
                    object_y0 = -Y0_shift + int.Parse(interracts[i1, interracts_PROP_y0]);
                    default_xy = 0;
                }
                s1 = interracts[i1, interracts_PROP_text];
                if (interracts[i1, interracts_PROP_type] == "button")
                {
                    int dx = 20;
                    if (interracts[i1, interracts_PROP_dx] != "") dx = int.Parse(interracts[i1, interracts_PROP_dx]);
                    else dx = s1.Length * 10;
                    g.FillRectangle(b8LightCoral, object_x0, object_y0, dx, 12);
                    g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + 0);
                }
                if (interracts[i1, interracts_PROP_type] == "value")
                {
                    int dx = 100;
                    if (interracts[i1, interracts_PROP_dx] != "") dx = int.Parse(interracts[i1, interracts_PROP_dx]);
                    else dx = (s1.Length + 50) * 10;
                    g.FillRectangle(b13Yellow, object_x0, object_y0, dx, 14);
                    if (interracts[i1, interracts_PROP_command].ToLower().Contains("name>global_pvs_factor") == true)
                        s1 += "= " + Global_PVs_factor.ToString("0.000");
                    if (interracts[i1, interracts_PROP_command].ToLower().Contains("name>global_loads_factor") == true)
                        s1 += "= " + Global_loads_factor.ToString("0.000");
                    if (interracts[i1, interracts_PROP_command].ToLower().Contains("name>grid_frequency") == true)
                        s1 += "= " + grid_frequency.ToString("0.000");
                    g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + 0);
                }

                //if (default_xy == 1) obj_number++;
            } // end painting interracts
        }

        /****************** Desenare obiecte de tip label **************/
        private void Paint_labels(object sender, PaintEventArgs e)
        {
            string s1 = "";

            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);

            // Paint labels
            for (int i1 = 0; i1 < labels_no; i1++)
            {
                int default_xy = 1;
                if ((labels[i1, labels_PROP_x0] == "") || (labels[i1, labels_PROP_y0] == ""))
                {
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    default_xy = 1;
                }
                else
                {
                    object_x0 = -X0_shift + int.Parse(labels[i1, labels_PROP_x0]);
                    object_y0 = -Y0_shift + int.Parse(labels[i1, labels_PROP_y0]);
                    default_xy = 0;
                }
                s1 = labels[i1, labels_PROP_text];
                int font = 8;
                if(labels[i1, labels_PROP_font] !="") font = int.Parse(labels[i1, labels_PROP_font]);
                Font Font_crt = new System.Drawing.Font("Arial", font);
                g.DrawString(s1, Font_crt, b0Black, object_x0 + 1, object_y0 + 0);
            } // end painting labels
        }

        private void Paint_measurements(object sender, PaintEventArgs e)
        {
            string s1 = "";

            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);

            // Paint interracts
            for (int i1 = 0; i1 < measurements_no; i1++)
            {
                int default_xy = 1;
                if ((measurements[i1, measurements_PROP_x0] == "") || (measurements[i1, measurements_PROP_y0] == ""))
                {
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    default_xy = 1;
                }
                else
                {
                    object_x0 = -X0_shift + int.Parse(measurements[i1, measurements_PROP_x0]);
                    object_y0 = -Y0_shift + int.Parse(measurements[i1, measurements_PROP_y0]);
                    default_xy = 0;
                }
                s1 = measurements[i1, measurements_PROP_text];
                if (measurements[i1, measurements_PROP_type] == "bargraph")
                {
                    int dx = 25;
                    if (measurements[i1, measurements_PROP_dx] != "") dx = int.Parse(measurements[i1, measurements_PROP_dx]);
                    else dx = 16;
                    int dy = 95;
                    if (measurements[i1, measurements_PROP_dy] != "") dy = int.Parse(measurements[i1, measurements_PROP_dy]);
                    else dy = 95;
                    // b13LightYellow b4LightBlue b2AliceBlue b11LightSalmon b8LightCoral b6LightPink b1White
                    g.FillRectangle(b1White, object_x0, object_y0, dx, dy);
                    g.DrawRectangle(p2LightGray, object_x0, object_y0, dx-1, dy);
                    g.FillRectangle(b6LightPink, object_x0, object_y0+dy, dx, dy/5); // zona valori negative pana la 20%
                    g.FillRectangle(b6LightPink, object_x0, object_y0-dy/5, dx, dy / 5); // zona valori peste 100%, pana la +20%
                    g.DrawString(s1, Font1, b0Black, object_x0, object_y0 - 14 - dy / 5);
                    g.DrawString("+20%", Font1, b0Black, object_x0 - 6, object_y0 - 20);
                    //g.DrawString("0", Font1, b0Black, object_x0 + 6, object_y0 + +dy);
                    g.DrawString("-20%", Font1, b0Black, object_x0-6, object_y0 + +dy +8);

                    int dymas = 95;
                    if(trafos[0, trafos_PROP_S] != "") dymas = int.Parse(trafos[0, trafos_PROP_S])* 95 / 630;
                    if((dymas>=0) && (dymas <= 95)) g.FillRectangle(b13Yellow, object_x0+2, object_y0 +dy - dymas, dx/4, dymas);
                    else if (dymas > 95) g.FillRectangle(b6Red, object_x0 + 2, object_y0 + dy - dymas, dx / 4, dymas);
                    else g.FillRectangle(b6Red, object_x0+2, object_y0 + dy + dymas, dx/4, -dymas);

                    dymas = 95;
                    if (trafos[0, trafos_PROP_P] != "") dymas = int.Parse(trafos[0, trafos_PROP_P]) * 95 / 630;
                    if (dymas >= 0) g.FillRectangle(b0Black, object_x0 + 2 + dx / 4, object_y0 + dy - dymas, dx / 4, dymas);
                    else g.FillRectangle(b6Red, object_x0 + 2 + dx / 4, object_y0 + dy, dx / 4, -dymas);

                    dymas = 95;
                    if (trafos[0, trafos_PROP_Q] != "") dymas = int.Parse(trafos[0, trafos_PROP_Q]) * 95 / 630;
                    if (dymas >= 0) g.FillRectangle(b7Green, object_x0 + 2 + dx * 2/4, object_y0 + dy - dymas, dx / 4, dymas);
                    else g.FillRectangle(b7Green, object_x0 + 2 + dx * 2 / 4, object_y0 + dy, dx / 4, -dymas);
                }

                //if (measurements[i1, measurements_PROP_type] == "phasor")
                //{
                    //Paint_phasors(sender, e, i1);
                //}
                //if (default_xy == 1) obj_number++;
            } // end painting interracts
        }

        private void Paint_graph_phasors(object sender, PaintEventArgs e)
        {
            string s1 = "";

            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            //GraphicsPath path_clip = new GraphicsPath();
            //path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            //Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            //e.Graphics.SetClip(region, CombineMode.Replace);

            // Paint phasors
            for (int i1 = 0; i1 < graph_phasors_no; i1++)
            {
                int default_xy = 1;
                if ((graph_phasors[i1, graph_phasors_PROP_x0] == "") || (graph_phasors[i1, graph_phasors_PROP_y0] == ""))
                {
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    default_xy = 1;
                }
                else
                {
                    object_x0 = int.Parse(graph_phasors[i1, graph_phasors_PROP_x0]);
                    object_y0 = int.Parse(graph_phasors[i1, graph_phasors_PROP_y0]);
                    default_xy = 0;
                }

                Paint_phasors(sender, e, i1);

            } // end painting graph_phasors
        }

        private void Paint_graph_sankeys(object sender, PaintEventArgs e)
        {
            string s1 = "";

            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            //GraphicsPath path_clip = new GraphicsPath();
            //path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            //Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            //e.Graphics.SetClip(region, CombineMode.Replace);

            // Paint phasors
            for (int i1 = 0; i1 < graph_sankeys_no; i1++)
            {
                int default_xy = 1;
                if ((graph_sankeys[i1, graph_sankeys_PROP_x0] == "") || (graph_sankeys[i1, graph_sankeys_PROP_y0] == ""))
                {
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    default_xy = 1;
                }
                else
                {
                    object_x0 = int.Parse(graph_sankeys[i1, graph_sankeys_PROP_x0]);
                    object_y0 = int.Parse(graph_sankeys[i1, graph_sankeys_PROP_y0]);
                    default_xy = 0;
                }

                Paint_sankeys(sender, e, i1);

            } // end painting graph_phasors
        }

        /****************** Desenare obiecte de tip node si alte functii ale acestui obiect ******************/
        private void nodes_properties_calculation()
        {
            for(int nr_node=0; nr_node< nodes_no; nr_node++)
            if ((nodes[nr_node, nodes_PROP_x0] != "") && (nodes[nr_node, nodes_PROP_y0] != ""))
            {
                // calculate pins position inside the line representation on the grid picture
                int x0 = int.Parse(nodes[nr_node, nodes_PROP_x0]);
                int y0 = int.Parse(nodes[nr_node, nodes_PROP_y0]);
                int pin1_x = 0, pin1_y = 0;
                // default pin1
                pin1_x = x0 + 5; pin1_y = y0 + 5;
                nodes[nr_node, nodes_PROP_pin1_x] = pin1_x.ToString();
                nodes[nr_node, nodes_PROP_pin1_y] = pin1_y.ToString();
                // specific pin1
                if (nodes[nr_node, nodes_PROP_draw_type] == "0")
                {
                    pin1_x = x0 + 5; pin1_y = y0 + 5;
                    nodes[nr_node, nodes_PROP_pin1_x] = pin1_x.ToString();
                    nodes[nr_node, nodes_PROP_pin1_y] = pin1_y.ToString();
                }
                if (nodes[nr_node, nodes_PROP_arrow] != "")
                {
                    pin1_x = x0 + 81; pin1_y = y0 + 5;
                    nodes[nr_node, nodes_PROP_pin1_x] = pin1_x.ToString();
                    nodes[nr_node, nodes_PROP_pin1_y] = pin1_y.ToString();
                }
            }
        }
        
        /****************** Desenare obiecte de tip nodes ******************/
        private void Paint_nodes(object sender, PaintEventArgs e)
        {
            string s1 = "";
            double U1 = 0, U1fi=0, U1_kilo = 0;

            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);

            int name_dx = 0, name_dy = 0;
            int U_dx = 0, U_dy = 0;

            // Paint nodes
            for (int i1 = 0; i1 < nodes_no; i1++)
            {
                int default_xy = 1;
                if ((nodes[i1, nodes_PROP_bus_name_x] != "") && (nodes[i1, nodes_PROP_bus_name_y] != ""))
                {
                    name_dx = int.Parse(nodes[i1, nodes_PROP_bus_name_x]);
                    name_dy = int.Parse(nodes[i1, nodes_PROP_bus_name_y]);
                }
                if ((nodes[i1, nodes_PROP_U_x] != "") && (nodes[i1, nodes_PROP_U_y] != ""))
                {
                    U_dx = int.Parse(nodes[i1, nodes_PROP_U_x]);
                    U_dy = int.Parse(nodes[i1, nodes_PROP_U_y]);
                }
                if ((nodes[i1, nodes_PROP_x0] == "") || (nodes[i1, nodes_PROP_y0] == ""))
                { 
                    // nodurile nu au desemnate coordonate x0 si y0, ele se deseneaza in partea de jos, parcate pentru operatii de alocare
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    default_xy = 1;

                    g.FillRectangle(b13LightYellow, object_x0, object_y0, object_dx*2/3, line_dy/2);
                    //g.DrawLine(p1Black3, object_x0 + 4, object_y0, object_x0 + 4, object_y0 + line_dy - 10);
                    s1 = "Nd=" + nodes[i1, nodes_PROP_name]; g.DrawString(s1, Font1, b0Black, object_x0 + 6, object_y0 + 12);
                    s1 = "Bs=" + nodes[i1, nodes_PROP_bus]; g.DrawString(s1, Font1, b0Black, object_x0 + 6, object_y0 + 22);
                }
                else
                { 
                    // nodurile au coordonate x0 si y0 rezultate din procesarea "nodes_metadata"
                    object_x0 = -X0_shift + int.Parse(nodes[i1, nodes_PROP_x0]);
                    object_y0 = -Y0_shift + int.Parse(nodes[i1, nodes_PROP_y0]);
                    default_xy = 0;

                    if (nodes[i1, nodes_PROP_draw_type] == "") { 
                        g.FillRectangle(b13Yellow, object_x0, object_y0, object_dx-16, 12);
                        g.FillEllipse(b6Red, object_x0, object_y0, 10, 10); // Pin_Bus stanga
                        g.DrawEllipse(p5DarkBlue, object_x0, object_y0, 10, 10); // Pin_Bus stanga
                        if (nodes[i1, nodes_PROP_arrow] == "")
                        {
                            g.FillEllipse(b6Red, object_x0 + object_dx - 27, object_y0, 10, 10); // Pin_Bus dreapta
                            g.DrawEllipse(p5DarkBlue, object_x0 + object_dx - 27, object_y0, 10, 10); // Pin_Bus_dreapta 
                        }
                        else
                        {
                            s1 = ">>" + nodes[i1, nodes_PROP_arrow];
                            g.DrawString(s1, Font1bold, b6Red, object_x0 + 60, object_y0);
                        }
                    }

                    if (nodes[i1, nodes_PROP_draw_type] == "0")
                    {
                        g.FillRectangle(b13Yellow, object_x0, object_y0, 12, 12);
                        g.FillEllipse(b6Red, object_x0, object_y0, 10, 10); // Pin_Bus stanga
                    }
                    // Draw the name of the node
                    int l1 = nodes[i1, nodes_PROP_bus].Length; if (l1 > 4) l1 = 0;
                    s1 = "" + nodes[i1, nodes_PROP_bus_name];
                    g.DrawString(s1, Font1, b0Black, object_x0 + 10 +l1*2 + name_dx, object_y0+ name_dy);
                    // Write U and fi for the node
                    if (nodes[i1, nodes_PROP_U1] != "") {
                        U1 = double.Parse(nodes[i1, nodes_PROP_U1]); U1_kilo = U1 / 1000;
                        if (Math.Abs(U1) >= 1000)
                        {
                            s1 = "U1= " + U1_kilo.ToString("#####0.00") + "k";
                        }
                        else s1 = "U1= " + U1.ToString("#00.00");
                        if (nodes[i1, nodes_PROP_draw_U1] == "1")
                        {
                            //if(nodes[i1, nodes_PROP_Font1]=="") g.DrawString(s1, Font1, b2Blue, object_x0 + 8, object_y0 - 13);
                            if (nodes[i1, nodes_PROP_Font1] == "11b") g.DrawString(s1, Font11b, b2Blue, object_x0 + 8 + U_dx, object_y0 - 13 + U_dy);
                            else if (nodes[i1, nodes_PROP_Font1] == "12b") g.DrawString(s1, Font12b, b2Blue, object_x0 + 8 + U_dx, object_y0 - 13 + U_dy);
                            else g.DrawString(s1, Font1, b2Blue, object_x0 + 8 + U_dx, object_y0 - 13 + U_dy);
                        }
                    }
                    if (nodes[i1, nodes_PROP_U1fi] != "")
                    {
                        U1fi = double.Parse(nodes[i1, nodes_PROP_U1fi]);
                        s1 = U1fi.ToString("##0.00");
                        if (nodes[i1, nodes_PROP_draw_U1fi] == "1") g.DrawString("U1ϕ = " + s1, Font1, b2Blue, object_x0 + 8 + U_dx, object_y0 - 23 + U_dy);
                    }
                }


                if (default_xy == 1) obj_number++;
            } // end painting nodes
        }


        private void Paint_polylines(object sender, PaintEventArgs e)
        {
            string s1 = "";
            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);

            for (int i = 0; i < polylines.Count(); i++)
            {
                List<List<double>> crt = polylines[i].lines;
                if (crt.Count() >= 2)
                {
                    PointF[] drawn = new PointF[crt.Count()];
                    for (int j = 0; j < crt.Count(); j++)
                    {
                        drawn[j].X = (float)crt[j][0];
                        drawn[j].Y = (float)crt[j][1];
                    }
                    g.DrawLines(polylines[i].penStyle, drawn);
                }
            }

        }

        private void Paint_polylines_from_nodes(object sender, PaintEventArgs e)
        {
            string s1 = "";
            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            //Point[] polyPoints = {
            //    new Point(100, 100),
            //    new Point(500, 100),
            //    new Point(500, 500),
            //    new Point(100, 500)};
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(polyPoints_clip_scheme_zone);
            // Construct a region based on the path.
            Region region = new Region(path);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);
            // Clipping the plygones end lines

            for (int n1 = 0; n1 < nodes_no; n1++)
            {
                List<Poly> crtpolys = new List<Poly>();

                List<List<List<double>>> polys;
                polys = new List<List<List<double>>>();
                if (nodes[n1, nodes_PROP_plylines] != "")
                {
                    var serializer = new JsonSerializer();
                    serializer.Populate(new JsonTextReader(new StringReader(nodes[n1, nodes_PROP_plylines])), polys);
                    int idx = 0;
                    //List<List<Double>> lns_shifted;
                    foreach (List<List<Double>> lns in polys)
                    {
                        Poly np = new Poly(nodes[n1, nodes_PROP_name] + "." + idx.ToString());
                        int pos_in_lns = 0;
                        foreach (List<Double> lnsx in lns)
                        {
                            // lnsx[0] = lns[0][0]; // - (double)X0_shift;
                            //lns[pos_in_lns][0] += -X0_shift;
                            lnsx[0] += -X0_shift;
                            lnsx[1] += -Y0_shift;
                            pos_in_lns++;
                        }
                        np.lines = lns;
                        np.penStyle = new Pen(Color.Black, 1); // valoare default pentru polyline
                        // deseneaza polylines functie de tensiunea de nod
                        if (nodes[n1, loads_PROP_voltage] != "")
                        {
                            double Nominal_voltage = double.Parse(nodes[n1, loads_PROP_voltage]);
                            double Vmax = Nominal_voltage * 1.1 / 1.73205;
                            double Vmin = Nominal_voltage * 0.9 / 1.73205;
                        }
                        if (nodes[n1, nodes_PROP_U1] != "")
                        {
                            // nodes[n1, nodes_PROP_U_source_object_avail_U_meas] = "1";
                            if ((double.Parse(nodes[n1, nodes_PROP_U1]) < (230.94 * 0.9)) &&
                                (double.Parse(nodes[n1, nodes_PROP_U1]) > 40.0)) np.penStyle = new Pen(Color.Red, 1);
                            if (double.Parse(nodes[n1, nodes_PROP_U1]) < 40.0) // lipsa tensiune
                            {
                                DateTime t1 = DateTime.Now;
                                if ((t1.Second % 2) == 0) np.penStyle = new Pen(Color.Red, 1);
                                else np.penStyle = new Pen(Color.White, 4);
                            }

                            if ((double.Parse(nodes[n1, nodes_PROP_U1]) > (230.94 * 1.1)) && (double.Parse(nodes[n1, nodes_PROP_U1]) < 1000.0))
                                np.penStyle = new Pen(Color.Red, 4);
                            if (double.Parse(nodes[n1, nodes_PROP_U1]) > 1000)
                                np.penStyle = new Pen(Color.Blue, 2);

                            if (nodes[n1, nodes_PROP_U_source_object_avail_U_meas] == "0") np.penStyle = new Pen(Color.Gray, 1);

                            if (nodes[n1, nodes_PROP_gph_selected] == "1") np.penStyle = new Pen(Color.Green, 3);
                        }
                        crtpolys.Add(np);
                        idx++;
                    }
                }
                // Acum se deseneaza polyline-urile create temporar
                for (int i = 0; i < crtpolys.Count(); i++)
                {
                    List<List<double>> crt = crtpolys[i].lines;
                    if (crt.Count() >= 2)
                    {
                        PointF[] drawn = new PointF[crt.Count()];
                        for (int j = 0; j < crt.Count(); j++)
                        {
                            drawn[j].X = (float)crt[j][0];
                            drawn[j].Y = (float)crt[j][1];
                        }
                        g.DrawLines(crtpolys[i].penStyle, drawn);
                    }
                }

                // ***** new function related to auto-draw of wires between pins of the node and lines, loads, generator ***** //
                // The function works only if the variable _GridMonK_nodes_wires_connection = "auto"
                int x1, y1, x2, y2;
                if(_GridMonK_nodes_wires_connection=="auto") { 
                    // Lines to nodes
                    for (int l1 = 0; l1 < lines_no; l1++) { 
                    if (lines[l1, lines_PROP_bus1] == nodes[n1, nodes_PROP_bus])
                        if ((lines[l1, lines_PROP_pin1_x] != "") && (nodes[n1, nodes_PROP_x0] != ""))
                        {
                            x1 = int.Parse(lines[l1, lines_PROP_pin1_x]);
                            y1 = int.Parse(lines[l1, lines_PROP_pin1_y]);
                            x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x]);
                            y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y]);
                            g.DrawLine(p1Black, x1, y1, x2, y2);
                        }
                        if (lines[l1, lines_PROP_bus2] == nodes[n1, nodes_PROP_bus])
                        if ((lines[l1, lines_PROP_pin2_x] != "") && (nodes[n1, nodes_PROP_x0] != ""))
                        {
                            x1 = int.Parse(lines[l1, lines_PROP_pin2_x]);
                            y1 = int.Parse(lines[l1, lines_PROP_pin2_y]);
                            x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x]);
                            y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y]);
                            g.DrawLine(p1Black, x1, y1, x2, y2);
                        }
            
                    }
                    // Loads to nodes 
                    for (int l1 = 0; l1 < loads_no; l1++)
                    {
                        if (loads[l1, loads_PROP_bus] == nodes[n1, nodes_PROP_bus])
                            if ((loads[l1, loads_PROP_pin1_x] != "") && (nodes[n1, nodes_PROP_x0] != ""))
                            {
                                x1 = int.Parse(loads[l1, loads_PROP_pin1_x]);
                                y1 = int.Parse(loads[l1, loads_PROP_pin1_y]);
                                x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x]);
                                y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y]);
                                g.DrawLine(p1Black, x1, y1, x2, y2);
                            }
                    }
                    // Generators to nodes 
                    for (int l1 = 0; l1 < generators_no; l1++)
                    {
                        if (generators[l1, generators_PROP_bus] == nodes[n1, nodes_PROP_bus])
                            if ((generators[l1, generators_PROP_pin1_x] != "") && (nodes[n1, nodes_PROP_x0] != ""))
                            {
                                x1 = int.Parse(generators[l1, generators_PROP_pin1_x]);
                                y1 = int.Parse(generators[l1, generators_PROP_pin1_y]);
                                x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x]);
                                y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y]);
                                g.DrawLine(p1Black, x1, y1, x2, y2);
                            }
                    }
                }
            }

        }

        private void Paint_EVs(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);

            // Create image.
            Image newImage1 = Image.FromFile(Grid_Projects_Path + @"/" + GridMonk_Project + @"\EV1.jpg"); // 190 x 148, 40 x 31 sau 51 x 40
                Image newImage2 = Image.FromFile(Grid_Projects_Path + @"/" + GridMonk_Project + @"\EV2.jpg"); // 169 x 154, 40 x 37 sau 44 x 40
                Image newImage3 = Image.FromFile(Grid_Projects_Path + @"/" + GridMonk_Project + @"\EV3_TM3_V01.jpg"); // 222 x 154, 58 x 40

                e.Graphics.DrawImage(newImage1, 200, 100, 51, 40);
                e.Graphics.DrawImage(newImage2, 260, 100, 44, 40);
                e.Graphics.DrawImage(newImage3, 310, 100, 114, 40);

        }

        private void Paint_Pie(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            string obj_type = "";

            for (int i1 = 0; i1 < graph_pies_no; i1++)
            {
                int value_1 = 0;
                int value_2 = 0;
                int value_3 = 0;
                double value_1d = 0;
                double value_2d = 0;
                double value_3d = 0;

                int value_max = 100; // value max, at right part fo the pie 
                int value_min = -100; // value min, at left part fo the pie
                int value_max_norm = 100; // normal value max, at right part fo the pie 
                int value_min_norm = -100; // normal value min, at left part fo the pie
                int obj_number = -1;
                int value_center = 0;
                string pie_meas_type = "";
                obj_number = int.Parse(graph_pies[i1, graph_pies_PROP_number]);
                obj_type = graph_pies[i1, graph_pies_PROP_obj];

                pie_meas_type = graph_pies[i1, graph_pies_PROP_meas_type];
                if (graph_pies[i1, graph_pies_PROP_max] != "")
                {
                    value_max = int.Parse(graph_pies[i1, graph_pies_PROP_max]);
                }
                if (graph_pies[i1, graph_pies_PROP_min] != "")
                {
                    value_min = int.Parse(graph_pies[i1, graph_pies_PROP_min]);
                }
                if (pie_meas_type == "PQ")
                    {
                    if(obj_type=="line") { 
                        if (lines[obj_number, lines_PROP_P] != "") {
                            value_1d = -double.Parse(lines[obj_number, lines_PROP_P]);
                            value_1 = (int)value_1d;//.Parse(lines[16, lines_PROP_P]);
                        };
                        if (lines[obj_number, lines_PROP_Q] != "")
                        {
                            value_2d = -double.Parse(lines[obj_number, lines_PROP_Q]);
                            value_2 = (int)value_2d;//.Parse(lines[16, lines_PROP_Q]);
                        };
                    }
                }
                if (pie_meas_type == "3U")
                {
                    if (obj_type == "load")
                    {
                        if (loads[obj_number, loads_PROP_U1] != "")
                        {
                            value_1d = double.Parse(loads[obj_number, loads_PROP_U1]);
                            value_1 = (int)value_1d;//.Parse(lines[16, lines_PROP_P]);
                        };
                        if (loads[obj_number, loads_PROP_U2] != "")
                        {
                            value_2d = double.Parse(loads[obj_number, loads_PROP_U2]);
                            value_2 = (int)value_2d;//.Parse(lines[16, lines_PROP_Q]);
                        };
                        if (loads[obj_number, loads_PROP_U3] != "")
                        {
                            value_3d = double.Parse(loads[obj_number, loads_PROP_U3]);
                            value_3 = (int)value_3d;//.Parse(lines[16, lines_PROP_Q]);
                        };
                    }
                    if (obj_type == "generator")
                    {
                        if (generators[obj_number, generators_PROP_U1] != "")
                        {
                            value_1d = double.Parse(generators[obj_number, generators_PROP_U1]);
                            value_1 = (int)value_1d;//.Parse(lines[16, lines_PROP_P]);
                        };
                        if (generators[obj_number, generators_PROP_U2] != "")
                        {
                            value_2d = double.Parse(generators[obj_number, generators_PROP_U2]);
                            value_2 = (int)value_2d;//.Parse(lines[16, lines_PROP_Q]);
                        };
                        if (generators[obj_number, generators_PROP_U3] != "")
                        {
                            value_3d = double.Parse(generators[obj_number, generators_PROP_U3]);
                            value_3 = (int)value_3d;//.Parse(lines[16, lines_PROP_Q]);
                        };
                    }
                }
                if (pie_meas_type == "3I")
                {
                    if (lines[obj_number, loads_PROP_I1] != "")
                    {
                        value_1d = double.Parse(lines[obj_number, lines_PROP_I1]);
                        value_1 = (int)value_1d;
                    };
                    if (loads[obj_number, loads_PROP_U2] != "")
                    {
                        value_2d = double.Parse(lines[obj_number, lines_PROP_I2]);
                        value_2 = (int)value_2d;
                    };
                    if (loads[obj_number, loads_PROP_U3] != "")
                    {
                        value_3d = double.Parse(lines[obj_number, lines_PROP_I3]);
                        value_3 = (int)value_2d;
                    };
                }

                if (value_1 > value_max) value_1 = value_max;
                if (value_1 < value_min) value_1 = value_min;
                if (value_2 > value_max) value_2 = value_max;
                if (value_2 < value_min) value_2 = value_min;
                if (value_3 > value_max) value_3 = value_max;
                if (value_3 < value_min) value_3 = value_min;
                value_center = (int)(1.0 * (value_max + value_min) / 2);

                int x0 = -X0_shift + int.Parse(graph_pies[i1, graph_pies_PROP_x0]); //1220;
                int y0 = -Y0_shift + int.Parse(graph_pies[i1, graph_pies_PROP_y0]); // 400;
                int diam_max = 120;
                int diam_max_y = 120;
                int text_type = 1;

                if (pie_meas_type == "PQ") { 
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
                    if (obj_type == "line") g.DrawString("Ln=" + lines[obj_number, lines_PROP_name], Font1, b5DarkBlue, x0 + diam_max / 2 - 30, y0 - 15);
                    if (obj_type == "load") g.DrawString("Ld=" + loads[obj_number, loads_PROP_name], Font1, b5DarkBlue, x0 + diam_max / 2 - 30, y0 - 15);
                    if (obj_type == "generator") g.DrawString("Gen=" + generators[obj_number, generators_PROP_name], Font1, b5DarkBlue, x0 + diam_max / 2 - 30, y0 - 15);
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
                        (int)(2.0 * 90 * (value_1 - value_min) / (value_max-value_min))); // measurement 1
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
                    g.DrawString("U3=" + value_2d.ToString("#0.00"), Font1, b5DarkBlue, x0 + diam_max / 3, y0 + diam_max / 2 + 12);

                    g.DrawString("" + value_min.ToString("###0"), Font1, b5DarkBlue, x0 + 23, y0 + diam_max / 2 - 12);
                    g.DrawString(value_max.ToString("###0"), Font1, b5DarkBlue, x0 + diam_max - 50, y0 + diam_max / 2 - 12);
                    //s1 = 
                    g.DrawString(value_center.ToString(), Font1, b5DarkBlue, x0 + diam_max / 2 - 12, y0 + 20);
                    if (obj_type == "load") g.DrawString("Ld=" + loads[obj_number, loads_PROP_name], Font1, b5DarkBlue, x0 + diam_max / 2 - 30, y0 - 15);
                    if (obj_type == "generator") g.DrawString("Gen=" + generators[obj_number, generators_PROP_name], Font1, b5DarkBlue, x0 + diam_max / 2 - 30, y0 - 15);
                }
                if (pie_meas_type == "3I")
                {
                    SolidBrush sb1 = new SolidBrush(Color.Azure), sb2 = new SolidBrush(Color.Aquamarine), sb3 = new SolidBrush(Color.Aqua);
                    e.Graphics.FillPie(sb2, x0, y0, diam_max, diam_max_y, -180, 180); // b4LightBlue, b13LightYellow, b9Lime, b3LightGray
                    e.Graphics.FillPie(b6Red, x0, y0, diam_max, diam_max_y, -180,
                        (int)(2.0 * 90 * (value_1 - value_min) / (value_max - value_min))); // measurement 1
                    e.Graphics.FillPie(b7Green, x0 + 6, y0 + 6, diam_max - 12, diam_max_y - 12, -180,     // b7Green, b13Yellow
                        (int)(2.0 * 90 * (value_2 - value_min) / (value_max - value_min))); // measurement 1
                    e.Graphics.FillPie(b2Blue, x0 + 12, y0 + 12, diam_max - 24, diam_max_y - 24, -180,
                        (int)(2.0 * 90 * (value_3 - value_min) / (value_max - value_min))); // measurement 1
                    e.Graphics.FillPie(b1White, x0 + 18, y0 + 18, diam_max - 36, diam_max - 36, -180, 180);
                    e.Graphics.DrawArc(p1Black, x0, y0, diam_max, diam_max, -180, 180);
                    e.Graphics.DrawArc(p1Black, x0 + 18, y0 + 18, diam_max - 36, diam_max - 36, -180, 180);
                    e.Graphics.DrawLine(p1Black, x0 + diam_max / 2, y0, x0 + diam_max / 2, y0 + 20);
                    e.Graphics.DrawLine(p1Black, x0, y0 + diam_max / 2, x0 + 20, y0 + diam_max / 2);
                    e.Graphics.DrawLine(p1Black, x0 + diam_max, y0 + diam_max / 2, x0 + diam_max - 20, y0 + diam_max / 2);

                    g.DrawString("I1=" + value_1d.ToString("#0.00"), Font1, b6Red, x0 - 2, y0 + diam_max / 2 + 2); // 
                    g.DrawString("I2=" + value_2d.ToString("#0.00"), Font1, b7Green, x0 + diam_max / 2, y0 + diam_max / 2 + 2);
                    g.DrawString("I3=" + value_2d.ToString("#0.00"), Font1, b5DarkBlue, x0 + diam_max / 3, y0 + diam_max / 2 + 12);

                    g.DrawString("" + value_min.ToString("###0"), Font1, b5DarkBlue, x0 + 23, y0 + diam_max / 2 - 12); // valoare de mijloc
                    g.DrawString(value_max.ToString("###0"), Font1, b5DarkBlue, x0 + diam_max - 50, y0 + diam_max / 2 - 12);
                    //s1 = 
                    g.DrawString(value_center.ToString(), Font1, b5DarkBlue, x0 + diam_max / 2 - 12, y0 + 20);
                    g.DrawString("Ln=" + lines[obj_number, lines_PROP_name], Font1, b5DarkBlue, x0 + diam_max / 2 - 30, y0 - 15);
                }
            }
        }


        int wnd_pos_x_crt = 0, wnd_pos_y_crt = 0;
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //this..MaximizeBox;

            DateTime t1 = DateTime.Now;
            string st1 = ">> Grid compute\n";
            int t1s = t1.Second, t1ms = t1.Millisecond;

            if (checkBox_expand.Checked == true) expand_obj_gph = 1; else expand_obj_gph = 0;

            Graphics g = e.Graphics;
            obj_number = 0;
            string s1 = "";
            //int obj_x = 0, obj_y = 0;

            //int Nr_X_wnd = 5, Nr_Y_wnd = 9;
            int Nr_X_wnd = 3, Nr_Y_wnd = 5, k1=16, k2=10; 
            Pen b1 = new Pen(Color.Black);
            // Draw the small version of the complete area, having 2x on horisontal and 3x on vertical
            g.DrawRectangle(b1, 6, 85, 5 + Electrical_scheme_zone_delta_X0/50 * Nr_X_wnd *k1 /k2, 
                                   7 + Electrical_scheme_zone_delta_Y0/50 * Nr_Y_wnd * k1 / k2);
            b1 = new Pen(Color.LightBlue);
            for (int i1=0; i1<= Nr_X_wnd; i1++)
                g.DrawLine(b1, 6 + 2 + Electrical_scheme_zone_delta_X0 / 50 * i1 * k1 / k2, 85 +2 , 
                               6 + 2 + Electrical_scheme_zone_delta_X0 / 50 * i1 * k1 / k2, 
                               85 + 2+ Electrical_scheme_zone_delta_Y0 / 50 * Nr_Y_wnd * k1 / k2);
            for (int i1 = 0; i1 <= Nr_Y_wnd; i1++)
                g.DrawLine(b1, 6 + 2, 85 + 3 + Electrical_scheme_zone_delta_Y0 / 50 * i1 * k1 / k2,
                               6 + 2 + Electrical_scheme_zone_delta_X0 / 50 * Nr_X_wnd * k1 / k2,
                               85 + 3 + Electrical_scheme_zone_delta_Y0 / 50 * i1 * k1 / k2);
            // current rectangle position
            b1 = new Pen(Color.Red,3);
            g.DrawRectangle(b1, 6 + 2 + Electrical_scheme_zone_delta_X0 / 50 * (wnd_pos_x_crt + Nr_X_wnd/2) * k1 / k2,
                           85 + 3 + Electrical_scheme_zone_delta_Y0 / 50 * (wnd_pos_y_crt + Nr_Y_wnd/2) * k1 / k2,
                           Electrical_scheme_zone_delta_X0 / 50 * k1 / k2, Electrical_scheme_zone_delta_Y0 / 50 * k1 / k2);

            b1 = new Pen(Color.LightBlue);
            //g.DrawRectangle(b1, 185, 50, 1348, 610); // Draw the complete area 1534, 649
            g.DrawRectangle(b1, Electrical_scheme_zone_X0_start, Electrical_scheme_zone_Y0_start,
                Electrical_scheme_zone_delta_X0, Electrical_scheme_zone_delta_Y0); // Draw the complete area 1534, 649


            try
            {
                if (Global_Gph_Info[0, Global_Gph_Info_PROP_background_image] != "")
                {
                    string image_file = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + Global_Gph_Info[0, Global_Gph_Info_PROP_background_image];
                    Image backgndImage1 = Image.FromFile(image_file); // 222 x 154, 58 x 40
                    e.Graphics.DrawImage(backgndImage1, Electrical_scheme_zone_X0_start, Electrical_scheme_zone_Y0_start,
                        Electrical_scheme_zone_delta_X0, Electrical_scheme_zone_delta_Y0);
                }
            }
            catch { }

            Paint_lines(sender, e);
            Paint_trafos(sender, e);
            Paint_loads(sender, e); // loads contain also storage and EV
            Paint_generators(sender, e);
            nodes_properties_calculation(); Paint_nodes(sender, e);
            Paint_labels(sender, e);
            Paint_interracts(sender, e);
            Paint_SimpleGph(sender, e);
            //Paint_polylines(sender, e);
            Paint_polylines_from_nodes(sender, e);
            Paint_measurements(sender, e);
            Paint_graph_phasors(sender, e);
            Paint_Pie(sender, e);
            Paint_Smart_Meter(sender, e);
            Paint_PMU(sender, e);
            //Paint_EVs(sender, e);
            Paint_console_Training(sender, e);
            Paint_graph_sankeys(sender, e);

            t1 = DateTime.Now;
            int t2s = t1.Second, t2ms = t1.Millisecond; // dupa generate_output_dss()
            int dt2_msec = t2s * 1000 + t2ms - t1s * 1000 - t1ms;
            if (dt2_msec < 0) dt2_msec = dt2_msec + 60000;
            textBox_GUI_dynamics.Text = dt2_msec.ToString() + "ms";
        }

    }
}