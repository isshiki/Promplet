using System.Runtime.InteropServices;
using System.Xml.Linq;
using Promplet.Services;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        var tests = new (string Name, Action Body)[]
        {
            ("default catalog contains four MVP prompts in order", PromptCatalogContainsExpectedButtons),
            ("SendInput uses the native Windows INPUT struct size", SendInputStructUsesNativeSize),
            ("main window uses the approved vertical palette contract", MainWindowUsesApprovedVerticalPaletteContract)
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
                Console.Error.WriteLine(ex);
            }
        }

        return failed;
    }

    private static void PromptCatalogContainsExpectedButtons()
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

    private static void SendInputStructUsesNativeSize()
    {
        var inputType = Type.GetType("Promplet.Win32.NativeMethods+INPUT, Promplet", throwOnError: true)!;
        var expectedSize = IntPtr.Size == 8 ? 40 : 28;
        var actualSize = Marshal.SizeOf(inputType);

        AssertEqual(expectedSize, actualSize, "Win32 INPUT struct size");
    }

    private static void MainWindowUsesApprovedVerticalPaletteContract()
    {
        var xaml = XDocument.Load(FindRepositoryFile("Promplet", "MainWindow.xaml"));
        var root = xaml.Root ?? throw new InvalidOperationException("MainWindow.xaml has no root element.");
        XNamespace wpf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";

        AssertEqual("CanResize", AttributeValue(root, "ResizeMode"), "palette resize mode");
        AssertEqual("Height", AttributeValue(root, "SizeToContent"), "palette height sizing");
        AssertTrue(double.Parse(AttributeValue(root, "Width")) >= 600, "palette starts wider than the old compact toolbar");

        var promptButtons = xaml.Descendants(wpf + "ItemsControl")
            .SingleOrDefault(element => AttributeValue(element, x + "Name") == "PromptButtons")
            ?? throw new InvalidOperationException("Could not find PromptButtons ItemsControl.");

        var panel = promptButtons.Descendants(wpf + "ItemsPanelTemplate")
            .Descendants(wpf + "StackPanel")
            .SingleOrDefault()
            ?? throw new InvalidOperationException("PromptButtons does not use a StackPanel items panel.");

        AssertEqual("Vertical", AttributeValue(panel, "Orientation"), "prompt button orientation");
        AssertTrue(xaml.Descendants(wpf + "Button").Any(element => AttributeValue(element, x + "Name") == "CloseButton"), "palette has close button");
    }

    private static string FindRepositoryFile(params string[] segments)
    {
        var directory = new DirectoryInfo(Environment.CurrentDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(segments).ToArray());

            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {Path.Combine(segments)}");
    }

    private static string AttributeValue(XElement element, string name)
    {
        return element.Attribute(name)?.Value
            ?? throw new InvalidOperationException($"Missing attribute '{name}' on '{element.Name.LocalName}'.");
    }

    private static string AttributeValue(XElement element, XName name)
    {
        return element.Attribute(name)?.Value
            ?? throw new InvalidOperationException($"Missing attribute '{name.LocalName}' on '{element.Name.LocalName}'.");
    }

    private static void AssertEqual<T>(T expected, T actual, string context)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{context}: expected '{expected}', got '{actual}'.");
        }
    }

    private static void AssertTrue(bool condition, string context)
    {
        if (!condition)
        {
            throw new InvalidOperationException(context);
        }
    }
}
