using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaveON.Controllers
{
  [Authorize(Roles = "Admin,Manager,User")]
  public class DashboardController : Controller
  {
    // GET: Dashboard
    private LeaveONEntities db = new LeaveONEntities();
    public ActionResult Index()
    {

      // db.Leaves.Sum(x=>x..LeaveTypeId!=0)
      return View();

    }
  }
}
