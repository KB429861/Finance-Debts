using Finance.Resources;

namespace Finance
{
    public class LocalizedStrings
    {
        private static readonly AppResources _localizedResources = new AppResources();

        public AppResources LocalizedResources => _localizedResources;
    }
}