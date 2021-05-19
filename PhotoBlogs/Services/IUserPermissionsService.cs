using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhotoBlogs.Models;

namespace PhotoBlogs.Services
{
    public interface IUserPermissionsService
    {
        Boolean CanEditPost(Post post);

        Boolean CanEditPostComment(PostComment postComment);
    }
}
