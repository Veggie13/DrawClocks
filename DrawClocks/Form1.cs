using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace DrawClocks
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            panel1.Paint += new PaintEventHandler(Form1_Paint);
            this.Load += new EventHandler(Form1_Load);
            panel1.MouseClick += new MouseEventHandler(panel1_MouseClick);
            vScrollBar1.ValueChanged += new EventHandler(vScrollBar1_ValueChanged);
            panel1.Resize += new EventHandler(panel1_Resize);
            this.KeyPress += new KeyPressEventHandler(Form1_KeyPress);
        }

        void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'O')
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.DefaultExt = ".txt";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    loadFromFile(dlg.FileName);
                }
            }
        }

        void panel1_Resize(object sender, EventArgs e)
        {
            bm = new Bitmap(panel1.Width, panel1.Height);
            panel1.Invalidate();
        }

        void loadFromFile(string filename)
        {
            vScrollBar1.Value = 1;
            vScrollBar1.Value = 0;

            StreamReader reader = new StreamReader(filename);

            int row = 0;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] times = line.Split(' ', '\t');
                for (int col = 0; col < times.Length; col++)
                {
                    _origTimes[row, col] = new TheTime(times[col]);
                    _times[row, col] = new TheTime(_origTimes[row, col]);
                }
                row++;
            }
            reader.Close();
        }

        void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            int row = e.Y / 51;
            int col = e.X / 51;
            if (row >= 15 || col >= 15)
                return;
            Form2 dlg = new Form2();
            dlg.Hour = _times[row, col].hour;
            dlg.Minute = _times[row, col].minute;
            if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                _times[row, col].hour = dlg.Hour;
                _times[row, col].minute = dlg.Minute;
                _origTimes[row, col] = new TheTime(_times[row, col]);
                int hour = vScrollBar1.Value / 60;
                int min = vScrollBar1.Value % 60;
                _origTimes[row, col].Sub(hour, min);
                panel1.Invalidate();
            }
        }

        class TheTime
        {
            public int hour;
            public int minute;
            public TheTime(int h, int m)
            {
                hour = h;
                minute = m;
            }

            public TheTime(TheTime other)
            {
                hour = other.hour;
                minute = other.minute;
            }

            public TheTime(string time)
            {
                string[] parts = time.Split(':');
                hour = int.Parse(parts[0]);
                minute = int.Parse(parts[1]);
            }

            public void Add(int h, int m)
            {
                hour = ((hour + 11 + h) % 12) + 1;
                minute += m;
                if (minute >= 60)
                {
                    hour++;
                    minute -= 60;
                }
            }

            public void Sub(int h, int m)
            {
                hour = ((hour + 11 - h) % 12) + 1;
                minute -= m;
                if (minute < 0)
                {
                    hour--;
                    minute += 60;
                }
            }
        }
        TheTime[,] _times = new TheTime[15, 15];
        TheTime[,] _origTimes = new TheTime[15, 15];

        void Form1_Load(object sender, EventArgs e)
        {
            bm = new Bitmap(panel1.Width, panel1.Height);

            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 15; j++)
                {
                    _times[i,j] = new TheTime(12, 0);
                    _origTimes[i, j] = new TheTime(_times[i, j]);
                }

            label1.Text = string.Format("Offset - Hours: {0}     Minutes: {1}", 0, 0);
        }

        Bitmap bm;
        void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics gc = Graphics.FromImage(bm);
            for (int row = 0; row < 15; row++)
                for (int col = 0; col < 15; col++)
                {
                    Point pt = new Point(col * 51, row * 51);
                    DrawClock(gc, pt, _times[row, col].hour, _times[row, col].minute);
                }
            
            e.Graphics.DrawImage(bm, 0, 0);
        }

        public void DrawClock(Graphics gc, Point pt, int hour, int minute)
        {
            Color bg = Color.White;
            Color fg = Color.Black;

            Pen fgPen = new Pen(fg);
            Brush bgFill = new SolidBrush(bg);
            Brush fgBrush = new SolidBrush(fg);
            var rect = new Rectangle(pt, new Size(50, 50));
            gc.FillRectangle(Brushes.White, pt.X, pt.Y, 51, 51);
            gc.FillEllipse(bgFill, rect);
            gc.DrawEllipse(fgPen, rect);
            var centerPt = new Point(pt.X + rect.Width / 2, pt.Y + rect.Height / 2);
            for (int i = 0; i < 12; i++)
            {
                var thisPt = new Point(centerPt.X, centerPt.Y);
                thisPt.Offset((int)(23 * Math.Cos(i * Math.PI / 6)), (int)(23 * Math.Sin(i * Math.PI / 6)));
                gc.FillRectangle(fgBrush, thisPt.X, thisPt.Y, 1, 1);
            }

            var hourPt = new Point(centerPt.X, centerPt.Y);
            hourPt.Offset((int)(15 * Math.Sin(hour * Math.PI / 6)), -(int)(15 * Math.Cos(hour * Math.PI / 6)));
            gc.DrawLine(new Pen(fg, 2), centerPt, hourPt);

            var minPt = new Point(centerPt.X, centerPt.Y);
            minPt.Offset((int)(20 * Math.Sin(minute * Math.PI / 30)), -(int)(20 * Math.Cos(minute * Math.PI / 30)));
            gc.DrawLine(fgPen, centerPt, minPt);
        }

        void vScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            int hour = vScrollBar1.Value / 60;
            int min = vScrollBar1.Value % 60;
            for (int row = 0; row < 15; row++)
                for (int col = 0; col < 15; col++)
                {
                    _times[row, col] = new TheTime(_origTimes[row, col].hour, _origTimes[row, col].minute);
                    _times[row, col].Add(hour, min);
                }
            label1.Text = string.Format("Offset - Hours: {0}     Minutes: {1}", hour, min);
            panel1.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int row = 0; row < 15; row++)
                for (int col = 0; col < 15; col++)
                {
                    _origTimes[row, col] = new TheTime(12, 0);
                }
            vScrollBar1.Value = 1;
            vScrollBar1.Value = 0;
        }
    }
}
