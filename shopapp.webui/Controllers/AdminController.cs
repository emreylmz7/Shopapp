using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using shopapp.business.Abstract;
using shopapp.entity;
using shopapp.webui.Extensions;
using shopapp.webui.Identity;
using shopapp.webui.Models;

namespace shopapp.webui.Controllers
{
    [Authorize(Roles="admin")]
    public class AdminController:Controller
    {
        //Injecktions
        private IProductService _productService;
        private ICategoryService _categoryService;
        private RoleManager<IdentityRole> _roleManager;
        private UserManager<User> _userManager;

        public AdminController(IProductService productService,ICategoryService categoryService,RoleManager<IdentityRole> roleManager,UserManager<User> userManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _productService = productService;
            _categoryService = categoryService;
        }

        
        
        // USER PAGES
        public IActionResult UserList()
        {
            return View(_userManager.Users);
        }
        [HttpGet]
        public async Task<IActionResult> UserEdit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user!=null)
            {
                var selectedRoles = await _userManager.GetRolesAsync(user);
                var roles = _roleManager.Roles.Select(i=>i.Name);

                ViewBag.Roles = roles;
                return View(new UserDetailsModel(){
                    UserId = user.Id,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    SelectedRoles = selectedRoles
                });
            }
            return Redirect("~/admin/user/list");
        }
        [HttpPost]
        public async Task<IActionResult> UserEdit(UserDetailsModel model,string[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user!=null)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.UserName = model.UserName;
                    user.Email = model.Email;
                    user.EmailConfirmed = user.EmailConfirmed;

                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        var userRoles = await _userManager.GetRolesAsync(user);
                        selectedRoles = selectedRoles?? new string[]{};
                        await _userManager.AddToRolesAsync(user,selectedRoles.Except(userRoles).ToArray<string>());
                        await _userManager.RemoveFromRolesAsync(user,userRoles.Except(selectedRoles).ToArray<string>());

                        return Redirect("/admin/user/list");
                    }
                } 
                return Redirect("/admin/user/list");
            }
            return View(model);


        }
        

        // ROLE PAGES
        public IActionResult RoleList()
        {
            return View(_roleManager.Roles);
        }
        [HttpGet]
        public async Task<IActionResult> RoleEdit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
        
            var members = new List<User>();
            var nonmembers = new List<User>();
            var userList = _userManager.Users.ToList();

            foreach (var user in userList)
            {
                var list = await _userManager.IsInRoleAsync(user,role.Name)?members:nonmembers;
                list.Add(user);
            }
            var model = new RoleDetails()
            {
                Role = role,
                Members = members,
                NonMembers = nonmembers
            };
            return View(model);
        }    
        [HttpPost]
        public async Task<IActionResult> RoleEdit(RoleEditModel model)
        {
            if(ModelState.IsValid)
            {
                foreach (var userId in model.IdsToAdd ?? new string[]{})
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if(user!=null)
                    {
                        var result = await _userManager.AddToRoleAsync(user,model.RoleName);
                        if(!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            { 
                                ModelState.AddModelError("", error.Description);  
                            }  
                        }
                    }
                }
          
                foreach (var userId in model.IdsToDelete ?? new string[]{})
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if(user!=null)
                    {
                        var result = await _userManager.RemoveFromRoleAsync(user,model.RoleName);
                        if(!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            { 
                                ModelState.AddModelError("", error.Description);  
                            }  
                            
                        }
                    }
                }
            }
            
            return Redirect("/admin/role/"+model.RoleId);          
        }
        [HttpGet]
        public IActionResult RoleCreate()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RoleCreate(RoleModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(model.Name));
                if (result.Succeeded)
                {
                    return RedirectToAction("RoleList");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("",error.Description);
                    }
                }
            }
            return View(model);
        }
        


        //PRODUCTS PAGES
        public async Task<IActionResult> ProductList()
        {
            var products = await _productService.GetAll();
            return View(new ProductListViewModel()
            {
                Products = products
            });
        }
        [HttpGet]
        public IActionResult ProductCreate()
        {
            return View();

        }
        [HttpPost]
        public IActionResult ProductCreate(ProductModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = new Product()
                {
                    Name = model.Name,
                    Url = model.Url,
                    ImageUrl =model.ImageUrl,
                    Price = model.Price,
                    Description = model.Description
                };

                if(_productService.Create(entity))
                {
                    TempData.Put("message", new AlertMessage()
                    {
                        Title ="Kayıt Eklendi.",
                        Message = "Kaydınız başarıyla eklendi.",
                        AlertType = "success"
                    });
                    return RedirectToAction("ProductList");
                }

                TempData.Put("message", new AlertMessage()
                {
                    Title ="Hata!",
                    Message = _productService.ErrorMessage,
                    AlertType = "danger"
                });
                return View(model);
            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> ProductEdit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }

            var entity = _productService.GetByIdWithCategories((int)id);

            if(entity==null)
            {
                 return NotFound();
            }

            var model = new ProductModel()
            {
                ProductId = entity.ProductId,
                Name = entity.Name,
                Url = entity.Url,
                Price = entity.Price,
                ImageUrl= entity.ImageUrl,
                Description = entity.Description,
                IsApproved = entity.IsApproved,
                IsHome = entity.IsHome,
                SelectedCategories = entity.ProductCategories.Select(i=>i.Category).ToList()
            };

            ViewBag.Categories = await _categoryService.GetAll();

            return View(model);
        }
         [HttpPost]
        public async Task<IActionResult> ProductEdit(ProductModel model,int[] categoryIds,IFormFile file)
        {
            if(ModelState.IsValid)
            {        
                var entity = await _productService.GetById(model.ProductId);
                if(entity==null)
                {
                    return NotFound();
                }
                entity.Name = model.Name;
                entity.Price = model.Price;
                entity.Url = model.Url;
                entity.Description = model.Description;
                entity.IsHome = model.IsHome;
                entity.IsApproved = model.IsApproved;

                if(file!=null)
                {
                    var extention = Path.GetExtension(file.FileName);
                    var randomName = string.Format($"{Guid.NewGuid()}{extention}");
                    entity.ImageUrl = randomName;
                    var path = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot\\images",randomName);

                    using(var stream = new FileStream(path,FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }

                if(_productService.Update(entity,categoryIds))
                {
                    TempData.Put("message", new AlertMessage()
                    {
                        Title ="Kayıt Güncellendi.",
                        Message = "Kaydınız başarıyla Güncellendi.",
                        AlertType = "success"
                    });                    
                    return RedirectToAction("ProductList");
                }

                TempData.Put("message", new AlertMessage()
                {
                    Title ="Hata!",
                    Message = _productService.ErrorMessage,
                    AlertType = "danger"
                });
    
            }
            ViewBag.Categories = await _categoryService.GetAll();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int ProductId)
        {
            var entity = await _productService.GetById(ProductId);
            if (entity!=null)
            {
                 _productService.Delete(entity);
            }

            TempData.Put("message", new AlertMessage()
            {
                Title ="Kayıt Silindi.",
                Message = $"{entity.Name} İsimli Ürün Silindi",
                AlertType = "danger"
            });         
            return RedirectToAction("ProductList");
        }


        //CATEGORY PAGES

        public async Task<IActionResult> CategoryList()
        {
            var categories = await _categoryService.GetAll();
            return View(new CategoryListViewModel()
            {
                Categories = categories
            });
        }
        [HttpGet]
        public IActionResult CategoryCreate()
        {
            return View();

        }
        [HttpPost]
        public IActionResult CategoryCreate(CategoryModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = new Category()
                {
                    Name = model.Name,
                    Url = model.Url,
                };

                _categoryService.Create(entity);

                TempData.Put("message", new AlertMessage()
                {
                    Title ="Kayıt Eklendi.",
                    Message = $"{entity.Name} İsimli Kategori Eklendi.",
                    AlertType = "success"
                });

                return RedirectToAction("CategoryList");                
            }

            return View(model);
            
            
        }
        [HttpGet]
        public IActionResult CategoryEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var entity = _categoryService.GetByIdWithProducts((int)id);
            
            if (entity == null)
            {
                return NotFound();
            }
            
            var model = new CategoryModel()
            {
                CategoryId = entity.CategoryId,
                Name = entity.Name,
                Url = entity.Url,
                Products = entity.ProductCategories.Select(p=>p.Product).ToList()   
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> CategoryEdit(CategoryModel model)
        {
            var entity = await _categoryService.GetById(model.CategoryId);
            if (entity==null)
            {
                return NotFound();
            }
            entity.Name = model.Name;
            entity.Url = model.Url;
    
            _categoryService.Update(entity);
            
            TempData.Put("message", new AlertMessage()
            {
                Title ="Kayıt Güncellendi.",
                Message = $"{entity.Name} İsimli Kategori Başarıyla Güncellendi",
                AlertType = "primary"
            });
            
            return RedirectToAction("CategoryList");   
        }
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int CategoryId)
        {
            var entity = await _categoryService.GetById(CategoryId);
            if (entity!=null)
            {
                 _categoryService.Delete(entity);
            }

            TempData.Put("message", new AlertMessage()
            {
                Title ="Kategori Silindi.",
                Message = $"{entity.Name} İsimli Kategori Silindi",
                AlertType = "danger"
            });

            return RedirectToAction("CategoryList");
        }
        [HttpPost]
        public IActionResult DeleteFromCategory(int categoryId,int productId)
        {
            _categoryService.DeleteFromCategory(categoryId,productId);       
            return Redirect("/admin/categories/"+categoryId);
        }
        
    }
}