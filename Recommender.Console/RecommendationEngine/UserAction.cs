using System;
using System.Collections.Generic;
using System.Text;

namespace RecommendationEngine
{
    public class UserAction
    {
        public RaterBase Rater { get; set; }
        public RateeBase Ratee { get; set; }
        public string Action { get; set;}
        
    }
}
