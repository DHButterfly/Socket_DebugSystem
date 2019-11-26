using System;
using System.Collections.Generic;
using System.ComponentModel;
using System ; 
using System.Drawing ; 
using System.Collections ; 
using System.ComponentModel ; 
using System.Windows.Forms ; 
using System.Data ; 
using System.Drawing.Imaging ;
using System.IO;
using System.Threading;
using System.Text;

namespace Screen_Test
{
    public partial class Form1 : Form
    {
        private bool flag = false;
        public Form1()
        {
            InitializeComponent();
            string str = "0#123";
            byte[] result1 = new byte[100];
            result1[0] = 1;
            result1[1] = 2;
            result1[2] = 3;
            result1= Encoding.UTF8.GetBytes(str);
            for(int i=0;i<result1.Length;i++)
            {
                Console.WriteLine(result1[i]);
            }
            Console.WriteLine("result1.length={0}", result1.Length);
        }
        //声明一个API函数 
        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        private static extern bool BitBlt(//Win32 API的CreateDC,
            IntPtr hdcDest, // 目标 DC的句柄 
            int nXDest,
            int nYDest,
            int nWidth,
            int nHeight,
            IntPtr hdcSrc, // 源DC的句柄 
            int nXSrc,
            int nYSrc,
            System.Int32 dwRop // 光栅的处理数值 
            );
        private void button1_Click(object sender, EventArgs e)
        {
            int num = 1;
            while(num-->0)
            {
                //获得当前屏幕的大小 
               /* Rectangle rect = new Rectangle();
                 rect = Screen.GetWorkingArea(this);
                 //创建一个以当前屏幕为模板的图象 
                 Graphics g1 = this.CreateGraphics();
                 //创建以屏幕大小为标准的位图 
                 Image MyImage = new Bitmap(rect.Width, rect.Height, g1);

                 Graphics g2 = Graphics.FromImage(MyImage);
                 //得到屏幕的DC 
                 IntPtr dc1 = g1.GetHdc();
                 //得到Bitmap的DC 
                 IntPtr dc2 = g2.GetHdc();
                 //调用此API函数，实现屏幕捕,保存在Bitmap的MyImage对象中
                 BitBlt(dc2, 0, 0, rect.Width, rect.Height, dc1, 0, 0, 13369376);
                 //释放掉屏幕的DC 
                 g1.ReleaseHdc(dc1);
                 //释放掉Bitmap的DC 
                 g2.ReleaseHdc(dc2);
                 //以JPG文件格式来保存 
                 //MyImage.Save(@"c:\Capture.jpg", ImageFormat.Jpeg);
                 //image类型转到二进制
                 byte[] b = ImageToByteArray(MyImage);

                 byte[] arr = ImageToByteArray(MyImage);
                 byte[] arrSend = new byte[arr.Length + 1];
                 Buffer.BlockCopy(arr, 0, arrSend, 1, arr.Length);
                 arrSend[0] = 4;//发送文件信息

                 Rectangle rect1 = new Rectangle();
                 rect1 = Screen.GetWorkingArea(this);
                 Image MyImage1 = new Bitmap(rect1.Width, rect1.Height);

                 MyImage1 = ByteArrayToImage(arrSend, arrSend.Length);

                 MemoryStream ms = new MemoryStream(arrSend, true);
                 ms.Write(arrSend, 1, arrSend.Length - 1);
                //Console.WriteLine("arrSend.Length={0}", arrSend.Length);
                 MyImage1.Save(@"c:\Capture11.jpg", ImageFormat.Jpeg);*/
                 Form2 fm = new Form2();
                 //fm.pictureBox1.Image = new Bitmap(ms, true);
                 //fm.pictureBox1.Image = MyImage1;
                 //fm.pictureBox1.Image = Image.FromFile("c:\\Capture11.jpg", false);
                //获得当前屏幕的分辨率            
               Screen scr = Screen.PrimaryScreen;
                Rectangle rc = scr.Bounds;
                int iWidth = rc.Width;
                int iHeight = rc.Height;
                //创建一个和屏幕一样大的Bitmap            
                Image myImage = new Bitmap(iWidth, iHeight);
                //从一个继承自Image类的对象中创建Graphics对象            
                Graphics g = Graphics.FromImage(myImage);
                //抓屏并拷贝到myimage里 
                int cnt = 1;
                while(cnt-->0)
                {
                    g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(iWidth, iHeight));
                    //保存为文件            
                    //myImage.Save(@"c:\\Capture11.jpg", ImageFormat.Jpeg);
                    byte[] arr = ImageToByteArray(myImage);
                    MemoryStream ms = new MemoryStream(arr, true);
                    ms.Write(arr, 0, arr.Length);
                    fm.pictureBox1.Image = new Bitmap(ms, true);
                    //fm.pictureBox1.Image = Image.FromFile("c:\\Capture11.jpg", false);
                    if (!flag)
                    {
                        fm.StartPosition = FormStartPosition.Manual;
                        fm.Show();
                        flag = true;
                    }
                    else fm.Refresh();
                    Thread.Sleep(1000);
                    //MessageBox.Show("转换后的图片保存为C盘的1.jpg文件！");
                }
            }
        }

        public byte[] ImageToByteArray(Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, ImageFormat.Jpeg);
            return ms.ToArray();
        }
        public Image ByteArrayToImage(byte[] byteArrayIn, int count)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn, 1, count-1);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = "我的小猪猪在哪儿呢！";
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
