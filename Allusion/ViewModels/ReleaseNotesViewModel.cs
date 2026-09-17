using Caliburn.Micro;

namespace Allusion.ViewModels;

public sealed record ReleaseNoteEntry(string Header, IReadOnlyList<string> Notes);

public class ReleaseNotesViewModel : Screen
{
    public ReleaseNotesViewModel()
    {
        DisplayName = "Release notes";
    }

    public string Version { get; } =
        System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "dev";

    public IReadOnlyList<ReleaseNoteEntry> ReleaseHistory { get; } =
    [
        new("v1.0.5", new[]
        {
            "Removed the Focus view",
        }),
        new("v1.0.4", new[]
        {
            "Fixed pasted / dropped images being imported twice",
            "Fixed update check wrongly reporting up to date when the check failed",
        }),
        new("Current – unreleased", new[]
        {
            "Arrange respects the margin you type and lays images out in reading order",
            "Arrange dialog: column count (Auto-6), margin slider, settings remembered",
            "Undo last arrange from the canvas menu",
            "Right-click menu now opens anywhere on the canvas, at any zoom",
            "Always-on-top toggle now sticks, including during drag and drop",
            "Help window with topic menu and release notes",
        }),
        new("Recently shipped", new[]
        {
            "Rubber-band box select, bring-to-front / send-to-back ordering and align tools",
            "Canvas snaps to a 25 px grid; Ctrl + mouse wheel scales the selection",
        }),
    ];
}
