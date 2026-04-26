/**
 * Represents a user in the system.
 * Used for authentication and authorization.
 */
export interface User {
    /** Gets or sets the user's full name. */
    fullName: string;
    /** Gets or sets the unique identifier. */
    id: number;
    /** Gets or sets the role. */
    role: Role;
}

/** Defines user roles. */
export enum Role {
    /** Administrator with full access. */
    Admin = 0,
    /** Standard user. */
    User = 1,
    Guest = 2
}
