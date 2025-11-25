using Gita.Practice.App.Models;

namespace Gita.Practice.App.ViewModels;

public static class WaitModeValues
{
    public static Array All => Enum.GetValues(typeof(WaitModeOption));
}
