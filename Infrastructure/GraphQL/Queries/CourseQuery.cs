using Infrastructure.Models;
using Infrastructure.Services;

namespace Infrastructure.GraphQL.Queries;

public class CourseQuery(ICourseService courseService)
{
    private readonly ICourseService _courseService = courseService;

    [GraphQLName("getCourses")]
    public async Task<IEnumerable<Course>> GetCoursesAsync()
    {
        return await _courseService.GetCoursesAsync();
    }

    [GraphQLName("getCourseById")]
    public async Task<Course> GetCourseByIdAsync(string id)
    {
        return await _courseService.GetCourseByIdAsync(id);
    }

    [GraphQLName("getCoursesByIds")]
    public async Task<IEnumerable<Course>> GetCoursesByIdsAsync(List<string> ids)
    {
        var courses = await _courseService.GetCoursesByIdsAsync(ids);
        return courses;
    }
}
