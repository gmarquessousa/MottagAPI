using App.Application.DTOs.Tags;
using App.Application.Exceptions;
using App.Application.Services;
using App.Domain.Entities;
using App.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace App.Tests.Services;

public class TagServiceTests : IClassFixture<TestFixture>
{
    private readonly TagService _svc;
    private readonly TestFixture _fx;

    public TagServiceTests(TestFixture fx)
    {
        _fx = fx;
        var createVal = new App.Application.Validation.CreateTagDtoValidator();
        var updateVal = new App.Application.Validation.UpdateTagDtoValidator();
        _svc = new TagService(_fx.RepoFactory.Get<Tag>(), _fx.RepoFactory.Get<Moto>(), createVal, updateVal, _fx.Mapper);
    }

    [Fact]
    public async Task DuplicidadeSerial_DeveLancarConflict()
    {
        var first = await _svc.CreateAsync(new CreateTagDto{ Serial="ABC123", Tipo=Domain.Enums.TagTipo.V1, BateriaPct=80});
        first.Serial.Should().Be("ABC123");
        var act = async () => await _svc.CreateAsync(new CreateTagDto{ Serial="ABC123", Tipo=Domain.Enums.TagTipo.V1, BateriaPct=50});
        await act.Should().ThrowAsync<ConflictException>();
    }
}