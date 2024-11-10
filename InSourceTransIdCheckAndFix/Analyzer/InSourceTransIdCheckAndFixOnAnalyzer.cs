
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace InSourceTransIdCheckAndFix.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InSourceTransIdCheckAndFixOnAnalyzer : BaseInSourceTransIdCheckAndFixAnalyzer
    {
        public const string DiagnosticId = "DTK002";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitleOn), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatOn), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionOn), Resources.ResourceManager, typeof(Resources));
        private const string Category = "DevTextToKeyMapper";

        private static readonly DiagnosticDescriptor OnRule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(OnRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                // Use the configuration to set up any other analysis components
                if (ConfigurationLoader.TryLoad(compilationStartContext.Options, out var config))
                {
                    // Pass the configuration to other analysis components if needed
                    compilationStartContext.RegisterSyntaxNodeAction(nodeContext => AnalyzeSyntaxNode(nodeContext, config!), SyntaxKind.InvocationExpression);
                }
            });
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context, DevTextToKeyMapperConfig config)
        {
            // Look for method calls like e.g. Trs.On("Key", "Text").
            var invocationExpr = context.Node as InvocationExpressionSyntax;

            if (invocationExpr?.Expression is not MemberAccessExpressionSyntax memberAccessExpr)
            {
                return;
            }

            var methodName = memberAccessExpr.Name.Identifier.Text;
            var targetClass = (memberAccessExpr.Expression as IdentifierNameSyntax)?.Identifier.Text;


            if (targetClass != config.MapperClassName || methodName != config.OnMethodName)
            {
                return;
            }

            // Extract arguments and check key-text mismatch
            var arguments = invocationExpr.ArgumentList.Arguments;
            if (arguments.Count != 2 ||
                arguments[0].Expression is not LiteralExpressionSyntax keyLiteral ||
                arguments[1].Expression is not LiteralExpressionSyntax textLiteral)
            {
                return;
            }

            var devTextKeyInSrc = keyLiteral.Token.ValueText;
            var devTextInSrc = textLiteral.Token.ValueText;

            // Use the external interface to verify if the key matches the text
            if (config.DevTextManager.VerifyKeyMatchesText(devTextKeyInSrc, devTextInSrc))
            {
                return;
            }

            var diagnostic = Diagnostic.Create(OnRule, memberAccessExpr.GetLocation(), devTextKeyInSrc, devTextInSrc);
            context.ReportDiagnostic(diagnostic);
        }

    }
}
