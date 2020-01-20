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

namespace Sea_Bottle
{
    /// <summary>
    /// Allows easy access to images loaded from resources
    /// </summary>
    /// <remarks>
    /// Before using any property of this class <see cref="Initialize"/> must be called first
    /// </remarks>
    static class ImageResourcesManager
    {
        public static BitmapImage emptyCell { get; private set; } = new BitmapImage();
        public static BitmapImage miss { get; private set; } = new BitmapImage();
        public static BitmapImage hit { get; private set; } = new BitmapImage();
        public static BitmapImage destroyedShip { get; private set; } = new BitmapImage();
        public static BitmapImage shot { get; private set; } = new BitmapImage();
        public static BitmapImage soundIcon { get; private set; } = new BitmapImage();
        public static BitmapImage soundOffIcon { get; private set; } = new BitmapImage();


        const string emptyCellUri = "Resources/kotwica.jpg";
        const string missUri = "Resources/miss.jpg";
        const string hitUri = "Resources/hit.jpg";
        const string destroyedShipUri = "Resources/destroyedShip.jpg";
        const string shotUri = "Resources/shot.jpg";
        const string soundIconUri = "Resources/sound.png";
        const string soundOffIconUri = "Resources/sound_x.png";

        /// <summary>
        /// Loads all images from resurces and initializes <see cref="BitmapImage"/> for each one of them
        /// </summary>
        public static void Initialize()
        {
            InitBitmap(emptyCell, emptyCellUri);
            InitBitmap(miss, missUri);
            InitBitmap(hit, hitUri);
            InitBitmap(destroyedShip, destroyedShipUri);
            InitBitmap(shot, shotUri);
            InitBitmap(soundIcon, soundIconUri);
            InitBitmap(soundOffIcon, soundOffIconUri);
        }

        /// <summary>
        /// Initializes <see cref="BitmapImage"/> using provided uri of image
        /// </summary>
        /// <param name="bitmap">Bitmap to initailize</param>
        /// <param name="uri">Uri leading to image. Must be relative</param>
        /// <exception cref="UriFormatException">When <paramref name="uri"/> is not valid uri for <see cref="UriKind.Relative"/></exception>
        static void InitBitmap(BitmapImage bitmap, string uri)
        {
            if (bitmap == null) throw new ArgumentNullException(nameof(bitmap), $"{nameof(BitmapImage)} to initialize can't be null");
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(uri, UriKind.Relative);
            bitmap.EndInit();
        }
    }
}
