namespace Karpaty.Api.Middlewares
{
    using System.Net;
    using System.Text.Json;

    namespace Karpaty.Api.Middlewares
    {
        public class GlobalExceptionMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILogger<GlobalExceptionMiddleware> _logger;

            public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
            {
                _next = next;
                _logger = logger;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Сталася непередбачена помилка на сервері.");
                    await HandleExceptionAsync(context, ex);
                }
            }

            private static Task HandleExceptionAsync(HttpContext context, Exception exception)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = new
                {
                    status = context.Response.StatusCode,
                    error = "Внутрішня помилка сервера. Спробуйте пізніше.",
                };

                return context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}
