using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace InSourceTransIdCheckAndFix.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InSourceTransIdCheckAndFixToDoAnalyzer : BaseInSourceTransIdCheckAndFixAnalyzer
{

    public const string DiagnosticId = "DTK001";

    // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
    // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitleToDo), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatToDo), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescriptionToDo), Resources.ResourceManager, typeof(Resources));
    private const string Category = "DevTextToKeyMapper";

    private static readonly DiagnosticDescriptor ToDoRule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ToDoRule);

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
        try
        {
            // Look for method calls like e.g. for Trs.ToDo().
            var invocationExpr = context.Node as InvocationExpressionSyntax;

            if (invocationExpr?.Expression is not MemberAccessExpressionSyntax memberAccessExpr)
            {
                return;
            }

            var methodName = memberAccessExpr.Name.Identifier.Text;
            var targetClass = (memberAccessExpr.Expression as IdentifierNameSyntax)?.Identifier.Text;

            if (targetClass != config.MapperClassName || methodName != config.ToDoMethodName)
            {
                return;
            }

            // If e.g. Trs.ToDo is found, report a warning.
            // Extract arguments and check key-text mismatch
            var arguments = invocationExpr.ArgumentList.Arguments;
            if ( arguments.Count != 1 || !(arguments[0].Expression is LiteralExpressionSyntax textLiteral) )
            {
                return;
            }

            //if (!System.Diagnostics.Debugger.IsAttached)
            //{
            //    System.Diagnostics.Debugger.Launch();
            //}

            var devText = textLiteral.Token.ValueText;


            // Check if the text is already in the "Pending" dictionary
            if (config.DevTextManager.IsPendingTranslationDevText(devText))
            {
                return;
            }


            var diagnostic = Diagnostic.Create(ToDoRule, memberAccessExpr.GetLocation(), devText);
            context.ReportDiagnostic(diagnostic);
        }
        catch (Exception ex)
        {
            // Protokollieren der Ausnahme
            File.AppendAllText(@"C:\Temp\AnalyzerExceptions.txt", ex.ToString());
        }
    }
}
