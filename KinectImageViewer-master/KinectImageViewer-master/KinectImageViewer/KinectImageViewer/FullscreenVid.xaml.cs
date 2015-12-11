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
    /// Interaction logic for FullscreenVid.xaml
    /// Class for making videos enter full screen mode
    /// </summary>
    public partial class FullscreenVid : Window
    {
        // Video fields
        private string[] vidFiles;
        private int currentVid = 0;
        private bool isPlaying = false;
        private double time = 0;
        private double volume = 0;
        private double speed = 0;
        private double maxTime = 0;
        private bool mediaOpened = false;

        MainWindow main;

        // Timers
        DispatcherTimer buttonTimer = new DispatcherTimer();
        DispatcherTimer videoTimer = new DispatcherTimer();

        /// <summary>
        /// Constructor sets fields
        /// </summary>
        /// <param name="vids"></param>
        /// <param name="shownVid"></param>
        /// <param name="playing"></param>
        /// <param name="timeIn"></param>
        /// <param name="volumeIn"></param>
        /// <param name="speedIn"></param>
        /// <param name="maxTimeIn"></param>
        /// <param name="opened"></param>
        /// <param name="m"></param>
        public FullscreenVid(string[] vids, int shownVid, bool playing, double timeIn, double volumeIn, double speedIn, double maxTimeIn, bool opened, MainWindow m)
        {
            main = m;
            vidFiles = vids;
            currentVid = shownVid;
            InitializeComponent();
            time = timeIn;
            volume = volumeIn;
            speed = speedIn;
            maxTime = maxTimeIn;
            mediaOpened = opened;     
            isPlaying = playing;

            myVideoElement.Source = new Uri(vidFiles[currentVid], UriKind.Absolute);
            if(isPlaying)
            {
                myVideoElement.Play();
            }

        }

        /// <summary>
        /// Shows current video
        /// </summary>
        protected void ShowCurrentVideo()
        {
            if (currentVid >= 0 && currentVid <= vidFiles.Length - 1)
            {
                myVideoElement.Source = new Uri(vidFiles[currentVid], UriKind.Absolute);
            }
        }

        /// <summary>
        /// Goes to previous video when previous button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void previousBtn_Click(object sender, RoutedEventArgs e)
        {
            myVideoElement.Stop();
            isPlaying = false;
            if (vidFiles.Length > 0)
            {
                currentVid = currentVid == 0 ? vidFiles.Length - 1 : --currentVid;
                ShowCurrentVideo();
            }
        }

        /// <summary>
        /// Goes to next video when next button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextBtn_Click(object sender, System.EventArgs e)
        {
            next();
        }

        /// <summary>
        /// Shows next video
        /// </summary>
        public void next()
        {
            myVideoElement.Stop();
            isPlaying = false;
            if (vidFiles.Length > 0)
            {
                currentVid = currentVid == vidFiles.Length - 1 ? 0 : ++currentVid;

                ShowCurrentVideo();
            }
        }

        /// <summary>
        /// Plays video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void OnMouseDownPlayMedia(object sender, MouseButtonEventArgs args)
        {
            // Plays video if not currently playing
            // Resumes playing if it has been paused
            // This will have no effect if the video is already running
            myVideoElement.Play();
            isPlaying = true;

            // Initialises the videoElement property values.
            InitializePropertyValues();

        }

        /// <summary>
        /// Pauses video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void OnMouseDownPauseMedia(object sender, MouseButtonEventArgs args)
        {
            myVideoElement.Pause();
            isPlaying = false;
        }

        /// <summary>
        /// Stops video from playing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
 
        void OnMouseDownStopMedia(object sender, MouseButtonEventArgs args)
        {

            // The Stop method stops and resets the media to be played from 
            // the beginning.
            myVideoElement.Stop();
            isPlaying = false;

        }

        /// <summary>
        /// Changes the volume of the video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
 
        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            try
            {
                myVideoElement.Volume = (double)volumeSlider.Value;
            }
            catch (NullReferenceException n)
            {
                Console.WriteLine("Error: " + n);
            }
        }


        /// <summary>
        /// Changes the speed of the video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
 
        private void ChangeMediaSpeedRatio(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            myVideoElement.SpeedRatio = (double)speedRatioSlider.Value;
        }

        /// <summary>
        /// Initialises the timelineSlider's maximum value to the length of video clip
        /// in miliseconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Element_MediaOpened(object sender, EventArgs e)
        {
            if (isPlaying)
            {
                myVideoElement.Pause();
            }
            if (mediaOpened)
            {
                timelineSlider.Maximum = maxTime;
                timelineSlider.Value = time;
                speedRatioSlider.Value = speed;
                volumeSlider.Value = volume;
                InitializePropertyValues();
                int intTime = (int)time;
                TimeSpan ts = new TimeSpan(0, 0, 0, 0, intTime);
                myVideoElement.Position = ts;
            }
            else if (!mediaOpened)
            {
                InitializePropertyValues();
                timelineSlider.Maximum = myVideoElement.NaturalDuration.TimeSpan.TotalMilliseconds;               
            }                
            videoTimer.Interval = TimeSpan.FromMilliseconds(1);
            videoTimer.Tick += timeSlider_Tick;
            videoTimer.Start();
            if (isPlaying)
            {
                myVideoElement.Play();
            }
        }

        /// <summary>
        /// When video finishes it stops it and starts again from the beginning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Element_MediaEnded(object sender, EventArgs e)
        {
            myVideoElement.Stop();
        }

        /// <summary>
        /// Seeks to a different part of the video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SeekToMediaPosition(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            if(!isPlaying)
            {
                int SliderValue = (int)timelineSlider.Value;

                // Overloaded constructor takes the arguments days, hours, minutes, seconds, miniseconds. 
                // Create a TimeSpan with miliseconds equal to the slider value.
                TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
                myVideoElement.Position = ts;
            }

        }

        /// <summary>
        /// Updates the timelineSlider value (the duration of the video)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timeSlider_Tick(object sender, object e)
        {
            timelineSlider.Value = myVideoElement.Position.TotalMilliseconds;
        }

        /// <summary>
        /// Initialises property values
        /// </summary>
        void InitializePropertyValues()
        {
            //Sets the volume and speed to their respective sliders
            myVideoElement.Volume = (double)volumeSlider.Value;
            myVideoElement.SpeedRatio = (double)speedRatioSlider.Value;

        }

        /// <summary>
        /// Exits full screen video mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit(object sender, System.EventArgs e)
        {
            main.closeFullVid(currentVid, isPlaying, myVideoElement.Position.TotalMilliseconds, volumeSlider.Value, speedRatioSlider.Value, timelineSlider.Maximum, mediaOpened);
            this.Close();
        }
        
        /// <summary>
        /// Exits full screen video mode
        /// </summary>
        public void exit()
        {
            main.closeFullVid(currentVid, isPlaying, myVideoElement.Position.TotalMilliseconds, volumeSlider.Value, speedRatioSlider.Value, timelineSlider.Maximum, mediaOpened);
            this.Close();
        }

        //Code for Mouse

        /// <summary>
        /// Changes cursor to hand when mouse leaves vidNext button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void vidNext_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressVidNextButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over vidNext button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void vidNext_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressVidNextButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press vidNext button when cursor hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressVidNextButton(object sender, object e)
        {
            myVideoElement.Stop();
            isPlaying = false;
            if (vidFiles.Length > 0)
            {
                currentVid = currentVid == vidFiles.Length - 1 ? 0 : ++currentVid;

                ShowCurrentVideo();
            }
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// hanges cursor to hand when mouse leaves vidPrev button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void vidPrev_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressVidPrevButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over vidPrev button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void vidPrev_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressVidPrevButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press vidPrev button when cursor hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressVidPrevButton(object sender, object e)
        {
            myVideoElement.Stop();
            isPlaying = false;
            if (vidFiles.Length > 0)
            {
                currentVid = currentVid == 0 ? vidFiles.Length - 1 : --currentVid;
                ShowCurrentVideo();
            }
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves play button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressPlayBtn;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over play button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressPlayBtn;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press play button when cursor hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressPlayBtn(object sender, object e)
        {
            // Plays video if not currently playing
            // Resumes playing if it has been paused
            // This will have no effect if the video is already running
            myVideoElement.Play();
            isPlaying = true;

            // Initialise the video property values.
            InitializePropertyValues();
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves pause button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pauseBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressPauseBtn;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over pause button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pauseBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressPauseBtn;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press pause button when cursor is hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressPauseBtn(object sender, object e)
        {
            myVideoElement.Pause();
            isPlaying = false;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves exitVidFullsrn button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitVidFullsrn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressExitVidFullsrnBtn;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over exitVidFullsrn button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitVidFullsrn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressExitVidFullsrnBtn;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press exitVidFullsrn button when cursor is hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressExitVidFullsrnBtn(object sender, object e)
        {
            main.closeFullVid(currentVid, isPlaying, (double)myVideoElement.Position.TotalMilliseconds, volumeSlider.Value, speedRatioSlider.Value, timelineSlider.Maximum, mediaOpened);
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
            this.Close();
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves stop button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressStopBtn;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over stop button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressStopBtn;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press stop button when cursor is hovering over it for 2 seconds
        /// Stops video from playing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressStopBtn(object sender, object e)
        {
            myVideoElement.Stop();
            isPlaying = false;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

    }
}
