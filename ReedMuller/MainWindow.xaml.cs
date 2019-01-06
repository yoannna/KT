using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ReedMuller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int r;
        private int m;
        private Random random;

        private byte[] imageHeader;
        private byte[] imageBytes;
        private byte[] bytes;
        public MainWindow()
        {
            InitializeComponent();
            random = new Random();
            selectionBox.Items.Add("Vector");
            selectionBox.Items.Add("Text");
            selectionBox.Items.Add("Image");
            selectionBox.SelectedItem = "Vector";
            vectorGrid.Visibility = Visibility.Visible;
            textGrid.Visibility = Visibility.Hidden;
            imageGrid.Visibility = Visibility.Hidden;
        }

        /// <summary>Checks if entered r and m are valid</summary>
        /// <returns>Are values valid</returns>
        private bool ValidateParameters()
        {
            if (!Int32.TryParse(rStr.Text, out r) || r < 1)
            {
                ShowNotification("r must be positive number");
                return false;
            }
            
            if (!Int32.TryParse(mStr.Text, out m) || m < 1)
            {
                ShowNotification("m must be positive number");
                return false;
            }

            if (r >= m)
            {
                ShowNotification("r must be less than m");
                return false;
            }

            return true;
        }

        /// <summary>Shows notification.</summary>
        /// <param name="text">Notification to display</param>
        private void ShowNotification(string text)
        {
            notificationsField.Text = text;
        }

        /// <summary>Clears notifications</summary>
        private void ClearNotifications()
        {
            notificationsField.Text = "";
        }
        
        /// <summary>Displays matrix in a new window.</summary>
        /// <remarks>Called on Matrix button click</remarks>
        private void Matrix_Button_Click(object sender, RoutedEventArgs e)
        {
            ClearNotifications();
            try
            {
                if (!ValidateParameters())
                {
                    ShowNotification("");
                    return;
                }
                Coder code = new Coder(m, r);
                MatrixWindow matrixWindow = new MatrixWindow(code.Matrix, m, r);
                matrixWindow.Show();
                ShowNotification("");
            }
            catch (OutOfMemoryException)
            {
                notificationsField.Text = "OutOfMemoryException was thrown";
            }
        }
        
        /// <summary>Encodes given vector.</summary>
        /// <remarks>Called on Encode button click</remarks>
        private void Encode_Button_Click(object sender, RoutedEventArgs e)
        {
            ClearNotifications();

            #region Vector validation
            if (vector.Text == "")
            {
                ShowNotification("Vector must be entered");
                return;
            }

            bool[] word = new bool[vector.Text.Length];
            for (int i = 0; i < word.Length; i++)
            {
                int parsed = (int)Char.GetNumericValue(vector.Text[i]);
                if (parsed < 0 || parsed > 1)
                {
                    ShowNotification("Vector must contain 1 and 0");
                    return;
                }

                word[i] = parsed == 1;
            }
            #endregion

            if (!ValidateParameters())
                return;

            Coder code = new Coder(m, r);
            if (word.Length != code.WordLenght)
            {
                ShowNotification("Vector length must be " + code.WordLenght + ". Current: " + word.Length);
                return;
            }

            StringBuilder encodedStr = new StringBuilder();
            foreach (bool c in code.Encode(word))
                encodedStr.Append(c ? 1 : 0);
            encoded.Text = encodedStr.ToString();
        }

        /// <summary>Checks if string is valid and converts it to a number.</summary>
        /// <param name="pStr">String representing probability</param>
        /// <returns>Number representing probability</returns>
        private int? ConverStringToProbability(string pStr)
        {
            int p;

            // Return null if probability is invalid
            if (!Int32.TryParse(pStr, out p))
                return null;
            if (p < 0 || p > 100)
                return null;

            return p;
        }

        /// <summary>Sends encoded vector through a channel.</summary>
        /// <remarks>Called on Send button click</remarks>
        private void Send_Button_Click(object sender, RoutedEventArgs e)
        {
            ClearNotifications();
            int? p = ConverStringToProbability(pStr.Text);
            if (p == null)
            {
                ShowNotification("Probability must be a number between 0 and 100");
                return;
            }
            
            StringBuilder strToSend = new StringBuilder(encoded.Text);
            StringBuilder mistakesStr = new StringBuilder();
            int mistakesCount = 0;
            for (int i = 0; i < strToSend.Length; i++)
            {
                // Check if valid string is entered.
                if (strToSend[i] != '0' && strToSend[i] != '1')
                    return;
                
                int rnd = random.Next(1, 101);
                if (rnd <= p)
                {
                    if (random.Next(1, 3) == 1)
                    {
                        // Write mistake numbers to string to print it.
                        if (mistakesStr.Length != 0)
                            mistakesStr.Append(", ");
                        mistakesStr.Append(i);
                    
                        if (strToSend[i] == '0')
                            strToSend[i] = '1';
                        else
                            strToSend[i] = '0';

                        mistakesCount++;
                    }
                }
            }

            // Display new string in the next text field
            changedCode.Text = strToSend.ToString();

            // Print mistakes information
            if (mistakesStr.Length == 0)
                ClearNotifications();
            else
            {
                StringBuilder mistakesLabel = new StringBuilder();
                mistakesLabel.Append(mistakesCount + " mistakes\n");
                mistakesLabel.Append("Mistake positions: ");
                mistakesLabel.Append(mistakesStr);
                ShowNotification(mistakesLabel.ToString());
            }
        }

        /// <summary>Decodes received vector.</summary>
        /// <remarks>Called on Decode button click</remarks>
        private void Decode_Button_Click(object sender, RoutedEventArgs e)
        {
            ClearNotifications();
            bool[] word = new bool[changedCode.Text.Length];
            for (int i = 0; i < word.Length; i++)
            {
                int parsed = (int)Char.GetNumericValue(changedCode.Text[i]);
                if (parsed < 0 || parsed > 1)
                {
                    return;
                }
                word[i] = parsed == 1;
            }
            
            if (!ValidateParameters())
                return;

            Coder code = new Coder(m, r);
            if (word.Length != code.CodeLenght)
            {
                return;
            }
            
            StringBuilder decodedStr = new StringBuilder();
            foreach (bool c in code.Decode(word))
                decodedStr.Append(c ? 1 : 0);
            decoded.Text = decodedStr.ToString();
            
            ShowNotification(vector.Text == decoded.Text ? "Decoded word matches encoded word" : "Words do not match.");
        }
        
        /// <summary>Randomly changes data.</summary>
        /// <param name="value">Value to randomly change</param>
        /// <param name="p">Probability</param>
        /// <returns>Random value</returns>
        private bool GetRandom (bool value, int p)
        {
            if (p < 0 || p > 100)
                throw new ArgumentException();

            if (random.Next(1, 101) <= p)
            {
                if (random.Next(1, 3) == 1)
                    value = !value;
            }

            return value;
        }

        /// <summary>Sends text through a channel.</summary>
        /// <remarks>Called on Send button click</remarks>
        private void SendText_Button_Click(object sender, RoutedEventArgs e)
        {
            ClearNotifications();

            if (!ValidateParameters())
                return;
            
            int? p = ConverStringToProbability(pTextStr.Text);
            if (p == null)
            {
                ShowNotification("Probability must be a number between 0 and 100");
                return;
            }

            #region Simple text
            List<bool> binaryText = Coder.ConvertToBooleans(originalTextField.Text);
            for (int i = 0; i < binaryText.Count; i++)
            {
                binaryText[i] = GetRandom(binaryText[i], (int)p);
            }
            textField.Text = Coder.ConvertToText(binaryText);
            #endregion

            #region Encoded text
            Coder code = new Coder(m, r);
            int addedZerosCount;
            List<bool[]> encodedText = code.Encode(originalTextField.Text, out addedZerosCount);
            
            foreach (bool[] word in encodedText)
            {
                for (int i  = 0; i < word.Length; i++)
                {
                    word[i] = GetRandom(word[i], (int)p);
                }
            }
            
            string decodedText = code.DecodeText(encodedText, addedZerosCount);
            encodedTextField.Text = decodedText;
            #endregion
        }

        /// <summary>Sends image through a channel.</summary>
        /// <remarks>Called on Send button click</remarks>
        private void SendImage_Button_Click(object sender, RoutedEventArgs e)
        {
            ClearNotifications();

            if (!ValidateParameters())
                return;

            int? p = ConverStringToProbability(pImageStr.Text);
            if (p == null)
            {
                ShowNotification("Probability must be a number between 0 and 100");
                return;
            }

            #region Simple image
            List<bool> binaryImage = Coder.ConvertToBooleans(imageBytes);
            for (int i = 0; i < binaryImage.Count; i++)
            {
                binaryImage[i] = GetRandom(binaryImage[i], (int)p);
            }
            byte[] bytes = Coder.ConvertToByteArray(binaryImage);

            List<byte> fullList = new List<byte>();
            fullList.AddRange(imageHeader);
            fullList.AddRange(bytes);

            image.Source = ConvertToImage(fullList.ToArray());
            #endregion

            #region Encoded image
            Coder code = new Coder(m, r);
            int addedZerosCount;
            List<bool[]> _encodedImage = code.Encode(imageBytes, out addedZerosCount);
            
            foreach (bool[] word in _encodedImage)
            {
                for (int i = 0; i < word.Length; i++)
                {
                    word[i] = GetRandom(word[i], (int)p);
                }
            }

            fullList = new List<byte>();
            fullList.AddRange(imageHeader);
            fullList.AddRange(code.DecodeByteArray(_encodedImage, addedZerosCount));
            
            encodedImage.Source = ConvertToImage(fullList.ToArray());
            #endregion
        }
        
        /// <summary>Converts bytes to image.</summary>
        /// <param name="array">Array to convert to an image</param>
        /// <returns>Image</returns>
        private BitmapImage ConvertToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        /// <summary>Lets to select an image</summary>
        /// <remarks>Called on FIle button click</remarks>
        private void File_Button_Click(object sender, EventArgs e)
        {
            OpenFileDialog FD = new OpenFileDialog();
            FD.AddExtension = true;
            FD.Filter = "Image Files (BMP)|*.BMP";
            if (FD.ShowDialog() == true)
            {
                // Read file into byte array:
                System.IO.FileInfo File = new System.IO.FileInfo(FD.FileName);
                var fileStream = File.OpenRead();
                long size = fileStream.Length;
                bytes = new byte[size];
                for (int i = 0; i < size; i++)
                    bytes[i] = (byte)fileStream.ReadByte();
                fileStream.Close();
                
                originalImage.Source = ConvertToImage(bytes);

                imageHeader = new byte[54];
                for (int i = 0; i < 54; i++)
                {
                    imageHeader[i] = bytes[i];
                }

                imageBytes = new byte[bytes.Length - 54];
                for (int i = 54; i < bytes.Length; i++)
                {
                    imageBytes[i - 54] = bytes[i];
                }
            }
        }
        
        /// <summary>Toggles view.</summary>
        private void Selection_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch ((string)selectionBox.SelectedItem)
            {
                case "Vector":
                    vectorGrid.Visibility = Visibility.Visible;
                    textGrid.Visibility = Visibility.Hidden;
                    imageGrid.Visibility = Visibility.Hidden;
                    break;
                case "Text":
                    vectorGrid.Visibility = Visibility.Hidden;
                    textGrid.Visibility = Visibility.Visible;
                    imageGrid.Visibility = Visibility.Hidden;
                    break;
                case "Image":
                    vectorGrid.Visibility = Visibility.Hidden;
                    textGrid.Visibility = Visibility.Hidden;
                    imageGrid.Visibility = Visibility.Visible;
                    break;

            }
        }
    }
}
