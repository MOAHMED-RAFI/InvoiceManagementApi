using InvoiceManagementApi.Services;

namespace InvoiceManagementApi.Endpoints;

public static class ResultExtensions
{
    /// <summary>Converts a service result into the matching HTTP response.</summary>
    public static IResult ToHttpResult<T>(this ServiceResult<T> result) =>
        result.Kind switch
        {
            ResultKind.Ok => Results.Ok(result.Value),
            ResultKind.NotFound => Results.NotFound(new { error = result.Message }),
            ResultKind.Conflict => Results.Conflict(new { error = result.Message }),
            _ => Results.Problem()
        };
}
