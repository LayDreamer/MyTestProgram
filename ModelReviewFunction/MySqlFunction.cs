//using MySql.Data.MySqlClient;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;


namespace ModelReviewFunction
{
    public class MySqlFunction
    {
        public static string connstr = "server=10.10.12.171;database=bds_checkmsg;user=bds_checkmsg;password=bds_checkmsg@123;charset=utf8;";

        /// <summary>
        /// 上传和下载数据（项目-问题数据）
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="projectInfo"></param>
        /// <param name="isDownload"></param>
        /// <returns></returns>
        public static Dictionary<string, string> UploadOrDownLoadData(string projectName = "", string projectInfo = "", bool isDownload = true)
        {
            Dictionary<string, string> projectNameInfo = new Dictionary<string, string>();
            MySqlConnection connect = new MySqlConnection(connstr);
            connect.Open();
            string createStatement = "CREATE TABLE ProjectReviewTable(name VARCHAR(50),info VARCHAR(9999))";
            string selectsql = "SELECT * FROM ProjectReviewTable";
            string insertsql = string.Format("insert into ProjectReviewTable(name,info) values('{0}','{1}')", projectName, projectInfo);
            try
            {
                if (isDownload)
                {
                    MySqlDataReader dataReader = MySqlHelper.ExecuteReader(connstr, CommandType.Text, selectsql, null);
                    while (dataReader.Read())
                    {
                        string name = ""; string info = "";
                        name = dataReader.GetString(0).ToString();
                        info = dataReader.GetString(1).ToString();
                        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(info)) { continue; }
                        projectNameInfo.Add(name, info);
                    }
                }
                else
                {
                    MySqlHelper.ExecuteNonQuery(connect, CommandType.Text, insertsql, null);
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1146)
                {
                    MySqlHelper.ExecuteNonQuery(connect, CommandType.Text, createStatement, null);
                    MySqlHelper.ExecuteNonQuery(connect, CommandType.Text, insertsql, null);
                }
            }
            finally
            {
                connect.Close();
            }
            return projectNameInfo;
        }

        public static void CheckConnectState()
        {
            MySqlConnection connect = new MySqlConnection(connstr);
            connect.Open();
            ConnectionState state = connect.State;
            connect.Close();
        }
    }
}
