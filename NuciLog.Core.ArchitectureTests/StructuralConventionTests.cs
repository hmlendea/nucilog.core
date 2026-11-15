using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace NuciLog.Core.ArchitectureTests
{
    /// <summary>
    /// Enforces structural coding conventions that are not covered by the dedicated
    /// fixtures: braced control-flow bodies, contiguous overload grouping, no
    /// <c>ref</c>/<c>out</c> parameters, public-method placement in documented eligible
    /// types, and the absence of public fields.
    /// </summary>
    [TestFixture]
    public sealed class StructuralConventionTests
    {
        // Scoped exception: the ILogger contract deliberately uses the neutral parameter
        // names 'details' and 'details2', while the Logger implementation uses the
        // descriptive 'logInfos' and 'extraLogInfos'. Renaming either surface would change
        // the public named-argument contract, so parameter-name parity is left to review.
        [Test]
        public void GivenProductionSources_WhenChecked_ThenEveryControlFlowBodyUsesBraces()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);
                SyntaxNode root = tree.GetRoot();

                SyntaxNode[] unbraced = root.DescendantNodes()
                    .OfType<IfStatementSyntax>()
                    .Where(statement => statement.Statement is not BlockSyntax)
                    .Select(statement => (SyntaxNode)statement)
                    .Concat(root.DescendantNodes().OfType<ElseClauseSyntax>()
                        .Where(clause => clause.Statement is not BlockSyntax && clause.Statement is not IfStatementSyntax)
                        .Select(clause => (SyntaxNode)clause))
                    .Concat(root.DescendantNodes().OfType<ForStatementSyntax>()
                        .Where(statement => statement.Statement is not BlockSyntax)
                        .Select(statement => (SyntaxNode)statement))
                    .Concat(root.DescendantNodes().OfType<ForEachStatementSyntax>()
                        .Where(statement => statement.Statement is not BlockSyntax)
                        .Select(statement => (SyntaxNode)statement))
                    .Concat(root.DescendantNodes().OfType<WhileStatementSyntax>()
                        .Where(statement => statement.Statement is not BlockSyntax)
                        .Select(statement => (SyntaxNode)statement))
                    .Concat(root.DescendantNodes().OfType<DoStatementSyntax>()
                        .Where(statement => statement.Statement is not BlockSyntax)
                        .Select(statement => (SyntaxNode)statement))
                    .ToArray();

                Assert.That(
                    unbraced,
                    Is.Empty,
                    $"{source}: every control-flow body must use braces.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenOverloadsAreGroupedContiguously()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                foreach (TypeDeclarationSyntax typeDeclaration in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    string[] names = typeDeclaration.Members
                        .OfType<MethodDeclarationSyntax>()
                        .Select(method => method.Identifier.ValueText)
                        .ToArray();

                    List<string> seen = [];
                    List<string> reopened = [];

                    foreach (string name in names)
                    {
                        if (seen.Count > 0 && seen[seen.Count - 1] != name && seen.Contains(name))
                        {
                            reopened.Add(name);
                        }

                        seen.Add(name);
                    }

                    string[] distinctReopened = reopened.Distinct(StringComparer.Ordinal).ToArray();

                    Assert.That(
                        distinctReopened,
                        Is.Empty,
                        $"{source}: overloads of '{string.Join(", ", distinctReopened)}' in '{typeDeclaration.Identifier.ValueText}' must be grouped contiguously.");
                }
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenNoRefOrOutParametersAreUsed()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                ParameterSyntax[] refOrOut = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ParameterSyntax>()
                    .Where(parameter => parameter.Modifiers.Any(modifier =>
                        modifier.IsKind(SyntaxKind.RefKeyword) ||
                        modifier.IsKind(SyntaxKind.OutKeyword)))
                    .ToArray();

                Assert.That(
                    refOrOut,
                    Is.Empty,
                    $"{source}: ref and out parameters are not allowed.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenEveryPublicMethodIsDeclaredInAnEligibleType()
        {
            string[] eligibleTypes = ["Logger", "LogInfoKey", "LogMessageBuilder"];

            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                string[] ineligibleTypes = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(classDeclaration => classDeclaration.Members
                        .OfType<MethodDeclarationSyntax>()
                        .Any(method => method.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PublicKeyword))))
                    .Select(classDeclaration => classDeclaration.Identifier.ValueText)
                    .Where(name => !eligibleTypes.Contains(name, StringComparer.Ordinal))
                    .ToArray();

                Assert.That(
                    ineligibleTypes,
                    Is.Empty,
                    $"{source}: only the documented contract and formatter types may declare public methods.");
            }
        }

        [Test]
        public void GivenProductionSources_WhenChecked_ThenNoPublicFieldsAreDeclared()
        {
            foreach (string source in GetProductionSources())
            {
                SyntaxTree tree = ParseTree(source);

                FieldDeclarationSyntax[] publicFields = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<FieldDeclarationSyntax>()
                    .Where(field => field.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)))
                    .ToArray();

                Assert.That(
                    publicFields,
                    Is.Empty,
                    $"{source}: public fields are not allowed; expose properties instead.");
            }
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
