using Kentico.Xperience.Admin.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.ContentEngine;
using XperienceCommunity.MemberRoles;

[assembly: CMS.RegisterModule(typeof(MemberRolesAdminModule))]

// Adds a new application category 

namespace XperienceCommunity.MemberRoles
{
    internal class MemberRolesAdminModule : AdminModule
    {
        public MemberRolesAdminModule()
            : base("MemberRoles")
        {
        }

        protected override void OnInit(ModuleInitParameters parameters)
        {
            base.OnInit();

            TaxonomyInfo.TYPEINFO.Events.Delete.Before += Taxonomy_Delete_Before;

            // Change the organization name and project name in the client scripts registration
            RegisterClientModule("memberroles", "web-admin");
        }

        private void Taxonomy_Delete_Before(object? sender, ObjectEventArgs e)
        {
            var taxonomy = (TaxonomyInfo)e.Object;

            // Prevent member role taxonomy from being deleted
            if(taxonomy.TaxonomyName.Equals(MemberRoleConstants._MemberRoleTaxonomy, StringComparison.OrdinalIgnoreCase)) {
                e.Cancel();
            }
        }
    }
}
