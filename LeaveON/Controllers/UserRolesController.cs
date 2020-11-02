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
using LeaveON.Models;

namespace LeaveON.Controllers
{
  public class UserRolesController : Controller
  {
    private LeaveONEntities db = new LeaveONEntities();

    // GET: AspNetUserClaims
    //public async Task<ActionResult> Index()
    public ActionResult Index()
    {
      ViewBag.Employees = new SelectList(db.AspNetUsers.OrderBy(x=>x.UserName), "Id", "UserName");
      //ViewBag.LeaveTypes = new SelectList(db.LeaveTypes, "Id", "Name");
      ViewBag.Roles = new SelectList(db.AspNetRoles.OrderBy(x=>x.Name), "Id", "Name");
      //var aspNetUserClaims = db.AspNetUserClaims.Include(a => a.AspNetUser);

      List<UserRoleModel> usersAndRoles = new List<UserRoleModel>(); // Adding this model just to have it in a nice list.
      //var users = db.AspNetUsers;

      foreach (AspNetUser user in db.AspNetUsers)
      {
        foreach (AspNetRole role in user.AspNetRoles)
        {
          usersAndRoles.Add(new UserRoleModel
                                {
                                  UserId=user.Id,
                                  UserName = user.UserName,
                                  RoleId=role.Id,
                                  RoleName = role.Name
                                });
        }
      }
      //var userRoles= usersAndRoles.AsQueryable<UserRoleModel>();
      //return View(await userRoles.ToListAsync().ConfigureAwait(false));
      return View(usersAndRoles);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Index([Bind(Include = "UserId,UserName,RoleId,RoleName")] UserRoleModel userRoleModel)
    {
      if (ModelState.IsValid)
      {
        //userRoleModel.ClaimType = db.AspNetUsers.FirstOrDefault(x => x.Id.ToString() == userRoleModel.UserId).Name;
        //db.AspNetUserClaims.Add(userRoleModel);

        AspNetUser user = db.AspNetUsers.FirstOrDefault(x => x.Id.ToString() == userRoleModel.UserId);
        user.AspNetRoles.Add(new AspNetRole
                                  {
                                    Id = userRoleModel.RoleId,
                                    Name = userRoleModel.UserName
                                  });

        //AspNetRole role=

        //UserManager.AddToRoleAsync(user.Id, "Admin");


        switch (userRoleModel.RoleName)
        {
          case "Admin":

            //await UserManager.AddToRoleAsync(user.Id, "Admin");
            //await UserManager.AddToRoleAsync(user.Id, "Manager");
            //await UserManager.AddToRoleAsync(user.Id, "User");
            
            user.AspNetRoles.Add(new AspNetRole
                                  {
                                    Id = "1",//userRoleModel.RoleId,
                                    Name = userRoleModel.UserName
                                  });

            user.AspNetRoles.Add(new AspNetRole
                                  {
                                    Id = "2", //userRoleModel.RoleId,
                                    Name = userRoleModel.UserName
                                  });
            user.AspNetRoles.Add(new AspNetRole
                                  {
                                    Id = "3", //userRoleModel.RoleId,
                                    Name = userRoleModel.UserName
                                  });

            break;
            
          case "Manager":

            user.AspNetRoles.Add(new AspNetRole
                                  {
                                    Id = "2", //userRoleModel.RoleId,
                                    Name = userRoleModel.UserName
                                  });
            user.AspNetRoles.Add(new AspNetRole
                                  {
                                    Id = "3", //userRoleModel.RoleId,
                                    Name = userRoleModel.UserName
                                  });

            break;

          case "User":

            user.AspNetRoles.Add(new AspNetRole
                                  {
                                    Id = "3", //userRoleModel.RoleId,
                                    Name = userRoleModel.UserName
                                  });

            break;
        }
        
        await db.SaveChangesAsync();
        return RedirectToAction("Index");
      }

      ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Hometown", userRoleModel.UserId);
      return View(userRoleModel);
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
    // POST: AspNetUserClaims/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> DeleteConfirmed(int id)
    {
      AspNetUserClaim aspNetUserClaim = await db.AspNetUserClaims.FindAsync(id);
      db.AspNetUserClaims.Remove(aspNetUserClaim);
      await db.SaveChangesAsync();
      return RedirectToAction("Index");
    }
    // GET: AspNetUserClaims/Create
    public ActionResult Create()
    {
      ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Hometown");
      return View();
    }

    // POST: AspNetUserClaims/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create([Bind(Include = "Id,UserId,ClaimType,ClaimValue")] AspNetUserClaim aspNetUserClaim)
    {
      if (ModelState.IsValid)
      {
        db.AspNetUserClaims.Add(aspNetUserClaim);
        await db.SaveChangesAsync();
        return RedirectToAction("Index");
      }

      ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Hometown", aspNetUserClaim.UserId);
      return View(aspNetUserClaim);
    }

    // GET: AspNetUserClaims/Edit/5
    public async Task<ActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      AspNetUserClaim aspNetUserClaim = await db.AspNetUserClaims.FindAsync(id);
      if (aspNetUserClaim == null)
      {
        return HttpNotFound();
      }
      ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Hometown", aspNetUserClaim.UserId);
      return View(aspNetUserClaim);
    }

    // POST: AspNetUserClaims/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit([Bind(Include = "Id,UserId,ClaimType,ClaimValue")] AspNetUserClaim aspNetUserClaim)
    {
      if (ModelState.IsValid)
      {
        db.Entry(aspNetUserClaim).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return RedirectToAction("Index");
      }
      ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Hometown", aspNetUserClaim.UserId);
      return View(aspNetUserClaim);
    }

    // GET: AspNetUserClaims/Delete/5
    public async Task<ActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      AspNetUserClaim aspNetUserClaim = await db.AspNetUserClaims.FindAsync(id);
      if (aspNetUserClaim == null)
      {
        return HttpNotFound();
      }
      return View(aspNetUserClaim);
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
