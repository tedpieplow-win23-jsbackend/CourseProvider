using Infrastructure.Data.Contexts;
using Infrastructure.GraphQL.Mutations;
using Infrastructure.GraphQL.ObjectTypes;
using Infrastructure.GraphQL.Queries;
using Infrastructure.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddPooledDbContextFactory<DataContext>(x =>
        {
            x.UseCosmos(Environment.GetEnvironmentVariable("COSMOS_URI")!, Environment.GetEnvironmentVariable("COSMOS_DBNAME")!)
            .UseLazyLoadingProxies();
        });

        services.AddScoped<ICourseService, CourseService>();

        services.AddGraphQLFunction()
                .AddQueryType<CourseQuery>()
                .AddMutationType<CourseMutation>()
                .AddType<CourseType>();

        var sp = services.BuildServiceProvider();
        using var scoped = sp.CreateScope();
        var dbcontextFactory = scoped.ServiceProvider.GetRequiredService<IDbContextFactory<DataContext>>();
        using var context = dbcontextFactory.CreateDbContext();
        context.Database.EnsureCreated();
    })
    .Build();

host.Run();
