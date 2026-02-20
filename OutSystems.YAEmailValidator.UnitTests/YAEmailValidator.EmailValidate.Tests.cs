using NUnit.Framework;

namespace OutSystems.YAEmailValidator.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="YAEmailValidator.EmailValidate"/>.
    /// All tests exercise the wrapper (not the underlying EmailValidation library directly).
    ///
    /// TEST INDEX
    /// ──────────────────────────────────────────────────────────────────
    ///  #  Method                                                 Cases  Line
    /// ──────────────────────────────────────────────────────────────────
    ///  1. Validate_ValidEmails_ReturnsTrue                          8    30
    ///  2. Validate_InvalidEmails_ReturnsFalse                      10    45
    ///  3. Validate_EmptyOrWhitespace_ReturnsFalse                   2    62
    ///  4. Validate_NullEmail_ThrowsArgumentNullException             1    69
    ///  5. Validate_LeadingTrailingWhitespace_WhenNotAllowed          3    80
    ///  6. Validate_LeadingTrailingWhitespace_WhenAllowed             3    89
    ///  7. Validate_InternationalEmails_WhenAllowed_ReturnsTrue       2    99
    ///  8. Validate_InternationalEmails_WhenNotAllowed_ReturnsFalse   1   108
    ///  9. Validate_TopLevelDomain_WhenAllowed_ReturnsTrue            1   116
    /// 10. Validate_TopLevelDomain_WhenNotAllowed_ReturnsFalse        1   123
    /// 11. Validate_ShouldRejectDisplayNamesAndComments               2   131
    /// 12. Validate_LocalPartBoundary                                 2   142
    /// 13. Validate_TotalLengthBoundary                               3   157
    /// 14. Validate_ShouldRejectInvalidDotPlacement                   3   179
    /// ──────────────────────────────────────────────────────────────────
    ///                                                     Total:   39
    ///
    /// COVERAGE BY FEATURE
    /// ──────────────────────────────────────────────────────────────────
    ///  Feature / Flag                       Tests
    /// ──────────────────────────────────────────────────────────────────
    ///  Basic valid/invalid emails            #1, #2
    ///  Empty/whitespace/null input           #3, #4
    ///  allowLeadingTrailingWhitespace flag   #5, #6
    ///  allowInternational flag (RFC 6531)    #7, #8
    ///  allowTopLevelDomains flag             #9, #10
    ///  RFC 5321 compliance                   #11 (display names), #12 (local part 64-char),
    ///                                        #13 (total 254-char), #14 (dot placement)
    /// ──────────────────────────────────────────────────────────────────
    /// </summary>
    [TestFixture]
    public class YAEmailValidatorTests
    {
        private readonly YAEmailValidator _validator = new();

        private bool Validate(string email,
            bool allowWhitespace = false,
            bool allowInternational = false,
            bool allowTopLevelDomains = false)
        {
            _validator.EmailValidate(email, allowWhitespace, allowInternational, allowTopLevelDomains, out bool result);
            return result;
        }

        // --- Valid emails ---

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

        // --- Empty and whitespace ---

        [TestCase("")]
        [TestCase("   ")]
        public void Validate_EmptyOrWhitespace_ReturnsFalse(string email)
        {
            Assert.That(Validate(email), Is.False);
        }

        // --- Null handling ---

        [Test]
        public void Validate_NullEmail_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _validator.EmailValidate(null!, false, false, false, out _));
        }

        // --- Whitespace flag ---

        [TestCase(" test@example.com")]
        [TestCase("test@example.com ")]
        [TestCase("  test@example.com  ")]
        public void Validate_LeadingTrailingWhitespace_WhenNotAllowed_ReturnsFalse(string email)
        {
            Assert.That(Validate(email, allowWhitespace: false), Is.False,
                $"Expected '{email}' to be invalid when whitespace is not allowed.");
        }

        [TestCase(" test@example.com")]
        [TestCase("test@example.com ")]
        [TestCase("  test@example.com  ")]
        public void Validate_LeadingTrailingWhitespace_WhenAllowed_ReturnsTrue(string email)
        {
            Assert.That(Validate(email, allowWhitespace: true), Is.True,
                $"Expected '{email}' to be valid when whitespace is allowed (trimmed before validation).");
        }

        // --- International support (RFC 6531) ---

        [TestCase("t\u00e9st@domain.com")]
        [TestCase("\u7528\u6237@\u4f8b\u5b50.\u5e7f\u544a")]
        public void Validate_InternationalEmails_WhenAllowed_ReturnsTrue(string email)
        {
            Assert.That(Validate(email, allowInternational: true), Is.True,
                $"Expected international email '{email}' to be valid.");
        }

        [TestCase("t\u00e9st@domain.com")]
        public void Validate_InternationalEmails_WhenNotAllowed_ReturnsFalse(string email)
        {
            Assert.That(Validate(email, allowInternational: false), Is.False,
                $"Expected international email '{email}' to be invalid when international is not allowed.");
        }

        // --- Top-level domain flag ---

        [Test]
        public void Validate_TopLevelDomain_WhenAllowed_ReturnsTrue()
        {
            Assert.That(Validate("user@localhost", allowTopLevelDomains: true), Is.True,
                "Expected 'user@localhost' to be valid when TLDs are allowed.");
        }

        [Test]
        public void Validate_TopLevelDomain_WhenNotAllowed_ReturnsFalse()
        {
            Assert.That(Validate("user@localhost", allowTopLevelDomains: false), Is.False,
                "Expected 'user@localhost' to be invalid when TLDs are not allowed.");
        }

        // --- Display name rejection (RFC 5321) ---

        [TestCase("Jeffrey Stedfast <jestedfa@microsoft.com>")]
        [TestCase("jestedfa@microsoft.com (Jeffrey Stedfast)")]
        public void Validate_ShouldRejectDisplayNamesAndComments(string email)
        {
            Assert.That(Validate(email), Is.False,
                "RFC 5321 Address literals should not include display names or comments.");
        }

        // --- Local part length boundary (64 chars max) ---

        [Test]
        public void Validate_LocalPartBoundary()
        {
            string sixtyFourChars = new string('a', 64);
            string sixtyFiveChars = new string('a', 65);

            Assert.Multiple(() =>
            {
                Assert.That(Validate($"{sixtyFourChars}@domain.com"), Is.True, "64 chars should pass.");
                Assert.That(Validate($"{sixtyFiveChars}@domain.com"), Is.False, "65 chars must fail.");
            });
        }

        // --- Total length boundary (254 chars max) ---

        [Test]
        public void Validate_TotalLengthBoundary()
        {
            // A valid domain must have labels no longer than 63 characters
            string label63 = new string('b', 63);
            string domain = $"{label63}.{label63}.{label63}.com"; // 63*3 + 3 dots + 3 'com' = 195 chars

            // 254 - 1 (@) - 195 (domain) = 58
            string local = new string('a', 58);
            string valid254 = $"{local}@{domain}";

            Assert.Multiple(() =>
            {
                Assert.That(valid254.Length, Is.EqualTo(254), "Manual check that string is 254");
                Assert.That(Validate(valid254), Is.True, "254 chars with valid labels should pass.");

                string invalid255 = "a" + valid254;
                Assert.That(Validate(invalid255), Is.False, "255 chars must fail.");
            });
        }

        // --- Invalid dot placement ---

        [TestCase("user..name@domain.com")] // Consecutive dots
        [TestCase(".user@domain.com")]     // Leading dot
        [TestCase("user.@domain.com")]     // Trailing dot in local part
        public void Validate_ShouldRejectInvalidDotPlacement(string email)
        {
            Assert.That(Validate(email), Is.False);
        }
    }
}
