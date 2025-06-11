using PayPalCheckoutSdk.Core;
using PayPalHttp;

namespace WebApp.ApiClients
{
    public class PayPalClient
    {
        // podaci sa sandboxa
        private static readonly string ClientId = "AR_NOjho6npaDIpZLO5oEuZ3bl4GDS3fcjluCHXHneR3UfMuj06lZFnUUwmBr0RW9_WcIxwCOL1eqbRx";
        private static readonly string ClientSecret = "EFPOUuflSwja1BQWUFXsFcSLjXmiteKUDm_cYVW5-JxE8CFtA3hq_MbrL98q0oEIea6RnXFN1KfDNXYs";

        public static PayPalEnvironment Environment =>
            new SandboxEnvironment(ClientId, ClientSecret);

        public static PayPalHttpClient Client() =>
            new PayPalHttpClient(Environment);
    }
}
