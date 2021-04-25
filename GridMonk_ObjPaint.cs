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
        double zoom = 1;

        // Clipping the graphics of scheme zone with this resctangle
        Point[] polyPoints_clip_scheme_zone = {
                new Point(Electrical_scheme_zone_X0_start, Electrical_scheme_zone_Y0_start),
                new Point(Electrical_scheme_zone_X0_start + Electrical_scheme_zone_delta_X0,
                    Electrical_scheme_zone_Y0_start),
                new Point(Electrical_scheme_zone_X0_start + Electrical_scheme_zone_delta_X0,
                    Electrical_scheme_zone_Y0_start + Electrical_scheme_zone_delta_Y0),
                new Point(Electrical_scheme_zone_X0_start, Electrical_scheme_zone_Y0_start + Electrical_scheme_zone_delta_Y0)};


        /****************** Desenare obiecte de tip line si alte functii ale acestui obiect ******************/
        private void Lines_properties_calculation(int nr_line)
        {
            if ((lines[nr_line, lines_PROP_x0] != "") && (lines[nr_line, lines_PROP_y0] != "")) {
                // calculate pins position inside the line representation on the grid picture
                int x0 = int.Parse(lines[nr_line, lines_PROP_x0]);
                int y0 = int.Parse(lines[nr_line, lines_PROP_y0]);
                int pin1_x = 0, pin1_y = 0, pin1_x0 = 0, pin1_y0 = 0, pin2_x = 0, pin2_y = 0, pin2_x0 = 0, pin2_y0 = 0;
                if ((lines[nr_line, lines_PROP_gph_DrawType] == "") || (lines[nr_line, lines_PROP_gph_DrawType] == "L1S0")
                    || (lines[nr_line, lines_PROP_gph_DrawType] == "L0S1"))
                if ((lines[nr_line, lines_PROP_gph_direction] == "N") || (lines[nr_line, lines_PROP_gph_direction] == "W")
                         || (lines[nr_line, lines_PROP_gph_direction] == "E"))
                {
                    // extend line terminals with polylines at 90 degree
                    int poly_x_final1 = 0, poly_y_final1 = 0;
                    if (lines[lines_no, lines_PROP_npoly1_xy] != "") // polylines connected to terminal 1 of the line aer defined
                    { // !!npoly1_xy={0,10,20,30,40,0}
                        string s1p = lines[lines_no, lines_PROP_npoly1_xy];
                        s1p = s1p.Replace("{", ""); s1p = s1p.Replace("}", "");
                        char[] delimiterChars1 = {','};
                        string[] line1 = s1p.Split(delimiterChars1);
                        for(int xy=0;  xy<line1.Length-1; xy++)
                        {
                            int val = int.Parse(line1[xy]);
                            if ((xy % 2) == 0) // variabila x
                            {
                                poly_x_final1 += val;
                            //    lines_npolys[lines_no, xy, line_Terminal_1, 0] = poly_x_final1; // x
                            //    lines_npolys[lines_no, xy, line_Terminal_1, 1] = poly_y_final1; // y
                            }
                            else // variabila y
                            {
                                poly_y_final1 += int.Parse(line1[xy]);
                            }
                            lines_npolys[lines_no, xy, line_Terminal_1, 0] = poly_x_final1; // x
                            lines_npolys[lines_no, xy, line_Terminal_1, 1] = poly_y_final1; // y
                            }
                            if (lines[nr_line, lines_PROP_gph_direction] == "W")
                            {
                                pin1_x0 = x0 + 6; pin1_y0 = y0 + 2;
                                pin1_x = x0 + 6 + poly_x_final1; pin1_y = y0 + 2 + poly_y_final1;
                                pin2_x0 = x0 + 6; pin2_y0 = y0 + 2;
                                pin2_x = x0 + 6; pin2_y = y0 + 2 + line_dy - 2;
                            }
                            if (lines[nr_line, lines_PROP_gph_direction] == "N")
                            {
                                pin1_x0 = x0 + 2; pin1_y0 = y0 + 4;
                                pin1_x = x0 + 2 + poly_x_final1; pin1_y = y0 + 4 + poly_y_final1;
                                pin2_x = x0 + object_dx - 4; pin2_y = y0 + 4 - 0;
                            }
                            if (lines[nr_line, lines_PROP_gph_direction] == "E")
                            {
                                pin1_x0 = x0 + object_dx - 6; pin1_y0 = y0 + 2;
                                pin1_x = x0 + object_dx - 6 + poly_x_final1; pin1_y = y0 + 2 + poly_y_final1;
                                pin2_x = x0 + object_dx - 4; pin2_y = y0 + line_dy + 4 - 2;
                            }
                            lines[nr_line, lines_PROP_pin1_x] = pin1_x.ToString();
                            lines[nr_line, lines_PROP_pin1_y] = pin1_y.ToString();
                            lines[nr_line, lines_PROP_pin1_x0] = pin1_x0.ToString();
                            lines[nr_line, lines_PROP_pin1_y0] = pin1_y0.ToString();
                        } else
                        {
                            if (lines[nr_line, lines_PROP_gph_direction] == "W")
                            {
                                pin1_x0 = x0 + 6; pin1_y0 = y0 + 2;
                                pin1_x = x0 + 6 + poly_x_final1; pin1_y = y0 + 2 + poly_y_final1;
                                pin2_x0 = x0 + 6; pin2_y0 = y0 + 2;
                                pin2_x = x0 + 6; pin2_y = y0 + 2 + line_dy - 2;
                            }
                            if (lines[nr_line, lines_PROP_gph_direction] == "N")
                            {
                                pin1_x0 = x0 + 2; pin1_y0 = y0 + 4;
                                pin1_x = x0 + 2 + poly_x_final1; pin1_y = y0 + 4 + poly_y_final1;
                                pin2_x = x0 + object_dx - 4; pin2_y = y0 + 4 - 0;
                            }
                            lines[nr_line, lines_PROP_pin1_x] = pin1_x.ToString();
                            lines[nr_line, lines_PROP_pin1_y] = pin1_y.ToString();
                            lines[nr_line, lines_PROP_pin1_x0] = pin1_x0.ToString();
                            lines[nr_line, lines_PROP_pin1_y0] = pin1_y0.ToString();
                        }
                    int poly_x_final2 = 0, poly_y_final2 = 0;
                    if (lines[lines_no, lines_PROP_npoly2_xy] != "") // polylines connected to terminal 2 of the line
                    { // !!npoly1_xy={0,10,20,30,40,0}
                        string s1p = lines[lines_no, lines_PROP_npoly2_xy];
                        s1p = s1p.Replace("{", ""); s1p = s1p.Replace("}", "");
                        char[] delimiterChars1 = { ',' };
                        string[] line1 = s1p.Split(delimiterChars1);
                        for (int xy = 0; xy < line1.Length - 1; xy++)
                        {
                                int val = int.Parse(line1[xy]);
                                if ((xy % 2) == 0) // variabila x
                            {
                                    poly_x_final2 += val;
                                    lines_npolys[lines_no, xy, line_Terminal_2, 0] = poly_x_final2; // x
                                    lines_npolys[lines_no, xy, line_Terminal_2, 1] = poly_y_final2; // y
                            }
                            else // variabila y
                            {
                                    poly_y_final2 += int.Parse(line1[xy]);
                                    lines_npolys[lines_no, xy, line_Terminal_2, 0] = poly_x_final2; // x
                                    lines_npolys[lines_no, xy, line_Terminal_2, 1] = poly_y_final2; // y
                            }
                        }
                            if (lines[lines_no, lines_PROP_gph_direction] == "W")
                            {
                                pin1_x0 = x0 + 6; pin1_y0 = y0 + 2;
                                pin1_x = x0 + 6 + poly_x_final2;
                                pin1_y = y0 + 2 + poly_y_final2;
                                pin2_x0 = x0 + 6;
                                pin2_y0 = y0 + line_dy + 2;
                                pin2_x = x0 + poly_x_final2 + 6;
                                pin2_y = y0 + 2 + line_dy - 2 + poly_y_final2;
                            }
                            if (lines[lines_no, lines_PROP_gph_direction] == "N")
                            {
                                pin1_x0 = x0 + 2; pin1_y0 = y0 + 4;
                                pin1_x = x0 + 2 + poly_x_final2;
                                pin1_y = y0 + 4 + poly_y_final2;
                                pin2_x0 = x0 + object_dx + 0;
                                pin2_y0 = y0 + 6;
                                pin2_x = x0 + object_dx + poly_x_final2;
                                pin2_y = y0 + 4 - 0 + poly_y_final2;
                            }
                            if (lines[lines_no, lines_PROP_gph_direction] == "E")
                            {
                                pin1_x0 = x0 + object_dx - 6; pin1_y0 = y0 + 2;
                                pin1_x = x0 + object_dx - 6 + poly_x_final2;
                                pin1_y = y0 + 2 + poly_y_final2;
                                pin2_x = x0 + object_dx - 4; pin2_y = y0 + line_dy + 4 - 2;
                            }
                            lines[lines_no, lines_PROP_pin2_x] = pin2_x.ToString();
                            lines[lines_no, lines_PROP_pin2_y] = pin2_y.ToString();
                            lines[lines_no, lines_PROP_pin2_x0] = pin2_x0.ToString();
                            lines[lines_no, lines_PROP_pin2_y0] = pin2_y0.ToString();
                        } else
                        {
                            if (lines[lines_no, lines_PROP_gph_direction] == "W")
                            {
                                pin1_x0 = x0 + 6; pin1_y0 = y0 + 2;
                                pin1_x = x0 + 6 + poly_x_final2;
                                pin1_y = y0 + 2 + poly_y_final2;
                                pin2_x0 = x0 + 6; pin2_y0 = y0 + 2;
                                pin2_x = x0 + 6; pin2_y = y0 + 2 + line_dy - 2;
                            }
                            if (lines[lines_no, lines_PROP_gph_direction] == "N")
                            {
                                pin1_x0 = x0 + 2; pin1_y0 = y0 + 4;
                                pin1_x = x0 + 2 + poly_x_final2;
                                pin1_y = y0 + 4 + poly_y_final2;
                                pin2_x = x0 + object_dx - 4; pin2_y = y0 + 4 - 0;
                            }
                            lines[lines_no, lines_PROP_pin2_x] = pin2_x.ToString();
                            lines[lines_no, lines_PROP_pin2_y] = pin2_y.ToString();
                            lines[lines_no, lines_PROP_pin2_x0] = pin2_x0.ToString();
                            lines[lines_no, lines_PROP_pin2_y0] = pin2_y0.ToString();
                        }
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
                if ((lines[nr_line, lines_PROP_gph_direction] == "W") && (lines[nr_line, lines_PROP_gph_DrawType] == "L2S0"))
                {
                    pin1_x = x0 + 6; pin1_y = y0 + 6;
                    lines[nr_line, lines_PROP_pin1_x] = pin1_x.ToString();
                    lines[nr_line, lines_PROP_pin1_y] = pin1_y.ToString();
                    pin2_x = x0 + 6; pin2_y = y0 + line_dy*2 - 2;
                    lines[nr_line, lines_PROP_pin2_x] = pin2_x.ToString();
                    lines[nr_line, lines_PROP_pin2_y] = pin2_y.ToString();
                }
                if ((lines[nr_line, lines_PROP_gph_direction] == "N") && (lines[nr_line, lines_PROP_gph_DrawType] == "L3S0"))
                {
                    pin1_x = x0 + 2; pin1_y = y0 + 6;
                    lines[nr_line, lines_PROP_pin1_x] = pin1_x.ToString();
                    lines[nr_line, lines_PROP_pin1_y] = pin1_y.ToString();
                    pin2_x = x0 + object_dx * 3 - 2; pin2_y = y0 + 6;
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
                    object_x0 = (int)(object_x0 * zoom);
                    object_y0 = -Y0_shift + int.Parse(lines[i1, lines_PROP_y0]);
                    //object_y0 = (int)(object_y0 * zoom);
                    //Brush brush1 = new SolidBrush(Color.FromArgb(127, 255, 255, 255)); // b1White
                    Brush brush1 = b1White;
                    int object_dx_zoom = (int)(object_dx * zoom);
                    if ((gph_direction == "N") && (extend_x == 1)) {
                        if (lines[i1, lines_PROP_ConnectionType] == "Separator")
                            g.FillRectangle(brush1, object_x0, object_y0, object_dx_zoom * extend_x * 5 / 6, line_dy);
                        else
                        {
                            if ((lines[i1, lines_PROP_gph_DrawType] == ""))
                                g.FillRectangle(brush1, object_x0, object_y0, object_dx_zoom * extend_x, line_dy);
                            if ((lines[i1, lines_PROP_gph_DrawType] == "L1S0"))
                                g.FillRectangle(brush1, object_x0, object_y0, object_dx_zoom * extend_x, line_dy / 2 + 7);
                        }
                    }
                    if ((gph_direction == "N") && (extend_x == 2)) // lines[i1, lines_PROP_gph_DrawType] == "L2S0";
                    {
                        //if ((gph_direction == "N") && (lines[i1, lines_PROP_gph_DrawType] == "L2S0"))
                            g.FillRectangle(brush1, object_x0, object_y0, object_dx * extend_x, line_dy - 40);
                    }
                    if ((gph_direction == "W") && (extend_x == 2)) // lines[i1, lines_PROP_gph_DrawType] == "L2S0";
                    {
                        //    g.FillRectangle(b4LightBlue, object_x0, object_y0, 60, line_dy * extend_x);
                        g.FillRectangle(brush1, object_x0, object_y0, 80, line_dy * extend_x);
                    }
                    if ((gph_direction == "N") && (extend_x == 3)) g.FillRectangle(brush1, object_x0, object_y0, object_dx*extend_x, line_dy - 30);
                    if ((gph_direction == "W") && (extend_x == 1))
                    {
                        if (lines[i1, lines_PROP_ConnectionType] == "Separator")
                            //g.FillRectangle(brush1, object_x0, object_y0, object_dx * extend_x, line_dy * 2 / 3);
                            g.FillRectangle(brush1, object_x0, object_y0, object_dx * extend_x, line_dy * 5 / 6);
                        else
                        {
                            if ((lines[i1, lines_PROP_gph_DrawType] == ""))
                                g.FillRectangle(brush1, object_x0, object_y0, object_dx * extend_x, line_dy);
                            if ((lines[i1, lines_PROP_gph_DrawType] == "L0S1"))
                                g.FillRectangle(brush1, object_x0, object_y0, object_dx /2 +15, line_dy);
                        }
                    }
                    if ((gph_direction == "E") && (extend_x == 1))
                    {
                        if (lines[i1, lines_PROP_ConnectionType] == "Separator")
                            g.FillRectangle(brush1, object_x0, object_y0, object_dx * extend_x, line_dy * 5 / 6);
                        else g.FillRectangle(brush1, object_x0, object_y0, object_dx * extend_x, line_dy);
                    }
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
                        if (lines[i1, lines_PROP_ConnectionType] == "Separator") { 
                            g.FillRectangle(b14Aqua, object_x0 + object_dx /2 - 10, object_y0 + 0, 20, 18);
                            if (lines[i1, lines_PROP_brk1] == "on")
                                g.DrawLine(p5DarkBlue3, object_x0 + object_dx / 2 - 7, object_y0 + 7, object_x0 + object_dx / 2 + 7, object_y0 + 7); // Desenare simbol separator
                            if (lines[i1, lines_PROP_brk1] == "off")
                                g.DrawLine(p5DarkBlue3, object_x0 + object_dx / 2 -2, object_y0 + 2, object_x0 + object_dx / 2 - 2, object_y0 + 14); // Desenare simbol separator
                        }
                        else g.DrawRectangle(p_crt, object_x0 + 22 + (extend_x - 1) * 10, object_y0 + 0, object_dx * extend_x - 44 - (extend_x - 1) * 20, 11);
                        int dy1 = 0;
                        if (lines[i1, lines_PROP_ConnectionType] == "Separator") dy1 = -5;
                        g.DrawString("1", Font1, b0Black, object_x0 + 22 + (extend_x - 1) * 10, object_y0 + 1 + dy1);
                        g.DrawString("2", Font0, b0Black, object_x0 + object_dx * extend_x - 33 - (extend_x - 1) * 10, object_y0 + 1 + dy1);
                        int dx = 0;
                        if (lines[i1, lines_PROP_ConnectionType] == "Separator") dx = 14;
                        g.DrawLine(p5DarkBlue, object_x0, object_y0 + 6, object_x0 + 22 + 10 * (extend_x - 1)+dx, object_y0 + 6); // draw lines in the line object
                        g.DrawLine(p5DarkBlue, object_x0 + object_dx * extend_x - 22 - 10 * (extend_x - 1)-dx, object_y0 + 6, object_x0 + object_dx * extend_x, object_y0 + 6);
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
                    p_crt = p5DarkBlue;
                    if ((lines[i1, lines_PROP_Imax] != "") && (lines[i1, lines_PROP_I1] != ""))
                    {
                        Imax = double.Parse(lines[i1, lines_PROP_Imax]);
                        if (Imax < double.Parse(lines[i1, lines_PROP_I1])) p_crt = p6Red2;
                    }
                    int dx = 0;
                    if (lines[i1, lines_PROP_ConnectionType] == "Separator") dx = -4;
                    g.DrawString("1", Font1, b0Black, object_x0 + 1 + dx, object_y0 + 22);
                    g.DrawString("2", Font0, b0Black, object_x0 + 1 + dx, object_y0 + line_dy * (extend_x-1) + 60);

                    if ((lines[i1, lines_PROP_ConnectionType] == "") || (lines[i1, lines_PROP_ConnectionType] == "line")) { 
                        if (extend_x == 1) g.DrawRectangle(p_crt, object_x0 + 0, object_y0 + 22, 11, line_dy - 46); // Desenare simbol linie
                        if (extend_x == 2) g.DrawRectangle(p_crt, object_x0 - 2, object_y0 + 22, 14, line_dy * extend_x - 46); // Desenare simbol linie
                    }
                    if (lines[i1, lines_PROP_ConnectionType] == "Separator") { 
                        g.FillRectangle(b14Aqua, object_x0 - 2, object_y0 + line_dy/2 - 10, 16, 20); // Desenare simbol linie
                        if (lines[i1, lines_PROP_brk1] == "on")
                            g.DrawLine(p5DarkBlue3, object_x0 + 5, object_y0 + line_dy / 2-7, object_x0 + 5, object_y0 + line_dy / 2 + 7); // Desenare simbol separator
                        if (lines[i1, lines_PROP_brk1] == "off")
                            g.DrawLine(p5DarkBlue3, object_x0 + 1, object_y0 + line_dy / 2, object_x0 + 12, object_y0 + line_dy / 2); // Desenare simbol separator
                    }
                    int dy2 = 0;
                    if (lines[i1, lines_PROP_ConnectionType] == "Separator") dy2 = 14;
                    g.DrawLine(p5DarkBlue, object_x0 + 6, object_y0, object_x0 + 6, object_y0 + 22 + dy2);
                    g.DrawLine(p5DarkBlue, object_x0 + 6, object_y0 + line_dy * extend_x - 22 -dy2, object_x0 + 6, object_y0 + line_dy * extend_x);
                    g.DrawEllipse(p5DarkBlue, object_x0 + 4, object_y0, 4, 4); // Pin_Bus1
                    if (lines[i1, lines_PROP_brk1] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                    //g.FillRectangle(b12, object_x0 + 10, object_y0 + 2, 8, 8); // Brk1
                    if (lines[i1, lines_PROP_ConnectionType] != "Separator")
                    {
                        g.FillRectangle(SolidBrush_crt, object_x0 + 2, object_y0 + 10, 8, 8); // Brk1
                    }
                    g.DrawEllipse(p5DarkBlue, object_x0 + 4, object_y0 + line_dy * extend_x - 4, 4, 4); // Pin_Bus2
                    if (lines[i1, lines_PROP_brk2] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                    if (lines[i1, lines_PROP_ConnectionType] != "Separator") { 
                        g.FillRectangle(SolidBrush_crt, object_x0 + 2, object_y0 + line_dy * (extend_x - 0) - 18, 8, 8); // Brk2
                    }
                }
                if (gph_direction == "E")
                {
                    p_crt = p5DarkBlue;
                    if ((lines[i1, lines_PROP_Imax] != "") && (lines[i1, lines_PROP_I1] != ""))
                    {
                        Imax = double.Parse(lines[i1, lines_PROP_Imax]);
                        if (Imax < double.Parse(lines[i1, lines_PROP_I1])) p_crt = p6Red2;
                    }
                    int dx = 0;
                    if (lines[i1, lines_PROP_ConnectionType] == "Separator") dx = -4;
                    g.DrawString("1", Font1, b0Black, object_x0 + object_dx  - (10 + dx), object_y0 + 22);
                    g.DrawString("2", Font0, b0Black, object_x0 + object_dx - (10 + dx), object_y0 + line_dy * (extend_x - 1) + 60);
                    //g.DrawString("2", Font0, b0Black, object_x0 + 1 + dx, object_y0 + 60);
                    //lines_PROP_ConnectionType
                    if ((lines[i1, lines_PROP_ConnectionType] == "") || (lines[i1, lines_PROP_ConnectionType] == "line"))
                    {
                        if (extend_x == 1) g.DrawRectangle(p_crt, object_x0 + object_dx - 11, object_y0 + 22, 11, line_dy - 46); // Desenare simbol linie
                        if (extend_x == 2) g.DrawRectangle(p_crt, object_x0 + object_dx - 2, object_y0 + 22, 14, line_dy * extend_x - 46); // Desenare simbol linie
                    }
                    if (lines[i1, lines_PROP_ConnectionType] == "Separator")
                    {
                        g.FillRectangle(b14Aqua, object_x0 - 2, object_y0 + line_dy / 2 - 10, 16, 20); // Desenare simbol linie
                        if (lines[i1, lines_PROP_brk1] == "on")
                            g.DrawLine(p5DarkBlue3, object_x0 + 5, object_y0 + line_dy / 2 - 7, object_x0 + 5, object_y0 + line_dy / 2 + 7); // Desenare simbol separator
                        if (lines[i1, lines_PROP_brk1] == "off")
                            g.DrawLine(p5DarkBlue3, object_x0 + 1, object_y0 + line_dy / 2, object_x0 + 12, object_y0 + line_dy / 2); // Desenare simbol separator
                    }
                    int dy2 = 0;
                    if (lines[i1, lines_PROP_ConnectionType] == "Separator") dy2 = 14;
                    g.DrawLine(p5DarkBlue, object_x0 + object_dx - 6, object_y0, object_x0 + object_dx - 6, object_y0 + 22 + dy2);
                    g.DrawLine(p5DarkBlue, object_x0 + object_dx - 6, object_y0 + line_dy * extend_x - 22 - dy2, object_x0 + object_dx - 6, object_y0 + line_dy * extend_x);
                    g.DrawEllipse(p5DarkBlue, object_x0 + object_dx - 8, object_y0, 4, 4); // Pin_Bus1
                    if (lines[i1, lines_PROP_brk1] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                    //g.FillRectangle(b12, object_x0 + 10, object_y0 + 2, 8, 8); // Brk1
                    if (lines[i1, lines_PROP_ConnectionType] != "Separator")
                    {
                        g.FillRectangle(SolidBrush_crt, object_x0 + object_dx - 9, object_y0 + 10, 8, 8); // Brk1
                    }
                    g.DrawEllipse(p5DarkBlue, object_x0 + object_dx - 8, object_y0 + line_dy * extend_x - 4, 4, 4); // Pin_Bus2
                    if (lines[i1, lines_PROP_brk2] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                    if (lines[i1, lines_PROP_ConnectionType] != "Separator")
                    {
                        g.FillRectangle(SolidBrush_crt, object_x0 + object_dx - 9, object_y0 + line_dy * (extend_x - 0) - 18, 8, 8); // Brk2
                    }
                }

                // draw polylines ********************************** 
                if (lines[i1, lines_PROP_npoly1_xy] != "") // polylines for terminal 1
                {
                    int pin1_x = 0, pin1_y = 0, pin1_x0 = 0, pin1_y0 = 0;
                    pin1_x = int.Parse(lines[i1, lines_PROP_pin1_x]);
                    pin1_y = int.Parse(lines[i1, lines_PROP_pin1_y]);
                    pin1_x0 = int.Parse(lines[i1, lines_PROP_pin1_x0]);
                    pin1_y0 = int.Parse(lines[i1, lines_PROP_pin1_y0]);
                    for (int pln = 0; pln < 10; pln++)
                    {
                        int x10 = 0;
                        int y10 = 0;
                        int x1 = Convert.ToInt32(lines_npolys[i1, pln, line_Terminal_1, 0]);
                        int y1 = Convert.ToInt32(lines_npolys[i1, pln, line_Terminal_1, 1]);
                        if (pln == 0)
                            g.DrawLine(p5DarkBlue3, pin1_x0, pin1_y0, pin1_x0 + x1, pin1_y0 + y1);
                        else
                        {
                            x10 = Convert.ToInt32(lines_npolys[i1, pln - 1, line_Terminal_1, 0]);
                            y10 = Convert.ToInt32(lines_npolys[i1, pln - 1, line_Terminal_1, 1]);
                            //if ((pln == 1) && (x10 != 0)) g.DrawLine(p4_Line, pin1_x0, pin1_y0, pin1_x0 + x10, pin1_y0 + y10);
                            if ((x1 != 0) || (y1 != 0))
                                g.DrawLine(p4_Line, pin1_x0 + x10, pin1_y0 + y10, pin1_x0 + x1, pin1_y0 + y1);
                        } 
                    }
                }
                if (lines[i1, lines_PROP_npoly2_xy] != "") // polylines for terminal 2
                {
                    int pin1_x = 0, pin1_y = 0, pin1_x0 = 0, pin1_y0 = 0;
                    pin1_x = int.Parse(lines[i1, lines_PROP_pin2_x]);
                    pin1_y = int.Parse(lines[i1, lines_PROP_pin2_y]);
                    pin1_x0 = int.Parse(lines[i1, lines_PROP_pin2_x0]);
                    pin1_y0 = int.Parse(lines[i1, lines_PROP_pin2_y0]);
                    for (int pln = 0; pln < 10; pln++)
                    {
                        int x10 = 0;
                        int y10 = 0;
                        int x1 = Convert.ToInt32(lines_npolys[i1, pln, line_Terminal_2, 0]);
                        int y1 = Convert.ToInt32(lines_npolys[i1, pln, line_Terminal_2, 1]);
                        if (pln == 0)
                            g.DrawLine(p5DarkBlue3, pin1_x0, pin1_y0, pin1_x0 + x1, pin1_y0 + y1);
                        else
                        {
                            x10 = Convert.ToInt32(lines_npolys[i1, pln - 1, line_Terminal_2, 0]);
                            y10 = Convert.ToInt32(lines_npolys[i1, pln - 1, line_Terminal_2, 1]);
                            if ((x1 != 0) || (y1 != 0))
                                g.DrawLine(p4_Line, pin1_x0 + x10, pin1_y0 + y10, pin1_x0 + x1, pin1_y0 + y1);
                        }
                    }
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

                // ************************* display line parameters
                int dy3 = 0;
                if (lines[i1, lines_PROP_ConnectionType] == "Separator") dy3 = 10;
                int indent = 0;
                if (gph_direction == "N") indent = 0; if (gph_direction == "W") indent = 11;

                s1 = "Ln=" + lines[i1, lines_PROP_name];
                if (lines[i1, lines_PROP_gph_selected] == "1") { Font_crt = Font1bold; SolidBrush_crt = b6Red; }
                    else { Font_crt = Font1; SolidBrush_crt = b0Black; }
                if (gph_direction == "N") {
                    if (extend_x == 1)
                    {
                        g.DrawString(s1, Font_crt, SolidBrush_crt, object_x0 + 2 + indent + object_dx / 2 * (extend_x - 1), object_y0 + 11 + dy3);
                    }
                    if (extend_x == 2) g.DrawString(s1, Font_crt, SolidBrush_crt, object_x0 + object_dx * 2 / 2 - 25, object_y0 + 13);
                    if (extend_x == 3) g.DrawString(s1, Font_crt, SolidBrush_crt, object_x0 + object_dx * 3 / 2 - 20, object_y0 + 13);
                }
                if (gph_direction == "W")
                {
                    if (extend_x == 1)
                    {
                        if ((lines[i1, lines_PROP_gph_DrawType] == ""))
                            g.DrawString(s1, Font_crt, SolidBrush_crt, object_x0 + 2 + indent, object_y0 + 11);
                        if ((lines[i1, lines_PROP_gph_DrawType] == "L0S1")) {
                            s1 = s1.Remove(6) + "*";
                            g.DrawString(s1, Font_crt, SolidBrush_crt, object_x0 + 2 + indent, object_y0 + 11);
                        }
                    }

                    if (extend_x == 2)
                    {
                        g.DrawString(s1, Font_crt, SolidBrush_crt, object_x0 + 12, object_y0 + 82);
                    }
                }

                string bus_display = "";
                bus_display = lines[i1, lines_PROP_bus1];
                if (lines[i1, lines_PROP_bus1].Contains("@")) bus_display = lines[i1, lines_PROP_bus1].Remove(lines[i1, lines_PROP_bus1].IndexOf('@'));
                if (bus_display.Length > 4) s1 = "B1=" + bus_display.Remove(4); else s1 = "B1=" + bus_display;
                if ((extend_x == 1) && (gph_direction == "N"))
                {
                    if((lines[i1, lines_PROP_gph_DrawType] == ""))
                    g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent, object_y0 + 22 +dy3);
                }
                if ((extend_x == 1) && (gph_direction == "W"))
                {
                    if ((lines[i1, lines_PROP_gph_DrawType] == ""))
                        g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent, object_y0 + 22);
                }
                if ((extend_x == 2) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 - 10 + (extend_x - 1) * 10, object_y0 + 13);
                if ((extend_x == 2) && (gph_direction == "W"))
                    g.DrawString(s1, Font1, b0Black, object_x0 +12, object_y0 + 8);
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 22 + (extend_x - 1) * 10, object_y0 + 13);

                bus_display = lines[i1, lines_PROP_bus2];
                if (lines[i1, lines_PROP_bus2].Contains("@")) bus_display = lines[i1, lines_PROP_bus2].Remove(lines[i1, lines_PROP_bus2].IndexOf('@'));
                if(bus_display.Length>4) s1 = "B2=" + bus_display.Remove(4); else s1 = "B2=" + bus_display;
                if ((extend_x == 1) && (gph_direction == "N"))
                {
                    if ((lines[i1, lines_PROP_gph_DrawType] == ""))
                        g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent + 47, object_y0 + 22+dy3);
                }
                if ((extend_x == 1) && (gph_direction == "W"))
                {
                    if ((lines[i1, lines_PROP_gph_DrawType] == ""))
                        g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent + 47, object_y0 + 22);
                }
                if ((extend_x == 2) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + object_dx * extend_x - 25 - (extend_x - 1) * 20, object_y0 + 13);
                if ((extend_x == 2) && (gph_direction == "W"))
                    g.DrawString(s1, Font1, b0Black, object_x0 + 12, object_y0 + object_dx * 2 - 22);
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + object_dx * extend_x - 44 - (extend_x - 1) * 20, object_y0 + 13);

                if(lines[i1, lines_PROP_length] != "")
                s1 = "L=" + double.Parse(lines[i1, lines_PROP_length]).ToString("##0.000") + "" + lines[i1, lines_PROP_units];
                double R_line=0, X_line = 0;
                R_line = lines_double[i1, lines_PROP_R1] * lines_double[i1, lines_PROP_length];
                X_line = lines_double[i1, lines_PROP_X1] * lines_double[i1, lines_PROP_length];
                if (extend_x == 1)
                {   // We calculate the line parameters R_line and X_line, using R1, X1 and the length of the line

                    if (R_line < 0.1) s1 = "R=" + R_line.ToString("#0.000");
                    else s1 = "R=" + R_line.ToString("#0.00");

                    if (X_line < 0.1) s1 += " X=" + X_line.ToString("#0.000");
                    else s1 += " X=" + X_line.ToString("#0.00");

                    //s1 += " X=" + lines[i1, lines_PROP_X1] + " ";
                }
                if (extend_x == 1)
                {
                    if (lines[i1, lines_PROP_ConnectionType] != "Separator")
                        if ((lines[i1, lines_PROP_gph_DrawType] == ""))
                            g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent, object_y0 + 32);
                }
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + 38);


                s1 = "Cod=" + lines[i1, lines_PROP_linecode];
                //s1 = "R=" + lines[i1, lines_PROP_R1] + " X=" + lines[i1, lines_PROP_X1] +" ";
                if (extend_x == 1)
                    if ((lines[i1, lines_PROP_gph_DrawType] == ""))
                        g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent, object_y0 + 42);
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 42, object_y0 + 25);

                s1 = "Imax=" + lines[i1, lines_PROP_Imax];
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 127, object_y0 + 25);

                s1 = "Umax=" + lines[i1, lines_PROP_Umax];
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 185, object_y0 + 25);

                //s1 = "R=" + lines[i1, lines_PROP_R1];
                s1 = "R=" + R_line.ToString("#0.00");
                if ((extend_x == 2) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 75, object_y0 + 28);
                if ((extend_x == 2) && (gph_direction == "W")) g.DrawString(s1, Font1, b0Black, object_x0 + 12, object_y0 + 95);
                if ((extend_x == 3) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 65, object_y0 + 38);

                //s1 = "X=" + lines[i1, lines_PROP_X1];
                s1 = "X=" + X_line.ToString("#0.00");
                if ((extend_x == 2) && (gph_direction == "N")) g.DrawString(s1, Font1, b0Black, object_x0 + 75, object_y0 + 40);
                if ((extend_x == 2) && (gph_direction == "W")) g.DrawString(s1, Font1, b0Black, object_x0 + 12, object_y0 + 107);
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
                        if (extend_x == 1)
                        {
                                g.DrawLine(p1Black4arrow, object_x0 + 33 + object_dx / 2 * (extend_x - 1), object_y0 + 3, object_x0 + 57 + object_dx / 2 * (extend_x - 1), object_y0 + 3);
                        }
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
                    if (Math.Abs(P3f) >= 1000)
                    {
                        if (Math.Abs(P3f) >= 1000) s1 = "P=" + P3f_kilo.ToString("##0.00") + "M";
                        if (Math.Abs(P3f) >= 10000) s1 = "P=" + P3f_kilo.ToString("##0.0") + "M";
                    }
                    else
                    {
                        //if (Math.Abs(P3f) >= 0) s1 = "P=" + P3f.ToString("####0.00") + "k";
                        //if (Math.Abs(P3f) >= 1)
                            s1 = "P=" + P3f.ToString("####0.00") + "k";
                        if (Math.Abs(P3f) >= 100) s1 = "P=" + P3f.ToString("####0.0") + "k";
                    }
                    if (Math.Abs(P3f_t2) >= 1000) s2 = "P=" + P3f_t2_kilo.ToString("##0.0") + "M";
                        else s2 = "P=" + P3f_t2.ToString("####0.0") + "k";
                    if (Math.Abs(delta_P3f) >= 10000) s3 = "ΔP=" + delta_P3f_kilo.ToString("##0.0") + "M";
                        else s3 = "ΔP=" + delta_P3f.ToString("####0.00") + "k";
                    //s1 = "P=" + lines[i1, lines_PROP_P];
                    if (extend_x == 1)
                    {
                        if ((lines[i1, lines_PROP_gph_DrawType] == ""))
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 52);
                        if ((lines[i1, lines_PROP_gph_DrawType] == "L1S0"))
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 22);
                        if ((lines[i1, lines_PROP_gph_DrawType] == "L0S1"))
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 22);
                    }
                    if ((extend_x == 2) && (gph_direction == "N"))
                    {
                        int dx1 = 0;
                        Font_crt = Font12b;
                        if (lines[i1, lines_PROP_Font_P] != "")
                        {
                            string F1 = lines[i1, lines_PROP_Font_P];
                            if (F1 == "8") Font_crt = Font1;
                            if (F1 == "10") Font_crt = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
                        }
                        if (lines[i1, lines_PROP_HidePinsNo] == "1") dx1 = -13;
                            g.DrawString(s1, Font_crt, b2Blue, object_x0 + 30 + dx1, object_y0 - 4);
                        if (lines[i1, lines_PROP_P_t2] != "") g.DrawString(s2, Font_crt, b2Blue, object_x0 + 100, object_y0 - 4);
                    }
                    if ((extend_x == 2) && (gph_direction == "W"))
                    {
                        int dx1 = 0;
                        Font_crt = Font12b;
                        if (lines[i1, lines_PROP_Font_P] != "")
                        {
                            string F1 = lines[i1, lines_PROP_Font_P];
                            if (F1 == "8") Font_crt = Font1;
                            if (F1 == "10") Font_crt = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
                        }
                        g.DrawString(s1, Font_crt, b2Blue, object_x0 + 12, object_y0 +20);
                        if (lines[i1, lines_PROP_P_t2] != "") g.DrawString(s2, Font_crt, b2Blue, object_x0 + 12, object_y0 + object_dx*2 - 35);
                        //g.DrawString(s3, Font1, b2Blue, object_x0 + 91, object_y0 - 1);
                    }
                    if ((extend_x == 3) && (gph_direction == "N"))
                    {
                        g.DrawString(s1, Font1, b2Blue, object_x0 + 30, object_y0 - 1);
                        if (lines[i1, lines_PROP_P_t2] != "") g.DrawString(s2, Font1, b2Blue, object_x0 + 188, object_y0 - 1);
                        g.DrawString(s3, Font1, b2Blue, object_x0 + 91, object_y0 - 1);
                    }
                }

                // Calculation and display of Q
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
                    if (extend_x == 1)
                    {
                        if ((lines[i1, lines_PROP_gph_DrawType] == ""))
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 50 + indent, object_y0 + 52);
                        if ((lines[i1, lines_PROP_gph_DrawType] == "L1S0") && (gph_direction == "N"))
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 50 + indent, object_y0 + 22);
                        if ((lines[i1, lines_PROP_gph_DrawType] == "L0S1") && (gph_direction == "W"))
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 32);
                    }
                    if ((extend_x == 2) && (gph_direction == "N"))
                    {
                        Font_crt = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
                        if (lines[i1, lines_PROP_Font_Q] != "")
                        {
                            string F1 = lines[i1, lines_PROP_Font_Q];
                            if (F1 == "8") Font_crt = Font1;
                            if (F1 == "10") Font_crt = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
                            if (F1 == "12") Font_crt = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
                        }
                        g.DrawString(s1, Font_crt, b2Blue, object_x0 + 30, object_y0 - 20);
                        if (lines[i1, lines_PROP_Q_t2] != "") g.DrawString(s2, Font_crt, b2Blue, object_x0 + 100, object_y0 - 20);
                        //g.DrawString(s3, Font1, b2Blue, object_x0 + 91, object_y0 - 14);
                    }
                    if ((extend_x == 2) && (gph_direction == "W"))
                    {
                        Font_crt = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
                        if (lines[i1, lines_PROP_Font_Q] != "")
                        {
                            string F1 = lines[i1, lines_PROP_Font_Q];
                            if (F1 == "8") Font_crt = Font1;
                            if (F1 == "10") Font_crt = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
                            if (F1 == "12") Font_crt = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
                        }
                        g.DrawString(s1, Font_crt, b2Blue, object_x0 + 12, object_y0 + 35);
                        if (lines[i1, lines_PROP_P_t2] != "") g.DrawString(s2, Font_crt, b2Blue, object_x0 + 12, object_y0 + object_dx * 2 - 50);

                        //g.DrawString(s1, Font_crt, b2Blue, object_x0 + 30, object_y0 - 20);
                        //if (lines[i1, lines_PROP_Q_t2] != "") g.DrawString(s2, Font_crt, b2Blue, object_x0 + 100, object_y0 - 20);
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
                    if (extend_x == 1)
                    {
                        if ((lines[i1, lines_PROP_gph_DrawType] == ""))
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 62);
                        if ((lines[i1, lines_PROP_gph_DrawType] == "L1S0"))
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 32);
                        if ((lines[i1, lines_PROP_gph_DrawType] == "L0S1"))
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 42);
                    }
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

                /* Display line current I */
                if (lines[i1, lines_PROP_I1] != "")
                {
                    if (extend_x == 1)
                    {
                        s1 = "I1=" + double.Parse(lines[i1, lines_PROP_I1]).ToString("#0.0");
                        if ((lines[i1, lines_PROP_gph_DrawType] == ""))
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 50 + indent, object_y0 + 62);
                        if ((lines[i1, lines_PROP_gph_DrawType] == "L1S0") && (gph_direction == "N"))
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 50 + indent, object_y0 + 32);
                        if ((lines[i1, lines_PROP_gph_DrawType] == "L0S1") && (gph_direction == "W"))
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 52);
                    }
                    if ((extend_x == 2) && (gph_direction == "W"))
                    {
                        s1 = "I1=" + double.Parse(lines[i1, lines_PROP_I1]).ToString("#0.0");
                        g.DrawString(s1, Font1, b2Blue, object_x0 + 12, object_y0 + 72);
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
                    if (U < 1000) { s1 = U.ToString("#00.0"); s2 = U_t2.ToString("#00.0"); }
                    else
                    {
                        U = U / 1000; // se va afisa in kV
                        U_t2 = U_t2 / 1000; // se va afisa in kV
                        if(U<100) s1 = U.ToString("#00.00") + "k";
                        else s1 = U.ToString("#00.0") + "k";
                        s2 = U_t2.ToString("#00.00") + "k";
                    }
                    if (extend_x == 1)
                    {
                        if (gph_direction == "N")
                        {
                            if ((lines[i1, lines_PROP_gph_DrawType] == ""))
                                g.DrawString("U1 =" + s1 + s1a, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 72);
                            if ((lines[i1, lines_PROP_gph_DrawType] == "L1S0"))
                                g.DrawString("U=" + s1 + s1a, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 42);
                        }
                        if (gph_direction == "W") {
                            if ((lines[i1, lines_PROP_gph_DrawType] == "")) { 
                                g.DrawString("U1 =" + s1, Font1, b2Blue, object_x0 - 2 + indent, object_y0 + 0);
                                g.DrawString("1", Font0s, b2Blue, object_x0 + 12 + indent, object_y0 + 5);
                                g.DrawString("U1 =" + s2, Font1, b2Blue, object_x0 - 2 + indent, object_y0 + 72);
                                g.DrawString("2", Font0s, b2Blue, object_x0 + 12 + indent, object_y0 + 77);
                            }
                            if ((lines[i1, lines_PROP_gph_DrawType] == "L0S1"))
                            {
                                g.DrawString("U=" + s1, Font1, b2Blue, object_x0 - 2 + indent, object_y0 + 0);
                                //g.DrawString("1", Font0s, b2Blue, object_x0 + 9 + indent, object_y0 + 5);
                                g.DrawString("U=" + s2, Font1, b2Blue, object_x0 - 2 + indent, object_y0 + 72);
                                //g.DrawString("2", Font0s, b2Blue, object_x0 + 9 + indent, object_y0 + 77);
                            }
                        }
                    }
                    if ((extend_x == 2) && (gph_direction == "N"))
                    {
                        Font_crt = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
                        if (lines[i1, lines_PROP_Font_U] != "")
                        {
                            string F1 = lines[i1, lines_PROP_Font_U];
                            if (F1 == "8") Font_crt = Font1;
                            if (F1 == "10") Font_crt = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
                            if (F1 == "12") Font_crt = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
                        }
                        g.DrawString("U1 =" + s1 + s1a, Font_crt, b2Blue, object_x0 + 4, object_y0 + 28);
                        if (lines[i1, lines_PROP_U1_t2] != "") g.DrawString(s2, Font_crt, b2Blue, object_x0 + 118, object_y0 + 28);
                    }
                    if ((extend_x == 2) && (gph_direction == "W"))
                    {
                        Font_crt = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
                        if (lines[i1, lines_PROP_Font_U] != "")
                        {
                            string F1 = lines[i1, lines_PROP_Font_U];
                            if (F1 == "8") Font_crt = Font1;
                            if (F1 == "10") Font_crt = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
                            if (F1 == "12") Font_crt = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
                        }
                        g.DrawString("U1 =" + s1 + s1a, Font_crt, b2Blue, object_x0 + 12, object_y0 + 48);
                        if (lines[i1, lines_PROP_U1_t2] != "") g.DrawString(s2, Font_crt, b2Blue, object_x0 + 12, object_y0 + object_dx * 2 - 61);
                    }
                    if ((extend_x == 3) && (gph_direction == "N"))
                    {
                        g.DrawString("U1 =" + s1 +s1a, Font1, b2Blue, object_x0 + 4, object_y0 + 50);
                        if (lines[i1, lines_PROP_U1_t2] != "") g.DrawString(s2, Font1, b2Blue, object_x0 + 215, object_y0 + 50); //145
                    }
                }

                // Angle of U
                if (lines[i1, lines_PROP_U1fi] != "")
                {
                    s1 = "" + double.Parse(lines[i1, lines_PROP_U1fi]).ToString("#0.0");
                    if (lines[i1, lines_PROP_U1fi_t2] != "") s2 = "" + double.Parse(lines[i1, lines_PROP_U1fi_t2]).ToString("#0.0");
                    else s2 = "";
                    if (extend_x == 1)
                    {
                        s1 = "" + double.Parse(lines[i1, lines_PROP_U1fi]).ToString("#0.0");
                        if (gph_direction == "N")
                        {
                            if ((lines[i1, lines_PROP_gph_DrawType] == ""))
                                g.DrawString(s1, Font1, b2Blue, object_x0 + 60 + indent, object_y0 + 72);
                        }
                        if (gph_direction == "W")
                        {
                            if ((lines[i1, lines_PROP_gph_DrawType] == "")) { 
                                g.DrawString(s1, Font1, b2Blue, object_x0 + 57 + indent, object_y0 + 0);
                                g.DrawString(s2, Font1, b2Blue, object_x0 + 57 + indent, object_y0 + 72);
                            }
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
                    if ((extend_x == 2) && (gph_direction == "W"))
                    {
                        s1 = "U1ϕ=" + double.Parse(lines[i1, lines_PROP_U1fi]).ToString("#0.000");
                        if (lines[i1, lines_PROP_U1fi_t2] != "") s2 = "U1ϕ=" + double.Parse(lines[i1, lines_PROP_U1fi_t2]).ToString("#0.000");
                        else s2 = "";

                        g.DrawString(s1, Font1, b2Blue, object_x0 + 12, object_y0 + 60);
                        g.DrawString(s2, Font1, b2Blue, object_x0 + 12, object_y0 + object_dx * 2 - 72);
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
                    if (Math.Abs(dU1) >= 1000) s1 = dU1_kilo.ToString("##0.000") + "k";
                    else s1 = dU1.ToString("##0.00");
                    //g.DrawString(s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 82);
                    if ((gph_direction == "N") && (extend_x == 3)) g.DrawString("ΔU1= " + s1, Font1, b2Blue, object_x0 + 101, object_y0 - 25);
                    if ((gph_direction == "W") && (extend_x == 1) && (lines[i1, lines_PROP_gph_DrawType] == ""))
                        g.DrawString("ΔU1= " + s1, Font1, b2Blue, object_x0 - 2 + indent, object_y0 +83);
                    if (((gph_direction == "N") || (gph_direction == "W")) && (extend_x == 1))
                    {
                        if ((lines[i1, lines_PROP_gph_DrawType] == "") && (gph_direction == "N"))
                            g.DrawString("ΔU1= " + s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 82);
                        if ((lines[i1, lines_PROP_gph_DrawType] == "L1S0") && (gph_direction == "N"))
                            g.DrawString("ΔU=" + s1, Font1, b2Blue, object_x0 + 50 + indent, object_y0 + 42);
                        if ((lines[i1, lines_PROP_gph_DrawType] == "L0S1") && (gph_direction == "W"))
                            g.DrawString("ΔU=" + s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 62);
                    }
                }

                if (default_xy == 1) obj_number++;
            }

        }

        /****************** Desenare obiecte de tip trafo si alte functii ale acestui obiect ******************/
        private void Trafos_properties_calculation(int nr_trafo)
        {
            if ((trafos[nr_trafo, trafos_PROP_x0] != "") && (trafos[nr_trafo, trafos_PROP_y0] != ""))
            {
                // calculate pins position inside the line representation on the grid picture
                int x0 = int.Parse(trafos[nr_trafo, trafos_PROP_x0]);
                int y0 = int.Parse(trafos[nr_trafo, trafos_PROP_y0]);
                //int pin1_x = 0, pin1_y = 0, pin2_x = 0, pin2_y = 0;
                int pin1_x = 0, pin1_y = 0, pin1_x0 = 0, pin1_y0 = 0, pin2_x = 0, pin2_y = 0, pin2_x0 = 0, pin2_y0 = 0;
                if ((trafos[nr_trafo, trafos_PROP_gph_direction] == "W") && (trafos[nr_trafo, trafos_PROP_gph_DrawType] == ""))
                {
                    /*
                    pin1_x = x0 + 14; pin1_y = y0 + 2;
                    trafos[nr_trafo, trafos_PROP_pin1_x] = pin1_x.ToString();
                    trafos[nr_trafo, trafos_PROP_pin1_y] = pin1_y.ToString();
                    pin2_x = x0 + 16; pin2_y = y0 + 2 + line_dy - 2;
                    trafos[nr_trafo, trafos_PROP_pin2_x] = pin2_x.ToString();
                    trafos[nr_trafo, trafos_PROP_pin2_y] = pin2_y.ToString();*/
                    // extend line terminals with polylines at 90 degree
                    int poly_x_final1 = 0, poly_y_final1 = 0;
                    if (trafos[nr_trafo, trafos_PROP_npoly1_xy] != "") // polylines connected to terminal 1 of the load are defined
                    {
                        string s1p = trafos[nr_trafo, loads_PROP_npoly1_xy];
                        s1p = s1p.Replace("{", ""); s1p = s1p.Replace("}", "");
                        char[] delimiterChars1 = { ',' };
                        string[] line1 = s1p.Split(delimiterChars1);
                        for (int xy = 0; xy < line1.Length - 1; xy++)
                        {
                            int val = int.Parse(line1[xy]);
                            if ((xy % 2) == 0) // variabila x
                            {
                                poly_x_final1 += val;
                                //    lines_npolys[lines_no, xy, line_Terminal_1, 0] = poly_x_final1; // x
                                //    lines_npolys[lines_no, xy, line_Terminal_1, 1] = poly_y_final1; // y
                            }
                            else // variabila y
                            {
                                poly_y_final1 += int.Parse(line1[xy]);
                            }
                            trafos_npolys[nr_trafo, xy, line_Terminal_1, 0] = poly_x_final1; // x
                            trafos_npolys[nr_trafo, xy, line_Terminal_1, 1] = poly_y_final1; // y
                        }
                        if (trafos[nr_trafo, loads_PROP_gph_direction] == "N")
                        {
                            pin1_x0 = x0 + 2 + object_dx / 2;
                            pin1_y0 = y0 + 4;
                            pin1_x = x0 + object_dx / 2 + 2 + poly_x_final1;
                            pin1_y = y0 + 4 + poly_y_final1;
                            pin2_x = x0 + object_dx - 4;
                            pin2_y = y0 + 4 - 0;
                        }
                        trafos[nr_trafo, trafos_PROP_pin1_x] = pin1_x.ToString();
                        trafos[nr_trafo, trafos_PROP_pin1_y] = pin1_y.ToString();
                        trafos[nr_trafo, trafos_PROP_pin1_x0] = pin1_x0.ToString();
                        trafos[nr_trafo, trafos_PROP_pin1_y0] = pin1_y0.ToString();
                    }
                    else
                    {                        
                        pin1_x = x0 + 14; pin1_y = y0 + 2;
                        trafos[nr_trafo, trafos_PROP_pin1_x] = pin1_x.ToString();
                        trafos[nr_trafo, trafos_PROP_pin1_y] = pin1_y.ToString();
                        pin2_x = x0 + 16; pin2_y = y0 + 2 + line_dy - 2;
                        trafos[nr_trafo, trafos_PROP_pin2_x] = pin2_x.ToString();
                        trafos[nr_trafo, trafos_PROP_pin2_y] = pin2_y.ToString();

                        trafos[nr_trafo, trafos_PROP_pin1_x0] = pin1_x0.ToString();
                        trafos[nr_trafo, trafos_PROP_pin1_y0] = pin1_y0.ToString();
                        /*
                        pin1_x = x0 + object_dx / 2; pin1_y = y0 + 2;
                        loads[nr_trafo, loads_PROP_pin1_x] = pin1_x.ToString();
                        loads[nr_trafo, loads_PROP_pin1_y] = pin1_y.ToString();
                        loads[nr_trafo, loads_PROP_pin1_x0] = pin1_x0.ToString();
                        loads[nr_trafo, loads_PROP_pin1_y0] = pin1_y0.ToString();*/
                    }
                }
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

                // draw polylines ********************************** 
                if (trafos[i1, trafos_PROP_npoly1_xy] != "") // polylines for terminal 1
                {
                    int pin1_x = 0, pin1_y = 0, pin1_x0 = 0, pin1_y0 = 0;
                    pin1_x = int.Parse(trafos[i1, trafos_PROP_pin1_x]);
                    pin1_y = int.Parse(trafos[i1, trafos_PROP_pin1_y]);
                    pin1_x0 = int.Parse(trafos[i1, trafos_PROP_pin1_x0]);
                    pin1_y0 = int.Parse(trafos[i1, trafos_PROP_pin1_y0]);
                    for (int pln = 0; pln < 10; pln++)
                    {
                        int x10 = 0;
                        int y10 = 0;
                        int x1 = Convert.ToInt32(trafos_npolys[i1, pln, line_Terminal_1, 0]);
                        int y1 = Convert.ToInt32(trafos_npolys[i1, pln, line_Terminal_1, 1]);
                        if (pln == 0)
                            g.DrawLine(p5DarkBlue3, pin1_x0, pin1_y0, pin1_x0 + x1, pin1_y0 + y1);
                        else
                        {
                            x10 = Convert.ToInt32(trafos_npolys[i1, pln - 1, line_Terminal_1, 0]);
                            y10 = Convert.ToInt32(trafos_npolys[i1, pln - 1, line_Terminal_1, 1]);
                            if ((x1 != 0) || (y1 != 0))
                                g.DrawLine(p4_Line, pin1_x0 + x10, pin1_y0 + y10, pin1_x0 + x1, pin1_y0 + y1);
                        }
                    }
                }

                g.FillRectangle(b4LightBlue, object_x0, object_y0, object_dx + 20, line_dy);

                //s1 = "P: "; g.DrawString(s1, Font1, b2Blue, object_x0 + 31, object_y0 + 2);
                //s1 = "Q: "; g.DrawString(s1, Font1, b2Blue, object_x0 + 31, object_y0 + 12);
                s1 = "T: " + trafos[i1, trafos_PROP_name];
                if (trafos[i1, trafos_PROP_gph_selected] == "1") g.DrawString(s1, Font1bold, b6Red, object_x0 + 31, object_y0 + 22);
                else g.DrawString(s1, Font1, b0Black, object_x0 + 31, object_y0 + 22);
                if (trafos[i1, trafos_PROP_conns] == "(delta,wye)") s1 = "Bs=" + trafos[i1, trafos_PROP_busses] + "dw";
                g.DrawString(s1, Font1, b0Black, object_x0 + 31, object_y0 + 32);
                s1 = "kVs=" + trafos[i1, trafos_PROP_kVs]; g.DrawString(s1, Font1, b0Black, object_x0 + 31, object_y0 + 42);

                if (trafos_double[i1, trafos_PROP_kVAs_sec1] >= 1000)
                    s1 = (trafos_double[i1, trafos_PROP_kVAs_sec1] / 1000).ToString("###0") + "k";
                else s1 = trafos_double[i1, trafos_PROP_kVAs_sec1].ToString("###0");
                s1 = "kVA=(" + s1 + ",";
                if (trafos_double[i1, trafos_PROP_kVAs_sec1] >= 1000)
                    s1 += (trafos_double[i1, trafos_PROP_kVAs_sec1] / 1000).ToString("###0") + "k";
                else s1 += trafos_double[i1, trafos_PROP_kVAs_sec1].ToString("###0");
                s1 += ")";
                g.DrawString(s1, Font1, b0Black, object_x0 + 31, object_y0 + 52);
                //if(trafos[i1, trafos_PROP_conns]=="(delta,wye)")
                //s1 = "conns=" + "(dw)"; g.DrawString(s1, Font1, b0Black, object_x0 + 20, object_y0 + 42);
                s1 = "tap=" + trafos[i1, trafos_PROP_tap]; g.DrawString(s1, Font1, b0Black, object_x0 + 31, object_y0 + 62);

                // display P,Q,S for the terminal 1 of the transformer (upper part for direction=N)
                double P3ph = 0;
                bool P3ph_is_calculated = false;
                if ((trafos[i1, trafos_PROP_P1] != "") && (trafos[i1, trafos_PROP_P2] != "") && (trafos[i1, trafos_PROP_P3] != "")) { 
                    P3ph = double.Parse(trafos[i1, trafos_PROP_P1]) + double.Parse(trafos[i1, trafos_PROP_P2]) + double.Parse(trafos[i1, trafos_PROP_P3]);
                    if (P3ph >= 1000) s1 = (P3ph / 1000).ToString("###0.0") + "M"; else s1 = P3ph.ToString("###0.0") + "k";
                    s1 = "P: " + s1;
                    trafos[i1, trafos_PROP_P] = P3ph.ToString("####0.0");
                    trafos_double[i1, trafos_PROP_P] = P3ph;
                    g.DrawString(s1, Font1, b2Blue, object_x0 + 20, object_y0 + 2);
                    P3ph_is_calculated = true;
                }

                double Q3ph = 0;
                bool Q3ph_is_calculated = false;
                if ((trafos[i1, trafos_PROP_Q1] != "") && (trafos[i1, trafos_PROP_Q2] != "") && (trafos[i1, trafos_PROP_Q3] != "")) { 
                    Q3ph = double.Parse(trafos[i1, trafos_PROP_Q1]) + double.Parse(trafos[i1, trafos_PROP_Q2]) + double.Parse(trafos[i1, trafos_PROP_Q3]);
                    if (Q3ph >= 1000) s1 = (Q3ph / 1000).ToString("###0.0") + "M"; else s1 = Q3ph.ToString("###0.0") + "k";
                    s1 = "Q: " + s1;
                    trafos[i1, trafos_PROP_Q] = Q3ph.ToString("####0.0");
                    trafos_double[i1, trafos_PROP_Q] = Q3ph;
                    g.DrawString(s1, Font1, b2Blue, object_x0 + 20, object_y0 + 12);
                    Q3ph_is_calculated = true;
                }

                double S3ph;
                if((P3ph_is_calculated==true) && (Q3ph_is_calculated==true)) { 
                    S3ph = Math.Sqrt(P3ph * P3ph + Q3ph * Q3ph);
                    if (S3ph >= 1000) s1 = (S3ph / 1000).ToString("###0.0") + "M"; else s1 = S3ph.ToString("###0.0") + "k";
                    s1 = "S: " + s1;
                    trafos[i1, trafos_PROP_S] = S3ph.ToString("####0.0");
                    trafos_double[i1, trafos_PROP_S] = S3ph;
                    g.DrawString(s1, Font1, b2Blue, object_x0 + 76, object_y0 + 12);
                }

                // display P,Q,S for the terminal 2 of the transformer (lower part for direction=N)
                P3ph_is_calculated = false; Q3ph_is_calculated = false;
                P3ph = 0;
                if ((trafos[i1, trafos_PROP_P1_t2] != "") && (trafos[i1, trafos_PROP_P2_t2] != "") && (trafos[i1, trafos_PROP_P3_t2] != "")) { 
                    P3ph = double.Parse(trafos[i1, trafos_PROP_P1_t2]) + double.Parse(trafos[i1, trafos_PROP_P2_t2]) + double.Parse(trafos[i1, trafos_PROP_P3_t2]);
                    if (Math.Abs(P3ph) >= 1000) s1 = (P3ph / 1000).ToString("###0.0") + "M"; else s1 = P3ph.ToString("###0.0") + "k";
                    s1 = "P: " + s1;
                    trafos[i1, trafos_PROP_P_t2] = P3ph.ToString("####0");
                    trafos_double[i1, trafos_PROP_P_t2] = P3ph;
                    g.DrawString(s1, Font1, b2Blue, object_x0 + 20, object_y0 + 72);
                    P3ph_is_calculated = true;
                }

                Q3ph = 0;
                if ((trafos[i1, trafos_PROP_Q1_t2] != "") && (trafos[i1, trafos_PROP_Q2_t2] != "") && (trafos[i1, trafos_PROP_Q3_t2] != "")) { 
                    Q3ph = double.Parse(trafos[i1, trafos_PROP_Q1_t2]) + double.Parse(trafos[i1, trafos_PROP_Q2_t2]) + double.Parse(trafos[i1, trafos_PROP_Q3_t2]);
                    if (Math.Abs(Q3ph) >= 1000) s1 = (Q3ph / 1000).ToString("###0.0") + "M"; else s1 = Q3ph.ToString("###0.0") + "k";
                    s1 = "Q: " + s1;
                    trafos[i1, trafos_PROP_Q_t2] = Q3ph.ToString("####0");
                    trafos_double[i1, trafos_PROP_Q_t2] = Q3ph;
                    g.DrawString(s1, Font1, b2Blue, object_x0 + 20, object_y0 + 82);
                    Q3ph_is_calculated = true;
                }

                if ((P3ph_is_calculated == true) && (Q3ph_is_calculated == true))
                {
                    S3ph = Math.Sqrt(P3ph * P3ph + Q3ph * Q3ph);
                    if (S3ph >= 1000) s1 = (S3ph / 1000).ToString("###0.0") + "M"; else s1 = S3ph.ToString("###0.0") + "k";
                    s1 = "S: " + s1;
                    trafos[i1, trafos_PROP_S_t2] = S3ph.ToString("####0");
                    trafos_double[i1, trafos_PROP_S_t2] = S3ph;
                    g.DrawString(s1, Font1, b2Blue, object_x0 + 76, object_y0 + 82);
                }

                g.DrawEllipse(p5DarkBlue, object_x0 + 11, object_y0, 6, 6); // Pin_Bus1
                g.DrawLine(p5DarkBlue, object_x0 + 14, object_y0 + 6, object_x0 + 14, object_y0 + 27);
                g.DrawEllipse(p5DarkBlue, object_x0 + 11, object_y0 + line_dy - 6, 6, 6); // Pin_Bus2
                g.DrawLine(p5DarkBlue, object_x0 + 14, object_y0 + line_dy - 6, object_x0 + 14, object_y0 + line_dy - 31);
                g.FillRectangle(b2Blue, object_x0 + 11, object_y0 + 10, 8, 8); // Brk1
                g.FillRectangle(b2Blue, object_x0 + 11, object_y0 + line_dy - 18, 8, 8); // Brk2

                g.DrawEllipse(p5DarkBlue2, object_x0, object_y0 + line_dy / 2 - 20, 28, 28); // Cicle 1
                g.DrawEllipse(p5DarkBlue2, object_x0, object_y0 + line_dy / 2 - 10, 28, 28); // Circle 2

                if (default_xy == 1) obj_number++;
            }
        }

        /****************** Desenare obiecte de tip load si alte functii ale acestui obiect ******************/
        private void Loads_properties_calculation(int nr_load)
        {
            if ((loads[nr_load, loads_PROP_x0] != "") && (loads[nr_load, loads_PROP_y0] != ""))
            {
                // calculate pins position inside the line representation on the grid picture
                int x0 = int.Parse(loads[nr_load, loads_PROP_x0]);
                int y0 = int.Parse(loads[nr_load, loads_PROP_y0]);
                int pin1_x = 0, pin1_y = 0, pin1_x0 = 0, pin1_y0 = 0, pin2_x = 0, pin2_y = 0, pin2_x0 = 0, pin2_y0 = 0;
                //int pin1_x = 0, pin1_y = 0;
                if ((loads[nr_load, loads_PROP_gph_direction] == "N") || (loads[nr_load, loads_PROP_gph_direction] == "E"))
                {
                    // extend line terminals with polylines at 90 degree
                    int poly_x_final1 = 0, poly_y_final1 = 0;
                    if (loads[nr_load, loads_PROP_npoly1_xy] != "") // polylines connected to terminal 1 of the load are defined
                    {
                        string s1p = loads[nr_load, loads_PROP_npoly1_xy];
                        s1p = s1p.Replace("{", ""); s1p = s1p.Replace("}", "");
                        char[] delimiterChars1 = { ',' };
                        string[] line1 = s1p.Split(delimiterChars1);
                        for (int xy = 0; xy < line1.Length - 1; xy++)
                        {
                            int val = int.Parse(line1[xy]);
                            if ((xy % 2) == 0) // variabila x
                            {
                                poly_x_final1 += val;
                                //    lines_npolys[lines_no, xy, line_Terminal_1, 0] = poly_x_final1; // x
                                //    lines_npolys[lines_no, xy, line_Terminal_1, 1] = poly_y_final1; // y
                            }
                            else // variabila y
                            {
                                poly_y_final1 += int.Parse(line1[xy]);
                            }
                            loads_npolys[nr_load, xy, line_Terminal_1, 0] = poly_x_final1; // x
                            loads_npolys[nr_load, xy, line_Terminal_1, 1] = poly_y_final1; // y
                        }
                        if (loads[nr_load, loads_PROP_gph_direction] == "N")
                        {
                            if (loads[nr_load, loads_PROP_gph_DrawType] != "Ld0S1") { 
                                pin1_x0 = x0 + 2 + object_dx / 2;
                                pin1_y0 = y0 + 4;
                                pin1_x = x0 + object_dx / 2 + 2 + poly_x_final1;
                                pin1_y = y0 + 4 + poly_y_final1;
                                pin2_x = x0 + object_dx - 4;
                                pin2_y = y0 + 4 - 0;
                            }
                            else // "Ld0S1"
                            {
                                pin1_x0 = x0 + 23;
                                pin1_y0 = y0 + 4;
                                pin1_x = x0 + 23 + 2 + poly_x_final1;
                                pin1_y = y0 + 4 + poly_y_final1;
                                pin2_x = x0 + 55;
                                pin2_y = y0 + 4 - 0;
                            }
                        }
                        if (loads[nr_load, loads_PROP_gph_direction] == "E")
                        {
                            pin1_x0 = x0 - 0 + object_dx;
                            pin1_y0 = y0 - 4 + line_dy / 2;
                            pin1_x = x0 + object_dx + poly_x_final1;
                            pin1_y = y0 - 4 + poly_y_final1 + line_dy / 2;
                            pin2_x = x0 + object_dx - 4;
                            pin2_y = y0 + 4 - 0;
                        }
                        loads[nr_load, loads_PROP_pin1_x] = pin1_x.ToString();
                        loads[nr_load, loads_PROP_pin1_y] = pin1_y.ToString();
                        loads[nr_load, loads_PROP_pin1_x0] = pin1_x0.ToString();
                        loads[nr_load, loads_PROP_pin1_y0] = pin1_y0.ToString();
                    } else
                    {
                        if (loads[nr_load, loads_PROP_gph_DrawType] != "Ld0S1")
                        {
                            pin1_x = x0 + object_dx / 2; pin1_y = y0 + 2;
                        }
                        else // "Ld0S1"
                        {
                            pin1_x = x0 + 27; pin1_y = y0 + 2;
                        }
                        loads[nr_load, loads_PROP_pin1_x] = pin1_x.ToString();
                        loads[nr_load, loads_PROP_pin1_y] = pin1_y.ToString();
                        loads[nr_load, loads_PROP_pin1_x0] = pin1_x0.ToString();
                        loads[nr_load, loads_PROP_pin1_y0] = pin1_y0.ToString();
                    }
                }
                if (loads[nr_load, loads_PROP_gph_direction] == "W")
                {
                    pin1_x = x0; pin1_y = y0 + +line_dy / 2 - 4;
                    loads[nr_load, loads_PROP_pin1_x] = pin1_x.ToString();
                    loads[nr_load, loads_PROP_pin1_y] = pin1_y.ToString();
                }
                if (loads[nr_load, loads_PROP_gph_direction] == "S")
                {
                    pin1_x = x0 + object_dx / 2; pin1_y = y0 + line_dy - 2;
                    if (loads[nr_load, loads_PROP_sim_type] == "EV") { 
                        pin1_x = x0 + object_dx / 2; pin1_y = y0 + line_dy/2 - 2;
                    }
                    loads[nr_load, loads_PROP_pin1_x] = pin1_x.ToString();
                    loads[nr_load, loads_PROP_pin1_y] = pin1_y.ToString();
                }
                /*if (loads[nr_load, loads_PROP_gph_direction] == "E")
                {
                    pin1_x = x0 + object_dx-2; pin1_y = y0 + +line_dy / 2 - 4;
                    loads[nr_load, loads_PROP_pin1_x] = pin1_x.ToString();
                    loads[nr_load, loads_PROP_pin1_y] = pin1_y.ToString();
                }*/
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

            SolidBrush solidBrush = b2Blue;
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
                    object_x0 = (int)(object_x0 * zoom);
                    object_y0 = -Y0_shift + int.Parse(loads[i1, loads_PROP_y0]);
                    //object_y0 = (int)(object_y0 * zoom);
                    default_xy = 0;
                }

                // desenare dreptunghi general
                int object_dx_zoom = (int)(object_dx * zoom);
                if ((loads[i1, loads_PROP_sim_storage] == "") && (loads[i1, loads_PROP_sim_type] == "")) {
                    SolidBrush_crt = b13LightYellow;
                    double voltage = 0;
                    bool undervoltage = false;
                    if ((loads[i1, loads_PROP_U1] != "") && (loads[i1, loads_PROP_U2] != "") && (loads[i1, loads_PROP_U3] != ""))
                    {
                        // decide if the node is under the allowed voltage
                        // al three phases are scanned and the lowest voltage value is chosen
                        voltage = double.Parse(loads[i1, loads_PROP_U1]);
                        if(voltage > double.Parse(loads[i1, loads_PROP_U2])) voltage = double.Parse(loads[i1, loads_PROP_U2]);
                        if (voltage > double.Parse(loads[i1, loads_PROP_U3])) voltage = double.Parse(loads[i1, loads_PROP_U3]);
                        if (loads[i1, loads_PROP_U1] == "400") if (voltage < 400000 / 1.73205 * 0.1) undervoltage = true;
                        if (loads[i1, loads_PROP_U1] == "220") if (voltage < 220000 / 1.73205 * 0.1) undervoltage = true;
                        if (loads[i1, loads_PROP_U1] == "110") if (voltage < 110000 / 1.73205 * 0.1) undervoltage = true;
                        if (loads[i1, loads_PROP_U1] == "20") if (voltage < 11547 * 0.1) undervoltage = true;
                        if (loads[i1, loads_PROP_U1] == "0.4") if (voltage < 230 * 0.1) undervoltage = true;
                        // for detecting undervoltage, the LightSalmon color will be shown on the "load" rectangle
                        if (undervoltage==true) SolidBrush_crt = b10LightSalmon;
                    }
                    // if there is undervoltage on load, the entire rectangle becomes "b10LightSalmon"; if not, it is standard color
                    if (loads[i1, loads_PROP_gph_DrawType] != "Ld0S1")
                        g.FillRectangle(SolidBrush_crt, object_x0, object_y0, object_dx_zoom, line_dy);
                    else g.FillRectangle(SolidBrush_crt, object_x0, object_y0, object_dx_zoom-40, line_dy-30);
                }
                if (loads[i1, loads_PROP_sim_storage] == "stor")
                { // avem storage
                    g.FillRectangle(b7LightGreen, object_x0, object_y0, object_dx * 1 / 2, line_dy);
                    g.FillRectangle(b13LightYellow, object_x0 + object_dx * 1 / 2, object_y0, object_dx * 1 / 2, line_dy);

                }
                if (loads[i1, loads_PROP_sim_storage] == "Prosumer")
                { // avem storage

                    double Prosumer_SaaS = double.Parse(loads[i1, loads_PROP_Prosumer_SaaS]);

                    //Brush brush1 = new SolidBrush(Color.FromArgb(127, 255, 255, 255)); // b1White
                    Color halfLightGreen = Color.FromArgb(64, Color.LightGreen); ; // b1White
                    Brush brush1 = new SolidBrush(halfLightGreen); // b1White

                    Brush crt_green_brush1;
                    if ((Prosumer_SaaS > 0.1) || (Prosumer_SaaS < -0.1)) crt_green_brush1 = brush1;
                    else
                    { // The prosumer does not have a SaaS order
                        crt_green_brush1 = b7LightGreen;
                        loads[i1, loads_PROP_gph_Draw_Highlighted] = ""; // Highlight is resetted
                    }
                    //g.FillRectangle(b7LightGreen, object_x0, object_y0, object_dx * 1 / 2, line_dy * 3 / 5);

                    int dy1 = line_dy * 55 / 100;
                    int dy2 = line_dy * 45 / 100;
                    g.FillRectangle(brush1, object_x0, object_y0, object_dx * 1 / 2, dy1);
                    g.FillRectangle(b13LightYellow, object_x0 + object_dx * 1 / 2, object_y0, object_dx * 1 / 2, dy1);

                    g.FillRectangle(b13LightYellow, object_x0, object_y0 + dy1, object_dx * 1 / 2, dy2);
                    g.FillRectangle(brush1, object_x0 + object_dx * 1 / 2, object_y0 + dy1, object_dx * 1 / 2, dy2);
                    if (Prosumer_SaaS > 0.1) {
                        //g.FillRectangle(b13Yellow, object_x0, object_y0 + dy1, object_dx * 1 / 2, dy2);
                        g.FillRectangle(b13Yellow, object_x0, object_y0 + dy1, object_dx, dy2);
                        //g.FillRectangle(brush1, object_x0 + object_dx * 1 / 2, object_y0 + dy1, object_dx * 1 / 2, dy2);
                    }
                    //g.FillRectangle(b7LightGreen, object_x0 + object_dx * 1 / 2, object_y0 + line_dy * 3 / 5, object_dx * 1 / 2, line_dy * 2 / 5);
                    if (Prosumer_SaaS < - 0.1)
                        g.FillRectangle(b7LightGreen2, object_x0, object_y0 + dy1, object_dx, dy2);
                    //g.FillRectangle(b7LightGreen2, object_x0 + object_dx * 1 / 2, object_y0 + dy1, object_dx * 1 / 2, dy2);

                    if(loads[i1, loads_PROP_gph_Draw_Highlighted] == "1") { 
                        if(even_second==1) g.DrawRectangle(p5DarkBlue3, object_x0-1, object_y0-1, object_dx_zoom+2, line_dy+2);
                    }

                }
                if (loads[i1, loads_PROP_sim_type] == "EV")
                { // avem EV

                }

                // draw polylines ********************************** 
                if (loads[i1, loads_PROP_npoly1_xy] != "") // polylines for terminal 1
                {
                    int pin1_x = 0, pin1_y = 0, pin1_x0 = 0, pin1_y0 = 0;
                    pin1_x = int.Parse(loads[i1, loads_PROP_pin1_x]);
                    pin1_y = int.Parse(loads[i1, loads_PROP_pin1_y]);
                    pin1_x0 = int.Parse(loads[i1, loads_PROP_pin1_x0]);
                    pin1_y0 = int.Parse(loads[i1, loads_PROP_pin1_y0]);
                    for (int pln = 0; pln < 10; pln++)
                    {
                        int x10 = 0;
                        int y10 = 0;
                        int x1 = Convert.ToInt32(loads_npolys[i1, pln, line_Terminal_1, 0]);
                        int y1 = Convert.ToInt32(loads_npolys[i1, pln, line_Terminal_1, 1]);
                        if (pln == 0)
                            g.DrawLine(p5DarkBlue3, pin1_x0, pin1_y0, pin1_x0 + x1, pin1_y0 + y1);
                        else
                        {
                            x10 = Convert.ToInt32(loads_npolys[i1, pln - 1, line_Terminal_1, 0]);
                            y10 = Convert.ToInt32(loads_npolys[i1, pln - 1, line_Terminal_1, 1]);
                            //if ((pln == 1) && (x10 != 0)) g.DrawLine(p4_Line, pin1_x0, pin1_y0, pin1_x0 + x10, pin1_y0 + y10);
                            if ((x1 != 0) || (y1 != 0))
                                g.DrawLine(p4_Line, pin1_x0 + x10, pin1_y0 + y10, pin1_x0 + x1, pin1_y0 + y1);
                        }
                    }
                }

                if (loads[i1, loads_PROP_sim_type] == "") // avem load clasic, nu storage sau EV
                {
                    int y0res = 0;
                    string gph_direction = loads[i1, loads_PROP_gph_direction];
                    // draw the line symbol
                    if (gph_direction == "N")
                    {
                        if (loads[i1, loads_PROP_gph_DrawType] != "Ld0S1") { 
                            g.DrawLine(p1Black3, object_x0 + 20, object_y0 + 3 + 16, object_x0 + object_dx - 20, object_y0 + 3 + 16); // Bus of the load
                            g.DrawLine(p1Black, object_x0 + object_dx / 2, object_y0 + 6, object_x0 + object_dx / 2, object_y0 + 20); // Bus of the load

                            if (loads[i1, loads_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                            g.FillRectangle(SolidBrush_crt, object_x0 + object_dx / 2 - 4, object_y0 + 8, 9, 8); // Brk1

                            g.DrawEllipse(p5DarkBlue, object_x0 + object_dx / 2 - 3, object_y0, 6, 6); // Pin_Bus1
                            y0res = 20;
                        }
                        else // "Ld0S1"
                        {
                            g.DrawLine(p1Black3, object_x0 + 2, object_y0 + 3 + 16, object_x0 + 55, object_y0 + 3 + 16); // Bus of the load
                            g.DrawLine(p1Black, object_x0 + object_dx / 2 -20, object_y0 + 6, object_x0 + object_dx / 2 -20, object_y0 + 20); // Bus of the load

                            if (loads[i1, loads_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                            g.FillRectangle(SolidBrush_crt, object_x0 + object_dx / 2 - 24, object_y0 + 8, 9, 8); // Brk1

                            g.DrawEllipse(p5DarkBlue, object_x0 + 24, object_y0, 6, 6); // Pin_Bus1
                            y0res = 20;
                        }
                    }
                    if (gph_direction == "S")
                    {
                        g.DrawLine(p1Black3, object_x0 + 20, object_y0 + line_dy - 18, object_x0 + object_dx - 20, object_y0 + line_dy - 18); // Bus of the generator
                        g.DrawLine(p1Black, object_x0 + object_dx / 2, object_y0 + line_dy - 6, object_x0 + object_dx / 2, object_y0 + line_dy - 18); // Bus of the load

                        if (loads[i1, loads_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                        g.FillRectangle(SolidBrush_crt, object_x0 + object_dx / 2 - 4, object_y0 + line_dy - 15, 9, 8); // Brk1

                        g.DrawEllipse(p5DarkBlue, object_x0 + object_dx / 2 - 3, object_y0 + line_dy - 6, 6, 6); // Pin_Bus1

                        y0res = -1;
                    }
                    if (gph_direction == "W")
                    {
                            g.DrawLine(p1Black3, object_x0 + 3 + 16, object_y0 + 22 + 12, object_x0 + 3 + 16, object_y0 + line_dy - 44);
                            g.DrawLine(p1Black, object_x0 + 12, object_y0 + +line_dy / 2 - 5, object_x0 + 20, object_y0 + line_dy / 2 - 5); // Bus of the load

                            if (loads[i1, loads_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                            g.FillRectangle(SolidBrush_crt, object_x0 + 8, object_y0 + line_dy / 2 - 3 - 5, 8, 8); // Brk1

                            g.DrawEllipse(p5DarkBlue, object_x0, object_y0 + line_dy / 2 - 3 - 5, 6, 6); // Pin_Bus1
                            y0res = 1;
                    }
                    if (gph_direction == "E")
                    {
                        g.DrawLine(p1Black3, object_x0 + object_dx - 20, object_y0 + 22 + 12, object_x0 + object_dx - 20, object_y0 + line_dy - 44);
                        g.DrawLine(p1Black, object_x0 + object_dx - 20, object_y0 + +line_dy / 2 - 5, object_x0 + object_dx - 6, object_y0 + line_dy / 2 - 5); // Bus of the load

                        if (loads[i1, loads_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                        g.FillRectangle(SolidBrush_crt, object_x0 + object_dx - 16, object_y0 + line_dy / 2 - 3 - 5, 8, 8); // Brk1

                        g.DrawEllipse(p5DarkBlue, object_x0 + object_dx - 7, object_y0 + line_dy / 2 - 3 - 5, 6, 6); // Pin_Bus1
                        y0res = 1;
                    }
                    if (loads[i1, loads_PROP_sim_type] == "") if (loads[i1, loads_PROP_gph_DrawType] != "Ld0S1") s1 = "Ld="; else s1 = "";
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

                    double dval = 0, dval_mega = 0, force_mega = 0;
                    if (loads[i1, loads_PROP_sim_storage] == "Prosumer")
                    {
                        if (loads[i1, loads_PROP_Prosumer_SaaS] != "") {
                            dval = double.Parse(loads[i1, loads_PROP_Prosumer_SaaS])* double.Parse(prosumers[0, prosumers_PROP_P_scal_factor]);
                            s1 = dval.ToString("###0.0");
                        }
                        g.DrawString(s1, Font1, b0Black, object_x0 + 55, object_y0 + 1);
                    }
                    //loads[loads_no, loads_PROP_gph_DrawType]
                    s1 = "Bus=" + loads[i1, loads_PROP_bus];
                    if (loads[i1, loads_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + y0res + 10);
                    // Display Pn
                    if(loads[i1, loads_PROP_Pn] != "")
                        dval = double.Parse(loads[i1, loads_PROP_Pn])* Global_loads_factor; dval_mega = dval / 1000;
                    if ((dval>=100)||(dval<=-100)) s1 = "Pn=" + dval.ToString("##0");
                    else s1 = "Pn=" + dval.ToString("##0.0");
                    if (Math.Abs(dval) >= 10000) { s1 = "Pn=" + dval_mega.ToString("##0") + "M"; force_mega = 1; }
                    if(loads[i1, loads_PROP_gph_DrawType]=="") g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + y0res + 20);
                    if (loads[i1, loads_PROP_gph_DrawType] == "Ld1S1") g.DrawString(s1, Font3, b0Black, object_x0 + 1, object_y0 + y0res + 10);

                    // Display Qn or PF 
                    if (loads[i1, loads_PROP_Qn] != "")
                    {
                        dval = double.Parse(loads[i1, loads_PROP_Qn]) * Global_loads_factor; dval_mega = dval / 1000;
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
                        if (loads[i1, loads_PROP_PF] != "")
                            if (loads[i1, loads_PROP_gph_DrawType] != "Ld0S1")
                                g.DrawString(s1, Font1, b0Black, object_x0 + 55, object_y0 + y0res + 20);
                    }

                    if (loads[i1, loads_PROP_P] != "")
                    {
                        force_mega = 0;
                        P3f = double.Parse(loads[i1, loads_PROP_P]); P3f_mega = P3f / 1000;
                        if(P3f>=0) solidBrush = b2Blue; else solidBrush = b7DarkGreen;
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
                            if (loads[i1, loads_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, solidBrush, object_x0 + 1, object_y0 + 52);
                            if (loads[i1, loads_PROP_gph_DrawType] == "Ld1S1") g.DrawString(s1, Font3, solidBrush, object_x0 + 1, object_y0 + y0res + 36);
                            if (loads[i1, loads_PROP_gph_DrawType] == "Ld0S1")
                                g.DrawString(s1, Font1, solidBrush, object_x0 + 1, object_y0 + y0res + 12);
                        }
                        if (gph_direction == "S")
                        {
                            if (loads[i1, loads_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, solidBrush, object_x0 + 1, object_y0 + 32);
                        }
                        if (gph_direction == "W") g.DrawString(s1, Font1, solidBrush, object_x0 + 1, object_y0 + y0res + 50);
                        if (gph_direction == "E") g.DrawString(s1, Font1, solidBrush, object_x0 + 1, object_y0 + y0res + 50);
                    }
                    solidBrush = b2Blue;
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
                            if (loads[i1, loads_PROP_gph_DrawType] == "Ld0S1")
                                g.DrawString(s1, Font1, b2Blue, object_x0 + 1, object_y0 + y0res + 22);
                        }
                        if (gph_direction == "S")
                        {
                            if (loads[i1, loads_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, b2Blue, object_x0 + 50, object_y0 + 32);
                        }
                        if (gph_direction == "W") g.DrawString(s1, Font1, b2Blue, object_x0 + 50, object_y0 + y0res + 50);
                        if (gph_direction == "E") g.DrawString(s1, Font1, b2Blue, object_x0 + 50, object_y0 + y0res + 50);
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
                        if (loads[i1, loads_PROP_gph_DrawType] == "") // desenare in format standard
                        {
                            if (gph_direction == "N") g.DrawString(s1, Font_crt, b2Blue, object_x0 + 0, object_y0 + 62);
                            if (gph_direction == "W") g.DrawString(s1, Font_crt, b2Blue, object_x0 + 0, object_y0 + 62);
                            if (gph_direction == "S") g.DrawString(s1, Font_crt, b2Blue, object_x0 + 0, object_y0 + 42);
                            if (gph_direction == "E") g.DrawString(s1, Font_crt, b2Blue, object_x0 + 0, object_y0 + 62);
                        }
                        if (loads[i1, loads_PROP_gph_DrawType] == "Ld1S1") // desenerae in format special "Ld1S1"
                        {
                            g.DrawString(s1, Font3, b2Blue, object_x0 + 0, object_y0 + y0res + 62);
                            g.DrawString(s2, Font1, b2Blue, object_x0 + 64, object_y0 + y0res + 64);
                        }
                        if (loads[i1, loads_PROP_gph_DrawType] == "Ld0S1") // desenerae in format special "Ld1S1"
                        {
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + y0res + 32);
                            //g.DrawString(s2, Font1, b2Blue, object_x0 + 64, object_y0 + y0res + 64);
                        }
                        //g.DrawString(s1, Font1, b2Blue, object_x0 + 55, object_y0 + 72);

                        u = double.Parse(loads[i1, loads_PROP_U2]); u_kilo = u/1000; s1 = "U2=";
                        if (u >= 1000) s1 += u_kilo.ToString("#00.00") + "k"; else s1 += u.ToString("#00.0");
                        fi = double.Parse(loads[i1, loads_PROP_U2fi]);
                        if (u >= 1000) s1 += "  "; else s1 += "   ";
                        if (fi > 0) s1 += "+";
                        if (u >= 1000) s1 += fi.ToString("#00"); else s1 += fi.ToString("#00.0");
                        if (loads[i1, loads_PROP_gph_DrawType] == "")
                        {
                            if (gph_direction == "N") g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 72);
                            if (gph_direction == "W") g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 72);
                            if (gph_direction == "S") g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 52);
                            if (gph_direction == "E") g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 72);
                        }

                        u = double.Parse(loads[i1, loads_PROP_U3]); u_kilo = u/1000; s1 = "U3=";
                        if (u >= 1000) s1 += u_kilo.ToString("#00.00") + "k"; else s1 += u.ToString("#00.0");
                        fi = double.Parse(loads[i1, loads_PROP_U3fi]);
                        if (u >= 1000) s1 += "  "; else s1 += "   ";
                        if (fi > 0) s1 += "+";
                        if (u >= 1000) s1 += fi.ToString("#00"); else s1 += fi.ToString("#00.0");
                        if (loads[i1, loads_PROP_gph_DrawType] == "")
                        {
                            if (gph_direction == "N") g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 82);
                            if (gph_direction == "W") g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 82);
                            if (gph_direction == "S") g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 62);
                            if (gph_direction == "E") g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 82);
                        }

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
                    if (gph_direction == "S")
                    {
                        g.DrawLine(p1Black3, object_x0 + 20, object_y0 + line_dy - 18, object_x0 + object_dx - 20, object_y0 + line_dy - 18); // Bus of the generator
                        g.DrawLine(p1Black, object_x0 + object_dx / 2, object_y0 + line_dy - 6, object_x0 + object_dx / 2, object_y0 + line_dy - 18); // Bus of the load

                        if (loads[i1, loads_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                        g.FillRectangle(SolidBrush_crt, object_x0 + object_dx / 2 - 4, object_y0 + line_dy - 15, 9, 8); // Brk1

                        g.DrawEllipse(p5DarkBlue, object_x0 + object_dx / 2 - 3, object_y0 + line_dy - 6, 6, 6); // Pin_Bus1

                        y0res = -1;
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
                        if (gph_direction == "S") g.DrawString(s1, Font1, b2Blue, object_x0 + 1, object_y0 + 32);
                    }
                    if (loads[i1, loads_PROP_Q] != "")
                    {
                        Q3f = double.Parse(loads[i1, loads_PROP_Q]);
                        s1 = "Q=" + Q3f.ToString("#####0.0");
                        if (gph_direction == "N") g.DrawString(s1, Font1, b2Blue, object_x0 + 50, object_y0 + 52);
                        if (gph_direction == "W") g.DrawString(s1, Font1, b2Blue, object_x0 + 50, object_y0 + y0res + 50);
                        if (gph_direction == "S") g.DrawString(s1, Font1, b2Blue, object_x0 + 50, object_y0 + 32);
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
                        if (gph_direction == "N")
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 62);
                        if (gph_direction == "W")
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 62);
                        if (gph_direction == "S")
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 42);

                        u = double.Parse(loads[i1, loads_PROP_U2]); s1 = "U2=";
                        if (u >= 1000) s1 += u.ToString("#00"); else s1 += u.ToString("#00.0");
                        fi = double.Parse(loads[i1, loads_PROP_U2fi]);
                        if (u >= 1000) s1 += "  "; else s1 += "   ";
                        if (fi > 0) s1 += "+";
                        if (u >= 1000) s1 += fi.ToString("#00"); else s1 += fi.ToString("#00.0");
                        if (gph_direction == "N")
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 72);
                        if (gph_direction == "W")
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 72);
                        if (gph_direction == "S")
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 52);

                        u = double.Parse(loads[i1, loads_PROP_U3]); s1 = "U3=";
                        if (u >= 1000) s1 += u.ToString("#00"); else s1 += u.ToString("#00.0");
                        fi = double.Parse(loads[i1, loads_PROP_U3fi]);
                        if (u >= 1000) s1 += "  "; else s1 += "   ";
                        if (fi > 0) s1 += "+";
                        if (u >= 1000) s1 += fi.ToString("#00"); else s1 += fi.ToString("#00.0");
                        if (gph_direction == "N")
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 82);
                        if (gph_direction == "W")
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 82);
                        if (gph_direction == "S")
                            g.DrawString(s1, Font1, b2Blue, object_x0 + 0, object_y0 + 62);

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
        private void Generators_properties_calculation(int nr_generator)
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
                if (generators[nr_generator, generators_PROP_gph_direction] == "W")
                {
                    pin1_x = x0; pin1_y = y0 + +line_dy / 2 - 4;
                    generators[nr_generator, generators_PROP_pin1_x] = pin1_x.ToString();
                    generators[nr_generator, generators_PROP_pin1_y] = pin1_y.ToString();
                }
                if (generators[nr_generator, generators_PROP_gph_direction] == "S")
                {
                    pin1_x = x0 + object_dx / 2; pin1_y = y0 + line_dy - 2;
                    generators[nr_generator, generators_PROP_pin1_x] = pin1_x.ToString();
                    generators[nr_generator, generators_PROP_pin1_y] = pin1_y.ToString();
                }
                if (generators[nr_generator, generators_PROP_gph_direction] == "E")
                {
                    pin1_x = x0 + object_dx - 2; pin1_y = y0 + +line_dy / 2 - 4;
                    generators[nr_generator, generators_PROP_pin1_x] = pin1_x.ToString();
                    generators[nr_generator, generators_PROP_pin1_y] = pin1_y.ToString();
                }
            }
        }

        /****************** Desenare obiecte de tip generators**************/
        private void Paint_generators(object sender, PaintEventArgs e)
        {
            string s1 = "", s2 = "";
            string s1r = "", s1s = "", s1t = ""; // string diferit pentru fiecare faza
            string s2r = "", s2s = "", s2t = ""; // string diferit pentru fiecare faza
            //int obj_x = 0, obj_y = 0;

            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);

            SolidBrush solidBrush = b2Blue;
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
                }
                if (gph_direction == "S")
                {
                    g.DrawLine(p1Black3, object_x0 + 20, object_y0 + line_dy - 18, object_x0 + object_dx - 20, object_y0 + line_dy - 18); // Bus of the generator
                    g.DrawLine(p1Black, object_x0 + object_dx / 2, object_y0 + line_dy - 6, object_x0 + object_dx / 2, object_y0 + line_dy - 18); // Bus of the load

                    if (generators[i1, generators_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                    g.FillRectangle(SolidBrush_crt, object_x0 + object_dx / 2 - 4, object_y0 + line_dy - 15, 9, 8); // Brk1

                    g.DrawEllipse(p5DarkBlue, object_x0 + object_dx / 2 - 3, object_y0 + line_dy - 6, 6, 6); // Pin_Bus1

                    y0res = -1;
                }
                if (gph_direction == "W")
                {
                    g.DrawLine(p1Black3, object_x0 + 3 + 16, object_y0 + 22 + 12, object_x0 + 3 + 16, object_y0 + line_dy - 44);
                    g.DrawLine(p1Black, object_x0 + 12, object_y0 + +line_dy / 2 - 5, object_x0 + 20, object_y0 + line_dy / 2 - 5); // Bus of the load

                    if (generators[i1, generators_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                    g.FillRectangle(SolidBrush_crt, object_x0 + 8, object_y0 + line_dy / 2 - 3 - 5, 8, 8); // Brk1

                    g.DrawEllipse(p5DarkBlue, object_x0, object_y0 + line_dy / 2 - 3 - 5, 6, 6); // Pin_Bus1
                    y0res = 1;
                }
                if (gph_direction == "E")
                {
                    g.DrawLine(p1Black3, object_x0 + object_dx - 20, object_y0 + 22 + 12, object_x0 + object_dx - 20, object_y0 + line_dy - 44);
                    g.DrawLine(p1Black, object_x0 + object_dx - 20, object_y0 + +line_dy / 2 - 5, object_x0 + object_dx - 6, object_y0 + line_dy / 2 - 5); // Bus of the load

                    if (generators[i1, generators_PROP_brk] == "on") SolidBrush_crt = b2Blue; else SolidBrush_crt = b3DarkGray;
                    g.FillRectangle(SolidBrush_crt, object_x0 + object_dx - 16, object_y0 + line_dy / 2 - 3 - 5, 8, 8); // Brk1

                    g.DrawEllipse(p5DarkBlue, object_x0 + object_dx - 7, object_y0 + line_dy / 2 - 3 - 5, 6, 6); // Pin_Bus1
                    y0res = 1;
                }

                s1 = "Ge=" + generators[i1, generators_PROP_name];
                if (generators[i1, generators_PROP_gph_selected] == "1") g.DrawString(s1, Font1bold, b6Red, object_x0 + 15, object_y0 + y0res-2);
                else g.DrawString(s1, Font1, b0Black, object_x0 + 15, object_y0 + y0res-2);

                //s1 = "Bus=" + loads[i1, loads_PROP_bus];
                //if (loads[i1, loads_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + y0res + 10);
                s1 = "Bus=" + generators[i1, generators_PROP_bus];
                s1 += " Un=" + generators_double[i1, generators_PROP_voltage].ToString("#0");
                if (generators[i1, generators_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + y0res + 8);
                //s1 = "Ph=" + generators[i1, generators_PROP_phases]; g.DrawString(s1, Font1, b0Black, object_x0 + 6, object_y0 + 32);
                //s1 = "U=" + generators[i1, generators_PROP_voltage]; g.DrawString(s1, Font1, b0Black, object_x0 + 50, object_y0 + 32);

                double P3f = 0, P3f_mega = 0, force_mega = 0;
                if (generators[i1, generators_PROP_Pn] != "") {
                    P3f = double.Parse(generators[i1, generators_PROP_Pn]); P3f_mega = P3f / 1000;
                    //if (Math.Abs(P3f) >= 1000) s1 = "Pn=" + P3f_mega.ToString("#####0") + "M";
                    //if (Math.Abs(P3f) >= 100) s1 = "Pn=" + P3f_mega.ToString("#####0.0") + "M";
                    //else s1 = "Pn=" + P3f.ToString("#####0.00");

                    if (Math.Abs(P3f) >= 10000)
                    {
                        s1 = "Pn=" + P3f_mega.ToString("#####0.0") + "M";
                        if (Math.Abs(P3f_mega) >= 100) s1 = "Pn=" + P3f_mega.ToString("#####0") + "M";
                        force_mega = 1;
                    }
                    else
                    {
                        s1 = "Pn=" + P3f.ToString("#####0.00");
                        if (Math.Abs(P3f) >= 1000) s1 = "Pn=" + P3f.ToString("#####0");
                        else if (Math.Abs(P3f) >= 100) s1 = "Pn=" + P3f.ToString("#####0.0");
                    }
                    if (generators[i1, generators_PROP_gph_DrawType] == "") g.DrawString(s1, Font1, b0Black, object_x0 + 1, object_y0 + y0res + 20);
                    if (generators[i1, generators_PROP_gph_DrawType] == "G1S1") g.DrawString(s1, Font3, b0Black, object_x0 + 1, object_y0 + y0res + 9);
                }

                double Q3f = 0, Q3f_mega = 0;
                s1 = "Qn=" + generators[i1, generators_PROP_Qn];
                if (generators[i1, generators_PROP_Qn] != "")
                {
                    Q3f = double.Parse(generators[i1, generators_PROP_Qn]); Q3f_mega = Q3f / 1000;
                    if (Math.Abs(Q3f) >= 10000)
                    {
                        s1 = "Qn=" + Q3f_mega.ToString("#####0.0") + "M";
                        if (Math.Abs(Q3f_mega) >= 100) s1 = "Qn=" + Q3f_mega.ToString("#####0") + "M";
                        force_mega = 1;
                    }
                    else
                    {
                        s1 = "Qn=" + Q3f.ToString("#####0.00");
                        if (Math.Abs(Q3f) >= 1000) s1 = "Qn=" + Q3f.ToString("#####0");
                        else if (Math.Abs(Q3f) >= 100) s1 = "Qn=" + Q3f.ToString("#####0.0");
                    }
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
                    P3f = double.Parse(generators[i1, generators_PROP_P]);
                    P3f_mega = P3f / 1000;
                    if(P3f>=0) solidBrush = b2Blue; else solidBrush = b6Red;
                    if (Math.Abs(P3f) >= 10000)
                    {
                        s1 = "P=" + P3f_mega.ToString("#####0.0") + "M";
                        if (Math.Abs(P3f_mega) >= 100) s1 = "P=" + P3f_mega.ToString("#####0") + "M";
                        force_mega = 1;
                    }
                    else {
                        s1 = "P=" + P3f.ToString("#####0.00");
                        if (Math.Abs(P3f) >= 1000) s1 = "P=" + P3f.ToString("#####0");
                        else if (Math.Abs(P3f) >= 100) s1 = "P=" + P3f.ToString("#####0.0");
                    }
                    if (generators[i1, generators_PROP_gph_DrawType] == "")
                    {
                        if (gph_direction == "N") g.DrawString(s1, Font1, solidBrush, object_x0 + 1, object_y0 + 52);
                        if (gph_direction == "W") g.DrawString(s1, Font1, solidBrush, object_x0 + 1, object_y0 + 52);
                        if (gph_direction == "S") g.DrawString(s1, Font1, solidBrush, object_x0 + 1, object_y0 + 32);
                        if (gph_direction == "E") g.DrawString(s1, Font1, solidBrush, object_x0 + 1, object_y0 + 52);
                    }
                    if (generators[i1, generators_PROP_gph_DrawType] == "G1S1") g.DrawString(s1, Font3, solidBrush, object_x0 + 1, object_y0 + y0res + 35);
                }
                solidBrush = b2Blue;
                if (generators[i1, generators_PROP_Q] != "")
                {
                    Q3f = double.Parse(generators[i1, generators_PROP_Q]);
                    if ((Math.Abs(Q3f) >= 10000) || (force_mega == 1))
                    {
                        Q3f_mega = Q3f / 1000;
                        s1 = "Q=" + Q3f_mega.ToString("#####0.0") + "M";
                    }
                    else s1 = "Q=" + Q3f.ToString("#####0.00");

                    //g.DrawString(s1, Font1, b2Blue, object_x0 + 6, object_y0 + 72);
                    if (generators[i1, generators_PROP_gph_DrawType] == "")
                    {
                        if (gph_direction == "N") g.DrawString(s1, Font1, b2Blue, object_x0 + 50, object_y0 + 52);
                        if (gph_direction == "W") g.DrawString(s1, Font1, b2Blue, object_x0 + 50, object_y0 + 52);
                        if (gph_direction == "S") g.DrawString(s1, Font1, b2Blue, object_x0 + 50, object_y0 + 32);
                        if (gph_direction == "E") g.DrawString(s1, Font1, b2Blue, object_x0 + 50, object_y0 + 52);
                    }
                    if (generators[i1, generators_PROP_gph_DrawType] == "G1S1") g.DrawString(s1, Font3, b2Blue, object_x0 + 1, object_y0 + y0res + 48);
                }

                double u = 0, u_kilo = 0;
                double fi = 0;
                double fi1 = 0, fi2 = 0, fi3 = 0;
                if ((generators[i1, generators_PROP_U1] != "") && (generators[i1, generators_PROP_U2] != "") && (generators[i1, generators_PROP_U3] != "")
                    && (generators[i1, generators_PROP_U1fi] != ""))
                {
                    u = double.Parse(generators[i1, generators_PROP_U1]); u_kilo = u / 1000; s1r = "U1=";
                    if (u >= 1000) s1r += u_kilo.ToString("#00.00") + "k"; else s1r += u.ToString("#00.0");
                    u = double.Parse(generators[i1, generators_PROP_U2]); u_kilo = u / 1000; s1s = "U2=";
                    if (u >= 1000) s1s += u_kilo.ToString("#00.00") + "k"; else s1s += u.ToString("#00.0");
                    u = double.Parse(generators[i1, generators_PROP_U3]); u_kilo = u / 1000; s1t = "U3=";
                    if (u >= 1000) s1t += u_kilo.ToString("#00.00") + "k"; else s1t += u.ToString("#00.0");

                    fi1 = double.Parse(generators[i1, generators_PROP_U1fi]); if (u >= 1000) s1r += "  "; else s1r += "   ";
                    fi2 = double.Parse(generators[i1, generators_PROP_U2fi]); if (u >= 1000) s1s += "  "; else s1s += "   ";
                    fi3 = double.Parse(generators[i1, generators_PROP_U3fi]); if (u >= 1000) s1t += "  "; else s1t += "   ";
                    if (generators[i1, generators_PROP_gph_DrawType] == "")
                    {
                        if (fi1 > 0) s1 += "+"; s1r += fi1.ToString("#0.0");
                        if (fi2 > 0) s1 += "+"; s1s += fi2.ToString("#0.0");
                        if (fi3 > 0) s1 += "+"; s1t += fi3.ToString("#0.0");
                    }
                    s2r = fi1.ToString("#0.00");
                    s2s = fi2.ToString("#0.00");
                    s2t = fi3.ToString("#0.00");
                    Font_crt = new System.Drawing.Font("Arial", 8);
                    if (generators[i1, generators_PROP_gph_DrawType] == "")
                    {
                        if (gph_direction == "N")
                        {
                            g.DrawString(s1r, Font_crt, b2Blue, object_x0 + 0, object_y0 + 62);
                            g.DrawString(s1s, Font_crt, b2Blue, object_x0 + 0, object_y0 + 72);
                            g.DrawString(s1t, Font_crt, b2Blue, object_x0 + 0, object_y0 + 82);
                        }
                        if (gph_direction == "W")
                        {
                            g.DrawString(s1r, Font_crt, b2Blue, object_x0 + 0, object_y0 + 62);
                            g.DrawString(s1s, Font_crt, b2Blue, object_x0 + 0, object_y0 + 72);
                            g.DrawString(s1t, Font_crt, b2Blue, object_x0 + 0, object_y0 + 82);
                        }
                        if (gph_direction == "S")
                        {
                            g.DrawString(s1r, Font_crt, b2Blue, object_x0 + 0, object_y0 + 42);
                            g.DrawString(s1s, Font_crt, b2Blue, object_x0 + 0, object_y0 + 52);
                            g.DrawString(s1t, Font_crt, b2Blue, object_x0 + 0, object_y0 + 62);
                        }
                        if (gph_direction == "E")
                        {
                            g.DrawString(s1r, Font_crt, b2Blue, object_x0 + 0, object_y0 + 62);
                            g.DrawString(s1s, Font_crt, b2Blue, object_x0 + 0, object_y0 + 72);
                            g.DrawString(s1t, Font_crt, b2Blue, object_x0 + 0, object_y0 + 82);
                        }
                    }
                    if (generators[i1, generators_PROP_gph_DrawType] == "G1S1")
                    {
                        g.DrawString(s1r, Font3, b2Blue, object_x0 + 0, object_y0 + y0res + 61);
                        g.DrawString(s2r, Font1, b2Blue, object_x0 + 64, object_y0 + y0res + 63);
                        g.DrawString(s1s, Font3, b2Blue, object_x0 + 0, object_y0 + y0res + 81);
                        g.DrawString(s2s, Font1, b2Blue, object_x0 + 64, object_y0 + y0res + 83);
                        g.DrawString(s1t, Font3, b2Blue, object_x0 + 0, object_y0 + y0res + 101);
                        g.DrawString(s2t, Font1, b2Blue, object_x0 + 64, object_y0 + y0res + 103);
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
                //int default_xy = 1;
                if ((interracts[i1, nodes_PROP_x0] == "") || (interracts[i1, nodes_PROP_y0] == ""))
                {
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    //default_xy = 1;
                }
                else
                {
                    object_x0 = -X0_shift + int.Parse(interracts[i1, interracts_PROP_x0]);
                    object_y0 = -Y0_shift + int.Parse(interracts[i1, interracts_PROP_y0]);
                    //default_xy = 0;
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
                //int default_xy = 1;
                if ((labels[i1, labels_PROP_x0] == "") || (labels[i1, labels_PROP_y0] == ""))
                {
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    //default_xy = 1;
                }
                else
                {
                    object_x0 = -X0_shift + int.Parse(labels[i1, labels_PROP_x0]);
                    object_y0 = -Y0_shift + int.Parse(labels[i1, labels_PROP_y0]);
                    //default_xy = 0;
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
                //int default_xy = 1;
                if ((measurements[i1, measurements_PROP_x0] == "") || (measurements[i1, measurements_PROP_y0] == ""))
                {
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    //default_xy = 1;
                }
                else
                {
                    object_x0 = -X0_shift + int.Parse(measurements[i1, measurements_PROP_x0]);
                    object_y0 = -Y0_shift + int.Parse(measurements[i1, measurements_PROP_y0]);
                    //default_xy = 0;
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
                    
                    long dymas = 95;
                    /*if(trafos[0, trafos_PROP_S] != "") dymas = int.Parse(trafos[0, trafos_PROP_S]) * 95 / 630;
                    if((dymas>=0) && (dymas <= 95)) g.FillRectangle(b13Yellow, object_x0+2, object_y0 +dy - dymas, dx/4, dymas);
                    else if (dymas > 95) g.FillRectangle(b6Red, object_x0 + 2, object_y0 + dy - dymas, dx / 4, dymas);
                    else g.FillRectangle(b6Red, object_x0+2, object_y0 + dy + dymas, dx/4, -dymas);
                    */
                    dymas = 95;
                    double ymas_d = 0;
                    if (trafos[0, trafos_PROP_P] != "")
                    {
                        ymas_d = double.Parse(trafos[0, trafos_PROP_P]) * 95 / 630;
                        dymas = (long)Math.Ceiling(ymas_d);
                    }
                    if (dymas >= 0) g.FillRectangle(b0Black, object_x0 + 2 + dx / 4, object_y0 + dy - dymas, dx / 4, dymas);
                    else g.FillRectangle(b6Red, object_x0 + 2 + dx / 4, object_y0 + dy, dx / 4, -dymas);

                    dymas = 95;
                    //if (trafos[0, trafos_PROP_Q] != "") dymas = long.Parse(trafos[0, trafos_PROP_Q]) * 95 / 630; // !!! Nu stiu de ce a dat eroare
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
            //string s1 = "";

            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            //GraphicsPath path_clip = new GraphicsPath();
            //path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            //Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            //e.Graphics.SetClip(region, CombineMode.Replace);

            // Paint phasors
            for (int i1 = 0; i1 < graph_phasors_no; i1++)
            {
                //int default_xy = 1;
                if ((graph_phasors[i1, graph_phasors_PROP_x0] == "") || (graph_phasors[i1, graph_phasors_PROP_y0] == ""))
                {
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    //default_xy = 1;
                }
                else
                {
                    object_x0 = int.Parse(graph_phasors[i1, graph_phasors_PROP_x0]);
                    object_y0 = int.Parse(graph_phasors[i1, graph_phasors_PROP_y0]);
                    //default_xy = 0;
                }

                Paint_phasors(sender, e, i1);

            } // end painting graph_phasors
        }

        private void Paint_graphs(object sender, PaintEventArgs e)
        {
            //string s1 = "";

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);
            Graphics g = e.Graphics;

            // Paint graphs
            for (int i1 = 0; i1 < graphs_no; i1++)
            {
                //int default_xy = 1;
                if ((graphs[i1, graphs_PROP_x0] == "") || (graphs[i1, graphs_PROP_y0] == ""))
                {
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    //default_xy = 1;
                }
                else
                {
                    object_x0 = int.Parse(graphs[i1, graph_phasors_PROP_x0]);
                    object_y0 = int.Parse(graphs[i1, graph_phasors_PROP_y0]);
                    //default_xy = 0;
                }

                Paint_Gph1(sender, e, i1);

            } // end painting graphs
        }

        private void Paint_graph_sankeys(object sender, PaintEventArgs e)
        {
            //string s1 = "";

            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            //GraphicsPath path_clip = new GraphicsPath();
            //path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            //Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            //e.Graphics.SetClip(region, CombineMode.Replace);

            // Paint phasors
            for (int i1 = 0; i1 < graph_sankeys_no; i1++)
            {
                //int default_xy = 1;
                if ((graph_sankeys[i1, graph_sankeys_PROP_x0] == "") || (graph_sankeys[i1, graph_sankeys_PROP_y0] == ""))
                {
                    object_x0 = x0_unordered + object_dxtot * 3 / 5 * (obj_number % nr_obj_parked_ox) + 20 * (obj_number / nr_obj_parked_ox);
                    object_y0 = y0_unordered + line_dytot * 1 / 3 * (obj_number / nr_obj_parked_ox);
                    //default_xy = 1;
                }
                else
                {
                    object_x0 = int.Parse(graph_sankeys[i1, graph_sankeys_PROP_x0]);
                    object_y0 = int.Parse(graph_sankeys[i1, graph_sankeys_PROP_y0]);
                    //default_xy = 0;
                }

                Paint_sankeys(sender, e, i1);

            } // end painting graph_phasors
        }

        /****************** Desenare obiecte de tip node si alte functii ale acestui obiect ******************/
        private void Nodes_properties_calculation()
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
                nodes[nr_node, nodes_PROP_pin1_x0] = pin1_x.ToString();
                nodes[nr_node, nodes_PROP_pin1_y0] = pin1_y.ToString();
                // specific pin1
                if (nodes[nr_node, nodes_PROP_draw_type] == "0")
                {
                    pin1_x = x0 + 5; pin1_y = y0 + 5;
                    nodes[nr_node, nodes_PROP_pin1_x0] = pin1_x.ToString();
                    nodes[nr_node, nodes_PROP_pin1_y0] = pin1_y.ToString();
                }
                if (nodes[nr_node, nodes_PROP_arrow] != "")
                {
                    pin1_x = x0 + 81; pin1_y = y0 + 5;
                    nodes[nr_node, nodes_PROP_pin1_x0] = pin1_x.ToString();
                    nodes[nr_node, nodes_PROP_pin1_y0] = pin1_y.ToString();
                }
                // calculate pins position of nodes connectors inside the line representation on the grid picture
                // Node connector 1
                int x1 = int.Parse(nodes[nr_node, nodes_PROP_x1]);
                int y1 = int.Parse(nodes[nr_node, nodes_PROP_y1]);
                int pin1_x1 = 0, pin1_y1 = 0;
                // default pin1
                pin1_x1 = x1 + 2; pin1_y1 = y1 + 2;
                nodes[nr_node, nodes_PROP_pin1_x1] = pin1_x1.ToString();
                nodes[nr_node, nodes_PROP_pin1_y1] = pin1_y1.ToString();
                // Node connector 2
                int x2 = int.Parse(nodes[nr_node, nodes_PROP_x2]);
                int y2 = int.Parse(nodes[nr_node, nodes_PROP_y2]);
                int pin1_x2 = 0, pin1_y2 = 0;
                // default pin1
                pin1_x2 = x2 + 2; pin1_y2 = y2 + 2;
                nodes[nr_node, nodes_PROP_pin1_x2] = pin1_x2.ToString();
                nodes[nr_node, nodes_PROP_pin1_y2] = pin1_y2.ToString();

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
                name_dx = 0; name_dy = 0;
                U_dx = 0; U_dy = 0;
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
                    object_x0 = -X0_shift + int.Parse(nodes[i1, nodes_PROP_x0]); // Node drawing node connector 0 (default)
                    object_y0 = -Y0_shift + int.Parse(nodes[i1, nodes_PROP_y0]); // Node drawing node connector 0 (default)
                    default_xy = 0;

                    if ((nodes[i1, nodes_PROP_draw_type] == "") && (nodes_Draw_busbar[i1].Enable =="0"))
                    { 
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
                    if((nodes_Draw_busbar[i1].Enable == "0"))
                    {
                        object_x1 = -X0_shift + int.Parse(nodes[i1, nodes_PROP_x1]);
                        object_y1 = -Y0_shift + int.Parse(nodes[i1, nodes_PROP_y1]);
                        g.FillEllipse(b5DarkBlue, object_x1, object_y1, 5, 5); // Node drawing node connector 1
                        object_x1 = -X0_shift + int.Parse(nodes[i1, nodes_PROP_x2]);
                        object_y1 = -Y0_shift + int.Parse(nodes[i1, nodes_PROP_y2]);
                        g.FillEllipse(b5DarkBlue, object_x1, object_y1, 5, 5); // Node drawing node connector 2
                    }

                    if ((nodes[i1, nodes_PROP_draw_type] == "0") && (nodes_Draw_busbar[i1].Enable == "0"))
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

                    if (nodes_Draw_busbar[i1].Enable == "1") // we have a node organized as a busbar
                    {
                        // 8 = 2 x capate de busbar
                        int busbar_length = nodes_Draw_busbar[i1].Bay_size + (nodes_Draw_busbar[i1].Bays_number - 1) * nodes_Draw_busbar[i1].Bay_size;
                        if (nodes_Draw_busbar[i1].Direction == "N")
                            g.DrawLine(p5DarkBlue3, object_x0, object_y0 + 4, 
                                object_x0, object_y0 - busbar_length + 4); // Desenare simbol busbar
                        if (nodes_Draw_busbar[i1].Direction == "E")
                            g.DrawLine(p5DarkBlue3, object_x0, object_y0 + 3, 
                                object_x0 + busbar_length + 4, object_y0 + 3); // Desenare simbol busbar
                        for (int b1 = 0; b1 < nodes_Draw_busbar[i1].Bays_number; b1++)
                        {
                            if (nodes_Draw_busbar[i1].Direction == "N")
                                g.FillEllipse(b8Magenta, object_x0 + nodes_Draw_busbar[i1].coord_x[b1] - 3,
                                object_y0 + nodes_Draw_busbar[i1].coord_y[b1], 6, 6); // Pin_Bus stanga
                            if (nodes_Draw_busbar[i1].Direction == "E")
                                g.FillEllipse(b8Magenta, object_x0 + nodes_Draw_busbar[i1].coord_x[b1] - 3,
                                object_y0 + nodes_Draw_busbar[i1].coord_y[b1], 6, 6); // Pin_Bus stanga

                        }
                    }

                    if (nodes[i1, nodes_PROP_U1fi] != "")
                    {
                        U1fi = double.Parse(nodes[i1, nodes_PROP_U1fi]);
                        s1 = U1fi.ToString("##0.00");
                        if (nodes[i1, nodes_PROP_draw_U1fi] == "1") g.DrawString("U1ϕ = " + s1, Font1, b2Blue, object_x0 + 8 + U_dx, object_y0 - 23 + U_dy);
                    }
                    if (nodes_Draw_U_proc[i1].Visible == "1") // percentage of voltage can be displayed
                    {
                        try
                        {
                            if((nodes[i1, nodes_PROP_U1] != "") && (nodes[i1, nodes_PROP_voltage] != "")) { 
                                U1 = double.Parse(nodes[i1, nodes_PROP_U1]);
                                double Un = double.Parse(nodes[i1, nodes_PROP_voltage]) * 1000 / Math.Sqrt(3);
                                double U1proc = U1 / Un * 100.0;
                                s1 = U1proc.ToString("##0.00")+"%";
                                g.DrawString("U% = " + s1, Font1, b2Blue, object_x0 + 8 + nodes_Draw_U_proc[i1].x0, object_y0 - 33 + nodes_Draw_U_proc[i1].y0);
                            }
                        } catch {
                            Console.WriteLine("nodes_Draw_U_proc-Err1");
                        }
                    }
                }


                if (default_xy == 1) obj_number++;
            } // end painting nodes
        }


        private void Paint_polylines(object sender, PaintEventArgs e)
        {
            //string s1 = "";
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
            //string s1 = "";
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
                        if (lines[l1, lines_PROP_bus1] == nodes[n1, nodes_PROP_bus]) // if the line node 1 corrspond to a node
                        if ((lines[l1, lines_PROP_pin1_x] != "") && (nodes[n1, nodes_PROP_x0] != ""))
                        {
                            x1 = int.Parse(lines[l1, lines_PROP_pin1_x]) - X0_shift;
                            y1 = int.Parse(lines[l1, lines_PROP_pin1_y]) - Y0_shift;
                            if(nodes_Draw_busbar[n1].Enable == "0") { 
                                x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x0]) - X0_shift;
                                y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y0]) - Y0_shift;
                            } else
                                {
                                    int poz = 0;
                                    if (lines[l1, lines_PROP_bus1poz] != "") poz = int.Parse(lines[l1, lines_PROP_bus1poz]);
                                    x2 = int.Parse(nodes[n1, nodes_PROP_x0]) + nodes_Draw_busbar[n1].coord_x[poz] - X0_shift;
                                    y2 = 2 + int.Parse(nodes[n1, nodes_PROP_y0]) + nodes_Draw_busbar[n1].coord_y[poz] - Y0_shift;
                                }
                                px = p1Black;
                            double voltage = 0;
                            if (nodes[n1, nodes_PROP_U1] != "")
                            {
                                // decide if the node is under the allowed voltage
                                voltage = double.Parse(nodes[n1, nodes_PROP_U1]);
                                if (nodes[n1, nodes_PROP_voltage] == "400") if (voltage < 400000 / 1.73205 * 0.95) px = p6Red2;
                                if (nodes[n1, nodes_PROP_voltage] == "230") if (voltage < 230000 / 1.73205 * 0.95) px = p6Red2;
                                if (nodes[n1, nodes_PROP_voltage] == "220") if (voltage < 220000 / 1.73205 * 0.95) px = p6Red2;
                                if (nodes[n1, nodes_PROP_voltage] == "110") if (voltage < 110000 / 1.73205 * 0.95) px = p6Red2;
                                if (nodes[n1, nodes_PROP_voltage] == "20") if (voltage < 11547 * 0.95) px = p6Red2;
                                if (nodes[n1, nodes_PROP_voltage] == "0.4") if (voltage < 230 * 0.90) px = p6Red2;
                            }
                            g.DrawLine(px, x1, y1, x2, y2);
                        }
                        if (lines[l1, lines_PROP_bus2] == nodes[n1, nodes_PROP_bus])
                        if ((lines[l1, lines_PROP_pin2_x] != "") && (nodes[n1, nodes_PROP_x0] != ""))
                        {
                            x1 = int.Parse(lines[l1, lines_PROP_pin2_x]) - X0_shift;
                            y1 = int.Parse(lines[l1, lines_PROP_pin2_y]) - Y0_shift;
                            if (nodes_Draw_busbar[n1].Enable == "0")
                            {
                                x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x0]) - X0_shift;
                                y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y0]) - Y0_shift;
                            }
                            else
                            {
                                int poz = 0;
                                if(lines[l1, lines_PROP_bus2poz] != "") poz = int.Parse(lines[l1, lines_PROP_bus2poz]);
                                x2 = int.Parse(nodes[n1, nodes_PROP_x0]) + nodes_Draw_busbar[n1].coord_x[poz] - X0_shift;
                                y2 = 2 + int.Parse(nodes[n1, nodes_PROP_y0]) + nodes_Draw_busbar[n1].coord_y[poz] - Y0_shift;
                            }
                            //x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x0]) - X0_shift;
                            //y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y0]) - Y0_shift;
                            px = p1Black; // normal color is black
                            double voltage = 0;
                            if (nodes[n1, nodes_PROP_U1] != "") {
                                // decide if the node is under the allowed voltage
                                voltage = double.Parse(nodes[n1, nodes_PROP_U1]);
                                if (nodes[n1, nodes_PROP_voltage] == "400") if (voltage < 400000 / 1.73205 * 0.95) px = p6Red2;
                                if (nodes[n1, nodes_PROP_voltage] == "230") if (voltage < 230000 / 1.73205 * 0.95) px = p6Red2;
                                if (nodes[n1, nodes_PROP_voltage] == "220") if (voltage < 220000 / 1.73205 * 0.95) px = p6Red2;
                                if (nodes[n1, nodes_PROP_voltage] == "110") if (voltage < 110000 / 1.73205 * 0.95) px = p6Red2;
                                if (nodes[n1, nodes_PROP_voltage] == "20") if (voltage < 11547 * 0.95) px = p6Red2;
                                if (nodes[n1, nodes_PROP_voltage] == "0.4") if (voltage < 230 * 0.90) px = p6Red2;
                            }
                            g.DrawLine(px, x1, y1, x2, y2);
                        }
            
                    }

                    // Trafos to nodes 
                    for (int l1 = 0; l1 < trafos_no; l1++)
                    {
                        if (trafos[l1, trafos_PROP_bus1] == nodes[n1, nodes_PROP_bus])
                            if ((trafos[l1, trafos_PROP_pin1_x] != "") && (nodes[n1, nodes_PROP_x0] != ""))
                            {
                                //x1 = int.Parse(trafos[l1, trafos_PROP_pin1_x]) - X0_shift;
                                //y1 = int.Parse(trafos[l1, trafos_PROP_pin1_y]) - Y0_shift;
                                //x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x0]) - X0_shift;
                                //y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y0]) - Y0_shift;
                                x1 = int.Parse(trafos[l1, trafos_PROP_pin1_x]) - X0_shift;
                                y1 = int.Parse(trafos[l1, trafos_PROP_pin1_y]) - Y0_shift;
                                if (nodes_Draw_busbar[n1].Enable == "0")
                                { // nu este valid busbar-ul
                                    //x2 = int.Parse(trafos[n1, trafos_PROP_pin1_x0]) - X0_shift; // default node position is position 0
                                    //y2 = int.Parse(trafos[n1, trafos_PROP_pin1_y0]) - Y0_shift; // default node position is position 0
                                    x2 = int.Parse(trafos[l1, trafos_PROP_pin1_x0]) - X0_shift; // default node position is position 0
                                    y2 = int.Parse(trafos[l1, trafos_PROP_pin1_y0]) - Y0_shift; // default node position is position 0
                                }
                                else
                                {
                                    int poz = 0;
                                    if (trafos[l1, trafos_PROP_bus_poz] != "") poz = int.Parse(trafos[l1, trafos_PROP_bus_poz]);
                                    x2 = int.Parse(nodes[n1, nodes_PROP_x0]) + nodes_Draw_busbar[n1].coord_x[poz] - X0_shift;
                                    y2 = 2 + int.Parse(nodes[n1, nodes_PROP_y0]) + nodes_Draw_busbar[n1].coord_y[poz] - Y0_shift;
                                }
                                if (trafos[l1, trafos_PROP_node_auto_draw] == "1")
                                {
                                    x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x1]) - X0_shift; // node position is 1
                                    y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y1]) - Y0_shift; // node position is 1
                                }
                                if (trafos[l1, trafos_PROP_node_auto_draw] == "2")
                                {
                                    x2 = int.Parse(trafos[n1, trafos_PROP_pin1_x2]) - X0_shift; // node position is 1
                                    y2 = int.Parse(trafos[n1, trafos_PROP_pin1_y2]) - Y0_shift; // node position is 1
                                }

                                g.DrawLine(p1Black, x1, y1, x2, y2);
                            }
                        if (trafos[l1, trafos_PROP_bus2] == nodes[n1, nodes_PROP_bus])
                            if ((trafos[l1, trafos_PROP_pin2_x] != "") && (nodes[n1, nodes_PROP_x0] != ""))
                            {
                                x1 = int.Parse(trafos[l1, trafos_PROP_pin2_x]) - X0_shift;
                                y1 = int.Parse(trafos[l1, trafos_PROP_pin2_y]) - Y0_shift;
                                x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x0]) - X0_shift;
                                y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y0]) - Y0_shift;
                                g.DrawLine(p1Black, x1, y1, x2, y2);
                            }
                    }

                    // Loads to nodes 
                    for (int l1 = 0; l1 < loads_no; l1++)
                    {
                        if (loads[l1, loads_PROP_bus] == nodes[n1, nodes_PROP_bus])
                            if ((loads[l1, loads_PROP_pin1_x] != "") && (nodes[n1, nodes_PROP_x0] != ""))
                            {
                                x1 = int.Parse(loads[l1, loads_PROP_pin1_x]) - X0_shift;
                                y1 = int.Parse(loads[l1, loads_PROP_pin1_y]) - Y0_shift;
                                if (nodes_Draw_busbar[n1].Enable == "0") { // nu este valid busbar-ul
                                    x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x0]) - X0_shift; // default node position is position 0
                                    y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y0]) - Y0_shift; // default node position is position 0
                                } else
                                {
                                    int poz = 0;
                                    if (loads[l1, loads_PROP_bus_poz] != "") poz = int.Parse(loads[l1, loads_PROP_bus_poz]);
                                    x2 = int.Parse(nodes[n1, nodes_PROP_x0]) + nodes_Draw_busbar[n1].coord_x[poz] - X0_shift;
                                    y2 = 2 + int.Parse(nodes[n1, nodes_PROP_y0]) + nodes_Draw_busbar[n1].coord_y[poz] - Y0_shift;
                                }
                                if (loads[l1, loads_PROP_node_auto_draw] == "1")
                                {
                                    x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x1]) - X0_shift; // node position is 1
                                    y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y1]) - Y0_shift; // node position is 1
                                }
                                if (loads[l1, loads_PROP_node_auto_draw] == "2")
                                {
                                    x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x2]) - X0_shift; // node position is 1
                                    y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y2]) - Y0_shift; // node position is 1
                                }
                                px = p1Black;
                                double voltage = 0;
                                if (nodes[n1, nodes_PROP_U1] != "")
                                {
                                    // decide if the node is under the allowed voltage
                                    voltage = double.Parse(nodes[n1, nodes_PROP_U1]);
                                    if (nodes[n1, nodes_PROP_voltage] == "400") if (voltage < 400000 / 1.73205 * 0.95) px = p6Red2;
                                    if (nodes[n1, nodes_PROP_voltage] == "230") if (voltage < 230000 / 1.73205 * 0.95) px = p6Red2;
                                    if (nodes[n1, nodes_PROP_voltage] == "220") if (voltage < 220000 / 1.73205 * 0.95) px = p6Red2;
                                    if (nodes[n1, nodes_PROP_voltage] == "110") if (voltage < 110000 / 1.73205 * 0.95) px = p6Red2;
                                    if (nodes[n1, nodes_PROP_voltage] == "20") if (voltage < 11547 * 0.95) px = p6Red2;
                                    if (nodes[n1, nodes_PROP_voltage] == "0.4") if (voltage < 230 * 0.90) px = p6Red2;
                                }
                                //if (nodes[n1, nodes_PROP_U1] != "")
                                //    if (double.Parse(nodes[n1, nodes_PROP_U1]) < 11500 * 0.95) px = p6Red2;
                                g.DrawLine(px, x1, y1, x2, y2);
                            }
                    }
                    // Generators to nodes 
                    for (int l1 = 0; l1 < generators_no; l1++)
                    {
                        if (generators[l1, generators_PROP_bus] == nodes[n1, nodes_PROP_bus])
                            if ((generators[l1, generators_PROP_pin1_x] != "") && (nodes[n1, nodes_PROP_x0] != ""))
                            {
                                x1 = int.Parse(generators[l1, generators_PROP_pin1_x]) - X0_shift;
                                y1 = int.Parse(generators[l1, generators_PROP_pin1_y]) - Y0_shift;
                                if (nodes_Draw_busbar[n1].Enable == "0")
                                {
                                    x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x0]) - X0_shift;
                                    y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y0]) - Y0_shift;
                                } else
                                {
                                    int poz = 0;
                                    if (generators[l1, generators_PROP_bus_poz] != "")
                                        poz = int.Parse(generators[l1, generators_PROP_bus_poz]);
                                    x2 = int.Parse(nodes[n1, nodes_PROP_x0]) + nodes_Draw_busbar[n1].coord_x[poz] - X0_shift;
                                    y2 = 3 + int.Parse(nodes[n1, nodes_PROP_y0]) + nodes_Draw_busbar[n1].coord_y[poz] - Y0_shift;
                                }
                                if (generators[l1, generators_PROP_node_auto_draw] == "1")
                                {
                                    x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x1]) - X0_shift; // node position is 1
                                    y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y1]) - Y0_shift; // node position is 1
                                }
                                if (generators[l1, generators_PROP_node_auto_draw] == "2")
                                {
                                    x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x2]) - X0_shift; // node position is 1
                                    y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y2]) - Y0_shift; // node position is 1
                                }
                                g.DrawLine(p1Black, x1, y1, x2, y2);
                            }
                    }
                    // Nodes to nodes 
                    for (int l1 = 0; l1 < nodes_no; l1++)
                    {
                        if ((nodes[n1, nodes_PROP_x0] != "") && (nodes[n1, nodes_PROP_x1] != ""))
                        {
                            x1 = int.Parse(nodes[n1, nodes_PROP_pin1_x0]) - X0_shift;
                            y1 = int.Parse(nodes[n1, nodes_PROP_pin1_y0]) - Y0_shift;
                            x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x1]) - X0_shift;
                            y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y1]) - Y0_shift;
                            g.DrawLine(p1Black, x1, y1, x2, y2);
                        }
                        if ((nodes[n1, nodes_PROP_con2from] == "") || (nodes[n1, nodes_PROP_con2from] == "0"))
                            if ((nodes[n1, nodes_PROP_x1] != "") && (nodes[n1, nodes_PROP_x2] != ""))
                            {
                                x1 = int.Parse(nodes[n1, nodes_PROP_pin1_x0]) - X0_shift;
                                y1 = int.Parse(nodes[n1, nodes_PROP_pin1_y0]) - Y0_shift;
                                x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x2]) - X0_shift;
                                y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y2]) - Y0_shift;
                                g.DrawLine(p1Black, x1, y1, x2, y2);
                            }
                        if (nodes[n1, nodes_PROP_con2from] == "1")
                        if ((nodes[n1, nodes_PROP_x1] != "") && (nodes[n1, nodes_PROP_x2] != ""))
                        {
                            x1 = int.Parse(nodes[n1, nodes_PROP_pin1_x1]) - X0_shift;
                            y1 = int.Parse(nodes[n1, nodes_PROP_pin1_y1]) - Y0_shift;
                            x2 = int.Parse(nodes[n1, nodes_PROP_pin1_x2]) - X0_shift;
                            y2 = int.Parse(nodes[n1, nodes_PROP_pin1_y2]) - Y0_shift;
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
            double pie_size = 1; // 1 means 100%, while 0.7 means 70%

            for (int i1 = 0; i1 < graph_pies_no; i1++)
            {
                int value_1 = 0;
                int value_2 = 0;
                int value_3 = 0;
                double value_1d = 0;
                double value_2d = 0;
                double value_3d = 0;
                string s1 = "";
                Font Font_crt = Font1; // Font("Arial", 8)
                pie_size = graph_pies_double[i1, graph_pies_PROP_size];

                int value_max = 100; // value max, at right part fo the pie 
                double value_max_d = 100;
                int value_min = -100; // value min, at left part fo the pie
                double value_min_d = -100;
                int value_max_norm = 100; // normal value max, at right part fo the pie 
                int value_min_norm = -100; // normal value min, at left part fo the pie
                int obj_number = -1;
                int value_center = 0;
                double value_center_d = 0;
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
                    if (obj_type == "line")
                    {
                        if (lines[obj_number, lines_PROP_P] != "")
                        {
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

                if (pie_meas_type == "1U")
                {
                    if (obj_type == "load")
                    {
                        if (loads[obj_number, loads_PROP_U1] != "")
                        {
                            value_1d = double.Parse(loads[obj_number, loads_PROP_U1]);
                            value_1 = (int)value_1d;//.Parse(lines[16, lines_PROP_P]);
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
                    if (obj_type == "line")
                    {
                        string bus = graph_pies[i1, graph_pies_PROP_bus];
                        if (bus != "2") bus = "1";
                        if (lines[obj_number, generators_PROP_U1] != "")
                        {
                            value_1d = double.Parse(lines[obj_number, lines_PROP_U1]); // by defaault it is bus=1
                            if (bus == "2") value_1d = double.Parse(lines[obj_number, lines_PROP_U1_t2]);
                            value_1 = (int)value_1d;//.Parse(lines[16, lines_PROP_P]);
                        };
                        if (lines[obj_number, generators_PROP_U2] != "")
                        {
                            value_2d = double.Parse(lines[obj_number, lines_PROP_U2]);
                            if (bus == "2") value_2d = double.Parse(lines[obj_number, lines_PROP_U2_t2]);
                            value_2 = (int)value_2d;//.Parse(lines[16, lines_PROP_Q]);
                        };
                        if (lines[obj_number, generators_PROP_U3] != "")
                        {
                            value_3d = double.Parse(lines[obj_number, lines_PROP_U3]);
                            if (bus == "2") value_3d = double.Parse(lines[obj_number, lines_PROP_U3_t2]);
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

                int diam_max = (int)Math.Truncate(120 * pie_size);

                int diam_max_y = 120;
                diam_max_y = (int)Math.Truncate(120 * pie_size);
                if (pie_size >= 0.75) diam_max_y = (int)Math.Truncate(120 * pie_size);
                else diam_max_y = (int)Math.Truncate(100 * pie_size);

                int text_type = 1;

                if (pie_meas_type == "PQ") { 
                    e.Graphics.FillPie(b13LightYellow, x0, y0, diam_max, diam_max_y, -180, 180);
                    e.Graphics.FillPie(b2Blue, x0, y0,
                        diam_max, diam_max_y, -90 - (int)(1.0 * 90 * value_1 / value_max), (int)(1.0 * 90 * value_1 / value_max)); // measurement 1
                    e.Graphics.FillPie(b7LightGreen, x0 + 8, y0 + 8,
                        diam_max - 16, diam_max_y - 16, -90 - (int)(1.0 * 90 * value_2 / value_max), 
                        (int)(1.0 * 90 * value_2 / value_max));  // measurement 2
                    e.Graphics.FillPie(b1White, x0 + 20, y0 + 20, diam_max - 40, diam_max_y - 40, -180, 180);
                    e.Graphics.DrawArc(p1Black, x0, y0, diam_max, diam_max_y, -180, 180);
                    e.Graphics.DrawArc(p1Black, x0 + 20, y0 + 20, diam_max - 40, diam_max_y - 40, -180, 180);
                    e.Graphics.DrawLine(p1Black, x0 + diam_max / 2, y0, x0 + diam_max / 2, y0 + 20);
                    e.Graphics.DrawLine(p1Black, x0, y0 + diam_max_y / 2, x0 + 20, y0 + diam_max_y / 2);
                    e.Graphics.DrawLine(p1Black, x0 + diam_max, y0 + diam_max_y / 2, x0 + diam_max - 20, y0 + diam_max_y / 2);
                    if (text_type == 1)
                    {
                        Font_crt = Font1;  if (pie_size < 0.75) Font_crt = Font0; // Chose font size as function of pie_dim
                        string multiplier = "k";
                        if(Math.Abs(value_1d)>=1000) { value_1d = value_1d / 1000; value_2d = value_2d / 1000; multiplier = "M"; }
                        if (Math.Abs(value_1d) < 100) s1 = value_1d.ToString("#0.00");
                            else s1 = value_1d.ToString("#0.0");
                        g.DrawString("P=" + s1 + multiplier, Font_crt, b5DarkBlue, x0 - 2, y0 + diam_max_y / 2 + 2); // 
                        g.DrawString("Q=" + value_2d.ToString("#0.00") + multiplier, Font_crt, b7Green, x0 + diam_max / 2, y0 + diam_max_y / 2 + 2);
                    }
                    if (text_type == 2)
                    {
                        Font_crt = Font3; if (pie_size < 0.75) Font_crt = Font1; // Chose font size as function of pie_dim
                        g.DrawString("P=" + value_1d.ToString("#0.00"), Font_crt, b5DarkBlue, x0 + 10, y0 + diam_max_y / 2); // 
                        g.DrawString("Q=" + value_2d.ToString("#0.00"), Font_crt, b7Green, x0 + 10, y0 + diam_max_y / 2 + 12);
                    }
                    // print minimal value on left side
                    Font_crt = Font1; if (pie_size < 0.75) Font_crt = Font0; // Chose font size as function of pie_dim
                    if (Math.Abs(value_min) >= 1000) s1 = (value_min / 1000).ToString("###0") + "M";
                        else s1 = value_min.ToString("###0") + "k";
                    if (pie_size >= 0.75) g.DrawString("" + s1, Font_crt, b5DarkBlue, x0 + 21, y0 + diam_max / 2 - 12);
                    else g.DrawString("" + s1, Font_crt, b5DarkBlue, x0 - 2, y0 - 2); // <0.75
                    //else g.DrawString("" + s1, Font_crt, b5DarkBlue, x0 + 23, y0 + diam_max_y / 2 - 8); // <0.75
                    // print maximal value on right side
                    if (Math.Abs(value_max) >= 1000) s1 = (value_max / 1000).ToString("###0") + "M";
                        else s1 = value_max.ToString("###0") + "k";
                    if (pie_size >= 0.75) g.DrawString(s1, Font_crt, b5DarkBlue, x0 + diam_max - 50, y0 + diam_max / 2 - 12);
                    else g.DrawString(s1, Font_crt, b5DarkBlue, x0 + diam_max - 20, y0 - 2); // <0.75
                    //else g.DrawString(s1, Font_crt, b5DarkBlue, x0 + diam_max - 40, y0 + diam_max_y / 2 - 8); // <0.75

                    Font_crt = Font1; if (pie_size < 0.75) Font_crt = Font0; // Chose font size as function of pie_dim
                    if (pie_size >= 0.75) g.DrawString("0", Font_crt, b5DarkBlue, x0 + diam_max / 2 - 5, y0 + 20);
                    else g.DrawString("0", Font_crt, b5DarkBlue, x0 + diam_max / 2 - 5, y0 + 18);
                    Font_crt = Font1;
                    if (obj_type == "line") g.DrawString("Ln=" + lines[obj_number, lines_PROP_name], Font_crt, b5DarkBlue, x0 + diam_max / 2 - 30, y0 - 15);
                    if (obj_type == "load") g.DrawString("Ld=" + loads[obj_number, loads_PROP_name], Font_crt, b5DarkBlue, x0 + diam_max / 2 - 30, y0 - 15);
                    if (obj_type == "generator") g.DrawString("Gen=" + generators[obj_number, generators_PROP_name], Font_crt, b5DarkBlue, x0 + diam_max / 2 - 30, y0 - 15);
                }
                if (pie_meas_type == "3U")
                {
                    Font_crt = Font1;
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

                    string sd = "";

                    sd = value_1d.ToString("#0.00");
                    if (Math.Abs(value_1d) >= 1000)
                    {
                        value_1d = value_1d / 1000.0;
                        sd = value_1d.ToString("###0.0") + "k";
                    }
                    g.DrawString("U1=" + sd, Font1, b6Red, x0 - 2, y0 + diam_max / 2 + 2); // 
                    sd = value_2d.ToString("#0.00");
                    if (Math.Abs(value_2d) >= 1000)
                    {
                        value_2d = value_2d / 1000.0;
                        sd = value_2d.ToString("###0.0") + "k";
                    }
                    g.DrawString("U2=" + sd, Font1, b11Orange, x0 + diam_max / 2, y0 + diam_max / 2 + 2);
                    sd = value_3d.ToString("#0.00");
                    if (Math.Abs(value_3d) >= 1000)
                    {
                        value_3d = value_3d / 1000.0;
                        sd = value_3d.ToString("###0.0") + "k";
                    }
                    g.DrawString("U3=" + sd, Font1, b5DarkBlue, x0 + diam_max / 3, y0 + diam_max / 2 + 12);

                    value_min_d = value_min;
                    sd = value_min_d.ToString();
                    if (Math.Abs(value_min) >= 1000)
                    {
                        value_min_d = value_min / 1000.0;
                        sd = value_min_d.ToString("###0.0") + "k";
                    }
                    g.DrawString("" + sd, Font1, b5DarkBlue, x0 + 23, y0 + diam_max / 2 - 12);
                    value_max_d = value_max;
                    sd = value_max_d.ToString();
                    if (Math.Abs(value_max) >= 1000)
                    {
                        value_max_d = value_max / 1000.0;
                        sd = value_max_d.ToString("###0.0") + "k";
                    }
                    g.DrawString(sd, Font1, b5DarkBlue, x0 + diam_max - 56, y0 + diam_max / 2 - 12);

                    value_center_d = value_center;
                    sd = value_center_d.ToString();
                    if (value_center>=1000)
                    {
                        value_center_d = value_center / 1000;
                        sd = value_center_d.ToString("###0.0")+"k";
                    }
                    g.DrawString(sd, Font1, b5DarkBlue, x0 + diam_max / 2 - 12, y0 + 20);
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

                    if (Math.Abs(value_min) >= 1000) s1 = (value_min / 1000).ToString("###0") + "k";
                        else s1 = value_min.ToString("###0");
                    g.DrawString("" + s1, Font1, b5DarkBlue, x0 + 23, y0 + diam_max / 2 - 12); // valoare de mijloc
                    if (Math.Abs(value_max) >= 1000) s1 = (value_max/1000).ToString("###0")+"k";
                        else s1 = value_max.ToString("###0");
                    g.DrawString(s1, Font1, b5DarkBlue, x0 + diam_max - 50, y0 + diam_max / 2 - 12);
                    //s1 = 
                    g.DrawString(value_center.ToString(), Font1, b5DarkBlue, x0 + diam_max / 2 - 12, y0 + 20);
                    g.DrawString("Ln=" + lines[obj_number, lines_PROP_name], Font1, b5DarkBlue, x0 + diam_max / 2 - 30, y0 - 15);
                }
            }
        }

        private void Paint_S4G_HIL(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            //string obj_type = "";
            if(activate_S4G_HIL == "yes") { 
                richTextBox_events.Text = "counter\ttime elapsed\tLESSAg time\ttotal time elapsed\n";
                richTextBox_events.Text += S4G_HIL_counter.ToString() + "\t" + S4G_HIL_dt + "\t" + S4G_HIL_LESSAg_dt + "\t" + S4G_HIL_timer;
            }
        }

        private void Paint_HIL_FrontEnd(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);

            int name_dx = 0, name_dy = 0;
            int U_dx = 0, U_dy = 0;

            // Paint nodes
            for (int i1 = 0; i1 < HIL_FrontEnd_no; i1++)
            {
                object_x0 = -X0_shift + int.Parse(HIL_FrontEnd[i1, HIL_FrontEnd_PROP_X0]);
                object_y0 = -Y0_shift + int.Parse(HIL_FrontEnd[i1, HIL_FrontEnd_PROP_Y0]);

                g.FillRectangle(b14Cyan, object_x0, object_y0, HIL_FrontEnd_dX, HIL_FrontEnd_dY);    // b8LightCoral, b14Aqua, b14Cyan
                g.DrawRectangle(p5DarkBlue2, object_x0, object_y0, HIL_FrontEnd_dX, HIL_FrontEnd_dY);    // b8LightCoral, b14Aqua, b14Cyan
                if (HIL_FrontEnd[i1, HIL_FrontEnd_PROP_gph_selected] == "1") { Font_crt = Font1bold; SolidBrush_crt = b6Red; }
                else { Font_crt = Font1; SolidBrush_crt = b0Black; }
                g.DrawString("HIL01", Font_crt, SolidBrush_crt, object_x0 + 1, object_y0 + 0);
                g.DrawString(HIL_FrontEnd[i1, HIL_FrontEnd_PROP_Name], Font1, b0Black, object_x0 + 1, object_y0 + 10);
            }
        }


        int wnd_pos_x_crt = 0, wnd_pos_y_crt = 0;
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //this..MaximizeBox;

            DateTime t1 = DateTime.Now;
            //string s1 = "";
            string st1 = ">> Grid compute\n";
            int t1s = t1.Second, t1ms = t1.Millisecond;

            if (checkBox_expand.Checked == true) expand_obj_gph = 1; else expand_obj_gph = 0;

            Graphics g = e.Graphics;
            obj_number = 0;
            string s1 = "";
            //int obj_x = 0, obj_y = 0;

            //int Nr_X_wnd = 5, Nr_Y_wnd = 9;
            int Nr_X_wnd = 3, Nr_Y_wnd = 5, k1=16, k2=10;
            int small_window_y0 = 115;
            Pen b1 = new Pen(Color.Black);
            // Draw the small version of the complete area, having 2x on horisontal and 3x on vertical
            g.DrawRectangle(b1, 6, small_window_y0, 5 + Electrical_scheme_zone_delta_X0/50 * Nr_X_wnd *k1 /k2, 
                                   7 + Electrical_scheme_zone_delta_Y0/50 * Nr_Y_wnd * k1 / k2);
            b1 = new Pen(Color.LightBlue);
            for (int i1=0; i1<= Nr_X_wnd; i1++)
                g.DrawLine(b1, 6 + 2 + Electrical_scheme_zone_delta_X0 / 50 * i1 * k1 / k2, small_window_y0 + 2 , 
                               6 + 2 + Electrical_scheme_zone_delta_X0 / 50 * i1 * k1 / k2,
                               small_window_y0 + 2+ Electrical_scheme_zone_delta_Y0 / 50 * Nr_Y_wnd * k1 / k2);
            for (int i1 = 0; i1 <= Nr_Y_wnd; i1++)
                g.DrawLine(b1, 6 + 2, small_window_y0 + 3 + Electrical_scheme_zone_delta_Y0 / 50 * i1 * k1 / k2,
                               6 + 2 + Electrical_scheme_zone_delta_X0 / 50 * Nr_X_wnd * k1 / k2,
                               small_window_y0 + 3 + Electrical_scheme_zone_delta_Y0 / 50 * i1 * k1 / k2);
            // current rectangle position (in red)
            b1 = new Pen(Color.Red,3);
            g.DrawRectangle(b1, 6 + 2 + Electrical_scheme_zone_delta_X0 / 50 * (wnd_pos_x_crt + Nr_X_wnd/2) * k1 / k2,
                           small_window_y0 + 3 + Electrical_scheme_zone_delta_Y0 / 50 * (wnd_pos_y_crt + Nr_Y_wnd/2) * k1 / k2,
                           Electrical_scheme_zone_delta_X0 / 50 * k1 / k2, Electrical_scheme_zone_delta_Y0 / 50 * k1 / k2);
            s1 = "DX=" + X0_shift.ToString();
            g.DrawString(s1, Font_crt, b0Black, 10, small_window_y0+5);
            s1 = "DY=" + Y0_shift.ToString();
            g.DrawString(s1, Font_crt, b0Black, 10, small_window_y0 + 18);

            b1 = new Pen(Color.LightBlue);
            //g.DrawRectangle(b1, 185, 50, 1348, 610); // Draw the complete area 1534, 649
            g.DrawRectangle(b1, Electrical_scheme_zone_X0_start, Electrical_scheme_zone_Y0_start,
                Electrical_scheme_zone_delta_X0, Electrical_scheme_zone_delta_Y0); // Draw the complete area 1534, 649

            string image_file = "";
            try
            {
                if (Global_Gph_Info[0, Global_Gph_Info_PROP_background_image] != "")
                {
                    image_file = Grid_Projects_Path + @"/" + GridMonk_Project + @"/" + Global_Gph_Info[0, Global_Gph_Info_PROP_background_image];
                    Image backgndImage1 = Image.FromFile(image_file); // 222 x 154, 58 x 40
                    e.Graphics.DrawImage(backgndImage1, Electrical_scheme_zone_X0_start, Electrical_scheme_zone_Y0_start,
                        Electrical_scheme_zone_delta_X0, Electrical_scheme_zone_delta_Y0);
                }
            }
            catch {
                Console.WriteLine("Form1_Paint-backgndImage1-Err1");
            }

            if(_GridMonK_GUI_refresh == "Refresh") {
                Paint_lines(sender, e);
                Paint_trafos(sender, e);
                Paint_loads(sender, e); // loads contain also storage and EV
                Paint_generators(sender, e);
                Paint_labels(sender, e);
                Paint_interracts(sender, e);
                Paint_SimpleGph(sender, e);

                //Paint_polylines(sender, e);
                Nodes_properties_calculation(); Paint_nodes(sender, e);
                Paint_polylines_from_nodes(sender, e);

                Paint_measurements(sender, e);
                Paint_graph_phasors(sender, e);
                Paint_Pie(sender, e);
                Paint_Prosumer(sender, e);
                Paint_Smart_Meter(sender, e);
                Paint_PMU(sender, e);
                //Paint_EVs(sender, e);
                Paint_console_Training(sender, e);
                Paint_graph_sankeys(sender, e);
                Paint_graphs(sender, e);

                // Print S4G_HIL module data
                Paint_S4G_HIL(sender, e);

                // Paint HIL_FrontEnd objects
                Paint_HIL_FrontEnd(sender, e);
            }

            t1 = DateTime.Now;
            int t2s = t1.Second, t2ms = t1.Millisecond; // dupa generate_output_dss()
            int dt2_msec = t2s * 1000 + t2ms - t1s * 1000 - t1ms;
            if (dt2_msec < 0) dt2_msec = dt2_msec + 60000;
            textBox_GUI_dynamics.Text = dt2_msec.ToString() + "ms";
        }

    }
}