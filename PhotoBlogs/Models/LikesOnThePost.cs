using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoBlogs.Models
{
    public class LikesOnThePost
    {
        public Int32 Id { get; set; }

        public Guid PostId { get; set; }

        public Post Post { get; set; }

        [Required]
        public String UserIdWhoPutTheLike { get; set; }

        public Boolean IsLikedIt { get; set; } = false; 
    }
}
