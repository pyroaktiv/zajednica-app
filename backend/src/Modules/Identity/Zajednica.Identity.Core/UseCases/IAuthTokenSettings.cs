namespace Zajednica.Identity.Core.UseCases;


public interface IAuthTokenSettings
{
    int RefreshTokenDays { get; }
    
    int EmailVerificationTokenHours { get; }
    
    string EmailActivationUrl { get; }
}
