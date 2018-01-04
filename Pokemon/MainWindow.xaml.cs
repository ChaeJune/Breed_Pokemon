using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace Pokemon
{

    /// Made By Woojin Lim (www.github.com/IdeaBank)
    /// 
    /// Vector Illustration by Vecteezy (https://www.vecteezy.com)
    /// 

    public partial class MainWindow : Window
    {
        #region Basic variables
        public uint CurrentExp = 0;
        public byte CurrentPokemon = 0;

        public static string WebResourcePath = "http://github.com/IdeaBank/Breed_Pokemon/blob/master/Pokemon/Resource/";

        public static string DirectoryPath = "C:/Users/" + Environment.UserName + "/Appdata/Local/Pokemon";
        public readonly string DataDirectoryPath = DirectoryPath + "/Data/";
        public readonly string ResourcePath = DirectoryPath + "/Resource/";

        private const int Kilobyte = 1024;
        #endregion

        public MainWindow()
        {
            #region Download files

            DownloadFile(WebResourcePath+ "Pikachu_Basic.png", ResourcePath+ "Pikachu_Basic.png");
            DownloadFile(WebResourcePath+ "Bulbasaur_Basic.png", ResourcePath+ "Bulbasaur_Basic.png");
            DownloadFile(WebResourcePath+ "Charmander_Basic.png", ResourcePath+ "Charmander_Basic.png");
            DownloadFile(WebResourcePath+ "Squirtle_Basic.png", ResourcePath+ "Squirtle_Basic.png");

            DownloadFile(WebResourcePath + "Pikachu_Eating.png", ResourcePath + "Pikachu_Eating.png");
            DownloadFile(WebResourcePath + "Bulbasaur_Eating.png", ResourcePath + "Bulbasaur_Eating.png");
            DownloadFile(WebResourcePath + "Charmander_Eating.png", ResourcePath + "Charmander_Eating.png");
            DownloadFile(WebResourcePath + "Squirtle_Eating.png", ResourcePath + "Squirtle_Eating.png");

            DownloadFile(WebResourcePath + "Pokeball.png", ResourcePath + "Pokeball.png");
            DownloadFile(WebResourcePath + "Pokeball.ico", ResourcePath + "Pokeball.ico");

            #endregion 

            #region Get data file

            //get Data file

            Directory.CreateDirectory(DataDirectoryPath);

            //Experiment
            if (!File.Exists(DataDirectoryPath + "exp.txt"))
            {
                File.Create(DataDirectoryPath + "exp.txt");
            }
            else
            {
                string exp = File.ReadAllText(DataDirectoryPath + "exp.txt");

                try
                {
                    CurrentExp = uint.Parse(exp);
                }
                catch (FormatException)
                {
                    File.Create(DataDirectoryPath + "exp.txt");
                }

                if (!File.Exists(DataDirectoryPath + "exp.txt"))
                {
                    File.Create(DataDirectoryPath + "exp.txt");
                }
            }
            //Status
            if (!File.Exists(DataDirectoryPath + "pokemon.txt"))
            {
                File.Create(DataDirectoryPath + "pokemon.txt");
            }
            else
            {
                string status = File.ReadAllText(DataDirectoryPath + "pokemon.txt");
                try
                {
                    CurrentPokemon = byte.Parse(status);
                }
                catch (FormatException)
                {
                    File.Create(DataDirectoryPath + "pokemon.txt");
                }
            }

            #endregion

            #region Basic configuration

            InitializeComponent();

            System.Windows.Rect desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width - 25;
            this.Top = desktopWorkingArea.Bottom - this.Height - 25;

            this.Icon = new BitmapImage(new Uri(ResourcePath + "/Pokeball.ico", UriKind.Relative));

            AllowDrop = true;
            Topmost = true;
            AllowsTransparency = true;
            ShowInTaskbar = false;

            Background = Brushes.Transparent;
            WindowState = WindowState.Normal;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.None;

            SizeChanged += Size_Changed;
            Drop += Window_DragDrop;
            MouseLeftButtonDown += Window_MouseLeftButtonDown;

            Change_Pokemon_Image(ResourcePath + GetCurrentPokemon() + "_Basic.png");
            
            #endregion
        }


        #region Operations needed for MainWindow

        void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
            if (e.ClickCount == 2)
            {
                Window status = new PokemonPicker();
                status.Show();
            }
        }

        private void Window_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            foreach (string file in fileList)
            {
                string[] laststring = file.Split('\\');
                if (MessageBox.Show(Application.Current.MainWindow ?? throw new InvalidOperationException(),
                        laststring[laststring.Length - 1] + "가 삭제됩니다!", "Alert", MessageBoxButton.OKCancel,
                        MessageBoxImage.Asterisk) == MessageBoxResult.OK)
                {
                    if (File.Exists(file))
                    {
                        CurrentExp += uint.Parse(new System.IO.FileInfo(file).Length.ToString()) / (uint)Kilobyte;
                        File.Delete(file);
                    }

                    else if (Directory.Exists(file))
                    {
                        CurrentExp += getDirectorySize(file) / (uint)Kilobyte;
                        Directory.Delete(file, true);
                    }

                    Thread eat = new Thread(() => Animate_Pokemon_Eat());
                    eat.Start();
                }
            }
        }

        public void Size_Changed(object sender, EventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Normal;
        }

        private void Close(object sender, EventArgs e)
        {
            File.WriteAllText(DataDirectoryPath + "exp.txt", CurrentExp.ToString());
            File.WriteAllText(DataDirectoryPath + "pokemon.txt", CurrentPokemon.ToString());
        }
        #endregion

        #region Operations needed for runtime

        public void DownloadFile(string remoteFilename, string localFilename)
        {
            try
            {
                if (!File.Exists(localFilename))
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(remoteFilename, localFilename);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        public void Animate_Pokemon_Eat()
        {
            //1
            Dispatcher.Invoke(
                new Action(() =>
                {
                    Change_Pokemon_Image(ResourcePath + GetCurrentPokemon() + "_Eating.png");
                }),
                DispatcherPriority.ContextIdle);
            System.Threading.Thread.Sleep(500);
            Dispatcher.Invoke(
                new Action(() =>
                {
                    Change_Pokemon_Image(ResourcePath + GetCurrentPokemon() + "_Basic.png");
                }),
                DispatcherPriority.ContextIdle);

            System.Threading.Thread.Sleep(500);
            //2
            Dispatcher.Invoke(
                new Action(() =>
                {
                    Change_Pokemon_Image(ResourcePath + GetCurrentPokemon() + "_Eating.png");
                }),
                DispatcherPriority.ContextIdle);
            System.Threading.Thread.Sleep(500);
            Dispatcher.Invoke(
                new Action(() =>
                {
                    Change_Pokemon_Image(ResourcePath + GetCurrentPokemon() + "_Basic.png");
                }),
                DispatcherPriority.ContextIdle);
            System.Threading.Thread.Sleep(500);
            //3
            Dispatcher.Invoke(
                new Action(() =>
                {
                    Change_Pokemon_Image(ResourcePath + GetCurrentPokemon() + "_Eating.png");
                }),
                DispatcherPriority.ContextIdle);
            System.Threading.Thread.Sleep(500);
            Dispatcher.Invoke(
                new Action(() =>
                {
                    Change_Pokemon_Image(ResourcePath + GetCurrentPokemon() + "_Basic.png");
                }),
                DispatcherPriority.ContextIdle);
            System.Threading.Thread.Sleep(500);
            //4
            Dispatcher.Invoke(
                new Action(() =>
                {
                    Change_Pokemon_Image(ResourcePath + GetCurrentPokemon() + "_Eating.png");
                }),
                DispatcherPriority.ContextIdle);

            System.Threading.Thread.Sleep(500);
            Dispatcher.Invoke(
                new Action(() =>
                {
                    Change_Pokemon_Image(ResourcePath + GetCurrentPokemon() + "_Basic.png");
                }),
                DispatcherPriority.ContextIdle);
            System.Threading.Thread.Sleep(500);
            //5
            Dispatcher.Invoke(
                new Action(() =>
                {
                    Change_Pokemon_Image(ResourcePath + GetCurrentPokemon() + "_Eating.png");
                }),
                DispatcherPriority.ContextIdle);
            System.Threading.Thread.Sleep(500);
            Dispatcher.Invoke(
                new Action(() =>
                {
                    Change_Pokemon_Image(ResourcePath + GetCurrentPokemon() + "_Basic.png");
                }),
                DispatcherPriority.ContextIdle);
        }

        public string GetCurrentPokemon()
        {
            switch (CurrentPokemon)
            {
                case 0:
                    return "Pikachu";
                case 1:
                    return "Bulbasaur";
                case 2:
                    return "Charmander";
                case 3:
                    return "Squirtle";
                default:
                    throw new System.InvalidOperationException("Your data has been crashed!!");
            }
        }

        private uint getDirectorySize(string path)
        {
            uint size = 0;
            try
            {
                var files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        FileInfo finfo = new FileInfo(file);
                        size += uint.Parse(finfo.Length.ToString());
                    }
                }

                foreach (string dir in Directory.GetDirectories(path))
                    size += getDirectorySize(dir);
            }
            catch (NotSupportedException e)
            {
                Console.WriteLine(@"Unable to calculate folder size: {0}", e.Message);
            }
            return size;
        }

        public void Change_Pokemon_Image(string path)
        {
            try
            {
                Uri uri = new Uri(path, UriKind.Absolute);
                BitmapImage bitmap = new BitmapImage(uri);
                PokemonImage.Source = bitmap;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        #endregion
    }
}
