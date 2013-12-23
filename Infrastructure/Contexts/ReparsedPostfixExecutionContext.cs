using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Tree;

namespace JetBrains.ReSharper.PostfixTemplates
{
  internal sealed class ReparsedPostfixExecutionContext : PostfixExecutionContext
  {
    [NotNull] private readonly ReparsedCodeCompletionContext myReparsedContext;

    private ReparsedPostfixExecutionContext(bool isForceMode,
      [NotNull] IPsiModule psiModule, [NotNull] ILookupItemsOwner lookupItemsOwner,
      [NotNull] ReparsedCodeCompletionContext reparsedContext, [NotNull] string reparseString)
      : base(isForceMode, psiModule, lookupItemsOwner, reparseString)
    {
      myReparsedContext = reparsedContext;
    }

    public override DocumentRange GetDocumentRange(ITreeNode treeNode)
    {
      return myReparsedContext.ToDocumentRange(treeNode);
    }

    public override PostfixExecutionContext WithForceMode(bool enabled)
    {
      if (enabled == IsForceMode) return this;

      return new ReparsedPostfixExecutionContext(
        enabled, PsiModule, LookupItemsOwner, myReparsedContext, ReparseString);
    }

    [NotNull]
    public static PostfixExecutionContext Create(
      [NotNull] CSharpCodeCompletionContext completionContext,
      [NotNull] ReparsedCodeCompletionContext reparsedContext,
      [NotNull] string reparseString)
    {
      var completionType = completionContext.BasicContext.CodeCompletionType;
      var lookupItemsOwner = completionContext.BasicContext.LookupItemsOwner;
      var isForceMode = (completionType == CodeCompletionType.BasicCompletion);

      return new ReparsedPostfixExecutionContext(
        isForceMode, completionContext.PsiModule,
        lookupItemsOwner, reparsedContext, reparseString);
    }
  }
}