using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using MicroElements.Functional;
using MicroElements.Metadata.Parsers;

namespace MicroElements.Metadata.Xml
{
    public static partial class XmlParser
    {
        /// <summary>
        /// Gets full element name from root parent to current element.
        /// Example: "A.B.C".
        /// </summary>
        /// <param name="element">Source element.</param>
        /// <param name="delimeter">Optional delimeter.</param>
        /// <returns>Full element name.</returns>
        public static string GetFullName(this XElement element, string delimeter = ".")
        {
            string name = element.Name.LocalName;

            XElement? parent = element.Parent;
            while (parent != null)
            {
                name = parent.Name.LocalName + delimeter + name;
                parent = parent.Parent;
            }

            return name;
        }

        public static string GetElementNameDefault(XElement element)
        {
            return element.Name.LocalName;
        }

        public static IEnumerable<IValueParser> CreateDefaultXmlParsers()
        {
            yield return StringParser.Instance;
            yield return new ValueParser<int>(ParseInt);
            yield return new ValueParser<double>(ParseDouble);
            yield return new ValueParser<DateTime>(ParseDateTime);

            yield return new ValueParser<int?>(ParseNullableInt);
            yield return new ValueParser<double?>(ParseNullableDouble);
        }

        public static IEnumerable<IParserRule> ToParserRules(this IEnumerable<IValueParser> parsers)
        {
            foreach (IValueParser valueParser in parsers)
            {
                yield return new ParserRule(valueParser, valueParser.Type);
            }
        }

        public static IEnumerable<IParserRule> CreateDefaultXmlParsersRules()
        {
            return CreateDefaultXmlParsers().ToParserRules();
        }

        public static Option<int> ParseInt(string text) => Prelude.ParseInt(text);

        public static Option<double> ParseDouble(string text) => Prelude.ParseDouble(text, NumberStyles.Any, CultureInfo.InvariantCulture);

        public static Option<DateTime> ParseDateTime(string text)
        {
            var parseResult = DateTime.TryParse(text, out DateTime result);
            return parseResult ? result : Option<DateTime>.None;
        }

        public static Option<int?> ParseNullableInt(string text) => ParseInt(text).MatchUnsafe(value => value, default(int?));

        public static Option<double?> ParseNullableDouble(string text) => ParseDouble(text).MatchUnsafe(value => value, default(double?));
    }
}
