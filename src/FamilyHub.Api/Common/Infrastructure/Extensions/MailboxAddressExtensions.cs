using FamilyHub.Api.Common.Email;
using MimeKit;

namespace FamilyHub.Api.Common.Infrastructure.Extensions;

public static class MailboxAddressExtensions
{
    extension(MailboxAddress)
    {
        public static MailboxAddress FromConfig(EmailConfiguration configuration)
            => new(configuration.FromName, configuration.FromAddress);
    }
}
