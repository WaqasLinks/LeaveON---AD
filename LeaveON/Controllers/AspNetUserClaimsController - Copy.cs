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
  public class AspNetUserClaimsController : Controller
  {
    private LeaveONEntities db = new LeaveONEntities();

    // GET: AspNetUserClaims
    public async Task<ActionResult> Index()
    {
      ViewBag.Employees = new SelectList(db.AspNetUsers.OrderBy(x=>x.UserName), "Id", "UserName");
      //ViewBag.LeaveTypes = new SelectList(db.LeaveTypes, "Id", "Name");
      ViewBag.Departments = new SelectList(db.Departments.OrderBy(x=>x.Name), "Id", "Name");
      var aspNetUserClaims = db.AspNetUserClaims.Include(a => a.AspNetUser);
      return View(await aspNetUserClaims.ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Index([Bind(Include = "Id,UserId,ClaimType,ClaimValue")] AspNetUserClaim aspNetUserClaim)
    {
      if (ModelState.IsValid)
      {
        aspNetUserClaim.ClaimType = db.Departments.FirstOrDefault(x => x.Id.ToString() == aspNetUserClaim.ClaimValue).Name;
        db.AspNetUserClaims.Add(aspNetUserClaim);
        await db.SaveChangesAsync();
        return RedirectToAction("Index");
      }

      ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Hometown", aspNetUserClaim.UserId);
      return View(aspNetUserClaim);
    }
    //DeleteRight
    [HttpPost]
    [ValidateAntiForgeryToken]
    //public async Task<ActionResult> DeleteRight([Bind(Include = "Id,UserId,ClaimType,ClaimValue")] AspNetUserClaim aspNetUserClaim)
    public async Task<ActionResult> DeleteClaim(int ClaimId)
    {
      AspNetUserClaim aspNetUserClaim = await db.AspNetUserClaims.FindAsync(ClaimId);
      db.AspNetUserClaims.Remove(aspNetUserClaim);
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
