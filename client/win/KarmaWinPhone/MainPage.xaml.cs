namespace KarmaWinPhone
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows;
    using Facebook;
    using Facebook.Client;

    /// <summary>
    ///     Implements the MainPage class
    /// </summary>
    public partial class MainPage : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        ///     The current user
        /// </summary>
        private GraphUser currentUser;

        /// <summary>
        /// The progress is visible.
        /// </summary>
        private bool progressIsVisible;

        /// <summary>
        /// The progress text.
        /// </summary>
        private string progressText;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the MainPage class
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += this.MainPageLoaded;
            this.DataContext = this;
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the current user.
        /// </summary>
        public GraphUser CurrentUser
        {
            get
            {
                return this.currentUser;
            }

            set
            {
                if (value != this.currentUser)
                {
                    this.currentUser = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether progress is visible.
        /// </summary>
        public bool ProgressIsVisible
        {
            get
            {
                return this.progressIsVisible;
            }

            set
            {
                if (value != this.progressIsVisible)
                {
                    this.progressIsVisible = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the progress text.
        /// </summary>
        public string ProgressText
        {
            get
            {
                return this.progressText;
            }

            set
            {
                if (value != this.progressText)
                {
                    this.progressText = value;
                    this.OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">
        /// Name of the property.
        /// </param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Handles the event for user tapping on the login button
        /// </summary>
        /// <param name="sender">
        /// Sender object
        /// </param>
        /// <param name="e">
        /// Event args
        /// </param>
        private async void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            SessionStorage.Remove();

            FacebookSessionClient fb = new FacebookSessionClient(Constants.AppId);

            // TODO: to login with facebook app (frictionless) use this code
            fb.LoginWithApp(Constants.FbPermissions, Constants.FBCustomString);
            // for now I am getting an error about app's redirect URI not matching the registeration
            // I suspect because I dont yet have the "published" app id that facebook can verify.
            // make sure we use this method by publishing the app - and using manual flow for publishing.

            // to login with the browser use this code.
            // var session = await fb.LoginAsync(Constants.FbPermissions);
            // UpdatePage(session);
        }

        /// <summary>
        /// Logouts the click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The <see cref="RoutedEventArgs"/> instance containing the event data.
        /// </param>
        private void LogoutClick(object sender, RoutedEventArgs e)
        {
            SessionStorage.Remove();
            this.CurrentUser = null;
        }

        /// <summary>
        /// Handles the page load event
        /// </summary>
        /// <param name="sender">
        /// Sender object
        /// </param>
        /// <param name="e">
        /// Event args
        /// </param>
        private async void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            var session = SessionStorage.Load();
            UpdatePage(session);
        }

        private async void UpdatePage(FacebookSession session)
        {
            if (this.CurrentUser == null && null != session)
            {
                App.AccessToken = session.AccessToken;
                App.FacebookId = session.FacebookId;
                
                this.ExpiryText.Text = string.Format("Login expires on: {0}", session.Expires.ToString());
                this.ProgressText = "Fetching details from Facebook...";
                this.ProgressIsVisible = true;

                try
                {
                    var fb = new FacebookClient(session.AccessToken);

                    dynamic result = await fb.GetTaskAsync("me");
                    var user = new GraphUser(result);
                    user.ProfilePictureUrl = new Uri(string.Format("https://graph.facebook.com/{0}/picture?access_token={1}", user.Id, session.AccessToken));

                    this.CurrentUser = user;
                }
                catch (FacebookOAuthException exception)
                {
                    MessageBox.Show("Error fetching user data: " + exception.Message);
                }

                this.ProgressText = string.Empty;
                this.ProgressIsVisible = false;
            }
        }

        #endregion

        private void EnterClick(object sender, RoutedEventArgs e)
        {
            var session = SessionStorage.Load();
            if (null != session)
            {
                App.AccessToken = session.AccessToken;
                App.FacebookId = session.FacebookId;
                NavigationService.Navigate(new Uri("/BrowserPage.xaml", UriKind.Relative));
            }
        }
    }
}