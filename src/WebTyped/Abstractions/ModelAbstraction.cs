using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebTyped.Abstractions
{
    public class OutputFileAbstraction
    {
        public string Kind { get => this.GetType().Name.Replace("Abstraction", ""); }
        public string Path { get; set; }
    }

    public class AttributeAbstraction
    {
        //Most of time we could not resolve the exact type
        //public string ConstructedFrom { get; set; }
        //So we only get simple name from syntax
        public string Name { get; set; }

        //public List<KeyValuePair<string, NamedArgumentAbstraction>> NamedArguments { get; set; }
        //public Dictionary<string, NamedArgumentAbstraction> NamedArguments { get; set; }
        //public List<string> ArgumentsExpressions { get; set; }
        public List<AttributeArgumentAbstraction> Arguments { get; set; }
    }

    public class AttributeArgumentAbstraction
    {
        public string Name { get; set; }

        //public string Kind { get; set; }
        //public bool IsNull { get; set; }
        public string Value { get; set; }
    }

    public class ModelAbstraction : OutputFileAbstraction
    {
        /// <summary>
        /// List of imported types
        /// </summary>
        //public object Imports { get; set; }

        public TypeResolution Type { get; set; }

        public IDictionary<string, string> Imports { get; set; }

        /// <summary>
        /// Model name
        /// </summary>
        //public string Name { get; set; }
        //public string ClassDeclaration { get; set; }


        public List<FieldAbstraction> Fields { get; set; }

        public List<FieldAbstraction> AllFields { get; set; }

        //public object Extends { get; set; }
    }

    public class EnumValueAbstraction
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class EnumAbstraction : OutputFileAbstraction
    {
        public string Name { get; set; }
        public List<EnumValueAbstraction> Values { get; set; }
    }

    public class ServiceAbstraction : OutputFileAbstraction
    {
        public string ClassName { get; set; }

        public string ControllerName { get; set; }

        public string Endpoint { get; set; }

        public List<ActionAbstraction> Actions { get; set; }

        public IDictionary<string, string> Imports { get; set; }
    }

    public class ParameterAbstraction
    {
        public string Name { get; set; }
        public bool IsOptional { get; set; }

        //public string TypeDescription { get; set; }
        public TypeResolution Type { get; set; }
    }

    public class ActionAbstraction
    {
        public string FunctionName { get; set; }

        public string ActionName { get; set; }

        public string HttpMethod { get; set; }

        public List<ParameterAbstraction> Parameters { get; set; }

        public string BodyParameterName { get; set; }

        public List<string> SearchParametersNames { get; set; }

        public TypeResolution ReturnType { get; set; }
    }

    public class FieldAbstraction
    {
        public string Name { get; set; }

        public string Summary { get; set; }

        public bool IsNullable { get; set; }

        public string TypeDeclaration { get; set; }

        public TypeResolution TypeResolution { get; set; }
        
        public List<AttributeAbstraction> Attributes { get; set; }

        public string FromQuery {
            get {
                var fromAttr = Attributes.FirstOrDefault(a => a.Name == "FromQuery" || a.Name == "FromUri");
                return fromAttr?.Arguments.FirstOrDefault()?.Value ?? Name;
            }
        }

        public string FromRoute {
            get {
                var fromAttr = Attributes.FirstOrDefault(a => a.Name == "FromRoute");
                if(fromAttr == null) { return null; }
                return fromAttr?.Arguments.FirstOrDefault()?.Value ?? Name;
            }
        }
    }

    //public class TypeAbstraction
    //{
    //    /// <summary>
    //    /// Type name
    //    /// </summary>
    //    public string Name { get; set; }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public TypeModuleAbstraction Module { get; set; }
    //}

    //public class NameAbstraction
    //{
    //    public NameAbstraction(string name)
    //    {
    //        Value = name;
    //        CamelCaseValue = name.ToCamelCase();
    //    }

    //    public string Value { get; set; }

    //    public string CamelCaseValue { get; set; }
    //}
}
