namespace SimpleIMDB;

public class Program
{
	public static async Task Main()
	{
		SimpleIMDB sIMDB = new SimpleIMDB();

		Task serverTask = Task.Run(() => sIMDB.startListening());
		await Task.Delay(15000);
		//sIMDB.stopListening();
		await serverTask;
	}
}