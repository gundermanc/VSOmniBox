namespace VSOmniBox.DefaultProviders.NPDTemplate
{
    using System;
    using System.Diagnostics;
    using Microsoft.Internal.VisualStudio.Shell;
    using Microsoft.Internal.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Imaging;
    using Microsoft.VisualStudio.Imaging.Interop;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.TemplateProviders;
    using VSOmniBox.API.Data;

    internal sealed class NPDTemplateSearchResultShim : OmniBoxItem
    {
        private readonly IVsSearchResultTemplate template;
        private readonly IVsTemplateProvider installedTemplateProvider;

        public NPDTemplateSearchResultShim(IVsSearchResultTemplate template, IVsTemplateProvider installedTemplateProvider)
        {
            Validate.IsNotNull(template, nameof(template));
            Validate.IsNotNull(installedTemplateProvider, nameof(installedTemplateProvider));

            this.template = template;
            this.installedTemplateProvider = installedTemplateProvider;
        }

        public override string Title => "Create Project -> " + template.DefaultName;

        public override string Description => template.Description;

        public override ImageMoniker Icon => KnownMonikers.Solution;

        public override int Priority => 1;

        public override void Invoke()
        {
            VSNEWPROJECTDLGINFO info = new VSNEWPROJECTDLGINFO();
            info.pwzExpand = (this.installedTemplateProvider as IVsTemplatePathServices)?.GetPathFromNode(template.OwnerNode);
            info.pwzSelect = template.Name;

            LaunchNewProjectDialog(info);
        }

        public static void LaunchNewProjectDialog(VSNEWPROJECTDLGINFO newProjectDialogInfo)
        {
            IVsDialogService dialogService = ServiceProvider.GlobalProvider.GetService(typeof(SVsDialogService)) as IVsDialogService;
            if (dialogService != null)
            {
                string loc;
                try
                {
                    dialogService.InvokeDialog(newProjectDialogInfo, out loc);
                }
                catch (Exception ex)
                {
                    Debug.Fail("Failed to invoke new project dialog: " + ex.Message);
                }
            }
        }
    }
   
}
