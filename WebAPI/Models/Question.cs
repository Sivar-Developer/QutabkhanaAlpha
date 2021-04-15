using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int SectionId { get; set; }
        [Required]
        public int Test { get; set; }
        public string Body { get; set; }
        public string Image { get; set; }
        [Required]
        public string OptionA { get; set; }
        [Required]
        public string OptionB { get; set; }
        [Required]
        public string OptionC { get; set; }
        [Required]
        public string OptionD { get; set; }
        [Required]
        public string Answer { get; set; }
        public Section Section { get; set; }
    }
}
