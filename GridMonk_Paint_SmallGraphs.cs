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

        static int SimpleGph1_X0_start = 850; //520; 
        static int SimpleGph1_Y0_start = 670;
        static int SimpleGph_dXlegend = 80;
        //static int SimpleGph1_delta_X0 = 600;
        static int SimpleGph1_delta_X0 = SimpleGph_dXlegend + SimpleGph_Y_width_MAX * 2 + 160;
        static int SimpleGph1_delta_Y0 = 120;

        // Clipping the graphics of scheme zone with this rectangle
        Point[] polyPoints_clip_small_gph_zone = {
                new Point(SimpleGph1_X0_start, SimpleGph1_Y0_start-5),
                new Point(SimpleGph1_X0_start+SimpleGph1_delta_X0, SimpleGph1_Y0_start-10),
                new Point(SimpleGph1_X0_start+SimpleGph1_delta_X0, SimpleGph1_Y0_start+SimpleGph1_delta_Y0+5),
                new Point(SimpleGph1_X0_start, SimpleGph1_Y0_start+SimpleGph1_delta_Y0+5)};

        const int SimpleGph_channels_MAX = 6; // max number of graphical channels
        const int SimpleGph_channels_depth_MAX = 1000; // max number of samples recored in each channel
        double[,] SimpleGph_channels1 = new double[SimpleGph_channels_MAX, SimpleGph_channels_depth_MAX];
        double[,] SimpleGph_channels2 = new double[SimpleGph_channels_MAX, SimpleGph_channels_depth_MAX];
        const int SimpleGph_X_width_MAX = 24*4*2; // 192
        const int SimpleGph_Y_width_MAX = 120; // 192
        const int SimpleGph1_X0 = 850; //540;
        int SimpleGph2_X0 = SimpleGph1_X0 + 220; //760;
        int SimpleGph_Y0 = 670;
        Pen[] Channel_Pen = new Pen[SimpleGph_channels_MAX];
        SolidBrush[] Channel_Brush = new SolidBrush[SimpleGph_channels_MAX];
        int step_w = 8;
        int samples = 24;
        int channel_free1 = 0;
        int channel_free2 = 0;
        string[] channel_name = new string[SimpleGph_channels_MAX];

        int SimpleGph1_P_max = 100; // the initial height standard value is set for 100 kW

        int Up_button_dX = 10, Up_button_dY = 80;
        private void Scan_SimpleGph(int xm, int ym)
        {
            // Interracts on the graph:  buttons and cursors
            int x1, y1, inside;

            // Prepare coordinates for command "Up"
            x1 = SimpleGph1_X0_start + Up_button_dX-6;
            y1 = SimpleGph_Y0 + Up_button_dY - 2;
            inside = Inside_rect(x1, y1, x1 + 35, y1 + 14, xm, ym);
            // Execute command "Up"
            if (inside == 1)
            {
                if (SimpleGph1_P_max == 10) SimpleGph1_P_max = 20;
                else if (SimpleGph1_P_max == 20) SimpleGph1_P_max = 50;
                else if (SimpleGph1_P_max == 50) SimpleGph1_P_max = 100;
                else if (SimpleGph1_P_max == 100) SimpleGph1_P_max = 200;
                else if (SimpleGph1_P_max == 200) SimpleGph1_P_max = 500;
                else if (SimpleGph1_P_max == 500) SimpleGph1_P_max = 1000;
                else if (SimpleGph1_P_max == 1000) SimpleGph1_P_max = 2000;
                else if (SimpleGph1_P_max == 2000) SimpleGph1_P_max = 5000;
                else if (SimpleGph1_P_max == 5000) SimpleGph1_P_max = 10000;
                else if (SimpleGph1_P_max == 10000) SimpleGph1_P_max = 20000; // 20 MW
            }
            // Prepare coordinates for command "Down"
            x1 = SimpleGph1_X0_start + Up_button_dX - 6;
            y1 = SimpleGph_Y0 + Up_button_dY +18;
            inside = Inside_rect(x1, y1, x1 + 35, y1 + 15, xm, ym);
            // execute command "Down", with 1, 2, 5 and 10 multipliers, from 10 kW to 20 MW 
            if (inside == 1)
            {
                if (SimpleGph1_P_max == 20) SimpleGph1_P_max = 10;
                else if (SimpleGph1_P_max == 50) SimpleGph1_P_max = 20;
                else if (SimpleGph1_P_max == 100) SimpleGph1_P_max = 50;
                else if (SimpleGph1_P_max == 200) SimpleGph1_P_max = 100;
                else if (SimpleGph1_P_max == 500) SimpleGph1_P_max = 200;
                else if (SimpleGph1_P_max == 1000) SimpleGph1_P_max = 500;
                else if (SimpleGph1_P_max == 2000) SimpleGph1_P_max = 1000;
                else if (SimpleGph1_P_max == 2000) SimpleGph1_P_max = 1000;
                else if (SimpleGph1_P_max == 5000) SimpleGph1_P_max = 2000;
                else if (SimpleGph1_P_max == 10000) SimpleGph1_P_max = 5000;
                else if (SimpleGph1_P_max == 20000) SimpleGph1_P_max = 10000;
            }

        }

        private void Paint_SimpleGph(object sender, PaintEventArgs e)
        {
            int y1, y2;
            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip1 = new GraphicsPath();
            path_clip1.AddPolygon(polyPoints_clip_small_gph_zone);
            Region region1 = new Region(path_clip1);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region1, CombineMode.Replace);

            Channel_Pen[0] = new Pen(Color.Brown); Channel_Pen[1] = new Pen(Color.Red);
            Channel_Pen[2] = new Pen(Color.Green); Channel_Pen[3] = new Pen(Color.Blue);
            Channel_Pen[4] = new Pen(Color.Magenta); Channel_Pen[5] = new Pen(Color.Black);

            Channel_Brush[0] = new SolidBrush(Color.Brown); Channel_Brush[1] = new SolidBrush(Color.Red);
            Channel_Brush[2] = new SolidBrush(Color.Green); Channel_Brush[3] = new SolidBrush(Color.Blue);
            Channel_Brush[4] = new SolidBrush(Color.Magenta); Channel_Brush[5] = new SolidBrush(Color.Black);

            Pen b1 = new Pen(Color.LightBlue);
            Pen b2 = new Pen(Color.LightGray);
            Pen b3 = new Pen(Color.DarkGray);
            Pen b4 = new Pen(Color.DarkBlue);
            Pen bx = b1;
            double val1;

            // Draw Up-Down buttons
            g.FillRectangle(b13Yellow, SimpleGph1_X0 + Up_button_dX-6, SimpleGph_Y0 + Up_button_dY-2, 35, 14);
            g.DrawString(" Up ", Font1bold, b0Black, SimpleGph1_X0 + Up_button_dX-4, SimpleGph_Y0 + Up_button_dY);
            g.FillRectangle(b10LightSalmon, SimpleGph1_X0 + Up_button_dX-6, SimpleGph_Y0 + Up_button_dY +18, 35, 15);
            g.DrawString("Down", Font1bold, b0Black, SimpleGph1_X0 + Up_button_dX-5, SimpleGph_Y0 + Up_button_dY + 20);

            // Draw graphics area
            g.DrawRectangle(b1, SimpleGph1_X0, SimpleGph_Y0-7,
                SimpleGph_dXlegend+SimpleGph_Y_width_MAX * 2 +160, SimpleGph_Y_width_MAX+14); // Draw the complete area 
            g.DrawRectangle(b1, SimpleGph1_X0+510, SimpleGph_Y0 - 7,
                SimpleGph_dXlegend + SimpleGph_Y_width_MAX * 2 + 160, SimpleGph_Y_width_MAX + 14); // Draw the complete area 
            // Graphics set 1 background
            g.FillRectangle(b1White, SimpleGph_dXlegend+SimpleGph1_X0, SimpleGph_Y0 - 5, SimpleGph_X_width_MAX, SimpleGph_Y_width_MAX+10);
            // Graphics set 2 background
            g.FillRectangle(b1White, SimpleGph_dXlegend+SimpleGph2_X0, SimpleGph_Y0 - 5, SimpleGph_X_width_MAX, SimpleGph_Y_width_MAX + 10);
            
            // drawing horisontal lines on the graphs
            for (int i1 = 0; i1 < 14; i1++)
            {
                if (i1 == 6) bx = b4; else bx = b2;

                // draw values on the vertical axis of powers
                val1 = SimpleGph1_P_max - (i1-1) * SimpleGph1_P_max/5;
                //if ((i1 % 2) == 1)
                    g.DrawString(val1.ToString(), Font0, b0Black, SimpleGph_dXlegend + SimpleGph1_X0 - 22, 
                        SimpleGph_Y0 + SimpleGph_Y_width_MAX / 2 + (i1 - 6) * 10 - 5);
                
                // draw values on the vertical axis of voltages
                val1 = 290 - i1 * 10;
                if((i1 % 2)==1) g.DrawString(val1.ToString(), Font0, b0Black, SimpleGph_dXlegend + SimpleGph2_X0-22, 
                    SimpleGph_Y0 + SimpleGph_Y_width_MAX / 2 + (i1 - 6) * 10 - 5);
                
                // graph of powers P
                g.DrawLine(bx, SimpleGph_dXlegend + SimpleGph1_X0, SimpleGph_Y0 + SimpleGph_Y_width_MAX / 2 + (i1-6)*10,
                     SimpleGph_dXlegend + SimpleGph1_X0 + SimpleGph_X_width_MAX, SimpleGph_Y0 + SimpleGph_Y_width_MAX / 2 + (i1 - 6) * 10);
                
                // graph of voltage level
                g.DrawLine(bx, SimpleGph_dXlegend + SimpleGph2_X0, SimpleGph_Y0 + SimpleGph_Y_width_MAX / 2 + (i1 - 6) * 10,
                     SimpleGph_dXlegend + SimpleGph2_X0 + SimpleGph_X_width_MAX, SimpleGph_Y0 + SimpleGph_Y_width_MAX / 2 + (i1 - 6) * 10);
            }
            // draw vertical lines
            for (int i1 = 0; i1 < 7; i1++)
            {
                g.DrawLine(bx, SimpleGph_dXlegend + SimpleGph1_X0 + SimpleGph_X_width_MAX / 6 * i1 - step_w / 2, SimpleGph_Y0,
                     SimpleGph_dXlegend + SimpleGph1_X0 + SimpleGph_X_width_MAX / 6 * i1 - step_w / 2, SimpleGph_Y0 + SimpleGph_Y_width_MAX);
                g.DrawLine(bx, SimpleGph_dXlegend + SimpleGph2_X0 + SimpleGph_X_width_MAX/6*i1- step_w/2, SimpleGph_Y0,
                     SimpleGph_dXlegend + SimpleGph2_X0 + SimpleGph_X_width_MAX/6*i1 - step_w/2, SimpleGph_Y0 + SimpleGph_Y_width_MAX);
            }
            // display channels (the legend)
            for (int i1 = 0; i1 < 6; i1++)
            {
                g.DrawString(i1.ToString() +": "+ channel_name[i1], Font0, Channel_Brush[i1], SimpleGph1_X0 + 1, SimpleGph_Y0 + 12 * i1);
            }

            double power_factor = 1;
            if (graph_smallgph[0, graph_smallgph_PROP_P_fact] != "")
                power_factor = double.Parse(graph_smallgph[0, graph_smallgph_PROP_P_fact]);

            double voltage_factor = 1;
            if(graph_smallgph[0, graph_smallgph_PROP_U_fact] !="")
                voltage_factor = double.Parse(graph_smallgph[0, graph_smallgph_PROP_U_fact]);
            //double voltage_factor = voltage_level / 230;
            // draw graphics
            for (int ch=0; ch< SimpleGph_channels_MAX; ch++)
            for (int x1=0; x1< samples; x1++)
            {
                    // Graphics set 1
                y1 = -(int)Math.Round(SimpleGph_channels1[ch, x1]/ power_factor / SimpleGph1_P_max * 50); // we scale that P_max has only 50 pixels in the graph
                y2 = -(int)Math.Round(SimpleGph_channels1[ch, x1+1]/ power_factor / SimpleGph1_P_max * 50);
                if (x1 != 0) g.DrawLine(Channel_Pen[ch], SimpleGph_dXlegend+SimpleGph1_X0 + (x1-1)* step_w + step_w, SimpleGph_Y0 + SimpleGph_Y_width_MAX/2 + y1,
                    SimpleGph_dXlegend+SimpleGph1_X0 + (x1-1) * step_w + step_w, SimpleGph_Y0 + SimpleGph_Y_width_MAX / 2 + y2);
                g.DrawLine(Channel_Pen[ch], SimpleGph_dXlegend+SimpleGph1_X0 + (x1 - 1) * step_w + step_w, SimpleGph_Y0 + SimpleGph_Y_width_MAX / 2 + y2,
                    SimpleGph_dXlegend+SimpleGph1_X0 + x1 * step_w + step_w, SimpleGph_Y0 + SimpleGph_Y_width_MAX / 2 + y2);

                    // Graphics set 2
                y1 = -(int)Math.Round(SimpleGph_channels2[ch, x1]/ voltage_factor - 230);
                y2 = -(int)Math.Round(SimpleGph_channels2[ch, x1 + 1] / voltage_factor - 230);
                if(x1!=0) g.DrawLine(Channel_Pen[ch], SimpleGph_dXlegend+SimpleGph2_X0 + (x1 - 1) * step_w+ step_w, 
                                                      SimpleGph_Y0 + SimpleGph_Y_width_MAX / 2 + y1,
                                                      SimpleGph_dXlegend+SimpleGph2_X0 + (x1 - 1) * step_w+ step_w, 
                                                      SimpleGph_Y0 + SimpleGph_Y_width_MAX / 2 + y2);
                g.DrawLine(Channel_Pen[ch], SimpleGph_dXlegend+SimpleGph2_X0 + (x1 - 1) * step_w+ step_w, 
                                            SimpleGph_Y0 + SimpleGph_Y_width_MAX / 2 + y2,
                                            SimpleGph_dXlegend+SimpleGph2_X0 + x1 * step_w+ step_w, 
                                            SimpleGph_Y0 + SimpleGph_Y_width_MAX / 2 + y2);
            }
        }

    }
}