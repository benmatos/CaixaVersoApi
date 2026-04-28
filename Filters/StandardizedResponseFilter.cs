using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace CaixaVersoApi.Filters;

public class StandardizedResponseFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();

        // Execute the action
        var executedContext = await next();

        stopwatch.Stop();

        // Only format the response if it was an ObjectResult (success or data returned)
        if (executedContext.Result is ObjectResult objectResult)
        {
            var responseData = new
            {
                dados_resposta = objectResult.Value,
                timestamp_resposta = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                tempo_da_resposta = $"{stopwatch.ElapsedMilliseconds} ms"
            };

            executedContext.Result = new ObjectResult(responseData)
            {
                StatusCode = objectResult.StatusCode
            };
        }
    }
}
