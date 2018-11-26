using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;

namespace scs2
{
    class BaseGenerator : Microsoft.CodeAnalysis.CSharp.CSharpSyntaxRewriter
    {
        protected TsWriter _writer;
        protected SemanticModel _model;

        public BaseGenerator(TsWriter writer, SemanticModel model)
        {
            _writer = writer;
            _model = model;
        }

        public void ThrowNotSupportedSyntax(SyntaxNode node)
        {
            throw new ArgumentException($"Not supported token {node.ToString()} in {node.GetLocation().ToString()}");
        }

        public override SyntaxNode Visit(SyntaxNode node)
        {
            _writer.StartNode(node);
            base.Visit(node);
            _writer.EndNode(node);
            return node;
        }
    }

    class FileGenerator : BaseGenerator
    {
        public FileGenerator(TsWriter writer, SemanticModel model)
            : base(writer, model)
        {
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            ClassGenerator.Generate(_writer, _model, node);
            return node;
        }
    }
}
