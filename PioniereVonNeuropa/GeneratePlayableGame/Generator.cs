using System;
using System.Collections.Generic;
using System.Linq;

namespace PioniereVonNeuropa.GeneratePlayableGame;

public static class Generator{
	private static Random Random = new(DateTime.Now.Nanosecond);
	private static bool   brick, sheep, ore, wheat, wood;

	public static Game GenerateGame(ref Game game) {
		brick = false;
		sheep = false;
		ore   = false;
		wheat = false;
		wood  = false;

		foreach (Tile gameTile in game.Tiles){
			if (!gameTile.Harbour)
				continue;

			if (gameTile.Resource == Resource.Brick){
				brick = true;
			}
			else if (gameTile.Resource == Resource.Wood){
				wood = true;
			}
			else if (gameTile.Resource == Resource.Wheat){
				wheat = true;
			}
			else if (gameTile.Resource == Resource.Ore){
				ore = true;
			}
			else if (gameTile.Resource == Resource.Sheep){
				sheep = true;
			}
		}

		int Deserts  = 0;
		int Harbours = 0;

		foreach (Tile tile in game.Tiles){
			if (tile.Harbour){
				if (game.MaxHarbour > Harbours){
					if (CreateHarbour(tile, game))
						Harbours++;
				}
				else{
					MakeWaterTile(tile);
				}
			}
			else if (tile.Resource == Resource.Land){
				CreateResourceTile(tile, game);
			}
			else{
				MakeWaterTile(tile);
			}
		}

		return game;
	}

	private static bool CreateHarbour(Tile tile, Game game) {
		foreach (int tileNeighbour in tile.Neighbours){
			if (tileNeighbour == 0){
				continue;
			}

			if (game.Tiles[tileNeighbour].Harbour){
				MakeWaterTile(tile);
				return false;
			}
		}

		MakeHarbourTile(tile, game);

		return true;
	}

	private static void MakeHarbourTile(Tile tile, Game game) {
		tile.Harbour = true;

		if (tile.Resource == Resource.None){
			int random = Random.Next(6);
			switch (random){
				case 0 when !brick:
					tile.Resource = Resource.Brick;
					brick         = true;
					break;
				case 1 when !sheep:
					tile.Resource = Resource.Sheep;
					sheep         = true;
					break;
				case 2 when !wood:
					tile.Resource = Resource.Wood;
					wood          = true;
					break;
				case 3 when !wheat:
					tile.Resource = Resource.Wheat;
					wheat         = true;
					break;
				case 4 when !ore:
					tile.Resource = Resource.Ore;
					ore           = true;
					break;
				default:
					tile.Resource = Resource.None;
					break;
			}
		}

		List<int> connections = new(2);
		foreach (int tileNeighbour in tile.Neighbours){
			if (tileNeighbour == 0 ||game.Tiles[tileNeighbour - 1].Harbour)
				continue;

			if (game.Tiles[tileNeighbour - 1].Resource == Resource.Water){
				continue;
			}

			connections.AddRange(from node in game.Tiles[tileNeighbour - 1].Nodes from tileNode in tile.Nodes where node == tileNode select node);
			break;
		}

		tile.HarbourConnections = connections.ToArray();
	}

	private static void CreateResourceTile(Tile tile, Game game) {
		int random = Random.Next(game.MaxBrick + game.MaxWood + game.MaxWheat + game.MaxOre + game.MaxSheep);

		if (random < game.MaxBrick){
			tile.Resource = Resource.Brick;
			game.MaxBrick--;
		}
		else if (random < game.MaxBrick + game.MaxWood){
			tile.Resource = Resource.Wood;
			game.MaxWood--;
		}
		else if (random < game.MaxBrick + game.MaxWood + game.MaxWheat){
			tile.Resource = Resource.Wheat;
			game.MaxWheat--;
		}
		else if (random < game.MaxBrick + game.MaxWood + game.MaxWheat + game.MaxOre){
			tile.Resource = Resource.Ore;
			game.MaxOre--;
		}
		else{
			tile.Resource = Resource.Sheep;
			game.MaxSheep--;
		}

		if (tile.Value is 0 or 1 or > 12){
			random = Random.Next(360);

			tile.Value = random switch {
							 < 10  => 2,
							 < 30  => 3,
							 < 60  => 4,
							 < 100 => 5,
							 < 150 => 6,
							 < 210 => 7,
							 < 260 => 8,
							 < 300 => 9,
							 < 330 => 10,
							 < 350 => 11,
							 _     => 12
						 };
		}
	}

	private static void MakeWaterTile(Tile tile) {
		tile.Resource = Resource.Water;
		tile.Harbour  = false;
		tile.Value    = 0;
	}
}