using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhotoBlogs.Data;
using PhotoBlogs.Models;
using PhotoBlogs.Models.PostViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PhotoBlogs.Services;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Net.Http.Headers;
using System.Collections.ObjectModel;

namespace PhotoBlogs.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private static readonly HashSet<String> AllowedExtensions = new HashSet<String> { ".jpg", ".jpeg", ".png", ".gif" };
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;
        private readonly IHostingEnvironment hostingEnvironment;

        public PostsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
             IUserPermissionsService userPermissions,
              IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
            this.hostingEnvironment = hostingEnvironment;

        }

        // GET: Posts
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            this.ViewBag.Title = "All photos";
            var applicationDbContext = _context.Posts.Include(p => p.Creator);
            return View(await applicationDbContext.ToListAsync());
        }

        [AllowAnonymous]
        public async Task<IActionResult> IndexWithTag(String tag)
        {
            if (tag==null)
            {
                return this.NotFound();
            }

            var posts = await _context.Posts
                .Where(x=> this._context.TagPosts.Any(t=> x.Id == t.PostId && t.Tag == tag))
                .Include(p => p.Creator)
                .ToListAsync();
            this.ViewBag.Title = $"Photos with tag #{tag}";
            return View("Index",posts);
        }

        [AllowAnonymous]
        public async Task<IActionResult> IndexWithUser(String userName)
        {
            if (userName == null)
            {
                return this.NotFound();
            }

            var posts = await _context.Posts
                .Include(p => p.Creator)
                .Where(c=>c.Creator.UserName == userName)
                .ToListAsync();
            this.ViewBag.Title = $"Photos by {userName}";
            return View("Index", posts);
        }

        //  GET: Posts/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid? postId)
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

            return View(post);
        }

        [AllowAnonymous]
        [HttpGet, ActionName("Details")]
        public async Task<IActionResult> DetailsWithEror(Guid? postId, String error)
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

            ViewBag.Error = error;

            return View("Details",post);
        }

        // GET: Posts/Create
        [Authorize]
        public IActionResult Create()
        {
            return View(new PostModel());
        }

        // POST: Posts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(PostModel model)
        {
            var user = await this.userManager.GetUserAsync(this.HttpContext.User);

            if (model.File == null)
            {
                this.ModelState.AddModelError(nameof(model.File), "This file is empty");
                return this.View(model);
            }

            var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.File.ContentDisposition).FileName.Value.Trim('"'));
            var fileExt = Path.GetExtension(fileName);
            if (!PostsController.AllowedExtensions.Contains(fileExt))
            {
                this.ModelState.AddModelError(nameof(model.File), "This file type is prohibited");
            }

            if (ModelState.IsValid && user != null)
            {

                var post = new Post
                {
                    Name = model.Name,
                    Description = model.Description,
                    Tags = new Collection<TagPost>(),
                    CreatorId = user.Id
                };
                post.Id = Guid.NewGuid();
                var attachmentPath = Path.Combine(this.hostingEnvironment.WebRootPath, "photos", post.Id.ToString("N") + fileExt);
                post.FilePath = $"/photos/{post.Id:N}{fileExt}";
                using (var fileStream = new FileStream(attachmentPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                {
                    await model.File.CopyToAsync(fileStream);
                }
                var tagId = 1;

                foreach (var item in model.Tags.Split(',').Select(x => x.Trim()).Where(x => !String.IsNullOrEmpty(x)))
                {
                    post.Tags.Add(new TagPost
                    {
                        PostId = post.Id,
                        TagId = tagId++,
                        Tag = item
                    });
                }

                try
                {
                    _context.Add(post);
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    return this.NotFound();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Posts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(Guid? postId)
        {
            if (postId == null)
            {
                return NotFound();
            }


            var post = await _context.Posts
                .Include(t=>t.Tags)
                .SingleOrDefaultAsync(m => m.Id == postId);
            if (post == null || !this.userPermissions.CanEditPost(post))
            {
                return NotFound();
            }

            var model = new PostEditModel
            {
                Name = post.Name,
                Description = post.Description,
                Tags = String.Join(", ", post.Tags.OrderBy(x => x.TagId).Select(x => x.Tag))
            };
            ViewBag.Post = post;

            return View(model);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(Guid? postId, PostEditModel model)
        {
            if (postId == null)
            {
                return NotFound();
            }


            var post = await _context.Posts
                .Include(t => t.Tags)
                .SingleOrDefaultAsync(m => m.Id == postId);
            if (post == null || !this.userPermissions.CanEditPost(post))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                post.Name = model.Name;
                post.Description = model.Description;

                var tagId = post.Tags.Any() ? post.Tags.Max(x => x.TagId) + 1 : 1;
                post.Tags.Clear();
                if (model.Tags != null)
                {
                    foreach (var item in model.Tags.Split(",").Select(x => x.Trim()).Where(x => !String.IsNullOrEmpty(x)))
                    {
                        post.Tags.Add(new TagPost
                        {
                            TagId = tagId++,
                            Tag = item
                        });
                    }
                }
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Post = post;
            return View(post);
        }

        // GET: Posts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(Guid? postId)
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

            if (post == null || !this.userPermissions.CanEditPost(post))
            {
                return NotFound();
            }

            this.ViewBag.Post = post;
            return View(post);
        }

        // POST: Posts/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid? postId)
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

            if (post == null || !this.userPermissions.CanEditPost(post))
            {
                return NotFound();
            }
            var attachmentPath = Path.Combine(this.hostingEnvironment.WebRootPath, "photos", post.Id.ToString("N") + Path.GetExtension(post.FilePath));
            System.IO.File.Delete(attachmentPath);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PutLikes(Guid? postId)
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
                 .Include(l=>l.LikesOnThePosts)
                 .SingleOrDefaultAsync(m => m.Id == postId);

            var user = await this.userManager.GetUserAsync(this.HttpContext.User);

            if (post == null || user == null)
            {
                return NotFound();
            }

            if (post.LikesOnThePosts == null)
            {
                post.LikesOnThePosts.Add(new LikesOnThePost
                {
                    UserIdWhoPutTheLike = user.Id,
                    IsLikedIt = true,
                    PostId = post.Id
                });
                post.NumberOfLikes++;
                await _context.SaveChangesAsync();
            }
            else
            {
                var like = await this._context.LikesOnThePosts
                    .Include(p => p.Post)
                    .Where(x => x.PostId == post.Id)
                    .SingleOrDefaultAsync(x => x.UserIdWhoPutTheLike == user.Id);

                if(like == null)
                {
                    post.LikesOnThePosts.Add(new LikesOnThePost
                    {
                        UserIdWhoPutTheLike = user.Id,
                        IsLikedIt = true,
                        PostId = post.Id
                    });
                    post.NumberOfLikes++;
                }
                else
                {
                    if (like.IsLikedIt)
                    {
                        like.IsLikedIt = false;
                        post.NumberOfLikes--;
                    }
                    else
                    {
                        like.IsLikedIt = true;
                        post.NumberOfLikes++;
                    }
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { postId = post.Id});
        }
    }
}
