using System.Collections.Immutable;
using System.Composition;
using InSourceTransIdCheckAndFix.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace InSourceTransIdCheckAndFix.CodeFix
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InSourceTransIdCheckAndFixOnCodeFixProvider)), Shared]
    public class InSourceTransIdCheckAndFixOnCodeFixProvider : BaseInSourceTransIdCheckAndFixCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(InSourceTransIdCheckAndFixOnAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return;
            }

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var node = root.FindNode(diagnosticSpan);

            if (!(node.FirstAncestorOrSelf<InvocationExpressionSyntax>() is InvocationExpressionSyntax invocationExpr))
            {
                return;
            }


            if (!(invocationExpr.ArgumentList.Arguments.FirstOrDefault()?.Expression is LiteralExpressionSyntax keyArgument))
            {
                return;
            }

            if (!(invocationExpr.ArgumentList.Arguments.ElementAtOrDefault(1)?.Expression is LiteralExpressionSyntax textArgument))
            {
                return;
            }


            if (ConfigurationLoader.TryLoad(context.Document.Project.AnalyzerOptions, out var config))
            {
                var mapperClassName = config!.MapperClassName;
                var onMethodName = config.OnMethodName;
                var toDoMethodName = config.ToDoMethodName;

                var devTextKeyInSrc = keyArgument.Token.ValueText;
                var devTextInSrc = textArgument.Token.ValueText;

                var devTextKeyDb = config!.DevTextManager.GetKeyForText(devTextInSrc);
                if (devTextKeyDb != null)
                {
                    // Register a code action to replace the wrong key with the correct one
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: string.Format(Resources.CodeFixExchangeOfTranslationKeys, devTextKeyInSrc, devTextKeyDb),
                            createChangedDocument: c =>
                                ReplaceKeyWithCorrectKeyAsync(context.Document, invocationExpr, mapperClassName, onMethodName, devTextKeyDb, devTextInSrc, c),
                            equivalenceKey: nameof(InSourceTransIdCheckAndFixOnCodeFixProvider) + "_Exchange"),
                        diagnostic);
                }
                else
                {
                    // No valid key exists, revert to Tsc.ToDo
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: string.Format(Resources.CodeFixRevertToToDo, onMethodName, toDoMethodName),
                    createChangedDocument: c =>
                                RevertToToDoAsync(context.Document, invocationExpr, mapperClassName, toDoMethodName, devTextInSrc, c),
                            equivalenceKey: nameof(InSourceTransIdCheckAndFixOnCodeFixProvider) + "_Revert"),
                        diagnostic);
                }
            }

        }

        private async Task<Document> ReplaceKeyWithCorrectKeyAsync(Document document, InvocationExpressionSyntax invocationExpr, string mapperClassName, string onMethodName, string correctKey, string text, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var onExpression = SyntaxFactory.ParseExpression($"{mapperClassName}.{onMethodName}(\"{correctKey}\", \"{text}\")").NormalizeWhitespace() as InvocationExpressionSyntax;
            if (root == null || onExpression == null)
            {
                return document;
            }
            var newRoot = root.ReplaceNode(invocationExpr, onExpression);
            return document.WithSyntaxRoot(newRoot);
        }

        private async Task<Document> RevertToToDoAsync(Document document, InvocationExpressionSyntax invocationExpr, string mapperClassName, string toDoMethodName, string text, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var toDoExpression = SyntaxFactory.ParseExpression($"{mapperClassName}.{toDoMethodName}(\"{text}\")").NormalizeWhitespace() as InvocationExpressionSyntax;
            if ( root == null || toDoExpression == null)
            {
                return document;
            }
            var newRoot = root.ReplaceNode(invocationExpr, toDoExpression);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
