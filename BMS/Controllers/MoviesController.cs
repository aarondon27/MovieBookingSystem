using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BMS.Data;
using BMS.Models;
using BMS.Services;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Eventing.Reader;
using Microsoft.AspNetCore.Authorization;

namespace BMS.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TmdbService _tmdb;

        public MoviesController(ApplicationDbContext context, TmdbService tmdb)
        {
            _context = context;
            _tmdb = tmdb;
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }

        public async Task<IActionResult> NowPlayingFromApi()
        {
            var data = await _tmdb.GetNowPlayingAsync();

            var movies = data["results"]!
                .Select(m => new Movie
                {   
                    TmdbId = (int)m["id"]!,
                    Title = (string)m["title"]!,
                    Description = (string)m["overview"]!,
                    PosterFileName = "https://image.tmdb.org/t/p/w500" + (string)m["poster_path"]!,
                    Duration = 120,
                    AgeRating = "U/A"
                })
                .ToList();

            return View(movies);
        }

        public async Task<IActionResult> DetailsFromApi(int id)
        {
            //Get the details info from TMDB
            var data = await _tmdb.GetMovieDetailsAsync(id);
            var releaseData = await _tmdb.GetMovieReleaseDatesAsync(id);

            string certification = "N/A";

            //IMDB Rating
            double imdbRating = data["vote_average"] != null
                ? Math.Round((double)data["vote_average"]!, 1)
                : 0.0;

            //Vote count for credibility
            int voteCount = data["vote_count"] != null
                ? (int)data["vote_count"]!
                : 0;


            //Get the certification
            try
            {
                var results = releaseData["results"];

                if (results != null)
                {
                    // Try India rating first
                    var india = results
                        .FirstOrDefault(r => (string)r["iso_3166_1"]! == "IN");

                    if (india != null)
                    {
                        var releaseInfo = india["release_dates"]?
                            .FirstOrDefault(r => !string.IsNullOrWhiteSpace((string)r["certification"]!));

                        if (releaseInfo != null)
                            certification = (string)releaseInfo["certification"]!;
                    }

                    // If still missing → use US MPAA rating as fallback
                    if (certification == "N/A")
                    {
                        var us = results
                            .FirstOrDefault(r => (string)r["iso_3166_1"]! == "US");

                        if (us != null)
                        {
                            var releaseInfo = us["release_dates"]?
                                .FirstOrDefault(r => !string.IsNullOrWhiteSpace((string)r["certification"]!));

                            if (releaseInfo != null)
                                certification = (string)releaseInfo["certification"]!;
                        }
                    }
                }
            }
            catch { certification = "N/A"; }

            var movie = new Movie
            {
                TmdbId = id,
                Title = (string)data["title"]!,
                Description = (string)data["overview"]!,
                PosterFileName = "https://image.tmdb.org/t/p/w500" + (string)data["poster_path"]!,
                Duration = (int?)data["runtime"] ?? 0,
                AgeRating = certification,
                ImdbRating = imdbRating,
                VoteCount = voteCount
            };

            HttpContext.Session.SetString("PosterUrl", movie.PosterFileName);

            return View(movie);
        }

        [Authorize]
        public async Task<IActionResult> Book(int movieid)
        {
            var data = await _tmdb.GetMovieDetailsAsync(movieid);

            var movie = new Movie
            {
                TmdbId = movieid,
                Title = (string)data["title"]!,
                Description = "", 
                AgeRating = "NA"
            };



            var dates = Enumerable.Range(0, 7)
                .Select(i => DateTime.Now.AddDays(i).ToString("ddd, dd MMM"))
                .ToList();

            var theatres = new Dictionary<string, List<string>>
            {
                { "PVR Phoenix Marketcity", new List<string> { "10:00 AM", "01:00 PM", "04:00 PM", "07:00 PM" } },
                { "INOX Mall of India",     new List<string> { "11:00 AM", "02:00 PM", "05:00 PM" } },
                { "Cinepolis Andheri",      new List<string> { "12:00 PM", "03:30 PM", "06:30 PM" } }
            };

            var vm = new BookingPage
            {
                Movie = movie,
                Dates = dates,
                Theatres = theatres
            };

            return View(vm);
        }

        public async Task<IActionResult> SelectSeats(int movieId, string date, string time, string theatre, string format)
        {
            //Fetch Id and Title for Select Seats page
            var data = await _tmdb.GetMovieDetailsAsync(movieId);
            var Title = (string)data["title"]!;

            //Fetch info about all booked seats from DB
            var bookedSeats = await _context.BookedSeats
                .Where(s => s.MovieId == movieId
                    && s.Date == date
                    && s.Time == time
                    && s.Theatre == theatre
                    && s.Format == format)
                .Select(s => s.SeatNumber)
                .ToListAsync();

            ViewData["MovieId"] = movieId;
            ViewData["Title"] = Title;
            ViewData["Date"] = date;
            ViewData["Time"] = time;
            ViewData["Theatre"] = theatre;
            ViewData["Format"] = format;


            //Pass the booked seats info to UI
            ViewBag.BookedSeats = bookedSeats;

            HttpContext.Session.SetString("MovieId", movieId.ToString());
            HttpContext.Session.SetString("Title", Title);
            HttpContext.Session.SetString("ShowDate", date);
            HttpContext.Session.SetString("ShowTime", time);
            HttpContext.Session.SetString("Theatre", theatre);
            HttpContext.Session.SetString("Format", format);

            return View();
        }

        public IActionResult Payment(string seats, int total)
        {
            ViewData["Seats"] = seats;
            ViewData["Total"] = total;

            ViewData["Title"] = HttpContext.Session.GetString("Title"); // ⭐ Fetch title

            ViewData["MovieId"] = HttpContext.Session.GetString("MovieId");
            ViewData["Date"] = HttpContext.Session.GetString("ShowDate");
            ViewData["Time"] = HttpContext.Session.GetString("ShowTime");
            ViewData["Theatre"] = HttpContext.Session.GetString("Theatre");
            ViewData["Format"] = HttpContext.Session.GetString("Format");

            return View();
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ConfirmPayment(string Seats, int Total)
        {
            if (string.IsNullOrWhiteSpace(Seats))
                return BadRequest("No seats selected.");

            //Get stored booking details
            var userId = User.Identity!.Name; // or GetUserId if needed


            //Split seats: "A1,A2" → ["A1", "A2"]
            var seatList = Seats.Split(',').Select(s => s.Trim()).ToList();

            //Retrieve booking context info from session
            var movieId = int.Parse(HttpContext.Session.GetString("MovieId")!);
            var date = HttpContext.Session.GetString("ShowDate")!;
            var time = HttpContext.Session.GetString("ShowTime")!;
            var theatre = HttpContext.Session.GetString("Theatre")!;
            var format = HttpContext.Session.GetString("Format")!;

            //reate BookedSeat entries
            var bookedSeats = seatList.Select(seat => new BookedSeat
            {
                MovieId = movieId,
                Theatre = theatre,
                Date = date,
                Time = time,
                SeatNumber = seat,
                UserId = userId,
                Format = format,
            }).ToList();

            //Save to DB 
            _context.BookedSeats.AddRange(bookedSeats);
            await _context.SaveChangesAsync();


            return RedirectToAction("BookingConfirmed", new { seats = Seats, total = Total });
        }

        public IActionResult BookingConfirmed(string seats, int total)
        {
            ViewData["Seats"] = seats;
            ViewData["Total"] = total;

            ViewData["Title"] = HttpContext.Session.GetString("Title");
            ViewData["Theatre"] = HttpContext.Session.GetString("Theatre");
            ViewData["Date"] = HttpContext.Session.GetString("ShowDate");
            ViewData["Time"] = HttpContext.Session.GetString("ShowTime");
            ViewData["Format"] = HttpContext.Session.GetString("Format");
            ViewData["Poster"] = HttpContext.Session.GetString("PosterUrl");

            return View();
        }

        public async Task<IActionResult> GetPosterImage(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(url);

                // Determine content type based on URL
                var contentType = url.Contains(".png") ? "image/png" : "image/jpeg";

                return File(imageBytes, contentType);
            }
            catch (Exception)
            {
                // Return a 1x1 transparent pixel if image fails
                return NotFound();
            }
        }
    }
}
