using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PioniereVonNeuropa;

public partial class Feldgroesse : Window{
	public new int Width = 7, Height = 7;
	public Feldgroesse() {
		InitializeComponent();

	}
	private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
	void TxtAnswer_OnPreviewTextInput(object sender, TextCompositionEventArgs e) {
		e.Handled = _regex.IsMatch(e.Text);
	}

	private void ButtonOKClick(object sender, RoutedEventArgs e) {
		Width = Convert.ToInt32(WidthBox.Text);
		Height = Convert.ToInt32(HeightBox.Text);

		DialogResult = true;
	}
}