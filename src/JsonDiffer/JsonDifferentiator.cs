using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace JsonDiffer
{
    public class JsonDifferentiator
    {
        static string sameValue = "same value(@): ";
        static string diffValue = "diff value(*): ";
        static string notContainValue = "not contained value(-): ";
        public OutputMode OutputMode { get; private set; }
        public bool ShowOriginalValues { get; private set; }

        public JsonDifferentiator(OutputMode outputMode, bool showOriginalValues)
        {
            this.OutputMode = outputMode;
            this.ShowOriginalValues = showOriginalValues;
        }

        private static TargetNode PointTargetNode(JToken diff, string property, ChangeMode mode, OutputMode outMode)
        {
            string symbol = string.Empty;

            switch (mode)
            {
                case ChangeMode.Changed:
                    symbol = outMode == OutputMode.Symbol ? $"*{property}" : "changed";
                    break;

                case ChangeMode.Added:
                    symbol = outMode == OutputMode.Symbol ? $"+{property}" : "added";
                    break;

                case ChangeMode.Removed:
                    symbol = outMode == OutputMode.Symbol ? $"-{property}" : "removed";
                    break;
                case ChangeMode.Same:
                    symbol = outMode == OutputMode.Symbol ? $"@{property}" : "samed";
                    break;
            }

            if (outMode == OutputMode.Detailed && diff[symbol] == null)
            {
                diff[symbol] = JToken.Parse("{}");
            }

            return new TargetNode(symbol, (outMode == OutputMode.Symbol) ? null : property);

        }
        /// <summary>
        /// Diff check from all json keys 
        /// </summary>
        /// <param name="first"></param>
        /// <returns></returns>
        public static void DiffCheckJToken3(JToken first, ref List<string> element, bool flag = false)
        {
            try
            {
                if (first is JArray)
                {
                    foreach (var jProperty in first)
                    {
                        DiffCheckJToken3(jProperty, ref element, flag: flag);
                    }
                }
                else
                {
                    //var propertyNames = (first?.Children() ?? default).Select(_ => (_ as JProperty)?.Name)?.Distinct();
                    foreach (JProperty jProperty in first)
                    {
                        string key = jProperty.Name;
                        JToken valueToken = jProperty.Value;
                        if (key.StartsWith("-"))
                        {
                            element.Add($"{notContainValue}{jProperty.Path}");
                        }
                        else { }
                        if (valueToken is JObject)
                        {
                            foreach (var obj in valueToken)
                            {

                                if (obj is JProperty && obj.First is JValue)
                                {
                                    bool hasPlusSign = (obj as JProperty).Name.StartsWith("*");
                                    bool hasAtSign = (obj as JProperty).Name.StartsWith("-");

                                    if (hasPlusSign || hasAtSign)
                                    {
                                        string check = $"{notContainValue}";
                                        if (hasPlusSign)
                                            check = $"{diffValue}";
                                        element.Add($"{check}{(obj as JProperty).Name}");
                                    }
                                }
                                else
                                {
                                    bool checkflag = false;
                                    if ( obj is JProperty )
                                    {
                                        bool hasAtSign = (obj as JProperty).Name.StartsWith("-");
                                        
                                        if ( hasAtSign)
                                        {
                                            element.Add($"{notContainValue}{(obj as JProperty).Name}");
                                            checkflag = true;
                                        }
                                        
                                    }
                                    DiffCheckJToken3(obj.First, ref element, flag: checkflag);
                                }

                            }

                        }
                        if (valueToken is JValue jValue)
                        {
                            /*
                            bool hasPlusSign = key.StartsWith("*");
                            bool hasAtSign = key.StartsWith("-");

                            if (hasPlusSign || hasAtSign || flag)
                            {
                                string check = $"{notContainValue}";
                                if( hasPlusSign )
                                    check = $"{diffValue}";

                                element.Add($"{check}{key}:{valueToken}");
                            }
                            */
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Diff check from all json keys 
        /// </summary>
        /// <param name="first"></param>
        /// <returns></returns>
        public static bool DiffCheckJToken2(JToken first)
        {
            try
            {
                if (first is JArray)
                {
                    foreach (var jProperty in first)
                    {
                        var diffcheck = DiffCheckJToken2(jProperty);

                        if (diffcheck)
                            return diffcheck;
                        else { }
                    }
                }
                else
                {
                    //var propertyNames = (first?.Children() ?? default).Select(_ => (_ as JProperty)?.Name)?.Distinct();
                    foreach (JProperty jProperty in first)
                    {
                        string key = jProperty.Name;
                        JToken valueToken = jProperty.Value;
                        if (key.StartsWith("-"))
                        {
                            return true;
                        }
                        else { }
                        if (valueToken is JObject)
                        {
                            foreach (var obj in valueToken)
                            {

                                if (obj is JProperty && obj.First is JValue)
                                {
                                    bool hasPlusSign = (obj as JProperty).Name.StartsWith("*");
                                    bool hasAtSign = (obj as JProperty).Name.StartsWith("-");

                                    if (hasPlusSign || hasAtSign)
                                    {
                                        return true;
                                    }
                                }
                                else
                                {
                                    var diffcheck = DiffCheckJToken2(obj.First);
                                    if (diffcheck)
                                        return diffcheck;
                                    else
                                    {

                                    }
                                }

                            }

                        }
                        if (valueToken is JValue jValue)
                        {
                            bool hasPlusSign = key.StartsWith("*");
                            bool hasAtSign = key.StartsWith("-");

                            if (hasPlusSign || hasAtSign)
                            {
                                return true;
                            }
                        }

                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }
        public static void NoteResult(string property, JObject jobj, JToken differnce, ref List<string> diffelement)
        {
            try
            {
                if( jobj != null && diffelement != null )
                {
                    string diffresultInfo = string.Empty;
                    if( differnce != null )
                    {
                        diffresultInfo = (differnce as JProperty).Name;
                    }
                    string generate = $@"{sameValue}: {jobj.Path}<br>{jobj.ToString()}";
                    if( string.IsNullOrEmpty(generate) == false )
                    {
                        diffelement.Add(generate);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        /// <summary>
        /// Note Result Generate
        /// </summary>
        /// <param name="property">json key property</param>
        /// <param name="first">expect information</param>
        /// <param name="second">output information</param>
        /// <param name="difference">difference information</param>
        /// <param name="diffelement">result information save list</param>
        public static void NoteResult(string property, JValue first, JValue second, JToken difference, ref List<string> diffelement)
        {
            if (first != null && diffelement != null)
            {
                string checkElement = "NONE";
                string diffreusltInfo = string.Empty;
                string secondValue = "";
                if (second != null)
                {
                    checkElement = second.Value.ToString();
                    secondValue = second.Path;
                }
                else
                { }
                if (difference != null)
                {
                    diffreusltInfo = (difference as JProperty).Name;
                }
                else
                { }
                string generate = $@"{first.Path},{secondValue} ({first.Value} <-> {checkElement})";
                if (diffreusltInfo.Contains("@"))
                {
                    generate = $"{sameValue}" + generate;
                }
                else if (diffreusltInfo.Contains("*"))
                {
                    generate = $"{diffValue}" + generate;
                }
                else if (diffreusltInfo.Contains("-"))
                {
                    generate = $"{notContainValue}" + generate;
                }
                else
                {
                    generate = string.Empty;
                }
                if (generate != string.Empty)
                {
                    diffelement.Add(generate);
                }
                else
                { }
            }
        }
        public static bool CustomDeepEquals(JToken value1, JToken value2)
        {
            bool resultValue = false;
            try
            {
                resultValue = JToken.DeepEquals(value1, value2);
                if (resultValue != true)
                {

                }
            }
            catch(Exception ex)
            {
                //Trace.WriteLine(ex.ToString());
            }
            return resultValue;
        }
        public static bool IsMatch(string input, string pattern)
        {
            try
            {
                // Escape the pattern for regular expressions
                pattern = pattern.Replace("#", "*");
                pattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";

                // Use Regex to match the input string with the pattern
                return Regex.IsMatch(input, pattern);
            }
            catch(Exception ex)
            {
                //Trace.WriteLine(ex.ToString());
            }
            return false;
        }
        public static JToken GetValue(JToken value, string Property)
        {
            if( value != null )
            {
                if (value[Property] != null )
                {
                    return value[Property];
                }
                else
                {
                    foreach (JProperty element in value)
                    {
                        if (element != null)
                        {
                            if (IsMatch( element.Name, Property))
                            {
                                return value[element.Name];
                            }
                        }

                    }
                }

            }
            return null;
        }
        public static bool conditionalCheck(string variable, string originVariable)
        {
            bool isCheck = false;
            if (variable != null && variable.Contains("~"))
            {
                var splited = variable.Split('~');
                if (splited.Count() == 2)
                {
                    double firstValue = 0, secondValue = 0;
                    var firstCheck = double.TryParse(splited[0], out firstValue);
                    var secondCheck = double.TryParse(splited[1], out secondValue);
                    if (firstCheck && secondCheck)
                    {
                        double resultValue = 0;
                        if (double.TryParse(originVariable, out resultValue))
                        {
                            if (firstValue <= resultValue && resultValue <= secondValue)
                            {
                                isCheck = true;
                            }
                            else
                            {

                            }
                        }
                        else
                        { }
                    }
                    else
                    {
                        if (splited[0] == "")
                        {
                            double diffVariable = 0;
                            if (double.TryParse(splited[1], out diffVariable))
                            {
                                double resultValue = 0;
                                if (double.TryParse(originVariable, out resultValue))
                                {
                                    if (resultValue <= diffVariable)
                                    {
                                        isCheck = true;
                                    }
                                }

                            }
                        }
                        else if (splited[1] == "")
                        {
                            double diffVariable = 0;
                            if (double.TryParse(splited[0], out diffVariable))
                            {
                                double resultValue = 0;
                                if (double.TryParse(originVariable, out resultValue))
                                {
                                    if (resultValue >= diffVariable)
                                    {
                                        isCheck = true;
                                    }
                                }

                            }
                        }
                    }
                }
            }
            else if (variable != null && variable.StartsWith("c,"))
            {
                var split = variable.Split(',');
                if (split.Count() > 1)
                {
                    double countNumber = 0;
                    if (double.TryParse(split[1], out countNumber))
                    {
                        if (originVariable != null && originVariable.Length == countNumber)
                        {
                            isCheck = true;
                        }
                    }
                }
            }
            else if (variable != null && variable.StartsWith("d,"))
            {

                DateTime tempDate;
                if (originVariable != null && DateTime.TryParseExact(originVariable, "yyyyMMddHHmmss", new CultureInfo("ko-KR"), DateTimeStyles.None, out tempDate))
                {
                    isCheck = true;
                }

            }
            else if (variable != null && variable.StartsWith("w,")) //regex check
            {
                var split = variable.Split(',');
                if (split.Count() > 1)
                {
                    if (IsMatch(originVariable, split[1]))
                    {
                        isCheck = true;
                    }
                }
            }
            else if (variable != null && variable.StartsWith("m,"))
            {
                var split = variable.Replace("m,", "").Trim().Split('/');
                if (split.Count() > 0)
                {
                    foreach (var element in split)
                    {
                        isCheck = conditionalCheck(element, originVariable);
                        if (isCheck)
                            break;
                    }
                }
            }
            else
            {
                if (variable == originVariable)
                {
                    isCheck = true;
                }
            }
            return isCheck;
        }
        public static JToken Differentiate(JToken first, JToken second, OutputMode outputMode = OutputMode.Symbol, bool showOriginalValues = false, List<string> diffelement = null)
        {
            if (CustomDeepEquals(first, second)) return null;

            if (first != null && second != null && first?.GetType() != second?.GetType())
                throw new InvalidOperationException($"Operands' types must match; '{first.GetType().Name}' <> '{second.GetType().Name}'");

            var propertyNames = (first?.Children() ?? default).Union(second?.Children() ?? default)?.Select(_ => (_ as JProperty)?.Name)?.Distinct();

            if (!propertyNames.Any() && (first is JValue || second is JValue))
            {
                return (first == null) ? second : first;
            }

            var difference = JToken.Parse("{}");

            foreach (var property in propertyNames)
            {
                if (property == null)
                {
                    if (first == null)
                    {
                        difference = second;
                    }
                    // array of object?
                    else if (first is JArray && first.Children().All(c => !(c is JValue)))
                    {
                        var difrences = new JArray();
                        var maximum = Math.Max(first?.Count() ?? 0, second?.Count() ?? 0);

                        for (int i = 0; i < maximum; i++)
                        {
                            var firstsItem = first?.ElementAtOrDefault(i);
                            var secondsItem = second?.ElementAtOrDefault(i);

                            var diff = Differentiate(firstsItem, secondsItem, outputMode, showOriginalValues, diffelement);

                            if (diff != null)
                            {
                                difrences.Add(diff);
                            }
                        }

                        if (difrences.HasValues)
                        {
                            difference = difrences;
                        }
                    }
                    else
                    {
                        difference = first;
                    }

                    continue;
                }

                if ( GetValue(first, property) == null) //first?[property] == null ||
                {
                    var secondVal = GetValue(second, property).Parent as JProperty;

                    var targetNode = PointTargetNode(difference, property, ChangeMode.Added, outputMode);

                    if (targetNode.Property != null)
                    {
                        difference[targetNode.Symbol][targetNode.Property] = secondVal.Value;
                    }
                    else
                        difference[targetNode.Symbol] = secondVal.Value;

                    continue;
                }

                if ( GetValue(second, property) == null )
                {
                    var firstVal = GetValue(first, property).Parent as JProperty;

                    var targetNode = PointTargetNode(difference, property, ChangeMode.Removed, outputMode);

                    if (targetNode.Property != null)
                    {
                        difference[targetNode.Symbol][targetNode.Property] = firstVal.Value;
                    }
                    else
                        difference[targetNode.Symbol] = firstVal.Value;
                    NoteResult(property, GetValue(first, property) as JValue, GetValue(second, property) as JValue, difference.Last, ref diffelement);
                    continue;
                }

                if (  GetValue(first, property) is JValue value && value.Value as string != string.Empty)
                {
                    if (!CustomDeepEquals(GetValue(first, property), GetValue(second, property)))
                    {
                        var targetNode = PointTargetNode(difference, property, ChangeMode.Changed, outputMode);
                        if (GetValue(second, property) is JValue value2)
                        {
                            string variable = value.Value.ToString();
                            string originVariable = value2.Value.ToString();
                            bool isCheck = conditionalCheck(variable, originVariable);

                            if (isCheck == false)
                            {
                                targetNode = PointTargetNode(difference, property, ChangeMode.Changed, outputMode);
                            }
                            else
                            {
                                targetNode = PointTargetNode(difference, property, ChangeMode.Same, outputMode);
                            }

                            if (targetNode.Property != null)
                            {
                                difference[targetNode.Symbol][targetNode.Property] = showOriginalValues ? GetValue(second, property) : value;
                            }
                            else
                                difference[targetNode.Symbol] = showOriginalValues ? GetValue(second, property) : value;

                        }
                        //difference["changed"][property] = showOriginalValues ? second?[property] : value;
                    }
                    else
                    {
                        var targetNode = PointTargetNode(difference, property, ChangeMode.Same, outputMode);
                        if (targetNode.Property != null)
                        {
                            difference[targetNode.Symbol][targetNode.Property] = showOriginalValues ? GetValue(second, property) : value;
                        }
                        else
                            difference[targetNode.Symbol] = showOriginalValues ? GetValue(second, property) : value;
                    }
                    NoteResult(property, GetValue(first, property) as JValue, GetValue(second, property)  as JValue, difference.Last, ref diffelement);
                   
                    continue;
                }

                if (first?[property] is JObject)
                {

                    var targetNode = second?[property] == null
                        ? PointTargetNode(difference, property, ChangeMode.Removed, outputMode)
                        : PointTargetNode(difference, property, ChangeMode.Changed, outputMode);

                    var firstsItem = first[property];
                    var secondsItem = second[property];
                    
                    var diffrence = Differentiate(firstsItem, secondsItem, outputMode, showOriginalValues,diffelement);

                    if (diffrence != null)
                    {

                        if (targetNode.Property != null)
                        {
                            difference[targetNode.Symbol][targetNode.Property] = diffrence;
                        }
                        else
                            difference[targetNode.Symbol] = diffrence;

                    }
                    else
                    {
                        //null means is same object 
                        targetNode = PointTargetNode(difference, property, ChangeMode.Same, outputMode);
                        if (targetNode.Property != null)
                        {
                            difference[targetNode.Symbol][targetNode.Property] = showOriginalValues ? GetValue(second, property) : secondsItem;
                        }
                        else
                            difference[targetNode.Symbol] = showOriginalValues ? GetValue(second, property) : secondsItem;
                        NoteResult(property, secondsItem as JObject, difference.Last, ref diffelement);
                    }

                        continue;
                }

                if (first?[property] is JArray)
                {
                    var difrences = new JArray();

                    var targetNode = second?[property] == null
                       ? PointTargetNode(difference, property, ChangeMode.Removed, outputMode)
                       : PointTargetNode(difference, property, ChangeMode.Changed, outputMode);

                    var maximum = Math.Max(first?[property]?.Count() ?? 0, second?[property]?.Count() ?? 0);

                    for (int i = 0; i < maximum; i++)
                    {
                        var firstsItem = first[property]?.ElementAtOrDefault(i);
                        var secondsItem = second[property]?.ElementAtOrDefault(i);

                        var diff = Differentiate(firstsItem, secondsItem, outputMode, showOriginalValues, diffelement);

                        if (diff != null)
                        {
                            difrences.Add(diff);
                        }
                    }

                    if (difrences.HasValues)
                    {
                        if (targetNode.Property != null)
                        {
                            difference[targetNode.Symbol][targetNode.Property] = difrences;
                        }
                        else
                            difference[targetNode.Symbol] = difrences;
                    }
                    else
                    {
                        targetNode = PointTargetNode(difference, property, ChangeMode.Same, outputMode);
                        if(second?[property] != null )
                        {
                            difference[targetNode.Symbol] = second?[property];
                        }
                    }

                    continue;
                }
            }
            return difference;
        }

        public JToken Differentiate(JToken first, JToken second)
        {
            return Differentiate(first, second, this.OutputMode, this.ShowOriginalValues);
        }
    }
}
