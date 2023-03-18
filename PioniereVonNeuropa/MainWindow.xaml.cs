using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using CatanTests;

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


			Game = new Game(6, 6);

			hexagonRadius = (int)(hexagonWidth / Math.Sqrt(3));
			hexagonHeight = 2 * hexagonRadius;
			for (int y = 0; y < Game.Height; y++){
				for (int x = 0; x < Game.Width; x++){
					Polygon uiElement = CreateHex(ref Game.Tiles[y * Game.Width + x]);

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

		private Polygon CreateHex(ref Tile tile) {
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

			int arrayAccesor = tile.ID - 1;
			hex.MouseLeftButtonDown += (sender, args) => {
				if (Game.Tiles[arrayAccesor].Resource == malt)
					return;

				hex.Fill = GetResourceBrush(malt);
				Game.Tiles[arrayAccesor].Resource = malt;
			};

			return hex;
		}

		private static Brush GetResourceBrush(RESOURCE resource) {
			Brush brush;
			switch (resource){
				case RESOURCE.None:
					brush = Brushes.Blue;
					break;
				case RESOURCE.Wood:
					brush = Brushes.DarkGreen;
					break;
				case RESOURCE.Wheat:
					brush = Brushes.Yellow;
					break;
				case RESOURCE.Brick:
					brush = Brushes.OrangeRed;
					break;
				case RESOURCE.Ore:
					brush = Brushes.DarkGray;
					break;
				case RESOURCE.Sheep:
					brush = Brushes.LawnGreen;
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

		private RESOURCE malt  = RESOURCE.None;
		private bool     hafen = false;
		private Game     Game;

		private void ButtonLandClick(object sender, RoutedEventArgs e) {
			malt = RESOURCE.Wood;
		}

		private void ButtonWasserClick(object sender, RoutedEventArgs e) {
			malt = RESOURCE.None;
		}

		private void ButtonHafenClick(object sender, RoutedEventArgs e) {
			malt = RESOURCE.None;
		}

		private void ButtonDefinitiverHafenClick(object sender, RoutedEventArgs e) {
			malt = RESOURCE.None;
		}
	}


	internal enum ROADDIRECTION{
		Vertical,
		UpDown,
		DownUp
	}
}