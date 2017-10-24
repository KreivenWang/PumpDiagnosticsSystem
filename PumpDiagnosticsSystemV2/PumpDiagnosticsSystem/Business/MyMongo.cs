using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Business
{
    public static class MyMongo
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        const string DBConn = "mongodb://127.0.0.1:27017";

        /// <summary>
        /// 数据库名称 
        /// </summary>
        const string PumpSysDB = "pumpSystem";

        static MyMongo()
        {
            IMongoCollection<BsonDocument> collection;
            if (!TryGetDb(DBCollections.ppSysReport.ToString(), out collection))
                Log.Inform("连接MongoDB失败");
        }

        private static bool TryGetDb(string dbCollection, out IMongoCollection<BsonDocument> collection)
        {
            try {
                //创建数据库链接 
                var client = new MongoClient(DBConn);
                //获得数据库cnblogs 
                var db = client.GetDatabase(PumpSysDB);

                collection = db.GetCollection<BsonDocument>(dbCollection);
            } catch (Exception ex) {
                Log.Inform(ex.Message);
                collection = null;
            }
            return collection != null;
        }

        public static bool BuildPropsPreview(PumpSystem ppSys)
        {
            IMongoCollection<BsonDocument> collection;
            if (!TryGetDb(DBCollections.ppSysReport + "_" + ppSys.Guid, out collection))
                return false;
            collection.DeleteMany(Builders<BsonDocument>.Filter.Empty);
            var insertItems =
                (from reportItem in ppSys.GetReport()
                    select new BsonDocument {
                        {nameof(reportItem.PumpCode), reportItem.PumpCode},
                        {nameof(reportItem.CompCode), reportItem.CompCode},
                        {nameof(reportItem.CompName), reportItem.CompName},
                        {nameof(reportItem.CompType), reportItem.CompType.ToString()},
                        {nameof(reportItem.TdPos), reportItem.TdPos?.ToString() ?? ""},
                        {nameof(reportItem.PropName), reportItem.PropName},
                        {nameof(reportItem.Variable), reportItem.Variable},
                        {nameof(reportItem.Value), reportItem.Value ?? ""}
                    }).ToList();
            collection.InsertMany(insertItems);
            return true;
        }


        //        public static bool BuildFMEAItems(IEnumerable<LogicItem> logicItems)
        //        {
        //            IMongoCollection<BsonDocument> collection;
        //            if (!TryGetDb(DBCollections.fmeaItems, out collection))
        //                return false;
        //            collection.DeleteMany(Builders<BsonDocument>.Filter.Empty);
        //            collection.InsertMany(logicItems.Select(li => FMEAItem.FromLogicItem(li).ToBsonDocument()));
        //            return true;
        //        }
        //
        //        public static bool BuildPropertyMap(IndirectPropertyExtracter extracter)
        //        {
        //            IMongoCollection<BsonDocument> collection;
        //            if (!TryGetDb(DBCollections.ppSysReport, out collection))
        //                return false;
        //            collection.DeleteMany(Builders<BsonDocument>.Filter.Empty);
        //            var insertItems = new List<Tuple<string,string>>();
        //            insertItems.AddRange(extracter.GetConstParas().Select(p => new Tuple<string, string>(p.Key, p.Value)));
        //            insertItems.AddRange(extracter.GetDirParas().Select(p => new Tuple<string, string>(p.Key, p.Value)));
        //            insertItems.AddRange(extracter.GetAutotraceParas().Select(p => new Tuple<string, string>(p.Key, p.Value)));
        //            insertItems.AddRange(extracter.GetRefParas().Select(p => new Tuple<string, string>(p.Key, p.Value)));
        //            collection.InsertMany(insertItems.Select(ii => new BsonDocument {
        //                {"name", ii.Item1},
        //                {"value", ii.Item2}
        //            }));
        //            return true;
        //        }

        public enum DBCollections
        {
            ppSysReport
        }

        public class FMEAItem
        {
            public string code { get; set; }
            public string text { get; set; }
            public string[] prevCodes { get; set; }
            public string[] nextCodes { get; set; }

//            public static FMEAItem FromLogicItem(LogicItem logicItem)
//            {
//                var text = string.Empty;
//                var evNode = logicItem.ThisNode as EntityNodeRelatedEventNode;
//                if (evNode != null) {
//                    text = evNode.DisplayText;
//                }
//
//                var result = new FMEAItem() {
//                    code = logicItem.ThisNode.Code,
//                    text = text,
//                    prevCodes = logicItem.PrevItemList.Select(item => item.ThisNode.Code).ToArray(),
//                    nextCodes = logicItem.NextItemList.Select(item => item.ThisNode.Code).ToArray()
//                };
//                return result;
//            }

            public BsonDocument ToBsonDocument()
            {
                var newItem = new BsonDocument {
                    {"code", code},
                    {"text", text},
                    {"prev", new BsonArray(prevCodes)},
                    {"next", new BsonArray(nextCodes)}
                };
                return newItem;
            }
        }
    }
}
