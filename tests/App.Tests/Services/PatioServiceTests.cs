using App.Application.DTOs.Patios;
using App.Application.Services;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace App.Tests.Services;

public class PatioServiceTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fx;
    private readonly PatioService _svc;

    public PatioServiceTests(TestFixture fx)
    {
        _fx = fx;
        var patioRepo = _fx.RepoFactory.Get<Patio>();
        var createVal = new App.Application.Validation.CreatePatioDtoValidator();
        var updateVal = new App.Application.Validation.UpdatePatioDtoValidator();
        _svc = new PatioService(patioRepo, createVal, updateVal, _fx.Mapper);
    }

    [Fact]
    public async Task Create_DevePersistir()
    {
        var dto = new CreatePatioDto{ Nome="Central", Cidade="SP", Estado="SP", Pais="BR", AreaM2=1000};
        var created = await _svc.CreateAsync(dto);
        created.Should().NotBeNull();
        created.Nome.Should().Be("Central");
        var count = await _fx.Db.Patios.CountAsync();
        count.Should().Be(1);
    }
}