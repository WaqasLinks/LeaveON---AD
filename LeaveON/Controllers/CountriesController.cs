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
    // GET: CountryNames/Edit/5
    public async Task<ActionResult> Edit(string id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      CountryName countryName = await db.CountryNames.FindAsync(id);
      if (countryName == null)
      {
        return HttpNotFound();
      }
      return View(countryName);
    }

    // POST: CountryNames/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit([Bind(Include = "Id,Name")] CountryName countryName)
    {
      if (ModelState.IsValid)
      {
        db.Entry(countryName).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return RedirectToAction("Index");
      }
      return View(countryName);
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
