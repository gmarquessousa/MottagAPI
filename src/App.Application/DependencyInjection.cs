using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using App.Application.Services;
using App.Application.DTOs.Patios;
using App.Application.DTOs.Motos;
using App.Application.DTOs.Tags;
using FluentValidation;

namespace App.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Mappings.ApplicationProfile).Assembly);

        // Validators already discovered by assembly scanning if using AddValidatorsFromAssembly, but explicit if needed
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Services
        services.AddScoped<IPatioService, PatioService>();
        services.AddScoped<IMotoService, MotoService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ILinkBuilder, LinkBuilder>();

        return services;
    }
}
