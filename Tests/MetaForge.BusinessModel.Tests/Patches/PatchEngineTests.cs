using FluentAssertions;
using MetaForge.BusinessModel.CommandLog;
using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches;
using MetaForge.BusinessModel.Patches.Operations;

namespace MetaForge.BusinessModel.Tests.Patches;

public class PatchEngineTests
{
    private readonly CommandLogStore _logStore = new();
    private BusinessAuthoringDocument _document = new();
    private PatchEngine _engine => new(_logStore);

    [Fact]
    public void Apply_AddEntity_AddsToDocument()
    {
        var engine = _engine;
        var op = new AddEntityOp("Customer");

        _document = engine.Apply(_document, op);

        _document.Entities.Should().HaveCount(1);
        _document.Entities[0].Name.Should().Be("Customer");
    }

    [Fact]
    public void Apply_AddEntity_CreatesLogEntry()
    {
        var engine = _engine;
        var op = new AddEntityOp("Customer");

        _document = engine.Apply(_document, op);

        _logStore.Count.Should().Be(1);
        _logStore.GetAll()[0].CommandType.Should().Be("AddEntity");
    }

    [Fact]
    public void Apply_AddAttribute_ThrowsForNonExistentEntity()
    {
        var engine = _engine;
        var op = new AddAttributeOp("nonexistent", "Name");

        var act = () => _document = engine.Apply(_document, op);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*neexistuje*");
    }

    [Fact]
    public void Apply_DeleteEntity_RemovesEntityAndRelations()
    {
        var engine = _engine;
        var addOp = new AddEntityOp("Customer");
        _document = engine.Apply(_document, addOp);
        var entityId = addOp.EntityId;

        var deleteOp = new DeleteEntityOp(entityId);
        _document = engine.Apply(_document, deleteOp);

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

    [Fact]
    public void Apply_ImmutablePattern_OriginalDocumentUnchanged()
    {
        var engine = _engine;
        var original = new BusinessAuthoringDocument { ProjectName = "Test" };
        var op = new AddEntityOp("Customer");

        var updated = engine.Apply(original, op);

        // Původní dokument zůstává nezměněn
        original.Entities.Should().BeEmpty();
        // Nový dokument obsahuje entitu
        updated.Entities.Should().HaveCount(1);
    }

    [Fact]
    public void Apply_SetCoreDetail_SetsCoreDetailOnAttribute()
    {
        var engine = _engine;
        var doc = new BusinessAuthoringDocument();
        var addEntityOp = new AddEntityOp("Customer");
        doc = engine.Apply(doc, addEntityOp);
        var entityId = addEntityOp.EntityId;

        var addAttrOp = new AddAttributeOp(entityId, "Email", "string");
        doc = engine.Apply(doc, addAttrOp);
        var attrId = addAttrOp.AttributeId;

        var coreDetail = new BusinessAttributeCoreDetail
        {
            Source = CoreInfoSource.Generated,
            ValueObjectName = "EmailAddress",
            IsStrongType = true,
        };
        var setCoreOp = new SetCoreDetailOp(entityId, attrId, coreDetail);

        doc = engine.Apply(doc, setCoreOp);

        var attr = doc.Entities[0].Attributes[0];
        attr.CoreDetail.Should().NotBeNull();
        attr.CoreDetail!.ValueObjectName.Should().Be("EmailAddress");
        attr.CoreDetail.IsStrongType.Should().BeTrue();
    }

    [Fact]
    public void Apply_UpdateSyncState_ChangesSyncState()
    {
        var engine = _engine;
        var doc = new BusinessAuthoringDocument();
        var addEntityOp = new AddEntityOp("Customer");
        doc = engine.Apply(doc, addEntityOp);
        var entityId = addEntityOp.EntityId;

        var addAttrOp = new AddAttributeOp(entityId, "Email", "string");
        doc = engine.Apply(doc, addAttrOp);
        var attrId = addAttrOp.AttributeId;

        // Nejdřív nastav CoreDetail
        var coreDetail = new BusinessAttributeCoreDetail
        {
            Source = CoreInfoSource.Generated,
            SyncState = AttributeSyncState.New,
        };
        var setCoreOp = new SetCoreDetailOp(entityId, attrId, coreDetail);
        doc = engine.Apply(doc, setCoreOp);

        // Pak změň SyncState
        var syncOp = new UpdateSyncStateOp(entityId, attrId, AttributeSyncState.Synced);
        doc = engine.Apply(doc, syncOp);

        var attr = doc.Entities[0].Attributes[0];
        attr.CoreDetail!.SyncState.Should().Be(AttributeSyncState.Synced);
    }
}
