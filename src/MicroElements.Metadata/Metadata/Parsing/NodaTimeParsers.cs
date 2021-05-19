using System;
using MicroElements.Shared;
using Error = MicroElements.Diagnostics.ErrorModel.Error;
using Message = MicroElements.Diagnostics.Message;

namespace MicroElements.Metadata.Parsing
{
    abstract class ReflectionParser : IValueParser
    {
        /// <inheritdoc />
        public Type Type { get; }

        protected ReflectionParser(Type type)
        {
            Type = type;
        }

        protected abstract bool ParseToCtorArgs(string? source, out object[]? args);

        /// <inheritdoc />
        public IParseResult ParseUntyped(string? source)
        {
            if (ParseToCtorArgs(source, out var args))
            {
                return TryCreateInstance(source, Type, args);
            }

            Message error = Error.CreateError("ParseError", "Value {text} can not be parsed as {type}.", source, Type).Message;
            return new ParserResultUntyped(Type, isSuccess: false, valueUntyped: null, error: error);
        }

        IParseResult TryCreateInstance(string? source, Type type, params object[]? args)
        {
            try
            {
                object instance = Activator.CreateInstance(type, args);
                return new ParserResultUntyped(type, isSuccess: true, valueUntyped: instance, error: null);
            }
            catch (Exception e)
            {
                Message error = Error.CreateError("CreateError", "Value {text} can not be parsed as {type}. Error: {error}", source, type, e.Message).Message;
                return new ParserResultUntyped(type, isSuccess: false, valueUntyped: null, error: error);
            }
        }
    }

    class ParserResultUntyped : IParseResult
    {
        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public bool IsSuccess { get; }

        /// <inheritdoc />
        public object? ValueUntyped { get; }

        /// <inheritdoc />
        public Message? Error { get; }

        public ParserResultUntyped(Type type, bool isSuccess, object? valueUntyped, Message? error)
        {
            Type = type;
            IsSuccess = isSuccess;
            ValueUntyped = valueUntyped;
            Error = error;
        }
    }

    class NodaLocalDateParser : ReflectionParser
    {
        /// <inheritdoc />
        public NodaLocalDateParser()
            : base(TypeCache.NodaTimeTypes.Value.GetByFullName("NodaTime.LocalDate"))
        {
        }

        /// <inheritdoc />
        protected override bool ParseToCtorArgs(string? source, out object[]? args)
        {
            if (DateTime.TryParse(source, out var dateTime))
            {
                args = new object[] { dateTime.Year, dateTime.Month, dateTime.Day };
                return true;
            }

            args = null;
            return false;
        }
    }

    class NodaLocalDateTimeParser : ReflectionParser
    {
        /// <inheritdoc />
        public NodaLocalDateTimeParser()
            : base(TypeCache.NodaTimeTypes.Value.GetByFullName("NodaTime.LocalDateTime"))
        {
        }

        /// <inheritdoc />
        protected override bool ParseToCtorArgs(string? source, out object[]? args)
        {
            if (DateTime.TryParse(source, out var dateTime))
            {
                args = new object[] { dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond };
                return true;
            }

            args = null;
            return false;
        }
    }
}
