// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using MicroElements.Functional;

namespace MicroElements.Reporting.Excel
{
    public enum ExcelError
    {
        CellFormatNotFound,
        CellFormatsIsNotRegistered
    }

    public class ExcelException : ExceptionWithError<ExcelError>
    {
        /// <inheritdoc />
        public ExcelException(Error<ExcelError> error)
            : base(error)
        {
        }

        /// <inheritdoc />
        protected ExcelException()
        {
        }

        /// <inheritdoc />
        protected ExcelException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public static class ExcelErrors
    {
        public static Error<ExcelError> CellFormatNotFound(string name) => Error.CreateError(
            ExcelError.CellFormatNotFound,
            "CellFormat not found by name {name}",
            name);

        public static Error<ExcelError> CellFormatsIsNotRegistered = Error.CreateError(
            ExcelError.CellFormatsIsNotRegistered,
            "No CellFormat registered in stylesheet");

        [DoesNotReturn]
        public static void Throw<TErrorCode>(this in Error<TErrorCode> error)
        {
            throw new ExceptionWithError<TErrorCode>(error);
        }

        [DoesNotReturn]
        public static void Throw(this in Error<ExcelError> error)
        {
            throw new ExcelException(error);
        }
    }
}
