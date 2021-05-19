using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoBlogs.Models.PostViewModels
{
    public class PostEditModel
    {

        [Required]
        public String Name { get; set; }

        [Required]
        public String Description { get; set; }
       
        [Required]
        public String Tags { get; set; }
    }

}
