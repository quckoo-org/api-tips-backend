// using Grpc.Core;
// using Grpc.Core.Interceptors;
//
// namespace ApiTips.Api.Interceptors;
//
// public class ReviseInterceptor : Interceptor
// {
//     /// <summary>
//     ///  Словарь, где ключ - название метода, а значение - тип запроса и наименование поля в запросе, которое хранит идентификатор сверки
//     /// </summary>
//     private readonly Dictionary<string, (Type, string)> _requestTypes = new()
//     {
//         { "SetReviseStep1", (typeof(SetReviseStep1Request), "ReviseId") },
//         { "SetReviseStep2", (typeof(SetReviseStep2Request), "ReviseId") },
//         { "SetReviseStep3", (typeof(SetReviseStep3Request), "ReviseId") },
//         { "SetReviseStep4", (typeof(SetReviseStep4Request), "Id") },
//         { "SetReviseStep5", (typeof(SetReviseStep5Request), "Id") },
//         { "CreateOrUpdateCorrectionReviseStep1", (typeof(CreateOrUpdateCorrectionReviseStep1Request), "Id") },
//         { "SetCorrectionReviseStep2", (typeof(SetCorrectionReviseStep2Request), "Id") },
//         { "SetCorrectionReviseStep3", (typeof(SetCorrectionReviseStep3Request), "CorrectionReviseId") },
//         { "SetCorrectionReviseStep4", (typeof(SetCorrectionReviseStep4Request), "Id") },
//         { "SetCorrectionReviseStep5", (typeof(SetCorrectionReviseStep5Request), "CorrectionReviseId") },
//     };
//     
//     private readonly Dictionary<string, (Type, string)> _requestReconciliationTypes = new()
//     {
//         { "SetReconciliationStepInfo", (typeof(SetReconciliationStepInfoRequest), "ReconciliationId") },
//         { "SetReconciliationStepCompare", (typeof(SetReconciliationStepCompareRequest), "ReconciliationId") },
//         { "SetReconciliationStepCalculations", (typeof(SetReconciliationStepCalculationsRequest), "ReconciliationId") },
//         { "SetReconciliationStepFines", (typeof(SetReconciliationStepFinesRequest), "ReconciliationId") },
//         { "SetReconciliationStepFinish", (typeof(SetReconciliationStepFinishRequest), "ReconciliationId") }
//     };
//
//     private readonly ILogger<ReviseInterceptor> _logger;
//     private IServiceProvider Services { get; }
//
//     public ReviseInterceptor(ILogger<ReviseInterceptor> logger, IServiceProvider services)
//     {
//         _logger = logger;
//         Services = services;
//     }
//
//     public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
//         ServerCallContext context,
//         UnaryServerMethod<TRequest, TResponse> continuation)
//     {
//         // Попытка найти метод в словаре
//         var methodName = _requestTypes.Keys.FirstOrDefault(x => context.Method.EndsWith(x));
//
//         var reviseId = GetIdIfExists(methodName, _requestTypes, request);
//         if (reviseId is not null)
//         {
//             await using var scope = Services.CreateAsyncScope();
//             await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
//             
//             if (await applicationContext.Revises.AnyAsync(x => x.Id == reviseId && x.CancellationTimestamp != null,
//                     context.CancellationToken))
//             {
//                 // Если сверка была отменена - запрет на выполнение метода
//                 _logger.LogWarning("Попытка изменить отмененную сверку с идентификатором {ReviseId}", reviseId);
//                 throw new RpcException(new Status(StatusCode.PermissionDenied, "Permission Denied"),
//                     new Metadata
//                     {
//                         { "grpc-status", nameof(StatusCode.PermissionDenied) },
//                         { "grpc-message", "Permission Denied" }
//                     });
//             }
//         }
//         
//         methodName = _requestReconciliationTypes.Keys.FirstOrDefault(x => context.Method.EndsWith(x));
//         var reconciliationId = GetIdIfExists(methodName, _requestReconciliationTypes, request);
//         if (reconciliationId is not null)
//         {
//             await using var scope = Services.CreateAsyncScope();
//             await using var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
//             
//             if (await applicationContext.Reconciliations.AnyAsync(x => x.Id == reconciliationId && x.CancellationTimestamp != null,
//                     context.CancellationToken))
//             {
//                 // Если сверка была отменена - запрет на выполнение метода
//                 _logger.LogWarning("Нельзя изменять отменённую эко-сверку с идентификатором {ReconciliationId}", reconciliationId);
//                 throw new RpcException(new Status(StatusCode.PermissionDenied, "Permission Denied"),
//                     new Metadata
//                     {
//                         { "grpc-status", nameof(StatusCode.PermissionDenied) },
//                         { "grpc-message", "Permission Denied" }
//                     });
//             }
//         }
//         return await continuation(request, context);
//     }
//
//     private long? GetIdIfExists <TRequest>(string? methodName, Dictionary<string, (Type, string)> methodsWithIdPropertyName, TRequest request)
//     {
//         if (methodName is not null && methodsWithIdPropertyName.TryGetValue(methodName, out var requestType))
//         {
//             // Каст запроса из TRequest в конкретный тип запроса
//             var requestData = Convert.ChangeType(request, requestType.Item1);
//
//             // Получение информации о поле, в котором находися идентификатор сверки
//             var propertyInfo = requestData.GetType().GetProperty(requestType.Item2);
//
//             // Получение идентификатора сверки
//             var propertyValue = (long?)propertyInfo?.GetValue(requestData);
//             return propertyValue;
//         }
//
//         return null;
//     }
// }