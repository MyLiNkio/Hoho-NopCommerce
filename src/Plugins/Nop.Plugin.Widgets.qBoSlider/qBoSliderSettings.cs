using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.qBoSlider
{
    /// <summary>
    /// Represents plugin settings
    /// </summary>
    public class qBoSliderSettings : ISettings
    {
        /// <summary>
        /// Gets or sets value indicating plugin static cache usage
        /// </summary>
        public bool UseStaticCache { get; set; }

        /// <summary>
        /// Gets or sets mobile screens breakpoint value. All screen sizes which les and equal to that value - are mobile screens
        /// </summary>
        public int MobileScreenMaxSize { get; set; }
    }
}