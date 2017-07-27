namespace VSOmniBox.Service
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using VSOmniBox.API;
    using VSOmniBox.UI;

    internal sealed class OmniBoxBroker : IOmniBoxBroker
    {
        private OmniBoxView view;

        public bool IsVisible
        {
            get
            {
                return view?.IsVisible ?? false;
            }
            set
            {
                if (this.IsVisible != value)
                {
                    if (value)
                    {
                        this.view = new OmniBoxView()
                        {
                            Owner = Application.Current.MainWindow
                        };
                        this.view.Closed += this.OnViewClosed;
                        this.view.Deactivated += this.OnDeactivated;
                        this.view.KeyUp += OnKeyUp;
                        this.view.Visibility = Visibility.Visible;

                        this.view.Activate();
                    }
                    else
                    {
                        this.view.Close();
                        this.view.Closed -= OnViewClosed;
                        this.view.Deactivated -= this.OnDeactivated;
                        this.view.KeyUp -= OnKeyUp;
                        this.view = null;
                    }
                }
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.IsVisible = false;
                e.Handled = true;
            }
        }

        private void OnDeactivated(object sender, EventArgs e) => this.IsVisible = false;

        private void OnViewClosed(object sender, EventArgs e) => this.IsVisible = false;
    }
}
