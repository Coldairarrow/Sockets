using Coldairarrow.Util.Sockets;
using System;
using System.Text;

namespace Console_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //创建客户端对象，默认连接本机127.0.0.1,端口为12345
            SocketClient client = new SocketClient(12345);

            //绑定当收到服务器发送的消息后的处理事件
            client.HandleRecMsg = new Action<byte[], SocketClient>((bytes, theClient) =>
            {
                string msg = Encoding.UTF8.GetString(bytes);
                Console.WriteLine($"收到消息:{msg}");
            });

            //绑定向服务器发送消息后的处理事件
            client.HandleSendMsg = new Action<byte[], SocketClient>((bytes, theClient) =>
            {
                string msg = Encoding.UTF8.GetString(bytes);
                Console.WriteLine($"向服务器发送消息:{msg}");
            });

            //开始运行客户端
            client.StartClient();

            while (true)
            {
                Console.WriteLine("输入:quit关闭客户端，输入其它消息发送到服务器");
                string str = Console.ReadLine();
                if (str == "quit")
                {
                    client.Close();
                    break;
                }
                else
                {
                    client.Send(str);
                }
            }
        }
    }
}
