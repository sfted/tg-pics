using Microsoft.UI.Xaml.Controls;
using System;

namespace TgPics.Desktop.Views.Pages
{
    public sealed partial class TwoFACodePage : Page
    {
        public TwoFACodePage()
        {
            InitializeComponent();
        }

        public int Code { get; private set; }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if(tb.Text.Length == 4)
            {
                try
                {
                    Code = Convert.ToInt32(tb.Text);
                }
                catch (Exception ex) { }
            }
        }
    }
}
