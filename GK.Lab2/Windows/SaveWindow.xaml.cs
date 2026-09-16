using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GK.Lab2.Windows
{
    public partial class SaveWindow : Window, INotifyPropertyChanged
    {
        // --- NOTIFY  ---
        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify([CallerMemberName] string v = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));

        // --- SOURCE IMAGE ---
        private BitmapSource source;

        // --- QUALITY VALUE ---
        // limits
        public int MinQuality => 1;
        public int MaxQuality => 100;

        // values
        private int quality;
        public int Quality
        {
            get => quality;
            set
            {
                if (value < MinQuality || value > MaxQuality)
                {
                    MessageBox.Show($"Enter value between {MinQuality} and {MaxQuality}.");
                    return;
                }

                quality = value;
                Notify();
                Notify(nameof(QualityString));
            }
        }

        // string
        public string QualityString
        {
            get => Quality.ToString();
            set
            {
                if (int.TryParse(value, out int q))
                    Quality = q;
            }
        }

        // --- DIRECTORY ---
        public string InitialDirectory { get; private set; }

        // --- CONSTRUCTOR ---
        public SaveWindow(string dir, BitmapSource src)
        {
            InitializeComponent();
            InitialDirectory = dir;
            source = src;
            Quality = 80;
        }

        // --- CANCEL ---
        private void Cancel_Click(object sender, RoutedEventArgs e)
            => Close();

        // --- SAVE JPEG ---
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Title = "Save Image";
            dlg.InitialDirectory = InitialDirectory;
            dlg.Filter = "JPEG (*.jpg)|*.jpg";

            if (dlg.ShowDialog() != true) return;

            InitialDirectory = System.IO.Path.GetDirectoryName(dlg.FileName);

            var encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = Quality;
            encoder.Frames.Add(BitmapFrame.Create(source));

            using var fs = System.IO.File.Create(dlg.FileName);
            encoder.Save(fs);

            Close();
        }
    }
}
