// System
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Windows;

// Associated to the base Wpf namespace
namespace Pickles.Forms
{
    /// <summary>
    /// Message (wpf)
    /// </summary>
    internal partial class WpfMessage : Window
    {
        private bool _yesNo = false;
        internal bool Affirmative = false;
        private PathHelper _pathHelper;

        /// <summary>
        /// Processes a Wpf form.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="showLeftButton"></param>
        /// <param name="leftButtonText"></param>
        /// <param name="rightButtonText"></param>
        /// <param name="yesNo"></param>
        /// <param name="resourcePath"></param>
        /// <param name="resourceText"></param>
        /// <param name="showMore"></param>
        internal WpfMessage(string title = null, string message = null, bool showLeftButton = true,
            string leftButtonText = null, string rightButtonText = null, bool yesNo = false,
            string resourcePath = "", string resourceText = "", string showMore = "")
        {
            InitializeComponent();
            this.Topmost = true;
            this.ShowInTaskbar = true;

            if (title.Ext_HasChars()) { this.Title = title; }
            if (message.Ext_HasChars()) { this.TooltipTextBlock.Text = message; }
            if (leftButtonText.Ext_HasChars()) { this.LeftButton.Content = leftButtonText; }
            if (rightButtonText.Ext_HasChars()) { this.RightButton.Content = rightButtonText; }

            if (!showLeftButton)
            {
                this.RightButton.Visibility = System.Windows.Visibility.Collapsed;
                System.Windows.Controls.Grid.SetColumnSpan(this.LeftButton, 2);
                this.LeftButton.Margin = new Thickness(0);
            }

            // Show more button
            if (showMore.Ext_HasChars())
            {
                this.ShowMoreButton.Visibility = System.Windows.Visibility.Visible;
                this.ShowMoreTextBlock.Text = showMore;
            }

            // Link button
            this._pathHelper = new PathHelper(resourcePath);

            if (this._pathHelper.ResourcePath.Ext_HasChars())
            {
                this.LinkButton.Visibility = System.Windows.Visibility.Visible;

                if (resourceText.Ext_HasChars())
                {
                    this.LinkButton.Content = resourceText;
                }
                else if (this._pathHelper.ResourceType == RESOURCE_TYPE.DIRECTORY)
                {
                    this.LinkButton.Content = "Open related directory...";
                }
                else if (this._pathHelper.ResourceType == RESOURCE_TYPE.FILE)
                {
                    this.LinkButton.Content = "Open related file...";
                }
                else if (this._pathHelper.ResourceType == RESOURCE_TYPE.URL)
                {
                    this.LinkButton.Content = "Open related web link...";
                }
                else
                {
                    this.LinkButton.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

            this._yesNo = yesNo;
        }

        /// <summary>
        /// Runs when left is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            this.Affirmative = true;
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Runs when right button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            this.Affirmative = false;
            if (this._yesNo) { this.DialogResult = false; }
            this.Close();
        }

        /// <summary>
        /// Runs when the show more button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowMoreButton_Click(object sender, RoutedEventArgs e)
        {
            this.ShowMoreBorder.Visibility = (this.ShowMoreBorder.Visibility == System.Windows.Visibility.Collapsed)
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            this._pathHelper.Open();
        }
    }
}