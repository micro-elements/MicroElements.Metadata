# 2.0.0-beta.4
- ExcelReportBuilder support for SharedStrings
- Fixed broken copy and paste in generated excel

# 2.0.0-beta.3
- Validation customization for last rule in chain
- NotNull for Nullable struct
- Added predefined search modes: Default, ExistingOnly, ExistingOnlyWithParent
- Search clean up.

# 2.0.0-beta.2
- Property search redesigned and unified
- Search API became more consistent and customizable
- PropertyContainer and MutablePropertyContainer can accept custom SearchOptions

# 2.0.0-beta.1
- Property search allows to know whether property exists, not exists or exists with undefined value
- New validation rules: Exists, Required
- IValidationRule now implements IMetadataProvider
- Added generic IValidationRule<T> for holding typed property
- Rich rule building with combine support
- IConfigurableValidationRule removed
- Validation message customization moved to ValidationMessageOptions and linked to IValidationRule as metadata

# 1.3.0
- IParserProvider: convert Parsers property to GetParsers method to be more functional
- Added PropertyParser extensions to create property parsers in fluent parser providers
- Added CachedParserProvider and Cached extension

# 1.2.0
- ValidationResult became readonly struct
- Added CachedValidator and Cached extension

# 1.1.0
- ValidationResult became generic
- ValidationResult.IsValid moved to extension methods
- ToValidationResult and ToValidationResults extensions

# 1.0.0
- ExcelReader added
- Validation added

# 0.14.0
- Extensions for easy excel styling

# 0.13.0
- Document customization fixed (was not applyed)

# 0.12.0
- ExcelReportBuilder sheet customizization after adding data
- FreezeTopRow moved to extension methods

# 0.11.0
- ExcelReportBuilder customize for document, sheet, column, cell

# 0.9.4
- Property Map now can be used for classes and structs
- GetPropertyValue method added new argument: useDefaultValue. So it can return null now
- ExcelMetadata moved to property model for easy extend
- Excel generator: added Transpose for Sheets

# 0.9.3
- Unified Property methods with prefixes Set, SetUntyped, With

# 0.9.2
- EnumerableReportProvider
- Property Map methods to convert property to property of other type with func
- Property WithName extension method 

# 0.9.1
- IReportProvider is now IMetadataProvider
- ExcelMetadata hierarchy

# 0.9.0
- CsvReportBuilder
- ExcelReportBuilder
- ExcelMetadata for document, sheet, column, cell

# 0.8.1
- PropertyRenderer by default renders in Invariant culture.

# 0.8.0
- ValueSource converted to class instead of enum to extend on client side.

# 0.7.0
- SetValueUntyped and WithValueUntyped extensions instead of WithValue and SetValue to reduce type errors

# 0.6.1
- Added PropertySetEnumerable and PropertySetBase

# 0.6.0
- IProperty, IPropertySet implements IEnumerable
- LocalizableString implements IEnumerable

# 0.5.0
- IPropertySet, IPropertyContainerMapper

# 0.4.0
- Added ConfigureMetadata extension for IMetadataPrivider
- Added AddRange extension for IMutablePropertyContainer

# 0.3.3
- Now returns null for DynamicContainer is property was not found

# 0.3.2
- GetDynamicMemberNames implemented for DynamicContainer

# 0.3.1
- Default value for GetMetadata
- Options for AsDynamic and DynamicContainer

# 0.3.0
- Many methods for untyped work
- Builders for properties
- Dynamic wrapper

# 0.2.x
- Added PropertyMapper
- Added initial table impl
- IMetadataProvider changed
- PropertyList removed in favor of PropertyContainer

# 0.1.0
- Initial version

Full release notes can be found at: https://github.com/micro-elements/MicroElements.MetadataModel.git/blob/master/CHANGELOG.md
