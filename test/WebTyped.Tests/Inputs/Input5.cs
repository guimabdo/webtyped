using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace Cblx.AspNetCore.Identity
{
    [Route("cblx-identity")]
    [ApiController]
    public class CblxIdentityController<TUser, TSignupModel> : Controller
        where TUser : IdentityUser, new()
        where TSignupModel : CblxSignupModel
    {

        readonly CblxIdentityManager<TUser, TSignupModel> manager;
        public CblxIdentityController(
            ICblxIdentityManager manager
        )
        {
            this.manager = manager as CblxIdentityManager<TUser, TSignupModel>;
        }

        [HttpPost("sign-up")]
        public async Task<CblxSignupResultModel> Signup([FromBody]TSignupModel model)
        {
            await manager.Signup(model, false);
            return new CblxSignupResultModel
            {
                Succeded = true,
                Message = "Thanks for signing up! We have sent you an email confirmation. Check your inbox and confirm your email."
            };

        }

        [HttpPost("password/reset")]
        public async Task ResetPassword([FromBody]string email)
        {
            await manager.StartResetPassword(email);
        }
    }

    public class CblxSignupModel
    {
        [Required, EmailAddress, MaxLength(256)]
        public virtual string Email { get; set; }

        [Required, MaxLength(256)]
        [Display(Name = "Name")]
        public virtual string FirstName { get; set; }

        [Required, MaxLength(256)]
        [Display(Name = "Family name")]
        public virtual string FamilyName { get; set; }

        //public string UserName { get; set; }
        [Required, MaxLength(32)]
        [DataType(DataType.Password)]
        public virtual string Password { get; set; }

        [Required, MaxLength(32)]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Display(Name = "Confirm password")]
        public virtual string ConfirmPassword { get; set; }
    }

    public class CblxSignupResultModel
    {
        public bool Succeded { get; set; }
        public string Message { get; set; }
    }
}
