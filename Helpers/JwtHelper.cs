using System.Security.Cryptography;

namespace coreWebApi.Helpers
{
    public class JwtHelper
    {
        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}