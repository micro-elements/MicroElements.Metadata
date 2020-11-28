// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MicroElements.Functional;

#region Supressions

// ReSharper disable CheckNamespace
#pragma warning disable SA1649 // File name should match first type name

#endregion

namespace MicroElements.Shared
{
    /// <summary>
    /// Reflection utils.
    /// </summary>
    public static class Reflection
    {
        /// <summary>
        /// Loads assemblies according <paramref name="assemblySource"/>.
        /// 1. Gets all assemblies from <see cref="AppDomain.CurrentDomain"/> if <see cref="AssemblyFilters.LoadFromDomain"/> is true.
        /// 2. Applies filters <see cref="AssemblyFilters.IncludePatterns"/> and <see cref="AssemblyFilters.ExcludePatterns"/>.
        /// 3. Optionally loads assemblies from <see cref="AssemblyFilters.LoadFromDirectory"/> with the same filters.
        /// </summary>
        /// <param name="assemblySource">Filters for getting and filtering assembly list.</param>
        /// <param name="messages">Message list for diagnostic messages.</param>
        /// <returns>Assemblies.</returns>
        public static IEnumerable<Assembly> LoadAssemblies(
            AssemblyFilters assemblySource,
            IMutableMessageList<Message>? messages = null)
        {
            assemblySource.AssertArgumentNotNull(nameof(assemblySource));

            IEnumerable<Assembly> assemblies = Array.Empty<Assembly>();

            if (assemblySource.LoadFromDomain)
                assemblies = AppDomain.CurrentDomain.GetAssemblies();

            assemblies = assemblies
                .IncludeByPatterns(assembly => assembly.FullName, assemblySource.IncludePatterns)
                .ExcludeByPatterns(assembly => assembly.FullName, assemblySource.ExcludePatterns);

            if (assemblySource.LoadFromDirectory != null)
            {
                if (!Directory.Exists(assemblySource.LoadFromDirectory))
                    throw new DirectoryNotFoundException($"Assembly ScanDirectory {assemblySource.LoadFromDirectory} is not exists.");

                var assembliesFromDirectory =
                    Directory
                        .EnumerateFiles(assemblySource.LoadFromDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                        .Concat(Directory.EnumerateFiles(assemblySource.LoadFromDirectory, "*.exe", SearchOption.TopDirectoryOnly))
                        .IncludeByPatterns(fileName => fileName, assemblySource.IncludePatterns)
                        .ExcludeByPatterns(fileName => fileName, assemblySource.ExcludePatterns)
                        .Select(assemblyFile => Reflection.TryLoadAssemblyFrom(assemblyFile, messages)!)
                        .Where(assembly => assembly != null);

                assemblies = assemblies.Concat(assembliesFromDirectory);
            }

            assemblies = assemblies.Distinct();

            return assemblies;
        }

        /// <summary>
        /// Gets types from assembly list according type filters.
        /// </summary>
        /// <param name="assemblies">Assembly list.</param>
        /// <param name="typeFilters">Type filters.</param>
        /// <param name="messages">Message list for diagnostic messages.</param>
        /// <returns>Types that matches filters.</returns>
        public static IReadOnlyCollection<Type> GetTypes(
            IReadOnlyCollection<Assembly> assemblies,
            TypeFilters typeFilters,
            IMutableMessageList<Message>? messages = null)
        {
            assemblies.AssertArgumentNotNull(nameof(assemblies));

            var types = assemblies
                .SelectMany(assembly => assembly.GetDefinedTypesSafe(messages))
                .Where(type => type.FullName != null)
                .Where(type => type.IsPublic == typeFilters.IsPublic)
                .IncludeByPatterns(type => type.FullName, typeFilters.FullNameIncludes)
                .ExcludeByPatterns(type => type.FullName, typeFilters.FullNameExcludes)
                .ToArray();

            return types;
        }

        /// <summary>
        /// Safely returns the set of loadable types from an assembly.
        /// </summary>
        /// <param name="assembly">The <see cref="T:System.Reflection.Assembly" /> from which to load types.</param>
        /// <param name="messages">Message list for diagnostic messages.</param>
        /// <returns>
        /// The set of types from the <paramref name="assembly" />, or the subset
        /// of types that could be loaded if there was any error.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown if <paramref name="assembly" /> is <see langword="null" />.
        /// </exception>
        public static IEnumerable<Type> GetDefinedTypesSafe(this Assembly assembly, IMutableMessageList<Message>? messages = null)
        {
            assembly.AssertArgumentNotNull(nameof(assembly));

            try
            {
                return assembly.DefinedTypes.Select(t => t.AsType());
            }
            catch (ReflectionTypeLoadException ex)
            {
                if (messages != null)
                {
                    foreach (Exception loaderException in ex.LoaderExceptions)
                    {
                        messages.AddError(loaderException.Message);
                    }
                }

                return ex.Types.Where(t => t != null);
            }
        }

        /// <summary>
        /// Tries to load assembly from file.
        /// </summary>
        /// <param name="assemblyFile">The name or path of the file that contains the manifest of the assembly.</param>
        /// <param name="messages">Message list for diagnostic messages.</param>
        /// <returns>Assembly or null if error occurred.</returns>
        public static Assembly? TryLoadAssemblyFrom(string assemblyFile, IMutableMessageList<Message>? messages = null)
        {
            try
            {
                return Assembly.LoadFrom(assemblyFile);
            }
            catch (Exception e)
            {
                messages?.AddError($"Error on load assembly {assemblyFile}. Message: {e.Message}");
                return null;
            }
        }
    }

    internal static class Filtering
    {
        internal static string WildcardToRegex(string pat) => "^" + Regex.Escape(pat).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";

        internal static bool FileNameMatchesPattern(string filename, string pattern) => Regex.IsMatch(Path.GetFileName(filename) ?? string.Empty, WildcardToRegex(pattern));

        internal static IEnumerable<T> IncludeByPatterns<T>(this IEnumerable<T> values, Func<T, string> filterComponent, IReadOnlyCollection<string>? includePatterns = null)
        {
            if (includePatterns == null)
                return values;
            return values.Where(value => includePatterns.Any(pattern => FileNameMatchesPattern(filterComponent(value), pattern)));
        }

        internal static IEnumerable<T> ExcludeByPatterns<T>(this IEnumerable<T> values, Func<T, string> filterComponent, IReadOnlyCollection<string>? excludePatterns = null)
        {
            if (excludePatterns == null)
                return values;
            return values.Where(value => excludePatterns.Any(excludePattern => !FileNameMatchesPattern(filterComponent(value), excludePattern)));
        }
    }

    /// <summary>
    /// Represents type cache and methods for working with types.
    /// Use to reduce reflection time.
    /// </summary>
    public class TypeCache
    {
        /// <summary>
        /// Gets default type cache with all assembly types.
        /// </summary>
        public static TypeCache Default { get; } = Create(AssemblySource.Default, TypeSource.Default);

        /// <summary>
        /// Gets Assembly filters that was used to get <see cref="Assemblies"/>.
        /// </summary>
        public AssemblySource AssemblySource { get; }

        /// <summary>
        /// Gets Type filters that was used to get <see cref="Types"/>.
        /// </summary>
        public TypeSource TypeSource { get; }

        /// <summary>
        /// Gets assemblies that matches assembly filters.
        /// </summary>
        public IReadOnlyCollection<Assembly> Assemblies { get; }

        /// <summary>
        /// Gets types that matches type filters.
        /// </summary>
        public IReadOnlyCollection<Type> Types { get; }

        /// <summary>
        /// Gets types indexed by <see cref="Type.FullName"/>.
        /// </summary>
        public IReadOnlyDictionary<string, Type> TypeByFullName { get; }

        /// <summary>
        /// Gets types indexed by type alias.
        /// </summary>
        public IReadOnlyDictionary<string, Type> TypeByAlias { get; }

        /// <summary>
        /// Creates type cache.
        /// </summary>
        /// <param name="assemblySource">Assembly filters that was used to get <see cref="Assemblies"/>.</param>
        /// <param name="typeSource">Type filters that was used to get <see cref="Types"/>.</param>
        public TypeCache(
            AssemblySource assemblySource,
            TypeSource typeSource)
        {
            assemblySource.AssertArgumentNotNull(nameof(assemblySource));
            typeSource.AssertArgumentNotNull(nameof(typeSource));

            AssemblySource = assemblySource;
            TypeSource = typeSource;
            Assemblies = assemblySource.ResultAssemblies.NotNull().ToArray();

            Types = TypeSource
                .TypeRegistrations
                .Select(registration => registration.Type)
                .Distinct()
                .ToArray();

            if (assemblySource.FilterByTypeFilters)
                Assemblies = Types.Select(type => type.Assembly).Distinct().ToArray();

            #region Indexes

            string[] typeNames = Types
                .Select(type => type.FullName)
                .Distinct()
                .ToArray();

            if (typeNames.Length != Types.Count)
            {
                TypeByFullName = Types
                    .GroupBy(type => type.FullName)
                    .Select(group => group.First())
                    .ToDictionary(type => type.FullName!, type => type);
            }
            else
            {
                TypeByFullName = Types
                    .ToDictionary(type => type.FullName!, type => type);
            }

            TypeByAlias = TypeSource
                .TypeRegistrations
                .Where(registration => registration.Alias != null)
                .ToDictionary(
                    registration => registration.Alias ?? registration.Type.FullName,
                    registration => registration.Type);

            #endregion
        }

        /// <summary>
        /// Creates <see cref="TypeCache"/> by <see cref="AssemblySource"/> and <see cref="TypeSource"/>.
        /// </summary>
        /// <param name="assemblySource">Assembly filters that was used to get <see cref="Assemblies"/>.</param>
        /// <param name="typeSource">Type filters that was used to get <see cref="Types"/>.</param>
        /// <returns>New <see cref="TypeCache"/> instance.</returns>
        public static TypeCache Create(AssemblySource assemblySource, TypeSource typeSource)
        {
            assemblySource.AssertArgumentNotNull(nameof(assemblySource));
            typeSource.AssertArgumentNotNull(nameof(typeSource));

            IMutableMessageList<Message> messages = new ConcurrentMessageList<Message>();

            Assembly[] assemblies = Reflection.LoadAssemblies(assemblySource.AssemblyFilters, messages).ToArray();

            TypeRegistration[] types = Reflection.GetTypes(assemblies, typeSource.TypeFilters, messages)
                .Select(type => new TypeRegistration(type, source: TypeRegistration.SourceType.AssemblyScan))
                .Concat(typeSource.TypeRegistrations.NotNull())
                .ToArray();

            AssemblySource assemblySourceResolved = assemblySource.With(resultAssemblies: assemblies);
            TypeSource typeSourceResolved = typeSource.With(typeRegistrations: types);

            return new TypeCache(assemblySourceResolved, typeSourceResolved);
        }

        public TypeCache With(
            AssemblySource? assemblySource = null,
            TypeSource? typeSource = null)
        {
            TypeCache source = this;
            return new TypeCache(
                assemblySource: assemblySource ?? source.AssemblySource,
                typeSource: typeSource ?? source.TypeSource);
        }

        public TypeCache WithType(Type type, string alias)
        {
            return With(typeSource: TypeSource.WithTypeRegistration(new TypeRegistration(type, alias: alias)));
        }

        /// <summary>
        /// Determines whether the type cache contains the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>true if the type cache contains the specified type; otherwise, false.</returns>
        public bool Contains(Type type)
        {
            return TypeByFullName.ContainsKey(type.FullName);
        }

        /// <summary>
        /// Determines whether the type cache contains the specified type.
        /// </summary>
        /// <param name="typeFullName">Type name to check.</param>
        /// <returns>true if the type cache contains the specified type; otherwise, false.</returns>
        public bool ContainsByFullName(string typeFullName)
        {
            return TypeByFullName.ContainsKey(typeFullName);
        }

        /// <summary>
        /// Gets type by specified <paramref name="typeFullName"/>.
        /// </summary>
        /// <param name="typeFullName">Type name to check.</param>
        /// <returns>true if the type cache contains the specified type; otherwise, false.</returns>
        public Type? GetByFullName(string typeFullName)
        {
            return TypeByFullName.GetValueOrDefault(typeFullName);
        }

        /// <summary>
        /// Gets type by specified <paramref name="alias"/>.
        /// </summary>
        /// <param name="alias">Type name alias.</param>
        /// <returns>true if the type cache contains the specified type; otherwise, false.</returns>
        public Type? GetByAlias(string alias)
        {
            return TypeByAlias.GetValueOrDefault(alias);
        }

        /// <summary>
        /// Gets Numeric types set.
        /// Types: byte, short, int, long, float, double, decimal.
        /// </summary>
        public static TypeCache NumericTypes { get; } = Create(
            AssemblySource.Empty,
            TypeSource.FromTypes(
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(float),
                typeof(double),
                typeof(decimal)));

        /// <summary>
        /// Gets Extended Numeric types set.
        /// Types: byte, short, int, long, float, double, decimal.
        /// Additional types: sbyte, ushort, uint, ulong.
        /// </summary>
        public static TypeCache NumericTypesExtended { get; } = Create(
            AssemblySource.Empty,
            TypeSource.FromTypes(
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(sbyte),
                typeof(ushort),
                typeof(uint),
                typeof(ulong)));
    }

    /// <summary>
    /// Assembly filters.
    /// </summary>
    public class AssemblyFilters
    {
        public static AssemblyFilters Empty = new AssemblyFilters(
            loadFromDomain: false,
            loadFromDirectory: null,
            includePatterns: null,
            excludePatterns: null);

        public static AssemblyFilters Default = new AssemblyFilters(
            loadFromDomain: true,
            loadFromDirectory: null,
            includePatterns: null,
            excludePatterns: null);

        /// <summary>
        /// Load assemblies from <see cref="AppDomain.CurrentDomain"/>.
        /// </summary>
        public bool LoadFromDomain { get; }

        /// <summary>
        /// Optional load assemblies from provided directory.
        /// </summary>
        public string? LoadFromDirectory { get; }

        /// <summary>
        /// <see cref="Assembly.FullName"/> wildcard include patterns.
        /// <example>MyCompany.*</example>
        /// </summary>
        public IReadOnlyCollection<string>? IncludePatterns { get; }

        /// <summary>
        /// <see cref="Assembly.FullName"/> wildcard exclude patterns.
        /// <example>System.*</example>
        /// </summary>
        public IReadOnlyCollection<string>? ExcludePatterns { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblySource"/> class.
        /// </summary>
        /// <param name="loadFromDomain">Load assemblies from <see cref="AppDomain.CurrentDomain"/>.</param>
        /// <param name="loadFromDirectory">Optional load assemblies from provided directory.</param>
        /// <param name="includePatterns"><see cref="Assembly.FullName"/> wildcard include patterns.</param>
        /// <param name="excludePatterns"><see cref="Assembly.FullName"/> wildcard exclude patterns.</param>
        public AssemblyFilters(
            bool loadFromDomain = false,
            string? loadFromDirectory = null,
            IReadOnlyCollection<string>? includePatterns = null,
            IReadOnlyCollection<string>? excludePatterns = null)
        {
            LoadFromDomain = loadFromDomain;
            LoadFromDirectory = loadFromDirectory;
            IncludePatterns = includePatterns;
            ExcludePatterns = excludePatterns;
        }
    }

    /// <summary>
    /// Assembly source.
    /// </summary>
    public class AssemblySource
    {
        public static AssemblySource Empty = new AssemblySource(assemblyFilters: AssemblyFilters.Empty);

        public static AssemblySource Default = new AssemblySource(assemblyFilters: AssemblyFilters.Default);

        /// <summary>
        /// Filters to filter assemblies.
        /// </summary>
        public AssemblyFilters AssemblyFilters { get; }

        /// <summary>
        /// Take user provided assemblies.
        /// </summary>
        public IReadOnlyCollection<Assembly> Assemblies { get; }

        /// <summary>
        /// Filter assemblies after type filtering and take only assemblies that owns filtered types.
        /// </summary>
        public bool FilterByTypeFilters { get; }

        /// <summary>
        /// Result assemblies.
        /// </summary>
        public IReadOnlyCollection<Assembly>? ResultAssemblies { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblySource"/> class.
        /// </summary>
        /// <param name="assemblyFilters">Filters for assemblies.</param>
        /// <param name="assemblies">User provided assemblies.</param>
        /// <param name="filterByTypeFilters">Filter assemblies after type filtering and take only assemblies that owns filtered types.</param>
        /// <param name="resultAssemblies">Result assemblies.</param>
        public AssemblySource(
            AssemblyFilters assemblyFilters,
            IReadOnlyCollection<Assembly>? assemblies = null,
            bool filterByTypeFilters = true,
            IReadOnlyCollection<Assembly>? resultAssemblies = null)
        {
            AssemblyFilters = assemblyFilters;
            Assemblies = assemblies ?? Array.Empty<Assembly>();
            FilterByTypeFilters = filterByTypeFilters;
            ResultAssemblies = resultAssemblies;
        }

        public AssemblySource With(
            AssemblyFilters? assemblyFilters = null,
            IReadOnlyCollection<Assembly>? assemblies = null,
            bool? filterByTypeFilters = null,
            IReadOnlyCollection<Assembly>? resultAssemblies = null)
        {
            AssemblySource source = this;
            return new AssemblySource(
                assemblyFilters: assemblyFilters ?? source.AssemblyFilters,
                assemblies: assemblies ?? source.Assemblies,
                filterByTypeFilters: filterByTypeFilters ?? source.FilterByTypeFilters,
                resultAssemblies: resultAssemblies ?? source.ResultAssemblies);
        }
    }

    /// <summary>
    /// Type filters.
    /// </summary>
    public class TypeFilters
    {
        /// <summary>
        /// All public types excluding anonymous.
        /// </summary>
        public static readonly TypeFilters Default = new TypeFilters(
            isPublic: true,
            fullNameExcludes: new[] { "<*" });

        /// <summary>
        /// Include only public types.
        /// </summary>
        public bool IsPublic { get; }

        /// <summary>
        /// Include types that <see cref="Type.FullName"/> matches filters.
        /// </summary>
        public IReadOnlyCollection<string>? FullNameIncludes { get; }

        /// <summary>
        /// Exclude types that <see cref="Type.FullName"/> matches filters.
        /// </summary>
        public IReadOnlyCollection<string>? FullNameExcludes { get; }

        /// <summary>
        /// Creates new instance of <see cref="TypeSource"/> class.
        /// </summary>
        /// <param name="isPublic">Include only public types.</param>
        /// <param name="fullNameIncludes">Include types that <see cref="Type.FullName"/> matches filters.</param>
        /// <param name="fullNameExcludes">Exclude types that <see cref="Type.FullName"/> matches filters.</param>
        public TypeFilters(
            bool isPublic = true,
            IReadOnlyCollection<string>? fullNameIncludes = null,
            IReadOnlyCollection<string>? fullNameExcludes = null)
        {
            IsPublic = isPublic;
            FullNameIncludes = fullNameIncludes;
            FullNameExcludes = fullNameExcludes;
        }

        public TypeFilters With(
            TypeFilters source,
            bool? isPublic = null,
            IReadOnlyCollection<string>? fullNameIncludes = null,
            IReadOnlyCollection<string>? fullNameExcludes = null)
        {
            return new TypeFilters(
                isPublic: isPublic ?? source.IsPublic,
                fullNameIncludes: fullNameIncludes ?? source.FullNameIncludes,
                fullNameExcludes: fullNameExcludes ?? FullNameExcludes);
        }
    }

    /// <summary>
    /// Type filters.
    /// </summary>
    public class TypeSource
    {
        /// <summary>
        /// All public types excluding anonymous.
        /// </summary>
        public static readonly TypeSource Default = new TypeSource(
            typeFilters: TypeFilters.Default,
            typeRegistrations: null);

        /// <summary>
        /// Type filters.
        /// </summary>
        public TypeFilters TypeFilters { get; }

        /// <summary>
        /// Defined type registrations.
        /// </summary>
        public IReadOnlyCollection<TypeRegistration> TypeRegistrations { get; }

        /// <summary>
        /// Creates new instance of <see cref="TypeSource"/> class.
        /// </summary>
        /// <param name="typeFilters">Filters to filter types.</param>
        /// <param name="typeRegistrations">User provided registrations.</param>
        public TypeSource(
            TypeFilters typeFilters,
            IReadOnlyCollection<TypeRegistration>? typeRegistrations = null)
        {
            TypeRegistrations = typeRegistrations ?? Array.Empty<TypeRegistration>();
            TypeFilters = typeFilters;
        }

        public TypeSource With(
            TypeFilters? typeFilters = null,
            IReadOnlyCollection<TypeRegistration>? typeRegistrations = null)
        {
            TypeSource source = this;
            return new TypeSource(
                typeFilters: typeFilters ?? source.TypeFilters,
                typeRegistrations: typeRegistrations ?? source.TypeRegistrations);
        }

        public TypeSource WithTypeRegistration(TypeRegistration registration)
        {
            return With(typeRegistrations: TypeRegistrations.Append(registration).ToArray());
        }

        public static TypeSource FromTypes(params Type[] types)
        {
            TypeRegistration[] typeRegistrations = types.NotNull().Select(type => new TypeRegistration(type)).ToArray();
            return TypeSource.Default.With(typeRegistrations: typeRegistrations);
        }
    }

    /// <summary>
    /// Allows to register type in cache without assembly scanning.
    /// Allows to register type alias.
    /// </summary>
    public class TypeRegistration : ValueObject
    {
        public enum SourceType
        {
            Unknown,
            AssemblyScan,
            Manual
        }

        /// <summary>
        /// Type that should be added to cache.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Optional type alias.
        /// For example for type 'System.Int32' set alias 'int'.
        /// </summary>
        public string? Alias { get; }

        /// <summary>
        /// Type source.
        /// </summary>
        public SourceType Source { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeRegistration"/> class.
        /// </summary>
        /// <param name="type">Type to register.</param>
        /// <param name="source">Type source.</param>
        /// <param name="alias">Optional type alias.</param>
        public TypeRegistration(Type type, string? alias = null, SourceType source = SourceType.Unknown)
        {
            type.AssertArgumentNotNull(nameof(type));

            Type = type;
            Alias = alias;
            Source = source;
        }

        /// <inheritdoc />
        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Type;
            yield return Alias;
        }
    }

    /// <summary>
    /// Extension methods for <see cref="TypeCache"/>.
    /// </summary>
    public static class TypeCacheExtensions
    {
        /// <summary>
        /// Gets type by FullName from <paramref name="typeCache"/>.
        /// </summary>
        /// <param name="typeCache">Source type cache.</param>
        /// <param name="fullName">Type FullName.</param>
        /// <param name="useDefaultGetTypeSearchIfNotFound">Use <see cref="Type.GetType()"/> if type was not found in cache.</param>
        /// <returns>Type or null.</returns>
        public static Type? GetType(
            this TypeCache typeCache,
            string fullName,
            bool useDefaultGetTypeSearchIfNotFound = true)
        {
            Type type = typeCache.TypeByFullName.GetValueOrDefault(fullName);
            if (type != null)
                return type;

            return useDefaultGetTypeSearchIfNotFound ? Type.GetType(fullName) : null;
        }
    }
}
