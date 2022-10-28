using AutoMapper;
using Contracts;
using Entities.ConfigurationModels;
using Entities.Exceptions;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Service.Contracts;
using Shared.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Service
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IOptions<JwtConfiguration> _configuration;
        private readonly JwtConfiguration _jwtConfiguration;
        private User? _user;

        public AuthenticationService(ILoggerManager logger,
            IMapper mapper,
            UserManager<User> userManager,
            IOptions<JwtConfiguration> configuration)
        {
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
            _jwtConfiguration = _configuration.Value;
            //_configuration.Bind(_jwtConfiguration.Section, _jwtConfiguration);
        }

        public async Task<TokenDTO> CreateToken(bool populateExp)
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaims();
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);

            //REFRESH TOKEN
            var refreshToken = GenerateRefreshToken();
            _user.RefreshToken = refreshToken;

            if (populateExp)
            {
                _user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            }

            await _userManager.UpdateAsync(_user);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return new TokenDTO(accessToken, refreshToken);
        }
        /// <summary>
        /// creates an object of the JwtSecurityToken type with all of the required options
        /// </summary>
        /// <param name="signingCredentials"></param>
        /// <param name="claims"></param>
        /// <returns></returns>
        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var tokenOptions = new JwtSecurityToken
                (
                issuer: _jwtConfiguration.ValidIssuer,
                audience: _jwtConfiguration.ValidAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_jwtConfiguration.Expires)),
                signingCredentials: signingCredentials
                );
            return tokenOptions;
        }
        /// <summary>
        /// creates a list of claims with the user name inside and all the roles the user belongs to
        /// </summary>
        /// <returns></returns>
        private async Task<List<Claim>> GetClaims()
        {
            //initialize list of user claims with a claim of NAME as the first item
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, _user.UserName) };
            //retrieve roles user is assigned to
            var roles = await _userManager.GetRolesAsync(_user);
            //for each role of the user found, add the role to list of claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];


            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// get the user principal from the expired access token
        /// </summary>
        /// <param name="token">expired access token</param>
        /// <returns></returns>
        /// <exception cref="SecurityTokenException"></exception>
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET"))),
                ValidateLifetime = true,
                ValidIssuer = _jwtConfiguration.ValidIssuer,
                ValidAudience = _jwtConfiguration.ValidAudience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken is null ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid Token");
            }
            return principal;
        }

        public async Task<TokenDTO> RefreshToken(TokenDTO tokenDTO)
        {
            var principal = GetPrincipalFromExpiredToken(tokenDTO.AccessToken);

            var user = await _userManager.FindByNameAsync(principal.Identity.Name);
            if (user == null
                || user.RefreshToken != tokenDTO.RefreshToken
                || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                throw new RefreshTokenBadRequest();
            }

            return await CreateToken(false);
        }


        /// <summary>
        /// returns our secret key as a byte array with the security algorithm
        /// </summary>
        /// <returns></returns>
        private SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET"));
            var secret = new SymmetricSecurityKey(key);
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }
        public async Task<IdentityResult> RegisterUser(UserForRegistrationDTO userForRegistration)
        {
            var user = _mapper.Map<User>(userForRegistration);
            var result = await _userManager.CreateAsync(user, userForRegistration.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(user, userForRegistration.Roles);
            }

            return result;
        }

        public async Task<bool> ValidateUser(UserForAuthenticationDTO userForAuthentication)
        {
            _user = await _userManager.FindByNameAsync(userForAuthentication.UserName);

            var result = (_user != null &&
                await _userManager.CheckPasswordAsync(_user, userForAuthentication.Password));
            if (!result)
            {
                _logger.LogWarn(
                    $"{nameof(ValidateUser)}: Authentication failed. Wrong user name or password");
            }

            return result;
        }
    }
}
