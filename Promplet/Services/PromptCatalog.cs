using Promplet.Models;

namespace Promplet.Services;

public static class PromptCatalog
{
    public static PromptDocument CreateDefaultDocument()
    {
        return new PromptDocument
        {
            App = new PromptAppSettings
            {
                RestoreClipboard = true,
                SelectedGroupId = "ai-chat"
            },
            Window = new PromptWindowState
            {
                Width = 620
            },
            Groups =
            [
                new PromptGroup
                {
                    Id = "ai-chat",
                    Name = "AI Chat",
                    Buttons = GetDefaultButtons().ToList()
                }
            ]
        };
    }

    public static IReadOnlyList<PromptButton> GetDefaultButtons()
    {
        return
        [
            new PromptButton(
                "summarize",
                "要約",
                "以下の文章を分かりやすく要約してください。\r\n\r\n"),
            new PromptButton(
                "rewrite",
                "整える",
                "以下の文章を、意味を変えずに読みやすく整えてください。\r\n\r\n"),
            new PromptButton(
                "review",
                "レビュー",
                "以下をレビューして、問題点と改善案を具体的に挙げてください。\r\n\r\n"),
            new PromptButton(
                "explain",
                "説明",
                "以下について、前提知識が少ない人にも分かるように説明してください。\r\n\r\n")
        ];
    }
}
