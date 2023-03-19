using System;

namespace PioniereVonNeuropa.GeneratePlayableGame;

public static class Generator{
	public static Game GenerateGame(Game game) {
		int Deserts  = 0;
		int Harbours = 0;
		bool brickHarbour = false,
			 oreHarbour   = false,
			 sheepHarbour = false,
			 wheatHarbour = false,
			 woodHarbour  = false;

		foreach (Tile tile in game.Tiles){
			if (tile.Resource == Resource.Land){
				Resource randomResource = GetRandomResource(game.Deserts > Deserts);
				tile.Resource = randomResource;

				if (randomResource == Resource.Desert)
					Deserts++;
			}
			else if (tile.Harbor){
				if (Harbours >= game.Harbours && new Random(DateTime.Now.Nanosecond).Next(2) == 0){
					tile.Harbor   = false;
					tile.Resource = Resource.Water;
				}
				else{
					Harbours++;
					switch (new Random(DateTime.Now.Nanosecond).Next(6)){
						case 0:
							if (!brickHarbour)
								tile.Resource = Resource.Brick;
							break;
						case 1:
							if (!sheepHarbour)
								tile.Resource = Resource.Sheep;
							break;
						case 2:
							if (!oreHarbour)
								tile.Resource = Resource.Ore;
							break;
						case 3:
							if (!wheatHarbour)
								tile.Resource = Resource.Wheat;
							break;
						case 4:
							if (!woodHarbour)
								tile.Resource = Resource.Wood;
							break;
					}
				}
			}
		}

		return game;
	}

	public static Resource GetRandomResource(bool desert) {
		Random random = new(DateTime.Now.Nanosecond);
		switch (random.Next(6), harbor: desert){
			case (0, _):
				return Resource.Brick;
			case (1, _):
				return Resource.Wood;
			case (2, _):
				return Resource.Wheat;
			case (3, _):
				return Resource.Sheep;
			case (4, _):
				return Resource.Ore;
			case (5, true):
				return Resource.Desert;
			case (5, false):
				return GetRandomResource(false);
			default:
				return Resource.None;
		}
	}
}