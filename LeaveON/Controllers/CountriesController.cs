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
  public class CountriesController : Controller
  {
    private LeaveONEntities db = new LeaveONEntities();

    // GET: Countries
    public async Task<ActionResult> Index()
    {
      return View(await db.CountryNames.ToListAsync());
    }

    
    // GET: Countries/Create
    public ActionResult Create()
    {
      return View();
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
