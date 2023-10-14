using MaloyClient.ViewModels;
using ReactiveUI;

namespace MaloyClient.Models;

public sealed class Command : ViewModelBase
{
    private string _commandName;
    private string _content;

    public Command(string commandName, string content, bool canEdit = true)
    {
        CanEdit = canEdit;
        _commandName = commandName;
        _content = content;
    }

    public string CommandName
    {
        get => _commandName;
        set => this.RaiseAndSetIfChanged(ref _commandName, value);
    }

    public string Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    public bool CanEdit { get; }
}