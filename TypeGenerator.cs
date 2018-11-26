using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scs2
{
    /// <summary>
    /// takes type node as input (such as int or Foo) and generates TS form
    /// </summary>
    class TypeGenerator : BaseGenerator
    {
        public TypeGenerator(TsWriter writer, SemanticModel model)
            : base(writer, model)
        {
        }

        public static string Generate(SemanticModel model, SyntaxNode node)
        {
            var writer = new TsWriter();
            var generator = new TypeGenerator(writer, model);
            generator.Visit(node);
            return writer.Output.ToString();
        }

        public override SyntaxNode VisitPointerType(PointerTypeSyntax node)
        {
            return base.VisitPointerType(node);
        }

        public override SyntaxNode VisitRefType(RefTypeSyntax node)
        {
            return base.VisitRefType(node);
        }

        public override SyntaxNode VisitTypeCref(TypeCrefSyntax node)
        {
            return base.VisitTypeCref(node);
        }

        public override SyntaxNode VisitPredefinedType(PredefinedTypeSyntax node)
        {
            _writer.Write(ToTsType(node));
            return node;
        }

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            return base.VisitToken(token);
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            var symbolInfo = _model.GetSymbolInfo(node);
            if (symbolInfo.Symbol.Kind != SymbolKind.NamedType)
            {
                ThrowNotSupportedSyntax(node);
            }

            _writer.Write(node.Identifier.ToString());

            return base.VisitIdentifierName(node);
        }

        private string ToTsType(PredefinedTypeSyntax node)
        {
            switch (node.ToString())
            {
                case "int": return "number";
                case "string": return "string";
                case "void": return "void";
                default: ThrowNotSupportedSyntax(node); return "";
            }
        }
    }
}
