using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class Section
    {
        public int Id { get; set; }
        public int Orderable { get; set; }
        public string Name { get; set; }
        public Stage Stage { get; set; }
        public Subject Subject { get; set; }
        public ICollection<Question> Questions { get; set; }


    }
}
