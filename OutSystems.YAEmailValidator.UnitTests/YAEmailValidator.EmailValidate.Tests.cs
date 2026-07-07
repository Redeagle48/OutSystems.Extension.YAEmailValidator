using NUnit.Framework;
using OutSystems.YAEmailValidator;

namespace OutSystems.YAEmailValidator.Tests
{
    /// <summary>
    /// Unit tests for <see cref="YAEmailValidator.EmailValidate"/>.
    /// All tests exercise the wrapper (not the underlying EmailValidation library directly).
    ///
    /// TEST INDEX
    /// ──────────────────────────────────────────────────────────────────
    ///  #  Method                                                    Cases
    /// ──────────────────────────────────────────────────────────────────
    ///  1. Validate_ValidEmails_ReturnsTrue                             8
    ///  2. Validate_InvalidEmails_ReturnsFalse                         10
    ///  3. Validate_NullEmptyOrWhitespace_ReturnsFalse                  3
    ///  4. Validate_LeadingTrailingWhitespace_RejectedWhenNotAllowed    1
    ///  5. Validate_LeadingTrailingWhitespace_TrimmedWhenAllowed        1
    ///  6. Validate_InternationalEmails_AllowedWhenFlagSet              2
    ///  7. Validate_InternationalEmails_RejectedWhenFlagUnset           2
    ///  8. Validate_TopLevelDomain_DependsOnFlag                        1
    ///  9. Validate_ShouldRejectDisplayNamesAndComments                 2
    /// 10. Validate_LocalPartBoundary                                   1
    /// 11. Validate_TotalLengthBoundary                                 1
    /// 12. Validate_ShouldRejectInvalidDotPlacement                     3
    /// ──────────────────────────────────────────────────────────────────
    ///                                                        Total:   35
    ///
    /// COVERAGE BY FEATURE
    /// ──────────────────────────────────────────────────────────────────
    ///  Feature / Flag                       Tests
    /// ──────────────────────────────────────────────────────────────────
    ///  Basic valid/invalid emails            #1, #2
    ///  Null/empty/whitespace input           #3
    ///  allowLeadingTrailingWhitespace flag   #4, #5
    ///  allowInternational flag (RFC 6531)    #6, #7
    ///  allowTopLevelDomains flag             #8
    ///  RFC 5321 compliance                   #9 (display names), #10 (local part 64-char),
    ///                                        #11 (total 254-char), #12 (dot placement)
    /// ──────────────────────────────────────────────────────────────────
    /// </summary>
    [TestFixture]
    public class YAEmailValidatorTests
    {
        // Invokes the extension wrapper under test and returns the out result.
        private static bool Validate(
            string? email,
            bool allowLeadingTrailingWhitespace = true,
            bool allowInternational = false,
            bool allowTopLevelDomains = false)
        {
            var sut = new YAEmailValidator();
            sut.EmailValidate(
                email!,
                allowLeadingTrailingWhitespace,
                allowInternational,
                allowTopLevelDomains,
                out bool isValidEmail);
            return isValidEmail;
        }

        // Tests for valid email addresses
        [TestCase("test@example.com")]
        [TestCase("firstname.lastname@domain.com")]
        [TestCase("email@subdomain.domain.com")]
        [TestCase("1234567890@domain.com")]
        [TestCase("email@domain-one.com")]
        [TestCase("_______@domain.com")]
        [TestCase("email@domain.name")]
        [TestCase("email@domain.co.jp")]
        public void Validate_ValidEmails_ReturnsTrue(string email)
        {
            Assert.That(Validate(email), Is.True, $"Expected '{email}' to be valid.");
        }

        // --- Invalid emails ---

        [TestCase("plainaddress")]               // No @ or domain
        [TestCase("#@%^%#$@#$@#.com")]           // Garbage characters
        [TestCase("@domain.com")]                // Missing username
        [TestCase("Joe Smith <email@domain.com>")] // Contains display name
        [TestCase("email.domain.com")]           // Missing @
        [TestCase("email@domain@domain.com")]    // Two @ symbols
        [TestCase(".email@domain.com")]          // Leading dot
        [TestCase("email.@domain.com")]          // Trailing dot in username
        [TestCase("email..email@domain.com")]    // Double dots
        [TestCase("email@domain..com")]          // Double dots in domain
        public void Validate_InvalidEmails_ReturnsFalse(string email)
        {
            Assert.That(Validate(email), Is.False, $"Expected '{email}' to be invalid.");
        }

        // Wrapper contract: null, empty and whitespace-only inputs are invalid, never throw.
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Validate_NullEmptyOrWhitespace_ReturnsFalse(string? email)
        {
            Assert.That(Validate(email), Is.False);
        }

        // Wrapper contract: allowLeadingTrailingWhitespace flag behavior.
        [Test]
        public void Validate_LeadingTrailingWhitespace_RejectedWhenNotAllowed()
        {
            Assert.That(
                Validate("  test@example.com  ", allowLeadingTrailingWhitespace: false),
                Is.False,
                "Whitespace-padded email must be rejected when trimming is disallowed.");
        }

        [Test]
        public void Validate_LeadingTrailingWhitespace_TrimmedWhenAllowed()
        {
            Assert.That(
                Validate("  test@example.com  ", allowLeadingTrailingWhitespace: true),
                Is.True,
                "Whitespace-padded email must be trimmed and accepted when trimming is allowed.");
        }

        // INTERNATIONALIZATION (RFC 6531)
        [TestCase("tést@domain.com")]
        [TestCase("用户@例子.广告")]
        public void Validate_InternationalEmails_AllowedWhenFlagSet(string email)
        {
            Assert.That(Validate(email, allowInternational: true), Is.True);
        }

        [TestCase("tést@domain.com")]
        [TestCase("用户@例子.广告")]
        public void Validate_InternationalEmails_RejectedWhenFlagUnset(string email)
        {
            Assert.That(Validate(email, allowInternational: false), Is.False);
        }

        // TOP-LEVEL DOMAINS: bare-TLD domains only accepted when the flag is set.
        [Test]
        public void Validate_TopLevelDomain_DependsOnFlag()
        {
            Assert.That(Validate("user@com", allowTopLevelDomains: false), Is.False,
                "Bare TLD domain must be rejected when the flag is unset.");
            Assert.That(Validate("user@com", allowTopLevelDomains: true), Is.True,
                "Bare TLD domain must be accepted when the flag is set.");
        }

        // THE "DISPLAY NAME" TRAP — EmailValidation correctly fails these.
        [TestCase("Jeffrey Stedfast <jestedfa@microsoft.com>")]
        [TestCase("jestedfa@microsoft.com (Jeffrey Stedfast)")]
        public void Validate_ShouldRejectDisplayNamesAndComments(string email)
        {
            Assert.That(Validate(email), Is.False,
                "RFC 5321 Address literals should not include display names or comments.");
        }

        // LOCAL PART LENGTH (Exactly 64 chars is allowed, 65 is not)
        [Test]
        public void Validate_LocalPartBoundary()
        {
            string sixtyFourChars = new string('a', 64);
            string sixtyFiveChars = new string('a', 65);

            Assert.That(Validate($"{sixtyFourChars}@domain.com"), Is.True, "64 chars should pass.");
            Assert.That(Validate($"{sixtyFiveChars}@domain.com"), Is.False, "65 chars must fail.");
        }

        // TOTAL LENGTH (Maximum 254 characters)
        [Test]
        public void Validate_TotalLengthBoundary()
        {
            // A valid domain must have labels no longer than 63 characters
            string label63 = new string('b', 63);
            string domain = $"{label63}.{label63}.{label63}.com"; // 63*3 + 3 dots + 3 'com' = 195 chars

            // 254 - 1 (@) - 195 (domain) = 58
            string local = new string('a', 58);
            string valid254 = $"{local}@{domain}";

            using (Assert.EnterMultipleScope())
            {
                Assert.That(valid254.Length, Is.EqualTo(254), "Manual check that string is 254");
                Assert.That(Validate(valid254), Is.True, "254 chars with valid labels should pass.");

                string invalid255 = "a" + valid254;
                Assert.That(Validate(invalid255), Is.False, "255 chars must fail.");
            }
        }

        // THE "DOUBLE DOT" AND PURE SYNTAX
        [TestCase("user..name@domain.com")] // Consecutive dots
        [TestCase(".user@domain.com")]     // Leading dot
        [TestCase("user.@domain.com")]     // Trailing dot in local part
        public void Validate_ShouldRejectInvalidDotPlacement(string email)
        {
            Assert.That(Validate(email), Is.False);
        }
    }
}
