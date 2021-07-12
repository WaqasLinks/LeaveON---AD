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
  
  [Authorize(Roles = "Admin")]
  public class DepartmentsController : Controller
  {
    private LeaveONEntities db = new LeaveONEntities();

    // GET: Departments
    public async Task<ActionResult> Index()
    {
      //var departments = db.Departments.Include(d => d.Country);
      return View(await db.DepartmentNames.ToListAsync());
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
