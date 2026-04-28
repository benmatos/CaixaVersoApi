using System.Diagnostics;

namespace CaixaVersoApi.Middlewares;

public class ResponseTimeMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseTimeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Let the pipeline execute
        await _next(context);

        stopwatch.Stop();
        
        // We will store the elapsed time in HttpContext.Items so the ResultFilter can access it
        // Or if we want to add a header: context.Response.Headers.Add("X-Response-Time-ms", stopwatch.ElapsedMilliseconds.ToString());
        // Since the requirement asks to format the response JSON, we can't easily intercept it after it's written in a middleware unless we buffer the response stream.
        // It's much cleaner to use an ActionFilter or ResultFilter for the response body, but the time of action execution is slightly less than total middleware time.
        // The prompt says "tempo_da_resposta", a ResultFilter can capture action time, which is usually sufficient. But if we must use Middleware, we can intercept the stream.
        
        // For simplicity and adherence to standard ASP.NET Core practices, we'll let the ResultFilter handle the body formatting.
        // But since we need to "Implementar pelo menos um middleware customizado", we can use this one for Logging or Response Time headers.
        // Let's also log the request time here to fulfill the middleware requirement.
        
        var logger = context.RequestServices.GetRequiredService<ILogger<ResponseTimeMiddleware>>();
        logger.LogInformation("Request {Method} {Path} took {ElapsedMilliseconds} ms", 
            context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
    }
}
