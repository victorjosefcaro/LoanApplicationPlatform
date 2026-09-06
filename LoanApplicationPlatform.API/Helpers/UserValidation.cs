using System.Text.RegularExpressions;

namespace LoanApplicationPlatform.API.Helpers
{
    /// <summary>
    /// Shared username/password rules so account creation (AuthService) and
    /// admin-driven password resets (UserService) validate identically.
    /// </summary>
    public static class UserValidation
    {
        public static (bool IsValid, string? ErrorMessage) ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            {
                return (false, "Username must be at least 3 characters long.");
            }

            if (username.Length > 50)
            {
                return (false, "Username cannot exceed 50 characters.");
            }

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9]+$"))
            {
                return (false, "Username can only contain alphanumeric characters (letters and numbers).");
            }

            return (true, null);
        }

        public static (bool IsValid, string? ErrorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return (false, "Password is required.");
            }

            var missingRequirements = new List<string>();

            if (password.Length < 8)
            {
                missingRequirements.Add("be at least 8 characters long");
            }

            if (!password.Any(char.IsUpper))
            {
                missingRequirements.Add("contain at least one uppercase letter");
            }

            if (!password.Any(char.IsLower))
            {
                missingRequirements.Add("contain at least one lowercase letter");
            }

            if (!password.Any(char.IsDigit))
            {
                missingRequirements.Add("contain at least one number");
            }

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                missingRequirements.Add("contain at least one special character");
            }

            if (missingRequirements.Count > 0)
            {
                return (false, $"Password must {string.Join(", ", missingRequirements)}.");
            }

            return (true, null);
        }
    }
}
