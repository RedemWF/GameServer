using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using GameServer.Controller;
using Common;
namespace GameServer.Servers
{
    /// <summary>
    /// 服务器
    /// </summary>
    public class Server
    {
        private IPEndPoint ipEndPoint;
        private Socket serverSocket;
        private List<Client> clientList = new List<Client>();
        private ControllerManager controllerManager;
        private List<Room> rooms = new List<Room>();
        public List<Room> Rooms => rooms;
        public Server() { }
        public Server(string ipStr, int port)
        {
            //获取控制器管理器
            controllerManager = new ControllerManager(this);
            //设置IP和端口
            SetIpAndPort(ipStr, port);
        }
        /// <summary>
        /// 设置IP和端口
        /// </summary>
        /// <param name="ipStr">IP</param>
        /// <param name="port">端口</param>
        public void SetIpAndPort(string ipStr, int port)
        {
            ipEndPoint = new IPEndPoint(IPAddress.Parse(ipStr), port);
        }

        public void Start()
        {
            //设置Socket对话
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //绑定IP端口
            serverSocket.Bind(ipEndPoint);
            //开启监听
            serverSocket.Listen(0);
            //开启异步对话
            serverSocket.BeginAccept(AcceptCallBack, null);
            Console.WriteLine("StartSuccess!");
        }
        /// <summary>
        /// 异步对话回调
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallBack(IAsyncResult ar)
        {
            //开启会话
            Socket clientSocket = serverSocket.EndAccept(ar);
            //设置客户端关联
            Client client = new Client(clientSocket, this);
            client.Start();
            clientList.Add(client);
            serverSocket.BeginAccept(AcceptCallBack, null);
        }
        /// <summary>
        /// 移除客户端
        /// </summary>
        /// <param name="client"></param>
        public void RemoveClient(Client client)
        {
            lock (clientList)
            {
                clientList.Remove(client);
            }
        }
        /// <summary>
        /// 发送响应
        /// </summary>
        /// <param name="client"></param>
        /// <param name="actionCode"></param>
        /// <param name="data"></param>
        public void SendResponse(Client client, ActionCode actionCode, string data)
        {
            client.Send(actionCode, data);
        }
        /// <summary>
        /// 执行Action
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="actionCode"></param>
        /// <param name="data"></param>
        /// <param name="client"></param>
        public void HandleRequest(RequestCode requestCode, ActionCode actionCode, string data, Client client)
        {
            controllerManager.HandleRequest(requestCode, actionCode, data, client);
        }
        /// <summary>
        /// 创建房间
        /// </summary>
        /// <param name="client"></param>
        public void CreatRoom(Client client)
        {
            Room room = new Room(this);
            room.AddClient(client);
            rooms.Add(room);
        }
        /// <summary>
        /// 移除房间
        /// </summary>
        /// <param name="room"></param>
        public void RemoveRoom(Room room)
        {
            if (room != null && rooms != null)
            {
                rooms.Remove(room);
            }
        }
        /// <summary>
        /// 根据id获取房间
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Room GetRoomById(int id)
        {
            foreach (var room in rooms)
            {
                if (room.GetId() == id)
                {
                    return room;
                }
            }

            return null;
        }

    }
}
