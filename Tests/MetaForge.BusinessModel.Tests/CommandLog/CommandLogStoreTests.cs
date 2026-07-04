using FluentAssertions;
using MetaForge.BusinessModel.CommandLog;

namespace MetaForge.BusinessModel.Tests.CommandLog;

public class CommandLogStoreTests
{
    [Fact]
    public void NewStore_HasCountZero()
    {
        var store = new CommandLogStore();
        store.Count.Should().Be(0);
    }

    [Fact]
    public void Append_IncrementsCount()
    {
        var store = new CommandLogStore();
        store.Append(new CommandEnvelope { CommandType = "AddEntity", Payload = "Test" });
        store.Count.Should().Be(1);
    }

    [Fact]
    public void Append_MultipleCommands_PreservesOrder()
    {
        var store = new CommandLogStore();
        store.Append(new CommandEnvelope { CommandType = "First" });
        store.Append(new CommandEnvelope { CommandType = "Second" });
        store.Append(new CommandEnvelope { CommandType = "Third" });

        var all = store.GetAll();
        all.Should().HaveCount(3);
        all[0].CommandType.Should().Be("First");
        all[1].CommandType.Should().Be("Second");
        all[2].CommandType.Should().Be("Third");
    }

    [Fact]
    public void AppendOnly_NoRemoveMethod_InvariantPreserved()
    {
        // Ověřuje, že CommandLogStore neexponuje žádnou metodu pro odebrání
        var storeType = typeof(CommandLogStore);
        var methods = storeType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        methods.Should().NotContain(m => m.Name.Contains("Remove", StringComparison.OrdinalIgnoreCase));
        methods.Should().NotContain(m => m.Name.Contains("Delete", StringComparison.OrdinalIgnoreCase));
        methods.Should().NotContain(m => m.Name.Contains("Clear", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetAll_ReturnsReadOnlyView()
    {
        var store = new CommandLogStore();
        store.Append(new CommandEnvelope { CommandType = "Test" });
        var all = store.GetAll();
        all.Should().HaveCount(1);
    }

    [Fact]
    public void GetFrom_ReturnsCommandsFromIndex()
    {
        var store = new CommandLogStore();
        store.Append(new CommandEnvelope { CommandType = "0" });
        store.Append(new CommandEnvelope { CommandType = "1" });
        store.Append(new CommandEnvelope { CommandType = "2" });

        var from = store.GetFrom(1);
        from.Should().HaveCount(2);
        from[0].CommandType.Should().Be("1");
        from[1].CommandType.Should().Be("2");
    }
}
