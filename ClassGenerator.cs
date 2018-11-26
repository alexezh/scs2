using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace scs2
{
    class BaseListGenerator : BaseGenerator
    {
        public BaseListGenerator(TsWriter writer, SemanticModel model)
            : base(writer, model)
        {
        }

        public static void Generate(TsWriter writer, SemanticModel model, BaseListSyntax baseList)
        {
            bool addSeparator = false;
            var generator = new BaseListGenerator(writer, model);

            foreach (var baseType in baseList.Types)
            {
                if (addSeparator)
                {
                    writer.Write(", ");
                }
                addSeparator = true;

                generator.Visit(baseType);
            }
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            var symbolInfo = _model.GetSymbolInfo(node);

            switch (symbolInfo.Symbol.Kind)
            {
                case SymbolKind.NamedType:
                    _writer.Write(node.Identifier.ToString());
                    break;
                default:
                    ThrowNotSupportedSyntax(node);
                    break;
            }

            return node;
        }
    }

    class PropertyDeclarationGenerator : BaseGenerator
    {
        private INamedTypeSymbol _classSymbol;
        private string _propertyName;

        public PropertyDeclarationGenerator(TsWriter writer, SemanticModel model, INamedTypeSymbol classSymbol)
            : base(writer, model)
        {
            _classSymbol = classSymbol;
        }

        public static void Generate(TsWriter writer, SemanticModel model, INamedTypeSymbol classSymbol, PropertyDeclarationSyntax node)
        {
            var generator = new PropertyDeclarationGenerator(writer, model, classSymbol);

            generator._propertyName = node.Identifier.ToString();

            if (node.AccessorList != null)
            {
                generator.Visit(node.AccessorList);
            }
            else if (node.ExpressionBody != null)
            {
                generator.Visit(node.ExpressionBody);
            }
            else
            {
                generator.ThrowNotSupportedSyntax(node);
            }
        }

        public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            return base.VisitAccessorDeclaration(node);
        }

        public override SyntaxNode VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
        {
            _writer.Write($"get_{_propertyName}()");

            using (_writer.StartBlock("{", "}"))
            {
                FunctionGenerator.GenerateMethod(_writer, _model, _classSymbol, node.Expression);
            }

            return node;
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            var symbolInfo = _model.GetSymbolInfo(node);
            if (symbolInfo.Symbol == null)
            {
                ThrowNotSupportedSyntax(node);
            }

            switch (symbolInfo.Symbol.Kind)
            {
                case SymbolKind.Field:
                    _writer.Write(node.Identifier.ToString());
                    break;
                case SymbolKind.NamedType:
                    _writer.Write(node.Identifier.ToString());
                    break;
                case SymbolKind.Parameter:
                    _writer.Write(node.Identifier.ToString());
                    break;
                default:
                    ThrowNotSupportedSyntax(node);
                    break;
            }

            return node;
        }
    }

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
            _writer.Write("class ");
            _writer.Write(node.Identifier.ToString());

            if (node?.BaseList?.Types != null && node.BaseList.Types.Count > 0)
            {
                _writer.Write(" extends ");

                BaseListGenerator.Generate(_writer, _model, node.BaseList);
            }

            _writer.SuppressTrivia();
            using (var block = _writer.StartBlock(node.OpenBraceToken.ToFullString(), node.CloseBraceToken.ToFullString()))
            {
                base.VisitClassDeclaration(node);
            }

            return node;
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
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

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            PropertyDeclarationGenerator.Generate(_writer, _model, _classSymbol, node);
            return node;
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
