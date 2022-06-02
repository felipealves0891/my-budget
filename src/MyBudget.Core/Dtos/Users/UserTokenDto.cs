namespace MyBudget.Core.Dtos.Users
{
    public class UserTokenDto
    {
        public string Token { get; set; }

        public DateTime Expiration { get; set; }
    }
}
