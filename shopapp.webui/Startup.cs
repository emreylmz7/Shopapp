using Microsoft.Extensions.FileProviders;
using shopapp.data.Abstract;
using shopapp.data.Concrete.EfCore;
using shopapp.business.Abstract;
using shopapp.business.Concrete;
using shopapp.webui.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using shopapp.webui.EmailServices;

public class Startup
{
    private IConfiguration _configuration;
    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
        
    }
    // public Startup(IConfiguration configuration)
    // {
    //     Configuration = configuration;
    // }
    // public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    { 
        services.AddDbContext<ApplicationContext>(options=>options.UseMySQL(_configuration.GetConnectionString("MySqlConnection")));
        services.AddDbContext<ShopContext>(options=>options.UseMySQL(_configuration.GetConnectionString("MySqlConnection")));
        services.AddIdentity<User,IdentityRole>().AddEntityFrameworkStores<ApplicationContext>().AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(options=> {
            //password
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric=true;

            //lockout
            options.Lockout.MaxFailedAccessAttempts =5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.AllowedForNewUsers = true;

            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        }); 
        services.ConfigureApplicationCookie(options=> {
            options.LoginPath = "/account/login";
            options.LogoutPath = "/account/logout";
            options.AccessDeniedPath = "/account/accessdenied";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(1);
            options.Cookie = new CookieBuilder
            {
                HttpOnly = true,
                Name = ".Shopapp.Security.Cookie",
                SameSite = SameSiteMode.Strict
            };
        });

        services.AddScoped<IUnitOfWork,UnitOfWork>();
        
        services.AddScoped<ICategoryService,CategoryManager>();
        services.AddScoped<IProductService,ProductManager>();
        services.AddScoped<ICardService,CardManager>();
        services.AddScoped<IOrderService,OrderManager>();



        services.AddScoped<IEmailSender,SmtpEmailSender>(i=>
            new SmtpEmailSender(
                _configuration["EmailSender:Host"],
                _configuration.GetValue<int>("EmailSender:Port"),
                _configuration.GetValue<bool>("EmailSender:EnableSSL"),
                _configuration["EmailSender:UserName"],
                _configuration["EmailSender:Password"]
            )
        );
       
        services.AddControllersWithViews();
    }

    // Burda Farklı Çözüm Kullandım ama burda dursun :  IConfiguration configuration,UserManager<User> userManager,RoleManager<IdentityRole> roleManager
     public void Configure(WebApplication app, IWebHostEnvironment env,IConfiguration configuration)
     {
        app.UseStaticFiles(); //wwwroot
        app.UseStaticFiles(new StaticFileOptions
        {
             FileProvider = new PhysicalFileProvider(
                 Path.Combine(Directory.GetCurrentDirectory(),"node_modules")),
                 RequestPath = "/modules"
        
        });

        if (env.IsDevelopment())
         {
             app.UseDeveloperExceptionPage();            
         }
         
         app.UseAuthentication();
         app.UseRouting();
         app.UseAuthorization();
        
         // if (!app.Environment.IsDevelopment())
         //  {
         
         //      app.UseExceptionHandler("/Error");
         //      // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
         //      app.UseHsts();
         //  }
      
         
         app.UseEndpoints(endpoints =>
         { 
             endpoints.MapControllerRoute(
                name: "orders", 
                pattern: "orders",
                defaults: new {controller="Card",action="GetOrders"}
            );

            endpoints.MapControllerRoute(
                name: "checkout", 
                pattern: "checkout",
                defaults: new {controller="Card",action="Checkout"}
            );
            endpoints.MapControllerRoute(
                name: "card", 
                pattern: "card",
                defaults: new {controller="Card",action="Index"}
            ); 

            //USER
            endpoints.MapControllerRoute(
                name: "adminusers", 
                pattern: "admin/user/list",
                defaults: new {controller="Admin",action="UserList"}
            ); 
            endpoints.MapControllerRoute(
                name: "adminuseredit", 
                pattern: "admin/user/{id?}",
                defaults: new {controller="Admin",action="UserEdit"}
            ); 


            //ADMIN
            endpoints.MapControllerRoute(
                name: "adminroles", 
                pattern: "admin/role/list",
                defaults: new {controller="Admin",action="RoleList"}
            );
            endpoints.MapControllerRoute(
                name: "adminrolecreate", 
                pattern: "admin/role/create",
                defaults: new {controller="Admin",action="RoleCreate"}
            );
            endpoints.MapControllerRoute(
                name: "adminroleedit", 
                pattern: "admin/role/{id?}",
                defaults: new {controller="Admin",action="RoleEdit"}
            );
            

             //PRODUCT 
            endpoints.MapControllerRoute(
                name: "adminproducts", 
                pattern: "admin/products",
                defaults: new {controller="Admin",action="Productlist"}
            );
            endpoints.MapControllerRoute(
                name: "adminproductcreate", 
                pattern: "admin/products/create",
                defaults: new {controller="Admin",action="ProductCreate"}
            );
            endpoints.MapControllerRoute(
                name: "adminproductedit", 
                pattern: "admin/products/{id?}",
                defaults: new {controller="Admin",action="ProductEdit"}
            );


             //CATEGORIES
            endpoints.MapControllerRoute(
                name: "admincategories", 
                pattern: "admin/categories",
                defaults: new {controller="Admin",action="CategoryList"}
            );
            endpoints.MapControllerRoute(
                name: "admincategorycreate", 
                pattern: "admin/categories/create",
                defaults: new {controller="Admin",action="CategoryCreate"}
            );
            endpoints.MapControllerRoute(
                name: "admincategoryedit", 
                pattern: "admin/categories/{id?}",
                defaults: new {controller="Admin",action="CategoryEdit"}
            );




            endpoints.MapControllerRoute(
                name: "search", 
                pattern: "search",
                defaults: new {controller="Shop",action="Search"}
            );
            endpoints.MapControllerRoute(
                name: "products", 
                pattern: "products",
                defaults: new {controller="Shop",action="List"}
            );
            endpoints.MapControllerRoute(
                name: "productdetails", 
                pattern: "{url}",
                defaults: new {controller="Shop",action="Details"}
            );
            endpoints.MapControllerRoute(
                name: "products", 
                pattern: "products/{category?}",
                defaults: new {controller="Shop",action="List"}
            );

            endpoints.MapControllerRoute(
                name: "default",
                pattern:"{controller=Home}/{action=Index}/{id?}"
            );
         });
        
        
        var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
        using (var scope = scopeFactory.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var cardService = scope.ServiceProvider.GetRequiredService<ICardService>();

            SeedIdentity.Seed(userManager,roleManager,cardService,configuration).Wait();
        }
         app.UseHttpsRedirection();
         app.Run();
     }
    


}


