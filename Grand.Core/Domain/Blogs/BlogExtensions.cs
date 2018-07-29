using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Core.Domain.Blogs
{
    public static class BlogExtensions
    {
        public static string[] ParseTags(this BlogPost blogPost)
        {
            if (blogPost == null)
            {
                throw new ArgumentNullException(nameof(blogPost));
            }

            var parsedTags = new List<string>();
            if (string.IsNullOrEmpty(blogPost.Tags))
            {
                return parsedTags.ToArray();
            }
            var tags2 = blogPost.Tags.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            parsedTags.AddRange(tags2.Select(tag2 => tag2.Trim()).Where(tmp => !string.IsNullOrEmpty(tmp)));
            return parsedTags.ToArray();
        }
    }
}
