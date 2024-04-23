﻿using System.Diagnostics;
using System.Text.Json;
using System.Web;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace MauiBarcodeApp
{
    public partial class MainPage : ContentPage, IQueryAttributable
    {
        HttpClient client = new HttpClient();


        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            EntryISBN.Text = HttpUtility.UrlDecode(query["barcode"].ToString());
            Console.WriteLine(HttpUtility.UrlDecode(query["format"].ToString()));
            query.Clear();
        }

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
            LabelTitle.Text = "Product Name: ";
            LabelSubtitle.Text = "Ingredients: ";
            Brand.Text = "Brand: ";
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
                if (rootElement.TryGetProperty("product", out var product))
                {
                    if (product.TryGetProperty("image_url", out var image_url))
                    {
                        ImageCover.Source = ImageSource.FromUri(new Uri(image_url.ToString()));
                    }


                    // Try to get the subtitle
                    if (product.TryGetProperty("ingredients_text", out var subtitle))
                    {
                        LabelSubtitle.Text += subtitle.ToString();
                    }

                    if (product.TryGetProperty("brands", out var brand))
                    {
                        Brand.Text += brand.ToString();
                    }


                    string[] keys = new string[] { "product_name", "abbreviated_product_name" };
                    foreach (var key in keys)
                    {
                        if (product.TryGetProperty(key, out var product_name) && !string.IsNullOrEmpty(product_name.ToString()))
                        {
                            LabelTitle.Text += product_name.ToString();
                            break;
                        }
                    }

                }

            }
            // update book cover image url
           // ImageCover.Source = ImageSource.FromUri(new Uri(
           //$"https://covers.openlibrary.org/b/isbn/{EntryISBN.Text.Trim()}-M.jpg"));
        }

        private async void FindBtn_Clicked(object sender, EventArgs e)
        {
            var test = EntryISBN.Text;
            if (EntryISBN.Text.Trim().Length == 0)
            {
                // No ISBN number is entered
                await Show_Toast("Please enter an ISBN number");
                return;
            }
            ResetBookDetail();
            try
            {
                await Show_Toast("Querying Product information");
                // API endpoint format: https://openlibrary.org/isbn/<ISBN of the book>.json
                //string ApiUrl = $"https://openlibrary.org/isbn/{EntryISBN.Text.Trim()}.json";
                string ApiUrl = $"https://world.openfoodfacts.org/api/v0/product/{EntryISBN.Text.Trim()}.json";
                var resp = await client.GetStringAsync(ApiUrl);
                LabelHttpResponse.Text = resp;
                JsonDocument.Parse(resp).RootElement.TryGetProperty("status", out var status);
                if (resp.Length < 13 || status.GetInt32() == 0)
                {
                    // Book not found
                    LabelMessage.Text = "Product not found";
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

        private async void ScanIsbnBtn_Clicked(object sender, EventArgs e)
        {
            ResetBookDetail();
            await Shell.Current.GoToAsync("barcodescanning");
        }


        public MainPage()
        {
            InitializeComponent();

            Routing.RegisterRoute("barcodescanning", typeof(BarcodeScanning));

            Network_test();
        }

     
    }

}
