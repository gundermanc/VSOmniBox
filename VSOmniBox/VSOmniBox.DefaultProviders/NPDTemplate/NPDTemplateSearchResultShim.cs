using Microsoft.Internal.VisualStudio.Shell;
using Microsoft.Internal.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOmniBox.API.Data;

namespace VSOmniBox.DefaultProviders.NPDTemplate
{
    internal class NPDTemplateSearchResultShim : OmniBoxItem
    {
        private IVsSearchResultTemplate template;
        private IVsTemplateProvider installedTemplateProvider;

        public NPDTemplateSearchResultShim(IVsSearchResultTemplate template, IVsTemplateProvider installedTemplateProvider)
        {
            Validate.IsNotNull(template, nameof(template));
            Validate.IsNotNull(installedTemplateProvider, nameof(installedTemplateProvider));

            this.template = template;
            this.installedTemplateProvider = installedTemplateProvider;
        }

        public override string Title => "Create Project -> " + template.DefaultName;

        public override string Description => template.Description;

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
                    
                }

                //UpdateProjectMRU();
            }
        }
    }
   
}
