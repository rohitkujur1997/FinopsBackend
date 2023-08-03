using Finops.Models;

namespace Finops.Repository
{
    public interface IEmailService
    {
        void SendEmail(EmailModel emailModel);
    }
}
