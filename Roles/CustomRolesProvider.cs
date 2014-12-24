using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace MTOLVN.Models
{
    public class CustomRolesProvider : RoleProvider
    {
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            using (var db = new DefaultConnection())
            {
                //var roles = db.Roles.Where(x => x.Groups.Any(g => g.Users.Any(u => u.Id == username))).Select(r => r.RoleName);
                var roles = db.proc_GetRolesForUser(username).ToList();
                if (roles.Any())
                {
                    return roles.ToArray();
                }
                return new[] { "User" };
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
            //using (var db = new DefaultConnection())
            //{
            //    var data = HttpContext.Current.User.Identity.Name;
            //    return db.proc_GetRolesForUser(data).Any(x => x == roleName);
            //}
        }
    }
}