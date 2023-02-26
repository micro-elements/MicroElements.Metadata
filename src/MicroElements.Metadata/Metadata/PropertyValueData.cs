namespace MicroElements.Metadata;

public struct PropertyValueData<T> : IPropertyValue<T>
{
    public IProperty<T> Property { get; }
    public T? Value { get; }
    public ValueSource Source { get; }

    public PropertyValueData(IProperty<T> property, T? value, ValueSource source)
    {
        Property = property;
        Value = value;
        Source = source;
    }
}
