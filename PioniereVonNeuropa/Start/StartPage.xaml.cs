using System.Windows;
using PioniereVonNeuropa.Debug;

namespace PioniereVonNeuropa.Start;

public partial class StartPage{
	public StartPage() {
		InitializeComponent();
	}

	private void OpenEditor(object sender, RoutedEventArgs e) {
		EditorWindow editorWindow = new EditorWindow();

		Hide();
		editorWindow.Show();
		editorWindow.Closed += (_, _) => Show();
	}

	private void OpenDebug(object sender, RoutedEventArgs e) {
		DebugWindow debugWindow = new DebugWindow();

		Hide();
		debugWindow.Show();
		debugWindow.Closed += (_, _) => Show();
	}
}