using System.Collections.Generic;
using Grand.Core.Plugins;

namespace Grand.Framework.Themes
{
    public partial interface IThemeProvider
    {
        ThemeConfiguration GetThemeConfiguration(string themeName);

        IList<ThemeConfiguration> GetThemeConfigurations();

        bool ThemeConfigurationExists(string themeName);

        ThemeDescriptor GetThemeDescriptorFromText(string text);
    }
}
