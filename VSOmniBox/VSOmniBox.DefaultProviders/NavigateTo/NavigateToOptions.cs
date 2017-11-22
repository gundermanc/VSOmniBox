namespace VSOmniBox.DefaultProviders.NavigateTo
{
    using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;

    internal sealed class NavigateToOptions : INavigateToOptions
    {
        public bool HideExternalItems => false;
    }
}
