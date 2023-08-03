using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finops.Helpers
{
    public class EmailBody
    {
        public static string EmailStringBody(string email, string emailToken)
        {
            return $@"<html>
            <head>
            </head>
            <body style=""margin:0;padding:0;font-family: Arial,Helvetica,sans-serif;"">
              <div style=""height: auto;background: linear-gradient(to top, #c9cff 50%,#6e6ef6 90%) no-repeat;width:400px;padding:30px"">
               <div>
                <div>
                <h2>Reset your Password</h2>
                <hr>
                <p>You're receiving this email because you have requested a password reset for your FinOps account.</p>

                <p>Please click on the link below to choose a new password.</p>
                <a href=""http://localhost:4200/reset?email={email}&code={emailToken}"" target=""_blank"" style=""background:#0d6efd;padding:10px;
                   border:none;color:white;border-radius:4px;display:block;margin:0 auto;width:50%;text-align:center;text-decoration:none"">ResetPassword</a><br>
                
                <p>Kind Regards,<br><br>
                FinOps Team</p>
                </div>
               </div>
              </div>
            </body>
            </html>";
        }
    }
}