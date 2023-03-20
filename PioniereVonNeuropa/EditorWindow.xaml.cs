using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using PioniereVonNeuropaLibrary;
using Brushes = System.Windows.Media.Brushes;

namespace PioniereVonNeuropa{
	/// <summary>
	/// Interaction logic for EditorWindow.xaml
	/// </summary>
	public partial class EditorWindow{
		private const           int    HexagonWidth  = 100;
		private static readonly int    HexagonRadius = (int)(HexagonWidth / Math.Sqrt(3));
		private static readonly int    HexagonHeight = 2 * HexagonRadius;
		private const           int    RoadWidth     = 6;
		private const           double NodeDiameter  = HexagonWidth * 0.15;

		private Resource PaintResource = Resource.None;
		private Game?    _game;
		private bool     PaintHarbour = false;

		public EditorWindow() {
			InitializeComponent();

			CreateGame(7, 7);
		}

		private void CreateInstance() {
			Feldgroesse feldgroesse = new Feldgroesse();
			if (feldgroesse.ShowDialog() == true)
				CreateGame(feldgroesse.Width, feldgroesse.Height);
		}

		private void CreateGame(int width, int height) {
			CreateGame(new Game(width, height));
		}

		private void CreateGame(Game newGame) {
			_game = newGame;
			CanvasBoard.Children.Clear();

			ReadSettings(newGame.Settings);

			for (int y = 0; y < _game.Height; y++){
				for (int x = 0; x < _game.Width; x++){
					Grid uiElement = CreateHex(_game.Tiles[y * _game.Width + x]);

					Canvas.SetTop(uiElement, NodeDiameter + y * (HexagonHeight + RoadWidth) * 0.75);
					if (y % 2 == 0)
						Canvas.SetLeft(uiElement, NodeDiameter + x * (HexagonWidth + RoadWidth));
					else
						Canvas.SetLeft(
							uiElement,
							NodeDiameter + x * (HexagonWidth + RoadWidth) + (HexagonWidth + RoadWidth) * 0.5);

					CanvasBoard.Children.Add(uiElement);
				}
			}
		}

		private void ReadSettings(Settings settings) {
			TextBoxHarbours.Text = settings.HarbourSettings.Harbours.ToString();
			TextBoxDeserts.Text  = settings.Deserts.ToString();
		}

		private Grid CreateHex(Tile tile) {
			Grid grid = new Grid();

			Polygon hex = new() {
				Points = new() {
					new(HexagonWidth       * 0.5, 0),
					new(HexagonWidth, 0.25 * HexagonHeight),
					new(HexagonWidth, 0.75 * HexagonHeight),
					new(HexagonWidth       * 0.5, HexagonHeight),
					new(0, 0.75            * HexagonHeight),
					new(0, 0.25            * HexagonHeight)
				},
			};

			Label value = new() {
				Content    = tile.Value,
				Background = Brushes.Bisque
			};

			if (!tile.Land)
				value.Visibility = Visibility.Hidden;

			value.HorizontalAlignment = HorizontalAlignment.Center;
			value.VerticalAlignment   = VerticalAlignment.Center;

			if (tile.Harbour){
				hex.Fill = FindResource("Hafen") as ImageBrush ??
						   throw new InvalidOperationException("Hafen Resource nicht gefunden");
			} else if (tile.Land){
				hex.Fill = GetLandBrush(tile.Resource);
			} else{
				hex.Fill = FindResource("Wasser") as ImageBrush ??
						   throw new InvalidOperationException("Wasser Resource nicht gefunden");
			}

			grid.MouseLeftButtonUp += (_, _) => {
				if (PaintHarbour){
					hex.Fill = FindResource("Hafen") as ImageBrush ??
							   throw new InvalidOperationException("Hafen Resource nicht gefunden");
					Game.MakeHarbourTile(tile);
					value.Visibility = Visibility.Hidden;
				} else if (PaintLand){
					hex.Fill = GetLandBrush(PaintResource);
					Game.MakeLandTile(tile, PaintResource);
					value.Visibility = Visibility.Visible;
				} else if (PaintValue != 0 && tile.Land){
					value.Content = PaintValue;
					tile.Value    = PaintValue;
				} else{
					hex.Fill = FindResource("Wasser") as ImageBrush ??
							   throw new InvalidOperationException("Wasser Resource nicht gefunden");
					Game.MakeWaterTile(tile);
					value.Visibility = Visibility.Hidden;
				}
			};

			grid.Children.Add(hex);
			grid.Children.Add(value);
			return grid;
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


		private void ButtonLandClick(object sender, RoutedEventArgs e) {
			PaintHarbour = false;
			PaintLand    = true;
			PaintValue   = 0;

			PaintResource = Resource.None;
		}

		private void ButtonWasserClick(object sender, RoutedEventArgs e) {
			PaintHarbour = false;
			PaintLand    = false;
			PaintValue   = 0;

			PaintResource = Resource.None;
		}

		public bool PaintLand { get; set; }

		private void ButtonHafenClick(object sender, RoutedEventArgs e) {
			PaintHarbour = true;
			PaintLand    = true;
			PaintValue   = 0;

			PaintResource = Resource.None;
		}


		private void ButtonsSpeichernClick(object sender, RoutedEventArgs e) {
			SaveFileDialog test = new() {
				Filter = "json files (*.json)|*.json"
			};

			_game.Settings.HarbourSettings.Harbours = Convert.ToInt32(TextBoxHarbours.Text);
			_game.Settings.Deserts                  = Convert.ToInt32(TextBoxDeserts.Text);

			if (test.ShowDialog() == true){
				Stream stream = test.OpenFile();

				stream.Write(JsonSerializer.SerializeToUtf8Bytes(_game));
				stream.Close();
			}
		}

		private void ButtonsNeuClick(object sender, RoutedEventArgs e) {
			CreateInstance();
		}

		private void ButtonsLadenClick(object sender, RoutedEventArgs e) {
			OpenFileDialog fileDialog = new() {
				Filter = "json files (*.json)|*.json"
			};

			if (fileDialog.ShowDialog() == true){
				Stream stream = fileDialog.OpenFile();


				Game? newGame = JsonSerializer.Deserialize<Game>(stream);
				stream.Close();
				if (newGame == null){
					MessageBox.Show("Fehler beim Laden der Datei");
					return;
				}

				CreateGame(newGame);
			}
		}

		private void ButtonLehmClick(object sender, RoutedEventArgs e) {
			PaintHarbour = false;
			PaintLand    = true;
			PaintValue   = 0;

			PaintResource = Resource.Brick;
		}

		private void ButtonWolleClick(object sender, RoutedEventArgs e) {
			PaintHarbour = false;
			PaintLand    = true;
			PaintValue   = 0;

			PaintResource = Resource.Sheep;
		}

		private void ButtonErzClick(object sender, RoutedEventArgs e) {
			PaintHarbour = false;
			PaintLand    = true;
			PaintValue   = 0;

			PaintResource = Resource.Ore;
		}

		private void ButtonWeizenClick(object sender, RoutedEventArgs e) {
			PaintHarbour = false;
			PaintLand    = true;
			PaintValue   = 0;

			PaintResource = Resource.Wheat;
		}

		private void ButtonHolzClick(object sender, RoutedEventArgs e) {
			PaintHarbour = false;
			PaintLand    = true;
			PaintValue   = 0;

			PaintResource = Resource.Wood;
		}

		private void ButtonWertClick(object sender, RoutedEventArgs e) {
			PaintHarbour  = false;
			PaintLand     = false;
			PaintValue    = Convert.ToInt32(ComboBoxWert.Text);
			PaintResource = Resource.None;
		}

		public int PaintValue { get; set; }


		private void ButtonWuesteClick(object sender, RoutedEventArgs e) {
			PaintHarbour  = false;
			PaintLand     = false;
			PaintValue    = 0;
			PaintResource = Resource.Desert;
		}

		private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text

		private void OnlyTypeNumber(object sender, TextCompositionEventArgs e) {
			e.Handled = _regex.IsMatch(e.Text);
		}
	}
}