CHANGELOG
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
