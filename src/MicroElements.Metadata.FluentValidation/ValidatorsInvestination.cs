using System;
using System.Collections.Generic;
using System.Text;

namespace MicroElements.Metadata.FluentValidation
{
    /*
     * -----------------------------------------------------------------------------------
     *            | Exists.NotNull | Exists.Null | Exists.Empty | NotExists   | Default?, Collection?
     * -----------------------------------------------------------------------------------
     * Required   | true           | true        | true         | false       | 
     * [Required] | true           | false       | false        | false       | 
     * NotNull    | true           | false       | false        | false       | 
     * NotEmpty   | true           | false       | false        | false       | 
     *-------------------------------------------------------------------------------------
     *
     * OpenAPI 3.0 does not have an explicit null type as in JSON Schema, but you can use nullable: true to specify that the value may be null. Note that null is different from an empty string "".
     * https://github.com/OAI/OpenAPI-Specification/blob/master/proposals/003_Clarify-Nullable.md
     * https://stackoverflow.com/questions/48111459/how-to-define-a-property-that-can-be-string-or-null-in-openapi-swagger
     *
     * https://github.com/OAI/OpenAPI-Specification/releases/tag/3.1.0-rc0
     *
     * JsonSchema and OpenApi 3.1
     * {
     *     "type": ["string", "null"],
     *     "maxLength": 255
     * }
     *
     * OpenAPI 3.0
     * {
     *     "type": "string",
     *     "nullable" : true
     *     "maxLength": 255
     * }
     *
     */

    // FV_NotEmpty: NotNull | NotDefault | NotEmptyCollection
    // NotNull->IAllowNull(false), NotDefault->IAllowDefault(false), NotEmptyCollection->IMinItems(>0)

    // From FluentValidation
    //protected override bool IsValid(PropertyValidatorContext context)
    //{
    //    switch (context.PropertyValue)
    //    {
    //        case null:
    //        case string s when string.IsNullOrWhiteSpace(s):
    //        case ICollection c when c.Count == 0:
    //        case Array a when a.Length == 0:
    //        case IEnumerable e when !e.Cast<object>().Any():
    //            return false;
    //    }
    //    if (Equals(context.PropertyValue, _defaultValueForType))
    //    {
    //        return false;
    //    }
    //    return true;
    //}
}
