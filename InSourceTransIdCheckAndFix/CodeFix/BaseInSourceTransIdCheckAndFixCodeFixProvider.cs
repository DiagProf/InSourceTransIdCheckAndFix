using Microsoft.CodeAnalysis.CodeFixes;

namespace InSourceTransIdCheckAndFix.CodeFix;
public abstract class BaseInSourceTransIdCheckAndFixCodeFixProvider : CodeFixProvider
{
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }
}