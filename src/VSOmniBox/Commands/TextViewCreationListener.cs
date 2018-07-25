namespace VSOmniBox.Commands
{
    using System;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Editor;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Microsoft.VisualStudio.Utilities;
    using VSOmniBox.API.UI;
    using VSOmniBox.UI;

    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("any")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    [TextViewRole(PredefinedTextViewRoles.EmbeddedPeekTextView)]
    internal sealed class TextViewCreationListener : IVsTextViewCreationListener
    {
        private readonly IVsEditorAdaptersFactoryService adaptersFactoryService;
        private readonly IOmniBoxUIService omniBoxBroker;

        [ImportingConstructor]
        public TextViewCreationListener(
            IVsEditorAdaptersFactoryService adaptersFactoryService,
            IOmniBoxUIService omniBoxBroker)
        {
            this.adaptersFactoryService = adaptersFactoryService
                ?? throw new ArgumentNullException(nameof(adaptersFactoryService));
            this.omniBoxBroker = omniBoxBroker
                ?? throw new ArgumentNullException(nameof(omniBoxBroker));
        }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
            => new EditorCommandTarget(textViewAdapter, (OmniBoxBroker)this.omniBoxBroker);
    }
}
