using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace scs2
{
    class Program
    {
        static void Main(string[] args)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(
          @"using System;
using System.Collections;
using System.Linq;
using System.Text;
 
namespace HelloWorld
{
    // well
    public class Foo
    {
        private int _val;
        private int _val2;

        public int Val => _val;

        public int Val2
        {
            get 
            {
                return _val2;
            }
            set
            {
                _val2 = value;
            }
        }

        public void Copy(Foo other)
        {
            _val = other._val;
        }

        // method comment
        public Foo(int val)
        {
            _val = val;
        }

        public void Add(int val)
        {
            _val += val;
            // just checking
            if(_val == 4)
            {
                _val = 5;
            }
        }

        public string Print(string s1, string s2 /* param comment */)
        {
            return String.Concat(s1, s2);
        }
    }

    class Bar : Foo
    {
        private string _name;

        public Bar(int val, string name)
            : base(val)
        {
            _name = name;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var bar = new Bar();
            bar.Val2 = 3;

            Console.WriteLine(""Hello, World!"");
        }
    }
}");

            var root = (CompilationUnitSyntax)tree.GetRoot();


            var compilation = CSharpCompilation.Create("TypeScript")
                                               .AddReferences(
                                                    MetadataReference.CreateFromFile(
                                                        typeof(object).Assembly.Location))
                                               .AddSyntaxTrees(tree);

            var model = compilation.GetSemanticModel(tree);

            GenerateTypeScript(model, root);

            Console.WriteLine(">>>> done <<<< ");
        }

        static void GenerateTypeScript(SemanticModel model, CompilationUnitSyntax syntaxTree)
        {
            // now convert 
            var tsWriter = new TsWriter();
            var fv = new FileGenerator(tsWriter, model);

            fv.Visit(syntaxTree);

            Console.Write(tsWriter.Output);
        }
    }
}
