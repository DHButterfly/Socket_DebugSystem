using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
namespace Serial_port_Form
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;//防止后面出现异常，会提示线程间操作无效: 从不是创建控件的线程访问它
        }
        private void Form1_Load(object sender, EventArgs e)//完成窗口初始化
        {
            for(int i=1;i<=10;i++)
            {
                comboBox1.Items.Add("COM" + i.ToString());            
            }
            for(int i=4800;i<=115200;i*=2)
            {
                comboBox2.Items.Add(i.ToString());
            }
            comboBox1.Text = "COM1";//串口号
            comboBox2.Text = "4800";//波特率
            //手动添加串口接收事件
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
        }
        private void port_DataReceived(object sender,SerialDataReceivedEventArgs e)
        {
            if(!radioButton4.Checked)//接受字符模式
            {
                string str = serialPort1.ReadExisting();//字符串方式读
                richTextBox2.Text += str;
            }
            else//数值接收
            {
                byte data;
                data = (byte)serialPort1.ReadByte();//int转换为byte
                string str = Convert.ToString(data, 16).ToUpper();
                richTextBox2.Text += "0x" + (str.Length == 1 ? "0" + str : str + " ");
            }
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)//串口号上
        {

        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)//波特率
        {

        }
        private void button1_Click(object sender, EventArgs e)//打开串口
        {
            try
            {
                serialPort1.PortName = comboBox1.Text;
                serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);//十进制数据转换
                serialPort1.Open();
                button1.Enabled = false;//打开串口条
                button2.Enabled = true;//关闭串口条
            }
            catch
            {
                MessageBox.Show("串口错误，请检查串口设置！");
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)//显示发送内容
        {
            richTextBox1.ScrollToCaret();
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)//显示接收内容
        {
            richTextBox2.ScrollToCaret();
        }
        private void button2_Click(object sender, EventArgs e)//关闭串口
        {
            try
            {
                serialPort1.Close();
                button1.Enabled = true;//打开串口条
                button2.Enabled = false;//关闭串口条
            }
            catch(Exception ex)//关闭串口一般不会出现异常
            {
                
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)//十六进制
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)//字符
        {

        }

        private void button3_Click(object sender, EventArgs e)//发送
        {
            byte[] Data = new byte[1];
            if(serialPort1.IsOpen)
            {
                if(textBox1.Text!="")
                {
                    if(!radioButton2.Checked)
                    {
                        try
                        {
                            serialPort1.WriteLine(textBox1.Text);
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show("串口数据写入错误!");
                        }
                    }
                    else
                    {
                        for (int i = 0; i < (textBox1.Text.Length - textBox1.Text.Length % 2)/ 2; i++)
                        {
                            Data[0] = Convert.ToByte(textBox1.Text.Substring(i * 2, 2), 16);
                            serialPort1.Write(Data,0,1);
                        }
                        if(textBox1.Text.Length%2!=0)
                        {
                            Data[0] = Convert.ToByte(textBox1.Text.Substring(textBox1.Text.Length - 1,1),16);
                            serialPort1.Write(Data, 0, 1);
                        }
                    }
                }
            }
           // serialPort1.Close();
           // button1.Enabled = false;//打开串口条
            //button2.Enabled = true;//关闭串口条
        }

        private void button4_Click(object sender, EventArgs e)//接收
        {

        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)//十六进制x
        {

        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)//字符x
        {

        }
        private void textBox1_TextChanged(object sender, EventArgs e)//发送内容
        {
            textBox1.ScrollToCaret();
        }
        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
