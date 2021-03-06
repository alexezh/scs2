﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scs2
{
    /// <summary>
    /// processes A.B.C style access and writes TS version
    /// fails for ?. type code (for now)
    /// </summary>
    class MemberAccessGenerator : BaseGenerator
    {
        private string _fullMemberName;
        private INamedTypeSymbol _classSymbol;

        public MemberAccessGenerator(TsWriter writer, SemanticModel model, INamedTypeSymbol classSymbol)
            : base(writer, model)
        {
            _classSymbol = classSymbol;
        }

        internal static void Generate(TsWriter writer, SemanticModel model, INamedTypeSymbol classSymbol, MemberAccessExpressionSyntax node)
        {
            var generator = new MemberAccessGenerator(writer, model, classSymbol);

            generator.VisitMemberAccessExpression(node);

            writer.Write(generator._fullMemberName);
        }

        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var op = node.OperatorToken.ToString();
            if (op != ".")
            {
                ThrowNotSupportedSyntax(node);
            }

            return base.VisitMemberAccessExpression(node);
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            var symbolInfo = _model.GetSymbolInfo(node);

            switch (symbolInfo.Symbol.Kind)
            {
                // case SymbolKind.Property:
            }
            _fullMemberName = (_fullMemberName != null) ? String.Concat(_fullMemberName, ".", node.Identifier.ToString()) : node.Identifier.ToString();
            return node;
        }
    }

    class IdentifierNameScanner : Microsoft.CodeAnalysis.CSharp.CSharpSyntaxRewriter
    {
        private IdentifierNameSyntax _node;

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            _node = node;
            return node;
        }

        public static IdentifierNameSyntax GetLastIdentifier(ExpressionSyntax node)
        {
            var scanner = new IdentifierNameScanner();
            scanner.Visit(node);
            return scanner._node;
        }
    }
}
