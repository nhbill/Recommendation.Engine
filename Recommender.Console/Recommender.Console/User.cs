using System;
using System.Collections.Generic;
using System.Linq; 
using System.Text;
using RecommendationEngine; 

namespace Recommender.Console
{
    public class User : RaterBase
    {
        public List<RateeBase> Views { get; set; }
        public List<RateeBase> Downloads { get; set; }

        public List<KeyValuePair<Tag, int>> TagsByViewNumber { get; set; }

        public List<KeyValuePair<RateeBase, double>> SuggestionsCalculatedByTags { get; set; }
    }
}
