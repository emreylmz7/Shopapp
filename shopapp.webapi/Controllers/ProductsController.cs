using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using shopapp.business.Abstract;
using shopapp.entity;
using shopapp.webapi.DTO;

namespace shopapp.webapi.Controllers
{
    //localhost:4200/api/products
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController:ControllerBase 
    {
        private IProductService _productService;
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        { 
            var products = await _productService.GetAll();
            var productDTO = new List<ProductDTO>(); 

            foreach (var p in products)
            {
                productDTO.Add(ProductToDTO(p));
            }


            return Ok(productDTO);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var p = await _productService.GetById(id);
            if (p == null)
            {
                return NotFound();  //404 kodu
            }

            return Ok(ProductToDTO(p)); //200 kodu
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id,Product entity)
        {
            if (id!=entity.ProductId)
            {
                return BadRequest();
            }
            
            var product = await _productService.GetById(id);

            if (product == null)
            {
                return NotFound();
            }

            await _productService.UpdateAsync(product,entity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _productService.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
            
            await _productService.DeleteAsync(product);
            return NoContent();
        }    
        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product entity)
        {
            await _productService.CreateAsync(entity);
            return CreatedAtAction(nameof(GetProduct), new {id=entity.ProductId},ProductToDTO(entity));
        }


        private static ProductDTO ProductToDTO(Product p)
        {
            return new ProductDTO
            {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Url = p.Url,
                    Price = p.Price,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl 
            };
        }
    }
}