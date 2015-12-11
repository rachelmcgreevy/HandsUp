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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KinectImageViewer
{
    /// <summary>
    /// Interaction logic for Help.xaml
    /// Help window
    /// </summary>
    public partial class Help : Window
    {
        //Button timer
        private DispatcherTimer buttonTimer = new DispatcherTimer();

        /// <summary>
        /// Intialises components
        /// </summary>
        public Help()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Changes cursor to hand when mouse leaves exit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Changes cursor to loading cursor when user hovers over exit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        /// <summary>
        /// Press exit button when cursor hovering over it for 2 seconds
        /// Closes help window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pressButton(object sender, object e)
        {
            this.Close();
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        /// <summary>
        /// Closes help window when button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
