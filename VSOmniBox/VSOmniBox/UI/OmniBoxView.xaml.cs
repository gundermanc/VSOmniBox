namespace VSOmniBox.UI
{
    using System.ComponentModel;
    using System.Windows;

    /// <summary>
    /// Interaction logic for OmniBoxView.xaml
    /// </summary>
    public partial class OmniBoxView : Window
    {
        public OmniBoxView()
        {
            this.InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        #region HACK: Update model's CaretIndex

        // HACK: Try and do this through some form of binding instead. CaretIndex
        // is sadly not a dependency property so it cannot be directly bound. Initial
        // attempts at creating an attached property + behavior were unsuccessful.

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is OmniBoxViewModel oldModel)
            {
                this.SearchTextBox.SelectionChanged -= OnSelectionChanged;
                oldModel.PropertyChanged -= OnModelChanged;
            }

            if (e.NewValue is OmniBoxViewModel newModel)
            {
                this.SearchTextBox.SelectionChanged += OnSelectionChanged;
                newModel.PropertyChanged += OnModelChanged;
            }
        }

        private void OnModelChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OmniBoxViewModel.CaretIndex))
            {
                this.SearchTextBox.CaretIndex = ((OmniBoxViewModel)this.DataContext).CaretIndex;
            }
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is OmniBoxViewModel model)
            {
                model.CaretIndex = this.SearchTextBox.CaretIndex;
            }
        }

        #endregion
    }
}
