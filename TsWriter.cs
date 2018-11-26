using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scs2
{
    class TsWriter
    {
        //public readonly TextWriter Writer = new StringWriter(new StringBuilder());

        private class NodeEnvelope
        {
            public SyntaxNode Node;
            public StringBuilder Body = new StringBuilder();
            public bool SuppressTrivia;
        }

        private readonly Stack<NodeEnvelope> _nodes = new Stack<NodeEnvelope>();
        private readonly Stack<NodeEnvelope> _free = new Stack<NodeEnvelope>();
        private readonly StringBuilder _output = new StringBuilder();

        public StringBuilder Output => _output;

        public TsWriter()
        {
            _nodes.Push(new NodeEnvelope()
            {
                Body = _output
            });
        }

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
            _nodes.Peek().Body.Append(openToken);
            return new Block(this, closeToken);
        }

        private void EndBlock(string closeToken)
        {
            _nodes.Peek().Body.Append(closeToken);
        }

        public void Write(string val)
        {
            _nodes.Peek().Body.Append(val);
        }

        public void SuppressTrivia()
        {
            _nodes.Peek().SuppressTrivia = true;
        }

        public void StartNode(SyntaxNode node)
        {
            NodeEnvelope e = (_free.Count > 0) ? _free.Pop() : new NodeEnvelope();

            e.Node = node;
            e.SuppressTrivia = false;
            e.Body.Clear();

            _nodes.Push(e);
        }

        public void EndNode(SyntaxNode node)
        {
            NodeEnvelope e = _nodes.Pop();
            if (!e.SuppressTrivia)
            {
                WriteLeadingTrivia(e.Node);
            }

            _nodes.Peek().Body.Append(e.Body);

            if (!e.SuppressTrivia)
            {
                WriteTrailingTrivia(e.Node);
            }

            _free.Push(e);
        }

        public void WriteLeadingTrivia(SyntaxNode node)
        {
            var triviaList = node?.GetLeadingTrivia();
            if (triviaList != null)
            {
                foreach (var trivia in triviaList)
                {
                    _nodes.Peek().Body.Append(trivia.ToFullString());
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
                    _nodes.Peek().Body.Append(trivia.ToFullString());
                }
            }
        }
    }
}
