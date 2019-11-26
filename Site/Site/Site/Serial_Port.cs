using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Site
{
    public partial class Serial_Port : Form
    {
        private bool isOpened = false;
        public Serial_Port()
        {
            InitializeComponent();
        }
        private void Serial_Port_Load(object sender, EventArgs e)
        {
            TextBox.CheckForIllegalCrossThreadCalls = false;
            richTextBox1.Multiline = true;     //将Multiline属性设置为true，实现显示多行
            richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
            string Way1 = "Virtual";
            comboBox1.Items.Add(Way1);
            string Way2 = "TCP/Server";
            comboBox1.Items.Add(Way2);
            comboBox1.Text = Way1;
            for (int i = 1; i <= 10; i++)//串口
            {
                comboBox2.Items.Add("COM" + i.ToString());
            }
            for (int i = 4800; i <= 115200; i *= 2)//波特率
            {
                comboBox3.Items.Add(i.ToString());
            }
            comboBox2.Text = "COM1";//串口号
            comboBox3.Text = "4800";//波特率
            comboBox4.Text = "8";//数据位 
            comboBox5.Text = "1";//停止位
            //手动添加串口接收事件
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(!isOpened)
            {
                serialPort1.PortName = comboBox2.Text;
                serialPort1.BaudRate = Convert.ToInt32(comboBox3.Text, 10);
                serialPort1.DataBits = Convert.ToInt32(comboBox4.Text, 10);
                //serialPort1.StopBits = StopBits.One;//使用1位停止位
                try
                {
                    serialPort1.Open();
                    MessageBox.Show("串口打开成功！");
                    button2.Text = "关闭串口";
                    comboBox2.Enabled = false;//关闭使能
                    comboBox3.Enabled = false;
                    comboBox4.Enabled = false;
                    comboBox5.Enabled = false;
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
                    button2.Text = "打开串口";
                    comboBox2.Enabled = true;
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = true;
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
            if (radioButton3.Checked)//ASCII
            {
                string str = serialPort1.ReadExisting();//字符串方式读
                richTextBox1.Text += "ASCII接收:";
                if (checkBox1.Checked) richTextBox1.Text += DateTime.Now + ":";//显示时间
                richTextBox1.Text += str;
                if (checkBox3.Checked) richTextBox1.Text += "\r\n";
                string SendState = null;
                Random ro = new Random();
                int iResult;
                int iUp = 7;
                int iDown = 6;
                iResult = ro.Next(iDown, iUp);
                if (iResult == 6) SendState = iDown.ToString();
                if (iResult == 7) SendState = iUp.ToString();
                serialPort1.Write(SendState);
            }
            if (radioButton2.Checked)//数值接收ASCII
            {
                byte data;
                richTextBox1.Text += "HEX接收:";
                data = (byte)serialPort1.ReadByte();//int转换为byte
                string str = Convert.ToString(data, 16).ToUpper();
                if (checkBox1.Checked) richTextBox1.Text += DateTime.Now + ":";//显示时间
                richTextBox1.Text += "0x" + (str.Length == 1 ? "0" + str : str + " ");
                if (checkBox3.Checked) richTextBox1.Text += "\r\n";
            }
        }
        private void button1_Click(object sender, EventArgs e)//串口发送
        {
            byte[] Data = new byte[1];
            if (serialPort1.IsOpen)
            {
                string str = textBox1.Text;
                if (str != "")
                {
                    if (radioButton1.Checked)//ASCII
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
                            richTextBox1.Text += "发送成功:" + str + "\r\n";
                            textBox1.Clear();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("串口数据写入错误!");
                        }
                    }
                    if (radioButton4.Checked)//HEX
                    {
                        if(str.Length!=2)
                        {
                            MessageBox.Show("请输入两位16进制数！");
                            return;
                        }
                        for (int i = 0; i < (textBox1.Text.Length - textBox1.Text.Length % 2) / 2; i++)
                        {
                            Data[0] = Convert.ToByte(textBox1.Text.Substring(i * 2, 2), 16);
                            serialPort1.Write(Data, 0, 1);
                            //循环发送（如果输入字符位0A0BB，则只发送0A,0B）
                        }
                        if (textBox1.Text.Length % 2 != 0)
                        {
                            Data[0] = Convert.ToByte(textBox1.Text.Substring(textBox1.Text.Length - 1, 1), 16);
                            serialPort1.Write(Data, 0, 1);
                        }
                        textBox1.Clear();
                        richTextBox1.Text += "发送成功:" + str + "\r\n";
                    }
                }
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)//发送方式选择
        {
            if (comboBox1.Text == "TCP/Server")
            {
                this.Hide();
                Site site = new Site();
                site.StartPosition = FormStartPosition.CenterScreen;
                site.Show();
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)//串口号
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)//波特率
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)//数据位
        {

        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)//停止位
        {

        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)//接收ASCII
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)//接收HEX
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)//接收显示时间
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)//发送显示
        {

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)//接收换行
        {

        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)//发送HEX
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)//发送ASCII
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)//发送消息
        {
            ;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)//信息收发
        {
            ;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)//显示发送
        {
            ;
        }
    }
}
