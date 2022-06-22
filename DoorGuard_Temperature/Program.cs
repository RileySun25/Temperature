using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace DoorGuard_Temperature
{
    class Program
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();  //Logger
       public static SqlConnection SQLconnectionOUR;  //我們的DB連線 
       public static SqlConnection SQLconnectionCUS;  //對方的DB連線
        public static DateTime Date = DateTime.Now;
        public static String TodayDate = Date.ToString("yyyy-MM-dd");  //抓今天日期
        public static DateTime Time_Now= DateTime.Now;

        public static List<string> OurList = new List<string>();  //存我的員工有體溫的員工LIST
        public static List<string> CusList = new List<string>();  //存對方員工目前有資料的員工LIST
        public static List<string> DifferentList = new List<string>(); //存員工差集


        static void Main(string[] args)
        {
            try
            {
                if (Time_Now <= DateTime.Parse(DateTime.Now.ToShortDateString() + " 13:30:00"))
                {
                    Console.WriteLine("LAST");
                    SQLConnectDB();
                    OurList.Clear(); CusList.Clear(); DifferentList.Clear();
                    Console.WriteLine("集合已清空");
                    FindDifferentLAST();
                    Console.WriteLine("FindDifferentLAST()已執行完畢");
                    SQLconnectionCUS.Close();SQLconnectionOUR.Close();
                }
                else
                {
                    Console.WriteLine("TODAY");
                    SQLConnectDB();
                    OurList.Clear(); CusList.Clear(); DifferentList.Clear();
                    FindDifferentTODAY();
                    Console.WriteLine("寫入完畢");
                    SQLconnectionCUS.Close(); SQLconnectionOUR.Close();
                }
                Console.Read();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex);
                _logger.Error(ex);
            }
        }
        public static void SQLConnectDB()
        {
            try 
            {
                string sql_our = ConfigurationManager.ConnectionStrings["SQLOUR"].ConnectionString.ToString();
                SQLconnectionOUR = new SqlConnection(sql_our);
                SQLconnectionOUR.Open();
                string sql_cus = ConfigurationManager.ConnectionStrings["SQLCUS"].ConnectionString.ToString();
                SQLconnectionCUS = new SqlConnection(sql_cus);
                SQLconnectionCUS.Open();
                Console.WriteLine("DB Connect Success!");
                _logger.Info("DB Connect Success!");
            }
            catch (Exception ex) 
            {
                _logger.Error(ex);
            }
        }
        public static void FindDifferentLAST() 
        {
            try
            {
                string sql = SQL.SelectDintictFromOGBodyAllFirst;
                ExcuteSqlCmdOurLast(sql);
                string sqlCus = SQL.SelectLastCusInfo;
                ExcuteSqlCmdCusLAST(sqlCus);
                DifferentList = OurList.Except(CusList).ToList();
                int Cus_Count = 0;
                foreach (string Cus_item in CusList)
                {
                    Console.WriteLine(Cus_item.ToString());
                    while (Cus_Count <= CusList.Count - 1)
                    {
                        SqlCommand cmsSerch = new SqlCommand(SQL.SelectFirstInfoLast, SQLconnectionOUR);
                        cmsSerch.Parameters.AddWithValue("@NewNumber", Cus_item);
                        DateTime dateMinnes = Date.AddDays(-1);
                        cmsSerch.Parameters.AddWithValue("@LastDate", Convert.ToDateTime(dateMinnes.ToShortDateString() + " 21:30:00"));
                        cmsSerch.Parameters.AddWithValue("@Today", Convert.ToDateTime(TodayDate.ToString() + " 13:30:00"));
                        SqlDataReader readerLast = cmsSerch.ExecuteReader();
                        int e = 0;
                        while (readerLast.Read())
                        {
                            Global.t_Number = readerLast["t_Number"].ToString();
                            Global.t_Time = Convert.ToDateTime(readerLast["t_Time"]);
                            Global.t_Value = Convert.ToDecimal(readerLast["t_Value"]);
                            Console.WriteLine(Global.t_Number, Global.t_Time, Global.t_Value);
                            e++;
                        }
                        _logger.Info("查詢CUS原有的員工INFO");
                        Console.WriteLine("查詢CUS原有的員工INFO");
                        if (e <= 0)
                        {
                            Console.WriteLine("沒有搜到查詢CUS原有的員工INFO");
                        }
                        readerLast.Close();

                        SqlCommand cmdupdate = new SqlCommand(SQL.UpdateLastCusTable, SQLconnectionCUS);
                        cmdupdate.Parameters.AddWithValue("@NewID", Global.t_Number);
                        cmdupdate.Parameters.AddWithValue("@NewTime02", Global.t_Time);
                        cmdupdate.Parameters.AddWithValue("@NewTemp02", Global.t_Value.ToString());
                        cmdupdate.Parameters.AddWithValue("@LastDate", Convert.ToDateTime(TodayDate.ToString() + " 00:30:00"));
                        cmdupdate.Parameters.AddWithValue("@Today", Convert.ToDateTime(TodayDate.ToString() + " 23:59:00"));
                        int row = cmdupdate.ExecuteNonQuery();
                        Console.WriteLine(row);
                        Cus_Count++;
                    }
                    _logger.Info("搜到查詢CUS原有的員工INFO更新完畢");
                    Console.WriteLine("搜到查詢CUS原有的員工INFO更新完畢");

                }
                if (Cus_Count == 0)
                {
                    _logger.Info("沒有搜到查詢CUS原有的員工INFO");
                    Console.WriteLine("沒有搜到查詢CUS原有的員工INFO");
                }
                int d = 0;
                foreach (string item_Name in DifferentList)
                {
                    while (d <= DifferentList.Count - 1)
                    {
                        SqlCommand cmsSerch = new SqlCommand(SQL.SelectFirstInfoLast, SQLconnectionOUR);
                        cmsSerch.Parameters.AddWithValue("@NewNumber", item_Name);
                        DateTime dateMinnes = Date.AddDays(-1);
                        cmsSerch.Parameters.AddWithValue("@LastDate", Convert.ToDateTime(dateMinnes.ToShortDateString() + " 13:30:00"));
                        cmsSerch.Parameters.AddWithValue("@Today", Convert.ToDateTime(TodayDate.ToString() + " 13:30:00"));
                        SqlDataReader readerLast = cmsSerch.ExecuteReader();
                        int e = 0;
                            while (readerLast.Read())
                            {
                                Global.t_Number = readerLast["t_Number"].ToString();
                                Global.t_Time = Convert.ToDateTime(readerLast["t_Time"]);
                                Global.t_Value = Convert.ToDecimal(readerLast["t_Value"]);
                                e++;
                            }
                            _logger.Info("將差集寫入LIS完畢");
                            if (e <= 0)
                            {
                                Console.WriteLine("沒有搜到關鍵字資料");
                            }
                            readerLast.Close();
                        SqlCommand cmdInsert = new SqlCommand(SQL.InsertDifferentLastIntoCusTable, SQLconnectionCUS);
                        cmdInsert.Parameters.AddWithValue("@NewID", Global.t_Number.ToString());
                        cmdInsert.Parameters.AddWithValue("@NewAlertDate", Date);
                        cmdInsert.Parameters.AddWithValue("@NewEndDate", Date);
                        cmdInsert.Parameters.AddWithValue("@NewDay", "1");
                        cmdInsert.Parameters.AddWithValue("@NewTime01", Global.t_Time);
                        cmdInsert.Parameters.AddWithValue("@NewTemp01", Global.t_Value.ToString());
                        int row = cmdInsert.ExecuteNonQuery();
                        Console.WriteLine(row);
                        d++;
                    }
                    _logger.Info("將差集寫入對方TABLE完畢");
                }
                if (d <= 0)
                {
                    _logger.Info("無找到差集，有體溫員工資料與對方有上傳的員工資料相符");
                    Console.WriteLine("無找到差集，有體溫員工資料與對方有上傳的員工資料相符");
                }
            }
            catch (Exception ex) {
                _logger.Warn("FindDifferentLAST() 執行發生錯誤：" + ex);
            }
        }
        public static void FindDifferentTODAY()
        {
            try
            {
                string sql = SQL.SelectDintictFromOGBodyAllSecond;
                ExcuteSqlCmdOurTOday();
                string sqlCus = SQL.SelectTodayCusInfo;
                ExcuteSqlCmdCusToday(sqlCus);
                DifferentList = OurList.Except(CusList).ToList();
                foreach (string Cus_item in CusList)
                {
                    int Cus_Count = 0;
                    while (Cus_Count < CusList.Count-1 )
                    {
                        SqlCommand cmsSerch = new SqlCommand(SQL.SelectFirstInfoToday, SQLconnectionOUR);
                        cmsSerch.Parameters.AddWithValue("@NewNumber", Cus_item.ToString());
                        Console.WriteLine(Cus_item);
                        cmsSerch.Parameters.AddWithValue("@LastDate", Convert.ToDateTime(TodayDate + " 13:30:00"));
                        cmsSerch.Parameters.AddWithValue("@Today", Convert.ToDateTime(TodayDate + " 21:30:00"));
                        SqlDataReader readerLast = cmsSerch.ExecuteReader();
                        int e = 0;
                        while (readerLast.Read())
                        {
                            Global.t_Number = readerLast["t_Number"].ToString();
                            Global.t_Time = Convert.ToDateTime(readerLast["t_Time"]);
                            Global.t_Value = Convert.ToDecimal(readerLast["t_Value"]);
                            e++;
                        }
                        if (e <= 0)
                        {
                            Console.WriteLine("沒有搜到查詢CUS原有的員工INFO");
                        }
                        readerLast.Close();cmsSerch.Dispose();

                        SqlCommand cmdInsert = new SqlCommand(SQL.UpdateTodayCusTable, SQLconnectionCUS);
                        cmdInsert.Parameters.AddWithValue("@NewTime02", Global.t_Time);
                        cmdInsert.Parameters.AddWithValue("@NewTemp02", Global.t_Value.ToString());
                        cmdInsert.Parameters.AddWithValue("@NewID", Global.t_Number.ToString());
                        cmdInsert.Parameters.AddWithValue("@LastDate", Convert.ToDateTime(TodayDate + " 00:00:00"));
                        cmdInsert.Parameters.AddWithValue("@Today", Convert.ToDateTime(TodayDate + " 23:59:00"));
                        int row = cmdInsert.ExecuteNonQuery();
                        Console.WriteLine(row);
                        _logger.Info("查詢CUS原有的員工INFO");
                        cmdInsert.Dispose();
                        Cus_Count++;
                     }
                    _logger.Info("搜到查詢CUS原有的員工INFO更新完畢");
                    if (Cus_Count <= 0)
                    {
                        _logger.Info("沒有搜到查詢CUS原有的員工INFO");
                        Console.WriteLine("沒有搜到查詢CUS原有的員工INFO");
                    }
                }
                Console.WriteLine("準備到差集");
                
                foreach (string item_Name in DifferentList)
                {
                    int d = 0;
                    Console.WriteLine(item_Name);
                    while (d < DifferentList.Count-1)
                    {
                        SqlCommand cmsSerch = new SqlCommand(SQL.SelectFirstInfoToday, SQLconnectionOUR);
                        cmsSerch.Parameters.AddWithValue("@NewNumber", item_Name.ToString());
                        Console.WriteLine(item_Name);
                        cmsSerch.Parameters.AddWithValue("@LastDate", Convert.ToDateTime(TodayDate + " 13:30:00"));
                        cmsSerch.Parameters.AddWithValue("@Today", Convert.ToDateTime(TodayDate + " 21:30:00"));
                        SqlDataReader readerLast = cmsSerch.ExecuteReader();
                        int e = 0;
                        if (readerLast.HasRows)
                        {
                            while (readerLast.Read())
                            {
                                Global.t_Number = readerLast["t_Number"].ToString();
                                Global.t_Time = Convert.ToDateTime(readerLast["t_Time"]);
                                Global.t_Value = Convert.ToDecimal(readerLast["t_Value"]);
                                e++;
                            }
                        }
                            _logger.Info("將差集寫入LIS完畢");
                        if (e <= 0)
                        {
                            Console.WriteLine("沒有搜到關鍵字資料");
                        }
                        readerLast.Close();cmsSerch.Dispose();

                        SqlCommand cmdInsert = new SqlCommand(SQL.InsertDifferentTodayIntoCusTable, SQLconnectionCUS);
                        cmdInsert.Parameters.AddWithValue("@NewID", Global.t_Number.ToString());
                        cmdInsert.Parameters.AddWithValue("@NewAlertDate",Date);
                        cmdInsert.Parameters.AddWithValue("@NewEndDate", Date);
                        cmdInsert.Parameters.AddWithValue("@NewDay", "1");
                        cmdInsert.Parameters.AddWithValue("@NewTime01", Global.t_Time);
                        cmdInsert.Parameters.AddWithValue("@NewTemp01", Global.t_Value.ToString());
                        int rows = cmdInsert.ExecuteNonQuery();
                        Console.WriteLine(rows);
                        Global.t_Number = "";
                        cmdInsert.Dispose();
                        d++;
                    }
          
                    _logger.Info("將差集寫入對方TABLE完畢");
                    Console.WriteLine("將差集寫入對方TABLE完畢");
                    if (d <= 0)
                    {
                        _logger.Info("無找到差集，有體溫員工資料與對方有上傳的員工資料相符");
                        Console.WriteLine("無找到差集，有體溫員工資料與對方有上傳的員工資料相符");
                    }
                }
            }
            catch (Exception ex) {
                _logger.Error("FindDifferentTODAY方法執行發生錯誤："+ex);
            }
        }
        public  static void ExcuteSqlCmdOurLast(string sql)
        {
            try
            {
                SqlCommand cmdOur = new SqlCommand(sql,SQLconnectionOUR);
                DateTime dateMinnes = Date.AddDays(-1);
                cmdOur.Parameters.AddWithValue("@LastDate", Convert.ToDateTime(dateMinnes.ToShortDateString() + " 21:30:00"));
                cmdOur.Parameters.AddWithValue("@Today", Convert.ToDateTime(TodayDate.ToString() + " 13:30:00"));
                SqlDataReader readerOurLast = cmdOur.ExecuteReader();
                int count = 0;
                if (readerOurLast.HasRows)
                {
                    while (readerOurLast.Read())
                    {
                        OurList.Add(readerOurLast["t_Number"].ToString());
                        count++;
                        Console.WriteLine("讀取目前體溫有的員工資料中_" + readerOurLast["t_Number"].ToString());
                    }
                    if (count <= 0)
                    {
                        Console.WriteLine("未讀取到任何資料");
                        _logger.Info("未讀取到任何資料");
                    }
                    _logger.Info("讀取目前體溫有的員工資料完畢");
                    Console.WriteLine("讀取目前體溫有的員工資料完畢");
                }
                readerOurLast.Close();
                cmdOur.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error("讀取ExcuteSqlCmdOurLast發生錯誤" + ex);
                Console.WriteLine(ex);
            }
        }
        public static void ExcuteSqlCmdOurTOday()
        {
            try
            {
                SqlCommand cmdOurToday = new SqlCommand(SQL.SelectDintictFromOGBodyAllSecond, SQLconnectionOUR);
                cmdOurToday.Parameters.AddWithValue("@LastDate", Convert.ToDateTime(TodayDate+" 13:30:00"));
                cmdOurToday.Parameters.AddWithValue("@Today", Convert.ToDateTime(TodayDate.ToString()+" 21:30:00"));
                Console.WriteLine(TodayDate.ToString());
                SqlDataReader readerOurTOday = cmdOurToday.ExecuteReader();
                int count = 0;
                if (readerOurTOday.HasRows)
                {
                    while (readerOurTOday.Read())
                    {
                        OurList.Add(readerOurTOday["t_Number"].ToString());
                        count++;
                        Console.WriteLine("讀取目前體溫有的員工資料中_" + readerOurTOday["t_Number"].ToString());
                    }
                    if (count <= 0)
                    {
                        Console.WriteLine("未讀取到任何資料");
                        _logger.Info("未讀取到任何資料");
                    }
                    _logger.Info("讀取目前體溫有的員工資料完畢");
                }
                readerOurTOday.Close();
                cmdOurToday.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error("讀取ExcuteSqlCmdOurTOda()" + ex);
                Console.WriteLine(ex);
            }
        }
        public static void ExcuteSqlCmdCusLAST(string sqlCus)
        {
            try
            {
                SqlCommand cmdCus = new SqlCommand(sqlCus, SQLconnectionCUS);
                DateTime dateMinnes = Date.AddDays(-1);
                cmdCus.Parameters.AddWithValue("@LastDate", Convert.ToDateTime(dateMinnes.ToShortDateString() + " 21:30:00"));
                cmdCus.Parameters.AddWithValue("@Today", Convert.ToDateTime(TodayDate.ToString() + " 13:30:00"));
                SqlDataReader readerCus = cmdCus.ExecuteReader();
                int i = 0;
                if (readerCus.HasRows)
                {
                    while (readerCus.Read())
                    {
                        CusList.Add(readerCus["id"].ToString());
                        i++;
                        Console.WriteLine("讀取對方員工資料_" + readerCus["id"]);
                    }
                    if (i <= 0)
                    { _logger.Info("未讀取到對方任何有上傳的員工資料"); }
                    Console.WriteLine("讀取對方有上傳體溫有的員工資料完畢");
                    _logger.Info("讀取對方有上傳體溫有的員工資料完畢");
                }
                readerCus.Close(); cmdCus.Dispose();
            }
            catch (Exception ex) { _logger.Error("讀取ExcuteSqlCmdCusLAST()" + ex); Console.WriteLine(ex); }
        }
        public static void ExcuteSqlCmdCusToday(string sqlCus)
        {
            try
            {
                SqlCommand cmdCus = new SqlCommand(sqlCus, SQLconnectionCUS);
                cmdCus.Parameters.AddWithValue("@LastDate", Convert.ToDateTime(TodayDate.ToString()+" 13:30:00"));
                cmdCus.Parameters.AddWithValue("@Today", Convert.ToDateTime(TodayDate.ToString()+" 21:30:00"));
                SqlDataReader readerCus = cmdCus.ExecuteReader();
                int i = 0;
                if (readerCus.HasRows)
                {
                    while (readerCus.Read())
                    {
                        CusList.Add(readerCus["id"].ToString());
                        i++;
                        Console.WriteLine("讀取對方員工資料_" + readerCus["id"]);
                    }
                    if (i <= 0)
                    { _logger.Info("未讀取到對方任何有上傳的員工資料"); }
                    Console.WriteLine("讀取對方有上傳體溫有的員工資料完畢");
                    _logger.Info("讀取對方有上傳體溫有的員工資料完畢");
                }
                readerCus.Close(); cmdCus.Dispose();
            }
            catch (Exception ex) { _logger.Error("讀取ExcuteSqlCmdCusToday錯誤" + ex); Console.WriteLine(ex); }
        }
    }
}
