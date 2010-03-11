using System;
using System.ServiceModel.Configuration;

namespace Autodocs.ServiceModel.Web
{
    public class AutodocsBehaviorExtension : BehaviorExtensionElement
    {
        #region Overrides of BehaviorExtensionElement

        /// <summary>
        /// Creates a behavior extension based on the current configuration settings.
        /// </summary>
        /// <returns>
        /// The behavior extension.
        /// </returns>
        protected override object CreateBehavior()
        {
            return new AutodocsBehavior();
        }

        /// <summary>
        /// Gets the type of behavior.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Type"/>.
        /// </returns>
        public override Type BehaviorType
        {
            get { return typeof(AutodocsBehavior); }
        }

        #endregion
    }
}