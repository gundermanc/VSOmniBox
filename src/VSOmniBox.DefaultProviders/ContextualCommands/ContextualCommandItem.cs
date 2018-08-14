namespace VSOmniBox.DefaultProviders.ContextualCommands
{
    using EnvDTE;
    using Microsoft.VisualStudio.Imaging.Interop;
    using System;
    using VSOmniBox.API.Data;

    internal sealed class ContextualCommandItem : OmniBoxItem
    {
        private readonly string commandName;
        private readonly DTE dte;

        public ContextualCommandItem(
            DTE dte,
            string title,
            string description,
            string commandName,
            ImageMoniker icon = default)
        {
            this.dte = dte ?? throw new ArgumentNullException(nameof(dte));
            this.Title = title ?? throw new ArgumentNullException(nameof(title));
            this.Description = description;
            this.commandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
            this.Icon = icon;
        }

        public override string Title { get; }

        public override string Description { get; }

        public override ImageMoniker Icon { get; }

        public override int Priority { get; } = 1;

        public override void Invoke()
        {
            try
            {
                this.dte.ExecuteCommand(this.commandName);
            }
            catch (Exception)
            {
                // Don't die on command not available.
            }
        }
    }
}
