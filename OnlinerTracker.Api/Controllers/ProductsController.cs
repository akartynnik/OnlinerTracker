﻿using Autofac.Integration.WebApi;
using OnlinerTracker.Api.ApiViewModels;
using OnlinerTracker.Api.Models;
using OnlinerTracker.Api.Models.Configs;
using OnlinerTracker.Api.Resources;
using OnlinerTracker.Data;
using OnlinerTracker.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace OnlinerTracker.Api.Controllers
{
    [AutofacControllerConfiguration]
    [RoutePrefix("api/Product")]
    public class ProductsController : ApiControllerBase
    {
        private ProductsControllerConfig _config;

        public ProductsController(ProductsControllerConfig config,
            IPrincipalService principalService) : base (principalService)
        {
            _config = config;;
        }

        [Route("GetAll", Name = "Get all products for current user")]
        [HttpGet]
        public async Task<IHttpActionResult> Get()
        {
            return Ok(_config.Mapper.Map<IEnumerable<Product>, IEnumerable<ExternalProduct>>(_config.ProductService.GetAll(User.Id)));
        }

        [Route("GetAllCompared", Name = "Get product by id with costs")]
        [HttpGet]
        public async Task<IHttpActionResult> GetAllCompared()
        {
            var products = _config.ProductService.GetAllCompared();
            return Ok(_config.Mapper.Map<IEnumerable<Product>, IEnumerable<ExternalProduct>>(products));
        }

        [Route("GetFromExternalServer", Name = "Get products from external server (proxy)")]
        [HttpGet]
        public async Task<IHttpActionResult> GetExternal(string searchQuery, int page)
        {
            var jsonProductsString = _config.ExternalProductService.Get(searchQuery, page);
            return Ok(_config.ExternalProductService.ConvertJsonToProducts(jsonProductsString, User.Id));
        }

        [Route("Follow", Name = "Follow product")]
        [HttpPost]
        public async Task<IHttpActionResult> Follow(ProductFollowModel model)
        {
            try
            {
                var product = _config.Mapper.Map<ProductFollowModel, Product>(model);
                product.Id = Guid.NewGuid();
                product.UserId = User.Id;
                product.Tracking = true;

                if (_config.ProductService.GetBy(product.OnlinerId, product.UserId) != null)
                {
                    _config.DialogService.SendInPopupForUser(PopupType.Warning, DialogResources.Warning_DuplicateTracking, User.DialogConnectionId);
                    return Duplicate();
                }

                var cost = _config.Mapper.Map<ProductFollowModel, Cost>(model);
                cost.Id = Guid.NewGuid();
                cost.ProductId = product.Id;
                cost.CratedAt = DateTime.Now;
                product.Costs = new List<Cost> {cost};

                _config.ProductService.Insert(product);

                _config.DialogService.SendInPopupForUser(PopupType.Success,
                    string.Format(DialogResources.Success_StartFollowProduct, product.Name), User.DialogConnectionId);
                return Successful();
            }
            catch (Exception ex)
            {
                _config.DialogService.SendInPopupForUser(PopupType.Error, DialogResources.Error_ServerError, User.DialogConnectionId);
                return InternalServerError(ex);
            }

        }

        [Route("ChangeTrackingStatus", Name = "Change tracking status")]
        [HttpPost]
        public IHttpActionResult ChangeTrackingStatus(Guid id, bool tracking)
        {
            var product = _config.ProductService.GetById(id);
            product.Tracking = tracking;
            _config.ProductService.Update(product);
            if (product.Tracking)
            {
                _config.DialogService.SendInPopupForUser(PopupType.Success,
                    string.Format(DialogResources.Success_TrackingStarted, product.Name), User.DialogConnectionId);
            }
            else
            {
                _config.DialogService.SendInPopupForUser(PopupType.Warning,
                   string.Format(DialogResources.Warning_TrackingStoped, product.Name), User.DialogConnectionId);
            }
            return Successful();
        }

        [Route("ChangeComparedStatus", Name = "Change compared status")]
        [HttpPost]
        public IHttpActionResult ChangeComparedStatus(Guid id, bool compared)
        {

            var product = _config.ProductService.GetById(id);
            product.Compared = compared;
            _config.ProductService.Update(product);
            if (product.Compared)
            {
                _config.DialogService.SendInPopupForUser(PopupType.Success,
                    string.Format(DialogResources.Success_ComparedStarted, product.Name), User.DialogConnectionId);
            }
            else
            {
                _config.DialogService.SendInPopupForUser(PopupType.Warning,
                   string.Format(DialogResources.Warning_ComparedStoped, product.Name), User.DialogConnectionId);
            }
            return Successful();
        }


        [Route("remove", Name = "Remove product")]
        [HttpPost]
        public IHttpActionResult Remove(DeletedObject obj)
        {
            _config.ProductService.Delete(obj.Id);
            _config.DialogService.SendInPopupForUser(PopupType.Warning,
                    string.Format(DialogResources.Warning_ProductDeleted, obj.Name), User.DialogConnectionId);
            return Successful();
        }
    }
}