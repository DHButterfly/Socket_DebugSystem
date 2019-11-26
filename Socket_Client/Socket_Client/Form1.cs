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
namespace Socket_Client
{
    public partial class Form1 : Form
    {
        private Socket ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Form1()
        {
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;
            richTextBox1.Multiline = true;     //将Multiline属性设置为true，实现显示多行
            richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)//显示是否连接成功
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)//IP
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)//PORT
        {

        }

        private void button1_Click(object sender, EventArgs e)//连接服务器
        {
            int Port = Convert.ToInt32(textBox2.Text);
            IPAddress IP = IPAddress.Parse((string)textBox1.Text);
            try
            {
                ClientSocket.Connect(new IPEndPoint(IP, Port));
                richTextBox1.Text += "连接服务器成功！\r\n";
                Thread thread = new Thread(ReceiveMessage);
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                richTextBox1.Text += "连接服务器失败！\r\n";
                return;
            }
        }
        private void ReceiveMessage()
        {
            long TotalLength = 0;
            while (true)
            {
                byte[] result = new byte[1024 * 1024];
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
                            richTextBox1.Text += "接收消息失败！\r\n";
                            ClientSocket.Shutdown(SocketShutdown.Both);
                            ClientSocket.Close();
                            break;
                        }
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("接收异常！");
                    break;
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)//发送消息
        {
            try
            {
                richTextBox1.Text += "向服务器发送消息:\r\n";
                richTextBox1.Text += textBox3.Text + "\r\n";
                string str = textBox3.Text;
                string Client = ClientSocket.RemoteEndPoint.ToString();
                string SendMessage = "接收客户端" + Client + "消息：" + DateTime.Now + "\r\n" + str + "\r\n";
                byte[] result1 = Encoding.UTF8.GetBytes(SendMessage);
                byte[] result = new byte[result1.Length + 1];
                result[0] = 0;
                Buffer.BlockCopy(result1, 0, result, 1, result1.Length);
                ClientSocket.Send(result);
                textBox3.Clear();
            }
            catch (Exception ex)
            {
                richTextBox1.Text += "发送消息失败，服务器可能已经关闭！\r\n";
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)//发送消息
        {

        }
    }
}
