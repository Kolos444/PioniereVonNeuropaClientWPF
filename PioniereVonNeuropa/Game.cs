using System.Text.Json;

namespace CatanTests;

public class Game{
	public Game(int width, int height) {
		Width  = width;
		Height = height;

		Tiles = new Tile[width * height];

		Nodes = new Node[Width * 2 * Height + Height * 2 + Width * 2];

		Roads = new Road[3 * Width * Height + 2 * Height + Width * 2 - 1];

		GenerateEmptyBoard();
	}

	public int    Width  { get; }
	public int    Height { get; }
	public Tile[] Tiles  { get; }
	public Node[] Nodes  { get; }
	public Road[] Roads  { get; }

	private void GenerateEmptyBoard() {
		int nodeId = 1;
		for (int y = 0; y < Height; y++){
			for (int x = 0; x < Width; x++){
				Tile tile = new(y * Width + x + 1);

				#region Tile generierung

				if (y % 2 == 0){
					if (y > 0){
						tile.Neighbours[0]                       = Tiles[(y - 1) * Width + x].ID; //Oben Rechts
						Tiles[(y - 1) * Width + x].Neighbours[3] = tile.ID;

						if (x > 0){
							tile.Neighbours[5] = Tiles[(y - 1) * Width + x - 1].ID; //Oben Links
							Tiles[(y - 1) * Width + x                  - 1].Neighbours[2] = tile.ID;
						}
					}

					if (x > 0){
						tile.Neighbours[4] = Tiles[y * Width + x - 1].ID; //links
						Tiles[y * Width + x                  - 1].Neighbours[1] = tile.ID;
					}
				}
				else{
					if (y > 0){
						if (x < Width - 1){
							tile.Neighbours[0] = Tiles[(y - 1) * Width + x + 1].ID; //Oben Rechts
							Tiles[(y - 1) * Width + x + 1].Neighbours[3] = tile.ID;
						}

						tile.Neighbours[5]                       = Tiles[(y - 1) * Width + x].ID; //Oben Links
						Tiles[(y - 1) * Width + x].Neighbours[2] = tile.ID;
					}


					if (x > 0){
						tile.Neighbours[4] = Tiles[y * Width + x - 1].ID; //Links
						Tiles[y * Width + x                  - 1].Neighbours[1] = tile.ID;
					}
				}

				#endregion

				#region Node generierung

				//Wenn es ein Gerade Reihe ist
				if (y % 2 == 0){
					//Wenn es das erste in der Reihe ist
					if (x == 0){
						Node nodeSouthWest = new Node(nodeId++);
						tile.Nodes[4] = nodeSouthWest.ID;
						Node nodeNorthWest = new Node(nodeId++);
						tile.Nodes[5] = nodeNorthWest.ID;

						nodeSouthWest.Tiles[0] = tile.ID;
						nodeNorthWest.Tiles[1] = tile.ID;

						Nodes[-1+nodeSouthWest.ID] = nodeSouthWest;
						Nodes[-1+nodeNorthWest.ID] = nodeNorthWest;
					}

					Node nodeNorth = new Node(nodeId++);
					tile.Nodes[0] = nodeNorth.ID;
					Node nodeNorthEast = new Node(nodeId++);
					tile.Nodes[1] = nodeNorthEast.ID;

					nodeNorth.Tiles[1]     = tile.ID;
					nodeNorthEast.Tiles[2] = tile.ID;


					if (x > 0){
						Nodes[-1+Tiles[tile.ID - 2].Nodes[1]].Tiles[1] = tile.ID;
						tile.Nodes[5]                               = Tiles[tile.ID - 2].Nodes[1];
					}

					if (y > 0){
						nodeNorthEast.Tiles[0] = Tiles[tile.ID - Width - 1].ID; //Oben

						if (x < Width - 1){
							nodeNorth.Tiles[0] = Tiles[tile.ID - Width - 1].ID; //Oben Rechts
						}

						if (x > 0){
							nodeNorth.Tiles[2] = Tiles[tile.ID - Width - 2].ID; //Oben links
						}

						Tiles[tile.ID - Width - 1].Nodes[3] = nodeNorthEast.ID;
						Tiles[tile.ID - Width - 1].Nodes[4] = nodeNorth.ID;

						Tiles[tile.ID - Width - 2].Nodes[2] = nodeNorth.ID;
					}

					Nodes[-1+nodeNorth.ID]     = nodeNorth;
					Nodes[-1+nodeNorthEast.ID] = nodeNorthEast;

					//Wenn es die letzte Reihe ist
					if (y == Height - 1){
						Node nodeSouth = new Node(nodeId++);
						tile.Nodes[3] = nodeSouth.ID;
						Node nodeSouthEast = new Node(nodeId++);
						tile.Nodes[2] = nodeSouthEast.ID;

						nodeSouth.Tiles[0]     = tile.ID;
						nodeSouthEast.Tiles[2] = tile.ID;

						//Wenn es nicht das letzte in der Reihe ist
						if (x > 0)
							tile.Nodes[4] = Tiles[tile.ID - 2].Nodes[2];

						Nodes[-1+nodeSouth.ID]     = nodeSouth;
						Nodes[-1+nodeSouthEast.ID] = nodeSouthEast;
					}
				}
				else{
					Node nodeNorthWest = new Node(nodeId++);
					tile.Nodes[5] = nodeNorthWest.ID;
					Node nodeNorth = new Node(nodeId++);
					tile.Nodes[0] = nodeNorth.ID;


					nodeNorth.Tiles[1]     = tile.ID; //Unten
					nodeNorthWest.Tiles[1] = tile.ID; //Unten Links

					tile.Nodes[5] = nodeNorthWest.ID;


					Tiles[tile.ID - Width - 1].Nodes[2] = nodeNorth.ID;
					Tiles[tile.ID - Width - 1].Nodes[3] = nodeNorthWest.ID;
					if (x < Width - 1)
						Tiles[tile.ID - Width].Nodes[4] = nodeNorth.ID;

					if (x > 0){
						nodeNorthWest.Tiles[2]      = tile.ID - 1; //Unten links
						Tiles[tile.ID - 2].Nodes[1] = nodeNorthWest.ID;
					}

					if (x < Width - 1){
						nodeNorth.Tiles[0] = Tiles[tile.ID - Width].ID; //Oben rechts
					}


					nodeNorth.Tiles[2]     = Tiles[tile.ID - Width - 1].ID; //Oben links
					nodeNorthWest.Tiles[0] = Tiles[tile.ID - Width - 1].ID; //Oben

					//Wenn es das Letzte in der Reihe ist
					if (x == Width - 1){
						Node nodeSouthEast = new Node(nodeId++);
						tile.Nodes[1] = nodeSouthEast.ID;
						Node nodeNorthEast = new Node(nodeId++);
						tile.Nodes[2] = nodeNorthEast.ID;

						nodeSouthEast.Tiles[2] = tile.ID;
						nodeNorthEast.Tiles[2] = tile.ID;

						Nodes[-1+nodeSouthEast.ID] = nodeSouthEast;
						Nodes[-1+nodeNorthEast.ID] = nodeNorthEast;
					}

					Nodes[-1+nodeNorth.ID]     = nodeNorth;
					Nodes[-1+nodeNorthWest.ID] = nodeNorthWest;


					//Wenn es die letzte Reihe ist
					if (y == Height - 1){
						Node nodeSouthWest = new Node(nodeId++);
						tile.Nodes[4] = nodeSouthWest.ID;
						Node nodeSouth = new Node(nodeId++);
						tile.Nodes[3] = nodeSouth.ID;

						nodeSouthWest.Tiles[0] = tile.ID;
						nodeSouth.Tiles[0]     = tile.ID;

						//Wenn es nicht das letzte in der Reihe ist
						if (x > 0){
							nodeSouthWest.Tiles[2]      = tile.ID - 1;
							Tiles[tile.ID - 2].Nodes[2] = nodeSouthWest.ID;
						}

						Nodes[-1+nodeSouthWest.ID] = nodeSouthWest;
						Nodes[-1+nodeSouth.ID]     = nodeSouth;
					}
				}

				#endregion

				Tiles[y * Width + x] = tile;
			}
		}

		int roadId = 1;
		for (int y = 0; y < Height; y++){
			for (int x = 0; x < Width; x++){
				Tile tile = Tiles[y * Width + x];

				#region Straßen generierung

				//Wenn es eine gerade Reihe ist (start ist 0)
				if (y % 2 == 0){
					//Wenn es die erste Reihe ist
					if (x == 0){
						Road southWest = new Road(roadId++) {
							Nodes = {
								[0] = tile.Nodes[4],
								[1] = tile.Nodes[3]
							}
						};

						Nodes[-1+tile.Nodes[4]].Roads[1] = southWest.ID;
						Nodes[-1+tile.Nodes[3]].Roads[2] = southWest.ID;

						Road west = new Road(roadId++) {
							Nodes = {
								[0] = tile.Nodes[5],
								[1] = tile.Nodes[4]
							}
						};

						Nodes[-1+tile.Nodes[5]].Roads[1] = west.ID;
						Nodes[-1+tile.Nodes[3]].Roads[0] = west.ID;

						tile.Roads[3] = southWest.ID;
						tile.Roads[4] = west.ID;


						Roads[-1+southWest.ID] = southWest;
						Roads[-1+west.ID]      = west;
					}

					Road northWest = new Road(roadId++) {
						Nodes = {
							[0] = tile.Nodes[0],
							[1] = tile.Nodes[5]
						}
					};
					Nodes[-1+tile.Nodes[0]].Roads[2] = northWest.ID;
					Nodes[-1+tile.Nodes[5]].Roads[0] = northWest.ID;
					Road northEast = new Road(roadId++) {
						Nodes = {
							[0] = tile.Nodes[0],
							[1] = tile.Nodes[1]
						}
					};
					Nodes[-1+tile.Nodes[0]].Roads[1] = northEast.ID;
					Nodes[-1+tile.Nodes[1]].Roads[2] = northEast.ID;
					Road east = new Road(roadId++) {
						Nodes = {
							[0] = tile.Nodes[1],
							[1] = tile.Nodes[2]
						}
					};
					Nodes[-1+tile.Nodes[1]].Roads[2] = east.ID;
					Nodes[-1+tile.Nodes[2]].Roads[0] = east.ID;

					tile.Roads[5] = northWest.ID;
					tile.Roads[0] = northEast.ID;
					tile.Roads[1] = east.ID;

					Roads[-1+northWest.ID] = northWest;
					Roads[-1+northEast.ID] = northEast;
					Roads[-1+east.ID]      = east;

					//Wenn es nicht die erste Reihe ist
					if (x > 0){
						tile.Roads[4] = northWest.ID - 1;
					}

					#region Oberen Tiles Roads zuweisen

					//Wenn es die Erste Reihe ist wird es geskippt
					if (y == 0)
						continue;

					Tiles[tile.ID - Width - 1].Roads[3] = northEast.ID;

					//Wenn es nicht die Erste Reihe ist
					if (x > 0){
						Tiles[tile.ID - Width - 2].Roads[2] = northWest.ID;
					}

					#endregion

					#region Untere Roads beim letzten Durchlauf

					if (y == Height - 1){
						//Wenn es nicht das Erste in der Reihe ist
						if (x > 0){
							Road southWest = new Road(roadId++) {
								Nodes = {
									[0] = tile.Nodes[4],
									[1] = tile.Nodes[3]
								}
							};
							Nodes[-1+tile.Nodes[4]].Roads[1] = southWest.ID;
							Nodes[-1+tile.Nodes[3]].Roads[2] = southWest.ID;

							tile.Roads[3] = southWest.ID;

							Roads[-1+southWest.ID] = southWest;
						}

						Road southEast = new Road(roadId++) {
							Nodes = {
								[0] = tile.Nodes[2],
								[1] = tile.Nodes[3]
							}
						};
						Nodes[-1+tile.Nodes[2]].Roads[2] = southEast.ID;
						Nodes[-1+tile.Nodes[3]].Roads[0] = southEast.ID;

						tile.Roads[2] = southEast.ID;

						Roads[-1+southEast.ID] = southEast;
					}

					#endregion
				}
				else{
					Road west = new Road(roadId++) {
						Nodes = {
							[0] = tile.Nodes[5],
							[1] = tile.Nodes[4]
						}
					};
					Nodes[-1+tile.Nodes[5]].Roads[1] = west.ID;
					Nodes[-1+tile.Nodes[4]].Roads[0] = west.ID;
					Road northWest = new Road(roadId++) {
						Nodes = {
							[0] = tile.Nodes[0],
							[1] = tile.Nodes[5]
						}
					};
					Nodes[-1+tile.Nodes[0]].Roads[2] = northWest.ID;
					Nodes[-1+tile.Nodes[5]].Roads[0] = northWest.ID;
					Road northEast = new Road(roadId++) {
						Nodes = {
							[0] = tile.Nodes[0],
							[1] = tile.Nodes[1]
						}
					};
					Nodes[-1+tile.Nodes[0]].Roads[1] = northEast.ID;
					Nodes[-1+tile.Nodes[1]].Roads[2] = northEast.ID;

					tile.Roads[4] = west.ID;
					tile.Roads[5] = northWest.ID;
					tile.Roads[0] = northEast.ID;
					tile.Roads[1] = northEast.ID + 1; //Spezialfall da wir dies immer definitiv wissen

					Roads[-1+west.ID]      = west;
					Roads[-1+northWest.ID] = northWest;
					Roads[-1+northEast.ID] = northEast;

					//Wenn es das Letzte in der Reihe ist
					if (x == Width - 1){
						Road east = new Road(roadId++) {
							Nodes = {
								[0] = tile.Nodes[1],
								[1] = tile.Nodes[2]
							}
						};
						Nodes[-1+tile.Nodes[1]].Roads[1] = east.ID;
						Nodes[-1+tile.Nodes[2]].Roads[0] = east.ID;
						Road southEast = new Road(roadId++) {
							Nodes = {
								[0] = tile.Nodes[2],
								[1] = tile.Nodes[3]
							}
						};
						Nodes[-1+tile.Nodes[2]].Roads[2] = southEast.ID;
						Nodes[-1+tile.Nodes[3]].Roads[0] = southEast.ID;

						tile.Roads[1] = east.ID;
						tile.Roads[2] = southEast.ID;

						Roads[-1+east.ID]      = east;
						Roads[-1+southEast.ID] = southEast;
					}

					#region Oberen Tiles Roads zuweisen

					Tiles[tile.ID - Width - 1].Roads[2] = northWest.ID;

					//Wenn es nicht das letzte in der Reihe ist
					if (x < Width - 1)
						Tiles[tile.ID - Width].Roads[3] = northEast.ID;

					#endregion

					#region Untere Roads zuweisen wenn es der Letzte Durchlauf ist

					if (y == Height - 1){
						Road southWest = new Road(roadId++) {
							Nodes = {
								[0] = tile.Nodes[4],
								[1] = tile.Nodes[3]
							}
						};
						Nodes[-1+tile.Nodes[4]].Roads[1] = southWest.ID;
						Nodes[-1+tile.Nodes[3]].Roads[2] = southWest.ID;

						tile.Roads[3] = southWest.ID;

						Roads[-1+southWest.ID] = southWest;

						//Wenn es nicht das Letzte in der Reihe ist
						if (x < Width - 1){
							Road southEast = new Road(roadId++) {
								Nodes = {
									[0] = tile.Nodes[2],
									[1] = tile.Nodes[3]
								}
							};
							Nodes[-1+tile.Nodes[2]].Roads[2] = southWest.ID;
							Nodes[-1+tile.Nodes[3]].Roads[0] = southWest.ID;

							tile.Roads[2]                 = southEast.ID;

							Roads[-1+southEast.ID] = southEast;
						}
					}

					#endregion
				}

				#endregion
			}
		}
	}
}