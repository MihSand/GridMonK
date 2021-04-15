using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GridMonC
{
    class LinesObj
    {
        // Grid MonC = Monitoring and Control
        // Grid MonK = Monitoring and control Knowledge

        public int x0, y0;

        public LinesObj(string[] properties)
        {
            x0 = y0 = 0;
        }
        
        /*void print(object sender, PaintEventArgs e, Graphics g)
        {

            g.FillRectangle(b1White, object_x0, object_y0, object_dx, line_dy);

            string gph_direction = lines[i1, lines_PROP_gph_direction];
            // draw the line symbol
            Pen p1Black4arrow = new Pen(Color.Black, 3);
            p1Black4arrow.EndCap = LineCap.ArrowAnchor;
            if (gph_direction == "N")
            {
                g.DrawString("1", Font1, b0Black, object_x0 + 22, object_y0 + 1);
                //g.FillRectangle(b2Blue, object_x0 + 35, object_y0 + 4, 20, 3);
                //Point[] points = new Point[3];
                //g.FillPolygon((b2Blue, points);
                g.DrawString("2", Font0, b0Black, object_x0 + 62, object_y0 + 1);
                g.DrawRectangle(p5DarkBlue, object_x0 + 22, object_y0 + 0, object_dx - 46, 11); // Desenare simbol linie
                g.DrawLine(p5DarkBlue, object_x0, object_y0 + 6, object_x0 + 22, object_y0 + 6);
                g.DrawLine(p5DarkBlue, object_x0 + object_dx - 22, object_y0 + 6, object_x0 + object_dx, object_y0 + 6);
                g.DrawEllipse(p5DarkBlue, object_x0, object_y0 + 4, 4, 4); // Pin_Bus1
                g.FillRectangle(b2Blue, object_x0 + 10, object_y0 + 2, 8, 8); // Brk1
                g.DrawEllipse(p5DarkBlue, object_x0 + object_dx - 4, object_y0 + 4, 4, 4); // Pin_Bus2
                g.FillRectangle(b2Blue, object_x0 + object_dx - 18, object_y0 + 2, 8, 8); // Brk2
                                                                                          //g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent, object_y0 + 22);
            }
            if (gph_direction == "W")
            {
                g.DrawRectangle(p5DarkBlue, object_x0 + 0, object_y0 + 22, 11, line_dy - 46); // Desenare simbol linie
                g.DrawLine(p5DarkBlue, object_x0 + 6, object_y0, object_x0 + 6, object_y0 + 22);
                g.DrawLine(p5DarkBlue, object_x0 + 6, object_y0 + line_dy - 22, object_x0 + 6, object_y0 + line_dy);
                g.DrawEllipse(p5DarkBlue, object_x0 + 4, object_y0, 4, 4); // Pin_Bus1
                g.FillRectangle(b2Blue, object_x0 + 2, object_y0 + 10, 8, 8); // Brk1
                g.DrawEllipse(p5DarkBlue, object_x0 + 4, object_y0 + line_dy - 4, 4, 4); // Pin_Bus2
                g.FillRectangle(b2Blue, object_x0 + 2, object_y0 + line_dy - 18, 8, 8); // Brk2
            }
            // display parameters
            int indent = 0;
            if (gph_direction == "N") indent = 0; if (gph_direction == "W") indent = 11;

            s1 = "Ln=" + lines[i1, lines_PROP_name];
            if (lines[i1, lines_PROP_gph_selected] == "1") g.DrawString(s1, Font1bold, b6Red, object_x0 + 2 + indent, object_y0 + 11);
            else g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent, object_y0 + 11);

            //s1 = "Ln=" + lines[i1, lines_PROP_name]; g.DrawString(s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 12);
            s1 = "B1=" + lines[i1, lines_PROP_bus1]; g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent, object_y0 + 22);
            s1 = "B2=" + lines[i1, lines_PROP_bus2]; g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent, object_y0 + 32);
            s1 = "Lng=" + lines[i1, lines_PROP_length] + " " + lines[i1, lines_PROP_units];
            g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent, object_y0 + 42);
            s1 = "Cod=" + lines[i1, lines_PROP_linecode]; g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent, object_y0 + 52);
            s1 = "Phases=" + lines[i1, lines_PROP_phases]; g.DrawString(s1, Font1, b0Black, object_x0 + 2 + indent, object_y0 + 62);
            double P3f = 0;
            if ((lines[i1, lines_PROP_P1] != "") && (lines[i1, lines_PROP_P2] != "") && (lines[i1, lines_PROP_P3] != ""))
            {
                P3f = double.Parse(lines[i1, lines_PROP_P1]) + double.Parse(lines[i1, lines_PROP_P2]) + +double.Parse(lines[i1, lines_PROP_P3]);
                lines[i1, lines_PROP_P] = P3f.ToString("#####0.0");
            }
            s1 = "P=" + lines[i1, lines_PROP_P];
            if (lines[i1, lines_PROP_P] != "") g.DrawString(s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 72);
            if ((P3f > 0) && (lines[i1, lines_PROP_P] != "") && (gph_direction == "N"))
                g.DrawLine(p1Black4arrow, object_x0 + 35, object_y0 + 5, object_x0 + 55, object_y0 + 5);
            if ((P3f < 0) && (lines[i1, lines_PROP_P] != "") && (gph_direction == "N"))
                g.DrawLine(p1Black4arrow, object_x0 + 55, object_y0 + 5, object_x0 + 35, object_y0 + 5);

            double Q3f = 0;
            if ((lines[i1, lines_PROP_Q1] != "") && (lines[i1, lines_PROP_Q2] != "") && (lines[i1, lines_PROP_Q3] != ""))
            {
                Q3f = double.Parse(lines[i1, lines_PROP_Q1]) + double.Parse(lines[i1, lines_PROP_Q2]) + double.Parse(lines[i1, lines_PROP_Q3]);
                lines[i1, lines_PROP_Q] = Q3f.ToString("#####0.0");
            }
            s1 = "Q=" + lines[i1, lines_PROP_Q];
            if (lines[i1, lines_PROP_Q] != "") g.DrawString(s1, Font1, b2Blue, object_x0 + 2 + indent, object_y0 + 82);

            if (default_xy == 1) obj_number++;
        }*/

        Pen p1Black = new Pen(Color.Black);
        Pen p1Black3 = new Pen(Color.Black, 3);
        Pen p2LightGray = new Pen(Color.LightGray);
        Pen p3DarkGray = new Pen(Color.DarkGray);
        Pen p4LightBlue2 = new Pen(Color.LightBlue, 2);
        Pen p5DarkBlue = new Pen(Color.DarkBlue);
        Pen p5DarkBlue2 = new Pen(Color.DarkBlue, 2);
        Pen p6Red2 = new Pen(Color.Red, 2);
        Pen p6Red4 = new Pen(Color.Red, 4);
        Pen p7Green = new Pen(Color.Green);
        Pen p7LightGreen = new Pen(Color.LightGreen);
        Pen p8Magenta = new Pen(Color.Magenta);
        Pen p9Lime = new Pen(Color.Lime);
        Pen p10Maroon = new Pen(Color.Maroon, 2);
        Pen p11Orange = new Pen(Color.Orange, 2);
        Font Font0 = new System.Drawing.Font("Arial", 7);
        Font Font0bold = new System.Drawing.Font("Arial", 7, FontStyle.Bold);
        Font Font1 = new System.Drawing.Font("Arial", 8);
        Font Font1bold = new System.Drawing.Font("Arial", 8, FontStyle.Bold);
        Font Font2 = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
        SolidBrush b0Black = new SolidBrush(Color.Black);
        SolidBrush b1White = new SolidBrush(Color.White);
        SolidBrush b2Blue = new SolidBrush(Color.Blue);
        SolidBrush b3DarkGray = new SolidBrush(Color.DarkGray);
        SolidBrush b3LightGray = new SolidBrush(Color.LightGray);
        SolidBrush b4LightBlue = new SolidBrush(Color.LightBlue);
        SolidBrush b5DarkBlue = new SolidBrush(Color.DarkBlue);
        SolidBrush b6Red = new SolidBrush(Color.Red);
        SolidBrush b7Green = new SolidBrush(Color.Green);
        SolidBrush b7LightGreen = new SolidBrush(Color.LightGreen);
        SolidBrush b8Magenta = new SolidBrush(Color.Magenta);
        SolidBrush b9Lime = new SolidBrush(Color.Lime);
        SolidBrush b10Maroon = new SolidBrush(Color.Maroon);
        SolidBrush b11Orange = new SolidBrush(Color.Orange);
        //SolidBrush b12;
        SolidBrush b13Yellow = new SolidBrush(Color.Yellow);
        SolidBrush b13LightYellow = new SolidBrush(Color.LightYellow);
        /*
        const int lines_MAX = 100; const int lines_prop_MAX = 40;
        string[,] lines = new string[lines_MAX, lines_prop_MAX]; // 100 x lines (connecting two nodes), 40 x properties
        const int lines_PROP_name = 0;
        const int lines_PROP_bus1 = 1;
        const int lines_PROP_bus2 = 2;
        const int lines_PROP_phases = 3;
        const int lines_PROP_length = 14;
        const int lines_PROP_units = 15;
        const int lines_PROP_linecode = 16;
        const int lines_PROP_draw_xy = 17;
        const int lines_PROP_x0 = 17;
        const int lines_PROP_y0 = 18;
        const int lines_PROP_plyline_name = 19;
        const int lines_PROP_plyline_xys = 20;
        const int lines_PROP_P1 = 21;
        const int lines_PROP_P2 = 22;
        const int lines_PROP_P3 = 23;
        const int lines_PROP_P = 24;
        const int lines_PROP_Q1 = 25;
        const int lines_PROP_Q2 = 26;
        const int lines_PROP_Q3 = 27;
        const int lines_PROP_Q = 28;
        const int lines_PROP_S = 29;
        const int lines_PROP_I1 = 30;
        const int lines_PROP_I2 = 31;
        const int lines_PROP_I3 = 32;
        const int lines_PROP_gph_selected = 33;
        const int lines_PROP_gph_direction = 39; // N,W,S,E
        int lines_no = 0;
         */
    }
}
