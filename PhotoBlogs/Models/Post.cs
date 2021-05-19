using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoBlogs.Models
{
    public class Post
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public String CreatorId { get; set; }

        public ApplicationUser Creator { get; set; }

        [Required]
        public String Name { get; set; }

        [Required]
        public String Description { get; set; }

        [Required]
        public String FilePath { get; set; }

        public Int32 NumberOfLikes { get; set; } = 0;

        public ICollection<LikesOnThePost> LikesOnThePosts { get; set; }

        public ICollection<TagPost> Tags { get; set; }

        public ICollection<PostComment> Comments { get; set; }

    }
}
