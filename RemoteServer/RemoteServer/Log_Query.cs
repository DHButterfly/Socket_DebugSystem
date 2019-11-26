using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
namespace RemoteServer
{
    public partial class Log_Query : Form
    {
        public int pageSize = 17;      //每页记录数
        public int recordCount = 0;    //总记录数
        public int pageCount = 0;      //总页数
        public int currentPage = 0;    //当前页
        DataTable dtSource = new DataTable();
        public Log_Query()
        {
            InitializeComponent();
        }

        private void button5_Click(object sender, EventArgs e)//返回
        {
            this.Hide();
            Socket_Server socket_Server = new Socket_Server();
            socket_Server.StartPosition= FormStartPosition.CenterScreen;
            socket_Server.Show();
        }
        /// <summary>
        /// 时间设置
        /// 日志初始化
        /// 根据column[0]的headercell的width计算整个datagridview的宽度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Log_Query_Load(object sender, EventArgs e)
        {
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            dateTimePicker2.Format = DateTimePickerFormat.Custom;
            dateTimePicker2.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            comboBox1.Items.Add("聊天日志");
            comboBox1.Items.Add("动作日志");
            comboBox1.Text = "聊天日志";

            DataTable dt = new DataTable();
            dt.Columns.Add("id", typeof(string));
            dt.Columns.Add("source", typeof(string));
            dt.Columns.Add("destination",typeof(string));
            dt.Columns.Add("datetime", typeof(string));
            dt.Columns.Add("message", typeof(string));
            dt.Columns.Add("format", typeof(string));
            //假设这里绑定了4列的datatable
            //this.dataGridView1.SelectionMode=FullRows
            this.dataGridView1.DataSource = dt;//绑定
            this.dataGridView1.RowHeadersVisible = false;//datagridview前面的空白部分去除
            this.dataGridView1.ScrollBars = ScrollBars.None;//滚动条去除
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;//充满整个datagirdview1
            this.dataGridView1.Width = this.dataGridView1.Columns[0].HeaderCell.Size.Width * 6;
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)//日志类型：动作日志、聊天日志
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)//确定转到x页  
        {
            int num = Convert.ToInt32(textBox3.Text);
            if(num<1)
            {
                MessageBox.Show("请输入正确的页数!");
                return;
            }
            if (num >= 1) currentPage = num;
            if (num >= pageCount) currentPage = pageCount;
            LoadPage();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)//转到x页
        {

        }

        private void button3_Click(object sender, EventArgs e)//下一页
        {
            currentPage++;
            LoadPage();
        }

        private void button2_Click(object sender, EventArgs e)//上一页
        {
            currentPage--;
            LoadPage();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)//当前页/总页
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)//当前多少条
        {

        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)//截止时间
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)//起始时间
        {

        }
        /*
        private void InitDataSet()
        {
            pageSize = 20;      //设置页面行数
            nMax = dtInfo.Rows.Count;
            pageCount = (nMax / pageSize);    //计算出总页数
            if ((nMax % pageSize) > 0) pageCount++;
            pageCurrent = 1;    //当前页数从1开始
            nCurrent = 0;       //当前记录数从0开始
        }*/
        private void LoadPage()//加载数据
        {
            if (currentPage < 1) currentPage = 1;
            if (currentPage > pageCount) currentPage = pageCount;

            int beginRecord;
            int endRecord;
            DataTable dtTemp;
            dtTemp = dtSource.Clone();

            beginRecord = pageSize * (currentPage - 1);
            if (currentPage == 1) beginRecord = 0;
            endRecord = pageSize * currentPage;

            if (currentPage == pageCount) endRecord = recordCount;
            for (int i = beginRecord; i < endRecord; i++)//加载数据
            {
                dtTemp.ImportRow(dtSource.Rows[i]);
            }
            dataGridView1.DataSource = dtTemp;
            textBox1.Text = (endRecord - beginRecord).ToString();
            textBox2.Text = currentPage.ToString() + "/" + pageCount.ToString();
        }
        private void button1_Click(object sender, EventArgs e)//查询
        {
            Socket_Server socket_Server = new Socket_Server();
            string constr = "Server=127.0.0.1;Database=tonghua;User Id=root;Password=5120154230;";
            MySqlConnection mycon = new MySqlConnection(constr);
            mycon.Open();//打开连接
            if (comboBox1.Text == "聊天日志")
            {
                dataGridView1.Columns[1].HeaderText = "源IP地址"; //改列名称
                dataGridView1.Columns[2].HeaderText = "目的IP地址";
                dataGridView1.Columns[3].HeaderText = "时间";//改列名称
                dataGridView1.Columns[4].HeaderText = "聊天信息";
                dataGridView1.Columns[5].HeaderText = "视频格式";
                string start_time = dateTimePicker1.Text.ToString();
                string end_time = dateTimePicker2.Text.ToString();
                Console.WriteLine("start_time={0}     end_time={1}", start_time,end_time);
                string check= "select * from recording where datetime between '" + start_time + "' and'" + end_time + "' order by id asc";
                //string check = "select * from recording order by id asc";
                MySqlDataAdapter da = new MySqlDataAdapter(check, mycon); //创建适配器
                DataSet ds = new DataSet(); //创建数据集
                da.Fill(ds, "recording");//填充数据集
                //dataGridView1.DataSource = ds;
                //dataGridView1.DataMember = "recording";
                dtSource = ds.Tables[0];
                recordCount = dtSource.Rows.Count;
                if(recordCount==0)
                {
                    MessageBox.Show("查询时间段无记录!");
                    mycon.Close();
                    mycon.Dispose();
                    return;
                }
                pageCount = (recordCount / pageSize);
                if ((recordCount % pageSize) > 0)
                {
                    pageCount++;
                }
                currentPage = 1;//默认第一页
                LoadPage();
                mycon.Close();
                mycon.Dispose();
            }
            if(comboBox1.Text=="动作日志")
            {
                dataGridView1.Columns[1].HeaderText = "源IP地址"; //改列名称
                dataGridView1.Columns[2].HeaderText = "目的IP地址";
                dataGridView1.Columns[3].HeaderText = "时间";//改列名称
                dataGridView1.Columns[4].HeaderText = "设备参数";
                dataGridView1.Columns[5].HeaderText = "调试参数";
                string start_time = dateTimePicker1.Text.ToString();
                string end_time = dateTimePicker2.Text.ToString();
                string check = "select * from debug_data where datetime between '" + start_time + "' and'" + end_time + "' order by id asc";
                //string check = "select * from recording order by id asc";
                MySqlDataAdapter da = new MySqlDataAdapter(check, mycon); //创建适配器
                DataSet ds = new DataSet(); //创建数据集
                da.Fill(ds, "debug_data");//填充数据集
                dtSource = ds.Tables[0];
                recordCount = dtSource.Rows.Count;
                if(recordCount==0)
                {
                    MessageBox.Show("查询时间段无记录!");
                    mycon.Close();
                    mycon.Dispose();
                    return;
                }
                pageCount = (recordCount / pageSize);
                if ((recordCount % pageSize) > 0)
                {
                    pageCount++;
                }
                currentPage = 1;//默认第一页
                LoadPage();
                mycon.Close();
                mycon.Dispose();
            }
            //dataGridView1.Columns[1].DataPropertyName = ds.Tables[0].Columns[2].ToString();//前台第1列，显示数据库第2列的内容
            //dataGridView1.Columns[2].DataPropertyName = ds.Tables[0].Columns[1].ToString();//前台第2列，显示数据库第1列的内容
            //int count = dataGridView1.RowCount; //总行数
            //for (int i = 0; i < count - 1; i++)
            //{
            //Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            //id列显示成序列号，从1开始
            // }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
