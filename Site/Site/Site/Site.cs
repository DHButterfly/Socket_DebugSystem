using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Site
{
    public partial class Site : Form
    {
        private Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private Dictionary<string, Socket> ClientInformation = new Dictionary<string, Socket>();//一个字典
        public Site()
        {
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;
            richTextBox1.Multiline = true;     //将Multiline属性设置为true，实现显示多行
            richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
            textBox1.Text = "127.0.0.1";
            textBox2.Text = "8888";
        }

        private void Site_Load(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)//设备信息
        {

        }

        private void button2_Click(object sender, EventArgs e)//启动服务
        {
            try
            {
                int PORT = Convert.ToInt32(textBox2.Text);
                IPAddress IP = IPAddress.Parse((string)textBox1.Text);
                ServerSocket.Bind(new IPEndPoint(IP, PORT));
                ServerSocket.Listen(20);
                richTextBox1.Text += "启动监听成功！\r\n";
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
                    richTextBox1.Text += "连接异常！\r\n";
                    ConnectionSocket.Shutdown(SocketShutdown.Both);
                    ConnectionSocket.Close();
                    break;
                }
                //获取客户端端口号和IP
                IPAddress ClientIP = (ConnectionSocket.RemoteEndPoint as IPEndPoint).Address;
                int ClientPort = (ConnectionSocket.RemoteEndPoint as IPEndPoint).Port;
                string remotePoint = ConnectionSocket.RemoteEndPoint.ToString();
                richTextBox1.Text += "成功与客户端" + remotePoint + "建立连接\r\n";
                ClientInformation.Add(remotePoint, ConnectionSocket);
                ParameterizedThreadStart pts = new ParameterizedThreadStart(ReceiveMessage);
                Thread thread = new Thread(pts);
                thread.IsBackground = true;//设置后台线程
                thread.Start(ConnectionSocket);
            }
        }
        private void ReceiveMessage(Object SocketClient)
        {
            Socket ReceiveSocket = (Socket)SocketClient;
            while (true)
            {
                try
                {
                    byte[] result = new byte[1024 * 1024];
                    int ReceiveLength = 0;
                    ReceiveLength = ReceiveSocket.Receive(result);
                    if (result[0] == 0)//表示接收聊天消息
                    {
                        try
                        {
                            string str = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                            richTextBox1.Text += "来自受控端聊天信息:" + str + "\r\n";

                        }
                        catch (Exception ex)
                        {
                            richTextBox1.Text += "接收聊天消息失败！\r\n";
                            ReceiveSocket.Shutdown(SocketShutdown.Both);
                            ReceiveSocket.Close();
                            break;
                        }
                    }
                    if (result[0] == 1)//表示接收设备状态信息
                    {
                        try
                        {
                            string str = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                            string Information = null;
                            for(int i=0;i<str.Length;i++)
                            {
                                int num = str[i] - '0';
                                if (i != 0 || i != str.Length) num -= 1;
                                Information += num.ToString();
                            }
                            textBox4.Text += "接收设备参数信息:"+DateTime.Now + ":" + Information + "\r\n";
                            //接受之后随机返回一个数表示调试成功与否,产生1-2的随机数
                            Random ro = new Random();
                            int iResult;
                            int iUp = 7;
                            int iDown = 6;
                            iResult = ro.Next(iDown, iUp);
                            //MessageBox.Show(iResult.ToString());
                            List<string> test = new List<string>(ClientInformation.Keys);
                            for (int i = 0; i < ClientInformation.Count; i++)
                            {//给每个在线客户端发送消息
                                Socket socket = ClientInformation[test[i]];
                                byte [] result1 = new byte[1024];
                                if (iResult == 6) result1[0] = 6;
                                else result1[0] = 7;
                                socket.Send(result1);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("现场数据接收异常！");
                            ReceiveSocket.Shutdown(SocketShutdown.Both);
                            ReceiveSocket.Close();
                            break;
                        }
                    }
                }
                catch(Exception ex)
                {
                    richTextBox1.Text += "受控端已经断开连接！";
                    ReceiveSocket.Shutdown(SocketShutdown.Both);
                    ReceiveSocket.Close(); 
                    break;
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)//发送聊天信息
        {
            try
            {
                richTextBox1.Text += "发送聊天消息:\r\n";
                richTextBox1.Text += textBox3.Text + "\r\n";
                string str = textBox3.Text;
                List<string> test = new List<string>(ClientInformation.Keys);
                for (int i = 0; i < ClientInformation.Count; i++)
                {//给每个在线客户端发送消息
                    Socket socket = ClientInformation[test[i]];
                    byte[] result1 = Encoding.UTF8.GetBytes(str);
                    byte[] result = new byte[result1.Length + 1];
                    result[0] = 0;
                    Buffer.BlockCopy(result1, 0, result, 1, result1.Length);
                    socket.Send(result);
                }
                textBox3.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("消息发送失败!" + ex.ToString());
                ServerSocket.Shutdown(SocketShutdown.Both);
                ServerSocket.Close();
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)//输入消息
        {
            ;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)//收发信息显示
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)//IP
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)//PORT
        {

        }

        private void button3_Click(object sender, EventArgs e)//数据发送
        {
            try
            {
                string str = textBox5.Text;
                bool flag = true;
                for(int i=0;i<str.Length;i++)
                {
                    int num = str[i] - '0';
                    if(num!=0&&num!=1)
                    {
                        flag = false;
                        break;
                    }
                }
                if(!flag||str.Length!=8)
                {
                    MessageBox.Show("请输入8位二进制数！");
                    return;
                }
                string SendData = null;
                for(int i=0;i<str.Length;i++)//数据加密01111110
                {
                    int num = str[i] - '0';
                    if (i != 0 || i != str.Length) num += 1;
                    SendData += num.ToString();
                }
                textBox4.Text += DateTime.Now+":"+textBox5.Text + "\r\n";
                List<string> test = new List<string>(ClientInformation.Keys);
                for (int i = 0; i < ClientInformation.Count; i++)
                {//给每个在线客户端发送消息
                    Socket socket = ClientInformation[test[i]];
                    byte[] result1 = Encoding.UTF8.GetBytes(SendData);
                    byte[] result = new byte[result1.Length + 1];
                    result[0] = 1;
                    Buffer.BlockCopy(result1, 0, result, 1, result1.Length);
                    socket.Send(result);
                }

                textBox5.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据发送失败！"+ex.ToString());
                ServerSocket.Shutdown(SocketShutdown.Both);
                ServerSocket.Close();
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)//数据信息
        {
            ;
        }

        private void button4_Click(object sender, EventArgs e)//返回
        {
            this.Hide();
            Serial_Port sp = new Serial_Port();
            //sp.StartPosition = FormStartPosition.CenterScreen;
            sp.Show();
        }
    }
}
