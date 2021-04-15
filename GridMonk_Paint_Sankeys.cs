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

        const int gph_sankeys_number_MAX = 36;
        int gph_sankeys_legend_dx = 70;

        public struct Sankey
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

        Sankey[] gph_Sankeys_properties = new Sankey[graph_sankeys_MAX];

        const int Sank_max_members = 5;
        string[,] Sank1 = new string[Sank_max_members, Sank_max_members+2];

        private void Gph_sankeys_ini() {
             // TBD
        }

        private void Paint_sankeys(object sender, PaintEventArgs e, int gph_object_no)
        {
            //string s1 = "";
            for (int i1 = 0; i1 < Sank_max_members; i1++)
                for (int j1 = 0; j1 < Sank_max_members + 2; j1++)
                    Sank1[i1, j1] = "";

            // Initialising matrix 1
            Sank1[0, 0] = "100";
            Sank1[1, 0] = "20"; Sank1[1, 1] = "80";
            Sank1[2, 0] = "10"; Sank1[2, 1] = "10"; Sank1[2, 2] = "50"; Sank1[2, 3] = "30";
            Sank1[3, 0] = "10"; Sank1[3, 1] = "10"; Sank1[3, 2] = "50"; Sank1[3, 3] = "10"; Sank1[3, 4] = "20";

            Graphics g = e.Graphics;

            // Clipping the plygones start lines
            GraphicsPath path_clip = new GraphicsPath();
            path_clip.AddPolygon(polyPoints_clip_scheme_zone);
            Region region = new Region(path_clip);            // Set the clipping region of the Graphics object.
            e.Graphics.SetClip(region, CombineMode.Replace);

            int sankey_dx = 150, sankey_dy=150;
            int lev_width = 30, lev_step = lev_width + 10;

            //int max_y=100;

            g.FillRectangle(b1White, object_x0, object_y0, sankey_dx, sankey_dy);
            //g.DrawRectangle(p1Black, object_x0, object_y0, sankey_dx, sankey_dy);

            int y_depth = 0, d1 = 0;
            int space_y = 5;
            //int space_x = 30;
            // draw matrix 1
            for (int lev1 = 0; lev1 < Sank_max_members; lev1++)
            {
                y_depth = 0;
                for (int i1 = 0; i1 < Sank_max_members; i1++)
                {
                    if (Sank1[lev1, i1] != "")
                    {
                        d1 = int.Parse(Sank1[lev1, i1]);
                        if (i1 == 0) SolidBrush_crt = b5DarkBlue;
                        if (i1 == 1) SolidBrush_crt = b6Red;
                        if (i1 == 2) SolidBrush_crt = b7Green;
                        if (i1 == 3) SolidBrush_crt = b4LightBlue;
                        if (i1 == 4) SolidBrush_crt = b10LightSalmon;
                        g.FillRectangle(SolidBrush_crt, object_x0 + lev_step * lev1, object_y0 + y_depth + space_y * i1, lev_width, d1);
                        y_depth += d1;
                    }
                }
            }
        }
    }

}