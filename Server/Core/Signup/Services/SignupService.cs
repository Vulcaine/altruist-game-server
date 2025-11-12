// using Altruist;

// using Microsoft.Extensions.Options;

// using Server.Email;
// using Server.Persistence;

// namespace Server.Signup;

// [Service]
// public class SignupService
// {
//     private readonly IVault<Account> _accountVault;
//     private readonly IEmailService _emailService;
//     private readonly AppUrls _urls;

//     public SignupService(IVault<Account> accountVault, IEmailService emailService, IOptions<AppUrls> urls)
//     {
//         _accountVault = accountVault;
//         _emailService = emailService;
//         _urls = urls.Value;
//     }

//     public async Task<SignupResult> Signup(string email, string username, string password)
//     {

//     }


// }



