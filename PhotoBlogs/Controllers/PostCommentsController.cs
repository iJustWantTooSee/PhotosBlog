using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhotoBlogs.Data;
using PhotoBlogs.Models;
using Microsoft.AspNetCore.Identity;
using PhotoBlogs.Services;
using PhotoBlogs.Models.PostViewModels;
using Microsoft.AspNetCore.Authorization;

namespace PhotoBlogs.Controllers
{
    [Authorize]
    public class PostCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;
        public PostCommentsController(ApplicationDbContext context,
             UserManager<ApplicationUser> userManager,
             IUserPermissionsService userPermissions)
        {
            _context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
        }




        // POST: PostComments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Guid? postId, PostCommentModel postCommentModel)
        {
            if (postId == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Creator)
                .Include(t => t.Tags)
                .Include(c => c.Comments)
                .ThenInclude(u => u.Creator)
                .SingleOrDefaultAsync(m => m.Id == postId);
            if (post == null)
            {
                return NotFound();
            }
            var user = await this.userManager.GetUserAsync(this.HttpContext.User);

            if (ModelState.IsValid && user != null)
            {
                var postComment = new PostComment
                {
                    Id = Guid.NewGuid(),
                    Created = DateTime.UtcNow,
                    Text = postCommentModel.Text,
                    PostId = post.Id,
                    CreatorId = user.Id
                };
                _context.Add(postComment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Posts", new { postId = postId });
            }
            return RedirectToAction("Details", "Posts", new { postId = postId, error = "Нельзя создавать пустой комментарий" });
        }


        // GET: PostComments/Delete/5
        //public async Task<IActionResult> Delete(Guid? id)
        //{
        //            if (id == null)
        //            {
        //                return NotFound();
        //    }

        //    var postComment = await _context.PostComments
        //        .Include(p => p.Creator)
        //        .Include(p => p.Post)
        //        .SingleOrDefaultAsync(m => m.Id == id);
        //            if (postComment == null)
        //            {
        //                return NotFound();
        //}

        //    return View(postComment);
        //}

        // POST: PostComments/Delete/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid? postId, Guid? commentId)
        {
            if (postId == null || commentId == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
               .Include(p => p.Creator)
               .Include(t => t.Tags)
               .Include(c => c.Comments)
               .ThenInclude(u => u.Creator)
               .SingleOrDefaultAsync(m => m.Id == postId);

            var postComment = await _context.PostComments
                .Include(x => x.Creator)
                .SingleOrDefaultAsync(m => m.Id == commentId);

            if (postComment == null || postComment == null || !this.userPermissions.CanEditPostComment(postComment))
            {
                return NotFound();
            }
           
            _context.PostComments.Remove(postComment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Posts", new { postId = postId });
        }

    }
}
