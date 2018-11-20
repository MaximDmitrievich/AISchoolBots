using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using WeatherBot.Models;

namespace WeatherBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        [NonSerialized]
        private readonly HttpClient _client;

        public RootDialog()
        {
            _client = new HttpClient();
        }

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            IMessageActivity message = await result;
            IMessageActivity responseMessage = context.MakeMessage();
            HttpRequestMessage locationRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(ConfigurationManager.AppSettings["AccuWeatherHost"] +
                ConfigurationManager.AppSettings["AccuWeatherLocations"] +
                "?apikey=" + ConfigurationManager.AppSettings["AccuWeatherApiKey"] +
                "&q=" + message.Text +
                "&language=ru-ru"),
                Method = HttpMethod.Get,
            };
            locationRequest.Headers.Add("Accept", "application/json");
            HttpResponseMessage locationResponse = await _client.SendAsync(locationRequest);
            string content = locationResponse.Content.ReadAsStringAsync().Result;
            List<City> location = JsonConvert.DeserializeObject<List<City>>(content);
            City city = location.Where(x => x.Rank == location.Max(y => y.Rank)).FirstOrDefault();
            HttpRequestMessage forecastRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(ConfigurationManager.AppSettings["AccuWeatherHost"] +
                ConfigurationManager.AppSettings["AccuWeatherForecast"] +
                 city.Key +
                "?apikey=" + ConfigurationManager.AppSettings["AccuWeatherApiKey"] +
                "&language=ru-ru" +
                "&details=true" +
                "&metric=true"),
            };
            forecastRequest.Headers.Add("Accept", "application/json");
            HttpResponseMessage forecastResponse = await _client.SendAsync(forecastRequest);
            content = forecastResponse.Content.ReadAsStringAsync().Result;
            Forecast forecast = JsonConvert.DeserializeObject<Forecast>(content);
            AdaptiveCard card = new AdaptiveCard
            {
                Body = new List<AdaptiveElement>()
                    {
                        new AdaptiveTextBlock
                        {
                            Text = city.LocalizedName,
                            Size = AdaptiveTextSize.Large,
                        },
                        new AdaptiveTextBlock
                        {
                            Text = $"{forecast.Headline.EffectiveDate.Day}/{forecast.Headline.EffectiveDate.Month}/{forecast.Headline.EffectiveDate.Year}",
                        },
                        new AdaptiveColumnSet
                        {
                            Columns = new List<AdaptiveColumn>
                            {
                                new AdaptiveColumn
                                {
                                    Width = AdaptiveColumnWidth.Auto,
                                    Items = new List<AdaptiveElement>
                                    {
                                        new AdaptiveImage
                                        {
                                            Url = new Uri("http://messagecardplayground.azurewebsites.net/assets/Mostly%20Cloudy-Square.png"),
                                            Size = AdaptiveImageSize.Small,
                                        },
                                    },
                                },
                                new AdaptiveColumn
                                {
                                    Width = AdaptiveColumnWidth.Auto,
                                    Items = new List<AdaptiveElement>
                                    {
                                        new AdaptiveTextBlock
                                        {
                                            Text = $"{forecast.DailyForecasts[0].Temperature.Minimum.Value}",
                                            Size = AdaptiveTextSize.ExtraLarge,
                                            Spacing = AdaptiveSpacing.None,
                                        },
                                    },
                                },
                                new AdaptiveColumn
                                {
                                    Width = AdaptiveColumnWidth.Auto,
                                    Items = new List<AdaptiveElement>
                                    {
                                        new AdaptiveTextBlock
                                        {
                                            Size = AdaptiveTextSize.Small,
                                            Text = $"RealFeel: {forecast.DailyForecasts[0].RealFeelTemperature.Minimum.Value}",
                                            HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                                        },
                                        new AdaptiveTextBlock
                                        {
                                            Size = AdaptiveTextSize.Small,
                                            Text = $"{forecast.Headline.Text}",
                                            HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                                        },
                                    },
                                },
                            },
                        },
                    },
                Actions = new List<AdaptiveAction>
                    {
                        new AdaptiveOpenUrlAction
                        {
                            Title = "Посмотреть подробнее",
                            Url = new Uri($"{forecast.Headline.Link}"),
                        },
                    },
            };
            card.Version = "1.0";
            card.Type = AdaptiveCard.TypeName;
            Attachment attachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
            responseMessage.Attachments.Add(attachment);
            await context.PostAsync(responseMessage);

            context.Wait(MessageReceivedAsync);
        }
    }
}