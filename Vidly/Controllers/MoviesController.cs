﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Vidly.Models;
using Vidly.ViewModels;

namespace Vidly.Controllers
{
	public class MoviesController : Controller
	{
		public ApplicationDbContext _context { get; set; }

		public MoviesController()
		{
			this._context = new ApplicationDbContext();
		}

		protected override void Dispose(bool disposing)
		{
			this._context.Dispose();
		}

		public ActionResult Index()
		{
			if (User.IsInRole(RoleName.CanManageMovies))
				return View("List");
			
			return View("ReadOnlyList");
		}

		public ActionResult Details(int id)
		{
			var movies = this._context.Movies.Include(m => m.Genre).SingleOrDefault(m => m.Id == id);

			if (movies == null)
			{
				return HttpNotFound();
			}

			return View(movies);
		}

		[Authorize(Roles = RoleName.CanManageMovies)]
		public ActionResult New()
		{
			var genres = _context.Genres.ToList();
			var ViewModel = new MovieFormViewModel
			{
				Genres = genres
			};

			return View("MovieForm", ViewModel);
		}

		[Authorize(Roles = RoleName.CanManageMovies)]
		public ActionResult Edit(int id)
		{
			var movie = _context.Movies.SingleOrDefault( m => m.Id == id);

			if (movie == null)
			{
				return HttpNotFound();
			}

			var viewModel = new MovieFormViewModel(movie)
			{
				Genres = _context.Genres.ToList()
			};

			return View("MovieForm", viewModel);

		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Save(Movie movie)
		{
			if (!ModelState.IsValid)
			{
				var viewModel = new MovieFormViewModel(movie)
				{
					Genres = _context.Genres.ToList()
				};

				return View("MovieForm", viewModel);
			}

			if (movie.Id == 0)
			{
				movie.DateAdded = DateTime.Now;
				_context.Movies.Add(movie);

			}
			else
			{
				var movieInDb = _context.Movies.Single(c => c.Id == movie.Id);

				movieInDb.Name = movie.Name;
				movieInDb.ReleaseDate = movie.ReleaseDate;
				movieInDb.GenreId = movie.GenreId;
				movieInDb.NumberInStock = movie.NumberInStock;
			}
			_context.SaveChanges();
			return RedirectToAction("Index", "Movies");
		}

	}
}