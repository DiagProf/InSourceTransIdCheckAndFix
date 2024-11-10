

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using InSourceTransIdCheckAndFix.Analyzer;

namespace InSourceTransIdCheckAndFix.CodeFix
{

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InSourceTransIdCheckAndFixToDoCodeFixProvider)), Shared]
    public class InSourceTransIdCheckAndFixToDoCodeFixProvider : BaseInSourceTransIdCheckAndFixCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(InSourceTransIdCheckAndFixToDoAnalyzer.DiagnosticId);

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


            if (!(invocationExpr.ArgumentList.Arguments.FirstOrDefault()?.Expression is LiteralExpressionSyntax argument))
            {
                return;
            }

            if (ConfigurationLoader.TryLoad(context.Document.Project.AnalyzerOptions, out var config))
            {
                var text = argument.Token.ValueText;
                var key = config!.DevTextManager.GetKeyForText(text);

                if (key != null)
                {
                    RegisterReplaceToDoWithOnCodeFix(context, invocationExpr, key, text, config);
                }
                else
                {
                    RegisterAddToPendingTranslationsCodeFix(context, text, config, node);
                }
            }

        }

        private void RegisterReplaceToDoWithOnCodeFix(CodeFixContext context, InvocationExpressionSyntax invocationExpr, string key, string text, DevTextToKeyMapperConfig config)
        {
            var mapperClassName = config.MapperClassName;
            var toDoMethodName = config.ToDoMethodName;
            var onMethodName = config.OnMethodName;

            // Register code action for replacing Tsc.ToDo with Tsc.On
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: string.Format(Resources.CodeFixReplaceWithOn, toDoMethodName, onMethodName, key, text),
                    createChangedDocument: c => ReplaceToDoWithOnAsync(context.Document, invocationExpr, mapperClassName, onMethodName, key, text, c),
                    equivalenceKey: nameof(InSourceTransIdCheckAndFixToDoCodeFixProvider) + "_Replace"),
                context.Diagnostics.First());
        }

        private async Task<Document> ReplaceToDoWithOnAsync(Document document, InvocationExpressionSyntax invocationExpr, string mapperClassName, string onMethodName, string key, string text, CancellationToken cancellationToken)
        {
            var onExpression = SyntaxFactory.ParseExpression($"{mapperClassName}.{onMethodName}(\"{key}\", \"{text}\")");
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if ( root == null )
            {
                return document;
            }

            var newRoot = root.ReplaceNode(invocationExpr, onExpression);
            return document.WithSyntaxRoot(newRoot);
        }

        private void RegisterAddToPendingTranslationsCodeFix(CodeFixContext context, string text, DevTextToKeyMapperConfig config, SyntaxNode node)
        {
            context.RegisterCodeFix(
                new NoPreviewCodeAction(
                    title: string.Format(Resources.CodeFixAddToPendingTranslation, text),
                    createChangedSolution: (c, isPreview) => AddToPendingTranslationsAsync(context.Document, text, config, node, c, isPreview),
                    equivalenceKey: $"{nameof(InSourceTransIdCheckAndFixToDoCodeFixProvider)}_AddPending"),
                context.Diagnostics.First());
        }

        private async Task<Solution> AddToPendingTranslationsAsync(Document document, string text, DevTextToKeyMapperConfig config, SyntaxNode node, CancellationToken cancellationToken, bool isPreview)
        {
            // now the magic
            if (!isPreview)
            {
               
                // Add the text to the list of pending translations
                config.DevTextManager.AddNewTranslationRequest(text);
                //Console.Beep();//only for Debug
               
            }

            // Nehme eine minimale, unsichtbare Änderung vor
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document.Project.Solution;
            }
            var newNode = node.WithTrailingTrivia(node.GetTrailingTrivia().Add(SyntaxFactory.Space)); //ElasticTab
            var newRoot = root.ReplaceNode(node, newNode);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument.Project.Solution;

        }

        private class NoPreviewCodeAction : CodeAction
        {
            //https://stackoverflow.com/questions/44243781/roslyn-code-action-how-to-check-if-preview-or-real-execution
            private readonly Func<CancellationToken, bool, Task<Solution>> _createChangedSolution;

            public override string Title { get; }

            public override string EquivalenceKey { get; }

            public NoPreviewCodeAction(string title, Func<CancellationToken, bool, Task<Solution>> createChangedSolution, string equivalenceKey = "")
            {
                _createChangedSolution = createChangedSolution;
                Title = title;
                EquivalenceKey = equivalenceKey;
            }

            protected override async Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync(CancellationToken cancellationToken)
            {
                const bool isPreview = true;
                // Content copied from http://source.roslyn.io/#Microsoft.CodeAnalysis.Workspaces/CodeActions/CodeAction.cs,81b0a0866b894b0e,references
                var changedSolution = await GetChangedSolutionWithPreviewAsync(cancellationToken, isPreview).ConfigureAwait(false);
                if (changedSolution == null)
                    return await Task.FromResult(Enumerable.Empty<CodeActionOperation>()); 

                return new CodeActionOperation[] { new ApplyChangesOperation(changedSolution) };
            }
            protected override async Task<Solution?> GetChangedSolutionAsync(CancellationToken cancellationToken)
            {
                const bool isPreview = false;
                return await GetChangedSolutionWithPreviewAsync(cancellationToken, isPreview).ConfigureAwait(false);
            }

            private Task<Solution> GetChangedSolutionWithPreviewAsync(CancellationToken cancellationToken, bool isPreview)
            {
                return _createChangedSolution(cancellationToken, isPreview);
            }

        }
    }
}
