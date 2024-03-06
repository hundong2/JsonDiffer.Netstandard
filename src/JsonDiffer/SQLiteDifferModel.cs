using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JsonDiffer
{
    public class SQLiteDifferModel
    {
        public static class constVariable
        {
            public const string RangeType = "range";
            public const string StringType = "string";
            public const string CountType = "count";

            public const string StatusOK = "OK";
            public const string StatusFail = "Fail";
            public const string StatusHold = "Hold"; //보류
            public const string StatusNone = "None";
        }
        public class Element
        {
            private string _type = constVariable.RangeType;
            public Element() { }
            public string LevelDB { get; set; } = string.Empty;
            public double MinValue { get; set; } = -1;
            public double MaxValue { get; set; } = -1;
            public string Type 
            {
                get
                {
                    return _type;
                }
                set
                {
                    if( value == "str" )
                    {
                        _type = constVariable.StringType;
                    }
                    else if ( value == "count")
                    {
                        _type = constVariable.CountType;
                    }
                    else
                    {
                        _type = constVariable.RangeType;
                    }
                    
                }
            }
            public string Descript { get; set; } = string.Empty;
            public string CheckResultValue { get; set; } = constVariable.StatusNone;
            public string OriginPath { get; set; } = string.Empty;
        }

        public List<Element> SQLiteModels { get; set; } = new List<Element>();
        public SQLiteDifferModel() { }

        /// <summary>
        /// Clear SQLiteModel Element 
        /// </summary>
        public void ClearElement()
        {
            try
            {
                foreach (var element in SQLiteModels)
                {
                    element.CheckResultValue = constVariable.StatusNone;
                    element.OriginPath = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public void CheckRangeFromFilePath(string path, string output, List<string> rangeresult = null)
        {
            using (StreamReader file = File.OpenText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                var jobj = serializer.Deserialize(file, typeof(JObject));
                if(jobj != null )
                {
                    CheckRange(jobj as JObject, rangeresult);
                }
                File.WriteAllText(output, jobj.ToString());
            }
        }
        public void CheckRange(JToken obj, List<string> rangeresult = null)
        {
            try
            {
                if (obj != null)
                {
                    if(obj.Path == "body.ResData3[0].StartPosInfo.PosInOpt")
                    {

                    }
                    if( obj is JObject || obj is JArray || obj is JProperty )
                    {
                        foreach( var element in obj )
                        {
                            //CheckRange(element.Value as JObject);
                            CheckRange(element, rangeresult);
                        }
                    }
                    else
                    {
                        if (obj is JValue)
                        {
                            //checkelement range 
                            SQLiteModels.ForEach(x =>
                            {
                                var check = x.LevelDB.ToLower().Substring(0, x.LevelDB.Length - 1);
                                if (obj.Path.ToLower().Contains(x.LevelDB.ToLower().Substring(0, x.LevelDB.Length - 1)))
                                {
                                    x.OriginPath = obj.Path; //push origin data path
                                    //Range check
                                    if (x.Type == SQLiteDifferModel.constVariable.RangeType)
                                    {
                                        int temp = 0;
                                        if (int.TryParse(obj.ToString(), out temp))
                                        {
                                            if (x.MinValue <= temp && x.MaxValue >= temp)
                                            {
                                                x.CheckResultValue = SQLiteDifferModel.constVariable.StatusOK;
                                            }
                                            else
                                            {
                                                x.CheckResultValue = SQLiteDifferModel.constVariable.StatusFail;
                                            }
                                            (obj as JValue).Value = $@"[{x.CheckResultValue}] {obj.ToString()} ({x.MinValue}~{x.MaxValue})";
                                            if (rangeresult != null)
                                            {
                                                rangeresult.Add($@"{obj.Path.ToString()} : {(obj as JValue).Value.ToString()}");
                                            }
                                            else;
                                        }
                                        else
                                        {
                                        }
                                    }
                                    else;
                                    //Other check
                                    
                                }
                                else;
                            });
                        }
                        else;
                    }
                }
                else;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        
    }
}
