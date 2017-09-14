using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace CyrillicAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CyrillicAnalyzerCodeFixProvider)), Shared]
    public class CyrillicAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Make uppercase";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CyrillicAnalyzerAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var memberDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<CSharpSyntaxNode>().First();

            string identifierText;

            switch (memberDeclaration)
            {
                case TypeDeclarationSyntax declaration:
                    identifierText = declaration.Identifier.Text;
                    break;
                case MethodDeclarationSyntax declaration:
                    identifierText = declaration.Identifier.Text;
                    break;
                case VariableDeclaratorSyntax declaration:
                    identifierText = declaration.Identifier.Text;
                    break;
                case IdentifierNameSyntax declaration:
                    identifierText = declaration.Identifier.Text;
                    memberDeclaration = declaration.Parent as NamespaceDeclarationSyntax;
                    if (memberDeclaration == null)
                        return;
                    break;
                case PropertyDeclarationSyntax declaration:
                    identifierText = declaration.Identifier.Text;
                    break;
                default:
                    return;
            }
            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedSolution: c => MakeUppercaseAsync(context.Document, memberDeclaration, identifierText, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Solution> MakeUppercaseAsync(Document document, CSharpSyntaxNode typeDecl, string identifier, CancellationToken cancellationToken)
        {
            // Compute new uppercase name.
            var newName = RemoveNonAsciiSymbols(identifier);

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }

        private string RemoveNonAsciiSymbols(string source)
        {
            var builder = new StringBuilder();
            byte[] array = Encoding.Unicode.GetBytes(source);

            for (int i = 0; i < array.Length; i += 2)
            {
                if (((array[i]) | (array[i + 1] << 8)) <= 128)
                {
                    builder.Append((char)(array[i] | (array[i + 1] << 8)));
                }
            }
            return builder.ToString();
        }
    }
}