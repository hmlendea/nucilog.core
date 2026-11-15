using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace NuciLog.Core.ArchitectureTests
{
    [TestFixture]
    public sealed class ExpressionBodyConventionTests
    {
        // Documented, narrowly scoped exception: the Logger overload matrix deliberately
        // uses expression bodies for the single-statement forwarding overloads and block
        // bodies only for the documented multi-statement detail-combination overloads.
        // A block that contains only a control-flow statement (if/switch/try/loop) is not
        // a single simple statement and is therefore not an expression-body candidate.
        [Test]
        public void GivenProductionSources_WhenChecked_ThenSingleStatementMethodsAreExpressionBodied()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                MethodDeclarationSyntax[] violations = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(method => method.Body is not null && method.Body.Statements.Count == 1)
                    .Where(method => !IsControlFlowStatement(method.Body!.Statements[0]))
                    .ToArray();

                Assert.That(
                    violations.Select(method => method.Identifier.ValueText),
                    Is.Empty,
                    $"{source}: every single-statement method must be expression-bodied (use '=>', never a block body or a temporary local).");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenSingleStatementGettersAreExpressionBodied()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                PropertyDeclarationSyntax[] violations = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<PropertyDeclarationSyntax>()
                    .Where(property => property.ExpressionBody is null && property.AccessorList is not null)
                    .Where(HasSingleStatementGetter)
                    .ToArray();

                Assert.That(
                    violations.Select(property => property.Identifier.ValueText),
                    Is.Empty,
                    $"{source}: computed read-only properties must be expression-bodied (use '=>').");
            }
        }

        private static bool IsControlFlowStatement(StatementSyntax statement)
            => statement is IfStatementSyntax ||
                statement is SwitchStatementSyntax ||
                statement is TryStatementSyntax ||
                statement is ForStatementSyntax ||
                statement is ForEachStatementSyntax ||
                statement is WhileStatementSyntax ||
                statement is DoStatementSyntax ||
                statement is LockStatementSyntax ||
                statement is UsingStatementSyntax;

        private static bool HasSingleStatementGetter(PropertyDeclarationSyntax property)
        {
            if (property.AccessorList is null || property.AccessorList.Accessors.Count != 1)
            {
                return false;
            }

            AccessorDeclarationSyntax accessor = property.AccessorList.Accessors[0];

            return accessor.IsKind(SyntaxKind.GetAccessorDeclaration) &&
                accessor.Body is not null &&
                accessor.Body.Statements.Count == 1;
        }

        private static IReadOnlyList<string> GetProductionSources()
        {
            IReadOnlyList<string> sources = ArchitectureResources.GetResourcePaths(
                ArchitectureResources.ProductionSourcePrefix);

            Assert.That(sources, Is.Not.Empty, "No production source files were found.");

            return sources;
        }

        private static SyntaxTree ParseTree(string source)
            => CSharpSyntaxTree.ParseText(
                ArchitectureResources.ReadResource(ArchitectureResources.ProductionSourcePrefix, source),
                path: source);
    }
}
