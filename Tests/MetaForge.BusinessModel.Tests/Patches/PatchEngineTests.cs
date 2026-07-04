using FluentAssertions;
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches;
using MetaForge.BusinessModel.Patches.Operations;

namespace MetaForge.BusinessModel.Tests.Patches;

public class PatchEngineTests
{
    private readonly CommandLogStore _logStore = new();
    private readonly BusinessAuthoringDocument _document = new();
    private PatchEngine _engine => new(_logStore);

    [Fact]
    public void Apply_AddEntity_AddsToDocument()
    {
        var engine = _engine;
        var op = new AddEntityOp("Customer");

        engine.Apply(_document, op);

        _document.Entities.Should().HaveCount(1);
        _document.Entities[0].Name.Should().Be("Customer");
    }

    [Fact]
    public void Apply_AddEntity_CreatesLogEntry()
    {
        var engine = _engine;
        var op = new AddEntityOp("Customer");

        engine.Apply(_document, op);

        _logStore.Count.Should().Be(1);
        _logStore.GetAll()[0].CommandType.Should().Be("AddEntity");
    }

    [Fact]
    public void Apply_AddAttribute_ThrowsForNonExistentEntity()
    {
        var engine = _engine;
        var op = new AddAttributeOp("nonexistent", "Name");

        var act = () => engine.Apply(_document, op);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*neexistuje*");
    }

    [Fact]
    public void Apply_DeleteEntity_RemovesEntityAndRelations()
    {
        var engine = _engine;
        var addOp = new AddEntityOp("Customer");
        engine.Apply(_document, addOp);
        var entityId = addOp.EntityId;

        var deleteOp = new DeleteEntityOp(entityId);
        engine.Apply(_document, deleteOp);

        _document.Entities.Should().BeEmpty();
        _logStore.Count.Should().Be(2);
    }

    [Fact]
    public void Apply_NullDocument_ThrowsArgumentNullException()
    {
        var engine = _engine;
        var op = new AddEntityOp("Test");

        var act = () => engine.Apply(null!, op);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Apply_NullOperation_ThrowsArgumentNullException()
    {
        var engine = _engine;

        var act = () => engine.Apply(_document, null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
