﻿using TimeAPI.API.Models.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Services;
using Microsoft.AspNetCore.Cors;
using TimeAPI.API.Filters;
using TimeAPI.Domain;
using System.Threading;
using TimeAPI.Domain.Entities;
using System.Globalization;
using System.Web;
using Microsoft.AspNetCore.DataProtection;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;


        public AccountController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IEmailSender emailSender,
            ILogger<AccountController> logger, IOptions<ApplicationSettings> AppSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody]LoginViewModel model)
        {
            //ViewData["ReturnUrl"] = returnUrl;
            //if (ModelState.IsValid)
            //{
            var user = await _userManager.FindByNameAsync(model.Email).ConfigureAwait(true);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password).ConfigureAwait(true))
            {
                var role = await _userManager.GetRolesAsync(user).ConfigureAwait(true);
                IdentityOptions options = new IdentityOptions();
                var tokenDescriptor = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(new Claim[] {

                        new Claim("UserID", user.Id.ToString()),
                        new Claim(options.ClaimsIdentity.RoleClaimType, role.FirstOrDefault())
                    }),
                    Expires = DateTime.Now.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                return Ok(new { token, user.Id });
            }
            else
                return Ok(new SuccessViewModel { Code = "201", Status = "Error", Desc = "Please enter a valid user and password." });
            //}
            //return BadRequest(new { message = "OOP! Please enter a valid user and password." });
        }

        [HttpPost]
        [Route("SignUp")]
        public async Task<object> SignUp([FromBody]RegisterViewModel UserModel)
        {
            //if (ModelState.IsValid)
            //{

            string _userName = "";
            if (UserModel.Email != null || !string.IsNullOrEmpty(UserModel.Email)
                || !string.IsNullOrWhiteSpace(UserModel.Email) || UserModel.Email != "")
            {
                _userName = UserModel.Email;
            }
            else if (UserModel.Phone != null || !string.IsNullOrEmpty(UserModel.Phone)
                || !string.IsNullOrWhiteSpace(UserModel.Phone) || (UserModel.Phone) != "")
            {
                if (UserModel.Phone.Contains("+"))
                    _userName = UserModel.Phone.Substring(1);
                else
                    _userName = UserModel.Phone;
            }

            var user = new ApplicationUser()
            {
                UserName = _userName,
                Email = UserModel.Email,
                FirstName = UserModel.FirstName,
                LastName = UserModel.LastName,
                FullName = UserModel.FullName,
                Role = "Superadmin",
                PhoneNumber = UserModel.Phone,
                isSuperAdmin = true
            };

            var result = await _userManager.CreateAsync(user, UserModel.Password).ConfigureAwait(true);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, user.Role).ConfigureAwait(true);
                _logger.LogInformation("User created a new account with password.");


                #region Employee

                var employee = new Employee()
                {
                    id = Guid.NewGuid().ToString(),
                    user_id = user.Id,
                    full_name = user.FullName,
                    first_name = user.FirstName,
                    last_name = user.LastName,
                    mobile = user.PhoneNumber,
                    workemail = user.Email,
                    createdby = user.FullName,
                    created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                    is_admin = false,
                    is_superadmin = true
                };

                _unitOfWork.EmployeeRepository.Add(employee);
                _unitOfWork.Commit();

                #endregion

                if (user.Email != "")
                {
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(true);
                    var code = await _userManager.GenerateUserTokenAsync(user, "Default", "Confirmation").ConfigureAwait(true);
                    //code = EncodeServerName(code); // HttpUtility.UrlEncode(code, );
                    code = HttpUtility.UrlPathEncode(code); 
                    var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    await _emailSender.SendEmailConfirmationAsync(UserModel.Email, callbackUrl).ConfigureAwait(true);
                }
                else
                {
                    // check if its a phone 
                }

                return Ok(new SuccessViewModel { Code = "200", Status = "Success", Desc = "User created a new account with password." });
            }
            else
            {
                AddErrors(result);
                string _Code = "", _Description = "";
                if (result.Errors != null)
                {
                    foreach (var error in result.Errors)
                    {
                        _Code = error.Code;
                        _Description = error.Description;
                    }
                }

                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = _Code, Desc = _Description });
            }
            //}
            //return BadRequest(new { message = "OOP! Please enter a valid user details." });
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<object> Logout()
        {
            await _signInManager.SignOutAsync().ConfigureAwait(true);
            _logger.LogInformation("User logged out.");
            return Ok(new SuccessViewModel { Code = "200", Status = "Success" });
        }

        [HttpGet]
        [Route("ConfirmEmail")]
        public async Task<object> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return Ok(new SuccessViewModel { Code = "201", Status = "Error" });
            }
            var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(true);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }

            #region
            //var result = await _userManager.ConfirmEmailAsync(user, code).ConfigureAwait(true);

            //string _Status = "", _Code = "", _Description = "";
            //if (result.Errors != null)
            //    foreach (var error in result.Errors)
            //    {
            //        _Status = "201";
            //        _Code = error.Code;
            //        _Description = error.Description;
            //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = _Code, Desc = _Description });
            //    }

            //if (result.Succeeded)
            //    _Status = "200"; _Code = "Success"; _Description = "Email Confirmed";

            //return Task.FromResult<object>(new SuccessViewModel { Status = _Status, Code = _Code, Desc = _Description });
            #endregion


            #region


            //code = HttpUtility.HtmlDecode(code); //HttpUtility.UrlDecode(code);
            //code = DecodeServerName(code);
            code = HttpUtility.UrlDecode(code);
            var result = await _userManager.VerifyUserTokenAsync(user, "Default", "Confirmation", code).ConfigureAwait(true);  //await _userManager.VerifyUserTokenAsync(user, code).ConfigureAwait(true);
            if (result)
            {
                await _userManager.UpdateSecurityStampAsync(user).ConfigureAwait(true);
                _unitOfWork.UserRepository.CustomEmailConfirmedFlagUpdate(user.Id);
                _unitOfWork.Commit();

                return Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Email Confirmed" });
            }
            else { return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = "Error", Desc = "Email Not Confirmed" }); }

            #endregion
        }

        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<object> ForgotPassword([FromBody]ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Ok(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(true);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user).ConfigureAwait(true)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return Ok(new SuccessViewModel { Code = "201", Status = "User not exists." });
            }

            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            var code = await _userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(true);
            var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
            await _emailSender.SendEmailAsync(model.Email, "Reset Password", $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>").ConfigureAwait(true);
            return Ok(new SuccessViewModel { Code = "200", Status = "Success", Desc = "Reset password email sent to registerd email." });
            //// If we got this far, something failed, redisplay form
            //return Ok(model);
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody]ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Ok(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email).ConfigureAwait(true);
            if (user == null)
            {
                return Ok(new SuccessViewModel { Code = "201", Status = "Error", Desc = "Please enter a valid email" });
            }
            //model.Code = System.Web.HttpUtility.UrlDecode(model.Code);
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password).ConfigureAwait(true);

            if (result.Succeeded)
            {
                return Ok(new SuccessViewModel { Code = "200", Status = "Success", Desc = "Password set successful." });
            }
            AddErrors(result);
            string _Code = "", _Description = "";
            if (result.Errors != null)
            {
                foreach (var error in result.Errors)
                {
                    _Code = error.Code;
                    _Description = error.Description;
                }
            }
            return Ok(new SuccessViewModel { Status = "201", Code = _Code, Desc = _Description });
        }

        #region Helpers
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }


        public static string EncodeServerName(string serverName)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(serverName));
        }

        public static string DecodeServerName(string encodedServername)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encodedServername));
        }
        #endregion
    }
}






