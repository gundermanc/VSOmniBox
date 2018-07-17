namespace VSOmniBox.UI
{
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for OmniBoxView.xaml
    /// </summary>
    public partial class OmniBoxView : Window
    {
        public OmniBoxView()
        {
            this.InitializeComponent();
            this.ResultsBox.MouseDoubleClick += OnMouseDoubleClick;
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is IInvokable invokable &&
                invokable.InvokeCommand.CanExecute(parameter: null))
            {
                invokable.InvokeCommand.Execute(parameter: null);
            }
        }
    }
}
