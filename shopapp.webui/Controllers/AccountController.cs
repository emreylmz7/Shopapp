using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using shopapp.business.Abstract;
using shopapp.webui.EmailServices;
using shopapp.webui.Extensions;
using shopapp.webui.Identity;
using shopapp.webui.Models;


namespace shopapp.webui.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class AccountController:Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _signInManager;
        private IEmailSender _emailSender;
        private ICardService _cardService;
        public AccountController(UserManager<User> userManager,SignInManager<User> signInManager,IEmailSender emailSender,ICardService cardService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _cardService = cardService;
        }

        //LOGIN AND REGISTER

        [HttpGet]
        public IActionResult Login()
        {
            return View();
            // return View(new LoginModel()
            // {
            //     ReturnUrl = ReturnUrl  
            // });
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(""," Bu Kullanıcı adı ile daha önce hesap oluşturulmamış");
                return View(model);
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError("","Lütfen hesabınıza gelen link ile Hesabınızı Onaylayınız.");
                return View(model);
            }
            
            var result = await _signInManager.PasswordSignInAsync(user,model.Password,false,false);
            
            if (result.Succeeded)
            {
                return RedirectToAction("Index","Home");
            }
            ModelState.AddModelError("","Kullanıcı adı veya Parolanız Hatalı.");
            return View(model);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user,model.Password);
            if (result.Succeeded)
            {
                //generate token
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var url = Url.Action("ConfirmEmail","Account",new{
                    userId = user.Id,
                    token = code
                });
                Console.WriteLine(url);
                
                //Email
                await _emailSender.SendEmailAsync(model.Email,"Hesabınızı Onaylayınız.",$"Lütfen Hesabınızı Doğrulamak için Linke <a href='http://localhost:5019{url}'>Tıklayınız</a>");


                return RedirectToAction("Login","Account");
            }
            ModelState.AddModelError("","Bilinmeyen bir hata oluştu.");
            return View(model);
        }
        public async Task<IActionResult> Logout(LoginModel model)
        {
            await _signInManager.SignOutAsync();
            TempData.Put("message", new AlertMessage()
            {
                Title ="Oturum Kapatıldı.",
                Message = "Hesabınız Güvenli Bir şekilde Kapatıldı.",
                AlertType = "warning"
            });
            return Redirect("~/");    
        }
        public async Task<IActionResult> ConfirmEmail(string userId,string token)
        {
            if (userId==null || token==null)
            {
                TempData.Put("message", new AlertMessage()
                {
                    Title ="Hata Tespit Edildi!",
                    Message = "Geçersiz token",
                    AlertType = "danger"
                }); 
                return View();
            }
            
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user!=null)
            {
                var result = await _userManager.ConfirmEmailAsync(user,token);
                if (result.Succeeded)
                {
                    _cardService.InitializeCard(user.Id);
                    TempData.Put("message", new AlertMessage()
                    {
                        Title ="Onaylama",
                        Message = "Hesabınız Onaylandı",
                        AlertType = "success"
                    }); 
                    return View();                
                }
            }
            TempData.Put("message", new AlertMessage()
            {
                Title ="Uyaro",
                Message = "Hesabınız Onaylanamadı",
                AlertType = "warning"
            }); 

            return View();
        }  
        

        //PASSWORD
     
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                return View();
            }
            var user = await _userManager.FindByEmailAsync(Email);
            if (user==null)
            {
                return View();
            }

             //generate token
            var code = await _userManager.GeneratePasswordResetTokenAsync(user); 
            var url = Url.Action("ResetPassword","Account",new {
                userId = user.Id,
                token = code
            });
            Console.WriteLine(url);
            
            //Email
            await _emailSender.SendEmailAsync(Email,"Reset Password",$"Parolanızı Yenilemek için Linke <a href='http://localhost:5019{url}'>Tıklayınız</a>");
            
            
            return View();
        }
        [HttpGet]
        public IActionResult ResetPassword(string userId,string token)
        {
            if (userId==null || token==null)
            {
                return RedirectToAction("Index","Home");
            }
            var model = new ResetPasswordModel {Token=token};
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);                
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData.Put("message", new AlertMessage()
                {
                    Title ="Uyarı",
                    Message = "Böyle bir kullanıcı mevcut değil.",
                    AlertType = "danger"
                }); 
                return RedirectToAction("Index","Home");
            }

            var result = await _userManager.ResetPasswordAsync(user,model.Token,model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("Login","Account");
            }
            return View(model);
        }
        

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}