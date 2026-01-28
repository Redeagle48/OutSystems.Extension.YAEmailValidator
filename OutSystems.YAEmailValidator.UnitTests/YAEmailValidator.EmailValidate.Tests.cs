using NUnit.Framework;
using EmailValidation;

namespace OutSystems.YAEmailValidalitor.Tests
{
    [TestFixture]
    public class EmailValidatorTests
    {
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
            bool result = EmailValidator.Validate(email);
            Assert.That(result, Is.True, $"Expected '{email}' to be valid.");
        }

        // Tests for invalid email addresses
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
            bool result = EmailValidator.Validate(email);
            Assert.That(result, Is.False, $"Expected '{email}' to be invalid.");
        }

        // Tests for null or whitespace
        [TestCase("")]
        [TestCase("   ")]
        public void Validate_EmptyOrNull_ReturnsFalse(string email)
        {
            bool result = EmailValidator.Validate(email);
            Assert.That(result, Is.False);
        }

        // To truly validate that your library is adhering to RFC 5321, you should use these specific test cases.
        // These are designed to catch "loose" validators that fail to enforce the technical limits of the SMTP protocol.

        // INTERNATIONALIZATION (RFC 6531) 
        [TestCase("tést@domain.com")]
        [TestCase("用户@例子.广告")]
        public void Validate_InternationalEmails(string email)
        {
            // Jeffrey's lib supports this if the overload is called correctly
            Assert.That(EmailValidator.Validate(email, allowInternational: true), Is.True);
        }

        // 1. THE "DISPLAY NAME" TRAP
        // MailAddress passes this; EmailValidation correctly fails it.
        [TestCase("Jeffrey Stedfast <jestedfa@microsoft.com>")]
        [TestCase("jestedfa@microsoft.com (Jeffrey Stedfast)")]
        public void Validate_ShouldRejectDisplayNamesAndComments(string email)
        {
            Assert.That(EmailValidator.Validate(email), Is.False,
                "RFC 5321 Address literals should not include display names or comments.");
        }

        // 2. LOCAL PART LENGTH (Exactly 64 chars is allowed, 65 is not)
        [Test]
        public void Validate_LocalPartBoundary()
        {
            string sixtyFourChars = new string('a', 64);
            string sixtyFiveChars = new string('a', 65);

            Assert.That(EmailValidator.Validate($"{sixtyFourChars}@domain.com"), Is.True, "64 chars should pass.");
            Assert.That(EmailValidator.Validate($"{sixtyFiveChars}@domain.com"), Is.False, "65 chars must fail.");
        }

        // 3. TOTAL LENGTH (Maximum 254 characters)
        [Test]
        public void Validate_TotalLengthBoundary()
        {// A valid domain must have labels no longer than 63 characters
            string label63 = new string('b', 63);
            string domain = $"{label63}.{label63}.{label63}.com"; // 63*3 + 3 dots + 3 'com' = 195 chars

            // 254 - 1 (@) - 195 (domain) = 58
            string local = new string('a', 58);

            string valid254 = $"{local}@{domain}";

            Assert.Multiple(() =>
            {
                Assert.That(valid254.Length, Is.EqualTo(254), "Manual check that string is 254");
                Assert.That(EmailValidator.Validate(valid254), Is.True, "254 chars with valid labels should pass.");

                string invalid255 = "a" + valid254;
                Assert.That(EmailValidator.Validate(invalid255), Is.False, "255 chars must fail.");
            });
        }

        // 4. THE "DOUBLE DOT" AND PURE SYNTAX
        [TestCase("user..name@domain.com")] // Consecutive dots
        [TestCase(".user@domain.com")]     // Leading dot
        [TestCase("user.@domain.com")]     // Trailing dot in local part
        public void Validate_ShouldRejectInvalidDotPlacement(string email)
        {
            Assert.That(EmailValidator.Validate(email), Is.False);
        }
    }
}