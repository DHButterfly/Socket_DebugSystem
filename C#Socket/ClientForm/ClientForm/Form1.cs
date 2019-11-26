using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
namespace ClientForm//客户端
{
    public partial class Form1 : Form
    {
        private object locker = new object();
        private Socket ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Form1()
        {
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;
            richTextBox1.Multiline = true;     //将Multiline属性设置为true，实现显示多行
            richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
            richTextBox2.Multiline = true;     //将Multiline属性设置为true，实现显示多行
            richTextBox2.ScrollBars = RichTextBoxScrollBars.Vertical;　//设
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ;
        }
        private void textBox1_TextChanged(object sender, EventArgs e)//输入消息
        {
            string str = Console.ReadLine();
            textBox1.Text += str;
        }
        private void textBox2_TextChanged(object sender, EventArgs e)//ip
        {
            ;
        }
        private void textBox3_TextChanged(object sender, EventArgs e)//port
        {
            ;
        }
        private void button4_Click(object sender, EventArgs e)//发送文件
        {
            if (string.IsNullOrEmpty(textBox5.Text))
            {
                MessageBox.Show("选择要发送的文件！！！");
            }
            else
            {
                //采用文件流方式打开文件
                using (FileStream fs = new FileStream(textBox5.Text, FileMode.Open))
                {
                    long TotalLength = fs.Length;//文件总长度
                    Form2 a = new Form2();
                    a.progressBar1.Value = 0;//设置进度条的当前位置为0
                    a.progressBar1.Minimum = 0; //设置进度条的最小值为0
                    a.progressBar1.Maximum = Convert.ToInt32(TotalLength);//设置进度条的最大值
                    richTextBox1.Text += "文件总长度为：" + TotalLength.ToString()+"\r\n";
                    string fileName = Path.GetFileName(textBox5.Text);//文件名
                    string fileExtension = Path.GetExtension(textBox5.Text);//扩展名
                    string str = string.Format("{0}-{1}", fileName, TotalLength);
                    byte[] arr = Encoding.UTF8.GetBytes(str);
                    byte[] arrSend = new byte[arr.Length + 1];
                    Buffer.BlockCopy(arr, 0, arrSend, 1, arr.Length);
                    arrSend[0] = 2;//发送文件信息
                    ClientSocket.Send(arrSend);
                    byte[] arrFile = new byte[1024 * 1024 * 10];
                    int FileLength=0;//将要发送的文件读到缓冲区
                    long SendFileLength = 0;
                    bool flag = true;
                    long num1 = 0;
                    while(TotalLength> SendFileLength&& (FileLength = fs.Read(arrFile, 0, arrFile.Length))>0)
                    {
                        SendFileLength+=FileLength;
                        if (flag)
                        {
                            byte[] arrFileSend = new byte[FileLength + 1];
                            arrFileSend[0] = 1;
                            Buffer.BlockCopy(arrFile, 0, arrFileSend, 1, FileLength);
                            ClientSocket.Send(arrFileSend,0,FileLength+1, SocketFlags.None);
                            flag = false; 
                        }
                        else
                        {
                            ClientSocket.Send(arrFile, 0, FileLength, SocketFlags.None);
                        }
                        a.progressBar1.Value += FileLength;
                        num1 += FileLength*100;
                        long num2 = TotalLength;
                        long num = num1 / num2;
                        a.label1.Text = (num1 / num2).ToString() + "%";
                        a.Show();
                        Application.DoEvents();//重点，必须加上，否则父子窗体都假死
                    }
                    try
                    {
                        if (a.progressBar1.Value == TotalLength)
                        {
                            MessageBox.Show("文件发送完毕！！！");
                            a.Close();
                            textBox5.Clear();
                            fs.Close();
                        }
                    }
                    catch
                    {
                        MessageBox.Show("文件发送失败！");
                        a.Close();
                        textBox5.Clear();
                        fs.Close();
                    }
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)//选择文件
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = "D:\\";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox5.Text += ofd.FileName;
            }
        }
        private void textBox5_TextChanged(object sender, EventArgs e)//选择文件
        {
            ;
        }
        private void button1_Click(object sender, EventArgs e)//发送消息
        {
            try
            {
                //Thread.Sleep(2000);
                if(string.IsNullOrEmpty(textBox1.Text))//判断输入框是否为空
                {
                    MessageBox.Show("请输入要发送的消息！");
                    return;
                }
                richTextBox1.Text += "向服务器发送消息:\r\n";
                richTextBox1.Text += textBox1.Text + "\r\n";
                string sb = textBox1.Text;
                string Client = ClientSocket.RemoteEndPoint.ToString();
                string SendMessage = "接收服务器" + Client + "转发消息：" + DateTime.Now + "\r\n" + sb + "\r\n";
                byte[] result1 = Encoding.UTF8.GetBytes(SendMessage);
                byte[] result = new byte[result1.Length + 1];
                result[0] = 0;
                Buffer.BlockCopy(result1, 0, result, 1, result1.Length);
                ClientSocket.Send(result);
                textBox1.Clear();
            }
            catch (Exception ex)
            {
                richTextBox1.Text += "发送消息失败，服务器可能已经关闭！\r\n";
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
            }
        }
        private void button2_Click(object sender, EventArgs e)//连接服务器
        {
            int Port = Convert.ToInt32(textBox3.Text);
            IPAddress IP = IPAddress.Parse((string)textBox2.Text);
            try
            {
                ClientSocket.Connect(new IPEndPoint(IP, Port));
                richTextBox2.Text += "连接服务器成功！\r\n";
                Thread thread = new Thread(ReceiveMessage);
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                richTextBox2.Text += "连接服务器失败！\r\n";
                return;
            }
        }
        private void label1_Click(object sender, EventArgs e)
        {
            ;
        }
        private void label2_Click(object sender, EventArgs e)
        {
            ;
        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)//显示发送的消息
        {
            ;
        }
        private void richTextBox2_TextChanged(object sender, EventArgs e)//显示接收的消息
        {
            ;
        }
        private void ReceiveMessage()
        {
            long TotalLength = 0;
            while (true)
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
                            string str = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                            richTextBox2.Text += "接收服务器消息：\r\n";
                            richTextBox2.Text += str + "\r\n";
                        }
                        catch (Exception ex)
                        {
                            richTextBox2.Text += "接收消息失败！\r\n";
                            ClientSocket.Shutdown(SocketShutdown.Both);
                            ClientSocket.Close();
                            break;
                        }
                    }
                    if (result[0] == 2)
                    {
                        string fileNameWithLength = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                        string str1 = fileNameWithLength.Split('-').First();
                        TotalLength = Convert.ToInt64(fileNameWithLength.Split('-').Last());
                        richTextBox2.Text += "接收服务器后缀名为：" + str1 + "的文件" + "\r\n";
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
                                    richTextBox2.Text += "文件保存成功：" + " " + fileSavePath + "\r\n";
                                    fs.Close();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            richTextBox2.Text += "文件保存失败！\r\n";
                            MessageBox.Show(ex.Message);
                            ClientSocket.Shutdown(SocketShutdown.Both);
                            ClientSocket.Close();
                            break;
                        }
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("系统异常！");
                    break;
                }
            }

        }

        private void button5_Click(object sender, EventArgs e)//保存文件入数据库，将文件转成二进制流读入数据库
        {
            if (textBox5.Text == null) MessageBox.Show("请先选择文件！");
            FileStream fs = new FileStream(textBox5.Text, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            Byte[] byData = br.ReadBytes(Convert.ToInt32(fs.Length));
            fs.Close();
            string constr = "Server=127.0.0.1;Initial Catalog=root;Database=testdb;User ID=root;Password=5120154230;SslMode=none";
            MySqlConnection myconn = new MySqlConnection(constr);
            myconn.Open();
            string str = "insert into pro_table (pro_path) values('"+textBox5.Text+"')";
            string check = "select * from pro_table where pro_path='" + textBox5.Text + "'";
            MySqlDataAdapter da = new MySqlDataAdapter(check, myconn); //创建适配器
            DataSet ds = new DataSet(); //创建数据集
            da.Fill(ds, "pro_table"); //填充数据集
            if (da.Fill(ds, "pro_table") > 0) //判断同名
            {
                MessageBox.Show("该文件路径已存在！");
                return;
            }
            MySqlCommand cmd = new MySqlCommand(str, myconn); //创建执行插入文件路径
            cmd.ExecuteNonQuery();//执行插入语句
            MessageBox.Show("文件路径保存成功！");
            myconn.Close();//关闭连接
        }
    }
}