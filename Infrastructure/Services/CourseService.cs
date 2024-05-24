﻿using Infrastructure.Data.Contexts;
using Infrastructure.Data.Entities;
using Infrastructure.Factories;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public interface ICourseService
{
    Task<Course> CreateCourseAsync(CourseCreateRequest request);
    Task<Course> GetCourseByIdAsync(string courseId);
    Task<IEnumerable<Course>> GetCoursesAsync();
    Task<IEnumerable<Course>> GetCoursesByIdsAsync(List<string> ids);
    Task<Course> UpdateCourseAsync(CourseUpdateRequest request);
    Task<bool> DeleteCourseAsync(string id);
}

public class CourseService(IDbContextFactory<DataContext> contextFactory) : ICourseService
{
    private readonly IDbContextFactory<DataContext> _contextFactory = contextFactory;

    public async Task<Course> CreateCourseAsync(CourseCreateRequest request)
    {
        await using var context = _contextFactory.CreateDbContext();

        var courseEntity = CourseFactory.Create(request);
        context.Courses.Add(courseEntity);
        await context.SaveChangesAsync();

        return CourseFactory.Create(courseEntity);
    }

    public async Task<bool> DeleteCourseAsync(string id)
    {
        await using var context = _contextFactory.CreateDbContext();
        var courseEntity = await context.Courses.FirstOrDefaultAsync(c => c.Id == id);
        if (courseEntity is null) return false;

        context.Courses.Remove(courseEntity);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Course> GetCourseByIdAsync(string id)
    {
        await using var context = _contextFactory.CreateDbContext();
        var courseEntity = await context.Courses.FirstOrDefaultAsync(c => c.Id == id);
        return courseEntity == null ? null! : CourseFactory.Create(courseEntity);
    }

    public async Task<IEnumerable<Course>> GetCoursesByIdsAsync(List<string> ids)
    {
        await using var context = _contextFactory.CreateDbContext();

        var courseEntities = await context.Courses
            .Where(c => ids.Contains(c.Id))
            .ToListAsync();

        return courseEntities.Select(CourseFactory.Create).ToList();
    }

    public async Task<IEnumerable<Course>> GetCoursesAsync()
    {
        await using var context = _contextFactory.CreateDbContext();
        var courseEntity = await context.Courses.ToListAsync();

        return courseEntity.Select(CourseFactory.Create).ToList();
    }

    public async Task<Course> UpdateCourseAsync(CourseUpdateRequest request)
    {
        try
        {
            await using var context = _contextFactory.CreateDbContext();
            var existingCourse = await context.Courses
                                              .Include(c => c.Content)
                                              .ThenInclude(c => c.ProgramDetails)
                                              .FirstOrDefaultAsync(c => c.Id == request.Id);
            if (existingCourse is null) return null!;

            context.Entry(existingCourse).CurrentValues.SetValues(CourseFactory.Create(request));

            if (existingCourse.Content == null)
            {
                existingCourse.Content = new ContentEntity();
            }

            if (request.Content != null)
            {
                existingCourse.Content.Description = request.Content.Description;
                existingCourse.Content.Includes = request.Content.Includes;

                var existingProgramDetails = existingCourse.Content.ProgramDetails;
                existingProgramDetails.Clear();

                foreach (var pd in request.Content.ProgramDetails)
                {
                    existingProgramDetails.Add(new ProgramDetailItemEntity
                    {
                        Id = pd.Id,
                        Title = pd.Title,
                        Description = pd.Description
                    });
                }
            }

            var result = await context.SaveChangesAsync();
            if (result > 0)
            {
                return CourseFactory.Create(existingCourse);
            }
            else
            {
                return null!;
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
}
