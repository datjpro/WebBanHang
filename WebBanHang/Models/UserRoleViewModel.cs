using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebBanHang.Models
{
    public class UserRoleViewModel
    {
        public ApplicationUser User { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class ManageUserRolesViewModel
    {
        public ApplicationUser User { get; set; }
        public IList<string> UserRoles { get; set; }
        public List<SelectListItem> AllRoles { get; set; }
    }
}
