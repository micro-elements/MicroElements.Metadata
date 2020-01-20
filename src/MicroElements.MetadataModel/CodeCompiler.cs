// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using MicroElements.Functional;

namespace MicroElements
{
    public static class CodeCompiler
    {

        /// <summary>
        /// Маркерный тип для указания, что функция будет сгенерирована для произвольных типов.
        /// </summary>
        public class GenericType { }

        public static Func<Type, Func<Arg0, Result>> CreateCompiledFunc<Arg0, Result>(Func<Arg0, Result> methodFunc)
        {
            MethodInfo methodInfo = methodFunc.Method.GetGenericMethodDefinition();
            return CreateCompiledFunc<Arg0, Result>(methodInfo);
        }

        public static Func<Type, Func<Arg0, Arg1, Result>> CreateCompiledFunc<Arg0, Arg1, Result>(Func<Arg0, Arg1, Result> methodFunc)
        {
            MethodInfo methodInfo = methodFunc.Method.GetGenericMethodDefinition();
            return CreateCompiledFunc<Arg0, Arg1, Result>(methodInfo);
        }

        public static Func<Type, Func<Arg0, Result>> CreateCompiledFunc<Arg0, Result>(Type methodOwnerType, string methodName)
        {
            MethodInfo methodInfo = methodOwnerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            return CreateCompiledFunc<Arg0, Result>(methodInfo);
        }

        public static Func<Type, Func<Arg0, Arg1, Result>> CreateCompiledFunc<Arg0, Arg1, Result>(Type methodOwnerType, string methodName)
        {
            MethodInfo methodInfo = methodOwnerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            return CreateCompiledFunc<Arg0, Arg1, Result>(methodInfo);
        }

        public static Func<Type, Func<Arg0, Result>> CreateCompiledFunc<Arg0, Result>(MethodInfo methodInfo, int cacheLimit = 20) =>
            Prelude.Memoize<Type, Func<Arg0, Result>>(
                propertyType =>
                {
                    MethodInfo genericMethod = methodInfo.MakeGenericMethod(propertyType);
                    ParameterExpression arg0 = Expression.Parameter(typeof(Arg0));
                    MethodCallExpression callExpression = Expression.Call(genericMethod, arg0);
                    return Expression
                        .Lambda<Func<Arg0, Result>>(callExpression, arg0)
                        .Compile();
                }, cacheLimit);

        public static Func<Type, Func<Arg0, Arg1, Result>> CreateCompiledFunc<Arg0, Arg1, Result>(MethodInfo methodInfo, int cacheLimit = 20) =>
            Prelude.Memoize<Type, Func<Arg0, Arg1, Result>>(
                propertyType =>
                {
                    MethodInfo genericMethod = methodInfo.MakeGenericMethod(propertyType);
                    ParameterExpression arg0 = Expression.Parameter(typeof(Arg0));
                    ParameterExpression arg1 = Expression.Parameter(typeof(Arg1));
                    MethodCallExpression callExpression = Expression.Call(genericMethod, arg0, arg1);
                    return Expression
                        .Lambda<Func<Arg0, Arg1, Result>>(callExpression, arg0, arg1)
                        .Compile();
                }, cacheLimit);

    }
}
