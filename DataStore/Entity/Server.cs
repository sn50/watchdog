namespace DataStore.Entity
{
    public class Server
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public long Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{IpAddress}, {Name}";
        }
    }
}