# CareFlow Interview Guide

## Angular JWT Authentication

### What is JWT?
A JSON Web Token is a signed, compact credential containing claims. CareFlow generates it in the ASP.NET Core `JwtTokenService` after successful login. The token includes the user ID, email, name, roles, issuer, audience, and expiration.

### Why use JWT authentication?
JWT provides stateless authentication suited to the Angular SPA. The API validates the signature and claims on each protected request without server-side session storage.

### How does Angular HttpInterceptor work?
CareFlow uses the functional `authInterceptor`. It retrieves the token from `AuthService`, clones immutable requests, and adds `Authorization: Bearer <token>` before forwarding them. Login requests are excluded.

### Why use an HTTP interceptor?
It centralizes authorization header handling so feature services do not duplicate token logic or risk omitting headers.

### Authentication versus authorization
Authentication establishes who the user is. Authorization determines what that user may access. CareFlow authenticates with JWT and enforces permissions in ASP.NET Core using role claims and `[Authorize]` attributes.

### What is AuthGuard?
`authGuard` protects Angular routes by checking `AuthService.isAuthenticated()`. It redirects unauthenticated users to `/login`, including the intended return URL.

### What is RoleGuard?
`roleGuard` reads route metadata such as `data: { roles: ['Admin'] }` and checks the user's roles through `AuthService`. It redirects authenticated users without the required role to `/unauthorized`.

### What happens on HTTP 401?
The interceptor clears the local token and user profile, updates the authentication state, and redirects to `/login`. It does not retry indefinitely, and it avoids redirect handling for the login request itself.

### What happens on HTTP 403?
The interceptor keeps the session intact and navigates to `/unauthorized` with a user-friendly access message. A 403 means the identity is valid but the requested resource is forbidden.

### Why should Angular not be trusted for authorization?
Angular code runs in the user's browser and can be modified or bypassed. CareFlow's API remains the security boundary and validates JWT signatures, expiration, issuer, audience, and roles on every protected request.

### Where is the JWT generated?
`JwtTokenService` in `CareFlow.Infrastructure` creates an HMAC-SHA256 signed token after `AuthService.LoginAsync()` validates the user's credentials.

### How does ASP.NET Core validate the JWT?
`Program.cs` configures `JwtBearer` authentication with issuer, audience, lifetime, signing-key validation, and zero clock skew. The signing key and token-generation settings come from `JwtSettings`.

### How are roles represented in the JWT?
The backend writes each role using `ClaimTypes.Role`. ASP.NET Core maps these claims for `[Authorize(Roles = "...")]`; Angular uses the user profile's `roles` array for route UX.

### How is JWT stored in CareFlow?
`AuthService` stores the token, expiration, and typed user profile under centralized localStorage keys. It exposes `currentUser$` as an RxJS observable so components can react to login and logout.

### What is the logout flow?
The dashboard calls `AuthService.logout()`, which removes all authentication storage, publishes `null` to the state stream, and navigates to `/login`. Protected routes then fail `authGuard`.
