using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using MySql.Data.MySqlClient;
namespace RemoteServer
{

    public partial class Regist : Form
    {
        public Regist()
        {
            InitializeComponent();
        }
        private void Regist_Load(object sender, EventArgs e)
        {

        }
        private void textBox1_TextChanged(object sender, EventArgs e)//用户名cbUser
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)//密码tbPwd
        {

        }
        private void button1_Click(object sender, EventArgs e)//注册
        {
            try
            {
                if (cbUser.Text == "")
                {
                    MessageBox.Show("用户名不能为空！");
                    return;
                }
                else if (tbPwd.Text.Length > 16 || tbPwd.Text.Length <= 3)
                {
                    MessageBox.Show("密码长度为4-16位!");
                    return;
                }
                else if (tbPwd2.Text.Length > 16 || tbPwd2.Text.Length <= 3)
                {
                    MessageBox.Show("确认密码长度为4-16位!");
                    return;
                }
                else if (tbPwd.Text != tbPwd2.Text)
                {
                    MessageBox.Show("两次密码不正确!");
                    return;
                }

                string constr = "Server=127.0.0.1;Database=tonghua;User Id=root;Password=5120154230;";
                MySqlConnection mycon = new MySqlConnection(constr);
                mycon.Open();//打开连接
                string check = "select password from information where user='" + cbUser.Text + "'";
                MySqlDataAdapter da = new MySqlDataAdapter(check, mycon); //创建适配器
                DataSet ds = new DataSet(); //创建数据集
                da.Fill(ds, "information"); //填充数据集
                if (da.Fill(ds, "user") > 0) //判断同名
                {
                    MessageBox.Show("该用户已经注册！"); //输出信息
                    cbUser.Clear();
                    tbPwd.Clear();
                    tbPwd2.Clear();
                    return;
                }
                else
                {
                    string strsql = "insert into information(user,password) values ('" + cbUser.Text + "','" + tbPwd.Text + "')";
                    MySqlCommand cmd = new MySqlCommand(strsql, mycon); //创建执行
                    cmd.ExecuteNonQuery(); //执行SQL
                    mycon.Close();//关闭连接

                    MessageBox.Show("注册成功!");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            this.Hide();
            Login login = new Login();
            login.StartPosition = FormStartPosition.CenterScreen;
            login.Show();
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
            else if(sender.Equals(tbPwd2))
            {
                labelPwd2.Visible = tbPwd2.Text.Length < 1;
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
            else if (sender.Equals(labelPwd2))
            {
                tbPwd2.Focus();
            }
        }
 
        private void labelPwd_Click(object sender, EventArgs e)
        {

        }

        private void labelUser_Click(object sender, EventArgs e)
        {

        }

        private void labelPwd2_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            Login login = new Login();
            login.StartPosition = FormStartPosition.CenterScreen;
            login.Show();
        }
    }
}
