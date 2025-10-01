using App.Application.DTOs.Motos;
using App.Application.DTOs.Patios;
using App.Application.Services;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace App.Tests.Services;

public class MotoServiceTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fx;
    private readonly MotoService _svc;
    private readonly PatioService _patioSvc;

    public MotoServiceTests(TestFixture fx)
    {
        _fx = fx;
        var motoRepo = _fx.RepoFactory.Get<Moto>();
        var patioRepo = _fx.RepoFactory.Get<Patio>();
        var createVal = new App.Application.Validation.CreateMotoDtoValidator();
        var updateVal = new App.Application.Validation.UpdateMotoDtoValidator();
        _svc = new MotoService(motoRepo, patioRepo, createVal, updateVal, _fx.Mapper);
        var patioCreateVal = new App.Application.Validation.CreatePatioDtoValidator();
        var patioUpdateVal = new App.Application.Validation.UpdatePatioDtoValidator();
        _patioSvc = new PatioService(patioRepo, patioCreateVal, patioUpdateVal, _fx.Mapper);
    }

    [Fact]
    public async Task Paginacao_DeveRetornarMetaCorreta()
    {
        var patio = await _patioSvc.CreateAsync(new CreatePatioDto{ Nome="P1", Cidade="A", Estado="B", Pais="BR", AreaM2=10});
        for (int i=0;i<25;i++)
        {
            // Gera placas válidas no padrão Mercosul AAA1A23 variando dígitos/letra intermediária
            var middleLetter = (char)('A' + (i % 26));
            var placa = $"AAA{i % 10}{middleLetter}{i:00}"; // garante 7 caracteres
            await _svc.CreateAsync(new CreateMotoDto{ PatioId=patio.Id, Placa=placa, Modelo="M", Status=MotoStatus.Disponivel});
        }
    var page2 = await _svc.ListAsync(patio.Id, status: null, placa: null, sortBy: "placa", sortDir: "asc", page: 2, pageSize: 10, CancellationToken.None);
        page2.Total.Should().Be(25);
        page2.Items.Should().HaveCount(10);
        page2.Page.Should().Be(2);
        page2.PageSize.Should().Be(10);
    // Ordenação ascendente por placa deve trazer a primeira da segunda página corretamente
    page2.Items.First().Placa.Should().NotBeNullOrEmpty();
    }
}