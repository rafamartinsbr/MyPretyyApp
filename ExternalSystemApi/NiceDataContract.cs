using System;

namespace ExternalSystemApi
{
	public class NiceDataContract
	{
		public NiceDataContract(int id, string value)
		{
			if (id < 1) throw new ArgumentNullException(nameof(value));

			Id = id;
			Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		public int Id { get; }
		public string Value { get; }
	}
}