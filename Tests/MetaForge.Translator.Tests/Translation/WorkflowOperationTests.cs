using FluentAssertions;
using MetaForge.BusinessModel.Models;
using MetaForge.BusinessModel.Patches.Operations;

namespace MetaForge.Translator.Tests.Translation;

public class WorkflowOperationTests
{
    // === AddWorkflowOp ===

    [Fact]
    public void AddWorkflowOp_Apply_AddsWorkflowToDocument()
    {
        var doc = EmptyDocument();
        var op = new AddWorkflowOp("ReviewProcess", "A review workflow");

        var result = op.Apply(doc);

        result.Workflows.Should().HaveCount(1);
        result.Workflows[0].Name.Should().Be("ReviewProcess");
        result.Workflows[0].Description.Should().Be("A review workflow");
    }

    [Fact]
    public void AddWorkflowOp_EmptyName_ThrowsArgumentException()
    {
        var act = () => new AddWorkflowOp("");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddWorkflowOp_Apply_DoesNotMutateOriginalDocument()
    {
        var doc = EmptyDocument();
        var op = new AddWorkflowOp("Test");

        var result = op.Apply(doc);

        doc.Workflows.Should().BeEmpty();
        result.Workflows.Should().HaveCount(1);
    }

    [Fact]
    public void AddWorkflowOp_ToEnvelope_HasCorrectCommandType()
    {
        var op = new AddWorkflowOp("Test");

        var envelope = op.ToEnvelope();

        envelope.CommandType.Should().Be("AddWorkflow");
    }

    // === AddWorkflowStepOp ===

    [Fact]
    public void AddWorkflowStepOp_Apply_AddsStepToExistingWorkflow()
    {
        var doc = DocumentWithWorkflow("wf1");
        var op = new AddWorkflowStepOp("wf1", "Approve", BusinessWorkflowStepKind.Manual);

        var result = op.Apply(doc);

        var workflow = result.Workflows.First(w => w.Id == "wf1");
        workflow.Steps.Should().HaveCount(1);
        workflow.Steps[0].Name.Should().Be("Approve");
        workflow.Steps[0].Kind.Should().Be(BusinessWorkflowStepKind.Manual);
    }

    [Fact]
    public void AddWorkflowStepOp_NonExistentWorkflow_ThrowsInvalidOperationException()
    {
        var doc = EmptyDocument();
        var op = new AddWorkflowStepOp("nonexistent", "Step", BusinessWorkflowStepKind.Automatic);

        var act = () => op.Apply(doc);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddWorkflowStepOp_EmptyName_ThrowsArgumentException()
    {
        var act = () => new AddWorkflowStepOp("wf1", "", BusinessWorkflowStepKind.Manual);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddWorkflowStepOp_ToEnvelope_HasCorrectCommandType()
    {
        var op = new AddWorkflowStepOp("wf1", "Step", BusinessWorkflowStepKind.Manual);

        var envelope = op.ToEnvelope();

        envelope.CommandType.Should().Be("AddWorkflowStep");
    }

    // === AddWorkflowTransitionOp ===

    [Fact]
    public void AddWorkflowTransitionOp_Apply_AddsTransition()
    {
        var doc = DocumentWithWorkflowAndSteps("wf1", "s1", "s2");
        var op = new AddWorkflowTransitionOp("wf1", "s1", "s2", "amount > 0", "Approve");

        var result = op.Apply(doc);

        var workflow = result.Workflows.First(w => w.Id == "wf1");
        workflow.Transitions.Should().HaveCount(1);
        workflow.Transitions[0].FromStepId.Should().Be("s1");
        workflow.Transitions[0].ToStepId.Should().Be("s2");
        workflow.Transitions[0].Condition.Should().Be("amount > 0");
        workflow.Transitions[0].Label.Should().Be("Approve");
    }

    [Fact]
    public void AddWorkflowTransitionOp_NonExistentWorkflow_Throws()
    {
        var doc = EmptyDocument();
        var op = new AddWorkflowTransitionOp("nonexistent", "s1", "s2");

        var act = () => op.Apply(doc);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddWorkflowTransitionOp_ToEnvelope_HasCorrectCommandType()
    {
        var op = new AddWorkflowTransitionOp("wf1", "s1", "s2");

        var envelope = op.ToEnvelope();

        envelope.CommandType.Should().Be("AddWorkflowTransition");
    }

    [Fact]
    public void AddWorkflowTransitionOp_EmptyFromStep_ThrowsArgumentException()
    {
        var act = () => new AddWorkflowTransitionOp("wf1", "", "s2");

        act.Should().Throw<ArgumentException>();
    }

    // === Helpers ===

    private static BusinessAuthoringDocument EmptyDocument()
    {
        return new BusinessAuthoringDocument
        {
            ProjectName = "Test"
        };
    }

    private static BusinessAuthoringDocument DocumentWithWorkflow(string workflowId)
    {
        var workflow = new BusinessWorkflowNode
        {
            Id = workflowId,
            Name = "TestWorkflow"
        };

        return new BusinessAuthoringDocument
        {
            ProjectName = "Test",
            Workflows = new[] { workflow }.ToList().AsReadOnly()
        };
    }

    private static BusinessAuthoringDocument DocumentWithWorkflowAndSteps(string workflowId, params string[] stepIds)
    {
        var steps = stepIds.Select(id => new BusinessWorkflowStepNode
        {
            Id = id,
            Name = $"Step{id}"
        }).ToList();

        var workflow = new BusinessWorkflowNode
        {
            Id = workflowId,
            Name = "TestWorkflow",
            Steps = steps.AsReadOnly()
        };

        return new BusinessAuthoringDocument
        {
            ProjectName = "Test",
            Workflows = new[] { workflow }.ToList().AsReadOnly()
        };
    }
}
