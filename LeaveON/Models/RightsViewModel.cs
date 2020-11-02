using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveON.Models
{
  public class RightsViewModel
  {
    public IQueryable<Department> departments { get; set; }
    public IQueryable<AspNetUser> Users { get; set; }
    public IQueryable<AspNetUserClaim> Claims { get; set; }
  }
}
