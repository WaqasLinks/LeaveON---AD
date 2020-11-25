using System;
using System.Collections.Generic;
using System.Linq;
using LeaveON.UtilityClasses;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(LeaveON.Startup))]

namespace LeaveON
{
  public partial class Startup
  {
    public void Configuration(IAppBuilder app)
    {
      ConfigureAuth(app);
      new ScheduledTasks().InitTimerForScheduleTasks();
      //reset.LeavePolicyValues();
    }

  }
}
