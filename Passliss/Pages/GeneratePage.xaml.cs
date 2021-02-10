﻿/*
MIT License

Copyright (c) Léo Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. 
*/
using LeoCorpLibrary;
using Passliss.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Passliss.Pages
{
    /// <summary>
    /// Logique d'interaction pour GeneratePage.xaml
    /// </summary>
    public partial class GeneratePage : Page
    {
        public GeneratePage()
        {
            InitializeComponent();

            LowerCaseChk.IsChecked = true; // Check the checkbox
            UpperCaseChk.IsChecked = true; // Check the checkbox
            NumbersChk.IsChecked = true; // Check the checkbox

            LenghtTxt.Text = "20"; // Set text

            PasswordTxt.Text = Password.Generate(int.Parse(LenghtTxt.Text) + 1, Global.GetFinalCaracters(LowerCaseChk.IsChecked.Value, UpperCaseChk.IsChecked.Value, NumbersChk.IsChecked.Value, SpecialCaractersChk.IsChecked.Value), ","); // Generate

        }

        private void GenerateBtn_Click(object sender, RoutedEventArgs e)
        {
            LenghtTxt.Text = LenghtTxt.Text.Replace(" ", ""); // Remove whitespaces
            if (LenghtTxt.Text.Length <= 0 || !(int.Parse(LenghtTxt.Text) > 0))
            {
                MessageBox.Show(Properties.Resources.PleaseSpecifyLenghtMsg, Properties.Resources.Passliss, MessageBoxButton.OK, MessageBoxImage.Information); // Show message
                return;
            }

            if (!IsNoCheckboxesChecked())
            {
                PasswordTxt.Text = Password.Generate(int.Parse(LenghtTxt.Text) + 1, Global.GetFinalCaracters(LowerCaseChk.IsChecked.Value, UpperCaseChk.IsChecked.Value, NumbersChk.IsChecked.Value, SpecialCaractersChk.IsChecked.Value), ","); // Generate 
            }
            else
            {
                MessageBox.Show(Properties.Resources.PleaseSelectChkMsg, Properties.Resources.Passliss, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// True if all unchecked.
        /// </summary>
        /// <returns>A <see cref="bool"/> value.</returns>
        private bool IsNoCheckboxesChecked()
        {
            return (!LowerCaseChk.IsChecked.Value && !UpperCaseChk.IsChecked.Value && !NumbersChk.IsChecked.Value && !SpecialCaractersChk.IsChecked.Value);
        }

        private void LenghtTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(PasswordTxt.Text); // Copy the password
        }
    }
}
