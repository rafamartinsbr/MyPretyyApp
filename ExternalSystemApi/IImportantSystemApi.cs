using System.Threading.Tasks;

namespace ExternalSystemApi
{
	public interface IImportantSystemApi
	{
		Task PublishData(NiceDataContract dataToPublish);
	}
}