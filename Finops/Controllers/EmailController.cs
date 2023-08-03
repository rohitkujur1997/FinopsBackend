using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Finops.Data;
using Finops.DTO;
using Finops.Helpers;
using Finops.Models;
using Finops.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceStack.Messaging;

namespace Finops.Controllers
{
    // [Route("[controller]")]
    // public class EmailController : Controller
    // {
    //     private readonly ILogger<EmailController> _logger;

    //     public EmailController(ILogger<EmailController> logger)
    //     {
    //         _logger = logger;
    //     }

    //     public IActionResult Index()
    //     {
    //         return View();
    //     }

    //     [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    //     public IActionResult Error()
    //     {
    //         return View("Error!");
    //     }
    // }

    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly FinopsDbContext _dbContext;
        private readonly IEmailRepository emailRepository;
        private readonly IConfiguration configuration;
        private readonly IEmailService emailService;
        public EmailController(FinopsDbContext dbContext, IEmailRepository emailRepository, IConfiguration configuration, IEmailService emailService)
        {
            _dbContext = dbContext;
            this.emailRepository = emailRepository;
            this.configuration = configuration;
            this.emailService = emailService;
        }

        [Route("api/[controller]")]
        [HttpPost]
        public IActionResult SendEmail(EmailDTO email)
        {
            try
            {
                emailRepository.SendEmail(email);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest("something went wrong while sending email" + e);
            }
        }

        [HttpPost("SendResetPasswordEmail/{email}")]
        public async Task<IActionResult> PasswordResetEmail(string email)
        {
            //var user = await _dbContext.Users.FirstOrDefaultAsync(a => a.Email == email);
            var user = await _dbContext.Login.FirstOrDefaultAsync(a => a.EmailId == email);
            if (user == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Email doesn't Exist"

                });
            }
            var tokenBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(64);
            var emailToken = Convert.ToBase64String(tokenBytes);
            user.ResetPasswordToken = emailToken;
            user.ResetPasswordExpiry = DateTime.Now.AddMinutes(15);
            string from = configuration["EmailSettings:EmailUsername"];
            var emailModel = new EmailModel(email, "Reset Password!!", EmailBody.EmailStringBody(email, emailToken));
            emailService.SendEmail(emailModel);
            _dbContext.Entry(user).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return Ok(new
            {
                StatusCode = 200,
                Message = "Email Sent!"
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            //var newToken = resetPasswordDto.EmailToken.Replace(" ", "+");
            //var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(a => a.Email == resetPasswordDto.Email);
            var user = await _dbContext.Login.AsNoTracking().FirstOrDefaultAsync(a => a.EmailId == resetPasswordDto.Email);
            if (user == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "user doesn't Exist"

                });
            }
            var tokenCode = user.ResetPasswordToken;
            DateTime emailTokenExpiry = user.ResetPasswordExpiry;
            if(tokenCode != resetPasswordDto.EmailToken || emailTokenExpiry < DateTime.Now)
            {
                return BadRequest("Invalid Reset link");
            }
            //user.Password = PasswordHasher.HashPassword(resetPasswordDto.NewPassword);
            user.Password = resetPasswordDto.NewPassword;
            _dbContext.Entry(user).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return Ok(new
            {
                StatusCode = 200,
                Message = "Password Reset Successfully"

            });
        }
    }    
}