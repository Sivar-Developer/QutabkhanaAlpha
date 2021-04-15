using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class User : IdentityUser
    {
        public int? StageId { get; set; }
        public Stage Stage { get; set; }
    }
}
