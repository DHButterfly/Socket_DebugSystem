using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Threading;
using System.Drawing.Imaging;

namespace RemoteServer
{
    public partial class Socket_Server : Form
    {
        MemoryStream ms;
        public string old_data = null;
        public string Information = null;
        private bool flagg = false;
        private string source = null;//源IP地址
        private string destination= null;//目的IP地址
        private string mess = null;//消息
        string fo = null;//视频格式
        private Dictionary<Int32, ClientInfo> _DicClient;
        private Socket ServerSocket;
        private Dictionary<string, Socket> ClientInformation = new Dictionary<string, Socket>();//一个字典
        public Socket_Server()
        {
            InitializeComponent();
            Form.CheckForIllegalCrossThreadCalls = false;//略程序跨越线程运行导致的错误
            TextBox.CheckForIllegalCrossThreadCalls = false;
            richTextBox1.Multiline = true;     //将Multiline属性设置为true，实现显示多行
            richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
            richTextBox2.Multiline = true;     //将Multiline属性设置为true，实现显示多行
            richTextBox2.ScrollBars = RichTextBoxScrollBars.Vertical;
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _DicClient = new Dictionary<Int32, ClientInfo>(100);//开100容量的字典，   键-值
            textBox1.Text = "127.0.0.1";
            textBox2.Text = "9999";
        }
        //把二进制转成image类型，显示出来，
        public Image ByteArrayToImage(byte[] byteArrayIn, int count)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn, 1, count-1);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }
        private void Socket_Server_Load(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)//显示发送消息
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length; //设定光标位置
            richTextBox1.ScrollToCaret(); //滚动到光标处
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)//显示调试状态消息
        {
            richTextBox2.SelectionStart = richTextBox2.Text.Length; //设定光标位置
            richTextBox2.ScrollToCaret(); //滚动到光标处
        }

        private void IP_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)//IP
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)//PORT
        {
            
        }
        private void Socket_Server_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(0);//当窗体关闭之后，强制关闭窗体所有运行线程
        }
        private void textBox3_TextChanged(object sender, EventArgs e)//选择文件
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)//发送消息
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)//在线列表
        {

        }
        private void button5_Click(object sender, EventArgs e)//历史记录
        {
            this.DialogResult = DialogResult.OK;
            this.Hide();
            Log_Query lq = new Log_Query();
            lq.StartPosition = FormStartPosition.CenterScreen;
            lq.Show();
        }

        private void button1_Click(object sender, EventArgs e)//启动服务
        {
            try
            {
                int Port = Convert.ToInt32(textBox2.Text);
                IPAddress IP = IPAddress.Parse((string)textBox1.Text);
                if(textBox1.Text==""||textBox2.Text=="")
                {
                    MessageBox.Show("请先完成服务器配置！");
                    return;
                }
                bool flag = false;
                if(!flag)
                {
                    flag = true;
                    ServerSocket.Bind(new IPEndPoint(IP, Port));
                    ServerSocket.Listen(20);
                    richTextBox2.Text += "启动监听成功！\r\n";
                    richTextBox2.Text += "监听本地" + ServerSocket.LocalEndPoint.ToString() + "成功\r\n";
                    ///测试显示
                    /*Screen_Show screen_Show = new Screen_Show();
                    screen_Show.pictureBox1.Image = Image.FromFile("c:\\Capture1111.jpg", false);
                    screen_Show.StartPosition = FormStartPosition.Manual;
                    screen_Show.Show();*/
                    //监听线程
                    Thread ThreadListen = new Thread(ListenConnection);
                    ThreadListen.IsBackground = true;
                    ThreadListen.Start();
                    //扫描线程
                    /*Thread thread = new Thread(Scanning);
                    thread.IsBackground = true;
                    thread.Start();*/
                }
                else
                {
                    MessageBox.Show("正在监听中...");
                }
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
            while (this.Visible)
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
                //string remotePoint = ConnectionSocket.RemoteEndPoint.ToString();
                string remotePoint = ClientIP.ToString();

                //Console.WriteLine ("remotePoint={0}", remotePoint);

                richTextBox2.Text += "成功与客户端" + remotePoint + "建立连接\r\n";
                listBox1.Items.Add(remotePoint);
                ClientInformation.Add(remotePoint, ConnectionSocket);
                ParameterizedThreadStart pts = new ParameterizedThreadStart(ReceiveMessage);
                Thread thread = new Thread(pts);
                thread.IsBackground = true;//设置后台线程
                thread.Start(ConnectionSocket);
                //Thread.Sleep(1000);
            }
        }
        private void ReceiveMessage(Object SocketClient)///接收消息
        {
            Socket ReceiveSocket = (Socket)SocketClient;
            long FileLength = 0;
            long PictureLength = 0;
            byte[] buffer = new byte[1024];
            //Screen_Show screen_Show = new Screen_Show();
            while (this.Visible)
            {
                string constr = "Server=127.0.0.1;Database=tonghua;User Id=root;Password=5120154230;";
                MySqlConnection mycon = new MySqlConnection(constr);
                mycon.Open();//打开连接
                int ReceiveLength = 0;
                byte[] result = new byte[1024 * 1024 * 10];
                try
                {
                    /*将IPAdress转换为int*/
                    //string addr = "11.22.33.44";
                    //System.Net.IPAddress IPAddr = System.Net.IPAddress.Parse(addr);
                    ////网上的代码是得到字节组再转换成int
                    //byte[] byt = IPAddr.GetAddressBytes();
                    //int intIP = System.BitConverter.ToInt32(byt, 0);
                    //Console.WriteLine("字节转换结果：{0}", intIP);
                    ////其实GetHashCode()方法直接就可以了
                    IPAddress ClientIP = (ReceiveSocket.RemoteEndPoint as IPEndPoint).Address;
                    int ClientPort = (ReceiveSocket.RemoteEndPoint as IPEndPoint).Port;
                    ReceiveLength = ReceiveSocket.Receive(result);
                    string str = ReceiveSocket.RemoteEndPoint.ToString();
                    if (result[0] == 0)//接收消息
                    {
                        string ReceiveMessage = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                        richTextBox1.Text +="接收客户端"+ClientIP.ToString()+"消息:"+DateTime.Now+"\r\n"+ ReceiveMessage+"\r\n";
                        destination = ServerSocket.LocalEndPoint.ToString();
                        source = ClientIP.ToString();
                        mess = ReceiveMessage;
                        fo = "rm";//视频格式
                        string record = "insert into recording (source,destination,datetime,message,format) " +
                            "values('" + source + "','" + destination + "','" + DateTime.Now + "','" + mess + "','" + fo + "')";
                        MySqlCommand cmd = new MySqlCommand(record, mycon); //创建执行
                        cmd.ExecuteNonQuery(); //执行SQL
                        mycon.Close();
                        mycon.Dispose();
                        //if (ClientInformation.Count == 1) continue;//只有一个客户端
                        List<string> test = new List<string>(ClientInformation.Keys);
                       /* for (int i = 0; i < ClientInformation.Count; i++)
                        {//将接收到的消息群发出去，即所谓的群聊了
                            Socket socket = ClientInformation[test[i]];
                            string s = ReceiveSocket.RemoteEndPoint.ToString();
                            if (test[i] != s)
                            {
                                richTextBox1.Text += DateTime.Now + "\r\n" + "客户端" + str + "向客户端" + test[i] + "发送消息：\r\n";
                                byte[] arrMsg = Encoding.UTF8.GetBytes(ReceiveMessage);
                                byte[] SendMsg = new byte[arrMsg.Length + 1];
                                SendMsg[0] = 0;
                                Buffer.BlockCopy(arrMsg, 0, SendMsg, 1, arrMsg.Length);
                                socket.Send(SendMsg);
                            }
                        }*/
                    }
                    if(result[0]==6)//表示收到设备参数数据
                    {
                        Information = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                        old_data = Information;
                        RemoteDebug rd = new RemoteDebug();
                        if (Information != null)
                        {
                            if (Information[0] == '1')
                            {
                                rd.flag11 = true;
                            }

                            if (Information[1] == '1')
                            {
                                rd.flag22 = true;
                            }
                            if (Information[2] == '1')
                            {
                                rd.flag33 = true;
                            }

                            if (Information[3] == '1')
                            {
                                rd.flag44 = true;
                            }

                            if (Information[4] == '1')
                            {
                                rd.flag55 = true;
                            }

                            if (Information[5] == '1')
                            {
                                rd.flag66 = true;
                            }

                            if (Information[6] == '1')
                            {
                                rd.flag77 = true;
                            }

                            if (Information[7] == '1')
                            {
                                rd.flag88 = true;
                            }
                        }
                        richTextBox1.Text += "接收参数信息:" + Information + "\r\n";
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
                            MessageBox.Show("出现异常！");
                            ReceiveSocket.Shutdown(SocketShutdown.Both);
                            ReceiveSocket.Close();
                        }
                    }
                    if(result[0]==5)
                    {
                        string fileNameWithLength = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                        PictureLength=Convert.ToInt64(fileNameWithLength.Split('-').Last());
                        richTextBox1.Text += "picturelength="+PictureLength.ToString() + "\r\n";
                    }
                    if (result[0] == 2)//接收文件信息和长度
                    {
                        string fileNameWithLength = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                        destination = ServerSocket.LocalEndPoint.ToString();
                        source = ClientIP.ToString();
                        mess = fileNameWithLength;
                        fo = "rmbv";//视频格式
                        string record = "insert into recording (source,destination,datetime,message,format) " +
                            "values('" + source + "','" + destination + "','" + DateTime.Now + "','" + mess + "','" + fo + "')";
                        MySqlCommand cmd = new MySqlCommand(record, mycon); //创建执行
                        cmd.ExecuteNonQuery(); //执行SQL
                        mycon.Close();
                        mycon.Dispose();
                        //recStr = fileNameWithLength.Split('-').First();
                        FileLength = Convert.ToInt64(fileNameWithLength.Split('-').Last());
                        richTextBox1.Text += "FileLength=" + FileLength.ToString() + "\r\n";
                    }
                   if (result[0] == 4)//收到图片的二进制信息
                    {
                        long Length = 0;
                        //MemoryStream ms = new MemoryStream();
                        bool flag1 = true;
                        int rec = 0;
                        ms= new MemoryStream();
                        while (Length<PictureLength)
                        {
                            //ReceiveLength = ReceiveSocket.Receive(buffer);
                            if (flag1)
                            {
                                ms.Write(result, 1, ReceiveLength - 1);
                                //ms.Flush();
                                Length += ReceiveLength - 1;
                                flag1 = false;
                            }
                            else
                            {
                                ReceiveLength = ReceiveSocket.Receive(result);
                                ms.Write(result, 0, ReceiveLength);
                                //ms.Flush();
                                Length += ReceiveLength;
                            }
                        }
                        /*Screen_Show screen_Show = new Screen_Show();
                        screen_Show.pictureBox1.Image = Bitmap.FromStream(ms);
                        //screen_Show.Show();
                        //screen_Show.pictureBox1.Refresh();
                        if (!flagg)//第一次打开显示图像窗体
                        {
                            flagg = true;
                            screen_Show.StartPosition = FormStartPosition.Manual;
                            screen_Show.Show();
                            screen_Show.pictureBox1.Refresh();
                        }
                        else
                        {
                            screen_Show.pictureBox1.Refresh();
                            Console.WriteLine("刷新picturebox\r\n");
                        }*/
                    }
                    if (result[0] == 3)//表示收到客户端发送的心跳包，更新客户端最后在线时间
                    {
                        string ReceiveMessage = Encoding.UTF8.GetString(result, 1, ReceiveLength - 1);
                        string str1 = "接收客户端心跳包:"+ReceiveMessage;
                        Console.WriteLine(str1);//将心跳包输出到后台
                        //发送应答包
                        string SendMessage = "#####";
                        List<string> test = new List<string>(ClientInformation.Keys);
                        for (int i = 0; i < ClientInformation.Count; i++)
                        {//给每个在线客户端发送应答消息
                            Socket socket = ClientInformation[test[i]];
                            byte[] arrMsg = Encoding.UTF8.GetBytes(SendMessage);
                            byte[] SendMsg = new byte[arrMsg.Length + 1];
                            SendMsg[0] = 3;
                            Buffer.BlockCopy(arrMsg, 0, SendMsg, 1, arrMsg.Length);
                            socket.Send(SendMsg);
                        }
                        try
                        {
                            string clientID = ClientIP.ToString();
                            Int32 clientID2 = ClientIP.GetHashCode();//将IPAdress转换为int得到的是哈希码
                            lock (_DicClient)
                            {
                                try
                                {
                                    ClientInfo clientInfo;
                                    if (_DicClient.TryGetValue(clientID2, out clientInfo))//客户端已经上线
                                    {
                                        clientInfo.LastHeartbeatTime = DateTime.Now;
                                    }
                                    else
                                    {
                                        clientInfo = new ClientInfo();
                                        clientInfo.ClientID = clientID;
                                        Console.WriteLine("加入集合中的clientInfo.ClientID={0}", clientInfo.ClientID);
                                        clientInfo.LastHeartbeatTime = DateTime.Now;
                                        clientInfo.State = true;
                                        _DicClient.Add(clientID2, clientInfo);//加入集合
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("更新异常！" + ex.ToString());
                                    return;
                                }
                            }
                            /*ParameterizedThreadStart pts1 = new ParameterizedThreadStart(ReceiveHeartbeat);
                            Thread thread = new Thread(pts1);
                            thread.IsBackground = true;//设置后台线程
                            thread.Start(ClientIP);*/
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                            return;
                        }
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
        private void ReceiveHeartbeat(object clientID1)//接收心跳包后需要的更新工作
        {
            string clientID = clientID1.ToString();
            Int32 clientID2 = clientID1.GetHashCode();//将IPAdress转换为int得到的是哈希码
            lock (_DicClient)
            {
                try
                {
                    ClientInfo clientInfo;
                    if (_DicClient.TryGetValue(clientID2, out clientInfo))//客户端已经上线
                    {
                        clientInfo.LastHeartbeatTime = DateTime.Now;
                    }
                    else
                    {
                        clientInfo = new ClientInfo();
                        clientInfo.ClientID = clientID;
                        Console.WriteLine("加入集合中的clientInfo.ClientID={0}", clientInfo.ClientID);
                        clientInfo.LastHeartbeatTime = DateTime.Now;
                        clientInfo.State = true;
                        _DicClient.Add(clientID2, clientInfo);//加入集合
                    }
            }
                catch(Exception ex)
                {
                    MessageBox.Show("更新异常！"+ex.ToString());
                    return;
                }
            }
        }
        private void Scanning()  //扫描离线客户端并从在线列表中移除
        {
            while (this.Visible)
            {
                Thread.Sleep(5000);
                lock(_DicClient)//保证同一时间只有一个进程对集合进行操作
                {
                    List<Int32> test = new List<Int32>(_DicClient.Keys);
                    for (Int32 clientID = _DicClient.Count-1;clientID>=0;clientID--)
                    //foreach (Int32 clientID in _DicClient.Keys)//foreach枚举无法修改集合，需要改成for倒着循环
                    {
                        ClientInfo clientInfo = _DicClient[test[clientID]];//test[clientID]是键,clientInfo是对象
                        string str = clientInfo.ClientID.ToString();
                        if (!clientInfo.State)
                        {
                            Console.WriteLine("移除集合中clientInfo.ClientID={0}", str);
                            listBox1.Items.Remove(str);
                            continue;
                        }
                        TimeSpan sp = DateTime.Now - clientInfo.LastHeartbeatTime;
                        if (sp.Seconds > 10)//判断心跳时间受否大于10秒,客户端两秒发一次，服务端1秒扫一次
                        {
                            listBox1.Items.Remove(clientInfo.ClientID);//移除
                            clientInfo.State = false;//离线
                            _DicClient.Remove(test[clientID]);//移除键
                            Console.WriteLine("移除集合中clientInfo.ClientID={0}", str);
                        }
                    }
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)//发送文件
        {
            if (string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("请选择你要发送的文件！！！");
                return;
            }
            else
            {

                ScheDule a = new ScheDule();
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
                        string constr = "Server=127.0.0.1;Database=tonghua;User Id=root;Password=5120154230;";
                        MySqlConnection mycon = new MySqlConnection(constr);
                        mycon.Open();//打开连接
                        ClientInformation[str1].Send(arrSend);//发送文件名和长度
                        int FileLength = 0;//每次读取长度,分包发送
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
                            long num2 = a.progressBar1.Maximum;
                            long num = num1 / num2;
                            a.label1.Text = (num1 / num2).ToString() + "%";
                            a.Show();
                            Application.DoEvents();//重点，必须加上，否则父子窗体都假死
                        }
                        if (a.progressBar1.Value == TotalLength)
                        {

                            MessageBox.Show("文件发送完毕！！！");
                            IPAddress ClientIP = (ClientInformation[str1].RemoteEndPoint as IPEndPoint).Address;
                            source = ServerSocket.LocalEndPoint.ToString();
                            destination = ClientIP.ToString();
                            mess = textBox3.Text;
                            fo = "mp4";//视频格式
                            string record = "insert into recording (source,destination,datetime,message,format) " +
                                "values('" + source + "','" + destination + "','" + DateTime.Now + "','" + mess + "','" + fo + "')";
                            MySqlCommand cmd = new MySqlCommand(record, mycon); //创建执行
                            cmd.ExecuteNonQuery(); //执行SQL
                            mycon.Close();
                            mycon.Dispose();
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

        private void button3_Click(object sender, EventArgs e)//选择文件
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = "D:\\";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text += ofd.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)//发送消息
        {
            string SendMessage = textBox4.Text;
            List<string> test = new List<string>(ClientInformation.Keys);
            for (int i = 0; i < ClientInformation.Count; i++)
            {//给每个在线客户端发送消息

               try
                {
                    string constr = "Server=127.0.0.1;Database=tonghua;User Id=root;Password=5120154230;";
                    MySqlConnection mycon = new MySqlConnection(constr);
                    mycon.Open();//打开连接

                    Socket socket = ClientInformation[test[i]];
                    IPAddress ClientIP = (socket.RemoteEndPoint as IPEndPoint).Address;
                    richTextBox1.Text += "向客户端"+ ClientIP.ToString() + "发送消息：\r\n" + SendMessage + "\r\n";
                    byte[] arrMsg = Encoding.UTF8.GetBytes(SendMessage);
                    byte[] SendMsg = new byte[arrMsg.Length + 1];
                    SendMsg[0] = 0;
                    Buffer.BlockCopy(arrMsg, 0, SendMsg, 1, arrMsg.Length);
                    socket.Send(SendMsg);
                    source = ServerSocket.LocalEndPoint.ToString();
                    destination = ClientIP.ToString();
                    mess = textBox4.Text.ToString();
                    fo = "rm";//视频格式
                    string record = "insert into recording (source,destination,datetime,message,format) " +
                        "values('" + source + "','" + destination + "','" + DateTime.Now + "','" + mess + "','" + fo + "')";
                    MySqlCommand cmd = new MySqlCommand(record, mycon); //创建执行
                    cmd.ExecuteNonQuery(); //执行SQL
                    mycon.Close();
                    mycon.Dispose();
                }
                catch(MySqlException me)
                {
                    MessageBox.Show("数据库异常！" + me.ToString());
                    return;
                }
            }
            textBox4.Clear();
        }

        private void button6_Click(object sender, EventArgs e)//参数发送
        {
            char[] a = new char[8];
            RemoteDebug rd = new RemoteDebug();
            if (!rd.flag11) a[0] = '0';
            if (rd.flag11) a[0] = '1';

            if (!rd.flag22) a[1] = '0';
            if (rd.flag22) a[1] = '1';

            if (!rd.flag33) a[2] = '0';
            if (rd.flag33) a[2] = '1';

            if (!rd.flag44) a[3] = '0';
            if (rd.flag44) a[3] = '1';

            if (!rd.flag55) a[4] = '0';
            if (rd.flag55) a[4] = '1';

            if (!rd.flag66) a[5] = '0';
            if (rd.flag66) a[5] = '1';

            if (!rd.flag77) a[6] = '0';
            if (rd.flag77) a[6] = '1';

            if (!rd.flag88) a[7] = '0';
            if (rd.flag88) a[7] = '1';
            //string SendData = new string(a);
            //MessageBox.Show(SendData);
            string SendData = textBox5.Text;
            if (SendData != "")
            {
                bool flag = true;
                for (int i = 0; i < SendData.Length; i++)
                {
                    int num = SendData[i] - '0';
                    if (num != 0 && num != 1)
                    {
                        flag = false;
                        break;
                    }
                }
                if (!flag || SendData.Length != 8)
                {
                    MessageBox.Show("请输入8位二进制数！");
                    return;
                }
            }
            //调试记录存入数据库
            string constr = "Server=127.0.0.1;Database=tonghua;User Id=root;Password=5120154230;";
            MySqlConnection mycon = new MySqlConnection(constr);
            mycon.Open();//打开连接
            byte[] arrMsg = Encoding.UTF8.GetBytes(SendData);
            byte[] SendMsg = new byte[arrMsg.Length + 1];
            SendMsg[0] = 6;
            Buffer.BlockCopy(arrMsg, 0, SendMsg, 1, arrMsg.Length);
            List<string> test = new List<string>(ClientInformation.Keys);
            for (int i = 0; i < ClientInformation.Count; i++)
            {
                Socket socket = ClientInformation[test[i]];
                socket.Send(SendMsg);
                textBox5.Clear();
                IPAddress ClientIP = (socket.RemoteEndPoint as IPEndPoint).Address;
                source = ServerSocket.LocalEndPoint.ToString();
                destination = ClientIP.ToString();
                string debug_data = SendData;
                old_data = Information;
                string record = "insert into debug_data (source,destination,datetime,old_data,debug_data) " +
                       "values('" + source + "','" + destination + "','" + DateTime.Now + "','" + old_data + "','" + debug_data + "')";
                MySqlCommand cmd = new MySqlCommand(record, mycon); //创建执行
                cmd.ExecuteNonQuery(); //执行SQL
                mycon.Close();
                mycon.Dispose();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            RemoteDebug sta = new RemoteDebug();
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
                RemoteDebug rd = new RemoteDebug();
                rd.a = Information.ToCharArray();
                sta.StartPosition = FormStartPosition.CenterScreen;//显示调试窗口
                sta.Show();
            }
            else
            {
                MessageBox.Show("暂无设备参数信息！");
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)//参数发送
        {

        }

        private void button8_Click(object sender, EventArgs e)//桌面监控
        {
            Screen_Show screen_Show = new Screen_Show();
            screen_Show.pictureBox1.Image = Bitmap.FromStream(ms);
            screen_Show.Show();
            screen_Show.pictureBox1.Refresh();
            /*if (!flagg)//第一次打开显示图像窗体
            {
                flagg = true;
                screen_Show.StartPosition = FormStartPosition.Manual;
                screen_Show.Show();
                //screen_Show.pictureBox1.Refresh();
            }
            else
            {
                screen_Show.pictureBox1.Refresh();
                Console.WriteLine("刷新picturebox\r\n");
            }*/
        }
    }
    public class ClientInfo
    {
        public string ClientID;//客户端IP
        public DateTime LastHeartbeatTime;//客户端最后心跳时间
        public Boolean State;//客户端在线状态
    }
}
