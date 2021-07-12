using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Repository.Models;

namespace LeaveON.Controllers
{

  [Authorize(Roles = "Admin,Manager")]
  public class AspNetUsersController : Controller
  {
    private LeaveONEntities db = new LeaveONEntities();

    // GET: AspNetUsers
    public async Task<ActionResult> Index()
    {
      var aspNetUsers = db.AspNetUsers;//.Include(a => a.Department);
      return View(await aspNetUsers.ToListAsync());
    }

    // GET: AspNetUsers/Details/5
    public async Task<ActionResult> Details(string id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      AspNetUser aspNetUser = await db.AspNetUsers.FindAsync(id);
      if (aspNetUser == null)
      {
        return HttpNotFound();
      }
      return View(aspNetUser);
    }

    // GET: AspNetUsers/Create
    public ActionResult Create()
    {
      ViewBag.DepartmentId = new SelectList(db.DepartmentNames, "Id", "Name");
      return View();
    }

    // POST: AspNetUsers/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create([Bind(Include = "Id,Hometown,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,DateCreated,DateModified,Remarks,DepartmentId")] AspNetUser aspNetUser)
    {
      if (ModelState.IsValid)
      {
        db.AspNetUsers.Add(aspNetUser);
        await db.SaveChangesAsync();
        return RedirectToAction("Index");
      }

      //ViewBag.DepartmentId = new SelectList(db.DepartmentNames, "Id", "Name", aspNetUser.DepartmentId);
      return View(aspNetUser);
    }

    // GET: AspNetUsers/Edit/5
    public async Task<ActionResult> Edit(string id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      AspNetUser aspNetUser = await db.AspNetUsers.FindAsync(id);
      if (aspNetUser == null)
      {
        return HttpNotFound();
      }
      //ViewBag.CountryId = new SelectList(db.CountryNames, "Id", "Name", aspNetUser.CountryId);
      //ViewBag.DepartmentId = new SelectList(db.DepartmentNames, "Id", "Name", aspNetUser.DepartmentId);
      ViewBag.UserLeavePolicyId = new SelectList(db.UserLeavePolicies, "Id", "Description", aspNetUser.UserLeavePolicyId);
      return View(aspNetUser);
    }

    // POST: AspNetUsers/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit([Bind(Include = "Id,Hometown,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,DateCreated,DateModified,Remarks,DepartmentId,CountryId,UserLeavePolicyId,BioStarEmpNum")] AspNetUser aspNetUser)//,int CountryId)
    {
      aspNetUser.DateModified = DateTime.Now;

      if (ModelState.IsValid)
      {
        //db.Entry(aspNetUser).State = EntityState.Modified;
        db.AspNetUsers.Attach(aspNetUser);

        db.Entry(aspNetUser).Property(x => x.Email).IsModified = true;
        db.Entry(aspNetUser).Property(x => x.UserName).IsModified = true;
        db.Entry(aspNetUser).Property(x => x.DateModified).IsModified = true;
        db.Entry(aspNetUser).Property(x => x.Remarks).IsModified = true;
        //db.Entry(aspNetUser).Property(x => x.DepartmentId).IsModified = true;
        //db.Entry(aspNetUser).Property(x => x.CountryId).IsModified = true;
        db.Entry(aspNetUser).Property(x => x.UserLeavePolicyId).IsModified = true;
        db.Entry(aspNetUser).Property(x => x.BioStarEmpNum).IsModified = true;
        //UserLeavePolicyId


        await db.SaveChangesAsync();
        return RedirectToAction("Index");
      }
      //ViewBag.DepartmentId = new SelectList(db.DepartmentNames, "Id", "Name", aspNetUser.DepartmentId);
      return View(aspNetUser);
    }

    // GET: AspNetUsers/Delete/5
    public async Task<ActionResult> Delete(string id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      AspNetUser aspNetUser = await db.AspNetUsers.FindAsync(id);
      if (aspNetUser == null)
      {
        return HttpNotFound();
      }
      return View(aspNetUser);
    }

    // POST: AspNetUsers/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> DeleteConfirmed(string id)
    {
      AspNetUser aspNetUser = await db.AspNetUsers.FindAsync(id);
      db.AspNetUsers.Remove(aspNetUser);
      await db.SaveChangesAsync();
      return RedirectToAction("Index");
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        db.Dispose();
      }
      base.Dispose(disposing);
    }
  }
}
