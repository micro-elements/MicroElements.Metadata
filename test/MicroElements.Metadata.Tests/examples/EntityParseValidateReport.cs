using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using FluentAssertions;
using MicroElements.Diagnostics;
using MicroElements.Metadata.OpenXml.Excel.Parsing;
using MicroElements.Metadata.OpenXml.Excel.Reporting;
using MicroElements.Metadata.Parsing;
using MicroElements.Validation;
using MicroElements.Validation.Rules;
using Xunit;

namespace MicroElements.Metadata.Tests.examples
{
    public class EntityParseValidateReport
    {
        public class Entity
        {
            public DateTime CreatedAt { get; }

            public string Name { get; }

            public Entity(DateTime createdAt, string name)
            {
                CreatedAt = createdAt;
                Name = name;
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return $"{nameof(CreatedAt)}: {CreatedAt:yyyy-MM-ddTHH:mm:ss.FFFFFFF}, {nameof(Name)}: {Name}";
            }
        }

        public class EntityMeta : IPropertySet, IModelMapper<Entity>
        {
            public static readonly EntityMeta Instance = new EntityMeta();

            public static readonly IProperty<DateTime> CreatedAt = new Property<DateTime>("CreatedAt");
            public static readonly IProperty<string> Name = new Property<string>("Name");

            /// <inheritdoc />
            public IEnumerable<IProperty> GetProperties()
            {
                yield return CreatedAt;
                yield return Name;
            }

            /// <inheritdoc />
            public IPropertyContainer ToContainer(Entity model)
            {
                return new MutablePropertyContainer()
                    .WithValue(CreatedAt, model.CreatedAt)
                    .WithValue(Name, model.Name);
            }

            /// <inheritdoc />
            public Entity ToModel(IPropertyContainer container)
            {
                return new Entity(
                    createdAt: container.GetValue(CreatedAt),
                    name: container.GetValue(Name));
            }
        }

        public class EntityParser : ParserProvider
        {
            protected ParseResult<DateTime> ParseDate(string? value) => Parser.ParseDateTime(value);

            /// <inheritdoc />
            public EntityParser()
            {
                Source("CreatedAt", ParseDate).Target(EntityMeta.CreatedAt);
                Source("Name").Target(EntityMeta.Name);
            }
        }

        public class EntityValidator : IValidator
        {
            /// <inheritdoc />
            public IEnumerable<IValidationRule> GetRules()
            {
                yield return EntityMeta.CreatedAt.NotDefault();
                yield return EntityMeta.Name.Required();
            }
        }

        public class EntityReport : ReportProvider
        {
            public EntityReport(string reportName = "Entities")
                : base(reportName)
            {
                Add(EntityMeta.CreatedAt).SetDateSerialFormat();
                Add(EntityMeta.Name);
            }
        }

        public Stream ReportToExcel(Entity[] entities, IReportBuilderSettings? reportBuilderSettings)
        {
            var reportRows = entities.Select(entity => EntityMeta.Instance.ToContainer(entity));

            var excelStream = new MemoryStream();

            ExcelReportBuilder.Create(excelStream, settings: reportBuilderSettings)
                .AddReportSheet(new EntityReport("Entities"), reportRows)
                .SaveAndClose();

            return excelStream;
        }

        public Entity[] ParseExcel(Stream stream)
        {
            var document = SpreadsheetDocument.Open(stream, false);

            var messages = new List<Message>();

            var entities = document
                .GetSheet("Entities")
                .GetRowsAs(new EntityParser(), list => new PropertyContainer(list))
                .ValidateAndFilter(new EntityValidator(), result => messages.AddRange(result.ValidationMessages))
                .Select(container => EntityMeta.Instance.ToModel(container))
                .ToArray();

            return entities;
        }

        [Fact]
        public void UseCase()
        {
            // Trim DateTime to milliseconds because default DateTime render trimmed to milliseconds
            DateTime NowTrimmed(long trim)
            {
                DateTime now = DateTime.Now;
                return now.AddTicks(-(now.Ticks % trim));
            }

            Entity[] entities = {
                new Entity(NowTrimmed(TimeSpan.TicksPerSecond), "Name1"),
                new Entity(NowTrimmed(TimeSpan.TicksPerMillisecond), "Name2"),
                new Entity(NowTrimmed(TimeSpan.TicksPerDay), "Name3"),
            };

            BuildExcelAndParseBack(null);
            BuildExcelAndParseBack(new ReportBuilderSettings(cellFactory: new CachedOpenXmlCellFactory()));

            void BuildExcelAndParseBack(IReportBuilderSettings? reportBuilderSettings)
            {
                Stream excelStream = ReportToExcel(entities, reportBuilderSettings);

                Entity[] fromExcel = ParseExcel(excelStream);

                fromExcel.Should().HaveCount(3);
                fromExcel.Should().BeEquivalentTo(entities);
            }
        }
    }
}
