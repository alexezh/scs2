using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace scs2
{
    class ClassGenerator : BaseGenerator
    {
        private INamedTypeSymbol _classSymbol;

        public ClassGenerator(TsWriter writer, SemanticModel model, ClassDeclarationSyntax node)
            : base(writer, model)
        {
            _classSymbol = (INamedTypeSymbol)_model.GetDeclaredSymbol(node);
            if (_classSymbol == null)
            {
                ThrowNotSupportedSyntax(node);
            }
        }

        public static void Generate(TsWriter writer, SemanticModel model, ClassDeclarationSyntax node)
        {
            var classVisitor = new ClassGenerator(writer, model, node);

            // do not call Visit (since we already reached class node)
            // call VisitClassDeclaration instead
            classVisitor.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            using (WithTrivia(node))
            {
                _writer.Write("class ");
                _writer.Write(node.Identifier.ToString());
                using (var block = _writer.StartBlock(node.OpenBraceToken.ToFullString(), node.CloseBraceToken.ToFullString()))
                {
                    base.VisitClassDeclaration(node);
                }

                return node;
            }
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            using (WithTrivia(node))
            {
                var tsType = TypeGenerator.Generate(_model, node.Declaration.Type);
                if (node.Declaration.Variables.Count > 1)
                {
                    ThrowNotSupportedSyntax(node);
                }

                Visit(node.Declaration.Variables[0]);
                _writer.Write(": ");
                _writer.Write(tsType);
                _writer.Write(node.SemicolonToken.ToString());

                return node;
            }
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            using (WithTrivia(node))
            {

                _writer.Writer.WriteLine(node.ToString());
                //var accessorList = node.AccessorList;

                return base.VisitPropertyDeclaration(node);
            }
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            return FunctionGenerator.GenerateMethod(_writer, _model, _classSymbol, node);
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            return FunctionGenerator.GenerateMethod(_writer, _model, _classSymbol, node);
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            using (WithTrivia(node))
            {
                var symbolInfo = _model.GetSymbolInfo(node);

                switch (symbolInfo.Symbol.Kind)
                {
                    case SymbolKind.Field:
                        _writer.Write(node.Identifier.ToString());
                        break;
                    case SymbolKind.NamedType:
                        break;
                    default:
                        ThrowNotSupportedSyntax(node);
                        break;
                }

                return node;
            }
        }

        public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            return base.VisitVariableDeclaration(node);
        }

        public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            _writer.Write(node.Identifier.ToString());
            return node;
        }

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            return base.VisitToken(token);
        }
    }
}
