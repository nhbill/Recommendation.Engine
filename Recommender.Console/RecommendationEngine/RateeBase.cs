using System;
using System.Collections.Generic;
using System.Text;

namespace RecommendationEngine
{
    public class RateeBase 
    {
        public List<RaterBase> Likes { get; set; }
        public List<RaterBase> Dislikes { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
