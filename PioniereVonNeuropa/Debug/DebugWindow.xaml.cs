using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PioniereVonNeuropa.Debug;

public partial class DebugWindow{
	#region Einstellbare Werte

	private const int    HexagonWidth = 140;
	private const int    RoadWidth    = 12;
	private const double NodeDiameter = HexagonWidth * 0.15;

	#endregion

	#region Kalkulierte Werte

	private static readonly int HexagonRadius = (int)(HexagonWidth / Math.Sqrt(3));
	private static readonly int HexagonHeight = 2 * HexagonRadius;

	#endregion

	private Game _game;


	public DebugWindow() {
		InitializeComponent();

		_game = JsonSerializer.Deserialize<Game>(File.ReadAllBytes("C:/Users/Luca/Desktop/Vanilla Catan.json")) ??
				new Game(7, 7);

		CreateField();
	}

	private void CreateField() {
		Grid[]  hexes = new Grid[_game.Width * _game.Height];
		Line?[] lines = new Line[_game.Roads.Length];
		for (int y = 0; y < _game.Height; y++){
			for (int x = 0; x < _game.Width; x++){
				Grid hex = CreateHex(ref _game.Tiles[y * _game.Width + x]);

				Canvas.SetTop(hex, NodeDiameter + y * (HexagonHeight + RoadWidth) * 0.75);
				if (y % 2 == 0)
					Canvas.SetLeft(hex, NodeDiameter + x * (HexagonWidth + RoadWidth));
				else
					Canvas.SetLeft(
						hex,
						NodeDiameter + x * (HexagonWidth + RoadWidth) + (HexagonWidth + RoadWidth) * 0.5);

				hexes[y * _game.Width + x] = hex;

				BoardCanvas.Children.Add(hex);
			}
		}

		for (int index = 0; index < hexes.Length; index++){
			ref Grid grid = ref hexes[index];
			double   left = Canvas.GetLeft(grid);
			double   top  = Canvas.GetTop(grid);

			ref Tile tile = ref _game.Tiles[index];

			for (int r = 0; r < tile.Roads.Length; r++){
				if (lines[_game.Roads[tile.Roads[r] - 1].ID - 1] != null)
					continue;

				Line road = CreateRoad(r);

				switch (r){
					case 0:
						Canvas.SetLeft(road, left + HexagonWidth * 0.5 + RoadWidth * 0.5);
						Canvas.SetTop(road, top);
						break;
					case 1:
						Canvas.SetLeft(road, left + HexagonWidth + RoadWidth * 0.5);
						Canvas.SetTop(road, top   + HexagonHeight            * 0.25);
						break;
					case 2:
						Canvas.SetLeft(road, left + HexagonWidth  * 0.5  + RoadWidth * 0.5);
						Canvas.SetTop(road, top   + HexagonHeight * 0.75 + RoadWidth * 0.25);
						break;
					case 3:
						Canvas.SetLeft(road, left - RoadWidth     * 0.5);
						Canvas.SetTop(road, top   + HexagonHeight * 0.75 +RoadWidth *0.25);
						break;
					case 4:
						Canvas.SetLeft(road, left - RoadWidth     * 0.5);
						Canvas.SetTop(road, top   + HexagonHeight * 0.25);
						break;
					case 5:
						Canvas.SetLeft(road, left - RoadWidth * 0.5);
						Canvas.SetTop(road, top   + 0);
						break;
				}

				BoardCanvas.Children.Add(road);
				lines[_game.Roads[tile.Roads[r] - 1].ID - 1] = road;
			}
		}
	}

	private static Line CreateRoad(int r) {
		PointCollection points = (r % 3) switch {
									 0 => new() { new(0, 0), new(HexagonWidth * 0.5, 0.25 * HexagonHeight), },
									 1 => new() { new(0, 0), new(0, 0.5 * HexagonHeight), },
									 2 => new() { new(0, 0.25 * HexagonHeight), new(HexagonWidth * 0.5, 0), },
									 _ => new() { new(0, 0), new(0, 0) }
								 };

		return new() {
			X1              = points[0].X,
			Y1              = points[0].Y,
			X2              = points[1].X,
			Y2              = points[1].Y,
			StrokeThickness = RoadWidth,
			Stroke          = Brushes.Chocolate
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
			},
			Fill = Brushes.DarkGreen
		};

		Label value = new() {
			Content             = gameTile.Value,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment   = VerticalAlignment.Center,
			FontSize            = HexagonHeight * 0.2,
			Background          = Brushes.GhostWhite
		};

		Grid grid = new() { Children = { hex, value } };

		return grid;
	}
}