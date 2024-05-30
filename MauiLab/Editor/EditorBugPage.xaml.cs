namespace MauiLab.Editor;

public partial class EditorBugPage : ContentPage
{
	public EditorBugPage()
	{
		InitializeComponent();
	}

    private void SaveButtonClicked(object sender, EventArgs e)
    {
		ReadOnlyLabel.Text = ReadWriteEditor.Text;
    }
}