using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Runtime.InteropServices;

namespace cam5
{
    public partial class Form1 : Form
    {
        private int centerx;
        private int centery;
        private float areahcn;
        private Rectangle hcn;
        private bool start = false;
        private bool end = false;
        private int startx;
        private int starty;
        private int startz;
        private int endx;
        private int endy;
        private int endz;
        private TimeSpan starttime;
        private TimeSpan endtime;
        private FilterInfoCollection cameras;
        private VideoCaptureDevice cam;
        private Screen screen = Screen.PrimaryScreen;
        private int sw;
        private int sh;
       
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            sw = screen.Bounds.Width;
            sh = screen.Bounds.Height;
            cameras = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo info in cameras) {
                comboBox1.Items.Add(info.Name);
            }
            comboBox1.SelectedIndex = 2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (cam != null && cam.IsRunning)
            {
                cam.Stop();
            }
            cam = new VideoCaptureDevice(cameras[comboBox1.SelectedIndex].MonikerString);
            cam.VideoResolution = cam.VideoCapabilities[0];
          
            cam.NewFrame += new NewFrameEventHandler(cam_NewFrame);
            cam.Start();
        }
        private void MouseClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
        //public void speed(int sx,int sy,int sz,TimeSpan st,int ex,int ey,int ez,TimeSpan et) {
            //int a = Math.Abs(sx - ex);
            //int b = Math.Abs(sy - ey);
            //int c = Math.Abs(sz - ez);
            //TimeSpan t = et.Subtract(st);
            //double t1 = (double)t.TotalMilliseconds;
            //double d = Math.Sqrt(a * a + b * b + c * c);

            //double speed = (double)(d ) / (t1 / 1000000);
            //Console.WriteLine(t1.ToString());
            //Console.WriteLine(d.ToString());
            //Console.WriteLine(speed.ToString() + "m/s");
            //sx = 0;
            //sy = 0;
            //sz = 0;
            //ex = 0;
            //ey = 0;
            //ez = 0;
            
            
        //}
        void cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {

            Bitmap bm = (Bitmap)eventArgs.Frame.Clone();
            Image<Bgr, byte> image = new Image<Bgr, byte>(bm);
            Image<Bgr, byte> emguimage = image.Flip(Emgu.CV.CvEnum.FLIP.HORIZONTAL);
            Image<Hsv, byte> hsvimage = emguimage.Convert<Hsv, byte>();
            //{'hmin': 30, 'smin': 111, 'vmin': 54, 'hmax': 60, 'smax': 255, 'vmax': 255}

            Hsv min = new Hsv(30, 111, 54);
            Hsv max = new Hsv(45, 255, 255);
            Image<Gray, byte> mask = hsvimage.InRange(min, max);


            Contour<Point> contours = mask.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL);

            for (; contours != null; contours = contours.HNext)
            {
                Contour<Point> app = contours.ApproxPoly(contours.Perimeter * 0.015);
                //contours.Area > 50 &&
                if ( contours.Area < 100000 && app.Total > 7)
                {

                    //emguimage.Draw(contours, new Bgr(Color.Red), 4);
                    hcn = contours.BoundingRectangle;
                    centerx = hcn.X + hcn.Width / 2;
                    centery = hcn.Y + hcn.Height / 2;
                    areahcn = hcn.Width * hcn.Height;
                    emguimage.Draw(hcn, new Bgr(0, 255, 255), 2);
                    emguimage.Draw(new CircleF(new PointF(centerx, centery), 2), new Bgr(0, 0, 255), -1);
                    int x = centerx * (sw / 640);
                    int y = centery * (sh / 480);
                    //Console.WriteLine(areahcn.ToString();
                    if (hcn.Height >= hcn.Width * 1.5)
                    {
                        double speed1 = Math.Sqrt(hcn.Width * hcn.Width + hcn.Height * hcn.Height) / (1 / 30);
                        decimal speed2 = (decimal)speed1;
                        MessageBox.Show("tốc độ " + hcn.Width.ToString() + " " + hcn.Height.ToString() + speed2.ToString());
                    }
                    else
                    {
                        if (areahcn <= 300 && areahcn >= 200 && start == false && end == false)
                        {
                            startx = centerx;
                            starty = centery;
                            startz = (int)areahcn;
                            starttime = DateTime.Now.TimeOfDay;
                            //Console.WriteLine(starttime.ToString());
                            start = true;
                            //MessageBox.Show(startx.ToString()+" "+starty.ToString()+" "+startz.ToString());
                        }
                        if (areahcn <= 100 && end == false && start == true)
                        {
                            endx = centerx;
                            endy = centery;
                            endz = (int)areahcn;
                            endtime = DateTime.Now.TimeOfDay;
                            int a = Math.Abs(startx - endx);
                            int b = Math.Abs(starty - endy);
                            int c = Math.Abs(startz - endz);
                            double d = Math.Sqrt(a * a + b * b + c * c);
                            TimeSpan t = endtime.Subtract(starttime);
                            double t1 = t.Milliseconds;
                            double speed = (d / 100000) / (t1 / 1000000);
                            MessageBox.Show(speed.ToString());
                            speed = 0;
                            //Console.WriteLine(endtime.ToString());
                            //console.writeline(endtime.tostring());
                            end = false;
                            start = false;
                        }
                    }
                    Console.WriteLine(areahcn.ToString());

                }
            }
            

            pictureBox1.Image = emguimage.ToBitmap();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            start = false;
            end = false;
        }
        
    }
}
