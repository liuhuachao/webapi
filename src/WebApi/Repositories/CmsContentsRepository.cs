﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Interfaces;
using WebApi.Models;
using AutoMapper;

namespace WebApi.Repositories
{
    public class CmsContentsRepository : ICmsContentsRespository
    {
        private readonly PigeonsContext _context;

        public CmsContentsRepository(PigeonsContext pigeonsContext)
        {
            _context = pigeonsContext;
        }

        public void AddCmsContents(CmsContents CmsContents)
        {
            this._context.CmsContents.Add(CmsContents);
        }

        public void DeleteCmsContents(CmsContents CmsContents)
        {
            _context.CmsContents.Remove(CmsContents);
        }

        public void UpdateCmsContents(CmsContents cmsContents)
        {
            var content = this._context.CmsContents.Find(cmsContents.CmsId);            
            _context.SaveChanges();
        }

        public CmsContents GetCmsContent(int CmsId)
        {
            return _context.CmsContents.Find(CmsId);
        }

        public IQueryable<CmsContents> GetCmsContents(int limit = 10,int start = 0,int orderType = 0)
        {
            var _limit = limit > 100 ? 100 : limit;
            IQueryable<CmsContents> contents;
            if (orderType == 0)
            {
                contents = this._context.CmsContents.OrderByDescending(x => x.CmsId).Skip(start).Take(_limit);
            }
            else
            {
                contents = this._context.CmsContents.OrderBy(x => x.CmsId).Skip(start).Take(_limit);
            }
            return contents;
        }

        public bool Save()
        {
            return _context.SaveChanges() >= 0;
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public bool IsExist(int CmsId)
        {
            return _context.CmsContents.Any(x => x.CmsId == CmsId);
        }

    }
}
