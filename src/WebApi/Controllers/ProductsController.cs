﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApi.Dtos;
using WebApi.Interfaces;
using WebApi.Repositories;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("v1/[controller]")]
    public class ProductsController : Controller
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IMailService _mail;
        private readonly IProductRepository _productRepository;

        public ProductsController(ILogger<ProductsController> logger,IMailService mail,IProductRepository productRepository)
        {
            _logger = logger;
            _mail = mail;
            _productRepository = productRepository;
        }

        [HttpGet]
        public IActionResult Get()
        {
            //return Ok(ProductService.Current.Products);

            var products = _productRepository.GetProducts();
            var results = Mapper.Map<IEnumerable<Dtos.ProductCreation>>(products);
            return Ok(results);
        }

        [Route("{id}", Name = "GetProduct")]
        public IActionResult GetProduct(int id)
        {
            try
            {
                //throw new Exception("抛个异常！");
                var product = ProductService.Current.Products.SingleOrDefault(x => x.Id == id);
                if (product == null)
                {
                    this._logger.LogInformation($"Id为{id}的产品没有找到！");
                    return NotFound();
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"查找Id为{id}的产品出现了异常!",ex);
                return StatusCode(500, "处理请求的时候发生了错误！");
            }            
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProductCreation product)
        {
            if (product == null)
            {
                return BadRequest();
            }

            if (product.Name == "产品")
            {
                ModelState.AddModelError("Name", "产品的名称不可以是'产品'二字");
            }

            // 添加数据注解验证逻辑
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var maxId = ProductService.Current.Products.Max(x => x.Id);
            var newProduct = new Product
            {
                Id = ++maxId,
                Name = product.Name,
                Price = product.Price,
                //Description = product.Description
            };
            ProductService.Current.Products.Add(newProduct);

            return CreatedAtRoute("GetProduct", new { id = newProduct.Id }, newProduct);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] ProductModification product)
        {
            if (product == null)
            {
                return BadRequest();
            }

            if (product.Name == "产品")
            {
                ModelState.AddModelError("Name", "产品的名称不可以是'产品'二字");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var model = ProductService.Current.Products.SingleOrDefault(x => x.Id == id);
            if (model == null)
            {
                return NotFound();
            }

            model.Name = product.Name;
            model.Price = product.Price;
            model.Description = product.Description;

            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult Patch(int id, [FromBody] JsonPatchDocument<ProductModification> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }
            var model = ProductService.Current.Products.SingleOrDefault(x => x.Id == id);
            if (model == null)
            {
                return NotFound();
            }
            var toPatch = new ProductModification
            {
                Name = model.Name,                
                Price = model.Price,
                Description = model.Description,
            };
            patchDoc.ApplyTo(toPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (toPatch.Name == "产品")
            {
                ModelState.AddModelError("Name", "产品的名称不可以是'产品'二字");
            }
            TryValidateModel(toPatch);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            model.Name = toPatch.Name;
            model.Price = toPatch.Price;
            model.Description = toPatch.Description;            

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var model = ProductService.Current.Products.SingleOrDefault(x => x.Id == id);
            if (model == null)
            {
                return NotFound();
            }
            ProductService.Current.Products.Remove(model);
            _mail.Send("Product Deleted", $"Id为{id}的产品被删除了");
            return NoContent();
        }
    }
}
