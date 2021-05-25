// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Metadata.Functional;
using MicroElements.Shared;
using Error = MicroElements.Diagnostics.ErrorModel.Error;
using Message = MicroElements.Diagnostics.Message;

namespace MicroElements.Metadata.Parsing
{
    public abstract class ReflectionParser : IValueParser
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

    public class NodaLocalDateParser : ReflectionParser
    {
        /// <inheritdoc />
        public NodaLocalDateParser()
            : base(TypeUtils.GetByFullName("NodaTime.LocalDate"))
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

    public class NodaLocalDateTimeParser : ReflectionParser
    {
        /// <inheritdoc />
        public NodaLocalDateTimeParser()
            : base(TypeUtils.GetByFullName("NodaTime.LocalDateTime"))
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
