using System.IO;

namespace Gita.Practice.App.ViewModels;

public class HelpViewModel : BaseViewModel
{
    private string _helpText = string.Empty;

    public HelpViewModel()
    {
        LoadHelpText();
    }

    public string HelpText
    {
        get => _helpText;
        private set => SetProperty(ref _helpText, value);
    }

    private void LoadHelpText()
    {
        try
        {
            // Try to find help file relative to the application directory
            var helpFilePath = GetHelpFilePath();
            
            if (File.Exists(helpFilePath))
            {
                HelpText = File.ReadAllText(helpFilePath);
            }
            else
            {
                // Fallback to default help text if file not found
                HelpText = "# Help is coming here\n\nHelp file not found. Please ensure the help.md file exists in the Help folder.";
            }
        }
        catch (Exception ex)
        {
            // If reading fails, show error message
            HelpText = $"# Help Error\n\nUnable to load help content: {ex.Message}";
        }
    }

    private string GetHelpFilePath()
    {
        // Get the directory where the executable is located
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        
        // Look for Help folder in the same directory as the executable
        var helpFilePath = Path.Combine(appDirectory, "Help", "help.md");
        
        // Also check if running from Visual Studio/debug, check project directory
        if (!File.Exists(helpFilePath))
        {
            var projectDirectory = Path.GetDirectoryName(typeof(HelpViewModel).Assembly.Location);
            if (projectDirectory != null)
            {
                // Try going up to the project root (if running from bin/Debug)
                var projectRoot = Directory.GetParent(projectDirectory)?.Parent?.Parent?.FullName;
                if (projectRoot != null)
                {
                    var projectHelpPath = Path.Combine(projectRoot, "Help", "help.md");
                    if (File.Exists(projectHelpPath))
                    {
                        return projectHelpPath;
                    }
                }
            }
        }
        
        return helpFilePath;
    }
}
