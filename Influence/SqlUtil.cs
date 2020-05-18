using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace Influence
{
    class SqlUtil
    {
        readonly string ConnectionString;

        public SQLiteConnection ConnectionToDB()
        {             
            SQLiteConnection dbConnection = new SQLiteConnection("Data Source=./Repository/Database.db;Version=3;");
            dbConnection.Open();
            return dbConnection;
        }

        public void DisconnectionToDB(SQLiteConnection dbConnection)
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
            }
        }

        // 작업량이 남은 유저 리스트
        public List<User> SelectWorkRemainUserList()
        {
            List<User> list = new List<User>();

            SQLiteConnection dbConnection = ConnectionToDB();

            String sql = ""
                     + "SELECT "
                     + "* "
                     + "FROM USER  U "
                     + "WHERE 1 = 1 "
                     + "AND U.USE_YN = 'Y' "
                     + "AND EXISTS ( "
                     + "	SELECT  1 "
                     + "	  FROM HASH H "
                     + "	 WHERE 1 = 1 "
                     + "	   AND H.NICK_NM = U.NICK_NM "
                     + "	   AND H.WORK_CNT < H.TOT_CNT "
                     + "	   AND H.WORK_YMD = strftime('%Y%m%d' ,'now', 'localtime')"
                    + ")";

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                User g = new User();
                g.nickNm = (string)reader["NICK_NM"];
                list.Add(g);
            }

            DisconnectionToDB(dbConnection);

            return list;
        }

        // 작업량이 남은 해시 리스트
        public List<Hash> SelectWorkRemainHashList(string nickNm)
        {
            List<Hash> list = new List<Hash>();

            SQLiteConnection dbConnection = ConnectionToDB();

            String sql = ""
                    + "SELECT "
                    + "	   * "
                    + "	  FROM HASH H "
                    + "	 WHERE 1 = 1 "
                    + "	   AND H.NICK_NM = '" + nickNm + "' "
                    + "	   AND H.WORK_CNT < H.TOT_CNT "
                    + "	   AND H.WORK_YMD = strftime('%Y%m%d' ,'now', 'localtime')";

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Hash h = new Hash();
                h.hashNm = (string)reader["HASH_NM"];
                h.totCnt = (long)reader["TOT_CNT"];
                h.workCnt = (long)reader["WORK_CNT"];
                h.workYmd = (string)reader["WORK_YMD"];
                list.Add(h);
            }

            DisconnectionToDB(dbConnection);

            return list;
        }


        // 일자별 해시 리스트 조회
        public List<Hash> SelectHashList(string nickNm, string workYmd)
        {
            List<Hash> list = new List<Hash>();

            SQLiteConnection dbConnection = ConnectionToDB();

            String sql = ""
                    + "select "
                    + "    H.WORK_YMD "
                    + "	,U.NICK_NM "
                    + "	,H.HASH_NM "
                    + "	,H.TOT_CNT "
                    + "	,H.WORK_CNT "
                    + "from USER U "
                    + "left outer join HASH H ON U.NICK_NM = H.NICK_NM "
                    + "where 1 = 1 "
                    + "and U.USE_YN = 'Y' "
                    + "and H.WORK_YMD > strftime('%Y%m%d' ,'now', 'localtime', '-7 day')";

                    if (nickNm != null && nickNm.Trim().Length != 0)
                    {
                        sql += "and U.NICK_NM = '" + nickNm + "' ";
                    }

                    if (workYmd != null && workYmd.Trim().Length != 0)
                    {
                        sql += "and H.WORK_YMD = '" + workYmd + "' ";
                    }

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Hash h = new Hash();
                h.workYmd = (string)reader["WORK_YMD"];
                h.nickNm = (string)reader["NICK_NM"];
                h.hashNm = (string)reader["HASH_NM"];
                h.totCnt = (long)reader["TOT_CNT"];
                h.workCnt = (long)reader["WORK_CNT"];
                
                list.Add(h);
            }

            DisconnectionToDB(dbConnection);

            return list;
        }


        // 일자별 해시 리스트 조회
        public List<IpHistory> selectIpHistory(int minutes, string ip)
        {
            List<IpHistory> list = new List<IpHistory>();

            SQLiteConnection dbConnection = ConnectionToDB();

            String sql = ""
                    + "select "
                    + "    H.YMDT "
                    + "	,H.IP "                   
                    + "from IP_HISTORY H "                    
                    + "where 1 = 1 "
                    + "and H.YMDT > strftime('%Y%m%d%H%M%S' ,'now', 'localtime', '-" + minutes + " minutes')"
                    + "and H.IP = '" + ip + "'";

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                IpHistory h = new IpHistory();
                h.ymdt = (string)reader["YMDT"];
                h.ip = (string)reader["IP"];

                list.Add(h);
            }

            DisconnectionToDB(dbConnection);

            return list;
        }

        public void InsertIpHistory(string ip)
        {
            SQLiteConnection dbConnection = ConnectionToDB();
            string sql = string.Format("INSERT INTO IP_HISTORY (YMDT,  IP) VALUES (strftime('%Y%m%d%H%M%S' ,'now', 'localtime') ,'{0}')", ip);

            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            command.ExecuteNonQuery();
            DisconnectionToDB(dbConnection);
        }

        public void UpdateHashWorkCnt(string workYmd, string nickNm, string hashNm)
        {
            SQLiteConnection dbConnection = ConnectionToDB();

            String sql = ""
                    + "UPDATE HASH "
                    + "SET WORK_CNT = WORK_CNT + 1 "
                    + "WHERE 1 = 1 "
                    + " AND WORK_YMD = '"+ workYmd  + "' "
                    + " AND NICK_NM = '"+ nickNm + "' "
                    + " AND HASH_NM = '"+ hashNm + "' ";
            
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            command.ExecuteNonQuery();

            DisconnectionToDB(dbConnection);
        }

    }
}

