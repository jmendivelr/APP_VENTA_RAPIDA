namespace LoginApp.Maui.UserControls;

public partial class FlyoutHeaderControl : ContentView
{
	public FlyoutHeaderControl()
	{
		InitializeComponent();
		if (App.user != null)
		{
			lblUserName.Text =  App.user.FullName;
			lblUserEmail.Text =  App.user.Email;
		}
	}
}