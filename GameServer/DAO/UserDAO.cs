using MySql.Data.MySqlClient;
using GameServer.Model;
using System.Data;
using Org.BouncyCastle.Cms;
namespace GameServer.DAO
{
    /// <summary>
    /// 用户数据
    /// </summary>
    class UserDAO
    {
        /// <summary>
        /// 校验用户
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public User VerifyUser(MySqlConnection conn, string username, string password)
        {
            MySqlDataReader reader = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand("select * from user where username = @username and password = @password", conn);
                cmd.Parameters.AddWithValue("username", username);
                cmd.Parameters.AddWithValue("password", password);
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int id = reader.GetInt32("id");
                    User user = new User(id, username, password);
                    return user;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("在VerifyUser的时候出现异常：" + e);
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            return null;
        }
        /// <summary>
        /// 跟据用户名获取用户信息
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool GetUserByUsername(MySqlConnection conn, string username)
        {
            MySqlDataReader reader = null;
            MySqlCommand cmd = new MySqlCommand("select * from user where username = @username", conn);
            try
            {
                cmd.Parameters.AddWithValue("username", username);
                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("在GetUserByUsername的时候出现异常：" + e);
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            return false;
        }
        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void AddUser(MySqlConnection conn, string username, string password)
        {
            MySqlTransaction tran = conn.BeginTransaction();
            MySqlDataReader dr = null;
            MySqlCommand cmd = new MySqlCommand("insert into user set username = @username , password = @password", conn);
            try
            {
                cmd.Parameters.AddWithValue("username", username);
                cmd.Parameters.AddWithValue("password", password);
                cmd.ExecuteNonQuery();

                tran.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine("在AddUser的时候出现异常：" + e);
                tran.Rollback();
            }
            finally
            {
                cmd.Dispose();
                if (dr != null) dr.Close();
                tran.Dispose();
            }
        }
    }
}
