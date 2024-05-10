namespace DotNetCourse.Models
{
    public class RankingModel
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public int Like { get; set; }
        public int Dislike { get; set; }
        public string Type { get; set; }
        public int Popularity { get; set; }
        public string User { get; set; }

        public RankingModel() { 
            Popularity = Like *2 - Dislike;
        }
    }
}
