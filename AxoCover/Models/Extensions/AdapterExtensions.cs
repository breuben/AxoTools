﻿using AxoCover.Common.Models;
using AxoCover.Common.Settings;
using AxoCover.Models.Data;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AxoCover.Models.Extensions
{
  public static class AdapterExtensions
  {
    public static string[] GetTestAdapterAssemblyPaths(TestAdapterMode adapterMode)
    {
      switch (adapterMode)
      {
        case TestAdapterMode.Integrated:
          {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            if (dte != null)
              return new[] { Path.Combine(Path.GetDirectoryName(dte.FullName),
              @"CommonExtensions\Microsoft\TestWindow\Extensions\Microsoft.VisualStudio.TestPlatform.Extensions.VSTestIntegration.dll")};
            else
              return null;
          }
        case TestAdapterMode.Standard:
          return Directory.GetFiles(AxoCoverPackage.PackageRoot, "*.TestAdapter.dll", SearchOption.AllDirectories).ToArray();
        default:
          throw new NotImplementedException();
      }
    }

    private static readonly string[] _integratedTestPlatformAssemblies = new string[]
    {
      @"CommonExtensions\Microsoft\TestWindow\msdia140typelib_clr0200.dll",
      @"CommonExtensions\Microsoft\TestWindow\Microsoft.VisualStudio.TestPlatform.ObjectModel.dll",
      @"CommonExtensions\Microsoft\TestWindow\Microsoft.VisualStudio.QualityTools.UnitTestFramework.dll"
    };

    private static readonly string[] _standardTestPlatformAssemblies = new string[]
    {
      @"TestPlatform\Microsoft.VisualStudio.TestPlatform.ObjectModel.dll"
    };

    public static string[] GetTestPlatformAssemblyPaths(TestAdapterMode adapterMode)
    {
      string root;
      string[] testPlatformAssemblies;

      switch (adapterMode)
      {
        case TestAdapterMode.Integrated:
          {
            testPlatformAssemblies = _integratedTestPlatformAssemblies;

            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            if (dte != null)
              root = Path.GetDirectoryName(dte.FullName);
            else
              throw new InvalidOperationException();
          }
          break;
        case TestAdapterMode.Standard:
          {
            root = AxoCoverPackage.PackageRoot;
            testPlatformAssemblies = _standardTestPlatformAssemblies;
          }
          break;
        default:
          throw new NotImplementedException();
      }

      return testPlatformAssemblies
        .Select(p => Path.Combine(root, p))
        .Where(p => File.Exists(p))
        .ToArray();
    }

    public static string GetShortName(this TestMessageLevel testMessageLevel)
    {
      switch (testMessageLevel)
      {
        case TestMessageLevel.Informational:
          return "INFO";
        case TestMessageLevel.Warning:
          return "WARN";
        case TestMessageLevel.Error:
          return "FAIL";
        default:
          return "MISC";
      }
    }

    public static TestState ToTestState(this TestOutcome testOutcome)
    {
      switch (testOutcome)
      {
        case TestOutcome.Failed:
          return TestState.Failed;
        case TestOutcome.Passed:
          return TestState.Passed;
        default:
          return TestState.Inconclusive;
      }
    }

    public static Data.TestResult ToTestResult(this Common.Models.TestResult testResult, TestMethod testMethod)
    {
      return new Data.TestResult()
      {
        Method = testMethod,
        Duration = testResult.Duration,
        Outcome = testResult.Outcome.ToTestState(),
        StdOut = ConcatenateMessagesInCategory(testResult.Messages, "StdOutMsgs"),
        StdErr = ConcatenateMessagesInCategory(testResult.Messages, "StdErrMsgs"),
        ErrorMessage = GetShortErrorMessage(testResult.ErrorMessage),
        StackTrace = StackItem.FromStackTrace(testResult.ErrorStackTrace)
      };
    }

    private static string ConcatenateMessagesInCategory(IList<Common.Models.TestResultMessage> messages, string categoryName)
    {
      var sb = new StringBuilder();

      foreach (var message in messages.Where(m => m.Category == categoryName))
      {
        sb.Append(message.Text);
      }

      return sb.Length > 0 ? sb.ToString() : null;
    }

    private static readonly Regex _exceptionRegex = new Regex("^Test method [^ ]* threw exception:(?<exception>.*)$",
      RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);

    private static string GetShortErrorMessage(string errorMessage)
    {
      if (errorMessage != null)
      {
        var errorMessageMatch = _exceptionRegex.Match(errorMessage);
        return errorMessageMatch.Success ? errorMessageMatch.Groups["exception"].Value.Trim() : errorMessage;
      }
      else
      {
        return errorMessage;
      }
    }
  }
}
