using System.Windows.Input;

namespace VSOmniBox.UI
{
    internal interface IInvokable
    {
        ICommand InvokeCommand { get; }
    }
}
