﻿using System;
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

namespace SeaBottle
{
    static class ImageResourcesManager
    {
        public static BitmapImage emptyCell { get; private set; } = new BitmapImage();
        public static BitmapImage miss { get; private set; } = new BitmapImage();
        public static BitmapImage hit { get; private set; } = new BitmapImage();
        public static BitmapImage destroyedShip { get; private set; } = new BitmapImage();
        public static BitmapImage shot { get; private set; } = new BitmapImage();

        const string emptyCellUri;
        const string missUri;
        const string hitUri;
        const string destroyedShipUri;
        const string shotUri;

        public static void Initialize()
        {
            InitBitmap(emptyCell, emptyCellUri);
            InitBitmap(miss, missUri);
            InitBitmap(hit, hitUri);
            InitBitmap(destroyedShip, destroyedShipUri);
            InitBitmap(shot, shotUri);
        }

        static void InitBitmap(BitmapImage bitmap, string uri)
        {
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(uri, UriKind.Relative);
            bitmap.EndInit();
        }
    }
}