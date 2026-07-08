using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Diagnostics;
using MetaForge.Core.Transforms;

namespace MetaForge.Core.Tests.Transforms;

public class TransformPipelineTests
{
    [Fact]
    public void Pipeline_Empty_RunsSuccessfully()
    {
        var pipeline = new TransformPipeline();
        var result = pipeline.Run(TypeModel.String);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(TypeModel.String);
    }

    [Fact]
    public void Pipeline_WithTransform_Applies()
    {
        var pipeline = new TransformPipeline();
        pipeline.Add(new IdentityTransform());

        var result = pipeline.Run(TypeModel.Int32);
        result.IsSuccess.Should().BeTrue();
        result.Value.BaseType.Should().Be(DataType.Int32);
    }

    [Fact]
    public void Pipeline_FailFast_StopsOnError()
    {
        var pipeline = new TransformPipeline();
        var counter = new CounterTransform();
        pipeline.Add(new ErrorTransform("MF-ERR-001", "fail"));
        pipeline.Add(counter);

        var result = pipeline.Run(TypeModel.String);
        result.IsSuccess.Should().BeFalse();
        result.Bag.ErrorCount.Should().Be(1);
        counter.InvocationCount.Should().Be(0); // Second transform never ran
    }

    // === Test doubles ===

    private sealed class IdentityTransform : IModelTransform
    {
        public string Name => "Identity";
        public TypeModel Apply(TypeModel model, TransformContext context) => model;
    }

    private sealed class ErrorTransform : IModelTransform
    {
        private readonly string _code;
        private readonly string _msg;
        public string Name => "ErrorTransform";

        public ErrorTransform(string code, string msg) { _code = code; _msg = msg; }

        public TypeModel Apply(TypeModel model, TransformContext context)
        {
            context.Diagnostics.Report(new Diagnostic(
                _code, _msg, DiagnosticSeverity.Error,
                new ElementPath("Model", "Test")));
            return model;
        }
    }

    private sealed class CounterTransform : IModelTransform
    {
        public string Name => "Counter";
        public int InvocationCount { get; private set; }

        public TypeModel Apply(TypeModel model, TransformContext context)
        {
            InvocationCount++;
            return model;
        }
    }
}
