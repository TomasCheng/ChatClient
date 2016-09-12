using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace ChatClient
{
    public partial class Form1 : Form
    {
        static TcpClient client = null;
        static NetworkStream stream = null;
        static string ServerIP = null;
        static string NickName = null;
        static int port = 9999;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnLogin.Enabled = true;
            btnQuit.Enabled = false;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if(tbServerIp.Text=="")
            {
                MessageBox.Show("服务器IP不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (tbNickName.Text == "")
            {
                MessageBox.Show("昵称不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ServerIP = tbServerIp.Text;
            NickName = tbNickName.Text;
            client = new TcpClient();
            client.Connect(ServerIP,port);
            lbStatus.Text = NickName + "（在线）";
            ThreadsStart();
            
        }
        public void ThreadsStart()
        {
            Thread myThread = new Thread(new ThreadStart(GetMsgs));
            myThread.Start();
        }
        private void GetMsgs()
        {
            while(true)
            {
                Byte[] bytes = new Byte[1024];
                stream = client.GetStream();
                stream.Read(bytes, 0, bytes.Length);
                string data = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                //对获取到的信息进行解析
                //U:add:name1
                //U:remove:name2
                string[] datas = data.Split(':');
                if(datas[0].Equals("U"))    //用户列表变化消息
                {
                    if(datas[1].Equals("add"))
                    {
                        tbUsers.Text += datas[2] + "\n";
                    }
                    else if(datas[1].Equals("remove"))
                    {
                        string[] users = tbUsers.Text.Split('\n');
                        for(int i=0;i<users.Length;i++)
                        {
                            if(users[i].Equals(datas[2]))
                            {
                                users[i] = "";
                                break;
                            }
                        }
                        for (int i = 0; i < users.Length; i++)
                        {
                            tbUsers.Text += users[i] + "\n";
                        }

                    }
                }
                //M:time:name3:content
                else if(datas[0].Equals("M"))   //消息变化消息
                {
                    tbMsgs.Text += "Time:" + datas[1] + "\n";
                    tbMsgs.Text += datas[2] + ":" + datas[3] + "\n\n";
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(tbSend.Text=="")
            {
                MessageBox.Show("请输入要发送的内容！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //对即将发送的消息的包装
            StringBuilder sb = new StringBuilder();
            sb.Append("M:");
            sb.Append(new DateTime().ToLocalTime() + ":");
            sb.Append(NickName + ":");
            sb.Append(tbSend.Text);

            byte[] msg = Encoding.UTF8.GetBytes(sb.ToString());
            stream = client.GetStream();
            stream.Write(msg, 0, msg.Length);
        }
    }
}
