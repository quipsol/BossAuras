using BossAuras.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace BossAuras.Core.Extensions;

public static class NMapScreenExtensions
{
    extension(NMapScreen mapScreen)
    {
        /// The <see cref="NMapAuraContainer"/> attached to this NMapScreen.
        public NMapAuraContainer AuraContainer => NMapAuraContainer.GetFromMapScreen(mapScreen);
    }
}