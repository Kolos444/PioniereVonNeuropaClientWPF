using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using PioniereVonNeuropaLibrary;
using Brushes = System.Windows.Media.Brushes;

namespace PioniereVonNeuropa.Debug;

public partial class DebugWindow{
	#region Einstellbare Werte

	private const           int    HexagonWidth = 100;
	private const           int    RoadWidth    = 6;
	private const           double NodeDiameter = HexagonWidth * 0.15;
	private static readonly Color  RoadColor    = Color.FromRgb(222, 180, 103);

	#endregion

	#region Kalkulierte Werte

	private static readonly int HexagonRadius = (int)(HexagonWidth / Math.Sqrt(3));
	private static readonly int HexagonHeight = 2 * HexagonRadius;

	#endregion

	private Game           Game;
	private Line?[]        Roads;
	private Grid[]         Hexes;
	private UIElement?[]   Nodes;
	private List<Player> Players;

	public DebugWindow() {
		InitializeComponent();

		Roads = Array.Empty<Line?>();
		Hexes = Array.Empty<Grid>();

		OpenFileDialog openFileDialog = new() { Filter = "json files (*.json)|*.json" };
		if (openFileDialog.ShowDialog() != true){
			Close();
		}

		Game = JsonSerializer.Deserialize<Game>(openFileDialog.OpenFile())!;
		Game.MakePlayable(Game);
		Players = new(Game.Settings.Players);
		CreateField();
	}


	private void CreateField() {
		Hexes = new Grid[Game.Width * Game.Height];
		for (int y = 0; y < Game.Height; y++){
			for (int x = 0; x < Game.Width; x++){
				Grid hex = CreateTileElement(y, x);
				Hexes[y * Game.Width + x] = hex;
				BoardCanvas.Children.Add(hex);
			}
		}

		Roads = new Line[Game.Roads.Length];
		Nodes = new UIElement[Game.Nodes.Length];
		for (int index = 0; index < Hexes.Length; index++){
			if (Game.Tiles[index].Resource == Resource.None || Game.Tiles[index].Harbour)
				continue;

			double left = Canvas.GetLeft(Hexes[index]);
			double top  = Canvas.GetTop(Hexes[index]);

			Tile tile = Game.Tiles[index];

			for (int r = 0; r < tile.Roads.Length; r++){
				if (Roads[Game.Roads[tile.Roads[r] - 1].ID - 1] != null)
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
				Roads[Game.Roads[tile.Roads[r] - 1].ID - 1] = road;
			}

			if (!tile.Land){
				continue;
			}

			for (int direction = 0; direction < tile.Nodes.Length; direction++){
				int nodeID = tile.Nodes[direction];
				if (Nodes[nodeID - 1] == null){
					Ellipse uiElement = CreateNodeUI(Game.Nodes[nodeID - 1]);

					Grid hex = Hexes[tile.ID - 1];

					Polygon polygon = hex.Children[0] as Polygon ??
									  throw new InvalidOperationException("hex.Children[0]");


					left = Canvas.GetLeft(hex);
					top  = Canvas.GetTop(hex);

					Canvas.SetLeft(uiElement, polygon.Points[direction].X + left - uiElement.Width *0.5);
					Canvas.SetTop(uiElement, polygon.Points[direction].Y  + top  - uiElement.Width *0.5);

					Nodes[nodeID - 1] = uiElement;
					BoardCanvas.Children.Add(uiElement);
				}
			}
		}
	}

	private Ellipse CreateNodeUI(Node gameNode) {
		Ellipse uiElement = new Ellipse() {
			Fill   = new SolidColorBrush(RoadColor),
			Width  = HexagonWidth * 0.2,
			Height = HexagonWidth * 0.2
		};

		return uiElement;
	}

	protected Grid CreateTileElement(int y, int x) {
		Grid hex = CreateHex(Game.Tiles[y * Game.Width + x]);

		Canvas.SetTop(hex, NodeDiameter + y * (HexagonHeight + RoadWidth) * 0.75);
		if (y % 2 == 0)
			Canvas.SetLeft(hex, NodeDiameter + x * (HexagonWidth + RoadWidth));
		else
			Canvas.SetLeft(
				hex, NodeDiameter + x * (HexagonWidth + RoadWidth) + (HexagonWidth + RoadWidth) * 0.5);
		return hex;
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

	private Grid CreateHex(Tile tile) {
		Grid grid = new();

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

		Label value = new() {
			Content             = tile.Value,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment   = VerticalAlignment.Center,
			FontSize            = HexagonHeight * 0.2,
			Background          = Brushes.GhostWhite
		};

		if (tile.Harbour){
			hex.Fill = FindResource("Hafen") as ImageBrush ??
					   throw new InvalidOperationException("Hafen Resource nicht gefunden");
		} else if (tile.Land){
			hex.Fill = GetLandBrush(tile.Resource);
		} else{
			hex.Fill = FindResource("Wasser") as ImageBrush ??
					   throw new InvalidOperationException("Wasser Resource nicht gefunden");
		}

		grid.Children.Add(hex);
		if (tile.Harbour)
			grid.Children.Add(CreateHarbourLines(tile));
		if (tile.Harbour && tile.Resource != Resource.None){
			value.Content = tile.Resource.ToString();
		}

		if (tile.Land || (tile.Harbour && tile.Resource != Resource.Desert))
			grid.Children.Add(value);

		return grid;
	}

	private UIElement CreateHarbourLines(Tile tile) {
		Grid grid = new Grid();

		for (int i = 0; i < tile.Nodes.Length; i++)
			if (tile.Nodes[i] != 0)
				grid.Children.Add(CreateHarbourLine(i));

		return grid;
	}

	private UIElement CreateHarbourLine(int direction) {
		Line line = new() {
			X1              = HexagonWidth * 0.5,
			Y1              = HexagonWidth * 0.5,
			Stroke          = new SolidColorBrush(Color.FromRgb(219, 148, 47)),
			StrokeThickness = RoadWidth * 1.2
		};

		switch (direction){
			case 0:
				line.X2 = HexagonWidth * 0.5;
				line.Y2 = 0;
				break;
			case 1:
				line.X2 = HexagonWidth;
				line.Y2 = HexagonHeight * 0.25;
				break;
			case 2:
				line.X2 = HexagonWidth;
				line.Y2 = HexagonHeight * 0.75;
				break;
			case 3:
				line.X2 = HexagonWidth * 0.5;
				line.Y2 = HexagonHeight;
				break;
			case 4:
				line.X2 = 0;
				line.Y2 = HexagonHeight * 0.75;
				break;
			case 5:
				line.X2 = 0;
				line.Y2 = HexagonHeight * 0.25;
				break;
		}

		return line;
	}

	private ImageBrush GetLandBrush(Resource resource) {
		ImageBrush brush;
		switch (resource){
			case Resource.None:
				brush = FindResource("Land") as ImageBrush ??
						throw new InvalidOperationException("Land Resource nicht gefunden");
				break;
			case Resource.Wood:
				brush = FindResource("Holz") as ImageBrush ??
						throw new InvalidOperationException("Holz Resource nicht gefunden");
				break;
			case Resource.Wheat:
				brush = FindResource("Weizen") as ImageBrush ??
						throw new InvalidOperationException("Weizen Resource nicht gefunden");
				break;
			case Resource.Sheep:
				brush = FindResource("Schaf") as ImageBrush ??
						throw new InvalidOperationException("Schaf Resource nicht gefunden");
				break;
			case Resource.Ore:
				brush = FindResource("Erz") as ImageBrush ??
						throw new InvalidOperationException("Erz Resource nicht gefunden");
				break;
			case Resource.Brick:
				brush = FindResource("Lehm") as ImageBrush ??
						throw new InvalidOperationException("Lehm Resource nicht gefunden");
				break;
			case Resource.Desert:
				brush = FindResource("Wueste") as ImageBrush ??
						throw new InvalidOperationException("Wueste Resource nicht gefunden");
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		return brush;
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
		if (!RoadActive){
			for (int index = 0; index < Roads.Length; index++){
				Line? road = Roads[index];
				if (road == null || Game.Roads[index].Player != 0)
					continue;

				int index1 = index;
				road.MouseLeftButtonUp += (_, _) => {
					if (!Game.MakeRoadPlayer(Game.Roads[index1], new(1,new() )))
						return;

					road.Fill = Brushes.Red;
					RoadClick(sender, e);
				};
				road.MouseEnter += (_, _) => {
					road.Cursor = Cursors.Hand;
					road.Stroke = Brushes.GreenYellow;
				};
				road.MouseLeave += (_, _) => {
					road.Cursor = Cursors.Arrow;
					road.Stroke = new SolidColorBrush(RoadColor);
				};
			}
		} else{
			foreach (Line? road in Roads){
				if (road ==null)
					continue;

				RemoveRoutedEventHandlers(road,Line.MouseUpEvent);
				RemoveRoutedEventHandlers(road,Line.MouseEnterEvent);
				RemoveRoutedEventHandlers(road,Line.MouseLeaveEvent);
			}
		}


		RoadActive = !RoadActive;
	}

	private bool RoadActive;

	public int CurrentPlayer { get; set; }

	public static void RemoveRoutedEventHandlers(UIElement element, RoutedEvent routedEvent)
	{
		// Get the EventHandlersStore instance which holds event handlers for the specified element.
		// The EventHandlersStore class is declared as internal.
		var eventHandlersStoreProperty = typeof(UIElement).GetProperty(
			"EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
		object eventHandlersStore = eventHandlersStoreProperty.GetValue(element, null);

		// If no event handlers are subscribed, eventHandlersStore will be null.
		// Credit: https://stackoverflow.com/a/16392387/1149773
		if (eventHandlersStore == null)
			return;

		// Invoke the GetRoutedEventHandlers method on the EventHandlersStore instance
		// for getting an array of the subscribed event handlers.
		var getRoutedEventHandlers = eventHandlersStore.GetType().GetMethod(
			"GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		var routedEventHandlers = (RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke(
			eventHandlersStore, new object[] { routedEvent });

		// Iteratively remove all routed event handlers from the element.
		foreach (var routedEventHandler in routedEventHandlers)
			element.RemoveHandler(routedEvent, routedEventHandler.Handler);
	}
}