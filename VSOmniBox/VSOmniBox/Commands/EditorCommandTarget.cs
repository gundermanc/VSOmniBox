namespace VSOmniBox.Commands
{
    using System;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;
    using VSOmniBox.Service;

    // Receives commands from the IDE when the editor TextView is open and focused.
    internal sealed class EditorCommandTarget : IOleCommandTarget
    {
        private readonly IOleCommandTarget next;
        private readonly OmniBoxBroker omniBoxBroker;

        public EditorCommandTarget(IVsTextView textViewAdapter, OmniBoxBroker omniBoxBroker)
        {
            this.omniBoxBroker = omniBoxBroker
                ?? throw new ArgumentNullException(nameof(omniBoxBroker));

            if (textViewAdapter == null)
            {
                throw new ArgumentNullException(nameof(textViewAdapter));
            }

            textViewAdapter.AddCommandFilter(this, out this.next);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == VSConstants.CMDSETID.StandardCommandSet2K_guid)
            {
                // VS only ever requests status of one command at a time.
                switch (prgCmds[0].cmdID)
                {
                    case (int)VSConstants.VSStd2KCmdID.DOWN:
                    case (int)VSConstants.VSStd2KCmdID.RETURN:
                    case (int)VSConstants.VSStd2KCmdID.UP:
                        prgCmds[0].cmdf = (int)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                        break;
                }
            }

            return this.next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == VSConstants.CMDSETID.StandardCommandSet2K_guid)
            {
                // VS only ever requests status of one command at a time.
                switch (nCmdID)
                {
                    case (uint)VSConstants.VSStd2KCmdID.BACKSPACE:
                        this.omniBoxBroker.InvokeBackspaceCommand();
                        return VSConstants.S_OK;
                    case (uint)VSConstants.VSStd2KCmdID.DOWN:
                        this.omniBoxBroker.InvokeDownCommand();
                        return VSConstants.S_OK;
                    case (uint)VSConstants.VSStd2KCmdID.RETURN:
                        this.omniBoxBroker.InvokeReturnCommand();
                        return VSConstants.S_OK;
                    case (uint)VSConstants.VSStd2KCmdID.LEFT:
                        this.omniBoxBroker.InvokeLeftCommand();
                        return VSConstants.S_OK;
                    case (uint)VSConstants.VSStd2KCmdID.RIGHT:
                        this.omniBoxBroker.InvokeRightCommand();
                        return VSConstants.S_OK;
                    case (uint)VSConstants.VSStd2KCmdID.UP:
                        this.omniBoxBroker.InvokeUpCommand();
                        return VSConstants.S_OK;
                }
            }

            return this.next.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }
    }
}
