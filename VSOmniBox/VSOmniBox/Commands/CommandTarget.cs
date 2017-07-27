namespace VSOmniBox.Commands
{
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using VSOmniBox.API;

    internal sealed class CommandTarget
    {
        private static readonly Guid CommandSetGuid = new Guid("0c9efb12-486c-4c6e-8e7c-7071409818c0");
        private static readonly CommandID InvokeSearchCommandId = new CommandID(CommandSetGuid, 0x0100);

        private readonly MenuCommand InvokeSearchCommand;
        private readonly IOmniBoxService omniBoxService;

        public static void CreateAndRegister(IServiceProvider serviceProvider)
        {
            var commandService = serviceProvider.GetService(typeof(IMenuCommandService)) as IMenuCommandService;

            Debug.Assert(commandService != null);

            if (commandService != null)
            {
                var omniBoxService = VSPackage.GetMefService<IOmniBoxService>();

                Debug.Assert(omniBoxService != null);

                if (omniBoxService != null)
                {
                    new CommandTarget(commandService, omniBoxService);
                }
            }
        }

        private CommandTarget(IMenuCommandService commandService, IOmniBoxService omniBoxService)
        {
            this.InvokeSearchCommand = new MenuCommand(this.OnInvokeSearch, InvokeSearchCommandId);
            commandService.AddCommand(InvokeSearchCommand);

            this.omniBoxService = omniBoxService;
        }

        private void OnInvokeSearch(object sender, EventArgs e) => this.omniBoxService.Broker.IsVisible = !this.omniBoxService.Broker.IsVisible;
    }
}
