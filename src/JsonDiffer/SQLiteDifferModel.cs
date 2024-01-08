using System;
using System.Collections.Generic;
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
        }
        public class Element
        {
            private string _type = constVariable.RangeType;
            public Element() { }
            public string LevelDB { get; set; }
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
            public string Descript { get; set; }
        }

        public List<Element> SQLiteModels { get; set; } = new List<Element>();
        public SQLiteDifferModel() { }
        
    }
}
