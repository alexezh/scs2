using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace scs2
{

    class FunctionGenerator : BaseGenerator
    {
        private INamedTypeSymbol _classSymbol;

        public FunctionGenerator(TsWriter writer, SemanticModel model, INamedTypeSymbol classSymbol)
            : base(writer, model)
        {
            _classSymbol = classSymbol;
        }

        public static SyntaxNode GenerateMethod(TsWriter writer, SemanticModel model, INamedTypeSymbol classSymbol, MethodDeclarationSyntax node)
        {
            var visitor = new FunctionGenerator(writer, model, classSymbol);

            writer.Write(node.Identifier.ToString());
            visitor.Visit(node.ParameterList);

            writer.Write(": ");
            writer.Write(node.ReturnType.ToString());
            visitor.Visit(node.Body);
            return node;
        }

        public static SyntaxNode GenerateMethod(TsWriter writer, SemanticModel model, INamedTypeSymbol classSymbol, ConstructorDeclarationSyntax node)
        {
            var visitor = new FunctionGenerator(writer, model, classSymbol);

            writer.Write("constructor");
            visitor.Visit(node.ParameterList);

            visitor.Visit(node.Body);
            return node;
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            using (var block = _writer.StartBlock(node.OpenBraceToken.ToFullString(), node.CloseBraceToken.ToFullString()))
            {
                return base.VisitBlock(node);
            }
        }

        public override SyntaxNode VisitParameterList(ParameterListSyntax node)
        {
            using (var block = _writer.StartBlock(node.OpenParenToken.ToFullString(), node.CloseParenToken.ToString()))
            {
                bool addSeparator = false;
                foreach (var param in node.Parameters)
                {
                    if (addSeparator)
                    {
                        _writer.Write(",");
                    }
                    Visit(param);
                    addSeparator = true;
                }
            }

            return node;
        }

        public override SyntaxNode VisitParameter(ParameterSyntax node)
        {
            _writer.Write(node.Identifier.ToString());
            _writer.Write(": ");
            TypeGenerator.Generate(_writer, _model, node.Type);

            return node;
        }

        public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            Visit(node.Left);
            var op = node.OperatorToken.ToString();
            switch (op)
            {
                case "=":
                case "+=":
                    _writer.Write(op);
                    break;
                default:
                    ThrowNotSupportedSyntax(node); return null;
            }
            Visit(node.Right);

            return node;
        }

        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            _writer.Write("if");
            using (var block = _writer.StartBlock(node.OpenParenToken.ToFullString(), node.CloseParenToken.ToFullString()))
            {
                Visit(node.Condition);
            }

            Visit(node.Statement);
            if (node.Else != null)
            {
                Visit(node.Else);
            }

            return node;
        }

        public override SyntaxNode VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            return base.VisitConditionalExpression(node);
        }

        public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            Visit(node.Left);
            string op = node.OperatorToken.ToString();
            if (op == "==")
            {
                _writer.Write("===");
            }
            else
            {
                ThrowNotSupportedSyntax(node);
            }
            Visit(node.Right);

            return node;
        }

        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            MemberAccessGenerator.Generate(_writer, _model, _classSymbol, node);
            return node;
        }

        public override SyntaxNode VisitSingleVariableDesignation(SingleVariableDesignationSyntax node)
        {
            return base.VisitSingleVariableDesignation(node);
        }

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            base.VisitExpressionStatement(node);
            if (node.SemicolonToken != null)
            {
                _writer.Write(";");
            }

            return node;
        }

        public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (node.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NumericLiteralExpression))
            {
                _writer.Write(node.ToString());
            }
            else if (node.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression))
            {
                _writer.Write($"\"{node.ToString()}\"");
            }

            return node;
        }

        public override SyntaxNode VisitRefValueExpression(RefValueExpressionSyntax node)
        {
            return base.VisitRefValueExpression(node);
        }

        public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            return base.VisitVariableDeclarator(node);
        }

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            return base.VisitToken(token);
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            var symbolInfo = _model.GetSymbolInfo(node);

            switch (symbolInfo.Symbol.Kind)
            {
                case SymbolKind.Field:
                    {
                        string name = "this." + node.Identifier.ToString();
                        _writer.Write(name);
                        break;
                    }
                case SymbolKind.Parameter:
                    // otherwise this is primitive type of some sort
                    _writer.Write(node.Identifier.ToString());
                    break;
                default:
                    ThrowNotSupportedSyntax(node);
                    break;
            }

            return node;
        }
    }
}
