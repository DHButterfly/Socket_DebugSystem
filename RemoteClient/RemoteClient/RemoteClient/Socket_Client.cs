using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO.Ports;
using System.Drawing.Imaging;
using System.IO;

namespace RemoteClient
{
    public partial class Socket_Client : Form
    {
        public string Parameter = null;
        private bool isOpened = false;
        public string Information = null;
        private object locker = new object();
        private Socket ClientSocket = null;
        public Socket_Client()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Server_Client sc = new Server_Client();
            sc.StartPosition = FormStartPosition.CenterScreen;
            sc.Show();
        }

        private void Socket_Client_Load(object sender, EventArgs e)
        {
            TextBox.CheckForIllegalCrossThreadCalls = false;
            richTextBox1.Multiline = true;     //将Multiline属性设置为true，实现显示多行
            richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = "127.0.0.1";
            comboBox2.Text = IP;
            string PORT = "8888";
            comboBox6.Text = PORT;
            for (int i = 1; i <= 10; i++)//串口
            {
                comboBox1.Items.Add("COM" + i.ToString());
            }
            for (int i = 4800; i <= 115200; i *= 2)//波特率
            {
                comboBox5.Items.Add(i.ToString());
            }
            comboBox1.Text = "COM1";//串口号
            comboBox5.Text = "4800";//波特率
            comboBox4.Text = "8";//数据位
            comboBox3.Text = "1";//停止位
            //手动添加串口接收事件
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
        }
        private void button7_Click(object sender, EventArgs e)
        {
            if (!isOpened)
            {
                serialPort1.PortName = comboBox1.Text;
                serialPort1.BaudRate = Convert.ToInt32(comboBox5.Text, 10);
                serialPort1.DataBits = Convert.ToInt32(comboBox4.Text, 10);
                //serialPort1.StopBits = StopBits.One;//使用1位停止位
                try
                {
                    serialPort1.Open();
                    MessageBox.Show("串口打开成功！");
                    button7.Text = "关闭串口";
                    comboBox1.Enabled = false;//关闭使能
                    comboBox4.Enabled = false;
                    comboBox5.Enabled = false;
                    comboBox3.Enabled = false;
                    isOpened = true;
                }
                catch
                {
                    MessageBox.Show("串口打开失败！");
                }
            }
            else
            {
                try
                {
                    serialPort1.Close();
                    button7.Text = "打开串口";
                    comboBox1.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = true;
                    comboBox3.Enabled = true;
                    isOpened = false;
                }
                catch
                {
                    MessageBox.Show("串口关闭失败！");
                }
            }
        }
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)//串口接收
        {
            if (radioButton1.Checked)//接受字符模式ASCII
            {
                string str = serialPort1.ReadExisting();//字符串方式读
                int num = str[0] - '0';
                if(num==6)
                {
                    MessageBox.Show("设备调试成功！");
                    return;
                }
                if(num==7)
                {
                    MessageBox.Show("请继续调试！！！");
                    return;
                }
                richTextBox2.Text += "ASCII接收:";
                if (checkBox1.Checked) richTextBox2.Text += DateTime.Now + ":";//显示时间
                richTextBox2.Text += str;
                if (checkBox3.Checked) richTextBox2.Text += "\r\n";
                Information = str;
                Server_Client sc = new Server_Client();
                sc.Information = Information;
            }
            if (radioButton2.Checked)//HEX接收，只接收两位HEX，接收后转为8位二进制方式
            {
                byte data;
                richTextBox2.Text += "HEX接收：";
                data = (byte)serialPort1.ReadByte();//int转换为byte
                string str = Convert.ToString(data, 16).ToUpper();
                if (checkBox1.Checked) richTextBox2.Text += DateTime.Now + ":";//显示时间
                richTextBox2.Text += "0x" + (str.Length == 1 ? "0" + str : str + " ");
                if (checkBox3.Checked) richTextBox2.Text += "\r\n";
                //16进制转8位2进制，不够补0
                string str2 = null;
                int[] a = new int[10];
                for (int i = 0; i <= str.Length - 1; i++)
                {
                    int cnt = 0;
                    int num = str[i] - '0';
                    if (num >= 10) num -= 7;
                    while (num != 0)
                    {
                        a[++cnt] = num % 2;
                        num /= 2;
                    }
                    for (int j = 1; j <= (4 - cnt); j++) str2 += "0";
                    for (int j = cnt; j >= 1; j--) str2 += a[j].ToString();
                }
                Information = str2;
                Server_Client sc = new Server_Client();
                sc.Information = Information;
            }
        }
        private void button2_Click(object sender, EventArgs e)//设备参数状态显示
        {//0为灯暗图片不变，1为灯亮图片
            Status sta = new Status();
            Color Red = Color.FromArgb(255,255, 0, 0);
            Color Black = Color.FromArgb(255, 0, 0, 0);
            MessageBox.Show(Information);
            if(Information!=null)
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
            sta.StartPosition= FormStartPosition.CenterScreen;
            sta.Show();
        }
        private void button5_Click(object sender, EventArgs e)//串口调试发送
        {
            byte[] Data = new byte[1];
            if (serialPort1.IsOpen)
            {
                string str = textBox3.Text;
                if (str != "")
                {
                    if (radioButton3.Checked)//ASCII
                    {
                        try
                        {
                            bool flag = true;
                            for (int i = 0; i < str.Length; i++)
                            {
                                int num = str[i] - '0';
                                if (num != 0 && num != 1)
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (!flag || str.Length != 8)
                            {
                                MessageBox.Show("请输入8位二进制数！");
                                return;
                            }
                            serialPort1.Write(str);
                            richTextBox2.Text += "发送成功:" + str + "\r\n";
                            textBox3.Clear();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("串口数据写入错误!");
                        }
                    }
                    if (radioButton4.Checked)//HEX
                    {
                        if (str.Length != 2)
                        {
                            MessageBox.Show("请输入两位16进制数！");
                            return;
                        }
                        for (int i = 0; i < (textBox3.Text.Length - textBox3.Text.Length % 2) / 2; i++)
                        {
                            Data[0] = Convert.ToByte(textBox3.Text.Substring(i * 2, 2), 16);
                            serialPort1.Write(Data, 0, 1);
                            //循环发送（如果输入字符位0A0BB，则只发送0A,0B）
                        }
                        if (textBox3.Text.Length % 2 != 0)
                        {
                            Data[0] = Convert.ToByte(textBox3.Text.Substring(textBox3.Text.Length - 1, 1), 16);
                            serialPort1.Write(Data, 0, 1);
                        }
                        textBox3.Clear();
                        richTextBox2.Text += "发送成功:" + str + "\r\n";
                    }
                }
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)//串口调试数据发送
        {
            ;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)//网口调试数据发送
        {
            ;
        }

        private void button6_Click(object sender, EventArgs e)//网口调试发送，应为8位二进制
        {
            try
            {
                string str = textBox4.Text;
                bool flag = true;
                for (int i = 0; i<str.Length; i++)
                {
                    int num = str[i] - '0';
                    if (num != 0 && num != 1)
                    {
                        flag = false;
                        break;
                    }
                }
                if (!flag || str.Length!= 8)
                {
                    MessageBox.Show("请输入8位二进制数！");
                    return;
                }
                //if (checkBox1.Checked) richTextBox1.Text += DateTime.Now;//显示时间
                //if (checkBox3.Checked) richTextBox1.Text += "\r\n";//自动换行
                //if (checkBox2.Checked) richTextBox1.Text += "发送数据:"+textBox4.Text;//显示发送
                richTextBox1.Text+=DateTime.Now+":数据发送"+textBox4.Text+"\r\n";
                string Information = null;
                for(int i=0;i<str.Length;i++)
                {
                    int num = str[i] - '0';
                    if (i != 0 || i != str.Length) num += 1;
                    Information += num.ToString();
                }
                byte[] result1 = Encoding.UTF8.GetBytes(Information);
                byte[] result = new byte[result1.Length + 1];
                result[0] = 1;
                Buffer.BlockCopy(result1, 0, result, 1, result1.Length);
                ClientSocket.Send(result);
                textBox4.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据发送失败！"+ex.ToString());
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)//网口调试
        {
            ;
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)//串口调试
        {
            ;
        }
        private void button3_Click(object sender, EventArgs e)//现场端连接
        {
            int PORT = Convert.ToInt32(comboBox6.Text);
            IPAddress IP = IPAddress.Parse((string)comboBox2.Text);
            try
            {
                ClientSocket.Connect(new IPEndPoint(IP, PORT));
                richTextBox1.Text += "现场端网口连接成功！\r\n";
                Thread thread = new Thread(ReceiveMessage);
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                richTextBox1.Text += "连接现场端网口失败！\r\n";
                return;
            }

        }
        private void ReceiveMessage()
        {
            while(true)
            {
                byte[] result = new byte[1024 * 1024];
                int ReceiveLength = 0;
                ReceiveLength = ClientSocket.Receive(result);
                if (result[0]==0)//表示接收聊天消息
                {
                    try
                    {
                        richTextBox3.Text += "接收现场端聊天消息：";
                        string str = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                        richTextBox3.Text += str;
                        if (checkBox1.Checked) richTextBox3.Text += DateTime.Now;//显示时间
                        if (checkBox3.Checked) richTextBox1.Text += "\r\n";//自动换行

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("接收消息失败！"+ex.ToString());
                        ClientSocket.Shutdown(SocketShutdown.Both);
                        ClientSocket.Close();
                        break;
                    }
                }
                if (result[0] == 6)//调试成功
                {
                    MessageBox.Show("设备参数调试成功！");
                }
                if(result[0]==7)
                {
                    MessageBox.Show("请继续调试！！！");
                }
                if(result[0]==1)//表示接收设备状态信息
                {
                    try
                    {
                        //richTextBox1.Text += "接收现场设备数据信息:";
                        string str = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                        for(int i=0;i<str.Length;i++)//解密处理
                        {
                            int num = str[i] - '0';
                            if (i != 0 || i != str.Length) num -= 1;
                            Information += num.ToString();
                        }
                        Server_Client sc = new Server_Client();
                        sc.Information = Information;
                        //if (checkBox1.Checked) textBox5.Text += DateTime.Now;//显示时间
                        textBox5.Text += DateTime.Now + ":" + Information + "\r\n";
                        //if (checkBox3.Checked) textBox5.Text += "\r\n";//自动换行
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("现场数据接收异常！"+ex.ToString());
                        ClientSocket.Close();
                        break;
                    }
                }
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)//显示现场数据信息
        {
            ;
        }
        //网口信息待录入
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)//IP
        {
            ;
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)//PORT
        {
            ;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)//串口
        {
            ;
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)//波特率
        {
            ;
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)//数据位
        {
            ;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            ;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)//停止位
        {
            ;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)//ASCII字符
        {
            ;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)//HEX
        {
            ;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)//显示时间
        {
            ;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)//显示发送
        {
            ;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)//自动换行
        {
            ;
        }

        private void richTextBox3_TextChanged(object sender, EventArgs e)//网口聊天内容显示
        {
            ;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)//网口发送
        {
            ;
        }

        private void button4_Click(object sender, EventArgs e)//网口发送
        {
            try
            {
                richTextBox3.Text += "向现场端发送聊天消息:\r\n";
                richTextBox3.Text += textBox1.Text + "\r\n";
                string str = textBox1.Text;
                string Client = ClientSocket.RemoteEndPoint.ToString();
                string SendMessage = Client + DateTime.Now + "\r\n" + str + "\r\n";
                byte[] result1 = Encoding.UTF8.GetBytes(SendMessage);
                byte[] result = new byte[result1.Length + 1];
                result[0] = 0;
                Buffer.BlockCopy(result1, 0, result, 1, result1.Length);
                ClientSocket.Send(result);
                textBox1.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("消息发送失败！");
                ClientSocket.Close();
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            ;
        }

        private void label3_Click(object sender, EventArgs e)
        {
            ;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)//发送ASCII
        {

        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)//发送HEX
        {

        }
    }
}
