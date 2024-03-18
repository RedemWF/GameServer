using System.Text;
using Common;
using GameServer.Servers;
namespace GameServer.Controller
{
    /// <summary>
    /// 房间控制器
    /// </summary>
    class RoomController : BaseController
    {
        public RoomController()
        {
            requestCode = RequestCode.Room;
        }
        /// <summary>
        /// 创建房间请求处理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string CreateRoom(string data, Client client, Server server)
        {
            server.CreatRoom(client);
            return ((int)ReturnCode.Success) + "," + (int)RoleType.Blue;
        }
        /// <summary>
        /// 获取房间请求处理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string ListRoom(string data, Client client, Server server)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var room in server.Rooms)
            {
                //判断是否等待加入
                if (room.ISWaitingJoin())
                {
                    sb.Append(room.GetHouseOwnerInfo() + "|");
                }
            }
            if (sb.Length == 0)
            {
                sb.Append('0');
            }
            else
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 加入房间请求处理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string JoinRoom(string data, Client client, Server server)
        {
            int id = int.Parse(data);
            //跟据房间id加入
            var room = server.GetRoomById(id);
            if (room == null)
            {
                return ((int)ReturnCode.NotFound).ToString();
            }
            if (!room.ISWaitingJoin())
            {
                return ((int)ReturnCode.Fail).ToString();
            }
            //向房间中加入客户端
            room.AddClient(client);
            var roomData = room.GetRoomData();
            //广播消息
            room.BroadcastMessage(client, ActionCode.UpdateRoom, roomData);
            return ((int)ReturnCode.Success) + "," + (int)RoleType.Red + "-" + roomData;
        }
        /// <summary>
        /// 推出房间请求处理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string QuitRoom(string data, Client client, Server server)
        {
            var isHouseOwner = client.IsHouseOwner();
            Room room = client.Room;
            //判断是否为房主
            if (isHouseOwner)
            {
                room.BroadcastMessage(client, ActionCode.QuitRoom, ((int)ReturnCode.Success).ToString());
                //房间关闭
                room.Close();
                return ((int)ReturnCode.Success).ToString();
            }
            //不为房主则移除客户端
            client.Room.RemoveClient(client);
            //广播消息
            room.BroadcastMessage(client, ActionCode.UpdateRoom, room.GetRoomData());
            return ((int)ReturnCode.Success).ToString();
        }
    }
}
