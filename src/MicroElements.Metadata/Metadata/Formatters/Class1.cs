namespace MicroElements.Metadata.Formatters
{
    public class PropertyContainerFormatter : IValueFormatter<IPropertyContainer>
    {
        public static IValueFormatter Instance { get; } = new CompositeFormatter()
            .With(new DefaultFormatProvider().GetFormatters())
            .With(new PropertyContainerFormatter())
            .With(new PropertyValueFormatter())
            .With(new CollectionFormatter(new RuntimeFormatter(() => Instance), setting =>
            {
                setting.ValueFormatter = new RuntimeFormatter(() => Instance);
                setting.StartSymbol = "{";
                setting.EndSymbol = "}";
                setting.Separator = ", ";
                setting.MaxItems = int.MaxValue;
                setting.MaxTextLength = int.MaxValue;
            }))
            .With(new KeyValuePairFormatter(new RuntimeFormatter(() => Instance)))
            .With(new ValueTuplePairFormatter(new RuntimeFormatter(() => Instance)))
            .With(DefaultToStringFormatter.Instance);

        public static IValueFormatter Instance2 { get; } = FormatterBuilder
            .Create()
            .AddStandardFormatters()
            .ConfigureCollectionFormatter(setting => setting.MaxItems = 5)
            .WithFormatter(new PropertyContainerFormatter())
            //.WithFormatter(formatter => new CollectionFormatter(formatter, setting => ))
            .Build();

        public string? Format(IPropertyContainer? value)
        {
            if (value != null)
                return Instance.TryFormat(value.Properties);
            return null;
        }
    }

    public class PropertyValueFormatter : IValueFormatter<IPropertyValue>
    {
        public string? Format(IPropertyValue? value)
        {
            if (value == null)
                return null;
            return $"{value.PropertyUntyped.Name}: {PropertyContainerFormatter.Instance.TryFormat(value.ValueUntyped)}";
        }
    }

}
