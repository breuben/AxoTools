using System;

namespace AxoCover.Models.Data
{
  public class TestResult
  {
    public TestMethod Method { get; set; }

    public TimeSpan Duration { get; set; }

    public TestState Outcome { get; set; }

    public string StdOut { get; set; }

    public string StdErr { get; set; }

    public string ErrorMessage { get; set; }

    public StackItem[] StackTrace { get; set; }
  }
}
