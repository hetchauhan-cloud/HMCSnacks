namespace HMCSnacks.Models
{
    public class City
    {
        public int Id { get; set; }
        public string CityName { get; set; }
        public int StateId { get; set; }
        public State State { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
