using CMS;
using CMS.Base;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.MemberRoles;

[assembly: RegisterModule(typeof(MemberRolesModule))]

namespace XperienceCommunity.MemberRoles
{
    public class MemberRolesModule : Module
    {
        private IServiceProvider? _services;
        private MemberRolesInstaller? _installer;

        public MemberRolesModule() : base("XperienceCommunity.MemberRolesModule")
        {

        }
        protected override void OnInit(ModuleInitParameters parameters)
        {
            base.OnInit();

            // Prevent Member Role Taxonomy from being deleted
            TaxonomyInfo.TYPEINFO.Events.Delete.Before += Taxonomy_Delete_Before;

            // Installation
            _services = parameters.Services;
            _installer = _services.GetRequiredService<MemberRolesInstaller>();
            ApplicationEvents.Initialized.Execute += InitializeModule;
        }

        private void InitializeModule(object? sender, EventArgs e)
        {
            _installer?.Install();
        }

        private void Taxonomy_Delete_Before(object? sender, ObjectEventArgs e)
        {
            var taxonomy = (TaxonomyInfo)e.Object;
            if (taxonomy.TaxonomyName.Equals(MemberRoleConstants._MemberRoleTaxonomy, StringComparison.OrdinalIgnoreCase)) {
                e.Cancel();
            }
        }
    }
}
