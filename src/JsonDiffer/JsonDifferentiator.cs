using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace JsonDiffer
{
    public class JsonDifferentiator
    {
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
        /// DiffCheck of Json Object from JToken 
        /// </summary>
        /// <param name="first"></param>
        /// <returns>true : changed something, false : not changed</returns>
        public static bool DiffCheckJToken( JToken first )
        {
            var valueTemp = first as JObject;
            foreach( var element in first as JObject )
            {
                if (element.Key.Contains("-"))
                {
                    return true;
                }
            }
            foreach( var item in first.First )
            {
                var temp = item.First as JProperty;
                if ( temp != null )
                {
                    if( temp.Name.Contains(SignInformation.Changed) || temp.Name.Contains(SignInformation.Removed) )
                    {
                        //changed value
                        return true;
                    }
                    else
                    {
                        //not changed 
                    }
                }
            }
            return false;
        }
        public static JToken Differentiate(JToken first, JToken second, OutputMode outputMode = OutputMode.Symbol, bool showOriginalValues = false)
        {
            if (JToken.DeepEquals(first, second)) return null;

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

                            var diff = Differentiate(firstsItem, secondsItem, outputMode, showOriginalValues);

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

                if (first?[property] == null)
                {
                    var secondVal = second?[property]?.Parent as JProperty;

                    var targetNode = PointTargetNode(difference, property, ChangeMode.Added, outputMode);

                    if (targetNode.Property != null)
                    {
                        difference[targetNode.Symbol][targetNode.Property] = secondVal.Value;
                    }
                    else
                        difference[targetNode.Symbol] = secondVal.Value;

                    continue;
                }

                if (second?[property] == null)
                {
                    var firstVal = first?[property]?.Parent as JProperty;

                    var targetNode = PointTargetNode(difference, property, ChangeMode.Removed, outputMode);

                    if (targetNode.Property != null)
                    {
                        difference[targetNode.Symbol][targetNode.Property] = firstVal.Value;
                    }
                    else
                        difference[targetNode.Symbol] = firstVal.Value;

                    continue;
                }

                if (first?[property] is JValue value && value.Value != string.Empty)
                {
                    if (!JToken.DeepEquals(first?[property], second?[property]))
                    {
                        var targetNode = PointTargetNode(difference, property, ChangeMode.Changed, outputMode);
                        if (second[property] is JValue value2)
                        {
                            string variable = value.Value.ToString();
                            string originVariable = value2.Value.ToString();
                            bool isCheck = false;
                            //added min, max routine 
                            if (variable != null && variable.Contains("~"))
                            {
                                var splited = variable.Split('~');
                                if (splited.Count() == 2)
                                {
                                    int firstValue = 0, secondValue = 0;
                                    var firstCheck = int.TryParse(splited[0], out firstValue);
                                    var secondCheck = int.TryParse(splited[1], out secondValue);
                                    if (firstCheck && secondCheck)
                                    {
                                        int resultValue = 0;
                                        if (int.TryParse(originVariable, out resultValue))
                                        {
                                            if (firstValue <= resultValue && resultValue <= secondValue)
                                            {
                                                isCheck = true;
                                            }
                                            else;
                                        }
                                        else;
                                    }
                                    else
                                    {
                                        if (splited[0] == "")
                                        {
                                            int diffVariable = 0;
                                            if (int.TryParse(splited[1], out diffVariable))
                                            {
                                                int resultValue = 0;
                                                if (int.TryParse(originVariable, out resultValue))
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
                                            int diffVariable = 0;
                                            if (int.TryParse(splited[0], out diffVariable))
                                            {
                                                int resultValue = 0;
                                                if (int.TryParse(originVariable, out resultValue))
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
                            else
                            {
                                if( variable == originVariable )
                                {
                                    isCheck = true;
                                }
                            }

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
                                difference[targetNode.Symbol][targetNode.Property] = showOriginalValues ? second?[property] : value;
                            }
                            else
                                difference[targetNode.Symbol] = showOriginalValues ? second?[property] : value;

                        }


                        //difference["changed"][property] = showOriginalValues ? second?[property] : value;
                    }
                    else
                    {
                        var targetNode = PointTargetNode(difference, property, ChangeMode.Same, outputMode);
                        if (targetNode.Property != null)
                        {
                            difference[targetNode.Symbol][targetNode.Property] = showOriginalValues ? second?[property] : value;
                        }
                        else
                            difference[targetNode.Symbol] = showOriginalValues ? second?[property] : value;
                    }

                    continue;
                }

                if (first?[property] is JObject)
                {

                    var targetNode = second?[property] == null
                        ? PointTargetNode(difference, property, ChangeMode.Removed, outputMode)
                        : PointTargetNode(difference, property, ChangeMode.Changed, outputMode);

                    var firstsItem = first[property];
                    var secondsItem = second[property];

                    var diffrence = Differentiate(firstsItem, secondsItem, outputMode, showOriginalValues);

                    if (diffrence != null)
                    {

                        if (targetNode.Property != null)
                        {
                            difference[targetNode.Symbol][targetNode.Property] = diffrence;
                        }
                        else
                            difference[targetNode.Symbol] = diffrence;

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

                        var diff = Differentiate(firstsItem, secondsItem, outputMode, showOriginalValues);

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
