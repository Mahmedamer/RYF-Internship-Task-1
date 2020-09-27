using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using NYFInter.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace NYFInter.Controllers
{
    public class AdminController : Controller
    {
        private DbEntries dbEntries = new DbEntries();
        private DbMessages dbmsg = new DbMessages();
        private ApplicationDbContext DbUsers = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        // GET: Admin
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return RedirectToAction("Messages");
        }
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Users_Submissions()
        {
            var model = await dbEntries.Entries.ToListAsync();
            foreach (var item in model)
            {
                item.Username = UserManager.FindByIdAsync(item.UserId).Result.UserName;
            }
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ChangeSubmissionStatus(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Entry entry = await dbEntries.Entries.FindAsync(id);
            entry.Username = UserManager.FindByIdAsync(entry.UserId).Result.UserName;
            if (entry == null)
            {
                return HttpNotFound();
            }
            ViewBag.AvailableStatus = new List<string> {
            "In Query",
            "Under Review",
            "Accepted",
            "Rejected"
            };
            return View(entry);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeSubmissionStatus([Bind(Include = "Id,UserId,Text,Status")] Entry entry)
        {
            if (ModelState.IsValid)
            {
                dbEntries.Entry(entry).State = EntityState.Modified;
                await dbEntries.SaveChangesAsync();
                return RedirectToAction("Users_Submissions");
            }
            ViewBag.AvailableStatus = new List<string> {
            "In Query",
            "Under Review",
            "Accepted",
            "Rejected"
            };
            return View(entry);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult Users()
        {
            var users = DbUsers.Users.AsEnumerable().ToList();
            var model = new List<UserModel>();
            foreach (var user in users)
            {
                model.Add(new UserModel
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber == null ? "Not Set Yet" : user.PhoneNumber,
                    Role = UserManager.GetRoles(userId: user.Id).FirstOrDefault()
                });
            }
            model = model.OrderBy(x => x.Role).ToList();
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult CreateUser()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateUser(UserModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Username == null)
                {
                    model.Username = model.Email;
                }
                var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var context = new ApplicationDbContext();
                    string roleName = "User";
                    var roleManager = new RoleManager<AppRole>(new RoleStore<AppRole>(context));
                    if (!roleManager.RoleExists(roleName))
                        roleManager.Create(new AppRole(roleName));
                    var currentuser = UserManager.FindByEmail(model.Email);
                    await UserManager.AddToRoleAsync(currentuser.Id, roleName);
                    return RedirectToAction("Users", "Admin");
                }
                AddErrors(result);
            }
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        public async Task <ActionResult> Messages()
        {
            var model = await dbmsg.Messages.ToListAsync();
            foreach (var item in model)
            {
                item.SentBy = UserManager.FindByIdAsync(item.AdminId).Result.UserName;
            }
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult CreateMessage()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task <ActionResult> CreateMessage([Bind(Include = "Id,AdminId,SentBy,Text,SentDate")] Message msg)
        {
            if (ModelState.IsValid)
            {
                msg.AdminId = User.Identity.GetUserId();
                msg.SentBy = UserManager.FindByIdAsync(msg.AdminId).Result.UserName;
                msg.SentDate = DateTime.Now;
                dbmsg.Messages.Add(msg);
                await dbmsg.SaveChangesAsync();
                return RedirectToAction("Messages");
            }
            return View();
        }
        /*---------------------------------------------------------------------------------*/
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> MessageDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Message Msg = await dbmsg.Messages.FindAsync(id);
            if (Msg == null)
            {
                return HttpNotFound();
            }
            Msg.SentBy = UserManager.FindByIdAsync(Msg.AdminId).Result.UserName;
            return View(Msg);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditMessage(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Message Msg = await dbmsg.Messages.FindAsync(id);
            if (Msg == null)
            {
                return HttpNotFound();
            }
            Msg.SentBy = UserManager.FindByIdAsync(Msg.AdminId).Result.UserName;
            return View(Msg);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditMessage([Bind(Include = "Id,AdminId,Text,SentBy,SentDate")] Message Msg)
        {
            if (ModelState.IsValid)
            {
                dbmsg.Entry(Msg).State = EntityState.Modified;
                await dbmsg.SaveChangesAsync();
                return RedirectToAction("Messages");
            }
            Msg.SentBy = UserManager.FindByIdAsync(Msg.AdminId).Result.UserName;
            return View(Msg);
        }

        // GET: Entries/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteMessage(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Message Msg = await dbmsg.Messages.FindAsync(id);
            if (Msg == null)
            {
                return HttpNotFound();
            }
            Msg.SentBy = UserManager.FindByIdAsync(Msg.AdminId).Result.UserName;
            return View(Msg);
        }

        // POST: Entries/Delete/5
        [HttpPost, ActionName("DeleteMessage")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Message Msg = await dbmsg.Messages.FindAsync(id);
            dbmsg.Messages.Remove(Msg);
            await dbmsg.SaveChangesAsync();
            return RedirectToAction("Messages");
        }
    }
}