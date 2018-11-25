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

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            // return base.VisitFieldDeclaration(node);
            return node;
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            _writer.Writer.WriteLine(node.ToString());
            //var accessorList = node.AccessorList;
            
            return base.VisitPropertyDeclaration(node);
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            return FunctionGenerator.GenerateMethod(_writer, _model, _classSymbol, node);
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            return FunctionGenerator.GenerateMethod(_writer, _model, _classSymbol, node);
        }
    }
}
