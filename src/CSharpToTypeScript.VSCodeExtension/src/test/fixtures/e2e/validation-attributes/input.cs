using System.ComponentModel.DataAnnotations;

namespace Contracts
{
    public class CreateUserRequest
    {
        /// <summary>Full name of the user</summary>
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Range(18, 120)]
        public int? Age { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [Url]
        public string? Website { get; set; }

        [RegularExpression(@"^[A-Z]{2,3}\d{4}$")]
        public string? Code { get; set; }
    }
}
