﻿using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace JetBrains.ReSharper.ControlFlow.PostfixCompletion
{
  // todo: maybe use NULL to indicate that expression is broken and types do not works
  // todo: calculate CanBeExpression?

  public sealed class PrefixExpressionContext
  {
    public PrefixExpressionContext(
      [NotNull] PostfixTemplateAcceptanceContext parent,
      [NotNull] ICSharpExpression expression, bool canBeStatement)
    {
      Parent = parent;
      Expression = expression;
      Type = expression.Type();
      //CanBeStatement = canBeStatement; // calculate here?
      CanBeStatement = CalculateCanBeStatement(expression);

      var referenceExpression1 = expression as IReferenceExpression;
      if (referenceExpression1 != null)
      {
        ReferencedElement = referenceExpression1.Reference.Resolve().DeclaredElement;
      }
      else
      {
        var typeExpression = expression as IPredefinedTypeExpression;
        if (typeExpression != null)
        {
          var typeName = typeExpression.PredefinedTypeName;
          if (typeName != null)
            ReferencedElement = typeName.Reference.Resolve().DeclaredElement;
        }
      }
    }

    private static bool CalculateCanBeStatement([NotNull] ICSharpExpression expression)
    {
      if (ExpressionStatementNavigator.GetByExpression(expression) != null)
        return true;

      // handle broken trees like: "lines.     \r\n   NextLineStatemement();"
      var containingStatement = expression.GetContainingNode<ICSharpStatement>();
      if (containingStatement != null)
      {
        var a = expression.GetTreeStartOffset();
        var b = containingStatement.GetTreeStartOffset();
        return (a == b);
      }

      return false;
    }

    [NotNull] public PostfixTemplateAcceptanceContext Parent { get; private set; }

    // "lines.Any()" : Boolean
    [NotNull] public ICSharpExpression Expression { get; private set; }
    // todo: review checks for Unknown
    [NotNull] public IType Type { get; private set; }
    [CanBeNull] public IDeclaredElement ReferencedElement { get; private set; }

    public bool CanBeStatement { get; private set; }

    // "lines.Any().if"
    [NotNull] public IReferenceExpression Reference
    {
      get { return Parent.PostfixReferenceExpression; }
    }

    // ranges
    public DocumentRange ExpressionRange
    {
      get { return Parent.ToDocumentRange(Expression); }
    }

    public DocumentRange ReplaceRange
    {
      get { return ExpressionRange.JoinRight(Parent.MostInnerReplaceRange); }
    }
  }
}