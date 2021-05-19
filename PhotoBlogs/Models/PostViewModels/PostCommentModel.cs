using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoBlogs.Models.PostViewModels
{
    public class PostCommentModel
    {
        [Required]
        public String Text { get; set; }
    }
}
