
using System.Text;
using Common;

namespace GameServer.Servers
{
    /// <summary>
    /// 房间
    /// </summary>
    public class Room
    {
        /// <summary>
        /// 房间状态
        /// </summary>
        enum RoomState
        {
            WaitingJoin,
            WaitingBattle,
            Battle,
            End
        }
        //所有房间
        private List<Client> rooms = new List<Client>();
        private Server server;
        //默认房间状态
        private RoomState state = RoomState.WaitingJoin;
        //玩家最大HP
        private const int MAX_HP = 200;

        public Room(Server server)
        {
            this.server = server;
        }

        /// <summary>
        /// 判断是否等待加入
        /// </summary>
        /// <returns>bool</returns>
        public bool ISWaitingJoin()
        {
            return state == RoomState.WaitingJoin;
        }
        /// <summary>
        /// 房间中添加客户端
        /// </summary>
        /// <param name="client"></param>
        public void AddClient(Client client)
        {
            client.HP = MAX_HP;
            rooms.Add(client);
            client.Room = this;
            if (rooms.Count >= 2)
            {
                state = RoomState.WaitingBattle;
            }
        }
        /// <summary>
        /// 房间中移除客户端
        /// </summary>
        /// <param name="client"></param>
        public void RemoveClient(Client client)
        {
            client.Room = null;
            rooms.Remove(client);

            if (rooms.Count >= 2)
            {
                state = RoomState.WaitingBattle;
            }
            else
            {
                state = RoomState.WaitingJoin;
            }
        }
        /// <summary>
        /// 获取房主i信息
        /// </summary>
        /// <returns></returns>
        public string GetHouseOwnerInfo()
        {
            return rooms[0].GetUserData();
        }
        /// <summary>
        /// 退出房间
        /// </summary>
        /// <param name="client"></param>
        public void QuitRoom(Client client)
        {
            //判断是否为房主
            if (client == rooms[0])
            {
                //关闭房间
                Close();
            }
            else
            {
                //移除客户
                rooms.Remove(client);
            }
        }
        /// <summary>
        /// 关闭房间
        /// </summary>
        public void Close()
        {
            foreach (var client in rooms)
            {
                client.Room = null;
            }
            server.RemoveRoom(this);
        }
        /// <summary>
        /// 获取房间ID
        /// </summary>
        /// <returns>RoomID</returns>
        public int GetId()
        {
            if (rooms.Count > 0)
            {
                return rooms[0].GetUserId();
            }

            return -1;
        }
        /// <summary>
        /// 获取所有房间
        /// </summary>
        /// <returns></returns>
        public string GetRoomData()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var client in rooms)
            {
                sb.Append(client.GetUserData() + "|");
            }

            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }
        /// <summary>
        /// 广播消息
        /// </summary>
        /// <param name="excludeClient">不需要通知的客户端</param>
        /// <param name="actionCode"></param>
        /// <param name="data"></param>
        public void BroadcastMessage(Client excludeClient, ActionCode actionCode, string data)
        {
            foreach (var client in rooms)
            {
                if (client != excludeClient)
                {
                    server.SendResponse(client, actionCode, data);
                }
            }
        }
        /// <summary>
        /// 判断是否为房主
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool IsHouseOwner(Client client)
        {
            return client == rooms[0];
        }
        /// <summary>
        /// 开启计时器
        /// </summary>
        public void StartTimer()
        {
            new Task(RunTimer).Start();

        }
        /// <summary>
        /// 计时器
        /// </summary>
        private async void RunTimer()
        {
            //等待1秒
            await Task.Delay(1000);
            //广播计时信息
            for (int i = 3; i > 0; i--)
            {
                BroadcastMessage(null, ActionCode.ShowTimer, i.ToString());
                await Task.Delay(1000);
            }
            //广播开始游戏信息
            BroadcastMessage(null, ActionCode.StartPlay, "r");
        }
        /// <summary>
        /// 造成伤害
        /// </summary>
        /// <param name="damage">伤害值</param>
        /// <param name="excludeClient">执行客户端</param>
        public void TakeDamage(int damage, Client excludeClient)
        {
            bool isDie = false;
            foreach (var client in rooms)
            {
                if (client != excludeClient)
                {
                    if (client.TakeDamage(damage))
                    {
                        isDie = true;
                    }
                }
            }
            //判断是否i死亡
            if (isDie)
            {
                foreach (var client in rooms)
                {
                    //如果死亡，游戏结束
                    if (client.IsDie())
                    {
                        //胜利消息
                        client.Send(ActionCode.GameOver, ((int)ReturnCode.Fail).ToString());
                    }
                    else
                    {
                        //失败消息
                        client.Send(ActionCode.GameOver, ((int)ReturnCode.Success).ToString());
                    }
                }
                Close();
            }
        }
    }
}