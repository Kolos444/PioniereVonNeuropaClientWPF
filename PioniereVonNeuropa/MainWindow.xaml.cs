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
		private static int    hexagonWidth = 90;
		private        int    hexagonRadius;
		private        int    hexagonHeight;
		private        int    roadWidth    = 6;
		private        double nodeDiameter = hexagonWidth * 0.15;


		public MainWindow() {
			InitializeComponent();

			CanvasBoard.Background = Brushes.Blue;

			Game game = new Game(6, 6);

			hexagonRadius = (int)(hexagonWidth / Math.Sqrt(3));
			hexagonHeight = 2 * hexagonRadius;
			for (int y = 0; y < game.Height; y++){
				for (int x = 0; x < game.Width; x++){
					Polygon uiElement = CreateHex(Brushes.Gray);

					Canvas.SetTop(uiElement, nodeDiameter + y * (hexagonHeight + roadWidth) * 0.75);
					if (y % 2 == 0)
						Canvas.SetLeft(uiElement, nodeDiameter + x * (hexagonWidth + roadWidth));
					else
						Canvas.SetLeft(uiElement, nodeDiameter + x * (hexagonWidth + roadWidth) + (hexagonWidth + roadWidth) * 0.5);

					CanvasBoard.Children.Add(uiElement);
				}
			}

			CanvasBoard.Children.Add(CreateNode());
		}

		private Polygon CreateHex(Brush brush) {
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


			return hex;
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
				Fill   = Brushes.Brown,
				Width  = nodeDiameter,
				Height = nodeDiameter,
			};


			return node;
		}
	}

	internal enum ROADDIRECTION{
		Vertical,
		UpDown,
		DownUp
	}
}