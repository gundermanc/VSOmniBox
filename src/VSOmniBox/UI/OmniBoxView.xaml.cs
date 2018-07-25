namespace VSOmniBox.UI
{
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using VSOmniBox.API.Data;

    /// <summary>
    /// Interaction logic for OmniBoxView.xaml
    /// </summary>
    public partial class OmniBoxView : Window
    {
        public OmniBoxView()
        {
            this.InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            var docWell = FindVisualChildren(Application.Current.MainWindow, "Microsoft.VisualStudio.PlatformUI.Shell.Controls.DocumentGroupContainerControl");
            var topLeftAnchor = docWell.PointToScreen(new Point(0, 0));
            this.Top = topLeftAnchor.Y;
            this.Left = (topLeftAnchor.X + docWell.ActualWidth) / 2 - this.Width/2;
        }

        public static FrameworkElement FindVisualChildren(DependencyObject depObj, string typeName) 
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null)
                    {
                        var type = child.GetType();
                        if (type.FullName.Equals(typeName, System.StringComparison.OrdinalIgnoreCase))
                        {
                            return child as FrameworkElement;
                        }
                        var containsSpecifiedChild = FindVisualChildren(child, typeName);
                        if (containsSpecifiedChild != null)
                            return containsSpecifiedChild as FrameworkElement;

                    }
                }
            }
            return null;
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is IInvokable invokable &&
                invokable.InvokeCommand.CanExecute(parameter: null))
            {
                invokable.InvokeCommand.Execute(parameter: null);
            }
        }

        private void SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.ResultsBox.SelectedItems.Count == 1)
            {
                this.ResultsBox.ScrollIntoView(this.ResultsBox.SelectedItem);
            }
        }

        private void OnTabChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.DataContext is IPivotable pivotable)
            {
                switch (this.TabControl.SelectedIndex)
                {
                    case 0:
                        pivotable.Pivot = OmniBoxPivot.Code | OmniBoxPivot.IDE | OmniBoxPivot.Help;
                        break;
                    case 1:
                        pivotable.Pivot = OmniBoxPivot.Code;
                        break;
                    case 2:
                        pivotable.Pivot = OmniBoxPivot.IDE;
                        break;
                    case 3:
                        pivotable.Pivot = OmniBoxPivot.Help;
                        break;
                    default:
                        Debug.Fail("No handling for this tab");
                        break;
                }
            }
        }
    }
}
