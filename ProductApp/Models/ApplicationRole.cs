using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper.Contrib.Extensions;

namespace ProductApp.Models
{
    [Dapper.Contrib.Extensions.Table("AspNetRoles")]
    public class ApplicationRole : IdentityRole
    {
    }
}