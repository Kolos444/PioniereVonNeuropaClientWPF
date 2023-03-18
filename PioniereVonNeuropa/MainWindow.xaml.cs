using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace PioniereVonNeuropa{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window{
		private        string path = "C:/Users/Luca/RiderProjects/CatanTests/CatanTests/bin/Debug/net7.0/board.json";
		private static int    hexagonWidth = 140;
		private        int    hexagonRadius;
		private        int    hexagonHeight;
		private        int    roadWidth    = 6;
		private        double nodeDiameter = hexagonWidth * 0.15;


		public MainWindow() {
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
			Game          = newGame;
			hexagonRadius = (int)(hexagonWidth / Math.Sqrt(3));
			hexagonHeight = 2 * hexagonRadius;
			for (int y = 0; y < Game.Height; y++){
				for (int x = 0; x < Game.Width; x++){
					Grid uiElement = CreateHex(ref Game.Tiles[y * Game.Width + x]);

					Canvas.SetTop(uiElement, nodeDiameter + y * (hexagonHeight + roadWidth) * 0.75);
					if (y % 2 == 0)
						Canvas.SetLeft(uiElement, nodeDiameter + x * (hexagonWidth + roadWidth));
					else
						Canvas.SetLeft(
							uiElement,
							nodeDiameter + x * (hexagonWidth + roadWidth) + (hexagonWidth + roadWidth) * 0.5);

					CanvasBoard.Children.Add(uiElement);
				}
			}
		}

		private Grid CreateHex(ref Tile tile) {
			Grid  grid = new Grid();
			Brush brush;
			brush = GetResourceBrush(tile.Resource);

			Polygon hex = new() {
				Points = new() {
					new(hexagonWidth       * 0.5, 0),
					new(hexagonWidth, 0.25 * hexagonHeight),
					new(hexagonWidth, 0.75 * hexagonHeight),
					new(hexagonWidth       * 0.5, hexagonHeight),
					new(0, 0.75            * hexagonHeight),
					new(0, 0.25            * hexagonHeight)
				},
				Fill = brush
			};

			Label value = new Label() {
				Content = tile.Value
			};
			value.HorizontalAlignment = HorizontalAlignment.Center;
			value.VerticalAlignment = VerticalAlignment.Center;

			int arrayAccesor = tile.ID - 1;
			grid.MouseLeftButtonDown += (sender, args) => {
				if (Game.Tiles[arrayAccesor].Resource == malt)
					return;

				if (malt == Resource.None){
					if(value.Content.ToString() == ComboBoxWert.Text)
						return;

					value.Content                  = ComboBoxWert.Text;
					Game.Tiles[arrayAccesor].Value = Convert.ToInt32(ComboBoxWert.Text);
					return;
				}

				hex.Fill                          = GetResourceBrush(malt);
				Game.Tiles[arrayAccesor].Resource = malt;
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
					throw new ArgumentOutOfRangeException("Resourcenvalue falsch");
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
						new(0, 0.5 * hexagonHeight),
					};
					break;
				case ROADDIRECTION.UpDown:
					points = new() {
						new(0, 0),
						new(hexagonWidth * 0.5, 0.25 * hexagonHeight),
					};
					break;
				case ROADDIRECTION.DownUp:
					points = new() {
						new(0, 0.25      * hexagonHeight),
						new(hexagonWidth * 0.5, 0),
					};
					break;
			}

			Line road = new() {
				X1              = points[0].X,
				Y1              = points[0].Y,
				X2              = points[1].X,
				Y2              = points[1].Y,
				StrokeThickness = roadWidth,
				Stroke          = Brushes.Black
			};


			return road;
		}

		private Ellipse CreateNode() {
			Ellipse node = new Ellipse() {
				Fill   = new SolidColorBrush(Color.FromArgb(130, 132, 55, 0)),
				Width  = nodeDiameter,
				Height = nodeDiameter,
				Stroke = Brushes.Black
			};


			return node;
		}

		private Resource malt  = Resource.Land;
		private bool     hafen = false;
		private Game     Game;

		private void ButtonLandClick(object sender, RoutedEventArgs e) {
			malt = Resource.Land;
		}

		private void ButtonWasserClick(object sender, RoutedEventArgs e) {
			malt = Resource.Water;
		}

		private void ButtonHafenClick(object sender, RoutedEventArgs e) {
			malt = Resource.Harbor;
		}

		private void ButtonDefinitiverHafenClick(object sender, RoutedEventArgs e) {
			malt = Resource.DefinitiveHarbor;
		}

		private void ButtonsSpeichernClick(object sender, RoutedEventArgs e) {
			SaveFileDialog test = new SaveFileDialog();
			test.Filter = "json files (*.json)|*.json";

			if (test.ShowDialog() == true){
				Stream stream = test.OpenFile();

				stream.Write(JsonSerializer.SerializeToUtf8Bytes(Game));
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
			malt = Resource.Brick;
		}

		private void ButtonWolleClick(object sender, RoutedEventArgs e) {
			malt = Resource.Sheep;
		}

		private void ButtonErzClick(object sender, RoutedEventArgs e) {
			malt = Resource.Ore;
		}

		private void ButtonWeizenClick(object sender, RoutedEventArgs e) {
			malt = Resource.Wheat;
		}

		private void ButtonHolzClick(object sender, RoutedEventArgs e) {
			malt = Resource.Wood;
		}

		private void ButtonWertClick(object sender, RoutedEventArgs e) {
			malt = Resource.None;
		}


		private void ButtonWuesteClick(object sender, RoutedEventArgs e) {
			malt = Resource.Desert;
		}
	}


	internal enum ROADDIRECTION{
		Vertical,
		UpDown,
		DownUp
	}
}