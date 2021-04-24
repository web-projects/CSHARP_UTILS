using System.Threading;

namespace Utils.Helper
{
    internal sealed class CancellationTokenManagerImpl
    {
        public const int InterruptFeatureNoTimeout = -1;
        public const int InterruptFeatureDefaultTimeout = 5000;

        private CancellationTokenSource tokenSource;
        private bool disposed;

        public CancellationToken GetShortLivedToken(int shortLivedDurationMS)
        {
            if (tokenSource != null)
            {
                tokenSource.Dispose();
                tokenSource = null;
            }

            tokenSource = shortLivedDurationMS switch
            {
                InterruptFeatureNoTimeout => new CancellationTokenSource(),
                _ => new CancellationTokenSource(shortLivedDurationMS)
            };

            return tokenSource.Token;
        }
    }
}
