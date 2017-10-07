# Sockets
A very helpful C# Sockets framework
<br/>
How to use it is here:http://www.cnblogs.com/coldairarrow/p/7501645.html

如何使用？<br/>
框架核心类：<br/>
SocketServer//Socket服务端<br/>
SocketConnection//Socket连接对象,双向通信<br/>
SocketClient//Socket客户端<br/>
1、建立连接<br/>
服务端： <br/>
SocketServer server = new SocketServer(12345);//默认监听地址0.0.0.0 端口12345,构造函数重载可以修改<br/>
server.StartServer();<br/>
客户端：<br/>
SocketClient client = new SocketClient(12345);//默认连接地址127.0.0.1 端口12345，构造函数重载可以修改<br/>
client.StartClient();<br/>
2、消息收发<br/>
服务端：<br/>
服务端主要就是维护一个客户端连接队列，每当新的客户端连接到服务端时，都会将新的连接对象添加到连接队列中。<br/>
因此，服务端要向客户端发送消息，必须先找到需要发送消息的连接对象。<br/>
那么才能如何找到需要发送消息的连接呢？<br/>
思路：当我们要寻找某些东西的时候，肯定需要某些东西的特征，比如说我们要确认一个人的身份，我们只需要知道这个人的身份证（黑的不算），那么我们就可以轻易的知道这个人的身份信息了。同样道理，我已经预先给SocketConnection开放了一个自定义属性Property,其类型为object，也就是说，你可以传字符串，也可以传自定义对象，这个Property就可以作为当前连接的身份标志了，当连接拥有身份标志之后，就可以通过Lambda表达式查询出来（不会的自己去补充基础），服务端调用GetConnectionList方法，传入筛选条件，即可找到符合条件的IEnumerable<SocketConnection>,也可以调用GetTheConnection方法，传入筛选条件，找到符合条件的一个SocketConnection，使用示例如下：<br/>
  var theConnection= server.GetTheConnection(x =><br/>
{<br/>
var Id = (string)x.Property;<br/>
return Id == "Admin";<br/>
});<br/>
<br/>
看代码吃力的，请补充基础<br/>
<br/>
发送消息：<br/>
theConnection.Send("Hello World!");//默认UTF-8编码格式发送字符串，有重载方法，不详解了<br/>
客户端：<br/>
发送消息:<br/>
client.Send("OK!");//默认UTF-8编码格式发送字符串，有重载方法，不详解了<br/>

事件处理：<br/>
客户端连接到服务端之后，双方肯定要进行通信，也就是收发数据，这里我只讲最常用的事件<br/>
1、新的客户端连接到服务端时触发（可以选择这个时候给对应的SocketConnection传入身份标识Property）<br/>
server.HandleNewClientConnected = new Action<SocketServer, SocketConnection>((theServer,theCon) =><br/>
{<br/>
  theCon.Property = "Admin";//身份标志，也可以传别的对象,自己定义，用的时候强制转下（不懂，百度：多态）<br/>
  Console.WriteLine($@"当前连接数：{theServer.GetConnectionCount()}");<br/>
});<br/>
2、服务端接收到客户端发送的消息时触发<br/>
//bytes为收到的数据（字节数组），client为对应的SocketConnection,theServer为维护连接的服务对象<br/>
server.HandleRecMsg = new Action<byte[], SocketConnection, SocketServer>((bytes,client,theServer)=> <br/>
{
   string msg = Encoding.UTF8.GetString(bytes);<br/>
   client.Send($"服务端已收到收到消息:{msg}");<br/>
   Console.WriteLine($"收到消息:{msg}");<br/>
});<br/>
3、客户端端接收到客户端发送的消息时触发<br/>
client.HandleRecMsg = new Action<byte[], SocketClient>((bytes, theClient) =><br/>
{<br/>
  string msg = Encoding.UTF8.GetString(bytes);<br/>
  Console.WriteLine($"收到消息:{msg}");<br/>
});<br/>

最后：其它还有很多操作，请看三个类的外部接口
