using System.Net.Sockets;
using Common;
using MySql.Data.MySqlClient;
using GameServer.Tool;
using GameServer.Model;
using GameServer.DAO;
namespace GameServer.Servers
{
    /// <summary>
    /// 客户端
    /// </summary>
    public class Client
    {
        public Client() { }
        public Client(Socket clientSocket, Server server)
        {
            this.clientSocket = clientSocket;
            this.server = server;
            mysqlConn = ConnHelper.Connect();
        }
        //Socket
        private Socket clientSocket;
        private Server server;
        private Message msg = new Message();
        /// <summary>
        /// 数据库连接
        /// </summary>
        private MySqlConnection mysqlConn;
        private User user;
        private Result result;
        private ResultDAO resultDAO = new ResultDAO();
        private Room room;
        public Room Room
        {
            get => room;
            set => room = value;
        }

        public int HP
        {
            get; set;
        }
        /// <summary>
        /// 造成伤害
        /// </summary>
        /// <param name="damage"></param>
        /// <returns></returns>
        public bool TakeDamage(int damage)
        {
            HP -= damage;
            //如果HP<0则设置为0
            HP = Math.Max(HP, 0);
            if (HP <= 0) return true;
            return false;
        }
        /// <summary>
        /// 是否死亡
        /// </summary>
        /// <returns></returns>
        public bool IsDie()
        {
            return HP <= 0;
        }
        public MySqlConnection MySQLConn
        {
            get { return mysqlConn; }
        }
        /// <summary>
        /// 设置用户数据
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="result">战绩</param>
        public void SetUserData(User user, Result result)
        {
            this.user = user;
            this.result = result;
        }
        /// <summary>
        /// 获取用户数据
        /// </summary>
        /// <returns></returns>
        public string GetUserData()
        {
            return user.Id + "," + user.Username + "," + result.TotalCount + "," + result.WinCount;
        }
        /// <summary>
        /// 获取用户ID
        /// </summary>
        /// <returns></returns>
        public int GetUserId()
        {
            return user.Id;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Start()
        {
            //判断对话是否为空
            if (clientSocket == null || clientSocket.Connected == false) return;
            //开启异步对话
            clientSocket.BeginReceive(msg.Data, msg.StartIndex, msg.RemainSize, SocketFlags.None, ReceiveCallback, null);
        }
        /// <summary>
        /// 对话回调
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                if (clientSocket == null || clientSocket.Connected == false) return;

                int count = clientSocket.EndReceive(ar);
                if (count == 0)
                {
                    Close();
                }
                //开始异步接收消息
                msg.ReadMessage(count, OnProcessMessage);
                //重新开始对话
                Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Close();
            }
        }
        /// <summary>
        /// 接收消息回调
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="actionCode"></param>
        /// <param name="data"></param>
        private void OnProcessMessage(RequestCode requestCode, ActionCode actionCode, string data)
        {
            server.HandleRequest(requestCode, actionCode, data, this);
        }
        /// <summary>
        /// 关闭对话
        /// </summary>
        private void Close()
        {
            ConnHelper.CloseConnection(mysqlConn);
            if (clientSocket != null)
                clientSocket.Close();
            if (room != null)
            {
                room.QuitRoom(this);
            }
            server.RemoveClient(this);
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="actionCode"></param>
        /// <param name="data"></param>
        public void Send(ActionCode actionCode, string data)
        {
            try
            {
                byte[] bytes = Message.PackData(actionCode, data);
                clientSocket.Send(bytes);
            }
            catch (Exception e)
            {
                Console.WriteLine("无法发送消息:" + e);
            }
        }
        /// <summary>
        /// 更新战绩
        /// </summary>
        /// <param name="isVictory"></param>
        public void UpdateResult(bool isVictory)
        {
            UpdateResultToDB(isVictory);
            UpdateResultToClient();
        }
        /// <summary>
        /// 将战绩更新到数据库
        /// </summary>
        /// <param name="isVictory"></param>
        private void UpdateResultToDB(bool isVictory)
        {
            result.TotalCount++;
            if (isVictory)
            {
                result.WinCount++;
            }
            resultDAO.UpdateOrAddResult(mysqlConn, result);
        }
        /// <summary>
        /// 将战绩更新到客户端
        /// </summary>
        private void UpdateResultToClient()
        {
            Send(ActionCode.UpdateResult, string.Format("{0},{1}", result.TotalCount, result.WinCount));
        }
        /// <summary>
        /// 判断是否为房主
        /// </summary>
        /// <returns></returns>
        public bool IsHouseOwner()
        {
            return room.IsHouseOwner(this);
        }
    }
}
