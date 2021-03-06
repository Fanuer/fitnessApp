using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using fITNat.Services;
using fIT.WebApi.Client.Data.Models.Exceptions;
using System.Threading.Tasks;
using System.Threading;

namespace fITNat
{
    class Dialog_SignIn : DialogFragment
    {
        private EditText txtUsername;
        private EditText txtPassword;
        private Button btnSignIn;
        private OnOffService ooService;
        private ScheduleActivity scheduleActivity;
        private Guid userId;

        public event EventHandler<OnSignInEventArgs> onSignInComplete;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            ooService = new OnOffService();

            var view = inflater.Inflate(Resource.Layout.Dialog_sign_in, container, false);

            txtUsername = view.FindViewById<EditText>(Resource.Id.txtUsername);
            txtPassword = view.FindViewById<EditText>(Resource.Id.txtPassword);
            btnSignIn = view.FindViewById<Button>(Resource.Id.btnDialogSignIn);


            btnSignIn.Click += BtnSignIn_Click;

            return view;
        }

        private async void BtnSignIn_Click(object sender, EventArgs e)
        {
            try{
                userId = await ooService.SignIn(txtUsername.Text, txtPassword.Text);
                if(userId != new Guid())
                {
                    //User clicked the Login-Button
                    onSignInComplete.Invoke(this, new OnSignInEventArgs
                        (txtUsername.Text, txtPassword.Text, userId));
                    //Dialog will slide to the side and will disappear
                    this.Dismiss();
                    Console.WriteLine("Result");
                }
                else
                {
                    Console.WriteLine("Result falsch");
                    //Falsche Logindaten
                    //Rückmeldung an den Login-Dialog, dass die Kombination User+PW nicht passt
                    SignInFail();
                }
            }
            catch(ServerException ex)
            {
                Console.WriteLine("Login-Fehler(Server): " + ex.StackTrace);
                SignInFail();
            }
            catch(Exception exc)
            {
                Console.WriteLine("Login-Fehler: " + exc.StackTrace + "" + exc.GetType());
                SignInFail();
            }
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle); //Sets the title bar to invisible
            base.OnActivityCreated(savedInstanceState);
            Dialog.Window.Attributes.WindowAnimations = Resource.Style.dialog_animation; //sets the animation
        }

        public void SignInFail()
        {
            txtPassword.Text = "";
            //txtUsername.SetError("Logindaten falsch",null);
            txtPassword.SetError("Logindaten falsch", null);
        }
    }

    public class OnSignInEventArgs : EventArgs
    {
        private string username;
        private string password;
        private Guid userId;

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public Guid UserId
        {
            get { return userId; }
            set { userId = value; }
        }

        public OnSignInEventArgs(string username, string password, Guid userId) : base()
        {
            Username = username;
            Password = password;
            UserId = userId;
        }
    }
}
 