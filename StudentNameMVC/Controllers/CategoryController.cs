using System.Security.Claims;
using AIVES.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudentNameMVC.Controllers;

public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;

}