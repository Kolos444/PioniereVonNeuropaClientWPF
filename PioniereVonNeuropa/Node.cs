namespace CatanTests;

public class Node{
	public Node(int id) {
		ID = id;

		Tiles    = new int[3];
		Roads    = new int[3];
		Building = BUILDING.None;
	}

	public int ID { get; }

	public BUILDING Building { get; }

	/// <summary>
	/// Von Oben im Uhrzeigersinn 0 - 2
	/// </summary>
	public int[] Tiles { get; }

	/// <summary>
	/// Von Oben im Uhrzeigersinn 0 - 2
	/// </summary>
	public int[] Roads { get; }
}