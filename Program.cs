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

/*
            var helloWorldString = root.DescendantNodes()
                                       .OfType<LiteralExpressionSyntax>()
                                       .First();

            var literalInfo = model.GetTypeInfo(helloWorldString);

            var stringTypeSymbol = (INamedTypeSymbol)literalInfo.Type;

            Console.Clear();
            foreach (var name in (from method in stringTypeSymbol.GetMembers()
                                                              .OfType<IMethodSymbol>()
                                  where method.ReturnType.Equals(stringTypeSymbol) &&
                                        method.DeclaredAccessibility ==
                                                   Accessibility.Public
                                  select method.Name).Distinct())
            {
                Console.WriteLine(name);
            }
*/
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
