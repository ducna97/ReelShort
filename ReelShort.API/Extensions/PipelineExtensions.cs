using ReelShort.API.Middlewares;

namespace ReelShort.API.Extensions;

public static class PipelineExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseMiddleware<RequestResponseMiddleware>();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors("AllowReactApp");

        app.UseAuthentication();
        
        app.UseMiddleware<TokenBlacklistMiddleware>();
        
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}