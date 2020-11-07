using EasyMongoNet;
using System.Web;

namespace AliaaCommon.Models
{
    public static class AuthUserDBExtentionX
    {
        public static AuthUser GetCurrentUser(this IReadOnlyDbContext db)
        {
            if (HttpContext.Current == null || HttpContext.Current.User == null || HttpContext.Current.User.Identity == null || string.IsNullOrEmpty(HttpContext.Current.User.Identity.Name))
                return null;
            return AuthUserDBExtention.GetUserByUsername(db, HttpContext.Current.User.Identity.Name);
        }
    }
}
