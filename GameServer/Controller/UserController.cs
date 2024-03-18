using Common;
using GameServer.Servers;
using GameServer.DAO;
using GameServer.Model;
using MySql.Data.MySqlClient;

namespace GameServer.Controller
{
    /// <summary>
    /// 用户控制器
    /// </summary>
    class UserController : BaseController
    {
        //用户模型
        private UserDAO userDAO = new UserDAO();
        //战绩模型
        private ResultDAO resultDAO = new ResultDAO();

        public UserController()
        {
            requestCode = RequestCode.User;
        }
        /// <summary>
        /// 登陆请求处理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string Login(string data, Client client, Server server)
        {
            string[] strs = data.Split(",");
            User user = userDAO.VerifyUser(client.MySQLConn, strs[0], strs[1]);
            if (user is null)
            {
                return ((int)ReturnCode.Fail).ToString();
            }
            else
            {
                Result result = resultDAO.GetResultByUserid(client.MySQLConn, user.Id);
                client.SetUserData(user, result);
                return string.Format("{0},{1},{2},{3}", ((int)ReturnCode.Success).ToString(), user.Username, result.TotalCount, result.WinCount);
                // return ((int)ReturnCode.Success).ToString();
            }

        }
        /// <summary>
        /// 注册请求处理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public string Register(string data, Client client, Server server)
        {
            string[] strs = data.Split(",");
            string username = strs[0];
            string password = strs[1];
            var res = userDAO.GetUserByUsername(client.MySQLConn, username);
            if (res) return ((int)ReturnCode.Fail).ToString();
            userDAO.AddUser(client.MySQLConn, username, password);
            return ((int)ReturnCode.Success).ToString();
        }
    }
}
