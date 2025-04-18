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
using Passliss.Enums;
using Passliss.UserControls;
using Passliss.Windows;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Passliss.Pages;

/// <summary>
/// Logique d'interaction pour GeneratePage.xaml
/// </summary>
public partial class GeneratePage : Page
{
	public GeneratePage()
	{
		InitializeComponent();

		// Set special characters in Global
		Global.SpecialCaracters = Global.Settings.UseSimpleSpecialChars ?? false ? ";,:,!,/,*,$,%,),=,+,-,',(,_,<,>,?" : Global.SpecialCaracters;
		OtherCharactersTxt.Text = Global.Settings.SaveCustomChars ?? true ? Global.Settings.CustomUserChars : "";

		for (int i = 0; i < Global.PasswordConfigurations.Count; i++)
		{
			if (Global.PasswordConfigurations[i].IsDefault.Value)
			{
				Global.DefaultPasswordConfiguration = Global.PasswordConfigurations[i];
				break;
			}
			else
			{
				Global.DefaultPasswordConfiguration = null; // Set to null
			}
		}

		if (Global.DefaultPasswordConfiguration is not null)
		{
			LowerCaseChk.IsChecked = Global.DefaultPasswordConfiguration.UseLowerCase; // Set value
			UpperCaseChk.IsChecked = Global.DefaultPasswordConfiguration.UseUpperCase; // Set value
			NumbersChk.IsChecked = Global.DefaultPasswordConfiguration.UseNumbers; // Set value
			SpecialCaractersChk.IsChecked = Global.DefaultPasswordConfiguration.UseSpecialCaracters; // Set value
			LenghtTxt.Text = Global.DefaultPasswordConfiguration.Length; // Set text 
		}
		else
		{
			LowerCaseChk.IsChecked = true; // Set value
			UpperCaseChk.IsChecked = true; // Set value
			NumbersChk.IsChecked = true; // Set value
			SpecialCaractersChk.IsChecked = false; // Set value
		}

		if (Global.Settings.UseRandomPasswordLengthOnStart.Value)
		{
			Random random = new();
			LenghtTxt.Text = random.Next(Global.Settings.MinRandomLength.Value, Global.Settings.MaxRandomLength.Value).ToString();
		}

		PasswordTxt.Text = Password.Generate(int.Parse(LenghtTxt.Text) + 1, Global.GetFinalCaracters(LowerCaseChk.IsChecked.Value, UpperCaseChk.IsChecked.Value, NumbersChk.IsChecked.Value, SpecialCaractersChk.IsChecked.Value) + OtherCharactersTxt.Text, ","); // Generate
		if (!Global.Settings.DisableHistory.Value)
		{
			PasswordHistory.Children.Add(new PasswordHistoryItem(PasswordTxt.Text, PasswordHistory)); // Add to history 
		}
		else
		{
			HistoryBtn.Visibility = Visibility.Collapsed; // Set visibility
		}
		UpdateStrengthIcon(); // Update the icon

	}

	private void GenerateBtn_Click(object sender, RoutedEventArgs e)
	{
		LenghtTxt.Text = LenghtTxt.Text.Replace(" ", ""); // Remove whitespaces
		if (!Global.Settings.DisableHistory.Value)
		{
			HistoryBtn.Visibility = Visibility.Visible; // Set visibility 
		}
		else
		{
			HistoryBtn.Visibility = Visibility.Collapsed; // Set visibility
		}

		if (LenghtTxt.Text.Length <= 0 || !(int.Parse(LenghtTxt.Text) > 0))
		{
			MessageBox.Show(Properties.Resources.PleaseSpecifyLenghtMsg, Properties.Resources.Passliss, MessageBoxButton.OK, MessageBoxImage.Information); // Show message
			return;
		}

		if (!IsNoCheckboxesChecked())
		{
			PasswordTxt.Text = Password.Generate(int.Parse(LenghtTxt.Text) + 1, Global.GetFinalCaracters(LowerCaseChk.IsChecked.Value, UpperCaseChk.IsChecked.Value, NumbersChk.IsChecked.Value, SpecialCaractersChk.IsChecked.Value) + OtherCharactersTxt.Text, ","); // Generate 
			if (!Global.Settings.DisableHistory.Value)
			{
				PasswordHistory.Children.Add(new PasswordHistoryItem(PasswordTxt.Text, PasswordHistory)); // Add to history 
			}

			UpdateStrengthIcon(); // Update the icon
		}
		else
		{
			MessageBox.Show(Properties.Resources.PleaseSelectChkMsg, Properties.Resources.Passliss, MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}

	private void UpdateStrengthIcon()
	{
		// Set the Strength icon
		var strength = Global.GetPasswordStrenght(PasswordTxt.Text); // Get strenght
		StrengthIcon.Text = strength switch
		{
			PasswordStrenght.VeryGood => "\uF6EA", // If the password strenght is very good
			PasswordStrenght.Good => "\uF299", // If the password strenght is good
			PasswordStrenght.Medium => "\uF882", // If the password strenght is medium
			PasswordStrenght.Low => "\uF36E", // If the password strenght is low
			_ => "\uF4AB" // If the password strenght is unknown
		};

		StrengthIcon.Foreground = Global.GetStrenghtColorBrush(strength); // Set color
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
		Regex regex = new("[^0-9]+");
		e.Handled = regex.IsMatch(e.Text);
	}

	private void CopyBtn_Click(object sender, RoutedEventArgs e)
	{
		Clipboard.SetText(PasswordTxt.Text); // Copy the password
	}

	readonly LoadPasswordConfigurationWindow LoadPasswordConfigurationWindow = new(); // Create a LoadPasswordConfigurationWindow
	private void LoadPwrConfig_Click(object sender, RoutedEventArgs e)
	{
		double factor = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11; // Get factor for DPI

		if (Global.PasswordConfigurations.Count > 0) // If there is Password Configurations (at least one)
		{
			NewPasswordConfigurationWindow.Hide(); // Hide the NewPasswordConfigurationWindow

			LoadPasswordConfigurationWindow.WindowStartupLocation = WindowStartupLocation.Manual; // Set the startup position to manual
			LoadPasswordConfigurationWindow.Left = (PointToScreen(Mouse.GetPosition(this)).X - LoadPasswordConfigurationWindow.Width / 2) / factor; // Calculate the X position
			LoadPasswordConfigurationWindow.Top = PointToScreen(Mouse.GetPosition(this)).Y / factor - (10 + LoadPasswordConfigurationWindow.Height); // Calculate the Y position
			LoadPasswordConfigurationWindow.InitUI(); // Refresh
			LoadPasswordConfigurationWindow.Show(); // Show
			LoadPasswordConfigurationWindow.Focus();
		}
		else
		{
			MessageBox.Show(Properties.Resources.NoPasswordConfigurations, Properties.Resources.PasswordConfigurations, MessageBoxButton.OK, MessageBoxImage.Exclamation);
		}
	}

	readonly NewPasswordConfigurationWindow NewPasswordConfigurationWindow = new(); // Create a NewPasswordConfigurationWindow
	private void NewPwrConfig_Click(object sender, RoutedEventArgs e)
	{
		double factor = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11; // Get factor for DPI

		LoadPasswordConfigurationWindow.Hide(); // Hide the LoadPasswordConfigurationWindow

		NewPasswordConfigurationWindow.WindowStartupLocation = WindowStartupLocation.Manual; // Set the startup position to manual
		NewPasswordConfigurationWindow.Left = (PointToScreen(Mouse.GetPosition(this)).X - NewPasswordConfigurationWindow.Width / 2) / factor; // Calculate the X position
		NewPasswordConfigurationWindow.Top = PointToScreen(Mouse.GetPosition(this)).Y / factor - (10 + NewPasswordConfigurationWindow.Height); // Calculate the Y position
		NewPasswordConfigurationWindow.Show(); // Show
		NewPasswordConfigurationWindow.Focus();
	}

	private void RandomizeLength_Click(object sender, RoutedEventArgs e)
	{
		Random random = new();
		LenghtTxt.Text = random.Next(Global.Settings.MinRandomLength.Value, Global.Settings.MaxRandomLength.Value).ToString();
	}

	internal void HistoryBtn_Click(object sender, RoutedEventArgs e)
	{
		if (PasswordHistory.Children.Count > 0)
		{
			HistoryBtn.Visibility = Visibility.Visible; // Set visibility
			if (sender is not PasswordHistoryItem)
			{
				if (Header.Visibility == Visibility.Visible)
				{
					Header.Visibility = Visibility.Collapsed; // Hide
					Content.Visibility = Visibility.Collapsed; // Hide
					ExportBar.Visibility = Visibility.Collapsed; // Hide

					PasswordHistory.Visibility = Visibility.Visible; // Show
					HistoryScroll.Visibility = Visibility.Visible; // Show
					HidePasswordBtn.Visibility = Visibility.Visible; // Show

					HistoryBtn.Content = "\uF36A"; // Set text
					for (int i = 0; i < PasswordHistory.Children.Count; i++)
					{
						if (PasswordHistory.Children[i] is PasswordHistoryItem passwordHistoryItem)
						{
							passwordHistoryItem.HideOrShowPasswordInPlainText(showPassword); // Show or hide password
						}
					}
				}
				else
				{
					Header.Visibility = Visibility.Visible; // Show
					Content.Visibility = Visibility.Visible; // Show
					ExportBar.Visibility = Visibility.Visible; // Show

					PasswordHistory.Visibility = Visibility.Collapsed; // Hide
					HistoryScroll.Visibility = Visibility.Collapsed; // Hide
					HidePasswordBtn.Visibility = Visibility.Collapsed; // Hide

					HistoryBtn.Content = "\uF47F"; // Set text
				}
			}
		}
		else
		{
			Header.Visibility = Visibility.Visible; // Show
			Content.Visibility = Visibility.Visible; // Show
			ExportBar.Visibility = Visibility.Visible; // Show

			PasswordHistory.Visibility = Visibility.Collapsed; // Hide
			HistoryScroll.Visibility = Visibility.Collapsed; // Hide

			HistoryBtn.Content = "\uF47F"; // Set text
			HistoryBtn.Visibility = Visibility.Collapsed; // Set visibility
			HidePasswordBtn.Visibility = Visibility.Collapsed; // Hide
			if (sender is not PasswordHistoryItem)
			{
				MessageBox.Show(Properties.Resources.HistoryEmpty, Properties.Resources.Passliss, MessageBoxButton.OK, MessageBoxImage.Information); // Show
			}
		}
	}

	private void RandomizeBtn_Click(object sender, RoutedEventArgs e)
	{
		Random random = new();
		LowerCaseChk.IsChecked = random.Next(0, 2) == 0; // Randomize
		UpperCaseChk.IsChecked = random.Next(0, 2) == 0; // Randomize
		NumbersChk.IsChecked = random.Next(0, 2) == 0; // Randomize
		SpecialCaractersChk.IsChecked = random.Next(0, 2) == 0; // Randomize
		LenghtTxt.Text = random.Next(Global.Settings.MinRandomLength.Value, Global.Settings.MaxRandomLength.Value).ToString();
	}

	private void ShowFullPasswordBtn_Click(object sender, RoutedEventArgs e)
	{
		new SeeFullPassword(PasswordTxt.Text).Show(); // Show the window
	}

	bool showPassword = !Global.Settings.AlwaysHidePasswordInHistory.Value;
	private void HidePasswordBtn_Click(object sender, RoutedEventArgs e)
	{
		showPassword = !showPassword; // Update
		HidePasswordBtn.Content = showPassword ? "\uF3F8" : "\uF3FC"; // Set icon text

		for (int i = 0; i < PasswordHistory.Children.Count; i++)
		{
			if (PasswordHistory.Children[i] is PasswordHistoryItem passwordHistoryItem)
			{
				passwordHistoryItem.HideOrShowPasswordInPlainText(showPassword); // Show or hide password
			}
		}
	}

	public MainWindow MainWindow { get; set; }
	private void TestStrengthBtn_Click(object sender, RoutedEventArgs e)
	{
		Global.StrenghtPage.PasswordTxt.Text = PasswordTxt.Text; // Set text
		Global.StrenghtPage.PasswordPwrBox.Password = PasswordTxt.Text; // Set text
		Global.StrenghtPage.InitSeeMoreUI(); // Init UI

		MainWindow.StrenghtTabBtn_Click(this, null);
	}
}
