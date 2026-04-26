export interface CreateUserRequest {
    /**
     * Full name of the user
     * @required
     * @maxLength 100
     * @minLength 2
     */
    name: string;
    /**
     * @required
     * @format email
     */
    email: string;
    /**
     * @minimum 18
     * @maximum 120
     */
    age: number | null;
    /** @format phone */
    phoneNumber: string | null;
    /** @format uri */
    website: string | null;
    /** @pattern ^[A-Z]{2,3}\d{4}$ */
    code: string | null;
}
