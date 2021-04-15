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

        int Gph1_X0_start = 850; //520; 
        int Gph1_Y0_start = 670;
        int Gph_dXlegend = 80;
        //static int Gph_Y_width_MAX = 300;
        //static int SimpleGph1_delta_X0 = 600;
        static int Gph1_delta_X0 = SimpleGph_dXlegend + Gph_Y_width_MAX * 2 + 160;
        static int Gph1_delta_Y0 = 120;

        const int Gph_channels_MAX = 6; // max number of graphical channels
        const int Gph_channels_depth_MAX = 1000; // max number of samples recored in each channel
        double[,] Gph_channels1 = new double[SimpleGph_channels_MAX, SimpleGph_channels_depth_MAX];
        double[,] Gph_channels2 = new double[SimpleGph_channels_MAX, SimpleGph_channels_depth_MAX];
        const int Gph_X_width_MAX = 24*4*2; // 192
        const int Gph_Y_width_MAX = 120; // 192
        int Gph1_X0 = 400; int Gph_Y0 = 200;

        Pen[] Gph1_Channel_Pen = new Pen[SimpleGph_channels_MAX];
        SolidBrush[] Gph1_Channel_Brush = new SolidBrush[SimpleGph_channels_MAX];
        int Gph1_step_w = 8;
        int Gph1_samples = 24;
        int Gph1_channel_free1 = 0;
        int Gph1_channel_free2 = 0;
        string[] Gph1_channel_name = new string[SimpleGph_channels_MAX];

        int Gph1_P_max = 100; // the initial height standard value is set for 100 kW

        int Gph1_Up_button_dX = 10, Gph1_Up_button_dY = 80;
        private void Scan_Gph1(int xm, int ym)
        {
            // Interracts on the graph:  buttons and cursors
            int x1, y1, inside;

            // Prepare coordinates for command "Up"
            x1 = Gph1_X0_start + Up_button_dX-6;
            y1 = Gph_Y0 + Up_button_dY - 2;
            inside = Inside_rect(x1, y1, x1 + 35, y1 + 14, xm, ym);
            // Execute command "Up"
            if (inside == 1)
            {
                if (Gph1_P_max == 10) Gph1_P_max = 20;
                else if (Gph1_P_max == 20) Gph1_P_max = 50;
                else if (Gph1_P_max == 50) Gph1_P_max = 100;
                else if (Gph1_P_max == 100) Gph1_P_max = 200;
                else if (Gph1_P_max == 200) Gph1_P_max = 500;
                else if (Gph1_P_max == 500) Gph1_P_max = 1000;
                else if (Gph1_P_max == 1000) Gph1_P_max = 2000;
                else if (Gph1_P_max == 2000) Gph1_P_max = 5000;
                else if (Gph1_P_max == 5000) Gph1_P_max = 10000;
                else if (Gph1_P_max == 10000) Gph1_P_max = 20000; // 20 MW
            }
            // Prepare coordinates for command "Down"
            x1 = Gph1_X0_start + Up_button_dX - 6;
            y1 = Gph_Y0 + Up_button_dY +18;
            inside = Inside_rect(x1, y1, x1 + 35, y1 + 15, xm, ym);
            // execute command "Down", with 1, 2, 5 and 10 multipliers, from 10 kW to 20 MW 
            if (inside == 1)
            {
                if (Gph1_P_max == 20) Gph1_P_max = 10;
                else if (Gph1_P_max == 50) Gph1_P_max = 20;
                else if (Gph1_P_max == 100) Gph1_P_max = 50;
                else if (Gph1_P_max == 200) Gph1_P_max = 100;
                else if (Gph1_P_max == 500) Gph1_P_max = 200;
                else if (Gph1_P_max == 1000) Gph1_P_max = 500;
                else if (Gph1_P_max == 2000) Gph1_P_max = 1000;
                else if (Gph1_P_max == 2000) Gph1_P_max = 1000;
                else if (Gph1_P_max == 5000) Gph1_P_max = 2000;
                else if (Gph1_P_max == 10000) Gph1_P_max = 5000;
                else if (Gph1_P_max == 20000) Gph1_P_max = 10000;
            }

        }

        public void Paint_Gph1(object sender, PaintEventArgs e, int gph_no)
        {
            //int y1, y2;
            Graphics g = e.Graphics;

            Gph1_Channel_Pen[0] = new Pen(Color.Brown); Gph1_Channel_Pen[1] = new Pen(Color.Red);
            Gph1_Channel_Pen[2] = new Pen(Color.Green); Gph1_Channel_Pen[3] = new Pen(Color.Blue);
            Gph1_Channel_Pen[4] = new Pen(Color.Magenta); Gph1_Channel_Pen[5] = new Pen(Color.Black);

            Gph1_Channel_Brush[0] = new SolidBrush(Color.Brown); Gph1_Channel_Brush[1] = new SolidBrush(Color.Red);
            Gph1_Channel_Brush[2] = new SolidBrush(Color.Green); Gph1_Channel_Brush[3] = new SolidBrush(Color.Blue);
            Gph1_Channel_Brush[4] = new SolidBrush(Color.Magenta); Gph1_Channel_Brush[5] = new SolidBrush(Color.Black);

            Pen b1 = new Pen(Color.LightBlue);
            Pen b1x3 = new Pen(Color.LightBlue, 3);
            Pen b2 = new Pen(Color.LightGray);
            Pen b3 = new Pen(Color.DarkGray);
            Pen b4 = new Pen(Color.DarkBlue);
            Pen bx = b1;
            double val1;

            Gph1_X0 = object_x0;
            Gph_Y0 = object_y0;
            if (graphs[gph_no, graphs_PROP_dX_legend] == "") Gph_dXlegend = 0;
                else Gph_dXlegend = int.Parse(graphs[gph_no, graphs_PROP_dX_legend]);
            int Gph1_dx = Gph_dXlegend + int.Parse(graphs[gph_no, graphs_PROP_Samples_max]) * int.Parse(graphs[gph_no, graphs_PROP_Samples_X_width]);
            int Gph1_dy;
            if (graphs[gph_no, graphs_PROP_graph_dY] == "") Gph1_dy = 100;
                else Gph1_dy = int.Parse(graphs[gph_no, graphs_PROP_graph_dY]);
            /*
            // Draw Up-Down buttons
            g.FillRectangle(b13Yellow, Gph1_X0 + Gph1_Up_button_dX - 6, Gph_Y0 + Gph1_Up_button_dY - 2, 35, 14);
            g.DrawString(" Up ", Font1bold, b0Black, Gph1_X0 + Gph1_Up_button_dX - 4, Gph_Y0 + Gph1_Up_button_dY);
            g.FillRectangle(b10LightSalmon, Gph1_X0 + Gph1_Up_button_dX - 6, Gph_Y0 + Gph1_Up_button_dY + 18, 35, 15);
            g.DrawString("Down", Font1bold, b0Black, Gph1_X0 + Gph1_Up_button_dX - 5, Gph_Y0 + Gph1_Up_button_dY + 20);
            */

            // classic drawing
            { 
                // Draw graphics area
                g.DrawRectangle(b1, Gph1_X0, Gph_Y0, Gph1_dx, Gph1_dy); // Draw the complete area 
                // Graphics set 1 background
                g.FillRectangle(b1White, Gph1_X0+1, Gph_Y0+1, Gph1_dx-2, Gph1_dy-2);
            }
            // additional shadow drawing if we are in "shadow mode", which appear when dragging the object on the screen
            if (graphs[gph_no, graphs_PROP_graph_shaddow] == "1")
            {
                g.DrawRectangle(b1x3, Gph1_X0 + dx_mousePressed_x, Gph_Y0 + dy_mousePressed_y, Gph1_dx, Gph1_dy); // Draw the complete area 
            }

            // Draw labels, legend
            g.DrawString("Title: " + graphs[gph_no, graphs_PROP_gph_title], Font0, b0Black, Gph1_X0 + 3, Gph_Y0 + 3);

            /*
            // drawing horisontal lines on the graphs
            for (int i1 = 0; i1 < 14; i1++)
            {
                if (i1 == 6) bx = b4; else bx = b2;

                // draw values on the vertical axis of powers
                val1 = Gph1_P_max - (i1-1) * Gph1_P_max/5;
                //if ((i1 % 2) == 1)
                    g.DrawString(val1.ToString(), Font0, b0Black, Gph_dXlegend + Gph1_X0 - 22, 
                        Gph_Y0 + Gph_Y_width_MAX / 2 + (i1 - 6) * 10 - 5);
                
                // graph of powers P
                g.DrawLine(bx, Gph_dXlegend + Gph1_X0, Gph_Y0 + Gph_Y_width_MAX / 2 + (i1-6)*10,
                     Gph_dXlegend + Gph1_X0 + Gph_X_width_MAX, Gph_Y0 + Gph_Y_width_MAX / 2 + (i1 - 6) * 10);
                
            }
            // draw vertical lines
            for (int i1 = 0; i1 < 7; i1++)
            {
                g.DrawLine(bx, Gph_dXlegend + Gph1_X0 + Gph_X_width_MAX / 6 * i1 - step_w / 2, Gph_Y0,
                     Gph_dXlegend + Gph1_X0 + Gph_X_width_MAX / 6 * i1 - step_w / 2, Gph_Y0 + Gph_Y_width_MAX);
            }
            // display channels (the legend)
            for (int i1 = 0; i1 < 6; i1++)
            {
                g.DrawString(i1.ToString() +": "+ Gph1_channel_name[i1], Font0, Gph1_Channel_Brush[i1], Gph1_X0 + 1, Gph_Y0 + 12 * i1);
            }

            double power_factor = 1;
            if (graph_smallgph[0, graph_smallgph_PROP_P_fact] != "")
                power_factor = double.Parse(graph_smallgph[0, graph_smallgph_PROP_P_fact]);

            // draw graphics
            for (int ch=0; ch< Gph_channels_MAX; ch++)
            for (int x1=0; x1< samples; x1++)
            {
                    // Graphics set 1
                y1 = -(int)Math.Round(SimpleGph_channels1[ch, x1]/ power_factor / SimpleGph1_P_max * 50); // we scale that P_max has only 50 pixels in the graph
                y2 = -(int)Math.Round(SimpleGph_channels1[ch, x1+1]/ power_factor / SimpleGph1_P_max * 50);
                if (x1 != 0) g.DrawLine(Channel_Pen[ch], SimpleGph_dXlegend+SimpleGph1_X0 + (x1-1)* step_w + step_w, SimpleGph_Y0 + SimpleGph_Y_width_MAX/2 + y1,
                    SimpleGph_dXlegend+SimpleGph1_X0 + (x1-1) * step_w + step_w, SimpleGph_Y0 + SimpleGph_Y_width_MAX / 2 + y2);
                g.DrawLine(Channel_Pen[ch], SimpleGph_dXlegend+SimpleGph1_X0 + (x1 - 1) * step_w + step_w, SimpleGph_Y0 + SimpleGph_Y_width_MAX / 2 + y2,
                    SimpleGph_dXlegend+SimpleGph1_X0 + x1 * step_w + step_w, SimpleGph_Y0 + SimpleGph_Y_width_MAX / 2 + y2);
            }
            */
        }

    }
}