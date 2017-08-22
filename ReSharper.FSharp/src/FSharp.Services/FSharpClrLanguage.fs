namespace JetBrains.ReSharper.Plugins.FSharp.Services

open JetBrains.Application
open JetBrains.ReSharper.Psi
open JetBrains.ReSharper.Plugins.FSharp.Psi
open JetBrains.ReSharper.Feature.Services.ClrLanguages

[<ShellComponent>]
type FSharpClrLanguage() =
    interface IClrLanguagesKnown with
        member x.Language = FSharpLanguage.Instance :> _
