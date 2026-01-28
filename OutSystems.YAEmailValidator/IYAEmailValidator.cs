using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.YAEmailValidator
{
    /// <summary>
    /// Validates the specified email address (using RFC 5321) with optional flags for trimming and international/TLD support. Uses the lib EmailValidation 1.3.0 (https://github.com/jstedfast/EmailValidation).
    /// </summary>
    [OSInterface(
        Description = "Validates the specified email address (using RFC 5321) with optional flags for trimming and international/TLD support. Uses the lib EmailValidation 1.3.0 (https://github.com/jstedfast/EmailValidation).",
        IconResourceName = "OutSystems.YAEmailValidator.resources.YAEmailValidator_icon.png"
    )]
    public interface IYAEmailValidator
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
        /// <param name="allowTopLevelDomains">If true, top-level domains are allowed (prevents local-only addresses like "user@localhost").
        /// True when the email is valid; otherwise false.</returns>
        [OSAction(
            Description = "Validates the specified email address (using the RFC 5321) with optional flags for trimming and international/TLD support.",
            IconResourceName = "OutSystems.YAEmailValidator.resources.YAEmailValidator_icon.png"
        )]
        void EmailValidate(
            [OSParameterAttribute(Description = "The email to validate.")]
            string emailToValidate,
            [OSParameterAttribute(Description = "When true, leading/trailing whitespace will be ignored (the email is trimmed before validation). When false, the method returns false if the provided email contains any leading or trailing whitespace.")]
            bool allowLeadingTrailingWhitespace,
            [OSParameterAttribute(Description = "If true, non-ASCII international addresses are allowed.")]
            bool allowInternational,
            [OSParameterAttribute(Description = "If true, top-level domains are allowed (prevents local-only addresses like 'user@localhost').Returns True when the email is valid; otherwise false.")]
            bool allowTopLevelDomains,
            [OSParameterAttribute(Description = "True, if email is valid according to RFC 5321. Otherwise, false.")]
            out bool isValidEmail
        );
    }
}