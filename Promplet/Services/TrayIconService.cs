using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace Promplet.Services;

public sealed class TrayIconService : IDisposable
{
    private readonly Forms.ContextMenuStrip _contextMenu;
    private readonly Drawing.Icon _icon;
    private readonly Forms.NotifyIcon _notifyIcon;
    private bool _disposed;

    public TrayIconService(
        string iconPath,
        Action showPalette,
        Action hidePalette,
        Action openSettings,
        Action reloadPrompts,
        Action exitApplication)
    {
        _icon = new Drawing.Icon(iconPath);
        _contextMenu = new Forms.ContextMenuStrip();
        _contextMenu.Items.Add("Show Promplet", null, (_, _) => showPalette());
        _contextMenu.Items.Add("Hide Promplet", null, (_, _) => hidePalette());
        _contextMenu.Items.Add("Settings...", null, (_, _) => openSettings());
        _contextMenu.Items.Add("Reload prompts", null, (_, _) => reloadPrompts());
        _contextMenu.Items.Add(new Forms.ToolStripSeparator());
        _contextMenu.Items.Add("Exit", null, (_, _) => exitApplication());

        _notifyIcon = new Forms.NotifyIcon
        {
            ContextMenuStrip = _contextMenu,
            Icon = _icon,
            Text = "Promplet",
            Visible = true
        };
        _notifyIcon.DoubleClick += (_, _) => showPalette();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _contextMenu.Dispose();
        _icon.Dispose();
    }
}
