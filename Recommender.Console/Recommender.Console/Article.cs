using System;
using System.Collections.Generic;
using System.Text;
using RecommendationEngine;

namespace Recommender.Console
{
    public class Article : RateeBase
    {
        public List<Tag> Tags { get; set; }
        public List<RaterBase> Views { get; set; }
        public List<RaterBase> Downloads { get; set; }
    }
}
