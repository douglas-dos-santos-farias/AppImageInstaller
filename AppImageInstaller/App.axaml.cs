using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;

namespace AppImageInstaller;

public partial class App : Application
{
    private static readonly Dictionary<string, Color> LightPalette = new()
    {
        ["AppBackgroundBrush"] = Color.Parse("#F4F6FB"),
        ["AppForegroundBrush"] = Color.Parse("#18181B"),
        ["MutedForegroundBrush"] = Color.Parse("#5B6474"),
        ["CardBackgroundBrush"] = Color.Parse("#FDFDFF"),
        ["AccentCardBackgroundBrush"] = Color.Parse("#EAF3FF"),
        ["CardBorderBrush"] = Color.Parse("#D9E1EE"),
        ["InputBackgroundBrush"] = Color.Parse("#F7F9FD"),
        ["InputBackgroundHoverBrush"] = Color.Parse("#ECF2FB"),
        ["ListBackgroundBrush"] = Color.Parse("#F7F9FD"),
        ["ListItemBackgroundBrush"] = Color.Parse("#F7F9FD"),
        ["ListItemHoverBrush"] = Color.Parse("#E8F0FC"),
        ["ListItemSelectedBrush"] = Color.Parse("#DCEAFE"),
        ["SecondaryButtonBrush"] = Color.Parse("#E9EEF7"),
        ["SecondaryButtonBrushHover"] = Color.Parse("#DDE7F5"),
        ["SecondaryButtonBrushPressed"] = Color.Parse("#D2DFF1"),
        ["SecondaryButtonBorderBrush"] = Color.Parse("#C4D0E3"),
        ["SecondaryButtonBorderBrushHover"] = Color.Parse("#AABBD6"),
        ["AccentBrush"] = Color.Parse("#0F6CBD"),
        ["AccentBrushHover"] = Color.Parse("#0D5FA8"),
        ["AccentBrushPressed"] = Color.Parse("#0A4F8C"),
        ["SuccessBrush"] = Color.Parse("#176448"),
        ["ErrorBrush"] = Color.Parse("#B42318")
    };

    private static readonly Dictionary<string, Color> DarkPalette = new()
    {
        ["AppBackgroundBrush"] = Color.Parse("#09111D"),
        ["AppForegroundBrush"] = Color.Parse("#F3F7FD"),
        ["MutedForegroundBrush"] = Color.Parse("#9AA9C0"),
        ["CardBackgroundBrush"] = Color.Parse("#111C2E"),
        ["AccentCardBackgroundBrush"] = Color.Parse("#132742"),
        ["CardBorderBrush"] = Color.Parse("#223955"),
        ["InputBackgroundBrush"] = Color.Parse("#162338"),
        ["InputBackgroundHoverBrush"] = Color.Parse("#1E2F49"),
        ["ListBackgroundBrush"] = Color.Parse("#162338"),
        ["ListItemBackgroundBrush"] = Color.Parse("#162338"),
        ["ListItemHoverBrush"] = Color.Parse("#21324B"),
        ["ListItemSelectedBrush"] = Color.Parse("#2E4A6F"),
        ["SecondaryButtonBrush"] = Color.Parse("#1A2B42"),
        ["SecondaryButtonBrushHover"] = Color.Parse("#243954"),
        ["SecondaryButtonBrushPressed"] = Color.Parse("#2C4362"),
        ["SecondaryButtonBorderBrush"] = Color.Parse("#335171"),
        ["SecondaryButtonBorderBrushHover"] = Color.Parse("#4B6D92"),
        ["AccentBrush"] = Color.Parse("#63AEFF"),
        ["AccentBrushHover"] = Color.Parse("#7ABBFF"),
        ["AccentBrushPressed"] = Color.Parse("#4C9DEF"),
        ["SuccessBrush"] = Color.Parse("#53D4A7"),
        ["ErrorBrush"] = Color.Parse("#FF8D8D")
    };

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void SetTheme(string themeKey)
    {
        var variant = themeKey switch
        {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Light
        };

        RequestedThemeVariant = variant;
        ApplyPalette(variant == ThemeVariant.Dark ? DarkPalette : LightPalette);
    }

    private void ApplyPalette(IReadOnlyDictionary<string, Color> palette)
    {
        foreach (var entry in palette)
        {
            if (Resources.TryGetResource(entry.Key, ThemeVariant.Default, out var resource) &&
                resource is SolidColorBrush brush)
            {
                brush.Color = entry.Value;
            }
        }
    }
}
