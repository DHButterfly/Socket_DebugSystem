using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Windows;
using Google.Protobuf;
using System.Drawing.Imaging;

namespace RemoteClient
{
    public partial class Server_Client : Form
    {
        public string Information = null;
        public bool connectCheck = false;//连接状态
        private int heartNum = 0;//心跳计数
        private object locker = new object();//线程锁
        private Socket ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Server_Client()
        {
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;
            richTextBox1.Multiline = true;     //将Multiline属性设置为true，实现显示多行
            richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
            richTextBox2.Multiline = true;     //将Multiline属性设置为true，实现显示多行
            richTextBox2.ScrollBars = RichTextBoxScrollBars.Vertical;
            textBox1.Text = "127.0.0.1";
            textBox2.Text = "9999";

        }

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
        private void Screen_Test()
        {
            while(true)
            {
                try
                {
                    //获得当前屏幕的分辨率
                    long ImageLength = 0;
                    Screen scr = Screen.PrimaryScreen;
                    Rectangle rc = scr.Bounds;
                    int iWidth = rc.Width;
                    int iHeight = rc.Height;
                    //创建一个和屏幕一样大的Bitmap            
                    Image MyImage = new Bitmap(iWidth, iHeight);
                    //从一个继承自Image类的对象中创建Graphics对象            
                    Graphics g = Graphics.FromImage(MyImage);
                    //将屏幕的(0,0)坐标截图内容copy到画布的(0,0)位置,尺寸到校 new Size(iWidth, iHeight)
                    //保存在MyImage中
                    g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(iWidth, iHeight));
                    ///需要将信息组包发送，使得服务端能够正确接收每一个包，区分每一帧图像
                    ///以字符#123标识一帧的开始加上标识发送类型一共5字节
                    MemoryStream ms = new MemoryStream();
                    MyImage.Save(ms, ImageFormat.Jpeg);//保存到流ms中
                    byte[] arr = ImageToByteArray(MyImage);
                    //发送图片信息
                    string length = arr.Length.ToString();//图片总长度
                    string str1 = string.Format("{0}-{1}", "dc", length);
                    byte[] arr1 = Encoding.UTF8.GetBytes(str1);
                    byte[] arrSend1 = new byte[arr1.Length + 1];
                    arrSend1[0] = 5;//发送图片信息
                    Buffer.BlockCopy(arr1, 0, arrSend1, 1, arr1.Length);
                    ClientSocket.Send(arrSend1);
                    Thread.Sleep(1000);
                    //发送图片
                    int end = 1;
                    bool vis = true;
                    ms.Position = 0;
                    
                    while (end != 0)
                    {
                        if (vis)
                        {
                            byte[] arrPictureSend = new byte[1025];
                            arrPictureSend[0] = 4;
                            end = ms.Read(arrPictureSend, 1, 1024);//end为0标识读取完毕
                            ClientSocket.Send(arrPictureSend, 0, 1024 + 1, SocketFlags.None);
                            vis = false;
                        }
                        else
                        {
                            byte[] arrPictureSend = new byte[1024];
                            end = ms.Read(arrPictureSend, 0, 1024);
                            ClientSocket.Send(arrPictureSend, 0, 1024, SocketFlags.None);
                        }
                    }
                    Thread.Sleep(30000);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("截图发送异常！！！");
                    ClientSocket.Shutdown(SocketShutdown.Both);
                    ClientSocket.Close();
                    break;
                }
            }
        }
        //ImageToByteArray函数
        public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
            ///byte[] bytes = ms.GetBuffer();byte[] bytes=ms.ToArray(); 这两句都可以
            ///MemoryStream的GetBuffer并不是得到这个流所存储的内容，
            ///而是返回这个流的基础字节数组，可能包括在扩充的时候一些没有使用到的字节
        }
        private void Server_Client_Load(object sender, EventArgs e)
        {

        }
        private void textBox1_TextChanged(object sender, EventArgs e)//IP
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)//PORT
        {

        }

        private void button2_Click(object sender, EventArgs e)//连接服务器
        {
            int PORT = Convert.ToInt32(textBox2.Text);
            IPAddress IP = IPAddress.Parse((string)textBox1.Text);
            if(textBox1.Text==""||textBox2.Text=="")
            {
                MessageBox.Show("请先完成服务器配置！");
                return;
            }
            try
            {
                ClientSocket.Connect(new IPEndPoint(IP, PORT));
               if(ClientSocket.Connected)//成功连上服务器
                {
                    connectCheck = true;
                    richTextBox1.Text += "连接服务器成功！\r\n";
                    //心跳包发送线程
                    /*Thread thread1 = new Thread(Heart);
                    thread1.IsBackground = true;
                    thread1.Start();//将此线程加入就绪队列，等待操作系统调度*/
                    //截图发送线程
                   Thread thread_test = new Thread(Screen_Test);
                    thread_test.IsBackground = true;
                    thread_test.Start();
                    //接收消息线程
                    Thread thread = new Thread(ReceiveMessage);
                    thread.IsBackground = true;
                    thread.Start();
                }
               else
                {
                    richTextBox1.Text += "连接服务器失败，请等下再试!";
                    connectCheck = false;
                }
            }
            catch (Exception ex)
            {
                connectCheck = false;
                richTextBox1.Text += "连接服务器失败！\r\n";
                return;
            }
        }
        private void ReceiveMessage()
        {
            long TotalLength = 0;
            //如果位while（true），当窗体关闭之后还会有线程一直在跑，这样服务端就会认为该客户端没断线
            while (this.Visible)
            {
                byte[] result = new byte[1024 * 1024 * 10];
                int ReceiveLength = 0;
                try
                {
                    ReceiveLength = ClientSocket.Receive(result);
                    if (result[0] == 0)//表示接收到的是消息
                    {
                        try
                        {
                            richTextBox1.Text += "接收服务器消息：\r\n";
                            string str = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                            richTextBox1.Text += str + "\r\n";
                        }
                        catch (Exception ex)
                        {
                            richTextBox1.Text += "接收服务器消息失败！\r\n";
                            ClientSocket.Shutdown(SocketShutdown.Both);
                            ClientSocket.Close();
                            break;
                        }
                    }
                    if(result[0]==6)//接收到监控端发送来的参数信息
                    {
                        string Information = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                        Socket_Client sc = new Socket_Client();
                        sc.Parameter = Information;
                        richTextBox1.Text += "接收修改参数：" + Information + "\r\n";
                        Status sta = new Status();
                       if (Information != null)
                        {
                            if (Information[0] == '1')
                            {
                                sta.flag11 = true;
                                sta.pictureBox1.Image = Properties.Resources.led_O;
                            }

                            if (Information[1] == '1')
                            {
                                sta.flag22 = true;
                                sta.pictureBox2.Image = Properties.Resources.led_O;
                            }
                            if (Information[2] == '1')
                            {
                                sta.flag33 = true;
                                sta.pictureBox3.Image = Properties.Resources.led_O;
                            }

                            if (Information[3] == '1')
                            {
                                sta.flag44 = true;
                                sta.pictureBox4.Image = Properties.Resources.led_O;
                            }

                            if (Information[4] == '1')
                            {
                                sta.flag55 = true;
                                sta.pictureBox5.Image = Properties.Resources.led_O;
                            }

                            if (Information[5] == '1')
                            {
                                sta.flag66 = true;
                                sta.pictureBox6.Image = Properties.Resources.led_O;
                            }

                            if (Information[6] == '1')
                            {
                                sta.flag77 = true;
                                sta.pictureBox7.Image = Properties.Resources.led_O;
                            }

                            if (Information[7] == '1')
                            {
                                sta.flag88 = true;
                                sta.pictureBox8.Image = Properties.Resources.led_O;
                            }
                        }
                        //sta.StartPosition = FormStartPosition.CenterScreen;
                        //sta.Show();
                    }
                    if(result[0]==3)//接收到服务端返回的认证包
                    {
                        richTextBox2.Text += DateTime.Now+"接受服务器认证包:";
                        string str = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                        richTextBox2.Text += str + "\r\n";
                        heartNum = 0;//收到认证包
                    }
                    if(heartNum>6)
                    {
                        connectCheck = false;
                        MessageBox.Show("服务器可能已经挂掉！");
                        ClientSocket.Shutdown(SocketShutdown.Both);
                        ClientSocket.Close();
                    }
                    if (result[0] == 2)
                    {
                        string fileNameWithLength = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                        string str1 = fileNameWithLength.Split('-').First();
                        TotalLength = Convert.ToInt64(fileNameWithLength.Split('-').Last());
                        richTextBox1.Text += "接收服务器后缀名为：" + str1 + "的文件" + "\r\n";
                    }
                    if (result[0] == 1)//表示接收到的是文件
                    {
                        try
                        {
                            richTextBox1.Text += "文件总长度" + TotalLength.ToString() + "\r\n";
                            SaveFileDialog sfd = new SaveFileDialog();
                            if (sfd.ShowDialog(this) == DialogResult.OK)
                            {
                                string fileSavePath = sfd.FileName;//获取文件保存路径
                                long receiveLength = 0;
                                bool flag = true;
                                int rec = 0;
                                using (FileStream fs = new FileStream(fileSavePath, FileMode.Create, FileAccess.Write))
                                {
                                    while (TotalLength > receiveLength)
                                    {
                                        if (flag)
                                        {
                                            fs.Write(result, 1, ReceiveLength - 1);
                                            fs.Flush();
                                            receiveLength += ReceiveLength - 1;
                                            flag = false;
                                        }
                                        else
                                        {
                                            rec = ClientSocket.Receive(result);
                                            fs.Write(result, 0, rec);
                                            fs.Flush();
                                            receiveLength += rec;
                                        }
                                    }
                                    richTextBox1.Text += "文件保存成功：" + " " + fileSavePath + "\r\n";
                                    fs.Close();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            richTextBox1.Text += "文件保存失败！\r\n";
                            MessageBox.Show(ex.Message);
                            connectCheck = false;
                            ClientSocket.Shutdown(SocketShutdown.Both);
                            ClientSocket.Close();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("接收出错，服务器可能已经挂了！");
                    break;
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)//发送文件
        {
            if (string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("选择要发送的文件！！！");
            }
            else
            {
                //采用文件流方式打开文件
                using (FileStream fs = new FileStream(textBox3.Text, FileMode.Open))
                {
                    long TotalLength = fs.Length;//文件总长度
                    ScheDule a = new ScheDule();
                    a.progressBar1.Value = 0;//设置进度条的当前位置为0
                    a.progressBar1.Minimum = 0; //设置进度条的最小值为0
                    a.progressBar1.Maximum = Convert.ToInt32(TotalLength);//设置进度条的最大值
                    richTextBox1.Text += "文件总长度为：" + TotalLength.ToString() + "\r\n";
                    string fileName = Path.GetFileName(textBox3.Text);//文件名
                    string fileExtension = Path.GetExtension(textBox3.Text);//扩展名
                    string str = string.Format("{0}-{1}", fileName, TotalLength);
                    byte[] arr = Encoding.UTF8.GetBytes(str);
                    byte[] arrSend = new byte[arr.Length + 1];
                    Buffer.BlockCopy(arr, 0, arrSend, 1, arr.Length);
                    arrSend[0] = 2;//发送文件信息
                    ClientSocket.Send(arrSend);

                    byte[] arrFile = new byte[1024 * 1024 * 10];
                    int FileLength = 0;//将要发送的文件读到缓冲区
                    long SendFileLength = 0;
                    bool flag = true;
                    long num1 = 0;
                    while (TotalLength > SendFileLength && (FileLength = fs.Read(arrFile, 0, arrFile.Length)) > 0)
                    {
                        SendFileLength += FileLength;
                        if (flag)
                        {
                            byte[] arrFileSend = new byte[FileLength + 1];
                            arrFileSend[0] = 1;
                            Buffer.BlockCopy(arrFile, 0, arrFileSend, 1, FileLength);
                            ClientSocket.Send(arrFileSend, 0, FileLength + 1, SocketFlags.None);
                            flag = false;
                        }
                        else
                        {
                            ClientSocket.Send(arrFile, 0, FileLength, SocketFlags.None);
                        }
                        a.progressBar1.Value += FileLength;
                        num1 += FileLength * 100;
                        long num2 = TotalLength;
                        long num = num1 / num2;
                        a.label1.Text = (num1 / num2).ToString() + "%";
                        a.Show();
                        Application.DoEvents();//重点，必须加上，否则父子窗体都假死
                    }
                    try
                    {
                        if (a.progressBar1.Value == a.progressBar1.Maximum)
                        {
                            MessageBox.Show("文件发送完毕！！！");
                            a.Close();
                            textBox3.Clear();
                            fs.Close();
                        }
                    }
                    catch
                    {
                        MessageBox.Show("文件发送失败！");
                        a.Close();
                        textBox3.Clear();
                        fs.Close();
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)//选择文件
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = "D:\\";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text += ofd.FileName;
            }
        }
        private void Heart()
        {
            while (this.Visible)//窗体关闭循环即结束
            {
                if (connectCheck)
                {
                    try
                    {
                        string str = "#####";
                        string Client = ClientSocket.RemoteEndPoint.ToString();
                        string SendMessage = "接收客户端" + Client + "心跳包：" + DateTime.Now + "\r\n" + str + "\r\n";
                        byte[] result1 = Encoding.UTF8.GetBytes(SendMessage);
                        byte[] result = new byte[result1.Length + 1];
                        result[0] = 3;
                        Buffer.BlockCopy(result1, 0, result, 1, result1.Length);
                        ClientSocket.Send(result);
                        heartNum++;//发一个包计数一次，收到返回包时清0，如果三次没收到认证包，则重连
                        /*Thread thread_test = new Thread(Screen_Test);
                        thread_test.IsBackground = true;
                        thread_test.Start();*/
                        Thread.Sleep(3000);
                        //获得当前屏幕的分辨率
                        /*long ImageLength = 0;
                        Screen scr = Screen.PrimaryScreen;
                        Rectangle rc = scr.Bounds;
                        int iWidth = rc.Width;
                        int iHeight = rc.Height;
                        //创建一个和屏幕一样大的Bitmap            
                        Image MyImage = new Bitmap(iWidth, iHeight);
                        //从一个继承自Image类的对象中创建Graphics对象            
                        Graphics g = Graphics.FromImage(MyImage);
                        //将屏幕的(0,0)坐标截图内容copy到画布的(0,0)位置,尺寸到校 new Size(iWidth, iHeight)
                        //保存在MyImage中
                        g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(iWidth, iHeight));
                        ///需要将信息组包发送，使得服务端能够正确接收每一个包，区分每一帧图像
                        ///以字符#123标识一帧的开始加上标识发送类型一共5字节
                        MemoryStream ms = new MemoryStream();
                        MyImage.Save(ms, ImageFormat.Jpeg);//保存到流ms中
                        byte[] arr = ImageToByteArray(MyImage);
                        //发送图片信息
                        string length = arr.Length.ToString();//图片总长度
                        string str1 = string.Format("{0}-{1}", "dc", length);
                        byte[] arr1 = Encoding.UTF8.GetBytes(str1);
                        byte[] arrSend1 = new byte[arr1.Length + 1];
                        arrSend1[0] = 5;//发送图片信息
                        Buffer.BlockCopy(arr1, 0, arrSend1, 1, arr1.Length);
                        ClientSocket.Send(arrSend1);
                        //Thread.Sleep(1000);
                        //发送图片
                        int end = 1;
                        bool vis = true;
                        ms.Position = 0;
                        while (end != 0)
                        {
                            if (vis)
                            {
                                byte[] arrPictureSend = new byte[1025];
                                arrPictureSend[0] = 4;
                                end = ms.Read(arrPictureSend, 1, 1024);//end为0标识读取完毕
                                ClientSocket.Send(arrPictureSend, 0, 1024 + 1, SocketFlags.None);
                                vis = false;
                            }
                            else
                            {
                                byte[] arrPictureSend = new byte[1024];
                                end = ms.Read(arrPictureSend, 0, 1024);
                                ClientSocket.Send(arrPictureSend, 0, 1024, SocketFlags.None);
                            }
                        }
                        ms.Dispose();
                        MyImage.Dispose();*/
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("心跳包发送异常！"+ex.ToString());
                        connectCheck = false;
                        ClientSocket.Shutdown(SocketShutdown.Both);
                        ClientSocket.Close();
                        break;
                    }
                }
            }
        }
        private void button5_Click(object sender, EventArgs e)//发送消息
        {
            try
            {
                richTextBox1.Text += "向服务器发送消息:" + textBox4.Text + "\r\n";
                string SendMessage = textBox4.Text;
                string Client = ClientSocket.RemoteEndPoint.ToString();
                byte[] result1 = Encoding.UTF8.GetBytes(SendMessage);
                byte[] result = new byte[result1.Length + 1];
                result[0] = 0;
                Buffer.BlockCopy(result1, 0, result, 1, result1.Length);
                ClientSocket.Send(result);
                textBox4.Clear();
            }
            catch (Exception ex)
            {
                richTextBox1.Text += "发送消息失败，服务器可能已经关闭！\r\n";
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
            }

        }

        private void textBox4_TextChanged(object sender, EventArgs e)//输入消息
        {
            string str = Console.ReadLine();
            textBox4.Text += str;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)//选择文件
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length; //设定光标位置
            richTextBox1.ScrollToCaret(); //滚动到光标处
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Socket_Client sc = new Socket_Client();
            sc.StartPosition = FormStartPosition.CenterScreen;
            sc.Show();
           // LoginForm dlg = new LoginForm();
           //ctrl +k,ctrl+C  实现多行注释
           // dlg.ShowDialog();
           // 这里ShowDialog方法表示你必须先操作完dlg窗口，才能操作后面的主窗体。
           // 如果要登录窗口显示在主窗口的中心，则在显示之前设置如下
           // dlg.StartPosition = FormStartPosition.CenterParent;
           // dlg.ShowDialog();
           // 能够这样做的前提是主窗体必须先定义和显示。否则登录窗体可能无法找到父窗体。
           //除此之外，也可以手动设置窗口显示的位置，即窗口坐标。
           //首先必须把窗体的显示位置设置为手动。
           //dlg.StartPosition = FormStartPosition.Manual;
           //随后获取屏幕的分辨率，也就是显示器屏幕的大小。
           //int xWidth = SystemInformation.PrimaryMonitorSize.Width;//获取显示器屏幕宽度
           //int yHeight = SystemInformation.PrimaryMonitorSize.Height;//高度
           //然后定义窗口位置，以主窗体为例
           //mainForm.Location = new Point(xWidth / 2, yHeight / 2);//这里需要再减去窗体本身的宽度和高度的一半
           //mainForm.Show();
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {
            richTextBox2.SelectionStart = richTextBox2.Text.Length; //设定光标位置
            richTextBox2.ScrollToCaret(); //滚动到光标处
        }
        #region
        /// <summary>
        /// 窗体界面关闭之后强制关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #endregion
        private void Server_Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);//当窗体关闭之后，强制关闭窗体所有运行线程
        }

        private void button6_Click(object sender, EventArgs e)//参数发送
        {
            string Information = textBox5.Text;
            if(Information!="")
            {
                bool flag = true;
                for (int i = 0; i < Information.Length; i++)
                {
                    int num = Information[i] - '0';
                    if (num != 0 && num != 1)
                    {
                        flag = false;
                        break;
                    }
                }
                if (!flag || Information.Length != 8)
                {
                    MessageBox.Show("请输入8位二进制数！");
                    return;
                }
            }
            byte[] arr1 = Encoding.UTF8.GetBytes(Information);
            byte[] arrSend1 = new byte[arr1.Length + 1];
            arrSend1[0] = 6;//发送参数信息
            Buffer.BlockCopy(arr1, 0, arrSend1, 1, arr1.Length);
            ClientSocket.Send(arrSend1);
            richTextBox1.Text += DateTime.Now + ":参数发送成功\r\n";
            textBox5.Clear();
        }
        private void textBox5_TextChanged(object sender, EventArgs e)//参数发送
        {

        }
    }
}
