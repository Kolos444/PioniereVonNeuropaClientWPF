using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace PioniereVonNeuropa{
	/// <summary>
	/// Interaction logic for EditorWindow.xaml
	/// </summary>
	public partial class EditorWindow{
		private const           int    HexagonWidth  = 140;
		private static readonly int    HexagonRadius = (int)(HexagonWidth / Math.Sqrt(3));
		private static readonly int    HexagonHeight = 2 * HexagonRadius;
		private const           int    RoadWidth     = 6;
		private const           double NodeDiameter  = HexagonWidth * 0.15;

		private Resource _malt = Resource.Land;
		private Game?    _game;

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
			CanvasBoard.Children.Clear();
			_game = newGame;
			for (int y = 0; y < _game.Height; y++){
				for (int x = 0; x < _game.Width; x++){
					Grid uiElement = CreateHex(ref _game.Tiles[y * _game.Width + x]);

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

		private Grid CreateHex(ref Tile tile) {
			Grid  grid  = new Grid();

			Polygon hex = new() {
				Points = new() {
					new(HexagonWidth       * 0.5, 0),
					new(HexagonWidth, 0.25 * HexagonHeight),
					new(HexagonWidth, 0.75 * HexagonHeight),
					new(HexagonWidth       * 0.5, HexagonHeight),
					new(0, 0.75            * HexagonHeight),
					new(0, 0.25            * HexagonHeight)
				},
				Fill = GetResourceBrush(tile.Resource)
			};

			Label value = new() {
				Content = tile.Value
			};
			value.HorizontalAlignment = HorizontalAlignment.Center;
			value.VerticalAlignment   = VerticalAlignment.Center;

			int arrayAccesor = tile.ID - 1;
			grid.MouseLeftButtonDown += (_, _) => {
				if (_game!.Tiles[arrayAccesor].Resource == _malt)
					return;

				if (_malt == Resource.None){
					if (value.Content.ToString() == ComboBoxWert.Text)
						return;

					value.Content                  = ComboBoxWert.Text;
					_game.Tiles[arrayAccesor].Value = Convert.ToInt32(ComboBoxWert.Text);
					return;
				}

				hex.Fill                          = GetResourceBrush(_malt);
				_game.Tiles[arrayAccesor].Resource = _malt;
			};
			grid.Children.Add(hex);
			grid.Children.Add(value);
			return grid;
		}

		private static Brush GetResourceBrush(Resource resource) {
			Brush brush;
			switch (resource){
				case Resource.None:
					brush = Brushes.Black;
					break;
				case Resource.Wood:
					brush = Brushes.DarkGreen;
					break;
				case Resource.Wheat:
					brush = Brushes.Yellow;
					break;
				case Resource.Brick:
					brush = Brushes.OrangeRed;
					break;
				case Resource.Ore:
					brush = Brushes.DarkGray;
					break;
				case Resource.Sheep:
					brush = Brushes.LawnGreen;
					break;
				case Resource.Water:
					brush = Brushes.Blue;
					break;
				case Resource.Harbor:
					brush = Brushes.Aqua;
					break;
				case Resource.DefinitiveHarbor:
					brush = Brushes.MediumAquamarine;
					break;
				case Resource.Land:
					brush = Brushes.Brown;
					break;
				case Resource.Desert:
					brush = Brushes.NavajoWhite;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(resource));
			}

			return brush;
		}


		private Line CreateRoad(ROADDIRECTION direction) {
			PointCollection points;

			switch (direction){
				default: //Eigentlich unnötig
				case ROADDIRECTION.Vertical:
					points = new() {
						new(0, 0),
						new(0, 0.5 * HexagonHeight),
					};
					break;
				case ROADDIRECTION.UpDown:
					points = new() {
						new(0, 0),
						new(HexagonWidth * 0.5, 0.25 * HexagonHeight),
					};
					break;
				case ROADDIRECTION.DownUp:
					points = new() {
						new(0, 0.25      * HexagonHeight),
						new(HexagonWidth * 0.5, 0),
					};
					break;
			}

			Line road = new() {
				X1              = points[0].X,
				Y1              = points[0].Y,
				X2              = points[1].X,
				Y2              = points[1].Y,
				StrokeThickness = RoadWidth,
				Stroke          = Brushes.Black
			};


			return road;
		}

		private Ellipse CreateNode() {
			Ellipse node = new Ellipse() {
				Fill   = new SolidColorBrush(Color.FromArgb(130, 132, 55, 0)),
				Width  = NodeDiameter,
				Height = NodeDiameter,
				Stroke = Brushes.Black
			};


			return node;
		}


		private void ButtonLandClick(object sender, RoutedEventArgs e) {
			_malt = Resource.Land;
		}

		private void ButtonWasserClick(object sender, RoutedEventArgs e) {
			_malt = Resource.Water;
		}

		private void ButtonHafenClick(object sender, RoutedEventArgs e) {
			_malt = Resource.Harbor;
		}

		private void ButtonDefinitiverHafenClick(object sender, RoutedEventArgs e) {
			_malt = Resource.DefinitiveHarbor;
		}

		private void ButtonsSpeichernClick(object sender, RoutedEventArgs e) {
			SaveFileDialog test = new SaveFileDialog();
			test.Filter = "json files (*.json)|*.json";

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
			OpenFileDialog fileDialog = new OpenFileDialog();

			fileDialog.Filter = "json files (*.json)|*.json";
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
			_malt = Resource.Brick;
		}

		private void ButtonWolleClick(object sender, RoutedEventArgs e) {
			_malt = Resource.Sheep;
		}

		private void ButtonErzClick(object sender, RoutedEventArgs e) {
			_malt = Resource.Ore;
		}

		private void ButtonWeizenClick(object sender, RoutedEventArgs e) {
			_malt = Resource.Wheat;
		}

		private void ButtonHolzClick(object sender, RoutedEventArgs e) {
			_malt = Resource.Wood;
		}

		private void ButtonWertClick(object sender, RoutedEventArgs e) {
			_malt = Resource.None;
		}


		private void ButtonWuesteClick(object sender, RoutedEventArgs e) {
			_malt = Resource.Desert;
		}
	}


	internal enum ROADDIRECTION{
		Vertical,
		UpDown,
		DownUp
	}
}