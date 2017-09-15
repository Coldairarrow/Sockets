using Coldairarrow.Util.Sockets;
using System;
using System.Text;

namespace Console_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //创建服务器对象，默认监听本机0.0.0.0，端口12345
            SocketServer server = new SocketServer(12345);

            //处理从客户端收到的消息
            server.HandleRecMsg = new Action<byte[], SocketConnection, SocketServer>((bytes, client, theServer) =>
            {
                string msg = Encoding.UTF8.GetString(bytes);
                Console.WriteLine($"收到消息:{msg}");
            });

            //处理服务器启动后事件
            server.HandleServerStarted = new Action<SocketServer>(theServer =>
            {
                Console.WriteLine("服务已启动************");
            });

            //处理新的客户端连接后的事件
            server.HandleNewClientConnected = new Action<SocketServer, SocketConnection>((theServer, theCon) =>
            {
                Console.WriteLine($@"一个新的客户端接入，当前连接数：{theServer.GetConnectionCount()}");
            });

            //处理客户端连接关闭后的事件
            server.HandleClientClose = new Action<SocketConnection, SocketServer>((theCon, theServer) =>
            {
                Console.WriteLine($@"一个客户端关闭，当前连接数为：{theServer.GetConnectionCount()}");
            });

            //处理异常
            server.HandleException = new Action<Exception>(ex =>
            {
                Console.WriteLine(ex.Message);
            });

            //服务器启动
            server.StartServer();

            while (true)
            {
                Console.WriteLine("输入:quit，关闭服务器");
                string op = Console.ReadLine();
                if (op == "quit")
                    break;
            }
        }
    }
}
