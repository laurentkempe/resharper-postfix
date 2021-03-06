﻿using JetBrains.Annotations;
using JetBrains.ReSharper.PostfixTemplates.LookupItems;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;

namespace JetBrains.ReSharper.PostfixTemplates.Templates
{
  [PostfixTemplate(
    templateName: "switch",
    description: "Produces switch over integral/string type",
    example: "switch (expr)")]
  public class SwitchStatementTemplate : IPostfixTemplate
  {
    public IPostfixLookupItem CreateItem(PostfixTemplateContext context)
    {
      var isAutoCompletion = context.ExecutionContext.IsAutoCompletion;
      foreach (var expressionContext in context.Expressions)
      {
        if (!expressionContext.CanBeStatement) continue;

        if (isAutoCompletion)
        {
          // disable for constant expressions
          if (!expressionContext.Expression.ConstantValue.IsBadValue()) continue;

          var expressionType = expressionContext.Type;
          if (!expressionType.IsResolved) continue;
          if (expressionType.IsNullable())
          {
            expressionType = expressionType.GetNullableUnderlyingType();
            if (expressionType == null || !expressionType.IsResolved) continue;
          }

          if (!expressionType.IsPredefinedIntegral() &&
              !expressionType.IsEnumType()) continue;
        }

        return new SwitchItem(expressionContext);
      }

      return null;
    }

    private sealed class SwitchItem : StatementPostfixLookupItem<ISwitchStatement>
    {
      public SwitchItem([NotNull] PrefixExpressionContext context) : base("switch", context) { }

      // switch statement can't be without braces
      protected override ISwitchStatement CreateStatement(CSharpElementFactory factory,
                                                          ICSharpExpression expression)
      {
        var template = "switch($0)" + RequiredBracesTemplate;
        return (ISwitchStatement) factory.CreateStatement(template, expression);
      }
    }
  }
}