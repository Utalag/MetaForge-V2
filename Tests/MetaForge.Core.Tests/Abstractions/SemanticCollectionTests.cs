using FluentAssertions;
using MetaForge.Core.Abstractions;

namespace MetaForge.Core.Tests.Abstractions;

public class SemanticCollectionTests
{
    private sealed class TestItem
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>Add vyvolá Changed událost.</summary>
    [Fact]
    public void Add_RaisesChangedEvent()
    {
        var collection = new SemanticCollection<TestItem>();
        var raised = false;
        collection.Changed += () => raised = true;

        collection.Add(new TestItem { Name = "x" });

        raised.Should().BeTrue();
    }

    /// <summary>Remove vyvolá Changed událost.</summary>
    [Fact]
    public void Remove_RaisesChangedEvent()
    {
        var collection = new SemanticCollection<TestItem>();
        var item = new TestItem { Name = "x" };
        collection.Add(item);

        var raised = false;
        collection.Changed += () => raised = true;

        collection.Remove(item);
        raised.Should().BeTrue();
    }

    /// <summary>Clear vyvolá Changed událost.</summary>
    [Fact]
    public void Clear_RaisesChangedEvent()
    {
        var collection = new SemanticCollection<TestItem>();
        collection.Add(new TestItem { Name = "x" });

        var raised = false;
        collection.Changed += () => raised = true;

        collection.Clear();
        raised.Should().BeTrue();
    }

    /// <summary>Changed není vyvoláno bez mutace.</summary>
    [Fact]
    public void ChangedEvent_NotRaisedBeforeMutation()
    {
        var collection = new SemanticCollection<TestItem>();
        var raised = false;
        collection.Changed += () => raised = true;

        raised.Should().BeFalse();
    }

    /// <summary>Každý Add vyvolá Changed zvlášť.</summary>
    [Fact]
    public void MultipleAdds_RaisesChangedEachTime()
    {
        var collection = new SemanticCollection<TestItem>();
        var count = 0;
        collection.Changed += () => count++;

        collection.Add(new TestItem { Name = "a" });
        collection.Add(new TestItem { Name = "b" });

        count.Should().Be(2);
    }

    /// <summary>Remove neexistující položky nevyvolá chybu.</summary>
    [Fact]
    public void Remove_NonExisting_DoesNotThrow()
    {
        var collection = new SemanticCollection<TestItem>();
        var act = () => collection.Remove(new TestItem { Name = "nonexistent" });
        act.Should().NotThrow();
    }

    /// <summary>Clear prázdné kolekce vyvolá Changed.</summary>
    [Fact]
    public void Clear_EmptyCollection_RaisesChanged()
    {
        var collection = new SemanticCollection<TestItem>();
        var raised = false;
        collection.Changed += () => raised = true;

        collection.Clear();
        raised.Should().BeTrue();
    }
}
