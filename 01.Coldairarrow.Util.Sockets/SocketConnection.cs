using System;
using System.Net.Sockets;
using System.Text;

namespace Coldairarrow.Util.Sockets
{
    /// <summary>
    /// Socket连接,双向通信
    /// </summary>
    public class SocketConnection
    {
        #region 构造函数
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socket">维护的Socket对象</param>
        /// <param name="server">维护此连接的服务对象</param>
        public SocketConnection(Socket socket,SocketServer server)
        {
            _socket = socket;
            _server = server;
        }

        #endregion

        #region 私有成员
        
        private readonly Socket _socket;
        private bool _isRec=true;
        private SocketServer _server = null;
        private bool IsSocketConnected()
        {
            bool part1 = _socket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (_socket.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }

        #endregion

        #region 外部接口

        /// <summary>
        /// 开始接受客户端消息
        /// </summary>
        public void StartRecMsg()
        {
            try
            {
                byte[] container = new byte[1024 * 1024 * 4];
                _socket.BeginReceive(container, 0, container.Length, SocketFlags.None, asyncResult =>
                {
                    try
                    {
                        int length = _socket.EndReceive(asyncResult);

                        //马上进行下一轮接受，增加吞吐量
                        if (length > 0 && _isRec && IsSocketConnected())
                            StartRecMsg();

                        if (length > 0)
                        {
                            byte[] recBytes = new byte[length];
                            Array.Copy(container, 0, recBytes, 0, length);
                            try
                            {
                                //处理消息
                                HandleRecMsg?.BeginInvoke(recBytes, this, _server, null, null);
                            }
                            catch (Exception ex)
                            {
                                HandleException?.Invoke(ex);
                            }
                        }
                        else
                            Close();
                    }
                    catch (Exception ex)
                    {
                        HandleException?.BeginInvoke(ex,null,null);
                        Close();
                    }
                }, null);
            }
            catch (Exception ex)
            {
                HandleException?.BeginInvoke(ex,null,null);
                Close();
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bytes">数据字节</param>
        public void Send(byte[] bytes)
        {
            try
            {
                _socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, asyncResult =>
                {
                    try
                    {
                        int length = _socket.EndSend(asyncResult);
                        HandleSendMsg?.BeginInvoke(bytes, this, _server,null,null);
                    }
                    catch (Exception ex)
                    {
                        HandleException?.BeginInvoke(ex,null,null);
                    }
                }, null);
            }
            catch (Exception ex)
            {
                HandleException?.BeginInvoke(ex,null,null);
            }
        }

        /// <summary>
        /// 发送字符串（默认使用UTF-8编码）
        /// </summary>
        /// <param name="msgStr">字符串</param>
        public void Send(string msgStr)
        {
            Send(Encoding.UTF8.GetBytes(msgStr));
        }

        /// <summary>
        /// 发送字符串（使用自定义编码）
        /// </summary>
        /// <param name="msgStr">字符串消息</param>
        /// <param name="encoding">使用的编码</param>
        public void Send(string msgStr,Encoding encoding)
        {
            Send(encoding.GetBytes(msgStr));
        }

        /// <summary>
        /// 传入自定义属性
        /// </summary>
        public object Property { get; set; }

        /// <summary>
        /// 关闭当前连接
        /// </summary>
        public void Close()
        {
            try
            {
                _isRec = false;
                _socket.Disconnect(false);
                _server.RemoveConnection(this);
                HandleClientClose?.BeginInvoke(this, _server,null,null);
            }
            catch (Exception ex)
            {
                HandleException?.BeginInvoke(ex,null,null);
            }
            finally
            {
                _socket.Dispose();
                GC.Collect();
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 客户端连接接受新的消息后调用
        /// </summary>
        public Action<byte[], SocketConnection, SocketServer> HandleRecMsg { get; set; }

        /// <summary>
        /// 客户端连接发送消息后回调
        /// </summary>
        public Action<byte[], SocketConnection, SocketServer> HandleSendMsg { get; set; }

        /// <summary>
        /// 客户端连接关闭后回调
        /// </summary>
        public Action<SocketConnection, SocketServer> HandleClientClose { get; set; }

        /// <summary>
        /// 异常处理程序
        /// </summary>
        public Action<Exception> HandleException { get; set; }

        #endregion
    }
}
