using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using GameServer.Model;
using MySql.Data.MySqlClient;

namespace GameServer.DAO
{
    /// <summary>
    /// 战绩数据
    /// </summary>
    class ResultDAO
    {
        /// <summary>
        /// 根据用户id获取战绩信息
        /// </summary>
        /// <param name="conn">数据库连接对象</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Result GetResultByUserid(MySqlConnection conn, int userId)
        {
            MySqlDataReader reader = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand("select * from result where userid = @userid", conn);
                cmd.Parameters.AddWithValue("userid", userId);
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int id = reader.GetInt32("id");
                    int totalCount = reader.GetInt32("totalcount");
                    int winCount = reader.GetInt32("wincount");

                    Result res = new Result(id, userId, totalCount, winCount);
                    return res;
                }
                else
                {
                    Result res = new Result(-1, userId, 0, 0);
                    return res;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("在GetResultByUserid的时候出现异常：" + e);
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            return null;
        }
        /// <summary>
        /// 更新和添加战绩
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="res">战绩模型</param>
        public void UpdateOrAddResult(MySqlConnection conn, Result res)
        {
            MySqlTransaction tran = conn.BeginTransaction();
            try
            {

                MySqlCommand cmd = null;

                if (res.Id <= -1)
                {
                    cmd = new MySqlCommand("insert into result set totalcount=@totalcount,wincount=@wincount,userid=@userid", conn);
                }
                else
                {
                    cmd = new MySqlCommand("update result set totalcount=@totalcount,wincount=@wincount where userid=@userid ", conn);
                }
                cmd.Parameters.AddWithValue("totalcount", res.TotalCount);
                cmd.Parameters.AddWithValue("wincount", res.WinCount);
                cmd.Parameters.AddWithValue("userid", res.UserId);
                cmd.ExecuteNonQuery();
                if (res.Id <= -1)
                {
                    Result tempRes = GetResultByUserid(conn, res.UserId);
                    res.Id = tempRes.Id;
                }
                tran.Commit();
            }
            catch (Exception e)
            {
                tran.Rollback();
                Console.WriteLine("在UpdateOrAddResult的时候出现异常：" + e);
            }
            finally
            {
                tran.Dispose();
            }
        }
    }
}
