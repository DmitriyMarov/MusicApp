using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net;
using System.Xml.XPath;
using HtmlAgilityPack;
using System.Security.Cryptography;
using System.Text.RegularExpressions;


//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Services;
//using Google.Apis.Upload;
//using Google.Apis.Util.Store;
//using Google.Apis.YouTube.v3;
//using Google.Apis.YouTube.v3.Data;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MusicApp.Controllers
{
    public class MusicSearchController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index(string searchString)
        {
            if (searchString == null)
                searchString = "Guns N' Roses - Knocking On Heaven's Door";

            ViewData["videoId"] = GetYoutubeVideo(searchString);

            string songText = String.Empty;
            string songTranslate = String.Empty;
            FindSongTranslation(searchString, ref songText, ref songTranslate);
            if (songText == String.Empty)
            {
                ViewData["songText"] = FindSongLyrics(searchString);
                ViewData["songTranslate"] = "<p>Перевод не найден</p>";
            }
            else
            {
                ViewData["songText"] = songText;
                ViewData["songTranslate"] = songTranslate;
            }
            return View();
        }


        /// <summary>
        /// Берёт первое в выборке видео по запросу через апи ютуба
        /// </summary>
        public string GetYoutubeVideo(string searchString)
        {
            string videoId = String.Empty;
            string query = "https://www.googleapis.com/youtube/v3/search?part=snippet&maxResults=1&q=" + Uri.EscapeDataString(searchString) + "&key=AIzaSyCjzzS8bOYKLw0uuTDwWmoMEKr9MpAlvxw";
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = client.GetAsync(query).Result)
            {
                if (response.IsSuccessStatusCode)
                {
                    using (HttpContent content = response.Content)
                    {
                        // ... Read the string.
                        var result = content.ReadAsStringAsync().Result;
                        JObject JObj = (JObject)JsonConvert.DeserializeObject(result);
                        var entry = JObj["items"];
                        foreach (var item in entry)
                        {
                            var videoDesc = item["id"];
                            videoId = videoDesc["videoId"].ToString();
                        }
                    }
                }
            }
            return videoId;
        }

        #region Поиск лирики без перевода

        /// <summary>
        /// Ищет лирику на разных сайтах, если не найден перевод на амальгаме
        /// </summary>
        public string FindSongLyrics(string searchString)
        {
            //пока только один
            string lyricsText = FindSongLyricsMusixmatch(searchString);
            return lyricsText;
        }

        /// <summary>
        /// Ищет лирику на Chartlyrics
        /// </summary>
        public string FindSongLyricsChartlyrics(string searchString)
        {
            string[] searchStringSplit = searchString.Split('-');
            string artist = Uri.EscapeDataString(searchStringSplit[0]);
            string song = Uri.EscapeDataString(searchStringSplit[1]);
            string query = "http://api.chartlyrics.com/apiv1.asmx/SearchLyricDirect?artist=" + artist + "&song=" + song;
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = client.GetAsync(query).Result)
            {
                if (response.IsSuccessStatusCode)
                {
                    using (HttpContent content = response.Content)
                    {
                        // ... Read the string.
                        var result = content.ReadAsStringAsync().Result;
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// Ищет лирику на musixmatch
        /// </summary>
        public string FindSongLyricsMusixmatch(string searchString)
        {
            string lyricsText = String.Empty;
            #region Поиск через API, возвращает 30% лирики, не катит, но вдруг пригодится

            #region track_id search
            //List<string> track_ids = new List<string>();
            //string query = "http://api.musixmatch.com/ws/1.1/track.search?q_track=" + Uri.EscapeDataString(searchString) + "&f_has_lyrics=1&apikey=97d0f66763309084b5163502bc2e9d8a";
            //using (HttpClient client = new HttpClient())
            //using (HttpResponseMessage response = client.GetAsync(query).Result)
            //{
            //    if (response.IsSuccessStatusCode)
            //    {
            //        using (HttpContent content = response.Content)
            //        {
            //            var result = content.ReadAsStringAsync().Result;
            //            JObject JObj = (JObject)JsonConvert.DeserializeObject(result);
            //            var entry = JObj["message"];

            //            //получаем id всех треков в списке (какого-то одного нельзя, не у всех есть лирика, даже при установленом флаге поиска)

            //            foreach (var track_list in entry["body"])
            //            {
            //                foreach (var track in track_list)
            //                {
            //                    foreach (var item in track)
            //                    {
            //                        foreach (var item1 in item)
            //                        {
            //                            foreach (var item2 in item1)
            //                            {
            //                                track_ids.Add(item2["track_id"].ToString());
            //                            }
            //                        }
            //                    }
            //                    break;
            //                }
            //                break;
            //            }
            //        }
            //    }
            //}
            #endregion
            #region lyrics get

            //получаем лирику для данного трека, если лирика пустая, получаем для следующего
            //foreach (string id in track_ids)
            //{
            //    string lyricsQuery = "http://api.musixmatch.com/ws/1.1/track.lyrics.get?track_id=" + id + "&apikey=97d0f66763309084b5163502bc2e9d8a";
            //    using (HttpClient client = new HttpClient())
            //    using (HttpResponseMessage response = client.GetAsync(lyricsQuery).Result)
            //    {
            //        if (response.IsSuccessStatusCode)
            //        {
            //            using (HttpContent content = response.Content)
            //            {
            //                var result = content.ReadAsStringAsync().Result;
            //                JObject JObj = (JObject)JsonConvert.DeserializeObject(result);
            //                var entry = JObj["message"];


            //                foreach (var lyrics in entry["body"])
            //                {
            //                    foreach (var item in lyrics)
            //                    {
            //                        lyricsText = item["lyrics_body"].ToString();
            //                        break;
            //                    }
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //    if (lyricsText != String.Empty)
            //        break;
            #endregion

            #endregion

            //API не вариант, но реально пропарсить лирику
            //Осуществить поиск по сайту будет трудно, но ссылки имеют вид www.musixmatch.com/lyrics/artist/song (где вместо пробелов и других знаков стоят тире), так что применю прямые URL

            string[] searchStringSplit = searchString.Split('-');
            if(searchStringSplit.Length<2)
                searchStringSplit = searchString.Split('–');
            Regex rgx = new Regex("[^a-zA-Z0-9 ]");
            string artist = rgx.Replace(searchStringSplit[0], "");
            artist = artist.Replace(" ", "-");
            string song = rgx.Replace(searchStringSplit[1], "");
            song = song.Replace(" ", "-");
            string url = "https://www.musixmatch.com/lyrics/" + artist + "/" + song;
            using (HttpClient http = new HttpClient())
            {
                var response = http.GetByteArrayAsync(url).Result;
                String source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                source = WebUtility.HtmlDecode(source);
                HtmlDocument resultat = new HtmlDocument();
                resultat.LoadHtml(source);
                List<HtmlNode> translates = resultat.DocumentNode.Descendants("span").Where(x => x.Attributes["id"] != null && x.Attributes["id"].Value.Contains("lyrics-html")).ToList();
                string[] songTextArray = translates[0].InnerText.Split(new string[] { "\n" }, StringSplitOptions.None);
                foreach (var item in songTextArray)
                {
                    if (item != "")
                        lyricsText = lyricsText + " <p>" + item + "</p>";
                    else lyricsText = lyricsText + " <br>";
                }
                lyricsText = lyricsText + " <br>";
                lyricsText = lyricsText + " <p>Lyrics powered by www.musiXmatch.com</p>";
            }

            return lyricsText;
        }

        #endregion

    /// <summary>
    /// Ищет перевод на амальгаме, если находит то берёт оттуда ещё и текст
    /// </summary>
    public void FindSongTranslation(string searchString, ref string songText, ref string songTranslate)
        {
            string songURL = String.Empty;
            string searchQuery = "http://ajax.googleapis.com/ajax/services/search/web?v=1.0&cx=partner-pub-6158979219687853:6425061692&num=1&q=" + searchString;

            //ищем ссылку на перевод
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = client.GetAsync(searchQuery).Result)
            {
                if (response.IsSuccessStatusCode)
                {
                    using (HttpContent content = response.Content)
                    {
                        var result = content.ReadAsStringAsync().Result;
                        JObject JObj = (JObject)JsonConvert.DeserializeObject(result);
                        var entry = JObj["responseData"];
                        foreach (var item in entry)
                        {
                            foreach (var element in item)
                            {
                                foreach (var element1 in element)
                                {
                                    songURL = element1["unescapedUrl"].ToString();
                                    break;
                                }
                                break;
                            }
                            break;
                        }
                    }
                }
            }


            if (songURL == String.Empty)
                return;

            HtmlParsing(songURL, ref songText, ref songTranslate);
        }

        /// <summary>
        /// Парсит HTML страницы с текстом и переводом песни на амальгаме
        /// </summary>
        public void HtmlParsing(string url, ref string songText, ref string songTranslate)
        {
            using (HttpClient http = new HttpClient())
            {
                var response = http.GetByteArrayAsync(url).Result;
                String source = Encoding.GetEncoding("windows-1251").GetString(response, 0, response.Length - 1);
                source = WebUtility.HtmlDecode(source);
                HtmlDocument resultat = new HtmlDocument();
                resultat.LoadHtml(source);
                List<HtmlNode> translates = resultat.DocumentNode.Descendants().Where(x => (x.Name == "div" && x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("translate"))).ToList();
                List<HtmlNode> originals = resultat.DocumentNode.Descendants().Where(x => (x.Name == "div" && x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("original"))).ToList();
                foreach (var item in originals)
                {
                    if (item.InnerText != "")
                        songText = songText + " <p>" + item.InnerText + "</p>";
                    else songText = songText + " <br>";
                }
                if (songText != String.Empty)
                {
                    songText = songText + " <br>";
                    songText = songText + " <p>Текст песни представлен www.amalgama-lab.com</p>";
                }

                foreach (var item in translates)
                {
                    if (item.InnerText != "")
                        songTranslate = songTranslate + " <p>" + item.InnerText + "</p>";
                    else songTranslate = songTranslate + " <br>";
                }
                if (songTranslate != String.Empty)
                {
                    songTranslate = songTranslate + " <br>";
                    songTranslate = songTranslate + " <p>Перевод песни представлен www.amalgama-lab.com</p>";
                }
            }
        }

            //Youtube API не поддерживает .NET 5.0, поэтому пока придётся извращаться
            //Теперь всё работает, потом посмотреть и этот метод, хотя тот пока работает

            //public string GetYoutubeVideoAPI(string searchString)
            //{
            //var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            //{
            //    ApiKey = "AIzaSyCjzzS8bOYKLw0uuTDwWmoMEKr9MpAlvxw",
            //    ApplicationName = this.GetType().ToString()
            //});

            //var searchListRequest = youtubeService.Search.List("snippet");
            //searchListRequest.Q = searchString; // Replace with your search term.
            //searchListRequest.MaxResults = 10;

            //// Call the search.list method to retrieve results matching the specified query term.
            //var searchListResponse = searchListRequest.Execute();

            //List<string> videos = new List<string>();
            //List<string> channels = new List<string>();
            //List<string> playlists = new List<string>();

            //// Add each result to the appropriate list, and then display the lists of
            //// matching videos, channels, and playlists.
            //foreach (var searchResult in searchListResponse.Items)
            //{
            //    switch (searchResult.Id.Kind)
            //    {
            //        case "youtube#video":
            //            videos.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.VideoId));
            //            break;

            //        case "youtube#channel":
            //            channels.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.ChannelId));
            //            break;

            //        case "youtube#playlist":
            //            playlists.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.PlaylistId));
            //            break;
            //    }
            //}

            //    return null;
            //}
        }
}
