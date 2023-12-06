using Newtonsoft.Json;

namespace DocuBot_Api.Models.RatingEngine_Models
{
    public class RatingCalculation
    {
        public int Id { get; set; }
        public string Criteria { get; set; }
        public int Cscore { get; set; }
    }

    public class RatingCalculationContainer
    {
        [JsonProperty("Rating Calculation")]
        public RatingCalculation[] RatingCalculation { get; set; }
    }
}
