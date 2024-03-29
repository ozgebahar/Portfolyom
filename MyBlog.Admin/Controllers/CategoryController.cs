﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBlog.Admin.Models;
using MyBlog.Data.Entities;
using MyBlog.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyBlog.Admin.Controllers
{
    [Authorize]
    public class CategoryController : BaseController
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [Authorize(Roles = "admin")]
        public IActionResult Add()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Add(CategoryViewModel model)
        {
           
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUserId = GetCurrentUserId();

            Category entity = new Category()
            { 
            CategoryName = model.CategoryName,
            Description = model.Description,
            CreatedById = currentUserId
            };

            #region Picture için düzenleme
            if (model.Picture.Length>0)
            {
                using (var ms = new MemoryStream())
                {
                    model.Picture.CopyTo(ms);
                    var fileByteArray = ms.ToArray();
                    entity.Picture = fileByteArray;
                }
            }
            else
            {
                ViewBag.Message = "Boş dosya olmaz.";
            }
            #endregion

            var eklendiMi = _categoryRepository.Add(entity);

            if (eklendiMi)
            {
                return RedirectToAction("List");
            }

            return View(model);
        }

        [Authorize(Roles ="admin")]
        public IActionResult List()
        {
            var categories = _categoryRepository.GetCategories().Select(x =>
            new CategoryViewModel()
            {
                Id = x.Id,
                CategoryName = x.CategoryName,
                Description = x.Description,
                PictureStr = Convert.ToBase64String(x.Picture)

            }).ToList();

            return View(categories);
        }
        public IActionResult Edit(int id)
        {
            var category = _categoryRepository.GetCategory(id);
            if (category != null)
            {
                var vm = new CategoryViewModel()
                { 
                Id = category.Id,
                CategoryName = category.CategoryName,
                Description = category.Description,

                };
                return View(vm);
            }
            return RedirectToAction("List"); ;
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Edit(CategoryViewModel model)
        {
           

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUserId = GetCurrentUserId();

            Category entity = new Category()
            {
                Id = model.Id,
                CategoryName = model.CategoryName,
                Description = model.Description,
               UpdatedById = currentUserId
            };

            #region Picture için düzenleme
            if (model.Picture.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    model.Picture.CopyTo(ms);
                    var fileByteArray = ms.ToArray();
                    entity.Picture = fileByteArray;
                }
            }
            else
            {
                ViewBag.Message = "Boş dosya olmaz.";
            }
            #endregion

            var eklendiMi = _categoryRepository.Edit(entity);

            if (eklendiMi)
            {
                return RedirectToAction("List");
            }

            return View(model);
        }

        public IActionResult Delete(int id)
        {
            var result = _categoryRepository.Delete(id);

            TempData["Message"] = result ? "Silme işlemi başarılı" : "Silme işlemi gerçekleştirilemedi";

            return RedirectToAction("List");
        }
    }
}
