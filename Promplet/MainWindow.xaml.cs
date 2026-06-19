using System.Windows;
using Promplet.Win32;

namespace Promplet;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        NonActivatingWindowBehavior.Attach(this);
    }
}
