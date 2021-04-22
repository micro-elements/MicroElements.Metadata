using System.Diagnostics.Contracts;

namespace MicroElements.Functional
{
    public static class Migrated
    {
        //TODO: Migrate?
        /// <summary>
        /// Gets result value for success and throws <see cref="ExceptionWithError{TErrorCode}"/> for failed result.
        /// </summary>
        /// <typeparam name="A">Success value type.</typeparam>
        /// <typeparam name="TErrorCode">Error code.</typeparam>
        /// <param name="source">Source result.</param>
        /// <param name="allowNullResult">Is null value allowed for result.</param>
        /// <returns>Success value.</returns>
        /// <exception cref="ExceptionWithError{TErrorCode}">Result is in failed state.</exception>
        [Pure]
        public static A GetValueOrThrow<A, TErrorCode>(this in Result<A, IError<TErrorCode>> source, bool allowNullResult = false)
            where TErrorCode : notnull
        {
            if (allowNullResult)
                return source.MatchUnsafe((a) => a, (error) => throw error.ToException());

            return source.Match((a) => a, (error) => throw error.ToException());
        }
    }
}
