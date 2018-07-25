namespace VSOmniBox.Commands
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;
    using VSOmniBox.UI;

    // This nasty bit of hackery is necessary to work around the editor's monopoly
    // on commands..when the editor is open, it seems to steal every keystroke
    // except TYPECHAR, even when we're the ones that are focused! This command
    // target hooks into the editor's command chain and intercepts these VS commands
    // and feeds them to the omni box again as WPF routed commands.
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
            if (this.omniBoxBroker.IsVisible)
            {
                if (pguidCmdGroup == VSConstants.CMDSETID.StandardCommandSet2K_guid)
                {
                    // VS only ever requests status of one command at a time.
                    switch (prgCmds[0].cmdID)
                    {
                        case (uint)VSConstants.VSStd2KCmdID.BACKSPACE:
                        case (uint)VSConstants.VSStd2KCmdID.COPY:
                        case (uint)VSConstants.VSStd2KCmdID.DOWN:
                        case (uint)VSConstants.VSStd2KCmdID.RETURN:
                        case (uint)VSConstants.VSStd2KCmdID.LEFT:
                        case (uint)VSConstants.VSStd2KCmdID.LEFT_EXT:
                        case (uint)VSConstants.VSStd2KCmdID.PASTE:
                        case (uint)VSConstants.VSStd2KCmdID.RIGHT:
                        case (uint)VSConstants.VSStd2KCmdID.RIGHT_EXT:
                        case (uint)VSConstants.VSStd2KCmdID.SELECTALL:
                        case (uint)VSConstants.VSStd2KCmdID.UP:
                            prgCmds[0].cmdf = (int)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                            break;
                    }
                }

                // We're visible, swallow all commands.
                return VSConstants.S_OK;
            }

            return this.next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (this.omniBoxBroker.IsVisible)
            {
                if (pguidCmdGroup == VSConstants.CMDSETID.StandardCommandSet2K_guid)
                {
                    switch (nCmdID)
                    {
                        case (uint)VSConstants.VSStd2KCmdID.BACKSPACE:
                            this.RaiseKeyEvent(Key.Back);
                            return VSConstants.S_OK;
                        case (uint)VSConstants.VSStd2KCmdID.COPY:
                            // NOTE: this probably breaks on non en-US keyboards and should be done via commands.
                            this.RaiseKeyEvent(Key.LeftCtrl);
                            this.RaiseKeyEvent(Key.C);
                            return VSConstants.S_OK;
                        case (uint)VSConstants.VSStd2KCmdID.CUT:
                            // NOTE: this probably breaks on non en-US keyboards and should be done via commands.
                            this.RaiseKeyEvent(Key.LeftCtrl);
                            this.RaiseKeyEvent(Key.X);
                            return VSConstants.S_OK;
                        case (uint)VSConstants.VSStd2KCmdID.DOWN:
                            this.RaiseKeyEvent(Key.Down);
                            return VSConstants.S_OK;
                        case (uint)VSConstants.VSStd2KCmdID.RETURN:
                            this.RaiseKeyEvent(Key.Return);
                            return VSConstants.S_OK;
                        case (uint)VSConstants.VSStd2KCmdID.LEFT:
                            this.RaiseKeyEvent(Key.Left);
                            return VSConstants.S_OK;
                        case (uint)VSConstants.VSStd2KCmdID.LEFT_EXT:
                            this.RaiseKeyEvent(Key.LeftShift);
                            this.RaiseKeyEvent(Key.Left);
                            return VSConstants.S_OK;
                        case (uint)VSConstants.VSStd2KCmdID.PASTE:
                            // NOTE: this probably breaks on non en-US keyboards and should be done via commands.
                            this.RaiseKeyEvent(Key.LeftCtrl);
                            this.RaiseKeyEvent(Key.V);
                            return VSConstants.S_OK;
                        case (uint)VSConstants.VSStd2KCmdID.RIGHT:
                            this.RaiseKeyEvent(Key.Right);
                            return VSConstants.S_OK;
                        case (uint)VSConstants.VSStd2KCmdID.RIGHT_EXT:
                            this.RaiseKeyEvent(Key.RightShift);
                            this.RaiseKeyEvent(Key.Right);
                            return VSConstants.S_OK;
                        case (uint)VSConstants.VSStd2KCmdID.SELECTALL:
                            // NOTE: this probably breaks on non en-US keyboards and should be done via commands.
                            this.RaiseKeyEvent(Key.LeftCtrl);
                            this.RaiseKeyEvent(Key.A);
                            return VSConstants.S_OK;
                        case (uint)VSConstants.VSStd2KCmdID.UP:
                            this.RaiseKeyEvent(Key.Up);
                            return VSConstants.S_OK;
                    }
                }

                // We're visible, swallow all commands.
                return VSConstants.S_OK;
            }

            return this.next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private void RaiseKeyEvent(Key key)
        {
            var target = Keyboard.FocusedElement;
            var eventArgs = new KeyEventArgs(
                Keyboard.PrimaryDevice,
                PresentationSource.FromVisual((Visual)target),
                0,
                key)
            {
                RoutedEvent = Keyboard.KeyDownEvent
            };

            target.RaiseEvent(eventArgs);
        }
    }
}
