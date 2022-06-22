using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorGuard_Temperature 
{
    public class SQL 
    {
        //撈出OGBody2130-1330總員工工號細項扣掉重複
        public static string SelectDintictFromOGBodyAllFirst = "SELECT distinct b.t_Number FROM OGBodyTemperature as a INNER JOIN OGEmp as b ON a.t_EmpPK = b.t_PK where a.t_Time> @LastDate  and a.t_Time< @Today ;";

        //撈出OGBody1330-2130總員工工號細項扣掉重複
        public static string SelectDintictFromOGBodyAllSecond = "SELECT distinct b.t_Number FROM OGBodyTemperature as a INNER JOIN OGEmp as b ON a.t_EmpPK = b.t_PK where a.t_Time> @LastDate and a.t_Time<@Today;";
        //撈出OGBody2130-1330單一員工條件內最早一筆資料
        public static string SelectFirstInfoLast = "SELECT top(1) b.t_Number,a.t_Value,a.t_Time FROM OGBodyTemperature as a INNER JOIN OGEmp as b ON a.t_EmpPK = b.t_PK where b.t_Number =@NewNumber and a.t_Time>@LastDate and a.t_Time<@Today order by a.t_Time; ";

        //撈出OGBody1330-2130單一員工條件內最早一筆資料
        public static string SelectFirstInfoToday = "SELECT top(1) b.t_Number,a.t_Value,a.t_Time FROM OGBodyTemperature as a INNER JOIN OGEmp as b ON a.t_EmpPK = b.t_PK  where b.t_Number =@NewNumber and a.t_Time>@LastDate and a.t_Time<@Today order by a.t_Time; ";

        //撈取對方目前所有有資料的員工的List
        public static string SelectLastCusInfo = "select*from Temperature where alert_date>@LastDate and alert_date <@Today   ";
        //撈取對方目前所有有資料的員工的List
        public static string SelectTodayCusInfo = "select*from Temperature where alert_date>@LastDate and alert_date <@Today";
        //將差集寫入對方TABLE(First)
        public static string InsertDifferentLastIntoCusTable = "Insert Into Temperature values (@NewID,@NewAlertDate,@NewEndDate,@NewDay,@NewTime01,@NewTemp01,NULL,NULL) ;";
        //將差集寫入對方TABLE(Second)
        public static string InsertDifferentTodayIntoCusTable = "Insert Into Temperature values (@NewID,@NewAlertDate,@NewEndDate,@NewDay,@NewTime01,@NewTemp01,NULL,NULL);";
        //將原有的UPDATE_TODAY
        public static string UpdateTodayCusTable = "update Temperature set time2=@NewTime02,temp2=@NewTemp02 where id=@NewID and time1 >@LastDate and  time1 <@Today ;";
        //將原有的UPDATE_LAST
        public static string UpdateLastCusTable = "update Temperature set time2=@NewTime02,temp2=@NewTemp02 where id=@NewID and time1 >@LastDate and time1  <@Today;";
    }
}