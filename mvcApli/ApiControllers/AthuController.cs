using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using mvcApli.Models;
using mvcApli.ViewModel;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace mvcApli.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AthuController : ControllerBase
    {
        private RoleManager<IdentityRole> roleManager;
        private UserManager<IdentityUser> userManager;
        private SignInManager<IdentityUser> signInManager;
        private IConfiguration configuration;
        private AppDbContext _context;
        public AthuController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration, AppDbContext context)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this._context = context;
        }

        [Route("list")]
        public IActionResult Index()
        {
            var users = userManager.Users;
            return Ok(users);
        }
        [Route("Create")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] RegisterViewModel model)
        {
            if (model == null)

                throw new NullReferenceException("Model is null");


            if (model.Password != model.PasswordConfirm)

                return Ok(new UserManagerResponse
                {
                    Message = "Password doesn't match",
                    IsSuccess = false,
                });

             var check= await userManager.FindByEmailAsync(model.Email); 
            if (check != null)
            {
                return Ok(new UserManagerResponse
                {
                    Message = "Email Already exist",
                    IsSuccess = false,
                });
            }

            var user = new IdentityUser
            {
                UserName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Customer");

                return Ok(new UserManagerResponse
                {
                    Message = "User created successfully.",
                    IsSuccess = true,
                });
            }
            return Ok(new UserManagerResponse
            {
                Message = "User is not Created",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description)
            });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromForm] LoginViewModel model)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return Ok(new UserManagerResponse { Message = "Wrong Creadentials", IsSuccess = false });
                }
                var result = await signInManager.PasswordSignInAsync(user, model.Password, false, false);
                if (result.Succeeded)
                {

                    var claims = new[] {
                new Claim("Email" ,model.Email),
                new Claim("Name" ,user.UserName),
                new Claim(ClaimTypes.NameIdentifier,user.Id)
                };
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AuthSetting:Key"]));
                    var token = new JwtSecurityToken
                    (
                        issuer: configuration["AuthSetting:Audience"],
                        audience: configuration["AuthSetting:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddDays(30),
                        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                    );
                    string tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);
                    var rolename = "Customer";
                    if (await userManager.IsInRoleAsync(user, "Admin"))
                        rolename = "Admin";


                    return Ok(new UserManagerResponse
                    {
                        Message = tokenAsString,
                        IsSuccess = true,
                        RoleName = rolename,
                        ExpireDate = DateTime.Today.AddDays(20)
                    });
                }
                return Ok(new UserManagerResponse { Message = "Wrong Password", IsSuccess = false });
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [Authorize]
        [HttpGet("GetUser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var contextUser = HttpContext.User;
            IdentityUser _currentUser = await userManager.GetUserAsync(contextUser);
            return Ok(_currentUser);
        }
        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {

            await signInManager.SignOutAsync();
            return Ok(true);

        }

    }
}
