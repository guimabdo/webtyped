using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebTyped.Abstractions;
using WebTyped.Elements;

namespace WebTyped
{
    public class Model : TsModelBase
    {
        public Model(INamedTypeSymbol modelType, TypeResolver typeResolver, Options options) : base(modelType, typeResolver, options) { }

        //Abstrafting
        public override OutputFileAbstraction GetAbstraction()
        {
            //If external
            if (this.HasCustomMap) { return null; }

            //Static
            if (TypeSymbol.IsStatic) { return null; }

            var abstraction = new ModelAbstraction();
            abstraction.Path = AbstractPath;
            abstraction.Fields = new List<FieldAbstraction>();
            abstraction.AllFields = new List<FieldAbstraction>();


            var context = new ResolutionContext(this);
            abstraction.Type = TypeResolver.Resolve(TypeSymbol, context, true);
            //Members
            var currentTypeSymbol = TypeSymbol;
            HashSet<string> existingFields = new HashSet<string>();
            do
            {
                foreach (var m in currentTypeSymbol.GetMembers())
                {
                    if (existingFields.Contains(m.Name)) { continue; }
                    if (m.Kind != SymbolKind.Field && m.Kind != SymbolKind.Property) { continue; }
                    if (m.IsStatic) { continue; }
                    if (m.DeclaredAccessibility != Accessibility.Public) { continue; }

                    var fieldAbstraction = new FieldAbstraction();

                    var prop = m as IPropertySymbol;
                    var isNullable = TypeResolver.IsNullable(prop.Type) || !prop.Type.IsValueType;
                    var name = Options.KeepPropsCase ? m.Name : m.Name.ToCamelCase();

                    var summary = prop.GetDocumentationCommentXml();
                    if (!string.IsNullOrWhiteSpace(summary))
                    {
                        summary = summary.Split("<summary>").Last().Split("</summary>").First().Trim();
                        fieldAbstraction.Summary = summary;
                    }

                    var typeResolution = TypeResolver.Resolve(prop.Type, context);

                    fieldAbstraction.IsNullable = isNullable;
                    fieldAbstraction.Name = name;
                    fieldAbstraction.TypeDeclaration = typeResolution.Declaration;
                    fieldAbstraction.TypeResolution = typeResolution;


                    //fieldAbstraction.Attributes = m.GetAttributes().Select(a => new AttributeAbstraction { 
                    //    ConstructedFrom = a.AttributeClass.ConstructedFrom.ToString(),
                    //    NamedArguments = a.NamedArguments
                    //    .Select(na => new KeyValuePair<string, NamedArgumentAbstraction>(
                    //        na.Key,
                    //        new NamedArgumentAbstraction
                    //        {
                    //            Kind = na.Value.Kind.ToString(),
                    //            Value = na.Value.Value,
                    //            IsNull = na.Value.IsNull
                    //        }
                    //    ))
                    //    .ToList()
                    //}).ToList();
                    fieldAbstraction.Attributes = new List<AttributeAbstraction>();
                    foreach(var attr in m.GetAttributes())
                    {
                        var attrAbstraction = new AttributeAbstraction();
                        //attrAbstraction.ArgumentsExpressions = new List<string>();
                        attrAbstraction.Arguments = new List<AttributeArgumentAbstraction>();
                        if (attr.ApplicationSyntaxReference == null) //From assembly
                        {
                            attrAbstraction.Name = attr.AttributeClass.Name;
                            if (attrAbstraction.Name.EndsWith("Attribute"))
                            {
                                attrAbstraction.Name = attrAbstraction.Name.Substring(0, attrAbstraction.Name.Length - 9);
                            }

                            foreach(var cArg in attr.ConstructorArguments)
                            {
                                attrAbstraction.Arguments.Add(new AttributeArgumentAbstraction
                                {
                                    Value = cArg.Value.ToString()
                                });
                                //attrAbstraction.ArgumentsExpressions.Add(cArg.Value.ToString());
                            }

                            foreach(var nArg in attr.NamedArguments)
                            {
                                attrAbstraction.Arguments.Add(new AttributeArgumentAbstraction
                                {
                                    //IsNull = nArg.Value.IsNull,
                                    //Kind = nArg.Value.Kind.ToString(),
                                    Name = nArg.Key,
                                    Value = nArg.Value.Value.ToString(),
                                });
                                //attrAbstraction.ArgumentsExpressions.Add($"{nArg.Key} = {nArg.Value.Value.ToString()}");
                            }
                        }
                        else //From Code
                        {

                            var attrSyntax = (attr.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax);
                            attrAbstraction.Name = attrSyntax.Name.ToString();
                            //attrAbstraction.NamedArguments = new List<KeyValuePair<string, NamedArgumentAbstraction>>();
                            //attrAbstraction.NamedArguments = new Dictionary<string, NamedArgumentAbstraction>();
                            if (attrSyntax.ArgumentList != null)
                            {
                                foreach (var arg in attrSyntax.ArgumentList.Arguments)
                                {
                                    if(arg.NameEquals != null)
                                    {
                                        attrAbstraction.Arguments.Add(new AttributeArgumentAbstraction
                                        {
                                            Name = arg.NameEquals.Name.Identifier.ValueText,
                                            Value = arg.Expression.ToFullString()
                                        });
                                    }
                                    else
                                    {

                                    }
                                    //attrAbstraction.ArgumentsExpressions.Add(arg.ToFullString());
                                    attrAbstraction.Arguments.Add(new AttributeArgumentAbstraction
                                    {
                                        Value = arg.ToFullString()
                                    });
                                }
                            }
                        }
                        fieldAbstraction.Attributes.Add(attrAbstraction);
                    }

                    abstraction.AllFields.Add(fieldAbstraction);
                    if (currentTypeSymbol == TypeSymbol)
                    {
                        abstraction.Fields.Add(fieldAbstraction);
                    }
                }
                currentTypeSymbol = currentTypeSymbol.BaseType;
            } while (currentTypeSymbol != null);

            abstraction.Imports = context.GetImports();

            return abstraction;
        }

        private (string file, string content) GenerateOutputModule()
        {
            //var hasConsts = false;
            //var subClasses = new List<INamedTypeSymbol>();
            var sb = new StringBuilder();
            var context = new ResolutionContext(this);
            var level = 0;
            if (!TypeSymbol.IsStatic)
            {

                string inheritance = "";

                if (TypeSymbol.BaseType != null && TypeSymbol.BaseType.SpecialType != SpecialType.System_Object)
                {
                    var inheritanceTypeResolution = TypeResolver.Resolve(TypeSymbol.BaseType, context);
                    if (inheritanceTypeResolution.IsAny)
                    {
                        if (inheritanceTypeResolution.Declaration.Contains("/*"))
                        {
                            inheritance = $"/*extends {inheritanceTypeResolution.Declaration.Replace("any/*", "").Replace("*/", "")}*/";
                        }

                    }
                    else
                    {
                        inheritance = $"extends {inheritanceTypeResolution.Declaration} ";
                    }
                }

                string genericArguments = "";
                if (TypeSymbol.TypeArguments.Any())
                {
                    genericArguments = $"<{string.Join(", ", TypeSymbol.TypeArguments.Select(t => t.Name))}>";
                }

                sb.AppendLine(level, $"export interface {ClassName}{genericArguments} {inheritance}{{");
                foreach (var m in TypeSymbol.GetMembers())
                {
                    if (m.Kind == SymbolKind.NamedType)
                    {
                        //subClasses.Add(m as INamedTypeSymbol);
                        continue;
                    }
                    if (m.IsStatic)
                    {
                        //if (m.Kind == SymbolKind.Field && (m as IFieldSymbol).IsConst)
                        //{
                        //    hasConsts = true;
                        //}
                        continue;
                    }
                    if (m.Kind != SymbolKind.Property) { continue; }
                    if (m.DeclaredAccessibility != Accessibility.Public) { continue; }
                    var prop = m as IPropertySymbol;
                    var isNullable = TypeResolver.IsNullable(prop.Type) || !prop.Type.IsValueType;
                    var name = Options.KeepPropsCase ? m.Name : m.Name.ToCamelCase();

                    var summary = prop.GetDocumentationCommentXml();
                    if (!string.IsNullOrWhiteSpace(summary))
                    {
                        summary = summary.Split("<summary>").Last().Split("</summary>").First().Trim();
                        sb.AppendLine(level + 1, "/**");
                        var lines = summary.Split(System.Environment.NewLine);
                        foreach (var l in lines)
                        {
                            sb.AppendLine(level + 1, $"*{l.Trim()}");
                        }
                        sb.AppendLine(level + 1, "**/");
                    }

                    sb.AppendLine(level + 1, $"{name}{(isNullable ? "?" : "")}: {TypeResolver.Resolve(prop.Type, context).Declaration};");
                }
                sb.AppendLine(level, "}");

                //if (hasConsts)
                //{
                //    sb.AppendLine();
                //}
            }
            //else
            //{
            //    hasConsts = true;
            //}

            ////Static part
            //if (hasConsts
            //             //removing const support for now
            //             //If we enable it again, we should merge these members with KeysAndNames...
            //             && false 
            //             ) {
            //	//https://github.com/Microsoft/TypeScript/issues/17372
            //	//Currently we can't merge namespaces and const enums.
            //	//This is supposed to be a bug.
            //	//Besides, even if this is fixed, I think enums will never be mergeable with interfaces,
            //	//so I think it will be always necessary to discriminate with something like $.
            //	//$ can't be used in C# classes, so will never happen a conflict.
            //	//We transpile C# Consts to TypeScript const enums because this is currently
            //	//the only way to inline values. Consts values direclty inside namespaces/modules are not inlined...
            //	//---R: Now, supporting only scoped types, maybe values dont need to be inlined...

            //	sb.AppendLine(level, $"export class {ClassName} {{");
            //	level++;
            //	foreach (var m in TypeSymbol.GetMembers()) {
            //		var fieldSymbol = (m as IFieldSymbol);
            //		if (fieldSymbol != null && fieldSymbol.IsConst) {
            //			//Consts names should not be changed, they are commonly uppercased both in client and server...
            //			var name = m.Name;
            //			sb.AppendLine(level, $"static readonly {name} = {JsonConvert.SerializeObject(fieldSymbol.ConstantValue)};");
            //		}
            //	}
            //	level--;
            //	sb.AppendLine(level, "}");
            //}


            level--;

            AppendKeysAndNames(sb);

            sb.Insert(0, context.GetImportsText());
            string content = sb.ToString();

            return (OutputFilePath, content);
        }

        public override (string file, string content)? GenerateOutput()
        {
            //If external
            if (this.HasCustomMap) { return null; }
            return GenerateOutputModule();
        }

        public static bool IsModel(INamedTypeSymbol t)
        {
            if (t.BaseType?.SpecialType == SpecialType.System_Enum) { return false; }
            if (t.Name.EndsWith("Controller")) { return false; }
            if (t.BaseType != null && t.BaseType.Name.EndsWith("Controller")) { return false; }
            return true;
        }
    }
}
