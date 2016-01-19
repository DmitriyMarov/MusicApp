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

        //TODO: настройки порядка отображения у пользователей; топ самых частых запросов
        public IActionResult Index(string searchString)
        {
            string chordsText = String.Empty;
            string chordsImages = String.Empty;
            string songText = String.Empty;
            string songTranslate = String.Empty;

            if (searchString == null)
                searchString = "Guns N' Roses - Knocking On Heaven's Door";

            //Получает id видео с песней на youtube
            ViewData["videoId"] = GetYoutubeVideo(searchString);

            //ищет перевод песни, если не находит то ищет просто текст
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
            //ищет аккорды песни
            FindSongChords(searchString, ref chordsText, ref chordsImages);
            if (chordsText != String.Empty)
            {
                ViewData["chordsText"] = chordsText;
                if (chordsImages != String.Empty)
                    ViewData["chordsImages"] = chordsImages;
                else ViewData["chordsImages"] = "<p>Аппликатур не найден</p>";
            }
            else
            {
                ViewData["chordsText"] = "<p>Аккордов не найдено</p>";
            }

            //ищет дополнительную информацию по песне: itunes, google music, spotify, last.fm и т.д.
            ViewData["itunesLink"] = GetItunesLink(searchString);
            ViewData["lastfmLink"] = GetLastfmLink(searchString);


            return View();
        }


        /// <summary>
        /// Берёт первое в выборке видео по запросу через апи ютуба
        /// </summary>
        public string GetYoutubeVideo(string searchString)
        {
            string videoId = String.Empty;
            string query = "https://www.googleapis.com/youtube/v3/search?part=snippet&maxResults=1&q=" + Uri.EscapeDataString(searchString) + "&key=AIzaSyA7W5ZNLIRIfxh8jV6vM9oXkDcN7AUH4uA";
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
            string lyricsText = FindSongLyricsMusixmatch(searchString);
            if (lyricsText == String.Empty)
                FindSongLyricsChartlyrics(searchString);
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
            string songURL = String.Empty;
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
            //Осуществить поиск по сайту будет трудно, но ссылки имеют вид www.musixmatch.com/lyrics/artist/song (где вместо пробелов и других знаков стоят тире), так что пока применю прямые URL

            //string[] searchStringSplit = searchString.Split('-');
            //if(searchStringSplit.Length<2)
            //    searchStringSplit = searchString.Split('–');
            //Regex rgx = new Regex("[^a-zA-Zа-яА-Я0-9 ]");
            //string artist = rgx.Replace(searchStringSplit[0], "");
            //artist = artist.Replace(" ", "-");
            //string song = rgx.Replace(searchStringSplit[1], "");
            //song = song.Replace(" ", "-");
            //string url = "https://www.musixmatch.com/lyrics/" + artist + "/" + song;

            //Ищем через API гугла песню по запросу, берём первую в списке
            string searchQuery = "http://ajax.googleapis.com/ajax/services/search/web?v=1.0&cx=011954953197928556725:rggrcphoxmc&num=1&q=" + searchString;
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
            lyricsText = HtmlParsingMusixmatch(songURL);
            return lyricsText;
        }

        /// <summary>
        /// Парсит HTML страницы с текстом и переводом песни на амальгаме
        /// </summary>
        public string HtmlParsingMusixmatch(string url)
        {
            string lyricsText = String.Empty;
            using (HttpClient http = new HttpClient())
            {
                var response = http.GetByteArrayAsync(url).Result;
                String source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                source = WebUtility.HtmlDecode(source);
                HtmlDocument resultat = new HtmlDocument();
                resultat.LoadHtml(source);
                List<HtmlNode> songText = resultat.DocumentNode.Descendants("span").Where(x => x.Attributes["id"] != null && x.Attributes["id"].Value.Contains("lyrics-html")).ToList();
                if (songText.Count != 0)
                {
                    string[] songTextArray = songText[0].InnerText.Split(new string[] { "\n" }, StringSplitOptions.None);
                    foreach (var item in songTextArray)
                    {
                        if (item != "")
                            lyricsText = lyricsText + " <p>" + item + "</p>";
                        else lyricsText = lyricsText + " <br>";
                    }
                    lyricsText = lyricsText + " <br>";
                    lyricsText = lyricsText + " <p>Lyrics powered by www.musiXmatch.com</p>";
                }
            }
            return lyricsText;
        }

        #endregion

        #region Поиск лирики с переводом
        /// <summary>
        /// Ищет перевод на амальгаме, если находит то берёт оттуда ещё и текст
        /// </summary>
        public void FindSongTranslation(string searchString, ref string songText, ref string songTranslate)
        {
            string songURL = String.Empty;
            string searchQuery = "http://ajax.googleapis.com/ajax/services/search/web?v=1.0&cx=011954953197928556725:o0_e_xj5cjg&num=1&q=" + searchString;

            //ищем ссылку на перевод
            //TODO: поиск слишком "мягкий", если искомой нет, то всё равно может выдать песню, где в названии хотя бы одно слово совпадает
            //заняться этим, если название песни на в какой-то степени не совпадает с введённым, считать, что поиск ничего не дал
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

            HtmlParsingAmalgama(songURL, ref songText, ref songTranslate);
        }

        /// <summary>
        /// Парсит HTML страницы с текстом и переводом песни на амальгаме
        /// </summary>
        public void HtmlParsingAmalgama(string url, ref string songText, ref string songTranslate)
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

        #endregion

        #region Поиск аккордов
        /// <summary>
        /// Ищет аккорды на amdmd.ru
        /// </summary>
        public void FindSongChords(string searchString, ref string chordsText, ref string chordsImages)
        {
            string songURL = String.Empty;
            //string searchQuery = "http://ajax.googleapis.com/ajax/services/search/web?v=1.0&q=site:amdm.ru%20" + searchString + "&num=1";
            string searchQuery = "http://ajax.googleapis.com/ajax/services/search/web?v=1.0&cx=011954953197928556725:i_zav28jkea&q=" + searchString + "&key=AIzaSyA7W5ZNLIRIfxh8jV6vM9oXkDcN7AUH4uA";
            
            //ищем ссылку на перевод
            //TODO: почему-то через api не всегда находит что нужно, даже если напрямую через гугл - первая ссылка, разобраться
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

            HtmlParsingAmDm(songURL, ref chordsText, ref chordsImages);
        }


        /// <summary>
        /// Парсит HTML страницы с текстом и переводом песни на амальгаме
        /// </summary>
        public void HtmlParsingAmDm(string url, ref string chordsText, ref string chordsImages)
        {
            using (HttpClient http = new HttpClient())
            {
                var response = http.GetByteArrayAsync(url).Result;
                String source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                source = WebUtility.HtmlDecode(source);
                HtmlDocument resultat = new HtmlDocument();
                resultat.LoadHtml(source);

                //Парсинг текста песни с аккордами
                List<HtmlNode> songText = resultat.DocumentNode.Descendants("pre").Where(x => x.Attributes["itemprop"] != null && x.Attributes["itemprop"].Value.Contains("chordsBlock")).ToList();
                if (songText.Count != 0)
                {
                    //string[] songTextArray = songText[0].InnerText.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);
                    //foreach (var item in songTextArray)
                    //{
                    //    if (item != "" || item != " ")
                    //        chordsText = chordsText + " <p>" + item + "</p>";
                    //    else chordsText = chordsText + " <br>";
                    //}
                    //chordsText = chordsText + " <br>";
                    //chordsText = chordsText + " <p>Аккорды представлены сайтом amdm.ru</p>";
                    chordsText = songText[0].InnerText;
                    chordsText += "\n\n____________________________________";
                    chordsText +=  "\n\nАккорды представлены сайтом amdm.ru";
                    //Парсинг изображений аккордов
                    List<HtmlNode> songChords = resultat.DocumentNode.Descendants("div").Where(x => x.Attributes["id"] != null && x.Attributes["id"].Value.Contains("song_chords")).ToList();
                    foreach (var item in songChords)
                    {
                        chordsImages += item.InnerHtml;
                    }
                }
            }
        }

        #endregion

        #region дополнительная информация о песне

        /// <summary>
        /// Получает ссылку на песню в iTunes
        /// </summary>
        public string GetItunesLink(string searchString)
        {
            string songURL = String.Empty;
            string searchQuery = "https://itunes.apple.com/search?term=" + Uri.EscapeDataString(searchString) + "&limit=1";
                using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = client.GetAsync(searchQuery).Result)
            {
                if (response.IsSuccessStatusCode)
                {
                    using (HttpContent content = response.Content)
                    {
                        var result = content.ReadAsStringAsync().Result;
                        JObject JObj = (JObject)JsonConvert.DeserializeObject(result);
                        var entry = JObj["results"];
                        foreach (var item in entry)
                        {
                            songURL = item["trackViewUrl"].ToString();
                            break;
                        }
                    }
                }
            }
            return songURL;
        }

        /// <summary>
        /// Получает ссылку в last.fm
        /// </summary>
        public string GetLastfmLink(string searchString)
        {
            string songURL = String.Empty;
            string searchQuery = "http://ws.audioscrobbler.com/2.0/?method=track.search&track=" + Uri.EscapeDataString(searchString) + "&limit=1&api_key=a54fb212c086eb69661f81a98fe56c92&format=json";
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = client.GetAsync(searchQuery).Result)
            {
                if (response.IsSuccessStatusCode)
                {
                    using (HttpContent content = response.Content)
                    {
                        var result = content.ReadAsStringAsync().Result;
                        JObject JObj = (JObject)JsonConvert.DeserializeObject(result);
                        var entry = JObj["results"];
                        var entry1 = entry["trackmatches"];
                        var entry2 = entry1["track"];
                        foreach (var item in entry1)
                        {
                            foreach (var item1 in item)
                            {
                                foreach (var item2 in item1)
                                {
                                    songURL = item2["url"].ToString();
                                    break;
                                }
                                break;
                            }
                            break;
                        }
                    }
                }
            }
            songURL = songURL.Replace("last.fm/", "last.fm/ru/");
            return songURL;
        }

        #endregion









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
