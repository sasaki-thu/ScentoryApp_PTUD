using System;
using System.Text.RegularExpressions;

namespace ScentoryApp.Utilities
{
    /// <summary>
    /// Utility class for validating password complexity requirements
    /// </summary>
    public static class PasswordValidator
    {
        /// <summary>
        /// Validates that a password contains at least one uppercase letter,
        /// one lowercase letter, and one special character.
        /// </summary>
        /// <param name="password">The password to validate</param>
        /// <returns>True if password meets all requirements, false otherwise</returns>
        public static bool IsPasswordValid(string password)
        {
            // Null or empty check
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Check for at least one uppercase letter (A-Z)
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // Check for at least one lowercase letter (a-z)
            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;

            // Check for at least one special character (!@#$%^&*()_+-=[]{};\:"|,.<>/?`~)
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};:\\'""|,.<>/?`~]"))
                return false;

            return true;
        }

        /// <summary>
        /// Validates password and returns detailed error messages in Vietnamese
        /// </summary>
        /// <param name="password">The password to validate</param>
        /// <returns>A list of error messages, empty if password is valid</returns>
        public static List<string> ValidatePasswordWithErrors(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("Mật khẩu không được để trống.");
                return errors;
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
                errors.Add("Mật khẩu phải chứa ít nhất một chữ cái viết hoa (A-Z).");

            if (!Regex.IsMatch(password, @"[a-z]"))
                errors.Add("Mật khẩu phải chứa ít nhất một chữ cái viết thường (a-z).");

            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};:\\'""|,.<>/?`~]"))
                errors.Add("Mật khẩu phải chứa ít nhất một ký tự đặc biệt (!@#$%^&*()_+-=[]{}\\;:\"|,.<>/?`~).");

            return errors;
        }

        /// <summary>
        /// Gets a user-friendly password requirement message in Vietnamese
        /// </summary>
        /// <returns>Description of password requirements</returns>
        public static string GetPasswordRequirements()
        {
            return "Mật khẩu phải chứa ít nhất một chữ cái viết hoa, một chữ cái viết thường và một ký tự đặc biệt.";
        }
    }
}
