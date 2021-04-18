using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models.DTOs.Requests
{
    public class StageRequest
    {
        [Required]
        public int StageId { get; set; }
    }
}
