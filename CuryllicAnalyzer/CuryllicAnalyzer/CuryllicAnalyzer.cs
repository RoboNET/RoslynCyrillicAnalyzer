using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CuryllicAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CyrillicAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CyrillicAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CyrillicAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Type = new LocalizableResourceString(nameof(Resources.Type), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Method = new LocalizableResourceString(nameof(Resources.Method), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Field = new LocalizableResourceString(nameof(Resources.Field), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Namespace = new LocalizableResourceString(nameof(Resources.Namespace), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Property = new LocalizableResourceString(nameof(Resources.Property), Resources.ResourceManager, typeof(Resources));

        private const string Category = "Naming";
        private Regex regex = new Regex(@"\b(?=[à-ÿÀ-ß¸¨]*[a-zA-Z])(?=[a-zA-Z]*[à-ÿÀ-ß¸¨])[\wà-ÿÀ-ß¸¨]+\b");
        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Field, SymbolKind.Property, SymbolKind.Namespace);
            context.RegisterSyntaxTreeAction(AnalyzeFileNames);
            context.RegisterSyntaxNodeAction(AnalyzeLocals, SyntaxKind.VariableDeclarator);
            context.RegisterCompilationAction(AnalyzeOtherFiles);
        }

        private void AnalyzeOtherFiles(CompilationAnalysisContext compilationAnalysisContext)
        {
            // Find the file with the invalid terms.
            var additionalFiles = compilationAnalysisContext.Options.AdditionalFiles;

            foreach (var additionalFile in additionalFiles)
            {
                var stringBuilder = new StringBuilder();
                SourceText fileText = additionalFile.GetText(compilationAnalysisContext.CancellationToken);

                foreach (var line in fileText.Lines)
                {
                    var lineText = line.ToString();
                    var match = regex.Match(lineText);

                    while (match.Success)
                    {
                        var matchStart = match.Index;
                        var matchEnd = matchStart + match.Length;

                        var symbol = lineText[match.Index];

                        compilationAnalysisContext.ReportDiagnostic(
                            Diagnostic.Create(Rule,
                            Location.Create(additionalFile.Path, TextSpan.FromBounds(matchStart, matchEnd), new LinePositionSpan(LinePosition.Zero, LinePosition.Zero)),
                                additionalFile.Path, symbol, match.Index, "File"));

                        match = match.NextMatch();
                    }
                }
            }
        }

        private void AnalyzeFileNames(SyntaxTreeAnalysisContext syntaxTreeContext)
        {
            var filePath = syntaxTreeContext.Tree.FilePath;

            if (filePath == null)
                return;

            var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);

            if (!string.IsNullOrEmpty(fileName) && IsContainsNonAsciiSymbol(fileName, out char symbol, out int index))
            {
                syntaxTreeContext.ReportDiagnostic(
                    Diagnostic.Create(Rule, Location.Create(syntaxTreeContext.Tree, TextSpan.FromBounds(0, 0)), fileName, symbol, index, "File"));
            }
        }

        private void AnalyzeLocals(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
        {
            var node = syntaxNodeAnalysisContext.Node as VariableDeclaratorSyntax;

            if (node == null)
                return;

            if (node.Parent.AncestorsAndSelf().OfType<FieldDeclarationSyntax>().Any())
            {
                return;
            }
            var name = node.Identifier.Text;

            if (!string.IsNullOrEmpty(name) && IsContainsNonAsciiSymbol(name, out char symbol, out int index))
            {
                var diagnostic = Diagnostic.Create(Rule, node.GetLocation(), name, symbol, index, "Local");

                syntaxNodeAnalysisContext.ReportDiagnostic(diagnostic);
            }

        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            string name = context.Symbol.Name;

            string nameType;
            switch (context.Symbol)
            {
                case INamedTypeSymbol _:
                    nameType = Type.ToString();
                    break;
                case IMethodSymbol method:
                    if (method.MethodKind == MethodKind.PropertyGet || method.MethodKind == MethodKind.PropertySet)
                        return;

                    nameType = Method.ToString();
                    break;
                case IFieldSymbol _:
                    nameType = Field.ToString();
                    break;
                case INamespaceSymbol _:
                    nameType = Namespace.ToString();
                    break;
                case IPropertySymbol _:
                    nameType = Property.ToString();
                    break;
                default:
                    nameType = "Symbol";
                    break;
            }

            if (!string.IsNullOrEmpty(name) && IsContainsNonAsciiSymbol(name, out char symbol, out int index))
            {
                var diagnostic = Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Name, symbol, index, nameType);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsContainsNonAsciiSymbol(string input, out char symbol, out int index)
        {
            byte[] array = Encoding.Unicode.GetBytes(input);

            for (int i = 0; i < array.Length; i += 2)
            {
                if ((array[i] | (array[i + 1] << 8)) > 128)
                {
                    symbol = (char)(array[i] | (array[i + 1] << 8));
                    index = i / 2;
                    return true;
                }
            }

            symbol = (char)0;
            index = 0;

            return false;
        }
    }
}
