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
using System.Threading;
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;
using System.Runtime.InteropServices;

namespace KinectImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        //Video fields
        protected string[] vidFiles;
        protected int currentVid = 0;
        protected bool isPlaying = false;

        //Count down camera sound
        System.Media.SoundPlayer playerCount = new System.Media.SoundPlayer("../../images/countdownCam-sound.wav");

        //Timers
        DispatcherTimer ticks = new DispatcherTimer();
        DispatcherTimer pauser = new DispatcherTimer();
        DispatcherTimer waiting = new DispatcherTimer();
        DispatcherTimer buttonTimer = new DispatcherTimer();

        [DllImport("user32")]

        public static extern int SetCursorPos(int x, int y);

        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;

        [DllImport("user32.dll",
            CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]

        public static extern void mouse_event(int dwflags, int dx, int dy, int cButtons, int dwExtraInfo);

        //Pictures
        protected string[] picFiles;
        protected int currentImg = 0;

        protected bool mediaOpened = false;
        protected bool tabOpened = false;
        protected bool checking = true;


        FullscreenPics fPic;
        FullscreenVid fVid;

        protected bool fullPic = false;
        protected bool fullVid = false;

        //Kinect
        bool closing = false;
        const int skeletonCount = 6;
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];

        /// <summary>
        /// Initialises MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads components
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoad(object sender, RoutedEventArgs e)
        {
            // Kinect
            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);

            // Gets pictures from MyPictures folder by default
            String i = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string[] ext = { ".jpg", ".jpeg", ".gif", ".png", ".bmp", ".tiff" };
            picFiles = Directory.GetFiles(i, "*.*")
                .Where(f => ext.Contains(new FileInfo(f).Extension.ToLower())).ToArray();

            // Gets videos from MyVideos folder by default
            String v = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            string[] v_ext = { ".mp4", ".wmv", ".wma", ".mov", ".avi" };
            vidFiles = Directory.GetFiles(v, "*.*")
                .Where(g => v_ext.Contains(new FileInfo(g).Extension.ToLower())).ToArray();
            
            // Displays images on thumbnails as well as current image being shown
            ShowCurrentImage();
            ShowNextImage();
            ShowSecondNextImage();
            ShowPreviousImage();
            ShowSecondPreviousImage();
            myMediaElement.Source = new Uri(vidFiles[currentVid], UriKind.Absolute);
            
            // Displays help when program first starts
            Help h = new Help();
            h.Show();
        }
       
        /// <summary>
        /// Pauses Video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pause_Media(object sender, EventArgs e)
        {
            myMediaElement.Pause();
            isPlaying = false;
            pauser.Stop();
        }

        /// <summary>
        /// Stops waiting 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stop_Waiting(object sender, EventArgs e)
        {
            checking = true;
            waiting.Stop();
        }

        /// <summary>
        /// Changes current image to previous image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void previousBtn_Click(object sender, RoutedEventArgs e)
        {
            if (picFiles.Length > 0)
            {
                currentImg = currentImg == 0 ? picFiles.Length - 1 : --currentImg;
                ShowCurrentImage();
                ShowNextImage();
                ShowSecondNextImage();
                ShowPreviousImage();
                ShowSecondPreviousImage();
            }
        }

        /// <summary>
        /// Takes picture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void takePicBtn_Click(object sender, RoutedEventArgs e)
        {
            pauser.Interval = TimeSpan.FromMilliseconds(3000);
            pauser.Tick += pressCapture;
            playerCount.Play();
            pauser.Start();

        }

        /// <summary>
        /// Takes picture when hovering over button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressCapture(object sender, object e)
        {
            CaptureScreen(476, 195, 795, 577);
            pauser.Stop();
        }

        /// <summary>
        /// Changes current image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextBtn_Click(object sender, System.EventArgs e)
        {
            if (picFiles.Length > 0)
            {
                currentImg = currentImg == picFiles.Length - 1 ? 0 : ++currentImg;
                ShowCurrentImage();
                ShowNextImage();
                ShowSecondNextImage();
                ShowPreviousImage();
                ShowSecondPreviousImage();
            }
        }

        /// <summary>
        /// Shows current image
        /// </summary>
        public void ShowCurrentImage()
        {
            if(picFiles.Length > 0)
            {
                if (currentImg >= 0 && currentImg <= picFiles.Length - 1)
                {
                    BitmapImage bm = new BitmapImage(new Uri(picFiles[currentImg], UriKind.RelativeOrAbsolute));
                    ImageBox.Source = bm;
                }
            }

        }

        /// <summary>
        /// Shows current video
        /// </summary>
        public void ShowCurrentVideo()
        {
            if (currentVid >= 0 && currentVid <= vidFiles.Length - 1)
            {
                myMediaElement.Source = new Uri(vidFiles[currentVid], UriKind.Absolute);
            }
        }

        /// <summary>
        /// Shows next image on thumbnail
        /// </summary>
        public void ShowNextImage()
        {
            if (picFiles.Length > 2)
            {
                int nextImg = currentImg + 1;

                if (nextImg > picFiles.Length - 1)
                {
                    BitmapImage bm = new BitmapImage(new Uri(picFiles[0], UriKind.RelativeOrAbsolute));
                    NextImageBox.Source = bm;
                }
                else
                {
                    BitmapImage bm = new BitmapImage(new Uri(picFiles[nextImg], UriKind.RelativeOrAbsolute));
                    NextImageBox.Source = bm;
                }
            }
        }

        /// <summary>
        /// Shows Second to next image on thumbnail
        /// </summary>
        public void ShowSecondNextImage()
        {
            if (picFiles.Length > 4)
            {
                int Img = currentImg + 2;

                if (Img > picFiles.Length)
                {
                    BitmapImage bm = new BitmapImage(new Uri(picFiles[1], UriKind.RelativeOrAbsolute));
                    SecondNextImageBox.Source = bm;
                }
                else if (Img > picFiles.Length - 1)
                {
                    BitmapImage bm = new BitmapImage(new Uri(picFiles[0], UriKind.RelativeOrAbsolute));
                    SecondNextImageBox.Source = bm;
                }
                else
                {
                    BitmapImage bm = new BitmapImage(new Uri(picFiles[Img], UriKind.RelativeOrAbsolute));
                    SecondNextImageBox.Source = bm;
                }
            }
        }

        /// <summary>
        /// Shoes previous image on thumbnail
        /// </summary>
        public void ShowSecondPreviousImage()
        {
            if (picFiles.Length > 4)
            {
                int Img = currentImg - 2;

                if (Img < -1)
                {
                    BitmapImage bm = new BitmapImage(new Uri(picFiles[picFiles.Length - 2], UriKind.RelativeOrAbsolute));
                    SecondPreviousImageBox.Source = bm;
                }
                else if (Img < 0)
                {
                    BitmapImage bm = new BitmapImage(new Uri(picFiles[picFiles.Length - 1], UriKind.RelativeOrAbsolute));
                    SecondPreviousImageBox.Source = bm;
                }
                else
                {
                    BitmapImage bm = new BitmapImage(new Uri(picFiles[Img], UriKind.RelativeOrAbsolute));
                    SecondPreviousImageBox.Source = bm;
                }
            }
        }

        /// <summary>
        /// Shows previous image on thumbnail
        /// </summary>
        public void ShowPreviousImage()
        {
            if (picFiles.Length > 2)
            {
                int Img = currentImg - 1;

                if (Img < 0)
                {
                    BitmapImage bm = new BitmapImage(new Uri(picFiles[picFiles.Length - 1], UriKind.RelativeOrAbsolute));
                    PreviousImageBox.Source = bm;
                }
                else
                {
                    BitmapImage bm = new BitmapImage(new Uri(picFiles[Img], UriKind.RelativeOrAbsolute));
                    PreviousImageBox.Source = bm;
                }
            }
        }

        /// <summary>
        /// Imports picture files from folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importBtn_Click(object sender, RoutedEventArgs e)
        {
            string[] ext = { ".jpg", ".jpeg", ".gif", ".png", ".bmp", ".tiff" };
            String directoryPath;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "JPEG|*.jpg;*.jpeg|Bitmaps|*.bmp|Gif|*.gif|PNG|*.png|TIFF|*.tiff";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                directoryPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                picFiles = Directory.GetFiles(directoryPath, "*.*")
                .Where(f => ext.Contains(new FileInfo(f).Extension.ToLower())).ToArray();
                ShowCurrentImage();
                ShowNextImage();
                ShowSecondNextImage();
                ShowPreviousImage();
                ShowSecondPreviousImage();
            }
        }

        /// <summary>
        /// Displays help for user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void helpBtn_Click(object sender, RoutedEventArgs e)
        {
            Help h = new Help();
            h.Show();
        }

        /// <summary>
        /// Opens full screen for pictures
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fullscrnBtn_Click(object sender, RoutedEventArgs e)
        {
            fPic = new FullscreenPics(currentImg, this, picFiles);
            fullPic = true;
            fPic.Show();
        }

        /// <summary>
        /// Opens full screen for videos
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fullscrnVidBtn_Click(object sender, RoutedEventArgs e)
        {
            myMediaElement.Pause();
            double timeIn = myMediaElement.Position.TotalMilliseconds;
            double maxTime = DurationSlider.Maximum;
            double volumeIn = volumeSlider.Value;
            double speedIn = speedRatioSlider.Value;
            fVid = new FullscreenVid(vidFiles, currentVid, isPlaying, timeIn, volumeIn, speedIn, maxTime, mediaOpened, this);
            fVid.Show();
            fullVid = true;
            isPlaying = false;
        }

        /// <summary>
        /// Sets the full screen video
        /// </summary>
        public void setFullVid()
        {
            myMediaElement.Pause();
            double timeIn = myMediaElement.Position.TotalMilliseconds;
            double maxTime = DurationSlider.Maximum;
            double volumeIn = volumeSlider.Value;
            double speedIn = speedRatioSlider.Value;
            fVid = new FullscreenVid(vidFiles, currentVid, isPlaying, timeIn, volumeIn, speedIn, maxTime, mediaOpened, this);
            fVid.Show();
            fullVid = true;
            isPlaying = false;
        }

        /// <summary>
        /// Closes full screen video mode
        /// </summary>
        /// <param name="shownVid"></param>
        /// <param name="playing"></param>
        /// <param name="timeIn"></param>
        /// <param name="volumeIn"></param>
        /// <param name="speedIn"></param>
        /// <param name="maxTimeIn"></param>
        /// <param name="opened"></param>
        public void closeFullVid(int shownVid, bool playing, double timeIn, double volumeIn, double speedIn, double maxTimeIn, bool opened)
        {
            currentVid = shownVid;
            myMediaElement.Source = new Uri(vidFiles[currentVid], UriKind.Absolute);
            InitializeComponent();
            DurationSlider.Value = timeIn;
            DurationSlider.Maximum  = maxTimeIn;
            volumeSlider.Value = volumeIn;
            speedRatioSlider.Value = speedIn;
            int intTime = (int)timeIn;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, intTime);
            myMediaElement.Position = ts;
            fullVid = false;
            isPlaying = playing;
            ticks.Interval = TimeSpan.FromMilliseconds(1);
            ticks.Tick += ticks_Tick;
            ticks.Start();
            if (isPlaying)
            {
                myMediaElement.Play();
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
            myMediaElement.Play();
            isPlaying = true;

            // Initialises the video property values
            InitializePropertyValues();
        }

        /// <summary>
        /// Pause media
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void OnMouseDownPauseMedia(object sender, MouseButtonEventArgs args)
        {

            // The Pause method pauses the media if it is currently running. 
            // The Play method can be used to resume.
            myMediaElement.Pause();
            isPlaying = false;

        }

        /// <summary>
        /// Pause media
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void OnMouseOverPauseMedia(object sender, MouseButtonEventArgs args)
        {

            // The Pause method pauses the media if it is currently running. 
            // The Play method can be used to resume.
            myMediaElement.Pause();
            isPlaying = false;

        }

        /// <summary>
        /// Stops media
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void OnMouseDownStopMedia(object sender, MouseButtonEventArgs args)
        {

            // The Stop method stops and resets the media to be played from 
            // the beginning.
            myMediaElement.Stop();
            isPlaying = true;

        }

        /// <summary>
        /// Changes media volume
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            try
            {
                myMediaElement.Volume = (double)volumeSlider.Value;
            }
            catch(NullReferenceException n)
            {
                Console.WriteLine("Error: " + n);
            }
        }


        /// <summary>
        /// Changes speed of image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ChangeMediaSpeedRatio(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            myMediaElement.SpeedRatio = (double)speedRatioSlider.Value;
        }

        /// <summary>
        /// When the media opens, initialize the "Seek To" slider maximum value
        /// to the total number of miliseconds in the length of the media clip.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Element_MediaOpened(object sender, EventArgs e)
        {
            mediaOpened = true;
            try { 
                DurationSlider.Maximum = myMediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            }
            catch (System.InvalidOperationException) { }
            
            ticks.Interval = TimeSpan.FromMilliseconds(1);
            ticks.Tick += ticks_Tick;
            ticks.Start();
        }
 
        /// <summary>
        /// When the media playback is finished. Stop() the media to seek to media start.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Element_MediaEnded(object sender, EventArgs e)
        {
            myMediaElement.Stop();
            isPlaying = false;
        }

        /// <summary>
        /// Jump to different parts of the media (seek to).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SeekToMediaPosition(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            if(!isPlaying)
            {
                int SliderValue = (int)DurationSlider.Value;

                // Overloaded constructor takes the arguments days, hours, minutes, seconds, miniseconds. 
                // Create a TimeSpan with miliseconds equal to the slider value.
                TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
                myMediaElement.Position = ts;
            }
        }

        /// <summary>
        /// Updating the Slider Value of Media(Video Duration) 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ticks_Tick(object sender, object e)
        {
            DurationSlider.Value = myMediaElement.Position.TotalMilliseconds;
        }

        /// <summary>
        /// Initialises property values
        /// </summary>
        void InitializePropertyValues()
        {
            //Sets the volume and speed to their respective sliders
            myMediaElement.Volume = (double)volumeSlider.Value;
            myMediaElement.SpeedRatio = (double)speedRatioSlider.Value;
        }

        /// <summary>
        /// Sets the current image
        /// </summary>
        /// <param name="n"></param>
        public void setCurrentImg(int n)
        {
            fullPic = false;
            currentImg = n;
            ShowCurrentImage();
            ShowNextImage();
            ShowSecondNextImage();
            ShowPreviousImage();
            ShowSecondPreviousImage();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            tabOpened = true;
            myMediaElement.Play();
            isPlaying = true;
            if(tabOpened)
            {
                pauser.Interval = TimeSpan.FromMilliseconds(100);
            }
            else
            {
                pauser.Interval = TimeSpan.FromMilliseconds(650);
            }
            pauser.Tick += pause_Media;
            pauser.Start();
        }

        //// KINECT CODE

        /// <summary>
        /// Kinect Sensor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor old = (KinectSensor)e.OldValue;

            StopKinect(old);

            KinectSensor sensor = (KinectSensor)e.NewValue;

            if (sensor == null)
            {
                return;
            }

            var parameters = new TransformSmoothParameters
            {
                Smoothing = 0.3f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 1.0f,
                MaxDeviationRadius = 0.5f
            };
            sensor.SkeletonStream.Enable(parameters);

            sensor.SkeletonStream.Enable();

            sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
            sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

            try
            {
                sensor.Start();
               
            }
            catch (System.IO.IOException)
            {
                kinectSensorChooser1.AppConflictOccurred();
            }
        }


        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (closing)
            {
                return;
            }

            //Get a skeleton
            Skeleton firstSkeleton = GetFirstSkeleton(e);

            if (firstSkeleton == null)
            {
                return;
            }


            //set scaled position
            ScalePosition(leftEllipse, firstSkeleton.Joints[JointType.HandLeft]);
            ScalePosition(rightEllipse, firstSkeleton.Joints[JointType.HandRight]);

            if (checking)
            {
                ProcessGesture(firstSkeleton.Joints[JointType.Head], firstSkeleton.Joints[JointType.HandLeft], firstSkeleton.Joints[JointType.HandRight]);
            }
            GetCameraPoint(firstSkeleton, e);

            Joint ScaledJoint = firstSkeleton.Joints[JointType.HandRight].ScaleTo(1920, 1080, .3f, .3f);

            int topOfScreen;
            int leftOfScreen;

            leftOfScreen = Convert.ToInt32(ScaledJoint.Position.X);
            topOfScreen = Convert.ToInt32(ScaledJoint.Position.Y);

            SetCursorPos(leftOfScreen, topOfScreen);

        }


        /// <summary>
        /// Process kinect gestures
        /// </summary>
        /// <param name="head"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private void ProcessGesture(Joint head, Joint left, Joint right)
        {

            if (right.Position.Y > head.Position.Y && left.Position.Y > head.Position.Y)
            {
                // Picture tab gestures
                if (tabPictures.IsSelected)
                {
                    if (fullPic)
                    {
                        fPic.exit();
                        fullPic = false;
                    }
                    else
                    {
                        fPic = new FullscreenPics(currentImg, this, picFiles);
                        fullPic = true;
                        fPic.Show();
                    }
                }
                else if (tabVideos.IsSelected)
                {
                    if (fullVid)
                    {
                        fVid.exit();
                        fullVid = false;
                    }
                    else
                    {
                        setFullVid();
                    }
                }
                checking = false;
                waiting.Interval = TimeSpan.FromMilliseconds(2000);
                waiting.Tick += stop_Waiting;
                waiting.Start();
            }
            else if (right.Position.Y < left.Position.Y && right.Position.X < left.Position.X)
            {
                if (tabPictures.IsSelected)
                {
                    if (picFiles.Length > 0)
                    {
                        if (fullPic == true)
                        {
                            fPic.next();
                        }
                        else
                        {
                            currentImg = currentImg == picFiles.Length - 1 ? 0 : ++currentImg;
                            ShowCurrentImage();
                            ShowNextImage();
                            ShowSecondNextImage();
                            ShowPreviousImage();
                            ShowSecondPreviousImage();
                            
                        }
                    }
                }
                // Video gestures
                else if (tabVideos.IsSelected)
                {
                    if (vidFiles.Length > 0)
                    {
                        if (fullVid == true)
                        {
                            fVid.next();
                        }
                        else
                        {
                            myMediaElement.Stop();
                            isPlaying = false;

                            currentVid = currentVid == vidFiles.Length - 1 ? 0 : ++currentVid;
                            ShowCurrentVideo();
                        }
                    }
                }


                checking = false;
                waiting.Interval = TimeSpan.FromMilliseconds(1000);
                waiting.Tick += stop_Waiting;
                waiting.Start();

            }
            else if (right.Position.Y > left.Position.Y && right.Position.X < left.Position.X)
            {
                if (tabPictures.IsSelected)
                {
                    if (picFiles.Length > 0)
                    {
                        if (fullPic == true)
                        {
                            fPic.previous();
                        }
                        else
                        {
                            currentImg = currentImg == 0 ? picFiles.Length - 1 : --currentImg;
                            ShowCurrentImage();
                            ShowNextImage();
                            ShowSecondNextImage();
                            ShowPreviousImage();
                            ShowSecondPreviousImage();
                        }
                    }
                }
                else if (tabVideos.IsSelected)
                {
                    if (vidFiles.Length > 0)
                    {
                        if (fullVid == true)
                        {
                            fVid.next();
                        }
                        else
                        {   
                            myMediaElement.Stop();
                            isPlaying = false;
                            
                            currentVid = currentVid == 0 ? vidFiles.Length - 1 : --currentVid;
                            ShowCurrentVideo();
                        }
                    }
                }
                checking = false;
                waiting.Interval = TimeSpan.FromMilliseconds(1000);
                waiting.Tick += stop_Waiting;
                waiting.Start();   
            }    
        }

        /// <summary>
        /// Takes picture
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void CaptureScreen(double x, double y, double width, double height)
        {
            int ix, iy, iw, ih;
            ix = Convert.ToInt32(x);
            iy = Convert.ToInt32(y);
            iw = Convert.ToInt32(width);
            ih = Convert.ToInt32(height);

            //  set the kinect hand icon invisible 
            Bitmap image = new Bitmap(iw, ih,
                   System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(image);
            g.CopyFromScreen(ix, iy, 0,0 ,
                     new System.Drawing.Size(iw, ih),
                     CopyPixelOperation.SourceCopy);
            String i = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            Console.Write(i);
            String fileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            image.Save(i+ "\\" + fileName +".jpg");
        }

        

        void GetCameraPoint(Skeleton first, AllFramesReadyEventArgs e)
        {

            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                if (depth == null ||
                    kinectSensorChooser1.Kinect == null)
                {
                    return;
                }


                // Maps a joint location to a point on the depth map
                
                // head
                DepthImagePoint headDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.Head].Position);
                // left hand
                DepthImagePoint leftDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandLeft].Position);
                // right hand
                DepthImagePoint rightDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandRight].Position);


                // Maps a depth point to a point on the colour image
                //head
                ColorImagePoint headColourPoint =
                    depth.MapToColorImagePoint(headDepthPoint.X, headDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //left hand
                ColorImagePoint leftColourPoint =
                    depth.MapToColorImagePoint(leftDepthPoint.X, leftDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //right hand
                ColorImagePoint rightColourPoint =
                    depth.MapToColorImagePoint(rightDepthPoint.X, rightDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
            }
        }

        /// <summary>
        /// Gets the first skeleton
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        Skeleton GetFirstSkeleton(AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null;
                }


                skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                //get the first tracked skeleton
                Skeleton first = (from s in allSkeletons
                                  where s.TrackingState == SkeletonTrackingState.Tracked
                                  select s).FirstOrDefault();

                return first;

            }
        }

        /// <summary>
        /// Stops kinectt
        /// </summary>
        /// <param name="sensor"></param>
        private void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                if (sensor.IsRunning)
                {
                    //stop sensor 
                    sensor.Stop();

                    //stop audio if not null
                    if (sensor.AudioSource != null)
                    {
                        sensor.AudioSource.Stop();
                    }
                }
            }
        }

        /*private void CameraPosition(FrameworkElement element, ColorImagePoint point)
        {
            //Divide by 2 for width and height so point is right in the middle 
            // instead of in top/left corner
            Canvas.SetLeft(element, point.X - element.Width / 2);
            Canvas.SetTop(element, point.Y - element.Height / 2);

        }*/

        /// <summary>
        /// Scales position of joints
        /// </summary>
        /// <param name="element"></param>
        /// <param name="joint"></param>
        private void ScalePosition(FrameworkElement element, Joint joint)
        {
            Joint scaledJoint = joint.ScaleTo(1280, 720, .3f, .3f);

            Canvas.SetLeft(element, scaledJoint.Position.X);
            Canvas.SetTop(element, scaledJoint.Position.Y);

        }

        /// <summary>
        /// Changes to next video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void vid_Next_Click(object sender, RoutedEventArgs e)
        {
            myMediaElement.Stop();
            isPlaying = false;
            if (vidFiles.Length > 0)
            {
                currentVid = currentVid == vidFiles.Length - 1 ? 0 : ++currentVid;

                ShowCurrentVideo();
            }
        }

        /// <summary>
        /// Changes to previous video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void vid_Prev_Click(object sender, RoutedEventArgs e)
        {
            myMediaElement.Stop();
            isPlaying = false;
            if (vidFiles.Length > 0)
            {
                currentVid = currentVid == 0 ? vidFiles.Length - 1 : --currentVid;
                ShowCurrentVideo();
            }
        }

        /// <summary>
        /// Opens slide show on full screen when slide button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slideBtn_Click(object sender, RoutedEventArgs e)
        {
            FullscreenPics f = new FullscreenPics(currentImg, this, picFiles);
            f.Show();
            f.slideShow();
        }

        /// <summary>
        /// Impots videos
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportVideos_Click(object sender, RoutedEventArgs e)
        {
            string[] v_ext = { ".mp4", ".wmv", ".wma", ".mov", ".avi" };
            String directoryPath;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "MP4|*.mp4|WMV|*.wmv|WMA|*.wma|MOV|*.mov|AVI|*.avi";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                directoryPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                vidFiles = Directory.GetFiles(directoryPath, "*.*")
                    .Where(g => v_ext.Contains(new FileInfo(g).Extension.ToLower())).ToArray();
                Console.WriteLine(vidFiles.Length);
                Console.WriteLine("hello");
                currentVid = 0;
                myMediaElement.Source = new Uri(vidFiles[currentVid], UriKind.Absolute);
            }
        }

        /// <summary>
        /// Stops kinect if window closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closing = true;
            StopKinect(kinectSensorChooser1.Kinect);
        }



        // Code for mouse

        /// <summary>
        /// Changes cursor to hand when mouse leaves help button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void helpBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressHelp;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over help button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void helpBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressHelp;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press help button whilst hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressHelp(object sender, object e)
        {
            Help h = new Help();
            h.Show();
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves import button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressImport;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over import button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressImport;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press import pictures whilst hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressImport(object sender, object e)
        {
            string[] ext = { ".jpg", ".jpeg", ".gif", ".png", ".bmp", ".tiff" };
            String directoryPath;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "JPEG|*.jpg;*.jpeg|Bitmaps|*.bmp|Gif|*.gif|PNG|*.png|TIFF|*.tiff";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                directoryPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                picFiles = Directory.GetFiles(directoryPath, "*.*")
                .Where(f => ext.Contains(new FileInfo(f).Extension.ToLower())).ToArray();
                ShowCurrentImage();
                ShowNextImage();
                ShowSecondNextImage();
                ShowPreviousImage();
                ShowSecondPreviousImage();
            }
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves importVideos button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importVideosBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressImportVideos;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over importVideos button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importVideosBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressImportVideos;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press import videos when hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressImportVideos(object sender, object e)
        {
            string[] v_ext = { ".mp4", ".wmv", ".wma", ".mov", ".avi" };
            String directoryPath;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "MP4|*.mp4|WMV|*.wmv|WMA|*.wma|MOV|*.mov|AVI|*.avi";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                directoryPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                vidFiles = Directory.GetFiles(directoryPath, "*.*")
                    .Where(g => v_ext.Contains(new FileInfo(g).Extension.ToLower())).ToArray();
                Console.WriteLine(vidFiles.Length);
                Console.WriteLine("hello");
                currentVid = 0;
                myMediaElement.Source = new Uri(vidFiles[currentVid], UriKind.Absolute);
            }
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves slideshow button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slideBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressSlide;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over slideshow button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slideBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressSlide;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press slideshow button when cursor hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressSlide(object sender, object e)
        {
            FullscreenPics f = new FullscreenPics(currentImg, this, picFiles);
            f.Show();
            f.slideShow();
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves the tools tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolsTab_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressToolsTab;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over tools tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolsTab_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressToolsTab;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press tools tab when cursor hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressToolsTab(object sender, object e)
        {
            mainTab.SelectedIndex = 3;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves camera tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cameraTab_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressCameraTab;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over camera tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cameraTab_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressCameraTab;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press camera tab when cursor hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressCameraTab(object sender, object e)
        {
            mainTab.SelectedIndex = 2;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves video tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void videoTab_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressVideoTab;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over video tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void videoTab_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressVideoTab;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press video tab when cursor hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressVideoTab(object sender, object e)
        {
            mainTab.SelectedIndex = 1;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves picture tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureTab_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressPictureTab;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over the picture tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureTab_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressPictureTab;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press picture tab when cursor hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressPictureTab(object sender, object e)
        {
            mainTab.SelectedIndex = 0;
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
            myMediaElement.Play();
            isPlaying = true;

            // Initialises the video property values
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
            myMediaElement.Pause();
            isPlaying = false;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
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
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressStopBtn(object sender, object e)
        {
            myMediaElement.Stop();
            isPlaying = false;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves camera button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cameraButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressCameraButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over camera button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cameraButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressCameraButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press camera button when cursor is hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressCameraButton(object sender, object e)
        {
            pauser.Interval = TimeSpan.FromMilliseconds(3000);
            pauser.Tick += pressCapture;
            pauser.Start();
            playerCount.Play();
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

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
        /// Press vidNext button when cursor is hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressVidNextButton(object sender, object e)
        {
            myMediaElement.Stop();
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
        /// Changes cursor to hand when mouse leaves vidPrev button
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
        /// Press vidPrev button when cursor is hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressVidPrevButton(object sender, object e)
        {
            myMediaElement.Stop();
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
        /// Changes cursor to hand when mouse leaves videoFullsrn button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void videoFullsrn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressVideoFullsrnButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over videoFullsrn button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void videoFullsrn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressVideoFullsrnButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press videoFullsrn button when cursor is hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressVideoFullsrnButton(object sender, object e)
        {
            myMediaElement.Pause();
            double timeIn = myMediaElement.Position.TotalMilliseconds;
            double maxTime = DurationSlider.Maximum;
            double volumeIn = volumeSlider.Value;
            double speedIn = speedRatioSlider.Value;
            FullscreenVid v = new FullscreenVid(vidFiles, currentVid, isPlaying, timeIn, volumeIn, speedIn, maxTime, mediaOpened, this);
            v.Show();
            isPlaying = false;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
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
                ShowNextImage();
                ShowSecondNextImage();
                ShowPreviousImage();
                ShowSecondPreviousImage();
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
                ShowNextImage();
                ShowSecondNextImage();
                ShowPreviousImage();
                ShowSecondPreviousImage();
            }
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves pictures fullsrn button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fullsrn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressFullsrnButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }
        /// <summary>
        /// Changes cursor to loading cursor when user hovers over pictures fullsrn button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fullsrn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressFullsrnButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press pictures fullsrn button when cursor is hovering over it for 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressFullsrnButton(object sender, object e)
        {
            FullscreenPics f = new FullscreenPics(currentImg, this, picFiles);
            f.Show();
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }
    }

}
