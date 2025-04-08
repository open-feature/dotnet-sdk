using OpenFeature.Model;

namespace OpenFeature.E2ETests.Utils;

public class State
{
    public FeatureClient? Client;
    public FlagState? Flag;
    public object? FlagEvaluationDetailsResult;
    public TestHook? TestHook;
    public object? FlagResult;
    public EvaluationContext? EvaluationContext;
}
