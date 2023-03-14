using System;
using System.Windows.Forms;
using System.Data.OleDb;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using OpenAI_API;
using OpenAI_API.Completions;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net;


namespace GPTwork
{
    public partial class Form1 : Form
    {
        private string temperature;
        private string humidity;
        private string weather;
        public Form1()
        {
            InitializeComponent();
        }

        private void txtAdvice_TextChanged(object sender, EventArgs e)
        {

        }
        private void GetWeatherData(string location)
        {
            string apiKey = "93f2085dd9cdbb782e0f83159f418af2";
            string apiUrl = $"https://api.openweathermap.org/data/2.5/weather?q={location}&appid={apiKey}&units=metric";
            var client = new WebClient();
            var response = client.DownloadString(apiUrl);

            // Deserialize the JSON data into a C# object
            dynamic jsonData = JsonConvert.DeserializeObject(response);

            // Check if the API call was successful
            int cod = jsonData.cod;
            if (cod != 200)
            {
                MessageBox.Show("Could not retrieve weather data.");
                return;
            }

            // Retrieve the temperature and wind speed data
            temperature = jsonData.main.temp;
            humidity = jsonData.main.humidity;
            weather = jsonData.weather[0].description;
            pb1.ImageLocation = "https://openweathermap.org/img/w/" + jsonData.weather[0].icon + ".png";
            MessageBox.Show($"{weather}");
        }
        private async void btnGetAdvice_Click(object sender, EventArgs e)
        {
            //Get Location
            string location = txtLocation.Text;

            // Call the GetWeatherData method to retrieve the weather data
            GetWeatherData(location);
            // Check if the user's medical history has been filled out
            string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\20241779\OneDrive - Farnborough College of Technology\Health Advice COLLEGE PROJECT\GPTwork\GPTworkDB.accdb";
            using (var connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                var command = new OleDbCommand("SELECT COUNT(*) FROM MedicalHistory", connection);
                int count = (int)command.ExecuteScalar();
                if (count == 0)
                {
                    MessageBox.Show("Please fill out your medical history before getting advice.");
                    return;
                }
            }

            // Generate health advice using OpenAI
            string model = "text-davinci-002";
            string prompt = $"give Health advice for these weather conditions {weather}, temp {temperature}, humidity {humidity}.";
            ;
            int maxTokens = 100;
            var openaiClient = new OpenAIAPI("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");
            var completionRequest = new CompletionRequest
            {
                Model = model,
                Prompt = prompt,
                MaxTokens = maxTokens,
            };

            var completionResponse = await openaiClient.Completions.CreateCompletionAsync(completionRequest);
            Console.WriteLine(completionResponse);


            // Display the advice in the txtAdvice TextBox
            txtAdvice.Text = completionResponse.ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
 }

