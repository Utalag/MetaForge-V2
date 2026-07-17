using Microsoft.Extensions.DependencyInjection;

namespace MetaForge.Feedback;

public static class FeedbackServiceRegistration
{
    public static IServiceCollection AddMetaForgeFeedback(this IServiceCollection services)
    {
        services.AddScoped<IAuthoringFeedbackService, AuthoringFeedbackService>();
        services.AddSingleton<IFeedbackCacheRepository, JsonFeedbackCacheRepository>();
        services.AddSingleton<IFeedbackLearningRepository, JsonFeedbackLearningRepository>();
        return services;
    }
}
