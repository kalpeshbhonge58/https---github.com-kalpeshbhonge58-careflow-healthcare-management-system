using AutoMapper;
using CareFlow.Application.DTOs.Auth;
using CareFlow.Application.Exceptions;
using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Shared.Constants;
using FluentValidation;

namespace CareFlow.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<User> _userRepositoryGeneric;
    private readonly IRepository<Role> _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthService(
        IUserRepository userRepository,
        IRepository<User> userRepositoryGeneric,
        IRepository<Role> roleRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<LoginRequest> loginValidator)
    {
        _userRepository = userRepository;
        _userRepositoryGeneric = userRepositoryGeneric;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _loginValidator = loginValidator;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException();

        if (!user.IsActive)
            throw new ForbiddenException("Your account has been deactivated.");

        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var (token, expiresAt) = _tokenService.GenerateToken(user, roles);

        await _auditService.LogAsync(user.Id, AuditActions.UserLogin, nameof(User), user.Id, "User logged in.", ipAddress, cancellationToken);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = _mapper.Map<UserResponse>(user)
        };
    }

    public async Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException("Email and password are required.");

        if (await _userRepository.GetByEmailAsync(request.Email, cancellationToken) is not null)
            throw new ConflictException("A user with this email already exists.");

        var role = (await _roleRepository.FindAsync(r => r.Name == request.Role, cancellationToken)).FirstOrDefault();
        if (role is null)
            throw new ValidationException($"Invalid role: {request.Role}.");

        var user = new User
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim()
        };

        user.UserRoles.Add(new UserRole { Role = role });
        await _userRepositoryGeneric.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdUser = await _userRepository.GetByIdWithRolesAsync(user.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), user.Id);

        return _mapper.Map<UserResponse>(createdUser);
    }

    public async Task<UserResponse> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        return _mapper.Map<UserResponse>(user);
    }
}
