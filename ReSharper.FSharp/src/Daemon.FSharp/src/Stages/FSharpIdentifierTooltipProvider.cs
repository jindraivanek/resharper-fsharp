﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.DataFlow;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Descriptions;
using JetBrains.ReSharper.Plugins.FSharp.Psi;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Tree;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Util;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.UI.RichText;
using JetBrains.Util;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Compiler.SourceCodeServices;
using Microsoft.FSharp.Core;

namespace JetBrains.ReSharper.Plugins.FSharp.Daemon.Cs.Stages
{
  [SolutionComponent]
  public class FSharpIdentifierTooltipProvider : IdentifierTooltipProvider<FSharpLanguage>
  {
    private readonly ISolution mySolution;
    private readonly ILogger myLogger;

    public FSharpIdentifierTooltipProvider(Lifetime lifetime, ISolution solution,
      IDeclaredElementDescriptionPresenter presenter, ILogger logger) : base(lifetime, solution, presenter)
    {
      mySolution = solution;
      myLogger = logger;
    }

    [NotNull]
    public override string GetTooltip(IHighlighter highlighter)
    {
      if (!ShouldShowTooltip(highlighter)) return string.Empty;
      var psiServices = mySolution.GetPsiServices();
      if (!psiServices.Files.AllDocumentsAreCommitted || psiServices.Caches.HasDirtyFiles) return string.Empty;

      var document = highlighter.Document;
      var sourceFile = document.GetPsiSourceFile(mySolution);
      if (sourceFile == null || !sourceFile.IsValid()) return string.Empty;

      var documentRange = new DocumentRange(document, highlighter.Range);
      var psiFile = GetPsiFile(sourceFile, documentRange) as IFSharpFile;
      var checkResults = psiFile?.GetParseAndCheckResults()?.Value.CheckResults;
      var token = psiFile?.FindTokenAt(documentRange.StartOffset) as FSharpIdentifierToken;
      if (checkResults == null || token == null) return string.Empty;

      var coords = document.GetCoordsByOffset(token.GetTreeEndOffset().Offset);
      var names = FSharpImplUtil.GetQualifiersAndName(token);
      var lineText = sourceFile.Document.GetLineText(coords.Line);

      // todo: provide tooltip for #r strings in fsx, should pass String tag
      var getTooltipAsync = checkResults.GetToolTipText((int) coords.Line + 1,
        (int) coords.Column, lineText, ListModule.OfArray(names), FSharpTokenTag.Identifier, FSharpOption<string>.None);
      var tooltips = FSharpAsyncUtil.RunSynchronouslySafe(getTooltipAsync, myLogger, "Getting F# tooltip", 2000)?.Item;

      if (tooltips == null)
        return string.Empty;

      var tooltipsTexts = new List<string>();
      foreach (var tooltip in tooltips)
      {
        var overloads = tooltip as FSharpToolTipElement<string>.Group;
        if (overloads != null)
          tooltipsTexts.AddRange(overloads.Item.Select(overload =>
            GetTooltipText(overload.MainDescription, overload.XmlDoc))); //, overload.Item2)));
      }
      return tooltipsTexts.Join("_RIDER_HORIZONTAL_LINE_TOOLTIP_SEPARATOR_");
    }

    [CanBeNull]
    public static string GetXmlDocText(FSharpXmlDoc xmlDoc)
    {
      if (xmlDoc.IsNone) return null;
      if (xmlDoc.IsText) return ((FSharpXmlDoc.Text) xmlDoc).Item;
      if (xmlDoc.IsXmlDocFileSignature)
      {
        var sig = xmlDoc as FSharpXmlDoc.XmlDocFileSignature;
        if (sig == null) return null;
        var s1 = sig.Item1;
        var s2 = sig.Item2;
        // todo: get doc from xml
      }
      return null;
    }

    [NotNull]
    public static string GetTooltipText(string text, FSharpXmlDoc xmlDoc)
    {
      var xmlDocText = GetXmlDocText(xmlDoc);
      return xmlDocText != null ? text + xmlDocText : text;
    }

    public override RichTextBlock GetRichTooltip(IHighlighter highlighter)
    {
      return new RichTextBlock(GetTooltip(highlighter));
    }
  }
}