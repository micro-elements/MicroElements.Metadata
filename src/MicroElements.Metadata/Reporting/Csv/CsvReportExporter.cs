// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MicroElements.Collections.Extensions.NotNull;
using MicroElements.Metadata;

namespace MicroElements.Reporting.Csv
{
    public class CsvReportExporter
    {
        private readonly char _separator;
        private readonly StringBuilder _csvBuilder = new StringBuilder();

        public CsvReportExporter(char separator = ',')
        {
            _separator = separator;
        }

        public CsvReportExporter AddRows(IReportProvider reportProvider, IEnumerable<IPropertyContainer> source)
        {
            void AddRow(IEnumerable<string> values) => _csvBuilder.AppendJoin(_separator, values).Append(Environment.NewLine);

            var headers = reportProvider.Renderers.Select(renderer => renderer.TargetName);
            AddRow(headers);

            foreach (var rowSource in source.NotNull())
            {
                var rowValues = reportProvider.Renderers.Select(renderer => ConstructCell(renderer, rowSource));
                AddRow(rowValues);
            }

            return this;
        }

        public string GetCsv() => _csvBuilder.ToString();

        private string ConstructCell(IPropertyRenderer propertyRenderer, IPropertyContainer source)
        {
            string textValue = propertyRenderer.Render(source);
            return textValue;
        }
    }

    public static class ReportFileSaver
    {
        public static string SaveToFile(this string content, string directoryName, string fileName = null, string extension = "txt")
        {
            Directory.CreateDirectory(directoryName);
            fileName ??= $"Report-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.{extension}";
            fileName = Path.Combine(directoryName, fileName);
            File.WriteAllText(fileName, content);
            return content;
        }
    }
}
