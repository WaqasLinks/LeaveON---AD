using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using LeaveON.Models;
using Repository.Models;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Web.Hosting;

namespace LeaveON.Controllers
{
  [Authorize]
  public class AccountController : Controller
  {
    private ApplicationSignInManager _signInManager;
    private ApplicationUserManager _userManager;
    private LeaveONEntities db = new LeaveONEntities();
    public AccountController()
    {
    }

    public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
    {
      UserManager = userManager;
      SignInManager = signInManager;
    }

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

    public ActionResult Index()
    {
      ViewBag.Employees = new SelectList(db.AspNetUsers.OrderBy(x => x.UserName), "Id", "UserName");
      //ViewBag.LeaveTypes = new SelectList(db.LeaveTypes, "Id", "Name");
      ViewBag.Roles = new SelectList(db.AspNetRoles.OrderBy(x => x.Name), "Id", "Name");
      //var aspNetUserClaims = db.AspNetUserClaims.Include(a => a.AspNetUser);

      List<UserRoleModel> usersAndRoles = new List<UserRoleModel>(); // Adding this model just to have it in a nice list.
      //var users = db.AspNetUsers;
      List<AspNetUser> AspNetUsers = db.AspNetUsers.ToList<AspNetUser>();
      foreach (AspNetUser user in AspNetUsers)//db.AspNetUsers)
      {
        foreach (AspNetRole role in user.AspNetRoles)
        {
          usersAndRoles.Add(new UserRoleModel
          {
            UserId = user.Id,
            UserName = user.UserName,
            RoleId = role.Id,
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

        userRoleModel.RoleName = db.AspNetRoles.FirstOrDefault(x => x.Id == userRoleModel.RoleId).Name;
        switch (userRoleModel.RoleName)
        {
          case "Admin":
            await UserManager.AddToRoleAsync(userRoleModel.UserId, "Admin");
            await UserManager.AddToRoleAsync(userRoleModel.UserId, "Manager");
            await UserManager.AddToRoleAsync(userRoleModel.UserId, "User");
            break;
          case "Manager":
            await UserManager.AddToRoleAsync(userRoleModel.UserId, "Manager");
            await UserManager.AddToRoleAsync(userRoleModel.UserId, "User");
            break;
          case "User":
            await UserManager.AddToRoleAsync(userRoleModel.UserId, "User");
            break;
        }

      }

      //ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Hometown", userRoleModel.UserId);
      //return View(userRoleModel);
      return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    //public async Task<ActionResult> DeleteRight([Bind(Include = "Id,UserId,ClaimType,ClaimValue")] AspNetUserClaim aspNetUserClaim)
    public async Task<ActionResult> DeleteRole(string UserIdRoleId)
    {
      string UserId = UserIdRoleId.Split(',').First();
      string RoleId = UserIdRoleId.Split(',').Last();

      string RoleName = db.AspNetRoles.FirstOrDefault(x => x.Id == RoleId).Name;
      switch (RoleName)
      {
        case "Admin":
          await UserManager.RemoveFromRoleAsync(UserId, "Admin");
          await UserManager.RemoveFromRoleAsync(UserId, "Manager");
          await UserManager.RemoveFromRoleAsync(UserId, "User");
          break;
        case "Manager":
          await UserManager.RemoveFromRoleAsync(UserId, "Manager");
          await UserManager.RemoveFromRoleAsync(UserId, "User");
          break;
        case "User":
          await UserManager.RemoveFromRoleAsync(UserId, "User");
          break;
      }

      //AspNetUserClaim aspNetUserClaim = await db.AspNetUserClaims.FindAsync(UserIdRoleId);
      //db.AspNetUserClaims.Remove(aspNetUserClaim);
      //await db.SaveChangesAsync();
      return RedirectToAction("Index");

    }



    // The Authorize Action is the end point which gets called when you access any
    // protected Web API. If the user is not logged in then they will be redirected to 
    // the Login page. After a successful login you can call a Web API.
    [HttpGet]
    public ActionResult Authorize()
    {
      var claims = new ClaimsPrincipal(User).Claims.ToArray();
      var identity = new ClaimsIdentity(claims, "Bearer");
      AuthenticationManager.SignIn(identity);
      return new EmptyResult();
    }

    //
    // GET: /Account/Login
    [AllowAnonymous]
    public ActionResult Login(string returnUrl, string ADUser)
    {
      //ADUser = "bsserviceaccount@intechww.com";
      //ADUser = "Ahsan.Ahmad@intechww.com";
      //ADUser = "muzammil.riaz@intechww.com";
      //ADUser = "asrar.ahmed@intechww.com";
      //var path = System.Web.HttpContext.Current.Server.MapPath(@"~/myLog.txt");
      //var identityName = HttpContext.User.Identity.Name;
      //using (HostingEnvironment.Impersonate())
      //{
      //  using (var context = new PrincipalContext(ContextType.Domain, "intechww.com", null, ContextOptions.Negotiate | ContextOptions.SecureSocketLayer))
      //  using (var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, @"NT AUTHORITY\IUSR"))
      //  {
      //    var emailAddress = userPrincipal.EmailAddress;
      //    var lastname = userPrincipal.Surname;
      //    var firstname = userPrincipal.GivenName;
      //  }
      //}
      //string ab= User.Identity.Name;
      //var identityName = HttpContext.User.Identity.Name;

      //System.IO.File.AppendAllLines(path, new List<string>(new string[] { identityName}));

      //UserPrincipal currentUser = UserPrincipal.Current;

      //string Name1 = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
      //System.IO.File.AppendAllLines(path, new List<string>(new string[] { Name1 }));

      //PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "intechww.com");
      //UserPrincipal user = UserPrincipal.FindByIdentity(ctx, HttpContext.Request.LogonUserIdentity.Name);
      //string assdf = HttpContext.User.Identity.Name;
      //System.IO.File.AppendAllLines(path, new List<string>(new string[] { user.Name.ToString(), assdf }));

      //List<string> loginsList = new List<string>();
      ////string id1 = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
      ////string id4 = Request.LogonUserIdentity.Name;
      ////PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
      ////UserPrincipal currentUser = UserPrincipal.FindByIdentity(ctx, User.Identity.Name);
      //loginsList.Add(currentUser.Name);
      //loginsList.Add(currentUser.DisplayName);
      //loginsList.Add(currentUser.GivenName);
      //loginsList.Add(currentUser.SamAccountName);
      //loginsList.Add(currentUser.UserPrincipalName);

      //System.IO.File.AppendAllLines(path, loginsList);
      ViewBag.ADUser = ADUser;//"bsserviceaccount@intechww.com";//ADUser;
      ViewBag.ReturnUrl = returnUrl;
      //ViewBag.ReturnUrl = @"\LeavesRequest\Index";
      AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie, DefaultAuthenticationTypes.ExternalCookie);
      Session.Abandon();
      return View();
    }

    //
    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    //public async Task<ActionResult> Login(LoginViewModel model, string returnUrl, FormCollection formCollection)
    public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
    {

      //var path = Server.MapPath(@"~/myLog.txt");
      //System.IO.File.AppendAllLines(path, loginsList);


      if (!ModelState.IsValid)
      {
        //Response.Cookies[".AspNet.ApplicationCookie"].Expires = DateTime.Now.AddDays(-1);
        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie, DefaultAuthenticationTypes.ExternalCookie);
        Session.Abandon();
        return View(model);

        //return RedirectToAction("Error404", "Error");
      }

      // This doesn't count login failures towards account lockout
      // To enable password failures to trigger account lockout, change to shouldLockout: true

      //string userName = formCollection.Get("UserName");//"Cash"; //cash//other
      //string IsAuthenticated = formCollection.Get("IsAuthenticated");

      //var result = await SignInManager.PasswordSignInAsync(model.Email, "Leaves12*", model.RememberMe, shouldLockout: false);
      var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
      //Mubashar.ali@intechww.com
      //var result = await SignInManager.PasswordSignInAsync("Mubashar.ali@intechww.com", "Leaves12*", model.RememberMe, shouldLockout: false);
      //var result = await SignInManager.PasswordSignInAsync("umer@tenf.loc", "Leaves12*", model.RememberMe, shouldLockout: false);


      //-----------------------
      switch (result)
      {
        case SignInStatus.Success:

          return RedirectToLocal(returnUrl);
        //return RedirectToAction("Index", "LeavesRequest");
        case SignInStatus.LockedOut:
          return View("Lockout");
        case SignInStatus.RequiresVerification:
          return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
        case SignInStatus.Failure:
        default:
          ModelState.AddModelError("", "Invalid login attempt.");
          //Response.Cookies[".AspNet.ApplicationCookie"].Expires = DateTime.Now.AddDays(-1);
          AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie, DefaultAuthenticationTypes.ExternalCookie);
          Session.Abandon();
          return View(model);
          //return RedirectToAction("Error404", "Error");
      }
    }

    //
    // GET: /Account/VerifyCode
    [AllowAnonymous]
    public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
    {
      // Require that the user has already logged in via username/password or external login
      if (!await SignInManager.HasBeenVerifiedAsync())
      {
        return View("Error");
      }
      return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
    }

    //
    // POST: /Account/VerifyCode
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      // The following code protects for brute force attacks against the two factor codes. 
      // If a user enters incorrect codes for a specified amount of time then the user account 
      // will be locked out for a specified amount of time. 
      // You can configure the account lockout settings in IdentityConfig
      var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
      switch (result)
      {
        case SignInStatus.Success:
          return RedirectToLocal(model.ReturnUrl);
        case SignInStatus.LockedOut:
          return View("Lockout");
        case SignInStatus.Failure:
        default:
          ModelState.AddModelError("", "Invalid code.");
          return View(model);
      }
    }

    //
    // GET: /Account/Register
    //[AllowAnonymous]
    [Authorize(Roles = "Admin,Manager")]
    public ActionResult Register()
    {
      ViewBag.Countries = db.CountryNames;
      //onchange country... department list is populating using ajax in view. but has little problem. so sending departements data from view. when done comment ViewBag.Departments = db.Departments;
      ViewBag.Departments = db.DepartmentNames;
      ViewBag.LeavePolicies = db.UserLeavePolicies;
      return View();
    }

    //
    // POST: /Account/Register
    [HttpPost]
    //[AllowAnonymous]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> Register(RegisterViewModel model)
    {
      if (ModelState.IsValid)
      {
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Hometown = model.Hometown, BioStarEmpNum = model.BioStarEmpNum, UserLeavePolicyId = model.UserLeavePolicyId };
        var result = await UserManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {

          switch (model.Role)
          {
            case "Admin":
              await UserManager.AddToRoleAsync(user.Id, "Admin");
              await UserManager.AddToRoleAsync(user.Id, "Manager");
              await UserManager.AddToRoleAsync(user.Id, "User");
              break;
            case "Manager":
              await UserManager.AddToRoleAsync(user.Id, "Manager");
              await UserManager.AddToRoleAsync(user.Id, "User");
              break;
            case "User":
              await UserManager.AddToRoleAsync(user.Id, "User");
              break;
          }



          //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

          // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
          // Send an email with this link
          // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
          // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
          // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

          return RedirectToAction("Index", "LeavesRequest");
        }
        AddErrors(result);
      }

      // If we got this far, something failed, redisplay form
      ViewBag.Departments = db.DepartmentNames;
      return View(model);
    }
    [HttpPost]
    //[AllowAnonymous]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> UpdateUser(UpdateUserViewModel model)
    {
      if (ModelState.IsValid)
      {
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Hometown = model.Hometown, BioStarEmpNum = model.BioStarEmpNum, UserLeavePolicyId = model.UserLeavePolicyId };
        //var result = await UserManager.CreateAsync(user, model.Password);
        var result = await UserManager.UpdateAsync(user);
        if (result.Succeeded)
        {
          await UserManager.RemoveFromRoleAsync(user.Id, "Admin");
          await UserManager.RemoveFromRoleAsync(user.Id, "Manager");
          await UserManager.RemoveFromRoleAsync(user.Id, "User");


          switch (model.Role)
          {
            case "Admin":
              await UserManager.AddToRoleAsync(user.Id, "Admin");
              await UserManager.AddToRoleAsync(user.Id, "Manager");
              await UserManager.AddToRoleAsync(user.Id, "User");
              break;
            case "Manager":
              await UserManager.AddToRoleAsync(user.Id, "Manager");
              await UserManager.AddToRoleAsync(user.Id, "User");
              break;
            case "User":
              await UserManager.AddToRoleAsync(user.Id, "User");
              break;
          }



          //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

          // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
          // Send an email with this link
          // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
          // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
          // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

          return RedirectToAction("Index", "LeavesRequest");
        }
        AddErrors(result);
      }

      // If we got this far, something failed, redisplay form
      ViewBag.Departments = db.DepartmentNames;
      return View(model);
    }

    [HttpPost]
    public ActionResult GetDepartmentByCountryId(int CountryId)
    {

      List<DepartmentName> Departments = db.DepartmentNames.Where(x => x.Id == CountryId).ToList<DepartmentName>(); //GetAllDepartment().Where(m => m.StateId == stateid).ToList();
      SelectList LstDepartments = new SelectList(Departments, "Id", "Name", 0);
      return Json(LstDepartments);
    }
    //
    // GET: /Account/ConfirmEmail
    [AllowAnonymous]
    public async Task<ActionResult> ConfirmEmail(string userId, string code)
    {
      if (userId == null || code == null)
      {
        return View("Error");
      }
      var result = await UserManager.ConfirmEmailAsync(userId, code);
      return View(result.Succeeded ? "ConfirmEmail" : "Error");
    }

    //
    // GET: /Account/ForgotPassword
    [AllowAnonymous]
    public ActionResult ForgotPassword()
    {
      return View();
    }

    //
    // POST: /Account/ForgotPassword
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
      if (ModelState.IsValid)
      {
        var user = await UserManager.FindByNameAsync(model.Email);
        if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
        {
          // Don't reveal that the user does not exist or is not confirmed
          return View("ForgotPasswordConfirmation");
        }

        // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
        // Send an email with this link
        // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
        // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
        // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
        // return RedirectToAction("ForgotPasswordConfirmation", "Account");
      }

      // If we got this far, something failed, redisplay form
      return View(model);
    }

    //
    // GET: /Account/ForgotPasswordConfirmation
    [AllowAnonymous]
    public ActionResult ForgotPasswordConfirmation()
    {
      return View();
    }

    //
    // GET: /Account/ResetPassword
    [AllowAnonymous]
    public ActionResult ResetPassword(string code)
    {
      return code == null ? View("Error") : View();
    }

    //
    // POST: /Account/ResetPassword
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }
      var user = await UserManager.FindByNameAsync(model.Email);
      if (user == null)
      {
        // Don't reveal that the user does not exist
        return RedirectToAction("ResetPasswordConfirmation", "Account");
      }
      var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
      if (result.Succeeded)
      {
        return RedirectToAction("ResetPasswordConfirmation", "Account");
      }
      AddErrors(result);
      return View();
    }

    //
    // GET: /Account/ResetPasswordConfirmation
    [AllowAnonymous]
    public ActionResult ResetPasswordConfirmation()
    {
      return View();
    }

    //
    // POST: /Account/ExternalLogin
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult ExternalLogin(string provider, string returnUrl)
    {
      // Request a redirect to the external login provider
      return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
    }

    //
    // GET: /Account/SendCode
    [AllowAnonymous]
    public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
    {
      var userId = await SignInManager.GetVerifiedUserIdAsync();
      if (userId == null)
      {
        return View("Error");
      }
      var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
      var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
      return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
    }

    //
    // POST: /Account/SendCode
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> SendCode(SendCodeViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View();
      }

      // Generate the token and send it
      if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
      {
        return View("Error");
      }
      return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
    }

    //
    // GET: /Account/ExternalLoginCallback
    [AllowAnonymous]
    public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
    {
      var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
      if (loginInfo == null)
      {
        //return RedirectToAction("Login");
        return RedirectToAction("Error404", "Error");
      }

      // Sign in the user with this external login provider if the user already has a login
      var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
      switch (result)
      {
        case SignInStatus.Success:
          return RedirectToLocal(returnUrl);
        case SignInStatus.LockedOut:
          return View("Lockout");
        case SignInStatus.RequiresVerification:
          return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
        case SignInStatus.Failure:
        default:
          // If the user does not have an account, then prompt the user to create an account
          ViewBag.ReturnUrl = returnUrl;
          ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
          return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
      }
    }

    //
    // POST: /Account/ExternalLoginConfirmation
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
    {
      if (User.Identity.IsAuthenticated)
      {
        return RedirectToAction("Index", "Manage");
      }

      if (ModelState.IsValid)
      {
        // Get the information about the user from the external login provider
        var info = await AuthenticationManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
          return View("ExternalLoginFailure");
        }
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Hometown = model.Hometown };
        var result = await UserManager.CreateAsync(user);
        if (result.Succeeded)
        {
          result = await UserManager.AddLoginAsync(user.Id, info.Login);
          if (result.Succeeded)
          {
            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            return RedirectToLocal(returnUrl);
          }
        }
        AddErrors(result);
      }

      ViewBag.ReturnUrl = returnUrl;
      return View(model);
    }

    //
    // POST: /Account/LogOff
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult LogOff()
    {
      AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
      return RedirectToAction("Index", "LeavesRequest");

    }
    public ActionResult SignOut()
    {
      //1
      AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
      //2
      //var AuthenticationManager = HttpContext.GetOwinContext().Authentication;
      //AuthenticationManager.SignOut();
      //3
      //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie, DefaultAuthenticationTypes.ExternalCookie);
      //Session.Abandon();
      //return RedirectToAction("Login", "Account");
      return RedirectToAction("Logout", "Account");
      //return Redirect("https://lms.intechww.com:1001/");
    }
    [AllowAnonymous]
    public ActionResult Logout()
    {
      return View();
    }
    [AllowAnonymous]
    public ActionResult LoginAgain()
    {
      return Redirect("https://lms.intechww.com:1001/");
    }
    //
    // GET: /Account/ExternalLoginFailure
    [AllowAnonymous]
    public ActionResult ExternalLoginFailure()
    {
      return View();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_userManager != null)
        {
          _userManager.Dispose();
          _userManager = null;
        }

        if (_signInManager != null)
        {
          _signInManager.Dispose();
          _signInManager = null;
        }
      }

      base.Dispose(disposing);
    }

    #region Helpers
    // Used for XSRF protection when adding external logins
    private const string XsrfKey = "XsrfId";

    private IAuthenticationManager AuthenticationManager
    {
      get
      {
        return HttpContext.GetOwinContext().Authentication;
      }
    }

    private void AddErrors(IdentityResult result)
    {
      foreach (var error in result.Errors)
      {
        ModelState.AddModelError("", error);
      }
    }

    private ActionResult RedirectToLocal(string returnUrl)
    {
      if (Url.IsLocalUrl(returnUrl))
      {
        return Redirect(returnUrl);
      }
      return RedirectToAction("Index", "LeavesRequest");
    }

    internal class ChallengeResult : HttpUnauthorizedResult
    {
      public ChallengeResult(string provider, string redirectUri)
          : this(provider, redirectUri, null)
      {
      }

      public ChallengeResult(string provider, string redirectUri, string userId)
      {
        LoginProvider = provider;
        RedirectUri = redirectUri;
        UserId = userId;
      }

      public string LoginProvider { get; set; }
      public string RedirectUri { get; set; }
      public string UserId { get; set; }

      public override void ExecuteResult(ControllerContext context)
      {
        var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
        if (UserId != null)
        {
          properties.Dictionary[XsrfKey] = UserId;
        }
        context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
      }
    }
    #endregion
  }
}
