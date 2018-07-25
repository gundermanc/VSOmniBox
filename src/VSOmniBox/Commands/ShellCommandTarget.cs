namespace VSOmniBox.Commands
{
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using VSOmniBox.API.UI;
    using VSOmniBox.UI;

    // Receives commands from the IDE when the TextView is not focused.
    internal sealed class ShellCommandTarget
    {
        private static readonly Guid CommandSetGuid = new Guid("0c9efb12-486c-4c6e-8e7c-7071409818c0");
        private static readonly CommandID InvokeSearchCommandId = new CommandID(CommandSetGuid, 0x0100);

        private readonly MenuCommand InvokeSearchCommand;
        private readonly OmniBoxBroker omniBoxBroker;

        public static void CreateAndRegister(IServiceProvider serviceProvider)
        {
            var commandService = serviceProvider.GetService(typeof(IMenuCommandService)) as IMenuCommandService;

            Debug.Assert(commandService != null);

            if (commandService != null)
            {
                var omniBoxBroker = VSPackage.GetMefService<IOmniBoxUIService>();

                Debug.Assert(omniBoxBroker != null);

                if (omniBoxBroker != null)
                {
                    new ShellCommandTarget(commandService, omniBoxBroker);
                }
            }
        }

        private ShellCommandTarget(IMenuCommandService commandService, IOmniBoxUIService omniBoxBroker)
        {
            this.InvokeSearchCommand = new MenuCommand(this.OnInvokeSearchCommand, InvokeSearchCommandId);
            commandService.AddCommand(this.InvokeSearchCommand);

            this.omniBoxBroker = (OmniBoxBroker)omniBoxBroker;
        }

        private void OnInvokeSearchCommand(object sender, EventArgs e)
            => this.omniBoxBroker.IsVisible = !this.omniBoxBroker.IsVisible;
    }
}
