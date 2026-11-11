#nullable disable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SoMRandomizer.forms
{
    /// <summary>
    /// Minimal Avalonia replacements for the WinForms MessageBox and Process.Start
    /// helpers used by the main form.
    /// </summary>
    public static class DialogUtil
    {
        public static async Task ShowMessage(Window owner, string title, string message)
        {
            await ShowDialog(owner, title, message, false);
        }

        public static async Task<bool> ShowYesNo(Window owner, string title, string message)
        {
            return await ShowDialog(owner, title, message, true);
        }

        private static async Task<bool> ShowDialog(Window owner, string title, string message, bool yesNo)
        {
            Window dialog = new Window
            {
                Title = title,
                Width = 430,
                SizeToContent = SizeToContent.Height,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(16), Spacing = 14 };
            panel.Children.Add(new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap });

            StackPanel buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
            };

            if (yesNo)
            {
                Button yes = new Button { Content = "Yes" };
                yes.Click += (s, e) => dialog.Close(true);
                Button no = new Button { Content = "No" };
                no.Click += (s, e) => dialog.Close(false);
                buttons.Children.Add(yes);
                buttons.Children.Add(no);
            }
            else
            {
                Button ok = new Button { Content = "OK" };
                ok.Click += (s, e) => dialog.Close(true);
                buttons.Children.Add(ok);
            }

            panel.Children.Add(buttons);
            dialog.Content = panel;

            bool result = await dialog.ShowDialog<bool>(owner);
            return result;
        }

        /// <summary>Open a URL in the system browser (or mail client for mailto:).</summary>
        public static void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
