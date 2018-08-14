namespace VSOmniBox.DefaultProviders.LightBulb
{
    using System;
    using System.Threading;
    using Microsoft.VisualStudio.Language.Intellisense;
    using VSOmniBox.API.Data;

    internal sealed class LightBulbItem : OmniBoxItem
    {
        private readonly ISuggestedAction lightBulbAction;

        public LightBulbItem(ISuggestedAction lightBulbAction)
        {
            this.lightBulbAction = lightBulbAction ?? throw new ArgumentNullException(nameof(lightBulbAction));
        }

        public override string Title => this.lightBulbAction.DisplayText;

        public override string Description => "Performs a lightbulb action on the current document...";

        public override void Invoke() => this.lightBulbAction.Invoke(CancellationToken.None);

        // Give highest priority since these items are most relevant to the user's context.
        public override int Priority { get; } = 2;
    }
}
