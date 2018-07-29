using System.Collections.Generic;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Seo;

namespace Grand.Core.Domain.Affiliates
{
    /// <summary>
    /// Represents an affiliate
    /// </summary>
    public partial class Affiliate : BaseEntity, ILocalizedEntity, ISlugSupported
    {
        public Affiliate()
        {
            Locales = new List<LocalizedProperty>();
        }
        /// <summary>
        /// Gets or sets the address identifier
        /// </summary>
        public int AddressId { get; set; }

        /// <summary>
        /// Gets or sets the admin comment
        /// </summary>
        public string AdminComment { get; set; }

        /// <summary>
        /// Gets or sets the friendly name for generated affiliate URL (by default affiliate ID is used)
        /// </summary>
        public string FriendlyUrlName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is active
        /// </summary>
        public bool Active { get; set; }

        public string Name { get; set; }
        public string WebsiteUrl { get; set; }
        public string AffiliateUrl { get; set; }
        public string Description { get; set; }
        public string Benefits { get; set; }
        public string Payouts { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AcountNumber { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }

        /// <summary>
        /// Gets or sets the address
        /// </summary>
        public virtual Address Address { get; set; }

        public IList<LocalizedProperty> Locales { get; set; }

        public string SeName { get; set; }
    }
}
