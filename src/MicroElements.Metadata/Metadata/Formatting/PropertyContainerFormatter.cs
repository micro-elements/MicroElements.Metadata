namespace MicroElements.Metadata.Formatting
{
    public class PropertyContainerFormatter : INotNullValueFormatter<IPropertyContainer>
    {
        public static IValueFormatter Instance { get; } = FormatterBuilder
            .Create()
            .AddStandardFormatters()
            .AddStandardCollectionFormatters()
            .ConfigureCollectionFormatter(setting =>
            {
                setting.StartSymbol = "{";
                setting.EndSymbol = "}";
                setting.Separator = ", ";
                setting.MaxTextLength = 10000;
            })
            .ConfigureKeyValuePairFormatter(settings => settings.Format = "{0}={1}")
            .WithFormatter(new PropertyContainerFormatter())
            .WithFormatter(new PropertyValueFormatter())
            .Build();

        public string? Format(IPropertyContainer value)
        {
            return Instance.TryFormat(value.Properties);
        }
    }

    public class PropertyValueFormatter : INotNullValueFormatter<IPropertyValue>
    {
        public string Format(IPropertyValue value)
        {
            return $"{value.PropertyUntyped.Name}: {PropertyContainerFormatter.Instance.TryFormat(value.ValueUntyped)}";
        }
    }
}
