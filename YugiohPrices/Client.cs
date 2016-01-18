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
        public Card Card;
        [JsonProperty("name")]
        public string SetName;
        [JsonProperty("print_tag")]
        public string PrintTag;
        [JsonProperty("rarity")]
        public string Rarity;
        [JsonProperty("listings")]
        public string[] Listings;

        public PriceData PriceData;
    }
    public struct PriceData
    {
        [JsonProperty("high")]
        public decimal HighPrice;
        [JsonProperty("average")]
        public decimal AveragePrice;
        [JsonProperty("low")]
        public decimal LowPrice;
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
        public static string BaseURL = "http://yugiohprices.com/api/";
        public static string CardData = "card_data/";
        public static string CardPrices = "get_card_prices/";
        public static string CardImage = "card_image/";
    }

    internal class WebWrapper
    {
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

    public class YugiohPricesSearcher
    {
        public YugiohPricesSearcher() { }

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
