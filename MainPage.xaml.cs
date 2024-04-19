using System.Diagnostics;
using System.Text.Json;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace MauiBarcodeApp
{
    public partial class MainPage : ContentPage
    {
        HttpClient client = new HttpClient();
        private void Network_test()
        {
            Debug.WriteLine("Network test start");
            // Try to get something from Internet
            try
            {
                var resp =
               client.GetAsync("https://freeipapi.com/api/json").Result;
                Debug.WriteLine("Network test ok. " +
               resp.Content.ReadAsStringAsync().Result);
                LabelHttpResponse.Text = "Network ready";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Network test fail. " + ex.Message);
                LabelHttpResponse.Text = "Network may not ready";
            }
        }

        private async Task Show_Toast(string message)
        {
            CancellationTokenSource cancellationTokenSource =new CancellationTokenSource();
            ToastDuration duration = ToastDuration.Short;
            double fontSize = 14;
            var toast = Toast.Make(message, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
        }

        private void ResetBookDetail()
        {
            // Default contents
            LabelTitle.Text = "Title: ";
            LabelSubtitle.Text = "Subtitle: ";
            LabelMessage.Text = string.Empty;
            LabelMessage.TextColor = Colors.Black;
            ImageCover.Source =
           ImageSource.FromFile("image_coming_soon.png");
        }


        private void ParseBookJSON(string json)
        {
            // Convert http response content to JSON object
            using (var jsonDocument = JsonDocument.Parse(json))
            {
                var rootElement = jsonDocument.RootElement;
                Console.WriteLine(rootElement.ToString());
                // try to get title
                if (rootElement.TryGetProperty("title", out var title))
                {
                    LabelTitle.Text += title.ToString();
                }
                // Try to get the subtitle
                if (rootElement.TryGetProperty("subtitle",
               out var subtitle))
                {
                    LabelSubtitle.Text += subtitle.ToString();
                }
            }
            // update book cover image url
            ImageCover.Source = ImageSource.FromUri(new Uri(
           $"https://covers.openlibrary.org/b/isbn/{EntryISBN.Text.Trim()}-M.jpg"));
        }

        private async void FindBtn_Clicked(object sender, EventArgs e)
        {
            if (EntryISBN.Text.Trim().Length == 0)
            {
                // No ISBN number is entered
                await Show_Toast("Please enter an ISBN number");
                return;
            }
            ResetBookDetail();
            try
            {
                await Show_Toast("Querying book information");
                // API endpoint format: https://openlibrary.org/isbn/<ISBN of the book>.json
                string ApiUrl = $"https://openlibrary.org/isbn/{EntryISBN.Text.Trim()}.json";
                var resp = await client.GetStringAsync(ApiUrl);
                LabelHttpResponse.Text = resp;
                if (resp.Length < 13)
                {
                    // Book not found
                    LabelMessage.Text = "Book not found";
                    return;
                }
                ParseBookJSON(resp);
            }
            catch (Exception ex)
            {
                LabelHttpResponse.Text =
               "Querying book information error. " + ex.Message;
                Debug.WriteLine(LabelHttpResponse.Text);
            }
        }



        public MainPage()
        {
            InitializeComponent();
            Network_test();
        }

     
    }

}
