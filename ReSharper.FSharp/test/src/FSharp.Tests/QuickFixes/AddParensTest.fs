namespace JetBrains.ReSharper.Plugins.FSharp.Tests.Features

open JetBrains.ReSharper.FeaturesTestFramework.Intentions
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.QuickFixes
open JetBrains.ReSharper.Plugins.FSharp.Tests.Common
open NUnit.Framework

[<FSharpTest>]
type AddParensTest() =
    inherit QuickFixTestBase<AddParensFix>()

    override x.RelativeTestDataPath = "features/quickFixes/addParens"

    [<Test>] member x.``Single line``() = x.DoNamedTest()
    [<Test>] member x.``Multi line``() = x.DoNamedTest()
    [<Test>] member x.``Successive qualifiers``() = x.DoNamedTest()
