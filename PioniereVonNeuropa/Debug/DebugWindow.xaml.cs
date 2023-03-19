using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using PioniereVonNeuropa.GeneratePlayableGame;

namespace PioniereVonNeuropa.Debug;

public partial class DebugWindow{
	#region Einstellbare Werte

	private const           int    HexagonWidth = 140;
	private const           int    RoadWidth    = 12;
	private const           double NodeDiameter = HexagonWidth * 0.15;
	private static readonly Color  RoadColor    = Color.FromRgb(255, 105, 0);

	private readonly ImageBrush WoodImage   = new(new BitmapImage(new("E:/Bilder/anime/Wood.jpg", UriKind.Absolute)));
	private readonly ImageBrush WheatImage  = new(new BitmapImage(new("E:/Bilder/anime/Wheat.jpg", UriKind.Absolute)));
	private readonly ImageBrush BrickImage  = new(new BitmapImage(new("E:/Bilder/anime/Brick.jpg", UriKind.Absolute)));
	private readonly ImageBrush SheepImage  = new(new BitmapImage(new("E:/Bilder/anime/Sheep.jpg", UriKind.Absolute)));
	private readonly ImageBrush OreImage    = new(new BitmapImage(new("E:/Bilder/anime/Ore.jpg", UriKind.Absolute)));
	private readonly ImageBrush LandImage   = new(new BitmapImage(new("E:/Bilder/anime/Dirt.JPG", UriKind.Absolute)));
	private readonly ImageBrush DesertImage = new(new BitmapImage(new("E:/Bilder/anime/Desert.jpg", UriKind.Absolute)));
	private readonly ImageBrush HarbourImage = new(new BitmapImage(new("E:/Bilder/anime/Harbour.png", UriKind.Absolute)));

	#endregion

	#region Kalkulierte Werte

	private static readonly int HexagonRadius = (int)(HexagonWidth / Math.Sqrt(3));
	private static readonly int HexagonHeight = 2 * HexagonRadius;

	#endregion

	private Game    _game;
	private Line?[] _roads;
	private Grid[]  _hexes;


	public DebugWindow() {
		InitializeComponent();

		OpenFileDialog teest = new() { Filter = "json files (*.json)|*.json" };
		do{
			if (teest.ShowDialog() == true){
				break;
			}
		} while (true);


		_game = JsonSerializer.Deserialize<Game>(teest.OpenFile())!;

		_game = Generator.GenerateGame(_game);

		CreateField();
	}


	private void CreateField() {
		_hexes = new Grid[_game.Width * _game.Height];
		_roads = new Line[_game.Roads.Length];
		for (int y = 0; y < _game.Height; y++){
			for (int x = 0; x < _game.Width; x++){
				Grid hex = CreateHex(ref _game.Tiles[y * _game.Width + x]);

				Canvas.SetTop(hex, NodeDiameter + y * (HexagonHeight + RoadWidth) * 0.75);
				if (y % 2 == 0)
					Canvas.SetLeft(hex, NodeDiameter + x * (HexagonWidth + RoadWidth));
				else
					Canvas.SetLeft(
						hex, NodeDiameter + x * (HexagonWidth + RoadWidth) + (HexagonWidth + RoadWidth) * 0.5);


				_hexes[y * _game.Width + x] = hex;

				BoardCanvas.Children.Add(hex);
			}
		}

		for (int index = 0; index < _hexes.Length; index++){
			if (_game.Tiles[index].Resource == Resource.Water || _game.Tiles[index].Harbor)
				continue;

			ref Grid grid = ref _hexes[index];

			double left = Canvas.GetLeft(grid);
			double top  = Canvas.GetTop(grid);

			ref Tile tile = ref _game.Tiles[index];

			for (int r = 0; r < tile.Roads.Length; r++){
				if (_roads[_game.Roads[tile.Roads[r] - 1].ID - 1] != null)
					continue;

				Line road = CreateRoad(r);

				switch (r){
					case 0:
						Canvas.SetLeft(road, left + HexagonWidth * 0.5);
						Canvas.SetTop(road, top   - RoadWidth    * 0.5);
						break;
					case 1:
						Canvas.SetLeft(road, left + HexagonWidth + RoadWidth * 0.5);
						Canvas.SetTop(road, top   + HexagonHeight            * 0.25);
						break;
					case 2:
						Canvas.SetLeft(road, left + HexagonWidth  * 0.5);
						Canvas.SetTop(road, top   + HexagonHeight * 0.75 + RoadWidth * 0.5);
						break;
					case 3:
						Canvas.SetLeft(road, left - RoadWidth     * 0.5);
						Canvas.SetTop(road, top   + HexagonHeight * 0.75 + RoadWidth * 0.25);
						break;
					case 4:
						Canvas.SetLeft(road, left - RoadWidth     * 0.5);
						Canvas.SetTop(road, top   + HexagonHeight * 0.25);
						break;
					case 5:
						Canvas.SetLeft(road, left - RoadWidth * 0.5);
						Canvas.SetTop(road, top   - RoadWidth * 0.25);
						break;
				}

				BoardCanvas.Children.Add(road);
				_roads[_game.Roads[tile.Roads[r] - 1].ID - 1] = road;
			}
		}
	}

	private static Line CreateRoad(int r) {
		PointCollection points;
		switch ((r % 3)){
			case 0:
				points = new()
					{ new(0, 0), new(HexagonWidth * 0.5 + RoadWidth * 0.5, 0.25 * HexagonHeight + RoadWidth * 0.25) };
				break;
			case 1:
				points = new() { new(0, 0), new(0, HexagonHeight * 0.5) };
				break;
			case 2:
				points = new()
					{ new(0, 0.25 * HexagonHeight), new(HexagonWidth * 0.5 + RoadWidth * 0.5, 0 - RoadWidth * 0.25) };
				break;
			default:
				points = new() { new(0, 0), new(0, 0) };
				break;
		}

		return new() {
			X1              = points[0].X,
			Y1              = points[0].Y,
			X2              = points[1].X,
			Y2              = points[1].Y,
			StrokeThickness = RoadWidth,
			Stroke          = new SolidColorBrush(RoadColor)
		};
	}

	private Grid CreateHex(ref Tile gameTile) {
		Polygon hex = new() {
			Points = new() {
				new(HexagonWidth       * 0.5, 0),
				new(HexagonWidth, 0.25 * HexagonHeight),
				new(HexagonWidth, 0.75 * HexagonHeight),
				new(HexagonWidth       * 0.5, HexagonHeight),
				new(0, 0.75            * HexagonHeight),
				new(0, 0.25            * HexagonHeight)
			}
		};

		switch (gameTile.Resource, gameTile.Harbor){
			case (Resource.Wood, _):
				hex.Fill = WoodImage;
				break;
			case (Resource.Wheat, _):
				hex.Fill = WheatImage;
				break;
			case (Resource.Brick, _):
				hex.Fill = BrickImage;
				break;
			case (Resource.Ore, _):
				hex.Fill = OreImage;
				break;
			case (Resource.Sheep, _):
				hex.Fill = SheepImage;
				break;
			case (Resource.Water, _):
				hex.Fill = Brushes.Transparent;
				break;
			case (Resource.Land, _):
				hex.Fill = LandImage;
				break;
			case (Resource.Desert, _):
				hex.Fill = DesertImage;
				break;
			case (_, true):
				hex.Fill = HarbourImage;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		Label value = new() {
			Content             = gameTile.Value,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment   = VerticalAlignment.Center,
			FontSize            = HexagonHeight * 0.2,
			Background          = Brushes.GhostWhite
		};

		Grid grid = new();
		grid.Children.Add(hex);
		if (gameTile.Resource != Resource.Water && !gameTile.Harbor)
			grid.Children.Add(value);

		return grid;
	}

	private void DevcardClick(object sender, RoutedEventArgs e) {
		throw new NotImplementedException();
	}

	private void EndClick(object sender, RoutedEventArgs e) {
		throw new NotImplementedException();
	}

	private void TradeClick(object sender, RoutedEventArgs e) {
		throw new NotImplementedException();
	}

	private void CityClick(object sender, RoutedEventArgs e) {
		throw new NotImplementedException();
	}

	private void VillageClick(object sender, RoutedEventArgs e) {
		throw new NotImplementedException();
	}

	private void RoadClick(object sender, RoutedEventArgs e) {
		for (int index = 0; index < _roads.Length; index++){
			Line? road = _roads[index];
			if (road == null || _game.Roads[index].Player != 0)
				continue;

			road.MouseLeftButtonUp += (_, _) => { };
			road.MouseEnter += (_, _) => {
				road.Cursor = Cursors.Hand;
				road.Stroke = Brushes.GreenYellow;
			};
			road.MouseLeave += (_, _) => {
				road.Cursor = Cursors.Arrow;
				road.Stroke = new SolidColorBrush(RoadColor);
			};
		}
	}
}