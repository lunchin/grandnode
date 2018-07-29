using System;
using System.Collections.Generic;
using System.Text;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Seo;

namespace Grand.Core.Domain.Reviews
{
    public class TopFive : BaseEntity, ISlugSupported, ILocalizedEntity
    {
        public TopFive()
        {
            Locales = new List<LocalizedProperty>();    
        }


        /// <summary>
        /// Gets or sets the blog post title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the blog tags
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// Gets or sets the blog post start date and time
        /// </summary>
        public DateTime? StartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the blog post end date and time
        /// </summary>
        public DateTime? EndDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the meta description
        /// </summary>
        public string MetaDescription { get; set; }

        /// <summary>
        /// Gets or sets the meta title
        /// </summary>
        public string MetaTitle { get; set; }

        public string SeName { get; set; }

        public IList<LocalizedProperty> Locales { get; set; }

        public string AffiliateIdOne{ get; set; }

        public string AffiliateIdTwo { get; set; }

        public string AffiliateIdThree { get; set; }

        public string AffiliateIdFour { get; set; }

        public string AffiliateIdFive { get; set; }
    }

}
