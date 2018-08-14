namespace VSOmniBox.DefaultProviders.ContextualCommands
{
    using System;
    using EnvDTE;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Editor;
    using Microsoft.VisualStudio.Imaging;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Operations;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Microsoft.VisualStudio.Threading;
    using VSOmniBox.API.Data;

    internal class ContextualCommandsItemsSource : IOmniBoxItemsSource
    {
        private readonly JoinableTaskContext joinableTaskContext;
        private readonly SVsServiceProvider serviceProvider;
        private readonly IVsEditorAdaptersFactoryService adaptersFactory;
        private readonly ITextStructureNavigatorSelectorService navigatorService;

        private DTE dte;
        private IVsTextManager2 textManager;

        public ContextualCommandsItemsSource(
            JoinableTaskContext joinableTaskContext,
            SVsServiceProvider serviceProvider,
            IVsEditorAdaptersFactoryService adaptersFactory,
            ITextStructureNavigatorSelectorService navigatorService)
        {
            this.joinableTaskContext = joinableTaskContext ?? throw new ArgumentNullException(nameof(joinableTaskContext));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.adaptersFactory = adaptersFactory ?? throw new ArgumentNullException(nameof(adaptersFactory));
            this.navigatorService = navigatorService ?? throw new ArgumentNullException(nameof(navigatorService));
        }

        public async System.Threading.Tasks.Task GetItemsAsync(string searchString, IOmniBoxSearchSession searchSession)
        {
            await this.joinableTaskContext.Factory.SwitchToMainThreadAsync();

            if (this.dte == null)
            {
                this.dte = (DTE)this.serviceProvider.GetService(typeof(DTE));
            }

            if (this.textManager == null)
            {
                this.textManager = (IVsTextManager2)this.serviceProvider.GetService(typeof(SVsTextManager));
            }

            ITextView textView;
            if (!ErrorHandler.Succeeded(this.textManager.GetActiveView2(fMustHaveFocus: 1, pBuffer: null, grfIncludeViewFrameType: 0, out var ppView)) ||
                (textView = this.adaptersFactory.GetWpfTextView(ppView)) == null)
            {
                this.ConsiderShellCommands(searchSession);
            }
            else
            {
                this.ConsiderEditorCommands(textView, searchSession);
            }
        }

        private void ConsiderEditorCommands(ITextView textView, IOmniBoxSearchSession searchSession)
        {
            if (textView.Selection.Mode == TextSelectionMode.Stream && !textView.Selection.StreamSelectionSpan.IsEmpty)
            {
                this.ConsiderEditorSelectionCommands(textView, searchSession);
            }
            else
            {
                var navigator = this.navigatorService.GetTextStructureNavigator(textView.TextBuffer);
                if (navigator.GetExtentOfWord(textView.Caret.Position.BufferPosition).IsSignificant)
                {
                    this.ConsiderEditorTokenCommand(textView, searchSession);
                }
                else
                {
                    this.ConsiderEditorWhitespaceCommands(textView, searchSession);
                }
            }
        }

        // TODO: consider taking into account what tool windows are open, whether or not we are debugging, etc.
        // when choosing what to display.

        private void ConsiderEditorWhitespaceCommands(ITextView textView, IOmniBoxSearchSession searchSession)
        {
            searchSession.AddItem(new ContextualCommandItem(
                this.dte,
                "Format Document",
                "Formats the current document according to your preferences...",
                "Edit.FormatDocument",
                KnownMonikers.FormatDocument));

            searchSession.AddItem(new ContextualCommandItem(
                this.dte,
                "Build Solution",
                "Build all projects in the currently open solution",
                "Build.BuildSolution",
                KnownMonikers.BuildSolution));

            searchSession.AddItem(new ContextualCommandItem(
                this.dte,
                "Start Debugging",
                "Build and debug the current 'startup' project...",
                "Debug.Start",
                KnownMonikers.Play));

            searchSession.AddItem(new ContextualCommandItem(
                this.dte,
                "Error List",
                "View errors for the current projects and solution",
                "View.ErrorList",
                KnownMonikers.BuildErrorList));
        }

        private void ConsiderEditorTokenCommand(ITextView textView, IOmniBoxSearchSession searchSession)
        {
            searchSession.AddItem(new ContextualCommandItem(
                this.dte,
                "Go to Definition",
                "Go to the definition of teh type directly under the caret...",
                "Edit.GoToDefinition",
                KnownMonikers.GoToDefinition));

            searchSession.AddItem(new ContextualCommandItem(
                this.dte,
                "Go to Implementation",
                "Go to the implementation of the type directly under the caret...",
                "Edit.GoToImplementation",
                KnownMonikers.ApplicationBarCommand));

            searchSession.AddItem(new ContextualCommandItem(
                this.dte,
                "Find all References",
                "Find all references to the type directly under the caret...",
                "Edit.FindAllReferences",
                KnownMonikers.FindSymbol));

            searchSession.AddItem(new ContextualCommandItem(
                this.dte,
                "Rename",
                "Rename the type directly under the caret...",
                "Refactor.Rename",
                KnownMonikers.Rename));
        }

        private void ConsiderEditorSelectionCommands(ITextView textView, IOmniBoxSearchSession searchSession)
        {
            searchSession.AddItem(new ContextualCommandItem(
                this.dte,
                "Quick Find",
                "Quickly perform a naive text search...",
                "Edit.QuickFind",
                KnownMonikers.QuickFind));

            searchSession.AddItem(new ContextualCommandItem(
                this.dte,
                "Format Selection",
                "Format selection in the current document...",
                "Edit.FormatSelection",
                KnownMonikers.FormatSelection));
        }

        private void ConsiderShellCommands(IOmniBoxSearchSession searchSession)
        {
            searchSession.AddItem(new ContextualCommandItem(
                this.dte,
                "New Project",
                "Start typing to search projects or click here to start NPD",
                "File.NewProject",
                KnownMonikers.Solution));

            searchSession.AddItem(new ContextualCommandItem(
                this.dte,
                "Error List",
                "View errors for the current projects and solution",
                "View.ErrorList",
                KnownMonikers.BuildErrorList));

            searchSession.AddItem(new ContextualCommandItem(
                this.dte,
                "Add Item to Project",
                "Add new class, control, test, or other file to this project",
                "Project.AddNewItem",
                KnownMonikers.AddItem));

            searchSession.AddItem(new ContextualCommandItem(
                this.dte,
                "Solution Explorer",
                "Explore files in the open projects and solution",
                "View.SolutionExplorer",
                KnownMonikers.Solution));
        }
    }
}