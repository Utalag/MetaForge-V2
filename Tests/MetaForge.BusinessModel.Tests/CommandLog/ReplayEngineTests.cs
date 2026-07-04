using FluentAssertions;
using MetaForge.BusinessModel.CommandLog;

namespace MetaForge.BusinessModel.Tests.CommandLog;

public class ReplayEngineTests
{
    private readonly ReplayEngine _engine = new();

    [Fact]
    public void Replay_EmptyLog_ReturnsEmptyDocument()
    {
        var commands = new List<CommandEnvelope>();
        var doc = _engine.Replay(commands);

        doc.Entities.Should().BeEmpty();
        doc.Relations.Should().BeEmpty();
    }

    [Fact]
    public void Replay_SingleAddEntity_ReturnsDocumentWithEntity()
    {
        var commands = new List<CommandEnvelope>
        {
            new()
            {
                CommandType = "AddEntity",
                TargetEntityId = "e1",
                Payload = "Customer",
            }
        };

        var doc = _engine.Replay(commands);

        doc.Entities.Should().HaveCount(1);
        doc.Entities[0].Name.Should().Be("Customer");
        doc.Entities[0].Id.Should().Be("e1");
    }

    [Fact]
    public void Replay_TwoEntities_BothPresent()
    {
        var commands = new List<CommandEnvelope>
        {
            new() { CommandType = "AddEntity", TargetEntityId = "e1", Payload = "Customer" },
            new() { CommandType = "AddEntity", TargetEntityId = "e2", Payload = "Order" },
        };

        var doc = _engine.Replay(commands);

        doc.Entities.Should().HaveCount(2);
        doc.Entities.Select(e => e.Name).Should().Contain(new[] { "Customer", "Order" });
    }

    [Fact]
    public void Replay_AddThenUpdate_NameIsUpdated()
    {
        var commands = new List<CommandEnvelope>
        {
            new() { CommandType = "AddEntity", TargetEntityId = "e1", Payload = "Customer" },
            new() { CommandType = "UpdateEntity", TargetEntityId = "e1", Payload = "Client" },
        };

        var doc = _engine.Replay(commands);

        doc.Entities.Should().HaveCount(1);
        doc.Entities[0].Name.Should().Be("Client");
    }

    [Fact]
    public void Replay_AddThenDelete_EntityRemoved()
    {
        var commands = new List<CommandEnvelope>
        {
            new() { CommandType = "AddEntity", TargetEntityId = "e1", Payload = "Customer" },
            new() { CommandType = "DeleteEntity", TargetEntityId = "e1" },
        };

        var doc = _engine.Replay(commands);

        doc.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Replay_Deterministic_SameInputSameOutput()
    {
        var commands = new List<CommandEnvelope>
        {
            new() { CommandType = "AddEntity", TargetEntityId = "e1", Payload = "A" },
            new() { CommandType = "AddEntity", TargetEntityId = "e2", Payload = "B" },
        };

        var doc1 = _engine.Replay(commands);
        var doc2 = _engine.Replay(commands);

        doc1.Entities.Should().HaveCount(doc2.Entities.Count);
        doc1.Entities[0].Name.Should().Be(doc2.Entities[0].Name);
        doc1.Entities[1].Name.Should().Be(doc2.Entities[1].Name);
    }

    [Fact]
    public void Replay_AddAttribute_AttributePresent()
    {
        var commands = new List<CommandEnvelope>
        {
            new() { CommandType = "AddEntity", TargetEntityId = "e1", Payload = "Customer" },
            new() { CommandType = "AddAttribute", TargetEntityId = "e1", TargetAttributeId = "a1", Payload = "FirstName|string|true" },
        };

        var doc = _engine.Replay(commands);

        doc.Entities[0].Attributes.Should().HaveCount(1);
        doc.Entities[0].Attributes[0].Name.Should().Be("FirstName");
        doc.Entities[0].Attributes[0].Type.Should().Be("string");
        doc.Entities[0].Attributes[0].IsRequired.Should().BeTrue();
    }
}
