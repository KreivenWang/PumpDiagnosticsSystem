using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using PumpDiagnosticsSystem.Datas;
using RedisDemo.Redis;

namespace PumpDiagnosticsSystem.Datas
{

    public static class SqlUtil
    {
        private static string strConn = ConfigurationManager.ConnectionStrings["PYWSDbContext"].ConnectionString;
        private static string psGuid = ConfigurationManager.AppSettings["PSGUID"];
        private static string psCode = ConfigurationManager.AppSettings["PSCODE"];
        //加载信号量配置
        private static string queryNovibraPhyDef = @"SELECT  [ID]
                                              ,[PDNVCODE]
                                              ,[PDNVNAME]
                                              ,[PDNVTYPE]
                                              ,[TAGNAME]
                                              ,[PSGUID]
                                              ,[PPGUID]
                                              ,[REMARK]
                                          FROM [PHYDEFNOVIBRA]
                                          WHERE PSGUID!=PPGUID AND PSGUID='" + psGuid + "';";

        //加载传感器配置
        private static string querySensor = @"SELECT A.[ID]
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
                                           LEFT JOIN [PUMP] B ON A.PPGUID=B.PPGUID WHERE B.PSGUID='" + psGuid + "';";
        private static string queryGetData = @"SELECT  *
                                      FROM  [dbo].[HISTORYDATA_{1}] 
                                        WHERE PICKDATE='{0}' AND PPGUID!='1C0989D1-999A-41EC-8118-8059B519AB3C' 
                                          AND PPGUID!='2D0AA667-F249-42F3-94BB-77B6BFEE7752'  
                                          AND PPGUID!='3D0AA667-F249-42F3-94BB-77B6BFEE7752';
    
                                        SELECT SSGUID,PPGUID, PickDate, OVERALL,SPEED,PPGUID  
                                      FROM  [VIBRADATA_{1}] B
                                      WHERE B.PICKDATE='{0}' AND B.PPGUID!='1C0989D1-999A-41EC-8118-8059B519AB3C'
                                          AND PPGUID!='2D0AA667-F249-42F3-94BB-77B6BFEE7752'  
                                          AND PPGUID!='3D0AA667-F249-42F3-94BB-77B6BFEE7752'";

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


        public static void GetDatas(string datetime)
        {

            SqlConnection connection = new SqlConnection(strConn);

            SqlDataAdapter sda = new SqlDataAdapter(String.Format(queryGetData, datetime), connection);//创建适配器实例对象

            DataSet ds = new DataSet();
            sda.Fill(ds);
            var dt = ds.Tables[0];
            var dt2 = ds.Tables[1];
            var list = (from p in dt.Select("PPGUID='A67C1178-3B96-412F-BBEF-045B926F3904'").AsEnumerable()  //这个list是查出全部的用户评论  

                        select new
                        {
                            A001 = p.Field<double>("A001"),
                            A002 = p.Field<double>("A002"),
                            A003 = p.Field<double>("A003"),
                            A004 = p.Field<double>("A004"),
                            A005 = p.Field<double>("A005"),
                            A006 = p.Field<double>("A006"),
                            A007 = p.Field<double>("A007"),
                            A008 = p.Field<double>("A008"),
                            A009 = p.Field<double>("A009"),
                            A010 = p.Field<double>("A010"),
                            A011 = p.Field<double>("A011"),
                            A012 = p.Field<double>("A012"),
                            A013 = p.Field<double>("A013"),
                            A014 = p.Field<double>("A014"),
                            A015 = p.Field<double>("A015"),
                            A016 = p.Field<double>("A016"),
                            A017 = p.Field<double>("A017"),
                            A018 = p.Field<double>("A018"),
                            A019 = p.Field<double>("A019")
                        });


            var list2 = (from p in dt.Select("PPGUID='B928FAE9-BAE2-4BC1-86F4-5406FE4F4915'").AsEnumerable()  //这个list是查出全部的用户评论  

                         select new
                         {

                             A020 = p.Field<double>("A020"),
                             A021 = p.Field<double>("A021"),
                             A022 = p.Field<double>("A022"),
                             A023 = p.Field<double>("A023"),
                             A024 = p.Field<double>("A024"),
                             A025 = p.Field<double>("A025"),
                             A026 = p.Field<double>("A026"),
                             A027 = p.Field<double>("A027"),
                             A028 = p.Field<double>("A028"),
                             A029 = p.Field<double>("A029"),
                             A030 = p.Field<double>("A030"),
                             A031 = p.Field<double>("A031"),
                             A032 = p.Field<double>("A032"),
                             A033 = p.Field<double>("A033"),
                             A034 = p.Field<double>("A034"),
                             A035 = p.Field<double>("A035"),
                             A036 = p.Field<double>("A036"),
                             A037 = p.Field<double>("A037"),
                             A038 = p.Field<double>("A038"),
                         });


            var list3 = (from p in dt.Select("PPGUID='C17B0B47-39C2-4382-8F19-31B3D6D91F97'").AsEnumerable()  //这个list是查出全部的用户评论  

                         select new
                         {


                             A039 = p.Field<double>("A039"),
                             A040 = p.Field<double>("A040"),
                             A041 = p.Field<double>("A041"),
                             A042 = p.Field<double>("A042"),
                             A043 = p.Field<double>("A043"),
                             A044 = p.Field<double>("A044"),
                             A045 = p.Field<double>("A045"),
                             A046 = p.Field<double>("A046"),
                             A047 = p.Field<double>("A047"),
                             A048 = p.Field<double>("A048"),
                             A049 = p.Field<double>("A049"),
                             A050 = p.Field<double>("A050"),
                             A051 = p.Field<double>("A051"),
                             A052 = p.Field<double>("A052"),
                             A053 = p.Field<double>("A053"),
                             A054 = p.Field<double>("A054"),
                             A055 = p.Field<double>("A055"),
                             A056 = p.Field<double>("A056"),
                             A057 = p.Field<double>("A057")

                         });

            var list4 = (from p in dt.Select("PPGUID='D12980BC-1884-4AE7-94AE-B7239016F884'").AsEnumerable()  //这个list是查出全部的用户评论  

                         select new
                         {



                             A058 = p.Field<double>("A058"),
                             A059 = p.Field<double>("A059"),
                             A060 = p.Field<double>("A060"),
                             A061 = p.Field<double>("A061"),
                             A062 = p.Field<double>("A062"),
                             A063 = p.Field<double>("A063"),
                             A064 = p.Field<double>("A064"),
                             A065 = p.Field<double>("A065"),
                             A066 = p.Field<double>("A066"),
                             A067 = p.Field<double>("A067"),
                             A068 = p.Field<double>("A068"),
                             A069 = p.Field<double>("A069"),
                             A070 = p.Field<double>("A070"),
                             A071 = p.Field<double>("A071"),
                             A072 = p.Field<double>("A072"),
                             A073 = p.Field<double>("A073"),
                             A074 = p.Field<double>("A074"),
                             A075 = p.Field<double>("A075"),
                             A076 = p.Field<double>("A076")
                         });













            var list5 = (from p in dt2.AsEnumerable()  //这个list是查出全部的用户评论  

                         select new
                         {
                             SSGUID = p.Field<Guid>("SSGUID"),
                             PickDate = p.Field<DateTime>("PickDate"),
                             OVERALL = p.Field<double>("OVERALL"),
                             SPEED = p.Field<double>("SPEED"),
                             PPGUID = p.Field<Guid>("PPGUID")
                         });

            using (var redisClient = RedisManager.GetClient()) {
                var guid = "A67C1178-3B96-412F-BBEF-045B926F3904";
                if (list.FirstOrDefault() == null || list2.FirstOrDefault() == null || list3.FirstOrDefault() == null || list4.FirstOrDefault() == null) {
                    return;
                }
                redisClient.Set("{" + guid + "}_A001", list.FirstOrDefault().A001);
                redisClient.Set("{" + guid + "}_A002", list.FirstOrDefault().A002);
                redisClient.Set("{" + guid + "}_A003", list.FirstOrDefault().A003);
                redisClient.Set("{" + guid + "}_A004", list.FirstOrDefault().A004);
                redisClient.Set("{" + guid + "}_A005", list.FirstOrDefault().A005);
                redisClient.Set("{" + guid + "}_A006", list.FirstOrDefault().A006);
                redisClient.Set("{" + guid + "}_A007", list.FirstOrDefault().A007);
                redisClient.Set("{" + guid + "}_A008", list.FirstOrDefault().A008);
                redisClient.Set("{" + guid + "}_A009", list.FirstOrDefault().A009);
                redisClient.Set("{" + guid + "}_A010", list.FirstOrDefault().A010);
                redisClient.Set("{" + guid + "}_A011", list.FirstOrDefault().A011);
                redisClient.Set("{" + guid + "}_A012", list.FirstOrDefault().A012);
                redisClient.Set("{" + guid + "}_A013", list.FirstOrDefault().A013);
                redisClient.Set("{" + guid + "}_A014", list.FirstOrDefault().A014);
                redisClient.Set("{" + guid + "}_A015", list.FirstOrDefault().A015);
                redisClient.Set("{" + guid + "}_A016", list.FirstOrDefault().A016);
                redisClient.Set("{" + guid + "}_A017", list.FirstOrDefault().A017);
                redisClient.Set("{" + guid + "}_A018", list.FirstOrDefault().A018);
                redisClient.Set("{" + guid + "}_A019", list.FirstOrDefault().A019);

                guid = "B928FAE9-BAE2-4BC1-86F4-5406FE4F4915";
                redisClient.Set("{" + guid + "}_A020", list2.FirstOrDefault().A020);
                redisClient.Set("{" + guid + "}_A021", list2.FirstOrDefault().A021);
                redisClient.Set("{" + guid + "}_A022", list2.FirstOrDefault().A022);
                redisClient.Set("{" + guid + "}_A023", list2.FirstOrDefault().A023);
                redisClient.Set("{" + guid + "}_A024", list2.FirstOrDefault().A024);
                redisClient.Set("{" + guid + "}_A025", list2.FirstOrDefault().A025);
                redisClient.Set("{" + guid + "}_A026", list2.FirstOrDefault().A026);
                redisClient.Set("{" + guid + "}_A027", list2.FirstOrDefault().A027);
                redisClient.Set("{" + guid + "}_A028", list2.FirstOrDefault().A028);
                redisClient.Set("{" + guid + "}_A029", list2.FirstOrDefault().A029);
                redisClient.Set("{" + guid + "}_A030", list2.FirstOrDefault().A030);
                redisClient.Set("{" + guid + "}_A031", list2.FirstOrDefault().A031);
                redisClient.Set("{" + guid + "}_A032", list2.FirstOrDefault().A032);
                redisClient.Set("{" + guid + "}_A033", list2.FirstOrDefault().A033);
                redisClient.Set("{" + guid + "}_A034", list2.FirstOrDefault().A034);
                redisClient.Set("{" + guid + "}_A035", list2.FirstOrDefault().A035);
                redisClient.Set("{" + guid + "}_A036", list2.FirstOrDefault().A036);
                redisClient.Set("{" + guid + "}_A037", list2.FirstOrDefault().A037);
                redisClient.Set("{" + guid + "}_A038", list2.FirstOrDefault().A038);


                guid = "C17B0B47-39C2-4382-8F19-31B3D6D91F97";
                redisClient.Set("{" + guid + "}_A039", list3.FirstOrDefault().A039);
                redisClient.Set("{" + guid + "}_A040", list3.FirstOrDefault().A040);
                redisClient.Set("{" + guid + "}_A041", list3.FirstOrDefault().A041);
                redisClient.Set("{" + guid + "}_A042", list3.FirstOrDefault().A042);
                redisClient.Set("{" + guid + "}_A043", list3.FirstOrDefault().A043);
                redisClient.Set("{" + guid + "}_A044", list3.FirstOrDefault().A044);
                redisClient.Set("{" + guid + "}_A045", list3.FirstOrDefault().A045);
                redisClient.Set("{" + guid + "}_A046", list3.FirstOrDefault().A046);
                redisClient.Set("{" + guid + "}_A047", list3.FirstOrDefault().A047);
                redisClient.Set("{" + guid + "}_A048", list3.FirstOrDefault().A048);
                redisClient.Set("{" + guid + "}_A049", list3.FirstOrDefault().A049);
                redisClient.Set("{" + guid + "}_A050", list3.FirstOrDefault().A050);
                redisClient.Set("{" + guid + "}_A051", list3.FirstOrDefault().A051);
                redisClient.Set("{" + guid + "}_A052", list3.FirstOrDefault().A052);
                redisClient.Set("{" + guid + "}_A053", list3.FirstOrDefault().A053);
                redisClient.Set("{" + guid + "}_A054", list3.FirstOrDefault().A054);
                redisClient.Set("{" + guid + "}_A055", list3.FirstOrDefault().A055);
                redisClient.Set("{" + guid + "}_A056", list3.FirstOrDefault().A056);
                redisClient.Set("{" + guid + "}_A057", list3.FirstOrDefault().A057);


                guid = "D12980BC-1884-4AE7-94AE-B7239016F884";
                redisClient.Set("{" + guid + "}_A058", list4.FirstOrDefault().A058);
                redisClient.Set("{" + guid + "}_A059", list4.FirstOrDefault().A059);
                redisClient.Set("{" + guid + "}_A060", list4.FirstOrDefault().A060);
                redisClient.Set("{" + guid + "}_A061", list4.FirstOrDefault().A061);
                redisClient.Set("{" + guid + "}_A062", list4.FirstOrDefault().A062);
                redisClient.Set("{" + guid + "}_A063", list4.FirstOrDefault().A063);
                redisClient.Set("{" + guid + "}_A064", list4.FirstOrDefault().A064);
                redisClient.Set("{" + guid + "}_A065", list4.FirstOrDefault().A065);
                redisClient.Set("{" + guid + "}_A066", list4.FirstOrDefault().A066);
                redisClient.Set("{" + guid + "}_A067", list4.FirstOrDefault().A067);
                redisClient.Set("{" + guid + "}_A068", list4.FirstOrDefault().A068);
                redisClient.Set("{" + guid + "}_A069", list4.FirstOrDefault().A069);
                redisClient.Set("{" + guid + "}_A070", list4.FirstOrDefault().A070);
                redisClient.Set("{" + guid + "}_A071", list4.FirstOrDefault().A071);
                redisClient.Set("{" + guid + "}_A072", list4.FirstOrDefault().A072);
                redisClient.Set("{" + guid + "}_A073", list4.FirstOrDefault().A073);
                redisClient.Set("{" + guid + "}_A074", list4.FirstOrDefault().A074);
                redisClient.Set("{" + guid + "}_A075", list4.FirstOrDefault().A075);
                redisClient.Set("{" + guid + "}_A076", list4.FirstOrDefault().A076);



                var filePath = ConfigurationManager.AppSettings["FilePath"];
                var gateway = ConfigurationManager.AppSettings["gateway"];
                var sensorlist = GetSensorList();
                foreach (var item in list5) {
                    redisClient.Set("{" + item.SSGUID.ToString().ToUpper() + "}_OVERALL", item.OVERALL);
                    redisClient.Set("{" + item.SSGUID.ToString().ToUpper() + "}_PICKDATE", item.PickDate);
                    redisClient.Set("{" + item.PPGUID.ToString().ToUpper() + "}_SPEED", item.SPEED);
                    var sensor = sensorlist.Where(p => p.SSGUID == item.SSGUID).FirstOrDefault();

                    if (sensor == null)
                        continue;

                    var url = filePath + @"\" + psCode + @"\" + item.PickDate.Year + @"\" + +item.PickDate.Month + @"\" + +item.PickDate.Day + @"\" + gateway;
                    url = url + @"\" + sensor.NODE + "_" + sensor.CHANNEL + "_" + (item.PickDate.Hour < 10 ? ("0" + item.PickDate.Hour) : (item.PickDate.Hour.ToString())) + "." + (item.PickDate.Minute < 10 ? ("0" + item.PickDate.Minute) : (item.PickDate.Minute.ToString())) + ".00_timewave.txt";
                    if (System.IO.File.Exists(url)) {

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
                        var values = txtcontent.Split('[')[1].Split(']')[0].Trim();
                        //   values = "0," + values;
                        redisClient.Set("{" + item.SSGUID.ToString().ToUpper() + "}_WaveTime", values);

                    }


                    url = url.Replace("timewave", "spec");
                    if (System.IO.File.Exists(url)) {
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
                        var values = txtcontent.Split('[')[1].Split(']')[0].Trim();
                        values = "0," + values;
                        redisClient.Set("{" + item.SSGUID.ToString().ToUpper() + "}_Spec", values);

                    }

                }

                redisClient.Set("IntouchUpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));







            }
        }


        public static string GetDatFromSqlToRedis(string datetime, IList<SENSOR> sensorlist, Dictionary<string, IEnumerable<string>> phyDefNoVibra)
        {
            StringBuilder sb = new StringBuilder();
            SqlConnection connection = new SqlConnection(strConn);

            SqlDataAdapter sda = new SqlDataAdapter(String.Format(queryGetData, datetime, psCode), connection);//创建适配器实例对象

            DataSet ds = new DataSet();
            sda.Fill(ds);
            var dt = ds.Tables[0];
            var dt2 = ds.Tables[1];

            using (var redisClient = RedisManager.GetClient()) {


                for (int i = 0; i < dt.Rows.Count; i++) {
                    string ppguid = dt.Rows[i]["PPGUID"].ToString();
                    foreach (var item in phyDefNoVibra[ppguid]) {
                        redisClient.Set("{" + ppguid + "}_" + item, dt.Rows[i][item]);
                        sb.AppendLine("{" + ppguid + "}_" + item + ":" + dt.Rows[i][item]);
                    }
                }


                var list6 = (from p in dt2.AsEnumerable()  //这个list是查出全部的用户评论  

                             select new
                             {
                                 SSGUID = p.Field<Guid>("SSGUID"),
                                 PickDate = p.Field<DateTime>("PickDate"),
                                 OVERALL = p.Field<double>("OVERALL"),
                                 SPEED = p.Field<double>("SPEED"),
                                 PPGUID = p.Field<Guid>("PPGUID")
                             });






                var filePath = ConfigurationManager.AppSettings["FilePath"];
                var gateway = ConfigurationManager.AppSettings["gateway"];

                foreach (var item in list6) {
                    redisClient.Set("{" + item.SSGUID.ToString().ToUpper() + "}_OVERALL", item.OVERALL);
                    redisClient.Set("{" + item.SSGUID.ToString().ToUpper() + "}_PICKDATE", item.PickDate);
                    redisClient.Set("{" + item.PPGUID.ToString().ToUpper() + "}_SPEED", item.SPEED); //item.SPEED);
                    sb.AppendLine("{" + item.PPGUID.ToString().ToUpper() + "}_SPEED:" + item.SPEED);

                    var sensor = sensorlist.Where(p => p.SSGUID == item.SSGUID).FirstOrDefault();

                    if (sensor == null)
                        continue;

                    var url = filePath + @"\" + psCode + @"\" + item.PickDate.Year + @"\" + +item.PickDate.Month + @"\" + +item.PickDate.Day + @"\" + gateway;
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
                        var values = txtcontent.Split('[')[1].Split(']')[0].Trim();
                        //   values = "0," + values;
                        redisClient.Set("{" + item.SSGUID.ToString().ToUpper() + "}_WaveTime", values);
                        sb.AppendLine("{" + item.SSGUID.ToString().ToUpper() + "}_WaveTime:" + values);

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
                        var values = txtcontent.Split('[')[1].Split(']')[0].Trim();
                        values = "0," + values;
                        redisClient.Set("{" + item.SSGUID.ToString().ToUpper() + "}_Spec", values);
                        sb.AppendLine("{" + item.SSGUID.ToString().ToUpper() + "}_Spec:" + values);
                    }

                }

                redisClient.Set("IntouchUpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            }

            return sb.ToString();
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
