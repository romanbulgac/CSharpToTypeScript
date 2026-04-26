using System;

namespace Models
{
    /// <summary>
    /// Represents a user in the system.
    /// Used for authentication and authorization.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the user's full name.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the role.
        /// </summary>
        public Role Role { get; set; }
    }

    /// <summary>
    /// Defines user roles.
    /// </summary>
    public enum Role
    {
        /// <summary>
        /// Administrator with full access.
        /// </summary>
        Admin,

        /// <summary>
        /// Standard user.
        /// </summary>
        User,

        Guest
    }
}
