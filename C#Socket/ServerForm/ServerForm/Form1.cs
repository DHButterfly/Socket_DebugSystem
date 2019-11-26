using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Threading;
namespace ServerForm//服务器
{
    public partial class Form1 : Form
    {
        private Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private Dictionary<string, Socket> ClientInformation = new Dictionary<string, Socket>();
        private Dictionary<string, Thread> ClientThread = new Dictionary<string, Thread>();
        public Form1()
        {
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;
            richTextBox1.Multiline = true;     //将Multiline属性设置为true，实现显示多行
            richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
            richTextBox2.Multiline = true;     //将Multiline属性设置为true，实现显示多行
            richTextBox2.ScrollBars = RichTextBoxScrollBars.Vertical;　//设
            //richTextBox3.Multiline = true;     //将Multiline属性设置为true，实现显示多行
            //richTextBox3.ScrollBars = RichTextBoxScrollBars.Vertical;　//设
            //在多线程程序中，新创建的线程不能访问UI线程创建的窗口控件
            //如果需要访问窗口控件则可以如此设置
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ;
        }
        private void label1_Click(object sender, EventArgs e)//ip
        {

        }
        private void label2_Click(object sender, EventArgs e)//Port
        {

        }
        private void label3_Click(object sender, EventArgs e)//在线列表
        {

        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)//显示客户端之间/客户端消息
        {

        }
        private void richTextBox2_TextChanged(object sender, EventArgs e)//显示连接成功的客户端
        {

        }
        private void textBox1_TextChanged(object sender, EventArgs e)//IP
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)//Port
        {

        }
        private void button1_Click(object sender, EventArgs e)//启动服务
        {
            try
            {
                int Port = Convert.ToInt32(textBox2.Text);
                IPAddress IP = IPAddress.Parse((string)textBox1.Text);
                ServerSocket.Bind(new IPEndPoint(IP, Port));
                ServerSocket.Listen(20);
                richTextBox2.Text += "启动监听成功！\r\n";
                richTextBox2.Text += "监听本地" + ServerSocket.LocalEndPoint.ToString() + "成功\r\n";
                Thread ThreadListen = new Thread(ListenConnection);
                ThreadListen.IsBackground = true;
                ThreadListen.Start();
            }
            catch (Exception ex)
            {
                richTextBox2.Text += "监听异常！！！\r\n";
                ServerSocket.Shutdown(SocketShutdown.Both);
                ServerSocket.Close();
            }
        }
        private void ListenConnection()
        {
            Socket ConnectionSocket = null;
            while (true)
            {
                try
                {
                    ConnectionSocket = ServerSocket.Accept();
                }
                catch (Exception ex)
                {
                    richTextBox2.Text += "监听套接字异常" + ex.Message;
                    break;
                }
                //获取客户端端口号和IP
                IPAddress ClientIP = (ConnectionSocket.RemoteEndPoint as IPEndPoint).Address;
                int ClientPort = (ConnectionSocket.RemoteEndPoint as IPEndPoint).Port;
                string SendMessage = "本地IP:" + ClientIP +
                    ",本地端口:" + ClientPort.ToString();
                ConnectionSocket.Send(Encoding.UTF8.GetBytes(SendMessage));
                string remotePoint = ConnectionSocket.RemoteEndPoint.ToString();
                richTextBox2.Text += "成功与客户端" + remotePoint + "建立连接\r\n";
                // richTextBox3.Text += DateTime.Now + ":" + remotePoint + "\r\n";
                listBox1.Items.Add(remotePoint);
                ClientInformation.Add(remotePoint, ConnectionSocket);
                ParameterizedThreadStart pts = new ParameterizedThreadStart(ReceiveMessage);
                Thread thread = new Thread(pts);
                thread.IsBackground = true;
                thread.Start(ConnectionSocket);
                ClientThread.Add(remotePoint, thread);
            }
        }
        private void ReceiveMessage(Object SocketClient)///接收消息
        {
            Socket ReceiveSocket = (Socket)SocketClient;
            long FileLength = 0;
            while (true)
            {
                int ReceiveLength = 0;
                byte[] result = new byte[1024 * 1024 * 10];
                try
                {
                    IPAddress ClientIP = (ReceiveSocket.RemoteEndPoint as IPEndPoint).Address;
                    int ClientPort = (ReceiveSocket.RemoteEndPoint as IPEndPoint).Port;
                    ReceiveLength = ReceiveSocket.Receive(result);
                    string str = ReceiveSocket.RemoteEndPoint.ToString();
                    if (result[0] == 0)//接收消息
                    {
                        string ReceiveMessage = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                        richTextBox1.Text += ReceiveMessage;
                        if (ClientInformation.Count == 1) continue;//只有一个客户端
                        List<string> test = new List<string>(ClientInformation.Keys);
                        for (int i = 0; i < ClientInformation.Count; i++)
                        {//将接收到的消息群发出去
                            Socket socket = ClientInformation[test[i]];
                            string s = ReceiveSocket.RemoteEndPoint.ToString();
                            if (test[i] != s)
                            {
                                richTextBox1.Text += DateTime.Now + "\r\n" + "客户端" + str + "向客户端" + test[i] + "发送消息：\r\n";
                                byte[] arrMsg = Encoding.UTF8.GetBytes("客户端"+str+"向您发送消息：\r\n"+ReceiveMessage);
                                byte[] SendMsg = new byte[arrMsg.Length + 1];
                                SendMsg[0] = 0;
                                Buffer.BlockCopy(arrMsg, 0, SendMsg, 1, arrMsg.Length);
                                socket.Send(SendMsg);
                            }
                        }
                    }
                    if (result[0] == 1)//接收文件
                    {
                        try
                        {
                            richTextBox1.Text += "接收客户端:" + ReceiveSocket.RemoteEndPoint.ToString() +
                          "时间：" + DateTime.Now.ToString() + "\r\n" + "文件:\r\n";
                            SaveFileDialog sfd = new SaveFileDialog();
                            if (sfd.ShowDialog(this) == DialogResult.OK)
                            {
                                long Rlength = 0;
                                bool flag = true;
                                int rec = 0;
                                string fileSavePath = sfd.FileName;
                                richTextBox1.Text += "文件总长度为：" + FileLength.ToString() + "\r\n";
                                using (FileStream fs = new FileStream(fileSavePath, FileMode.Create))
                                {
                                    while (FileLength > Rlength)
                                    {
                                        if (flag)
                                        {
                                            fs.Write(result, 1, ReceiveLength - 1);
                                            fs.Flush();
                                            Rlength += (ReceiveLength - 1);
                                            flag = false;
                                        }
                                        else
                                        {
                                            rec = ReceiveSocket.Receive(result);
                                            fs.Write(result, 0, rec);
                                            fs.Flush();
                                            Rlength += rec;
                                        }
                                    }
                                    richTextBox1.Text += "文件保存成功：" + fileSavePath + "\r\n";
                                    fs.Close();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("服务器出现异常！");
                        }
                    }
                    if (result[0] == 2)//接收文件信息和长度
                    {
                        string fileNameWithLength = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                        //recStr = fileNameWithLength.Split('-').First();
                        FileLength = Convert.ToInt64(fileNameWithLength.Split('-').Last());
                        richTextBox1.Text += "FileLength=" + FileLength.ToString() +"\r\n";
                    }
                }
                catch (Exception ex)
                {
                    richTextBox2.Text += "监听出现异常！\r\n";
                    richTextBox2.Text += "客户端" + ReceiveSocket.RemoteEndPoint + "已经连接中断" + "\r\n" +
                    ex.Message + "\r\n" + ex.StackTrace + "\r\n";
                    listBox1.Items.Remove(ReceiveSocket.RemoteEndPoint.ToString());//从listbox中移除断开连接的客户端
                    string s = ReceiveSocket.RemoteEndPoint.ToString();
                    ClientInformation.Remove(s);
                    ReceiveSocket.Shutdown(SocketShutdown.Both);
                    ReceiveSocket.Close();
                    break;
                }
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)//选择文件
        {
            ;
        }

        private void button4_Click(object sender, EventArgs e)//发送文件
        {
            if (string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("请选择你要发送的文件！！！");
            }
            else
            {
                Form2 a = new Form2();
                long TotalLength1 = new FileInfo(textBox3.Text).Length;//文件总长度
                a.progressBar1.Value = 0;//设置进度条的当前位置为0
                a.progressBar1.Minimum = 0; //设置进度条的最小值为0
                a.progressBar1.Maximum = Convert.ToInt32(TotalLength1);//设置进度条的最大值
                string fileName = Path.GetFileName(textBox3.Text);//文件名
                string fileExtension = Path.GetExtension(textBox3.Text);//扩展名字||后缀
                string str = string.Format("{0}-{1}", fileExtension, TotalLength1);
                byte[] arr = Encoding.UTF8.GetBytes(str);
                byte[] arrSend = new byte[arr.Length + 1];
                arrSend[0] = 2;//发送文件信息
                Buffer.BlockCopy(arr, 0, arrSend, 1, arr.Length);
                foreach (string str1 in ClientInformation.Keys)
                {
                    ClientInformation[str1].Send(arrSend);
                    break;
                }
                byte[] arrFile = new byte[1024 * 1024 * 10];
                using (FileStream fs = new FileStream(textBox3.Text, FileMode.Open, FileAccess.Read))
                {
                    long TotalLength = fs.Length;//文件总长度
                    richTextBox1.Text += "文件总长度" + TotalLength.ToString() + "\r\n";
                    Buffer.BlockCopy(arr, 0, arrSend, 1, arr.Length);
                    foreach (string str1 in ClientInformation.Keys)
                    {
                        ClientInformation[str1].Send(arrSend);//发送文件名和长度
                        int FileLength = 0;//每次读取长度
                        long sendFileLength = 0;
                        bool flag = true;
                        long num1 = 0;
                        while ((TotalLength > sendFileLength) && (FileLength = fs.Read(arrFile, 0, arrFile.Length)) > 0)
                        {
                            sendFileLength += FileLength;
                            if (flag)
                            {
                                byte[] arrFileSend = new byte[FileLength + 1];
                                arrFileSend[0] = 1; // 用来表示发送的是文件数据
                                Buffer.BlockCopy(arrFile, 0, arrFileSend, 1, FileLength);
                                ClientInformation[str1].Send(arrFileSend, 0, FileLength + 1, SocketFlags.None);
                                flag = false;
                            }
                            else
                            {
                                ClientInformation[str1].Send(arrFile, 0, FileLength, SocketFlags.None);
                            }
                            a.progressBar1.Value += FileLength;
                            num1 += FileLength * 100;
                            long num2 = a.progressBar1.Maximum;//这里可能超范围，必要时使用高精度
                            long num = num1 / num2;
                            a.label1.Text = (num1/num2).ToString() + "%";
                            a.Show();
                            Application.DoEvents();//重点，必须加上，否则父子窗体都假死
                        }
                        if(a.progressBar1.Value== TotalLength)
                        {
                            
                            MessageBox.Show("文件发送完毕！！！");
                            a.Close();
                            textBox3.Clear();
                            fs.Close();
                            break;
                        }
                        else
                        {
                            MessageBox.Show("文件发送失败！！！");
                            a.Close();
                            textBox3.Clear();
                            fs.Close();
                            break;
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)//选择文件
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = "D:\\";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text += ofd.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)//发送消息
        {
            richTextBox1.Text += "向客户端发送消息：\r\n";
            string sb = textBox4.Text;
            richTextBox1.Text += sb + "\r\n";
            string SendMessage = "接收服务器" + ServerSocket.LocalEndPoint.ToString() + "消息：" + DateTime.Now + "\r\n" + sb + "\r\n";
            List<string> test = new List<string>(ClientInformation.Keys);
            for (int i = 0; i < ClientInformation.Count; i++)
            {//给每个在线客户端发送消息
                Socket socket = ClientInformation[test[i]];
                byte[] arrMsg = Encoding.UTF8.GetBytes(SendMessage);
                byte[] SendMsg = new byte[arrMsg.Length + 1];
                SendMsg[0] = 0;
                Buffer.BlockCopy(arrMsg, 0, SendMsg, 1, arrMsg.Length);
                socket.Send(SendMsg);
            }
            textBox4.Clear();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)//发送消息
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
