using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using System.IO.Ports;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private StringBuilder sb = new StringBuilder();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //添加波特率下拉表
            for (int i = 300; i <= 38400; i = i * 2) {
                comboBox2.Items.Add( i.ToString() );
            }
            string[] baud = { "43000", "56000", "57600", "115200", "128000", "230400", "256000", "460800" };
            comboBox2.Items.AddRange( baud );

            //添加数据位下拉表
            string[] databit = { "8", "7", "9" };
            comboBox3.Items.AddRange( databit );

            //添加校验位下拉表
            string[] judgebit = { "None", "odd", "Even", "Mard", "Space" };
            comboBox4.Items.AddRange( judgebit );

            //添加停止位下拉表
            string[] stopbit = { "1", "1.5", "2" };
            comboBox5.Items.AddRange( stopbit );

            //获取电脑当前可用的串口号并设置默认值
            comboBox1.Items.AddRange( System.IO.Ports.SerialPort.GetPortNames() );
            if ( comboBox1.Items.Count != 0 ) 
                comboBox1.SelectedIndex = 0;

            //设置默认值
            comboBox2.Text = "115200";
            comboBox3.Text = "8";
            comboBox4.Text = "None";
            comboBox5.Text = "1";

            //手动添加串口接收中断函数
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (radioButton1.Checked)
            {
                //接收到的用ASCII模式显示
                string str = serialPort1.ReadExisting();
                //采用委托的方式
                this.Invoke(new EventHandler(delegate
                {
                    textBox1.AppendText(str);
                }));
            }
            else
            {
                //将接收到的字符存入数组中
                int num = serialPort1.BytesToRead;  //接收到的字节数
                byte[] receive_buf = new byte[num];
                serialPort1.Read(receive_buf, 0, num);

                sb.Clear();
                //接收到的用HEX模式显示
                foreach (byte b in receive_buf)
                {
                    sb.Append("0X"+b.ToString("X2")+" ");
                }
                this.Invoke(new EventHandler(delegate
                {
                    textBox1.AppendText(sb.ToString());
                }));
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //清零
            comboBox1.Items.Clear();
            //获取电脑当前可用的串口号
            comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            if (comboBox1.Items.Count != 0)
                comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try 
            {
                //判断串口是否处于打开状态
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    //表示串口此时处于关闭状态
                    button1.Text = "打开串口";
                    label6.Text = "串口已关闭";
                    label6.ForeColor = Color.Red;

                    //使能comboBox
                    comboBox1.Enabled = true;
                    comboBox2.Enabled = true;
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = true;

                    //失能发送控件
                    button2.Enabled = false;
                }
                else
                {
                    //此时串口处于打开状态
                    //失能comboBox
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;
                    comboBox4.Enabled = false;
                    comboBox5.Enabled = false;

                    //配置串口的相关属性
                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                    serialPort1.DataBits = Convert.ToInt16(comboBox3.Text);

                    if (comboBox4.Text.Equals("None"))
                        serialPort1.Parity = System.IO.Ports.Parity.None;
                    else if (comboBox4.Text.Equals("Odd"))
                        serialPort1.Parity = System.IO.Ports.Parity.Odd;
                    else if (comboBox4.Text.Equals("Even"))
                        serialPort1.Parity = System.IO.Ports.Parity.Even;
                    else if (comboBox4.Text.Equals("Mark"))
                        serialPort1.Parity = System.IO.Ports.Parity.Mark;
                    else if (comboBox4.Text.Equals("Space"))
                        serialPort1.Parity = System.IO.Ports.Parity.Space;

                    if (comboBox5.Text.Equals("1"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.One;
                    else if (comboBox5.Text.Equals("1.5"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.OnePointFive;
                    else if (comboBox5.Text.Equals("2"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.Two;
                    
                    //打开串口
                    serialPort1.Open();

                    //表示此时串口处于打开状态
                    button1.Text = "关闭串口";
                    label6.Text = "串口已打开";
                    label6.ForeColor = Color.Red;
                    //使能发送控件
                    button2.Enabled = true;
                }
            }
            catch
            {
                MessageBox.Show("端口错误,请检查串口", "错误");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] temp = new byte[1];
            try
            {
                if (radioButton4.Checked)
                {
                    //串口处于开启状态，将发送区文本发送
                    //ASCII发送模式
                    serialPort1.Write(textBox2.Text);
            
                }
                else
                {
                    //HEX发送模式
                    //用正则表达式去除空格
                    string buf = textBox2.Text;
                    string pattern = @"\s";
                    string replacement = "";
                    Regex rgx = new Regex(pattern);
                    string str_send = rgx.Replace(buf, replacement);

                    //发送数据
                    int num = (str_send.Length - str_send.Length % 2) / 2;
                    for (int i = 0; i < num; i ++)
                    {
                        temp[0] = Convert.ToByte(str_send.Substring(i*2, 2),16);    //将两个发送字符转换为16进制的数
                        serialPort1.Write(temp, 0, 1);
                    }
                    //若还存在最后一位，截取1位并发送
                    if (str_send.Length%2 != 0)
                    {
                        temp[0] = Convert.ToByte(str_send.Substring(str_send.Length - 1, 1), 16);
                        serialPort1.Write(temp, 0, 1);
                    }
                }
                
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //清空发送区
            textBox2.Text = "";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //清空接受区
            textBox1.Text = "";
        }

    }
}
