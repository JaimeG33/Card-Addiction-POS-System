using System.Threading.Tasks;

namespace Card_Addiction_POS_System.Security
{
    /// <summary>
    /// Simple in-memory password provider. Call SetPassword once (at login),
    /// obtain the password via GetPasswordAsync, and Clear when done.
    /// </summary>
    public sealed class PasswordProvider
    {
        private string? _password;

        public void SetPassword(string? password) => _password = password;

        public void Clear() => _password = null;

        public Task<string> GetPasswordAsync() => Task.FromResult(_password ?? string.Empty);
    }
}
