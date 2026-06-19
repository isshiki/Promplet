using Promplet.Services;

var tests = new (string Name, Action Body)[]
{
    ("default catalog contains four MVP prompts in order", PromptCatalogContainsExpectedButtons)
};

var failed = 0;

foreach (var (name, body) in tests)
{
    try
    {
        body();
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.Error.WriteLine($"FAIL {name}");
        Console.Error.WriteLine(ex.Message);
    }
}

return failed;

static void PromptCatalogContainsExpectedButtons()
{
    var buttons = PromptCatalog.GetDefaultButtons();

    AssertEqual(4, buttons.Count, "button count");
    AssertEqual("summarize", buttons[0].Id, "first id");
    AssertEqual("要約", buttons[0].Label, "first label");
    AssertTrue(buttons[0].Text.EndsWith("\r\n\r\n", StringComparison.Ordinal), "first prompt keeps trailing blank line");
    AssertEqual("rewrite", buttons[1].Id, "second id");
    AssertEqual("review", buttons[2].Id, "third id");
    AssertEqual("explain", buttons[3].Id, "fourth id");
}

static void AssertEqual<T>(T expected, T actual, string context)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{context}: expected '{expected}', got '{actual}'.");
    }
}

static void AssertTrue(bool condition, string context)
{
    if (!condition)
    {
        throw new InvalidOperationException(context);
    }
}
