using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;


namespace DCMS.Services.Stores
{
    /// <summary>
    /// ������ӳ��
    /// </summary>
    public partial class StoreMappingService : BaseService, IStoreMappingService
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public StoreMappingService(
            IStaticCacheManager cacheManager,
            IWorkContext workContext,
            IEventPublisher eventPublisher,
             IStoreContext storeContext,
            IServiceGetter getter) : base(getter, cacheManager, eventPublisher)
        {
            _workContext = workContext;
            _storeContext = storeContext;
        }



        #region Methods

        /// <summary>
        /// Deletes a store mapping record
        /// </summary>
        /// <param name="storeMapping">Store mapping record</param>
        public virtual void DeleteStoreMapping(StoreMapping storeMapping)
        {
            if (storeMapping == null)
            {
                throw new ArgumentNullException("storeMapping");
            }

            var uow = StoreMappingRepository.UnitOfWork;
            StoreMappingRepository.Delete(storeMapping);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(storeMapping);
        }

        /// <summary>
        /// Gets a store mapping record
        /// </summary>
        /// <param name="storeMappingId">Store mapping record identifier</param>
        /// <returns>Store mapping record</returns>
        public virtual StoreMapping GetStoreMappingById(int storeMappingId)
        {
            if (storeMappingId == 0)
            {
                return null;
            }

            return StoreMappingRepository.ToCachedGetById(storeMappingId);
        }

        /// <summary>
        /// Gets store mapping records
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Store mapping records</returns>
        public virtual IList<StoreMapping> GetStoreMappings<T>(T entity) where T : BaseEntity, IStoreMappingSupported
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            var query = from sm in StoreMappingRepository.Table
                        where sm.EntityId == entityId &&
                        sm.EntityName == entityName
                        select sm;
            var storeMappings = query.ToList();
            return storeMappings;
        }


        /// <summary>
        /// Inserts a store mapping record
        /// </summary>
        /// <param name="storeMapping">Store mapping</param>
        public virtual void InsertStoreMapping(StoreMapping storeMapping)
        {
            if (storeMapping == null)
            {
                throw new ArgumentNullException("storeMapping");
            }

            var uow = StoreMappingRepository.UnitOfWork;
            StoreMappingRepository.Insert(storeMapping);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(storeMapping);
        }

        /// <summary>
        /// Inserts a store mapping record
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="storeId">Store id</param>
        /// <param name="entity">Entity</param>
        public virtual void InsertStoreMapping<T>(T entity, int storeId) where T : BaseEntity, IStoreMappingSupported
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (storeId == 0)
            {
                throw new ArgumentOutOfRangeException("storeId");
            }

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            var storeMapping = new StoreMapping()
            {
                EntityId = entityId,
                EntityName = entityName,
                StoreId = storeId
            };

            InsertStoreMapping(storeMapping);
        }

        /// <summary>
        /// Updates the store mapping record
        /// </summary>
        /// <param name="storeMapping">Store mapping</param>
        public virtual void UpdateStoreMapping(StoreMapping storeMapping)
        {
            if (storeMapping == null)
            {
                throw new ArgumentNullException("storeMapping");
            }

            var uow = StoreMappingRepository.UnitOfWork;
            StoreMappingRepository.Update(storeMapping);
            uow.SaveChanges();

            _eventPublisher.EntityUpdated(storeMapping);
        }

        /// <summary>
        /// Find store identifiers with granted access (mapped to the entity)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>Store identifiers</returns>
        public virtual int[] GetStoresIdsWithAccess<T>(T entity) where T : BaseEntity, IStoreMappingSupported
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            var key = DCMSDefaults.STOREMAPPING_BY_ENTITYID_NAME_KEY.FillCacheKey(entityId, entityName);
            return _cacheManager.Get(key, () =>
            {
                var query = from sm in StoreMappingRepository.Table
                            where sm.EntityId == entityId &&
                            sm.EntityName == entityName
                            select sm.StoreId;
                var result = query.ToArray();
                //little hack here. nulls aren't cacheable so set it to ""
                if (result == null)
                {
                    result = new int[0];
                }

                return result;
            });
        }

        /// <summary>
        /// Authorize whether entity could be accessed in the current store (mapped to this store)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize<T>(T entity) where T : BaseEntity, IStoreMappingSupported
        {
            //return Authorize(entity, _workContext.CurrentStore);
            return Authorize(entity, _storeContext.CurrentStore);
        }

        /// <summary>
        /// Authorize whether entity could be accessed in a store (mapped to this store)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="store">Store</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize<T>(T entity, Store store) where T : BaseEntity, IStoreMappingSupported
        {
            if (entity == null)
            {
                return false;
            }

            if (store == null)
            {
                //return true if no store specified/found
                return true;
            }

            if (!entity.LimitedToStores)
            {
                return true;
            }

            foreach (var storeId in GetStoresIdsWithAccess(entity))
            {
                if (store.Id == storeId)
                {
                    //yes, we have such permission
                    return true;
                }
            }

            //no permission found
            return false;
        }

        #endregion
    }
}