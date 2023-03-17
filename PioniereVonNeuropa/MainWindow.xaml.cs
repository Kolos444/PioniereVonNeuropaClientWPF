using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace PioniereVonNeuropa{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window{
		public MainWindow() {
			InitializeComponent();

		}

		private Polygon CreateHex(int size) {
			Polygon hex = new() {
				Points = new() {
					new(size       * 0.5, 0),
					new(size, size * 0.3),
					new(size, size * 0.7),
					new(size       *0.5, size),
					new(0, size * 0.7),
					new(0, size * 0.3)

				},
				Fill = Brushes.Red
			};

			return hex;
		}
	}
}