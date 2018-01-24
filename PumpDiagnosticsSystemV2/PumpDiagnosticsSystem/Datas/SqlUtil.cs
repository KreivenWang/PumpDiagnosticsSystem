using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using System.Security.AccessControl;
using PumpDiagnosticsSystem.Datas;
using PumpDiagnosticsSystem.Util;
using RedisDemo.Redis;
using VF = PumpDiagnosticsSystem.Datas.SysConstants.VibraFields;

namespace PumpDiagnosticsSystem.Datas
{

    public static class SqlUtil
    {
        private static readonly string strConn = ConfigurationManager.ConnectionStrings["PYWSDbContext"].ConnectionString;
//        private static string psGuid = ConfigurationManager.AppSettings["PSGUID"];
//        private static string psCode = ConfigurationManager.AppSettings["PSCODE"];
        //加载信号量配置
        private static readonly string queryNovibraPhyDef = @"SELECT  [ID]
                                              ,[PDNVCODE]
                                              ,[PDNVNAME]
                                              ,[PDNVTYPE]
                                              ,[TAGNAME]
                                              ,[PSGUID]
                                              ,[PPGUID]
                                              ,[REMARK]
                                          FROM [PHYDEFNOVIBRA]
                                          WHERE PSGUID!=PPGUID AND PSGUID='" + GlobalRepo.PSInfo.PSGuid + "';";

        //加载传感器配置
        private static readonly string querySensor = @"SELECT A.[ID]
                                          ,[SSGUID]
                                          ,[SSNAME]
                                          ,[IP]
                                          ,[NODE]
                                          ,[CHANNEL]
                                          ,[AQSPEED]
                                          ,[SSTGUID]
                                          ,A.[PPGUID]
                                          ,[DIRECTION]
                                          ,[LOCATION]
                                          ,B.CRAFT
                                           FROM [SENSOR] A
                                           LEFT JOIN [PUMP] B ON A.PPGUID=B.PPGUID WHERE B.PSGUID='" + GlobalRepo.PSInfo.PSGuid + "';";

        public static IList<PHYDEFNOVIBRA> GetPhydefNoVibraList()
        {

            SqlConnection connection = new SqlConnection(strConn);
            try {
                SqlDataAdapter sda = new SqlDataAdapter(queryNovibraPhyDef, connection);//创建适配器实例对象

                DataTable dt = new DataTable();//新建数据表实例对象
                sda.FillSchema(dt, SchemaType.Source);//用于自动填入主键
                sda.Fill(dt);//填充数据表


                var list = (from p in dt.AsEnumerable()  //这个list是查出全部的 

                            select new PHYDEFNOVIBRA {
                                ID = p.Field<int>("ID"),
                                PDNVCODE = p.Field<string>("PDNVCODE"),
                                PPGUID = p.Field<Guid>("PPGUID"),
                                PDNVNAME = p.Field<string>("PDNVNAME"),
                                PDNVTYPE = p.Field<string>("PDNVTYPE"),
                                PSGUID = p.Field<Guid>("PSGUID"),
                                REMARK = p.Field<string>("REMARK"),
                                ITEMCODE = "$" + p.Field<string>("REMARK") + "_" + p.Field<Guid>("PPGUID")
                            }).ToList(); //将这个集合转换成list



                return list;
            } 
            finally {
                connection.Close();//关闭数据库,可以在打开
                connection.Dispose();//关闭数据库，释放控件，不可在连接。
            }
        }


        public static IList<SENSOR> GetSensorList()
        {

            SqlConnection connection = new SqlConnection(strConn);
            try {
                SqlDataAdapter sda = new SqlDataAdapter(querySensor, connection);//创建适配器实例对象

                DataTable dt = new DataTable();//新建数据表实例对象
                sda.FillSchema(dt, SchemaType.Source);//用于自动填入主键
                sda.Fill(dt);//填充数据表


                var list = (from p in dt.AsEnumerable()  //这个list是查出全部的用户评论  

                            select new SENSOR {
                                AQSPEED = p.Field<int>("AQSPEED"), //p.Filed<int>("LibId") 其实就是获取DataRow中ID列。即：row["ID"]  
                                CHANNEL = p.Field<int>("CHANNEL"),
                                DIRECTION = p.Field<string>("DIRECTION"),
                                ID = p.Field<int>("ID"),
                                IP = p.Field<string>("IP"),
                                LOCATION = p.Field<string>("LOCATION"),
                                NODE = p.Field<int>("NODE"),
                                PPGUID = p.Field<Guid>("PPGUID"),
                                SSGUID = p.Field<Guid>("SSGUID"),
                                SSNAME = p.Field<string>("SSNAME"),
                                ITEMCODE = "$" + SysConstants.SENSORSETTING[p.Field<string>("LOCATION") + "_" + p.Field<string>("DIRECTION")] + "_" + p.Field<Guid>("PPGUID")
                            }).ToList(); //将这个集合转换成list



                return list;
            }
            //  catch (Exception ex)
            // {
            //    throw ex;
            //}
            finally {
                connection.Close();//关闭数据库,可以在打开
                connection.Dispose();//关闭数据库，释放控件，不可在连接。
            }
        }


        public static void GetDatFromSqlToRedis(string datetime, IList<SENSOR> sensorlist, Dictionary<string, IEnumerable<string>> phyDefNoVibra, Dictionary<string, string> _pumpRun, StringBuilder SBphynovibra, StringBuilder SBphyvibra, StringBuilder SBpumprun)
        {
            const string bssc = "BSSC";
            const string shiyan = "SHIYAN";
            var psCode = GlobalRepo.PSInfo.PSCode;

            var isNeedPhase = psCode.ToUpper() == bssc || psCode.ToUpper() == shiyan;

            var timeStr = DateTime.Parse(datetime).ToString("yyyy-MM-dd HH:mm:ss");

            var v3PhaseField = isNeedPhase ? VF.V3Phase : "''";

            var sql =
                $@"SELECT  *
FROM  [dbo].[HISTORYDATA_{psCode}] 
WHERE PICKDATE='{timeStr}';

SELECT {VF.SSGuid},{VF.PPGuid}, {VF.PickDate}, {VF.Overall},{VF.Speed},{VF.V1mag},{VF.V2mag},{VF.V3mag},{VF.V1Phase},{VF.V2Phase},{v3PhaseField} as {VF.V3Phase}
FROM  [VIBRADATA_{psCode}] B
WHERE B.PICKDATE='{timeStr}'";

            SqlConnection connection = new SqlConnection(strConn);
            SqlDataAdapter sda = new SqlDataAdapter(sql, connection);//创建适配器实例对象
            DataSet ds = new DataSet();
            sda.Fill(ds);
            var dt = ds.Tables[0];
            var dt2 = ds.Tables[1];

            //RedisManager.GetClient()报错的话, 问题一般是: Config里的type后面没有设置成Redis.Demo所在程序集的名称, 此处是PumpDiagnosticsSystem
            using (var redisClient = RedisManager.GetClient()) { 

                for (int i = 0; i < dt.Rows.Count; i++) {
                    string ppguid = dt.Rows[i]["PPGUID"].ToString();
                    //var dd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    redisClient.Set("{" + ppguid.ToUpper() + "}_PICKDATE", timeStr);
                    SBpumprun.AppendLine(ppguid + ":" + timeStr);
                    //如果没有运行设置power为0
                    if (!isNeedPhase) {
                        //数据库记录里THSC的表混入了不对的PPGUID 所以这里做个判断
                        if (_pumpRun.Any() && _pumpRun.ContainsKey(ppguid)) {
                            if (dt.Rows[i][_pumpRun[ppguid]].ToString() == "0") {
                                redisClient.Set("{" + ppguid.ToUpper() + "}_Power", 0);
                                SBpumprun.AppendLine(ppguid + "机泵没运行");
                            } else {
                                redisClient.Set("{" + ppguid.ToUpper() + "}_Power", 1);
                                SBpumprun.AppendLine(ppguid + "机泵运行");
                            }
                            foreach (var item in phyDefNoVibra[ppguid]) {
                                redisClient.Set("{" + ppguid.ToUpper() + "}_" + item, dt.Rows[i][item]);
                                SBphynovibra.AppendLine("{" + ppguid + "}_" + item + ":" + dt.Rows[i][item]);
                            }
                        }
                    }
                }

                

                foreach (DataRow dataRow in dt2.Rows) {
                    
                }

                var vibDatas = from p in dt2.AsEnumerable()
                    select new {
                        SSGUID = p.Field<Guid>(VF.SSGuid),
                        PickDate = p.Field<DateTime>(VF.PickDate),
                        OVERALL = p.Field<double>(VF.Overall),
                        SPEED = p.Field<double>(VF.Speed),
                        PPGUID = p.Field<Guid>(VF.PPGuid),
                        V1mag = p.Field<double>(VF.V1mag),
                        V2mag = p.Field<double>(VF.V2mag),
                        V3mag = p.Field<double>(VF.V3mag),
                        V1Phase = p.Field<double>(VF.V1Phase),
                        V2Phase = p.Field<double>(VF.V2Phase),

                        //有时候V3Phase是"", 读出来就是string类型
                        //如果V3Phase是有值的,粗出来就是double类型
                        V3Phase = p.Field<object>(VF.V3Phase) is string ? -1D : p.Field<double>(VF.V3Phase),
                    };

                var filePath = ConfigurationManager.AppSettings["FilePath"];
                var gateway = ConfigurationManager.AppSettings["gateway"];



                foreach (var item in vibDatas) {

                    redisClient.Set("{" + item.SSGUID.ToString().ToUpper() + "}_OVERALL", item.OVERALL);
                    redisClient.Set("{" + item.SSGUID.ToString().ToUpper() + "}_PICKDATE", timeStr);
                    redisClient.Set("{" + item.PPGUID.ToString().ToUpper() + "}_SPEED", item.SPEED); //item.SPEED);
                    redisClient.Set("{" + item.PPGUID.ToString().ToUpper() + "}_SPEED_PICKDATE", timeStr);


                    redisClient.Set(("{" + item.SSGUID + "}_" + VF.V2mag).ToUpper(), item.V2mag);
                    redisClient.Set(("{" + item.SSGUID + "}_" + VF.V2Phase).ToUpper(), item.V2Phase);
                    redisClient.Set(("{" + item.SSGUID + "}_" + VF.V1mag).ToUpper(), item.V1mag);
                    redisClient.Set(("{" + item.SSGUID + "}_" + VF.V1Phase).ToUpper(), item.V1Phase);
                    redisClient.Set(("{" + item.SSGUID + "}_" + VF.V3mag).ToUpper(), item.V3mag);
                    redisClient.Set(("{" + item.SSGUID + "}_" + VF.V3Phase).ToUpper(), item.V3Phase);

                    SBpumprun.AppendLine(item.PPGUID + "转速" + item.SPEED);


                    if (isNeedPhase) {
                        if (item.SPEED > 50) {
                            redisClient.Set("{" + item.PPGUID.ToString().ToUpper() + "}_Power", 1);
                            SBpumprun.AppendLine(item.PPGUID + "机泵运行");
                        } else {
                            redisClient.Set("{" + item.PPGUID.ToString().ToUpper() + "}_Power", 0);
                            SBpumprun.AppendLine(item.PPGUID + "机泵没运行");
                        }
                    }

                    var sensor = sensorlist.FirstOrDefault(p => p.SSGUID == item.SSGUID);
                    if (sensor == null)
                        continue;

                    var url = filePath + @"\" + psCode + @"\" + item.PickDate.Year + @"\" + +item.PickDate.Month + @"\" + +item.PickDate.Day + (string.IsNullOrEmpty(gateway) ? "" : (@"\" + gateway));
                    url = url + @"\" + sensor.NODE + "_" + sensor.CHANNEL + "_" + (item.PickDate.Hour < 10 ? ("0" + item.PickDate.Hour) : (item.PickDate.Hour.ToString())) + "." + (item.PickDate.Minute < 10 ? ("0" + item.PickDate.Minute) : (item.PickDate.Minute.ToString())) + ".00_timewave.txt";
                    if (File.Exists(url)) {

                        FileStream aFile = null;
                        StreamReader sr = null;
                        string txtcontent = "";
                        try {
                            aFile = new FileStream(url, FileMode.Open);
                            sr = new StreamReader(aFile);
                            txtcontent = sr.ReadToEnd();

                        } finally {
                            sr.Close();
                            aFile.Close();

                        }
                        if (string.IsNullOrEmpty(txtcontent))
                            throw new Exception($"文件内容为空 {url}");
                        var values = txtcontent.Split('[')[1].Split(']')[0].Trim();
                        //   values = "0," + values;
                        redisClient.Set("{" + item.SSGUID.ToString().ToUpper() + "}_WaveTime", String.Format("{1}|{{Period: 0.20000, AmpliateRef: 1.12098, Amplitude: [{0}]}}", values, item.PickDate.ToString("yyyy-MM-dd HH:mm:ss")));
                        SBphyvibra.AppendLine("{" + item.SSGUID.ToString().ToUpper() + "}_WaveTime:" + values);

                    }


                    url = url.Replace("timewave", "spec");
                    if (File.Exists(url)) {
                        FileStream aFile = null;
                        StreamReader sr = null;
                        string txtcontent = "";
                        try {
                            aFile = new FileStream(url, FileMode.Open);
                            sr = new StreamReader(aFile);
                            txtcontent = sr.ReadToEnd();

                        } finally {
                            sr.Close();
                            aFile.Close();

                        }
                        if (string.IsNullOrEmpty(txtcontent))
                            throw new Exception($"文件内容为空 {url}");
                        var values = txtcontent.Split('[')[1].Split(']')[0].Trim();
                        values = "0," + values;
                        redisClient.Set("{" + item.SSGUID.ToString().ToUpper() + "}_Spec", String.Format("{1}|{{FMax: {2},Amplitude: [{0}]}}", values, item.PickDate.ToString("yyyy-MM-dd HH:mm:ss"), txtcontent.Split('[')[0].Split(',')[0].Split(':')[1]));
                        SBphyvibra.AppendLine("{" + item.SSGUID.ToString().ToUpper() + "}_Spec:" + values);
                    }

                }

                redisClient.Set("IntouchUpdateTime", timeStr);

            }

        }
    }
}



/// <summary>
/// 传感器
/// </summary>
public class SENSOR
{

    public SENSOR()
    {

        //  ITEMCODE = SysConstants.SENSORSETTING[LOCATION + "_" + DIRECTION] + "_" + PPGUID;
    }
    /// <summary>
    /// ID
    /// </summary>
    public int ID { get; set; }
    /// <summary>
    /// SSGUID
    /// </summary>
    public Guid SSGUID { get; set; }
    /// <summary>
    /// 传感器名称
    /// </summary>
    public string SSNAME { get; set; }
    /// <summary>
    /// 模块
    /// </summary>
    public int NODE { get; set; }
    /// <summary>
    /// IP
    /// </summary>
    public string IP { get; set; }
    /// <summary>
    /// 通道
    /// </summary>
    public int CHANNEL { get; set; }

    //是否采转速 1采  0不采
    public Nullable<int> AQSPEED { get; set; }

    /// <summary>
    /// 关联机泵
    /// </summary>
    public Guid PPGUID { get; set; }
    /// <summary>
    /// 方向
    /// </summary>
    public string DIRECTION { get; set; }
    /// <summary>
    /// 安装位置
    /// </summary>
    public string LOCATION { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    public Nullable<int> STATUS { get; set; }
    /// <summary>
    /// 编号
    /// </summary>
    public string ITEMCODE { get; set; }

}




/// <summary>
/// 变量-振动类
/// </summary>
/// <summary>
/// 变量PLC
/// </summary>
public class PHYDEFNOVIBRA
{

    public PHYDEFNOVIBRA()
    {


    }
    /// <summary>
    /// ID
    /// </summary>
    public int ID { get; set; }
    /// <summary>
    /// 信号量
    /// </summary>
    public Guid PDNVGUID { get; set; }
    /// <summary>
    /// 变量编号
    /// </summary>
    public string PDNVCODE { get; set; }
    /// <summary>
    /// 变量名称
    /// </summary>
    public string PDNVNAME { get; set; }
    /// <summary>
    /// 单位
    /// </summary>
    public string UNIT { get; set; }
    /// <summary>
    /// 类型
    /// </summary>
    public string PDNVTYPE { get; set; }
    /// <summary>
    /// 对应取值编号
    /// </summary>
    public string TAGNAME { get; set; }

    /// <summary>
    /// 关联泵站表
    /// </summary>
    public Guid PSGUID { get; set; }
    /// <summary>
    /// 关联机泵表
    /// </summary>
    public Guid PPGUID { get; set; }
    /// <summary>
    /// 编号
    /// </summary>
    public string ITEMCODE { get; set; }

    public string REMARK { get; set; }
}

public class UtilDataTable
{

    public static IList<T> ConvertTo<T>(DataTable table)
    {
        if (table == null) {
            return null;
        }

        List<DataRow> rows = new List<DataRow>();

        foreach (DataRow row in table.Rows) {
            rows.Add(row);
        }

        return ConvertTo<T>(rows);
    }

    public static IList<T> ConvertTo<T>(IList<DataRow> rows)
    {
        IList<T> list = null;

        if (rows != null) {
            list = new List<T>();

            foreach (DataRow row in rows) {
                T item = CreateItem<T>(row);
                list.Add(item);
            }
        }

        return list;
    }

    public static T CreateItem<T>(DataRow row)
    {
        T obj = default(T);
        if (row != null) {
            obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in row.Table.Columns) {
                PropertyInfo prop = obj.GetType().GetProperty(column.ColumnName);
                try {
                    object value = row[column.ColumnName];
                    prop.SetValue(obj, value, null);
                } catch {  //You can log something here     
                           //throw;    
                }
            }
        }

        return obj;
    }

}
