package com.jetbrains.rider.plugins.fsharp.test.cases.markup

import com.jetbrains.rdclient.testFramework.waitForDaemon
import com.jetbrains.rider.plugins.fsharp.test.withCultureInfo
import com.jetbrains.rider.test.annotations.TestEnvironment
import com.jetbrains.rider.test.asserts.shouldContains
import com.jetbrains.rider.test.base.DocumentationTestBase
import com.jetbrains.rider.test.env.enums.SdkVersion
import org.testng.annotations.Test

@TestEnvironment(solution = "CoreConsoleApp", sdkVersion = SdkVersion.DOT_NET_6)
class FSharpHoverDocTest : DocumentationTestBase() {
  @Test
  fun `test hover docs for EntryPoint`() = doTest("Program.fs", "Program.fs")

  @Test
  fun `test hover docs for a function definition`() = doTest("Program.fs", "Program.fs")

  @Test
  fun `test hover docs for a parameter`() = doTest("Program.fs", "Program.fs")

  @Test
  fun `test hover docs for a submodule`() = doTest("Program.fs", "Program.fs")

  @Test
  fun `test xml doc with symbol reference`() = doTest("Program.fs", "Program.fs")

  @Test
  fun `test empty xml doc`() = doTest("Program.fs", "Program.fs")

  @Test(enabled = false)
  @TestEnvironment(solution = "ConsoleAppTwoTargetFrameworks")
  fun `test multiple frameworks`() = doTest("Program.fs", "Program.fs")

  @Test
  @TestEnvironment(
    solution = "SwaggerProviderCSharp",
    sdkVersion = SdkVersion.DOT_NET_6
  )
  fun `provided method in csharp`() = doTest("CSharpLibrary.cs", "CSharpLibrary.cs")

  @Test
  @TestEnvironment(
    solution = "SwaggerProviderCSharp",
    sdkVersion = SdkVersion.DOT_NET_6
  )
  fun `provided abbreviation in csharp`() = doTestWithTypeProviders("OpenAPI Provider for")

  @Test
  fun `test xml doc parsing error`() {
    withCultureInfo(project, "en-US") {
      doTest("Program.fs", "Program.fs")
    }
  }

  @Suppress("SameParameterValue")
  private fun doTestWithTypeProviders(summary: String) {
    doTestWithMarkupModel("CSharpLibrary.cs", "CSharpLibrary.cs") {
      waitForDaemon()
      generateBackendHoverDoc().shouldContains(summary)
    }
  }
}
