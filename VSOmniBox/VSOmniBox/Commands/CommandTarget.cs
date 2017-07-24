namespace VSOmniBox.Commands
{
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;

    internal sealed class CommandTarget
    {
        private static readonly Guid CommandSetGuid = new Guid("0c9efb12-486c-4c6e-8e7c-7071409818c0");
        private static readonly CommandID InvokeSearchCommandId = new CommandID(CommandSetGuid, 0x0100);

        private readonly MenuCommand InvokeSearchCommand;

        public static void CreateAndRegister(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var commandService = serviceProvider.GetService(typeof(IMenuCommandService)) as IMenuCommandService;

            Debug.Assert(commandService != null);

            if (commandService != null)
            {
                new CommandTarget(commandService);
            }
        }

        private CommandTarget(IMenuCommandService commandService)
        {
            this.InvokeSearchCommand = new MenuCommand(this.OnInvokeSearch, InvokeSearchCommandId);
            commandService.AddCommand(InvokeSearchCommand);
        }

        private void OnInvokeSearch(object sender, EventArgs e)
        {
            // TODO: bring up search.
        }
    }
}
