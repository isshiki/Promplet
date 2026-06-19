using System.IO;
using System.Text.Json;
using Promplet.Models;

namespace Promplet.Services;

public sealed class PromptStore
{
    public const string FileName = "prompts.json";
    public const int MaximumGroups = 10;
    public const int MaximumButtonsPerGroup = 10;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public PromptStore()
        : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Promplet"))
    {
    }

    public PromptStore(string directoryPath)
    {
        DirectoryPath = directoryPath;
        JsonPath = Path.Combine(directoryPath, FileName);
    }

    public string DirectoryPath { get; }

    public string JsonPath { get; }

    public PromptDocument LoadOrCreate()
    {
        Directory.CreateDirectory(DirectoryPath);

        if (!File.Exists(JsonPath))
        {
            var defaultDocument = PromptCatalog.CreateDefaultDocument();
            Save(defaultDocument);
            return defaultDocument;
        }

        try
        {
            var json = File.ReadAllText(JsonPath);
            var document = JsonSerializer.Deserialize<PromptDocument>(json, JsonOptions)
                ?? throw new JsonException("Prompt JSON did not contain a document.");

            return Normalize(document);
        }
        catch (Exception ex) when (ex is JsonException or InvalidDataException)
        {
            BackupInvalidJson();
            var defaultDocument = PromptCatalog.CreateDefaultDocument();
            Save(defaultDocument);
            return defaultDocument;
        }
    }

    public void Save(PromptDocument document)
    {
        Directory.CreateDirectory(DirectoryPath);
        var normalized = Normalize(document);
        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        File.WriteAllText(JsonPath, json);
    }

    private static PromptDocument Normalize(PromptDocument document)
    {
        if (document.App is null || document.Window is null || document.Groups is null)
        {
            throw new InvalidDataException("Prompt JSON is missing required sections.");
        }

        var groups = document.Groups
            .Where(group => group is not null
                && group.Buttons is not null
                && !string.IsNullOrWhiteSpace(group.Id)
                && !string.IsNullOrWhiteSpace(group.Name))
            .Take(MaximumGroups)
            .Select(group => new PromptGroup
            {
                Id = group.Id.Trim(),
                Name = group.Name.Trim(),
                Buttons = group.Buttons
                    .Where(button => button is not null
                        && !string.IsNullOrWhiteSpace(button.Id)
                        && !string.IsNullOrWhiteSpace(button.Label))
                    .Take(MaximumButtonsPerGroup)
                    .Select(button => button with
                    {
                        Id = button.Id.Trim(),
                        Label = button.Label.Trim(),
                        Text = button.Text ?? string.Empty
                    })
                    .ToList()
            })
            .Where(group => group.Buttons.Count > 0)
            .ToList();

        if (groups.Count == 0)
        {
            return PromptCatalog.CreateDefaultDocument();
        }

        var selectedGroupId = document.App.SelectedGroupId;
        if (!groups.Any(group => group.Id == selectedGroupId))
        {
            selectedGroupId = groups[0].Id;
        }

        return new PromptDocument
        {
            App = new PromptAppSettings
            {
                RestoreClipboard = document.App.RestoreClipboard,
                SelectedGroupId = selectedGroupId
            },
            Window = new PromptWindowState
            {
                Left = document.Window.Left,
                Top = document.Window.Top,
                Width = NormalizeWidth(document.Window.Width)
            },
            Groups = groups
        };
    }

    private static double NormalizeWidth(double width)
    {
        return double.IsFinite(width)
            ? Math.Clamp(width, PaletteWindowState.MinimumWidth, PaletteWindowState.MaximumWidth)
            : 620;
    }

    private void BackupInvalidJson()
    {
        if (!File.Exists(JsonPath))
        {
            return;
        }

        var timestamp = DateTimeOffset.Now.ToString("yyyyMMddHHmmss");
        var backupPath = Path.Combine(DirectoryPath, $"{FileName}.{timestamp}.bak");
        File.Copy(JsonPath, backupPath, overwrite: true);
    }
}
