namespace MT5Wrapper
{
	public class ConnectionParams
	{
		public string Name { get; set; }
		private string _IP;
		public string IP {
			get => _IP;
			set => _IP = value?.Trim();
		}
		public ulong Login { get; set; }
		public string Password { get; set; }
		public uint ConnectionTimeout { get; set; } = 30000;
	}
}
