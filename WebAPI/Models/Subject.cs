using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }
        public int StageId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public Stage Stage { get; set; }
        public ICollection<Section> Sections { get; set; }
    }
}
