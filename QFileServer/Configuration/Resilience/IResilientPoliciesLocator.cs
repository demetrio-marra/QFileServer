using Polly;

namespace QFileServer.Configuration.Resilience
{
    public interface IResilientPoliciesLocator
    {
        IAsyncPolicy GetPolicy(ResilientPolicyType policyType);
    }
}
