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
using System.Security.Cryptography;
using TimeAPI.API.Extensions;


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
        private string _userName = "";

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
                if (!await _userManager.IsEmailConfirmedAsync(user).ConfigureAwait(false))
                {
                    return Ok(new SuccessViewModel { Code = "201", Status = "Error", Desc = "Please verify your user." });
                }

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
            //check if user has input email or phone as user
            GetUserName(UserModel);

            var user = GetUserProperty(UserModel, _userName);
            var result = await _userManager.CreateAsync(user, UserModel.Password).ConfigureAwait(true);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, user.Role).ConfigureAwait(true);
                _logger.LogInformation("User created a new account with password.");

                #region Employee

                var employee = GetEmployeeProperty(user);
                _unitOfWork.EmployeeRepository.Add(employee);

                #endregion

                if (_unitOfWork.Commit())
                    await UserVerificationCode(UserModel, user).ConfigureAwait(true);

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
            
            var xcode = Base64UrlEncoder.Decode(code);
            IdentityResult identityResult = await _userManager.ConfirmEmailAsync(user, xcode).ConfigureAwait(true);

            if (identityResult.Succeeded)
            {
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
            var xcode = Base64UrlEncoder.Encode(code);
            var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, xcode, Request.Scheme);
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
            var xcode = Base64UrlEncoder.Decode(model.Code);
            var result = await _userManager.ResetPasswordAsync(user, xcode, model.Password).ConfigureAwait(true);

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


        private static string EncodeServerName(string serverName)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(serverName));
        }

        private static string DecodeServerName(string encodedServername)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encodedServername));
        }

        private void GetUserName(RegisterViewModel UserModel)
        {
            if (UserModel.Email != null || !string.IsNullOrEmpty(UserModel.Email) || !string.IsNullOrWhiteSpace(UserModel.Email) || UserModel.Email != "")
            {
                _userName = UserModel.Email;
            }
            else if (UserModel.Phone != null || !string.IsNullOrEmpty(UserModel.Phone) || !string.IsNullOrWhiteSpace(UserModel.Phone) || (UserModel.Phone) != "")
            {
                _userName = UserModel.Phone.Contains("+") ? UserModel.Phone.Substring(1) : UserModel.Phone;
            }
        }

        private static Employee GetEmployeeProperty(ApplicationUser user)
        {
            return new Employee()
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
        }

        private async Task UserVerificationCode(RegisterViewModel UserModel, ApplicationUser user)
        {
            if (user.Email != "")
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(true);
                var xcode = Base64UrlEncoder.Encode(code); 
                var callbackUrl = Url.EmailConfirmationLink(user.Id, xcode, Request.Scheme);
                await _emailSender.SendEmailConfirmationAsync(UserModel.Email, callbackUrl).ConfigureAwait(true);
            }
            else
            {
                // check if its a phone 
            }
        }

        private static ApplicationUser GetUserProperty(RegisterViewModel UserModel, string _userName)
        {
            return new ApplicationUser()
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
        }

        #endregion
    }
}






