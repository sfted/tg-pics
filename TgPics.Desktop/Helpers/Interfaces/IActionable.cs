namespace TgPics.Desktop.Helpers.Interfaces;

using System.Windows.Input;

internal interface IActionable
{
    string ActionName { get; }
    ICommand Action { get; }
}
