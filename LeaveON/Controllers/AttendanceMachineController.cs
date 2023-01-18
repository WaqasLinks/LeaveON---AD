using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using System.Net.Http;
using Newtonsoft.Json;
using RestSharp;

namespace LeaveON.Controllers
{
  [Authorize(Roles = "Admin,Manager,User")]
  public class AttendanceMachineController : Controller
  {
    //https://localhost:44380/AttendanceMachine/AttendaceUserData
    // GET: AttendanceMachine
    public ActionResult AttendaceUserData()
    {
      string UserId = "admin";
      string password = "@Intech#963";
      string token =""; ;
     string baseAddress = "https://10.1.10.28:81/api/login";

      var client = new RestClient("https://10.1.10.28:81");
      //client.Timeout = -1;
      var request = new RestRequest("api/login", Method.Post);
      request.AddHeader("Content-Type", "application/json");
      request.AddHeader("Cookie", "JSESSIONID=90A913454E961B333231999BAF0E4903");
      var body = @"{
" + "\n" +
      @"  ""User"": {
" + "\n" +
      @"    ""login_id"": ""admin"",
" + "\n" +
      @"    ""password"": ""@Intech#123""
" + "\n" +
      @"  }
" + "\n" +
      @"}";
      request.AddParameter("application/json", body, ParameterType.RequestBody);
      RestResponse response = client.Execute(request);

      //var client = new HttpClient("https://10.1.10.28:81/api/login");
      //var request = new HttpRequest"create", FormMethod.Post);
      //request.AddHeader("Accept", "application/json");
      //request.RequestFormat = DataFormat.Json;
      //request.AddJsonBody(new { login_id = "admin", password = "@Intech#963" });
      //var response = client.ExecutePostAsync(request);



      //string[] scopes = new string[] { "user.read" };
      //string accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(scopes);

      //// Use the access token to call a protected web API.
      //HttpClient httpClient = new HttpClient();
      //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

      //var response = await httpClient.GetAsync($"{webOptions.GraphApiUrl}/beta/me");

      //if (response.StatusCode == HttpStatusCode.OK)
      //{
      //  var content = await response.Content.ReadAsStringAsync();

      //  dynamic me = JsonConvert.DeserializeObject(content);
      //  ViewData["Me"] = me;
      //}


      //  try
      //  {
      //  HttpClient client = new HttpClient();
      //  var url = new Uri(baseAddress);
      //  MultipartFormDataContent form = new MultipartFormDataContent();
      //  Dictionary<string, string> parameters = new Dictionary<string, string>();
      //  parameters.Add("login_id", UserId);
      //  parameters.Add("password", password);
      //  HttpContent DictionaryItems = new FormUrlEncodedContent(parameters);
      //  form.Add(DictionaryItems, "User");


      //   var response = client.PostAsync(url.ToString(), form, System.Threading.CancellationToken.None);
      //  //var response = client.PostAsync(url.ToString(), DictionaryItems);

      //  if (response.Result.StatusCode == System.Net.HttpStatusCode.OK)
      //  {
      //    //Get body
      //    var bodyRes = response.Result.Content.ReadAsStringAsync().Result;
      //    var tokenAccess = JsonConvert.DeserializeObject(bodyRes);
      //    //Get Header
      //    //   var headers = response.Result.Headers.GetValues("appToken");
      //  }
      //  //  else
      //  //  {
      //  //    var a = response.Result.Content.ReadAsStringAsync().Result;
      //  //  }

      //}
      //  catch (Exception ex)
      //  {

      //  }



      return View();
    }
  }
 
}
