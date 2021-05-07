using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CodeAnalysis
{
    class Program
    {
        private static ArgumentListSyntax CreateSingleArgument(string value)
        {
            return SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value)))));
        }

        static SyntaxTree CreateTree()
        {
            var consoleWriteline = SyntaxFactory.MemberAccessExpression(
                          SyntaxKind.SimpleMemberAccessExpression,
                          SyntaxFactory.IdentifierName("Console"),
                          SyntaxFactory.IdentifierName("WriteLine"));

            var statement = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(consoleWriteline)
                             .WithArgumentList(CreateSingleArgument("Hello World")));

            var main = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "Main")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .AddBodyStatements(statement);

            var program = SyntaxFactory.ClassDeclaration("Program").AddMembers(main);

            var rootNamespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName("CodingTutorials.Demo"));

            rootNamespace = rootNamespace.AddMembers(program);

            var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"));

            var comment = SyntaxFactory.Comment("// Generated code - do not edit");

            var unit = SyntaxFactory.CompilationUnit()
                                    .AddUsings(usingDirective)
                                    .AddMembers(rootNamespace)
                                    .WithLeadingTrivia(comment);

            return unit.SyntaxTree;
        }

        static void BuildAssembly(SyntaxTree syntaxTree, string assemblyName)
        {
            List<MetadataReference> references = new List<MetadataReference>()
           {
               MetadataReference.CreateFromFile(Assembly.Load("System.Private.CoreLib").Location),
               MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
               MetadataReference.CreateFromFile(Assembly.Load("System.Console").Location)
           };

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                new[] {syntaxTree},
                references,
                new CSharpCompilationOptions(OutputKind.ConsoleApplication)
                );

            Directory.CreateDirectory("Output");

            using FileStream stream = new FileStream(Path.Combine("Output", assemblyName), FileMode.Create);

            EmitResult result = compilation.Emit(stream);

            foreach (var diagnostic in result.Diagnostics)
                Console.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
        }

        static void Main()
        {
            SyntaxTree tree = CreateTree();

            Console.WriteLine(tree.GetRoot().NormalizeWhitespace().ToFullString());

            BuildAssembly(tree, "MyHelloWorld.dll");
        }
    }
}
