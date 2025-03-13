using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace TeamUp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RegistryKey registryKey;
        Animations animations = new Animations();


        public struct WindowProperties
        {
            public bool firstLaunch {  get; set; }
            public bool toolbarHidden { get; set; }
            public bool isDragging { get; set; }
            public Point startPoint { get; set; }
        }

        public struct LocalAccount
        {
         
            public bool hasAccount { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string DateCreated { get; set; }

            public int CollaborationsMade { get; set; }
            public int CollaborationsJoined { get; set; }
            public float CollaborationPoints { get; set; }
            public float TotalLikes { get; set; }

            public int CollaborationsWeek { get; set; }
            public int GroupsJoinedWeek { get; set; }
            public int LikesWeek { get; set; }
            public int ContributionPointsWeek { get; set; }

            public string secretID { get; set; }
        }

        public enum WorkspaceUserType { CREATOR, ADMIN, EDITOR, VIEWER }
        public enum CurrentWindow { NONE, HOME, WORKSPACE, ESSAY, LOGIN, PROFILE, SETTINGS }
        
        public enum WorkspaceType { NONE, ESSAY, PROJECT, CODE }

        LocalAccount localAccount;
        WindowProperties windowProperties;
        WorkspaceType workspaceType;
        CurrentWindow currentWindow;
        private Timer receiveTimer;
        private Timer sendTimer;
        private System.Timers.Timer typingTimer;
        private bool isTyping = false;
        private bool isSending = false;
        private string workspaceName;

        public MainWindow()
        {
            InitializeComponent();
            LoadWindow();

            windowProperties.firstLaunch = true;
            typingTimer = new System.Timers.Timer(500);
            typingTimer.AutoReset = false; // Only fire once after typing stops
            typingTimer.Elapsed += TypingTimerElapsed;
        }

        private void TypingTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Console.WriteLine("Stopped typing.");
                isTyping = false;
                typingTimer.Stop(); // Ensure the timer is stopped
            });

        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            windowProperties.isDragging = true;
            windowProperties.startPoint = e.GetPosition(this);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (windowProperties.isDragging)
            {
                Point currentPoint = e.GetPosition(this);
                double diffX = currentPoint.X - windowProperties.startPoint.X;
                double diffY = currentPoint.Y - windowProperties.startPoint.Y;

                Left += diffX;
                Top += diffY;
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            windowProperties.isDragging = false;
        }

        private void LoadWindow()
        {
            windowProperties.firstLaunch = true;
            HomeButton_Click(null, null);
        }

        public async void MoveToolbar(bool hide)
        {
            if (hide)
            {
                // Slide toolbar out of view
                animations.ObjectSlider(ToolbarGrid, new Thickness(10, 7, 865, -8), new Thickness(-124, 7, 999, -8));
                await Task.Delay(600);
                animations.ObjectSlider(ButtonDesign, new Thickness(-122, 673, 0, 0), new Thickness(13, 673, 0, 0));
                animations.ObjectSlider(ShowButton, new Thickness(-112, 683, 0, 0), new Thickness(23, 683, 0, 0));

                windowProperties.toolbarHidden = true;
            }
            else
            {
                // Slide buttons into view
                animations.ObjectSlider(ButtonDesign, new Thickness(13, 673, 0, 0), new Thickness(-122, 673, 0, 0));
                animations.ObjectSlider(ShowButton, new Thickness(23, 683, 0, 0), new Thickness(-112, 683, 0, 0));
                await Task.Delay(600);
                animations.ObjectSlider(ToolbarGrid, new Thickness(-124, 7, 999, -8), new Thickness(10, 7, 865, -8));

                windowProperties.toolbarHidden = false;
            }
        }

        public void HideWindow(CurrentWindow window)
        {
            // Hide windows based on the current window type
            if (window == CurrentWindow.PROFILE) { MoveMyProfile(true); }
            else if (window == CurrentWindow.LOGIN) { MoveLogin(true); }
            else if (window == CurrentWindow.SETTINGS) { MoveSettings(true); }
            else if (window == CurrentWindow.HOME) { MoveHome(true); }
            else if (window == CurrentWindow.WORKSPACE) { MoveWorkspaces(true); }

            if (workspaceType != WorkspaceType.NONE && window == CurrentWindow.WORKSPACE)
            {
                HideWorkspace("", workspaceType);
            }
        }

        public void MoveLogin(bool Hide)
        {
            // Slide login menu in or out of view based on the hide parameter
            if (Hide)
            {
                animations.ObjectSlider(LoginBorder, new Thickness(71, 77, 60, 39), new Thickness(1038, 77, -886, 39));
                animations.ObjectSlider(BackgroundBlurLogin, new Thickness(264, 120, 231, 81), new Thickness(1231, 120, -715, 81));
                animations.ObjectSlider(LoginButtonBorder, new Thickness(346, 510, 477, 175), new Thickness(1313, 510, -469, 175));
                animations.ObjectSlider(RegisterBorder, new Thickness(516, 510, 307, 175), new Thickness(1483, 510, -639, 175));
                animations.ObjectSlider(UsernameBorder, new Thickness(384, 368, 351, 343), new Thickness(1351, 368, -595, 343));
                animations.ObjectSlider(PasswordBorder, new Thickness(384, 428, 351, 283), new Thickness(1351, 428, -595, 283));
                animations.ObjectSlider(AccountLabel, new Thickness(384, 210, 338, 480), new Thickness(1351, 210, -608, 480));
                animations.ObjectSlider(AccountLabelParagraph, new Thickness(418, 250, 372, 464), new Thickness(1385, 250, -574, 464));
            }
            else
            {
                animations.ObjectSlider(LoginBorder, new Thickness(1038, 77, -886, 39), new Thickness(71, 77, 60, 39));
                animations.ObjectSlider(BackgroundBlurLogin, new Thickness(1231, 120, -715, 81), new Thickness(264, 120, 231, 81));
                animations.ObjectSlider(LoginButtonBorder, new Thickness(1313, 510, -469, 175), new Thickness(346, 510, 477, 175));
                animations.ObjectSlider(RegisterBorder, new Thickness(1483, 510, -639, 175), new Thickness(516, 510, 307, 175));
                animations.ObjectSlider(UsernameBorder, new Thickness(1351, 368, -595, 343), new Thickness(384, 368, 351, 343));
                animations.ObjectSlider(PasswordBorder, new Thickness(1351, 428, -595, 283), new Thickness(384, 428, 351, 283));
                animations.ObjectSlider(AccountLabel, new Thickness(1351, 210, -608, 480), new Thickness(384, 186, 355, 519));
                animations.ObjectSlider(AccountLabelParagraph, new Thickness(1385, 250, -574, 464), new Thickness(418, 230, 392, 484));
            }
        }

        public void MoveMyProfile(bool Hide)
        {
            // Slide profile menu in or out of view based on the hide parameter
            if (Hide)
            {
                animations.ObjectSlider(MyProfileGrid, new Thickness(5, 11, 5, -1), new Thickness(1036, 11, -1026, -1));
            }
            else
            {
                animations.ObjectSlider(MyProfileGrid, new Thickness(1036, 11, -1026, -1), new Thickness(5, 11, 5, -1));
            }
        }

        public void MoveHome(bool hide)
        {
            // Slide home menu in or out of view based on the hide parameter
            if (hide)
            {
                animations.ObjectSlider(HomeGrid, new Thickness(10, 10, 0, -1), new Thickness(1005, 9, -995, 0));
            }
            else
            {
                animations.ObjectSlider(HomeGrid, new Thickness(1005, 9, -995, 0), new Thickness(10, 10, 0, -1));
            }
        }

        public void MoveSettings(bool Hide)
        {
            // Slide settings menu in or out of view based on the hide parameter
            if (Hide)
            {
                animations.ObjectSlider(SettingsGrid, new Thickness(0, 7, 0, 0), new Thickness(1050, 7, -1040, 2));
            }
            else
            {
                animations.ObjectSlider(SettingsGrid, new Thickness(1050, 7, -1040, 2), new Thickness(0, 7, 0, 0));
            }
        }

        bool firstTime = true;
        public void MoveWorkspaces(bool hide)
        {
            if(firstTime)
                FetchWorkspacesFromServer();

            firstTime = false;

            // Slide workspace menu in or out of view based on the hide parameter
            if (hide)
            {
                animations.ObjectSlider(SelectWorkspaceGrid, new Thickness(0, 0, 0, 0), new Thickness(1032, 0, -1022, 2));
            }
            else
            {
                animations.ObjectSlider(SelectWorkspaceGrid, new Thickness(1032, 0, -1022, 2), new Thickness(0, 0, 0, 0));
            }
        }

        public async void ShowWindow(CurrentWindow window)
        {
            // If it's the first launch, delay for animations not to overlap
            if (windowProperties.firstLaunch)
            {
                await Task.Delay(610);
            }

            windowProperties.firstLaunch = false;

            // If there's a workspace type and the current window is workspace, move out of workspace
            if (workspaceType != WorkspaceType.NONE && currentWindow == CurrentWindow.WORKSPACE)
            {
                MoveEssayWorkspace("", false);
            }
            else
            {
                // Show the selected window
                if (window == CurrentWindow.PROFILE) { MoveMyProfile(false); }
                else if (window == CurrentWindow.LOGIN) { MoveLogin(false); }
                else if (window == CurrentWindow.SETTINGS) { MoveSettings(false); }
                else if (window == CurrentWindow.HOME) { MoveHome(false); }
                else if (window == CurrentWindow.WORKSPACE) { MoveWorkspaces(false); }
            }

            // Hide the toolbar if it's not already hidden
            if (!windowProperties.toolbarHidden) { MoveToolbar(true); }
        }


        private void UpdateHomeStats()
        {
            // Update statistics for the home window
            HomeUsername.Content = localAccount.Username;
            DateCreated.Content = "Date Created: " + localAccount.DateCreated;
            CollabsMadeWeek.Content = "Collaborations made: " + localAccount.CollaborationsWeek;
            GroupsJoinedWeek.Content = "Workspaces joined: " + localAccount.GroupsJoinedWeek;
            ContributionPointsWeek.Content = "Contribution points: " + localAccount.ContributionPointsWeek;
            LikesWeek.Content = "Likes: " + localAccount.LikesWeek;
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            // Load home window on button click, or toggle hide if already on home window
            if (localAccount.hasAccount)
            {
                if (currentWindow != CurrentWindow.HOME)
                {
                    UpdateHomeStats();
                    HideWindow(currentWindow);
                    currentWindow = CurrentWindow.HOME;
                    ShowWindow(currentWindow);
                }
                else
                {
                    HideWindow(currentWindow);
                    currentWindow = CurrentWindow.NONE;
                }
            }
            else
            {
                if (currentWindow != CurrentWindow.LOGIN)
                {
                    HideWindow(currentWindow);
                    currentWindow = CurrentWindow.LOGIN;
                    ShowWindow(currentWindow);
                }
                else
                {
                    HideWindow(currentWindow);
                    currentWindow = CurrentWindow.NONE;
                }
            }
        }

        private void WorkspacesButton_Click(object sender, RoutedEventArgs e)
        {
            // Load workspaces window on button click, or toggle hide if already on workspaces window
            if (currentWindow != CurrentWindow.WORKSPACE)
            {
                HideWindow(currentWindow);
                currentWindow = CurrentWindow.WORKSPACE;
                ShowWindow(currentWindow);
            }
            else
            {
                HideWindow(currentWindow);
                currentWindow = CurrentWindow.NONE;
            }
        }

        private void UpdateProfileStats()
        {
            // Update statistics for the profile window
            Username.Content = localAccount.Username;
            DateCreated.Content = "Date Created: " + localAccount.DateCreated;
            CollabMade.Content = "Collaborations made: " + localAccount.CollaborationsMade;
            CollabJoined.Content = "Workspaces joined: " + localAccount.CollaborationsJoined;
            Likes.Content = "Likes: " + localAccount.TotalLikes;
            LikesWeek.Content = "Contribution points: " + localAccount.CollaborationPoints;
        }

        private void MyProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Load profile window on button click, or toggle hide if already on profile window
            if (localAccount.hasAccount)
            {
                if (currentWindow != CurrentWindow.PROFILE)
                {
                    HideWindow(currentWindow);
                    currentWindow = CurrentWindow.PROFILE;
                    UpdateProfileStats();
                    ShowWindow(currentWindow);
                }
                else
                {
                    HideWindow(currentWindow);
                    currentWindow = CurrentWindow.NONE;
                }
            }
            else
            {
                if (currentWindow != CurrentWindow.LOGIN)
                {
                    HideWindow(currentWindow);
                    currentWindow = CurrentWindow.LOGIN;
                    ShowWindow(currentWindow);
                }
                else
                {
                    HideWindow(currentWindow);
                    currentWindow = CurrentWindow.NONE;
                }
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Load settings window on button click, or toggle hide if already on settings window
            if (currentWindow != CurrentWindow.SETTINGS)
            {
                HideWindow(currentWindow);
                currentWindow = CurrentWindow.SETTINGS;
                ShowWindow(currentWindow);
            }
            else
            {
                HideWindow(currentWindow);
                currentWindow = CurrentWindow.NONE;
            }
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            // Register new account
            try
            {
                using (TcpClient client = new TcpClient("127.0.0.1", 54000))
                using (NetworkStream stream = client.GetStream())
                {
                    // Create login request JSON
                    var loginRequest = new
                    {
                        action = "REGISTER",
                        user = User1.Text,
                        pass = Pass1.Text,
                        DateCreated = DateTime.UtcNow.ToString("MM/dd/yyyy")
                    };

                    string jsonRequest = JsonConvert.SerializeObject(loginRequest);
                    byte[] requestData = Encoding.UTF8.GetBytes(jsonRequest + "\n");

                    // Send request to server
                    await stream.WriteAsync(requestData, 0, requestData.Length);
                    await stream.FlushAsync();

                    // Read response from server
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var serverResponse = JsonConvert.DeserializeObject<dynamic>(response);

                    if(serverResponse.status == "success"){

                        string secretID = serverResponse["userData"]["secretID"].ToString();
                        string dateCreated = serverResponse["userData"]["DateCreated"]?.ToString();
                        int collaborationsMade = serverResponse["userData"]["stats"]["CollaborationsMade"]?.ToObject<int>() ?? 0;
                        int collaborationsJoined = serverResponse["userData"]["stats"]["CollaborationsJoined"]?.ToObject<int>() ?? 0;
                        float collaborationPoints = serverResponse["userData"]["stats"]["CollaborationPoints"]?.ToObject<float>() ?? 0f;
                        float totalLikes = serverResponse["userData"]["stats"]["TotalLikes"]?.ToObject<float>() ?? 0f;

                        int collaborationsWeek = serverResponse["userData"]["stats"]["CollaborationsWeek"]?.ToObject<int>() ?? 0;
                        int groupsJoinedWeek = serverResponse["userData"]["stats"]["GroupsJoinedWeek"]?.ToObject<int>() ?? 0;
                        int likesWeek = serverResponse["userData"]["stats"]["LikesWeek"]?.ToObject<int>() ?? 0;
                        int contributionPointsWeek = serverResponse["userData"]["stats"]["ContributionPointsWeek"]?.ToObject<int>() ?? 0;

                        // Now set the local account with the received data
                        localAccount.Username = User1.Text;
                        localAccount.Password = Pass1.Text;
                        localAccount.DateCreated = dateCreated;
                        localAccount.secretID = secretID;
                        localAccount.hasAccount = true;

                        // Set all the stats
                        localAccount.CollaborationsMade = collaborationsMade;
                        localAccount.CollaborationsJoined = collaborationsJoined;
                        localAccount.CollaborationPoints = collaborationPoints;
                        localAccount.TotalLikes = totalLikes;
                        localAccount.CollaborationsWeek = collaborationsWeek;
                        localAccount.GroupsJoinedWeek = groupsJoinedWeek;
                        localAccount.LikesWeek = likesWeek;
                        localAccount.ContributionPointsWeek = contributionPointsWeek;

                        // Save to registry
                        registryKey.SetValue("Username", localAccount.Username);
                        registryKey.SetValue("Password", localAccount.Password);
                        registryKey.SetValue("DateCreated", localAccount.DateCreated);
                        registryKey.SetValue("hasAccount", localAccount.hasAccount);
                        registryKey.SetValue("secretID", localAccount.secretID);
                        registryKey.SetValue("CollaborationsMade", localAccount.CollaborationsMade);
                        registryKey.SetValue("CollaborationsJoined", localAccount.CollaborationsJoined);
                        registryKey.SetValue("CollaborationPoints", localAccount.CollaborationPoints);
                        registryKey.SetValue("TotalLikes", localAccount.TotalLikes);
                        registryKey.SetValue("CollaborationsWeek", localAccount.CollaborationsWeek);
                        registryKey.SetValue("GroupsJoinedWeek", localAccount.GroupsJoinedWeek);
                        registryKey.SetValue("LikesWeek", localAccount.LikesWeek);
                        registryKey.SetValue("ContributionPointsWeek", localAccount.ContributionPointsWeek);

                        // Hide login window and switch to profile window
                        MoveLogin(true);
                        await Task.Delay(200);
                        MyProfileButton_Click(MyProfileButton, null);
                    }
                    else
                    {
                        MessageBox.Show("Registration failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                }
            }
            catch (Exception ex)
            {
                //:)
            }
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            using (TcpClient client = new TcpClient("127.0.0.1", 54000))
            {
                NetworkStream stream = client.GetStream();

                var loginRequest = new
                {
                    action = "LOGIN",
                    user = User1.Text,
                    pass = Pass1.Text
                };

                // Send JSON request to server
                string requestJson = JsonConvert.SerializeObject(loginRequest);
                byte[] requestData = Encoding.UTF8.GetBytes(requestJson);
                stream.Write(requestData, 0, requestData.Length);

                // Read response
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (string.IsNullOrWhiteSpace(response))
                {
                    MessageBox.Show("Server response is empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var serverResponse = JsonConvert.DeserializeObject<JObject>(response);

                // Check if "status" key exists
                if (serverResponse["status"]?.ToString() == "success")
                {
                    string secretID = serverResponse["userData"]["secretID"]?.ToString() ;
                    string dateCreated = serverResponse["userData"]["DateCreated"]?.ToString() ;
                    Console.WriteLine("Response from server: " + response); // Debugging line

                    if (string.IsNullOrWhiteSpace(response))
                    {
                        MessageBox.Show("Server response is empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }


                    if (string.IsNullOrEmpty(secretID) || string.IsNullOrEmpty(dateCreated))
                    {
                        MessageBox.Show("Login response missing necessary data. secretID: " + secretID + " DateCreated: " + dateCreated,
                                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    int collaborationsMade = serverResponse["userData"]["stats"]["CollaborationsMade"]?.ToObject<int>() ?? 0;
                    int collaborationsJoined = serverResponse["userData"]["stats"]["CollaborationsJoined"]?.ToObject<int>() ?? 0;
                    float collaborationPoints = serverResponse["userData"]["stats"]["CollaborationPoints"]?.ToObject<float>() ?? 0f;
                    float totalLikes = serverResponse["userData"]["stats"]["TotalLikes"]?.ToObject<float>() ?? 0f;

                    int collaborationsWeek = serverResponse["userData"]["stats"]["CollaborationsWeek"]?.ToObject<int>() ?? 0;
                    int groupsJoinedWeek = serverResponse["userData"]["stats"]["GroupsJoinedWeek"]?.ToObject<int>() ?? 0;
                    int likesWeek = serverResponse["userData"]["stats"]["LikesWeek"]?.ToObject<int>() ?? 0;
                    int contributionPointsWeek = serverResponse["userData"]["stats"]["ContributionPointsWeek"]?.ToObject<int>() ?? 0;

                    localAccount.Username = User1.Text;
                    localAccount.Password = Pass1.Text;
                    localAccount.DateCreated = dateCreated;
                    localAccount.secretID = secretID;
                    localAccount.hasAccount = true;

                    localAccount.CollaborationsMade = collaborationsMade;
                    localAccount.CollaborationsJoined = collaborationsJoined;
                    localAccount.CollaborationPoints = collaborationPoints;
                    localAccount.TotalLikes = totalLikes;
                    localAccount.CollaborationsWeek = collaborationsWeek;
                    localAccount.GroupsJoinedWeek = groupsJoinedWeek;
                    localAccount.LikesWeek = likesWeek;
                    localAccount.ContributionPointsWeek = contributionPointsWeek;

                   
                    MoveLogin(true);
                    MoveMyProfile(false);
                    await Task.Delay(200);
                    MyProfileButton_Click(MyProfileButton, null);
                }
                else
                {
                    MessageBox.Show("Wrong name or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            // Show toolbar if user has an account
            if (localAccount.hasAccount)
            {
                MoveToolbar(false);
                windowProperties.toolbarHidden = false;
            }
            else
            {
                MessageBox.Show("Please Login before doing any other actions.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            // Close the application and set registry setting for topmost
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            // Minimize the application window
            WindowState = WindowState.Minimized;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Set window to be topmost
            Topmost = true;
            registryKey.SetValue("TopMost", true);
        }

        private void CheckBox_unChecked(object sender, RoutedEventArgs e)
        {
            // Unset window from being topmost
            Topmost = false;
            registryKey.SetValue("TopMost", false);
        }

        private void EraseData()
        {
            using (TcpClient client = new TcpClient("127.0.0.1", 54000))
            {
                NetworkStream stream = client.GetStream();

                if (registryKey != null)
                {
                    try
                    {
                        var eraseRequest = new
                        {
                            action = "ERASE_ACCOUNT",
                            secretID = localAccount.secretID,
                            user = localAccount.Username
                        };

                        Console.WriteLine($"Sending request with secretID: {localAccount.secretID}");


                        // Send JSON request to server
                        string requestJson = JsonConvert.SerializeObject(eraseRequest);
                        byte[] requestData = Encoding.UTF8.GetBytes(requestJson);
                        stream.Write(requestData, 0, requestData.Length);

                        // Read response
                        byte[] buffer = new byte[1024];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        if (response == "success")
                        {
                            registryKey = Registry.CurrentUser.OpenSubKey("Software\\TeamUps", true);
                            registryKey.DeleteValue("secretID");
                            MessageBox.Show("Your app data has been cleared, re-open the application.", "Completed.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            localAccount.hasAccount = false;
                        }
                        else if(response == "error")
                        {
                            Logout_Click(null, null);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());   
                    }
                }
            }
        }

        private void LogOutAccount()
        {
            // Log out current account by erasing saved login data
            if (registryKey != null)
            {
                registryKey = Registry.CurrentUser.OpenSubKey("Software\\TeamUps", true);
                registryKey.DeleteValue("secretID");
            }
        }

        private void ResetAppButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset application data by erasing local data
            MessageBoxResult result = MessageBox.Show("Erasing your app data will require you to login again, and your settings will be reset.", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Hand);

            if (result == MessageBoxResult.Yes)
            {
                EraseData();      
            }
        }

        private void EraseAccount_Click(object sender, RoutedEventArgs e)
        {
            // Prompt user for confirmation before erasing account data
            MessageBoxResult result = MessageBox.Show("Erasing your account data will remove ALL of your collaborations, stats, settings and more.", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Hand);
            if (result == MessageBoxResult.Yes)
            {
                EraseData();
                MessageBox.Show("Your account has been erased, re-open the application.", "Completed.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                MoveSettings(true);
                MoveLogin(false);
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Prompt user for confirmation before logging out
            MessageBoxResult result = MessageBox.Show("Are you sure you want to log out?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Hand);
            if (result == MessageBoxResult.Yes)
            {
                LogOutAccount();
                MessageBox.Show("You have been logged out.", "Completed.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                MoveMyProfile(true);
                MoveLogin(false);
                localAccount.hasAccount = false;
            }
        }

        private void MoveEssayWorkspace(string name, bool hide)
        {
            if (hide)
            {
                animations.ObjectSlider(EssayWorkspaceGrid, new Thickness(0, 0, 0, 0), new Thickness(1032, 0, -1022, 2));
            }
            else
            {
                animations.ObjectSlider(EssayWorkspaceGrid, new Thickness(1032, 0, -1022, 2), new Thickness(0, 0, 0, 0));
            }
        }


        private void MoveCodeWorkspace(string name, bool hide)
        {
            WorkspaceName.Content = name;
            if (hide)
            {
                animations.ObjectSlider(EssayWorkspaceGrid, new Thickness(0, 0, 0, 0), new Thickness(1032, 0, -1022, 2));
            }
            else
            {
                animations.ObjectSlider(EssayWorkspaceGrid, new Thickness(1032, 0, -1022, 2), new Thickness(0, 0, 0, 0));
            }
        }

        public async void ShowWorkspace(string name, WorkspaceType workspaceType)
        {
            // Show the workspace based on its type with a delay for animation
            await Task.Delay(600);
            if (workspaceType == WorkspaceType.ESSAY) { MoveEssayWorkspace(name, false); }
            else if (workspaceType == WorkspaceType.CODE) { MoveCodeWorkspace(name, false); }
            SetupRealTimeCollaboration(currentWorkspace.Name);
        }

        public void HideWorkspace(string name, WorkspaceType workspaceType)
        {

            // Hide the workspace based on its type
            if (workspaceType == WorkspaceType.ESSAY) { MoveEssayWorkspace(name, true); }
            else if (workspaceType == WorkspaceType.CODE) { MoveCodeWorkspace(name, true); }
        }

        private List<Workspace> workspaces = new List<Workspace>();
        private Workspace currentWorkspace;

        private async void CreateWorkspace(string name, WorkspaceType type)
        {
            var workspace = new Workspace { Name = name, Type = type };
            workspaces.Add(workspace);
            RefreshWorkspaceList();
            currentWorkspace = workspace;
            await SyncCurrentWorkspaceToServer();
            currentWorkspace = null;
        }

        private void RefreshWorkspaceList()
        {
            addedWorkspaces.Clear();
            Workspaces.Items.Clear();

            foreach (var workspace in workspaces)
            {
                string workspaceName = workspace.Name;

                if (!addedWorkspaces.Contains(workspaceName))
                {
                    addedWorkspaces.Add(workspaceName);

                    var item = new ListBoxItem { Content = workspace.Name, Tag = workspace };
                    item.Selected += Workspace_Selected;
                    Workspaces.Items.Add(item);
                }
            }
        }

        private async void Workspace_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is ListBoxItem item && item.Tag is Workspace workspace)
            {
                currentWorkspace = workspace;
                Name = item.Name;
                string content = await FetchWorkspaceContent(workspace.Name);

                // Show the workspace with the freshly loaded content
                HideWindow(currentWindow);
                ShowWorkspace(Name, workspace.Type);
                EssayTextBox.Text = content;
            }
        }

        private async Task<string> FetchWorkspaceContent(string workspaceName)
        {
            using (TcpClient client = new TcpClient("127.0.0.1", 54000))
            {
                NetworkStream stream = client.GetStream();

                try
                {
                    var requestData = new
                    {
                        action = "FETCH_WORKSPACE_CONTENT",
                        workspaceName = workspaceName
                    };

                    string requestJson = JsonConvert.SerializeObject(requestData);
                    byte[] request = Encoding.UTF8.GetBytes(requestJson);
                    stream.Write(request, 0, request.Length);

                    // Read response
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    var responseData = JsonConvert.DeserializeObject<JObject>(response);
                    if (responseData["status"].ToString() == "success")
                    {
                        string serverContent = responseData["content"].ToString();

                        if (!isTyping)
                        {
                            EssayTextBox.Dispatcher.Invoke(() =>
                            {
                                EssayTextBox.Text = serverContent;
                            });
                        }

                        return serverContent;
                    }
                    else
                    {
                        MessageBox.Show($"Error: {responseData["message"]}");
                        return string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return string.Empty;
                }
            }
        }

        private async Task SyncCurrentWorkspaceToServer()
        {
            if (currentWorkspace == null)
                return;

            using (TcpClient client = new TcpClient("127.0.0.1", 54000))
            {
                NetworkStream stream = client.GetStream();

                try
                {
                    // Ensure you're accessing UI elements on the UI thread
                    string content = string.Empty;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        content = EssayTextBox.Text;  // Access the TextBox content on the UI thread
                    });

                    var workspaceData = new
                    {
                        action = "SYNC_WORKSPACE",
                        name = currentWorkspace.Name,
                        content = content,
                        secretID = localAccount.secretID // Ensure the server knows the user identity
                    };

                    string requestJson = JsonConvert.SerializeObject(workspaceData);  // Serialize data to JSON
                    byte[] requestData = Encoding.UTF8.GetBytes(requestJson);  // Convert to byte array
                    stream.Write(requestData, 0, requestData.Length);  // Send data to server

                    // Read response from server
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    if (response == "success")
                    {
                    
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private HashSet<string> addedWorkspaces = new HashSet<string>(); 


        private async Task FetchWorkspacesFromServer()
        {
            using (TcpClient client = new TcpClient("127.0.0.1", 54000))
            {
                NetworkStream stream = client.GetStream();

                try
                {
                    var requestData = new
                    {
                        action = "FETCH_WORKSPACES"
                    };

                    string requestJson = JsonConvert.SerializeObject(requestData);
                    byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);
                    stream.Write(requestBytes, 0, requestBytes.Length);

                    // Read the server response
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    var responseData = JsonConvert.DeserializeObject<JObject>(response);
                    if (responseData["status"].ToString() == "success")
                    {
                        var workspacesL = responseData["workspaces"].ToObject<Dictionary<string, Workspace>>();

                        // Now update the UI with workspaces (this example assumes you have a UI element for displaying workspaces)
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // Clear the current list or workspace content
                            Workspaces.Items.Clear();

                            // Add workspaces to the UI
                            foreach (var workspaceL in workspacesL)
                            {
                                string workspaceName = workspaceL.Key;

                                // Prevent duplicates by checking if it exists in the 'addedWorkspaces' set, it would've re-added them after each show and hide.
                                if (!addedWorkspaces.Contains(workspaceName))
                                {
                                    addedWorkspaces.Add(workspaceName); 
                                    string content = workspaceL.Value.Name;

                                    // Create workspace UI element
                                    CreateWorkspace(workspaceL.Key, WorkspaceType.CODE); 
                                }
                            }
                        });
                    }
                    else
                    {
                        MessageBox.Show("Error fetching workspaces: " + responseData["message"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private void SetupRealTimeCollaboration(string workspace)
        {

            if (sendTimer == null)
            {
                sendTimer = new Timer(300);
                sendTimer.Elapsed += async (s, e) => await SyncCurrentWorkspaceToServer();
                sendTimer.AutoReset = false;
            }

            if (receiveTimer == null)
            {
                receiveTimer = new Timer(500);
                receiveTimer.Elapsed += async (s, e) => await FetchWorkspaceContent(workspace);
                receiveTimer.AutoReset = true;
                receiveTimer.Start();
                Console.WriteLine("Receive timer started.");
            }
        }

        private async void EssayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isTyping)
            {
                Console.WriteLine("Started typing."); // Debugging for conflict issues between two clients.
            }

            isTyping = true; 
            typingTimer.Stop(); 
            typingTimer.Start();

            // Send the updated content
            await SyncCurrentWorkspaceToServer();
        }

        public class Workspace
        {
            public string Name { get; set; }
            public WorkspaceType Type { get; set; }
        }

        private void GoBackWP_Click(object sender, RoutedEventArgs e)
        {
            HideWorkspace("", WorkspaceType.ESSAY);
            ShowWindow(currentWindow);
            workspaceType = WorkspaceType.NONE;
        }

        bool WShidden = true;
        private void CreateWorkspaceButon_Click(object sender, RoutedEventArgs e)
        {
            

            if (WShidden)
            {
                WorkspaceNameBox.Visibility = Visibility.Visible;
                EssayRadio.Visibility = Visibility.Visible;
                ProgRadio.Visibility = Visibility.Visible;
                WorkspaceNameBox.Visibility = Visibility.Visible;
                ProfileBorder_Copy2.Visibility = Visibility.Visible;
                WShidden = false;
            }
            else
            {
                WorkspaceNameBox.Visibility = Visibility.Hidden;
                EssayRadio.Visibility = Visibility.Hidden;
                ProgRadio.Visibility = Visibility.Hidden;
                WorkspaceNameBox.Visibility = Visibility.Hidden;
                ProfileBorder_Copy2.Visibility = Visibility.Hidden;

                WorkspaceType selectedWorkspace = WorkspaceType.NONE;
                if(ProgRadio.IsChecked == true)
                {
                    selectedWorkspace = WorkspaceType.CODE;
                }
                else
                {
                    selectedWorkspace = WorkspaceType.ESSAY;
                }
                CreateWorkspace(NameTextBoxWorkspace.Text, selectedWorkspace);

                WShidden = true;
            }
        }

    }
}
