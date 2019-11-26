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

namespace Socket_Server
{
    public partial class Form1 : Form
    {
        private Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private Dictionary<string, Socket> ClientInformation = new Dictionary<string, Socket>();//一个字典
        public Form1()
        {
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;
            richTextBox1.Multiline = true;     //将Multiline属性设置为true，实现显示多行
            richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)//显示消息状态
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)//发送消息
        {

        }

        private void button2_Click(object sender, EventArgs e)//发送消息
        {
            richTextBox1.Text += "向客户端发送消息：\r\n";
            string sb = textBox3.Text;
            richTextBox1.Text += sb + "\r\n";
            string SendMessage = "接收服务器" + ServerSocket.LocalEndPoint.ToString() + "消息：" 
                + DateTime.Now + "\r\n" + sb + "\r\n";
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
            textBox3.Clear();
        }

        private void button1_Click(object sender, EventArgs e)//启动服务
        {
            try
            {
                int Port = Convert.ToInt32(textBox2.Text);
                IPAddress IP = IPAddress.Parse((string)textBox1.Text);
                ServerSocket.Bind(new IPEndPoint(IP, Port));
                ServerSocket.Listen(10);
                richTextBox1.Text += "启动监听成功！\r\n";
                richTextBox1.Text += "监听本地" + ServerSocket.LocalEndPoint.ToString() + "成功\r\n";
                Thread ThreadListen = new Thread(ListenConnection);
                ThreadListen.IsBackground = true;
                ThreadListen.Start();
            }
            catch (Exception ex)
            {
                richTextBox1.Text += "监听异常！！！\r\n";
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
                    richTextBox1.Text += "监听套接字异常" + ex.Message;
                    break;
                }
                //获取客户端端口号和IP
                IPAddress ClientIP = (ConnectionSocket.RemoteEndPoint as IPEndPoint).Address;
                int ClientPort = (ConnectionSocket.RemoteEndPoint as IPEndPoint).Port;
                string SendMessage = "本地IP:" + ClientIP +
                    ",本地端口:" + ClientPort.ToString();
                ConnectionSocket.Send(Encoding.UTF8.GetBytes(SendMessage));
                string remotePoint = ConnectionSocket.RemoteEndPoint.ToString();
                richTextBox1.Text += "成功与客户端" + remotePoint + "建立连接\r\n";
                ClientInformation.Add(remotePoint, ConnectionSocket);
                ParameterizedThreadStart pts = new ParameterizedThreadStart(ReceiveMessage);
                Thread thread = new Thread(pts);
                thread.IsBackground = true;//设置后台线程
                thread.Start(ConnectionSocket);
               // ClientThread.Add(remotePoint, thread);
            }
        }
        private void ReceiveMessage(Object SocketClient)///接收消息
        {
            Socket ReceiveSocket = (Socket)SocketClient;
            while(true)
            {
                try
                {
                    int ReceiveLength = 0;
                    byte[] result = new byte[1024 * 1024 * 10];
                    ReceiveLength = ReceiveSocket.Receive(result);
                    string str = ReceiveSocket.RemoteEndPoint.ToString();
                    if(result[0]==0)
                    {
                        string ReceiveMessage = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                        richTextBox1.Text += ReceiveMessage;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("接收异常！");
                    break;
                }
            }
        }
        private void textBox2_TextChanged(object sender, EventArgs e)//PORT
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)//IP
        {

        }
    }
}
