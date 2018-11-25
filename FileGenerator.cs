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
    public enum TsAccess
    {
        Public,
        Private,
    }

    public class TsMethod
    {
        public TsAccess Access;
        public string Name;
        public string Body;
    }

    class TsWriter
    {
        public readonly TextWriter Writer = new StringWriter(new StringBuilder());

        public class Block : IDisposable
        {
            private TsWriter _writer;
            private string _closeToken;

            internal Block(TsWriter writer, string closeToken)
            {
                _writer = writer;
                _closeToken = closeToken;
            }

            public void Dispose()
            {
                _writer.EndBlock(_closeToken);
            }
        }

        public Block StartBlock(string openToken, string closeToken)
        {
            Writer.Write(openToken);
            return new Block(this, closeToken);
        }

        private void EndBlock(string closeToken)
        {
            Writer.Write(closeToken);
        }

        public void Write(string val)
        {
            Writer.Write(val);
        }
    }

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
            WriteLeadingTrivia(node);
            base.Visit(node);
            WriteTrailingTrivia(node);
            return node;
        }

        public void WriteLeadingTrivia(SyntaxNode node)
        {
            var triviaList = node?.GetLeadingTrivia();
            if (triviaList != null)
            {
                foreach (var trivia in triviaList)
                {
                    trivia.WriteTo(_writer.Writer);
                }
            }
        }

        public void WriteTrailingTrivia(SyntaxNode node)
        {
            var triviaList = node?.GetTrailingTrivia();
            if (triviaList != null)
            {
                foreach (var trivia in triviaList)
                {
                    trivia.WriteTo(_writer.Writer);
                }
            }
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
