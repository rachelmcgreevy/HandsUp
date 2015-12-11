using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;

namespace KinectImageViewer
{
    /// <summary>
    /// Interaction logic for FullscreenPics.xaml
    /// Full screen for pictures 
    /// </summary>
    public partial class FullscreenPics : Window
    {
        //Picture files
        private string[] picFiles;
        private int currentImg = 0;

        //Main window
        MainWindow main;

        //Timers
        private DispatcherTimer slide_timer = new DispatcherTimer();
        private DispatcherTimer buttonTimer = new DispatcherTimer();

        /// <summary>
        /// FullScreenPics constructor
        /// Images from mainWindow are passed into the full screen
        /// </summary>
        /// <param name="shownImg"></param>
        /// <param name="m"></param>
        /// <param name="pics"></param>
        public FullscreenPics(int shownImg, MainWindow m, string[] pics)
        {
            currentImg = shownImg;
            main = m;
            picFiles = pics;
            InitializeComponent();            
        }

        /// <summary>
        /// OnLoad method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoad(object sender, RoutedEventArgs e)
        {
            ShowCurrentImage();            
        }

        /// <summary>
        /// Navigates to previous picture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void previousBtn_Click(object sender, RoutedEventArgs e)
        {
            previous();
        }

        /// <summary>
        /// Navigates to next picture when button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextBtn_Click(object sender, System.EventArgs e)
        {
            next();
        }

        /// <summary>
        /// Navigates to previous image
        /// </summary>
        public void previous()
        {
            if (picFiles.Length > 0)
            {
                currentImg = currentImg == 0 ? picFiles.Length - 1 : --currentImg;
                ShowCurrentImage();

            }
        }

        /// <summary>
        /// Navigates to next image
        /// </summary>
        public void next()
        {
            if (picFiles.Length > 0)
            {
                currentImg = currentImg == picFiles.Length - 1 ? 0 : ++currentImg;
                ShowCurrentImage();
            }
        }

        /// <summary>
        /// Shows current image
        /// </summary>
        protected void ShowCurrentImage()
        {
            if (currentImg >= 0 && currentImg <= picFiles.Length - 1)
            {
                BitmapImage bm = new BitmapImage(new Uri(picFiles[currentImg], UriKind.RelativeOrAbsolute));
                FullscreenImageBox.Source = bm;
            }
        }

        /// <summary>
        /// Cycles through images on folder
        /// </summary>
        public void slideShow()
        {
            slide_timer.Interval = TimeSpan.FromMilliseconds(2000);
            slide_timer.Tick += nextImage_Timer;
            slide_timer.Start();
        }

        /// <summary>
        /// Goes to next image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextImage_Timer(object sender, EventArgs e)
        {
            next();
        }

        /// <summary>
        /// Exits fullScreen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit(object sender, System.EventArgs e)
        {
            main.setCurrentImg(currentImg);
            this.Close();
        }

        /// <summary>
        /// Exits fullScreen
        /// </summary>
        public void exit()
        {
            main.setCurrentImg(currentImg);
            this.Close();
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves next button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressNextButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over next button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressNextButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press next button when cursor is hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressNextButton(object sender, object e)
        {
            if (picFiles.Length > 0)
            {
                currentImg = currentImg == picFiles.Length - 1 ? 0 : ++currentImg;
                ShowCurrentImage();
            }
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves previous button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void previousBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressPrevButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over previous button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void previousBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressPrevButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press previous button when cursor is hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressPrevButton(object sender, object e)
        {
            if (picFiles.Length > 0)
            {
                currentImg = currentImg == 0 ? picFiles.Length - 1 : --currentImg;
                ShowCurrentImage();
            }
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves exit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exit_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressExitButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over exit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exit_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressExitButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press exit button when cursor hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressExitButton(object sender, object e)
        {
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
            main.setCurrentImg(currentImg);
            slide_timer.Stop();
            this.Close();
        }
    }
}
