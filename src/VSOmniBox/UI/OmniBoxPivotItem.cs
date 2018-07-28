namespace VSOmniBox.UI
{
    using System;
    using VSOmniBox.API.Data;

    internal sealed class OmniBoxPivotItem : OmniBoxItem
    {
        private readonly Action action;

        public OmniBoxPivotItem(
            string title,
            string description,
            Action action = null)
        {
            this.Title = title;
            this.Description = description;
            this.action = action;
        }

        public bool IsPivot { get; } = true;

        public override string Title { get; }

        public override string Description { get; }

        public override void Invoke() => this.action?.Invoke();
    }
}
