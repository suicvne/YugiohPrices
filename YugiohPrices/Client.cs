using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace YugiohPrices
{
    public enum CardType
    {
        Trap, Spell, Monster
    }

    public struct Card
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("text")]
        public string Description;
        [JsonProperty("card_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public CardType Type;
        [JsonProperty("type")]
        public string MonsterType;
        [JsonProperty("family")]
        public string Attribute;
        [JsonProperty("atk")]
        public int? Attack;
        [JsonProperty("def")]
        public int? Defense;
        [JsonProperty("level")]
        public int? Level;
        [JsonProperty("property")]
        public string ExtraProperties;

        public Image CardImage;
    }

    public struct CardPrices
    {
        /// <summary>
        /// The card these prices belong to.
        /// </summary>
        public Card Card;

        /// <summary>
        /// The name of the set these prices came from.
        /// </summary>
        [JsonProperty("name")]
        public string SetName;

        /// <summary>
        /// Print tag assosciated with the card from the set.
        /// </summary>
        [JsonProperty("print_tag")]
        public string PrintTag;

        /// <summary>
        /// Rarity of the card.
        /// </summary>
        [JsonProperty("rarity")]
        public string Rarity;

        /// <summary>
        /// Unknown yet.
        /// </summary>
        [JsonProperty("listings")]
        public string[] Listings;

        /// <summary>
        /// An object containing the price data of this card.
        /// </summary>
        public PriceData PriceData;
    }
    public struct PriceData
    {
        /// <summary>
        /// The highest price, in USD.
        /// Use .ToString("C") to automatically format to money.
        /// </summary>
        [JsonProperty("high")]
        public decimal HighPrice;

        /// <summary>
        /// The average price of the card, in USD.
        /// Use .ToString("C") to automatically format to money.
        /// </summary>
        [JsonProperty("average")]
        public decimal AveragePrice;

        /// <summary>
        /// The lowest price of the card. If you paid this, you're lucky. Of course, in USD.
        /// Use .ToString("C") to automatically format to money.
        /// </summary>
        [JsonProperty("low")]
        public decimal LowPrice;

        /// <summary>
        /// The timestamp assosciated with the time the listing was last updated.
        /// </summary>
        [JsonProperty("updated_at")]
        public DateTime LastUpdated;

        [JsonProperty("shift")]
        public decimal Shift;
        [JsonProperty("shift_3")]
        public decimal Shift3;
        [JsonProperty("shift_7")]
        public decimal Shift7;
        [JsonProperty("shift_30")]
        public decimal Shift30;
        [JsonProperty("shift_90")]
        public decimal Shift90;
        [JsonProperty("shift_180")]
        public decimal Shift180;
        [JsonProperty("shift_365")]
        public decimal Shift365;
    }

    internal class Endpoints
    {
        /// <summary>
        /// Base URL
        /// </summary>
        public static string BaseURL = "http://yugiohprices.com/api/";

        /// <summary>
        /// For retrieving card data like name, description, attribute, etc.
        /// </summary>
        public static string CardData = "card_data/";

        /// <summary>
        /// For retrieving card prices.
        /// </summary>
        public static string CardPrices = "get_card_prices/";

        /// <summary>
        /// For retrieving the card's image.
        /// </summary>
        public static string CardImage = "card_image/";
    }

    internal class WebWrapper
    {
        /// <summary>
        /// performs a get request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> Get(string url)
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(url).ConfigureAwait(false))
                {
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Internally performs a get request and returns an image.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<Image> GetImage(string url)
        {
            using (var wc = new WebClient())
            {
                byte[] rawImageBytes = await Task.Run(()=>wc.DownloadData(new Uri(url))).ConfigureAwait(false);
                var ms = new MemoryStream(rawImageBytes);
                return new Bitmap(ms);
            }
        }
    }

    /// <summary>
    /// A wrapper around the http://yugiohprices.com/ database.
    /// </summary>
    public class YugiohPricesSearcher
    {
        public YugiohPricesSearcher() { }

        /// <summary>
        /// Returns a Card object found by its name.
        /// </summary>
        /// <param name="name">The, case sensitive, name of the card to retrieve.</param>
        /// <returns>A card object. What else did you expect?</returns>
        /// <exception cref="HttpRequestException">If the 'status' of the request is not success, this exception will throw with the message from the Json as the exception's message.</exception>
        public async Task<Card> GetCardByName(string name)
        {
            JObject message = JObject.Parse(await WebWrapper.Get(Endpoints.BaseURL + Endpoints.CardData + name).ConfigureAwait(false));
            if(message["status"].ToString() == "success")
            {
                Card c = new Card();
                c.Name = "<NULL CARD>";
                await Task.Factory.StartNew(() =>
                {
                    c = JsonConvert.DeserializeObject<Card>(message["data"].ToString());
                    
                }).ConfigureAwait(false);
                if (c.Name != "<NULL CARD>")
                {
                    c.CardImage = await WebWrapper.GetImage(Endpoints.BaseURL + Endpoints.CardImage + name).ConfigureAwait(false);
                    return c;
                }
            }
            else
            {
                throw new HttpRequestException(message["message"].ToString());
            }

            Card nil = new Card();
            nil.Name = "<NULL CARD>";
            return nil;
        }

        /// <summary>
        /// Gets *all* of the card's prices.
        /// </summary>
        /// <param name="name">The case sensitive name of the card.</param>
        /// <returns>An array of CardPrices objects. Remember: CardPrices has a field called Card that contains the Card object so you don't have to perform a GetCardByName also.</returns>
        /// <exception cref="HttpRequestException">If the 'status' of the request is not success, this exception will throw with the message from the Json as the exception's message.</exception>
        public async Task<CardPrices[]> GetAllCardPricesByName(string name)
        {
            JObject message = JObject.Parse(await WebWrapper.Get(Endpoints.BaseURL + Endpoints.CardPrices + name).ConfigureAwait(false));
            if(message["status"].ToString() == "success")
            {
                List<CardPrices> list = new List<CardPrices>();
                Card card = await GetCardByName(name).ConfigureAwait(false);
                await Task.Factory.StartNew(() => 
                {
                    JArray data = JArray.Parse(message["data"].ToString());
                    foreach(var priceDataObject in data.Children())
                    {
                        CardPrices p = JsonConvert.DeserializeObject<CardPrices>(priceDataObject.ToString());
                        if(priceDataObject["price_data"]["status"].ToString() == "success")
                        {
                            p.PriceData = JsonConvert.DeserializeObject<PriceData>(priceDataObject["price_data"]["data"]["prices"].ToString());
                        }
                        p.Card = card;
                        list.Add(p);
                    }
                });
                if (list.Count > 0)
                    return list.ToArray();
            }
            else
            {
                throw new HttpRequestException(message["message"].ToString());
            }
            return null;
        }

        /// <summary>
        /// Retrieves the first card price assosciated with the card.
        /// </summary>
        /// <param name="name">The case sensitive name of the card to base the search off of.</param>
        /// <returns>A CardPrices object.</returns>
        /// <exception cref="HttpRequestException">If the 'status' of the request is not success, this exception will throw with the message from the Json as the exception's message.</exception>
        public async Task<CardPrices> GetCardPricesByName(string name)
        {
            JObject message = JObject.Parse(await WebWrapper.Get(Endpoints.BaseURL + Endpoints.CardPrices + name).ConfigureAwait(false));
            if(message["status"].ToString() == "success")
            {
                CardPrices prices = new CardPrices();
                await Task.Factory.StartNew(() => 
                {
                    JArray data = JArray.Parse(message["data"].ToString());
                    if (data.Count > 0)
                    {
                        prices = JsonConvert.DeserializeObject<CardPrices>(data[0].ToString());
                        if (data[0]["price_data"]["status"].ToString() == "success")
                        {
                            prices.PriceData = JsonConvert.DeserializeObject<PriceData>(data[0]["price_data"]["data"]["prices"].ToString());
                        }
                    }
                });
                prices.Card = await GetCardByName(name).ConfigureAwait(false);
                return prices;
            }
            else
            {
                throw new HttpRequestException(message["message"].ToString());
            }
        }
    }
}
