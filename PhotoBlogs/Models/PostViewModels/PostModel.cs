using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PhotoBlogs.Models.PostViewModels
{
    public class PostModel
    {
        [Required]
        public String Name { get; set; }

        [Required]
        public String Description { get; set; }

        [Required]
        public IFormFile File { get; set; }

        [Required]
        public String Tags { get; set; }
    }
}
