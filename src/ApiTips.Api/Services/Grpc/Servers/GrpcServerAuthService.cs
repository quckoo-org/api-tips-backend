// using Google.Apis.Auth;
// using Grpc.Core;
// using X10Archery.CustomEnums.V1;
// using X10Archery.X10Auth.V1;
//
// namespace ApiTips.Api.Services.Grpc.Servers;
//
// public class GrpcServerAuthService(ILogger<GrpcServerAuthService> logger, IConfiguration configuration)
//     : AuthenticateService.AuthenticateServiceBase
// {
//     private readonly ILogger<GrpcServerAuthService> _logger = logger;
//     private readonly string? _googleClientId = configuration.GetValue<string>("Google:ClientId");
//
//     public override async Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
//     {
//         var response = new ValidateTokenResponse
//         {
//             IsValid = false,
//             OperationStatus = OperationStatus.Unspecified
//         };
//         
//         try
//         {
//             var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token, new GoogleJsonWebSignature.ValidationSettings
//             {
//                 Audience = new[] { _googleClientId }
//             });
//
//             if (payload is not null)
//             {
//                 response.IsValid = true;
//                 response.UserInfo = new UserInfo
//                 {
//                     Name = payload.Name,
//                     Email = payload.Email,
//                     Picture = payload.Picture
//                 };
//                 response.OperationStatus = OperationStatus.Ok;
//             }
//         }
//         catch (Exception e)
//         {
//             response.Error = e.Message;
//             response.OperationStatus = OperationStatus.Error;
//             
//             _logger.LogError("An error was occured | Exception {Exception} | InnerException {InnerException}", e.Message, e.InnerException?.Message);
//             
//             return response;
//         }
//         
//         return response;
//     }
//     
//     // Новый метод для обновления access token с использованием refresh token
//     public override async Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request, ServerCallContext context)
//     {
//         var response = new RefreshTokenResponse
//         {
//             OperationStatus = OperationStatus.Unspecified
//         };
//
//         try
//         {
//             // Получаем refresh token из запроса
//             var refreshToken = request.RefreshToken;
//
//             if (string.IsNullOrEmpty(refreshToken))
//             {
//                 response.Error = "No refresh token provided.";
//                 response.OperationStatus = OperationStatus.Error;
//                 return response;
//             }
//
//             // Получаем новый access token от Google
//             var newAccessToken = await GetAccessTokenFromRefreshToken(refreshToken);
//
//             if (string.IsNullOrEmpty(newAccessToken))
//             {
//                 response.Error = "Failed to refresh token.";
//                 response.OperationStatus = OperationStatus.Error;
//                 return response;
//             }
//
//             // Возвращаем новый access token
//             response.AccessToken = newAccessToken;
//             response.OperationStatus = OperationStatus.Ok;
//         }
//         catch (Exception e)
//         {
//             response.Error = e.Message;
//             response.OperationStatus = OperationStatus.Error;
//             _logger.LogError("Error during token refresh: {Error}", e.Message);
//         }
//
//         return response;
//     }
//     
//     private async Task<string?> GetAccessTokenFromRefreshToken(string refreshToken)
//     {
//         // Создайте запрос для обмена refresh token на новый access token через Google API
//         var client = new HttpClient();
//         var response = await client.PostAsJsonAsync("https://oauth2.googleapis.com/token", new
//         {
//             client_id = _googleClientId,
//             client_secret = "",
//             refresh_token = refreshToken,
//             grant_type = "refresh_token"
//         });
//
//         if (response.IsSuccessStatusCode)
//         {
//             var responseData = await response.Content.ReadFromJsonAsync<TokenResponse>();
//             return responseData?.AccessToken;
//         }
//
//         return null;
//     }
//     
// }
//
// public class TokenResponse
// {
//     public string? AccessToken { get; set; }
//     public string? TokenType { get; set; }
//     public int ExpiresIn { get; set; }
// }