using EmailValidation;
using System;

namespace OutSystems.YAEmailValidator
{
    /// <summary>
    /// Provides email validation utilities that wrap the EmailValidation library.
    /// </summary>
    public class YAEmailValidator : IYAEmailValidator
    {
        /// <summary>
        /// Validates the specified email address with optional flags for trimming and international/TLD support.
        /// </summary>
        /// <param name="emailToValidate">The email to validate.</param>
        /// <param name="allowLeadingTrailingWhitespace">
        /// When true, leading/trailing whitespace will be ignored (the email is trimmed before validation).
        /// When false, the method returns false if the provided email contains any leading or trailing whitespace.
        /// </param>
        /// <param name="allowInternational">If true, non-ASCII international addresses are allowed.</param>
        /// <param name="allowTopLevelDomains">When true, addresses whose domain is a bare top-level domain (e.g. "user@com" or "user@localhost") are accepted.</param>
        /// <param name="isValidEmail">Set to true when the email is valid according to RFC 5321; otherwise false.</param>
        public void EmailValidate(
            string emailToValidate,
            bool allowLeadingTrailingWhitespace,
            bool allowInternational,
            bool allowTopLevelDomains,
            out bool isValidEmail)
        {
            if (string.IsNullOrEmpty(emailToValidate))
            {
                isValidEmail = false;
                return;
            }

            // If trimming is not allowed and the provided email contains leading/trailing whitespace,
            // treat it as invalid (return false).
            if (!allowLeadingTrailingWhitespace)
            {
                if (!string.Equals(emailToValidate, emailToValidate.Trim(), StringComparison.Ordinal))
                {
                    isValidEmail = false;
                    return;
                }

                // No trimming allowed, validate the exact provided value.
                isValidEmail = EmailValidator.Validate(emailToValidate,
                    allowInternational: allowInternational,
                    allowTopLevelDomains: allowTopLevelDomains);
                return;
            }

            // Trimming allowed: remove leading/trailing whitespace and validate the trimmed value.
            var email = emailToValidate.Trim();
            isValidEmail =  EmailValidator.Validate(email,
                allowInternational: allowInternational,
                allowTopLevelDomains: allowTopLevelDomains);
            return;
        }
    }
}