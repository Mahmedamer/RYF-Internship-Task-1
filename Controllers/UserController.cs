using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NYFInter;
using Microsoft.AspNet.Identity;
using NYFInter.Models;
using Microsoft.AspNet.Identity.Owin;

namespace NYFInter.Controllers
{
    public class UserController : Controller
    {
        private DbEntries dbEntries = new DbEntries();
        private DbMessages dbMessages = new DbMessages();
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
        // GET: Entries
        [Authorize(Roles = "User")]
        public async Task<ActionResult> Index()
        {
            var model = await dbMessages.Messages.ToListAsync();
            foreach (var item in model)
            {
                item.SentBy = UserManager.FindByIdAsync(item.AdminId).Result.UserName;
            }
            return View(model);
        }

        public async Task<ActionResult> MyEntries()
        {
            var UserId = User.Identity.GetUserId();
            return View(await dbEntries.Entries.Where(x => x.UserId == UserId).ToListAsync());
        }
        // GET: Entries/Details/5
        [Authorize(Roles = "User")]
        public async Task<ActionResult> EntryDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Entry entry = await dbEntries.Entries.FindAsync(id);
            if (entry == null)
            {
                return HttpNotFound();
            }
            return View(entry);
        }

        // GET: Entries/Create
        [Authorize(Roles = "User")]
        public ActionResult CreateEntry()
        {
            return View();
        }

        // POST: Entries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateEntry([Bind(Include = "Id,UserId,Text,Status")] Entry entry)
        {
            if (ModelState.IsValid)
            {
                entry.UserId = User.Identity.GetUserId();
                dbEntries.Entries.Add(entry);
                await dbEntries.SaveChangesAsync();
                return RedirectToAction("MyEntries");
            }
            return View(entry);
        }

        // GET: Entries/Edit/5
        [Authorize(Roles = "User")]
        public async Task<ActionResult> EditEntry(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Entry entry = await dbEntries.Entries.FindAsync(id);
            if (entry == null)
            {
                return HttpNotFound();
            }
            return View(entry);
        }

        // POST: Entries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditEntry([Bind(Include = "Id,UserId,Text,Status")] Entry entry)
        {
            if (ModelState.IsValid)
            {
                dbEntries.Entry(entry).State = EntityState.Modified;
                await dbEntries.SaveChangesAsync();
                return RedirectToAction("MyEntries");
            }
            return View(entry);
        }

        // GET: Entries/Delete/5
        [Authorize(Roles = "User")]
        public async Task<ActionResult> DeleteEntry(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Entry entry = await dbEntries.Entries.FindAsync(id);
            if (entry == null)
            {
                return HttpNotFound();
            }
            return View(entry);
        }

        // POST: Entries/Delete/5
        [HttpPost, ActionName("DeleteEntry")]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Entry entry = await dbEntries.Entries.FindAsync(id);
            dbEntries.Entries.Remove(entry);
            await dbEntries.SaveChangesAsync();
            return RedirectToAction("MyEntries");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dbEntries.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
