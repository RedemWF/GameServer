using Common;
using GameServer.Servers;

namespace GameServer.Controller
{
    /// <summary>
    /// 游戏i控制器
    /// </summary>
    class GameController : BaseController
    {
        public GameController()
        {
            //设置请求代码
            requestCode = RequestCode.Game;
        }
        /// <summary>
        /// 开始游戏请求处理
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string StartGame(string data, Client client, Server server)
        {
            //判断是否为房主
            if (client.IsHouseOwner())
            {
                //获取客户端房间信息
                var clientRoom = client.Room;
                //广播开始游戏消息
                clientRoom.BroadcastMessage(client, ActionCode.StartGame, ((int)ReturnCode.Success).ToString());
                //开始倒计时
                clientRoom.StartTimer();
                return ((int)ReturnCode.Success).ToString();
            }
            else
            {
                //返回失败
                return ((int)ReturnCode.Fail).ToString();
            }
        }
        /// <summary>
        /// 移动请求处理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string Move(string data, Client client, Server server)
        {
            //获取房间信息
            var room = client.Room;
            if (room is not null)
            {
                //广播移动消息
                room.BroadcastMessage(client, ActionCode.Move, data);
            }
            return null;

        }
        /// <summary>
        /// 射击请求处理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string Shoot(string data, Client client, Server server)
        {
            var room = client.Room;
            if (room is not null)
            {
                room.BroadcastMessage(client, ActionCode.Shoot, data);
            }
            return null;
        }
        /// <summary>
        /// 攻击请求处理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string Attack(string data, Client client, Server server)
        {
            //获得伤害值
            var damage = int.Parse(data);
            Room room = client.Room;
            if (room == null)
            {
                return null;
            }
            //造成伤害方法
            room.TakeDamage(damage, client);
            return null;
        }
    }
}
