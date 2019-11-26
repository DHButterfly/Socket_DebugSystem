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

    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }
        private void textChanged(object sender, EventArgs e)
        {
            if (sender.Equals(cbUser))
            {
                labelUser.Visible = cbUser.Text.Length < 1;
            }
            else if (sender.Equals(tbPwd))
            {
                labelPwd.Visible = tbPwd.Text.Length < 1;
            }
        }
        private void label_Click(object sender, EventArgs e)
        {
            if (sender.Equals(labelUser))
            {
                cbUser.Focus();
            }
            else if (sender.Equals(labelPwd))
            {
                tbPwd.Focus();
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)//账号cbUser
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)//密码tbPwd
        {

        }

        private void button1_Click(object sender, EventArgs e)//登陆
        {
            if(cbUser.Text=="")
            {
                MessageBox.Show("用户名不能为空！");
                return;
            }
            if(tbPwd.Text=="")
            {
                MessageBox.Show("没有密码就想登陆吗？");
            }
            string constr = "Server=127.0.0.1;Database=tonghua;User Id=root;Password=5120154230;";
            MySqlConnection mycon = new MySqlConnection(constr);
            mycon.Open();//打开连接
            string check = "select user,password from information where user='" + cbUser.Text + "'and password='"+tbPwd.Text+"'";
            MySqlDataAdapter da = new MySqlDataAdapter(check, mycon); //创建适配器
            DataSet ds = new DataSet(); //创建数据集
            if (da.Fill(ds, "information")!=0) //判断同名
            {
                MessageBox.Show("恭喜你，登陆成功!"); //输出信息
                this.Hide();
                Socket_Server server = new Socket_Server();
                server.StartPosition = FormStartPosition.CenterScreen;
                server.Show();
            }
            else
            {
                MessageBox.Show("用户名或密码错误！");
                return;
            }
        }

        private void button2_Click(object sender, EventArgs e)//注册
        {

            this.Hide();
            Regist rs = new Regist();
            rs.StartPosition = FormStartPosition.CenterScreen;
            rs.Show();
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void labelPwd_Click(object sender, EventArgs e)
        {

        }

        private void labelUser_Click(object sender, EventArgs e)
        {

        }
    }
}
