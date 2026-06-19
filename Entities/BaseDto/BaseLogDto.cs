using AutoMapper;
using Entities.CustomMapping;
using Entities.DTOs;
using System;
using System.ComponentModel.DataAnnotations;

namespace Entities.Base
{
    public abstract class BaseLogDto<TDto, TEntity, TKey> : IHaveCustomMapping
        where TDto : class, new()
        where TEntity : BaseEntity<TKey>, new()
    {
        [Display(Name = "ردیف")]
        [NotEmptyGuid]
        public TKey Id { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }

        public TEntity ToEntity(IMapper mapper)
        {

            return mapper.Map<TEntity>(CastToDerivedClass(this, mapper));
        }

        public TEntity ToEntity(IMapper mapper, TEntity entity)
        {
            return mapper.Map(CastToDerivedClass(this, mapper), entity);
        }

        public static TDto FromEntity(IMapper mapper, TEntity model)
        {
            return mapper.Map<TDto>(model);
        }

        protected TDto CastToDerivedClass(BaseLogDto<TDto, TEntity, TKey> baseInstance, IMapper mapper)
        {
            return mapper.Map<TDto>(baseInstance);
        }

        public void CreateMappings(Profile profile)
        {
            var mappingExpression = profile.CreateMap<TDto, TEntity>();

            var dtoType = typeof(TDto);
            var entityType = typeof(TEntity);
            //Ignore any property of source (like Post.Author) that dose not contains in destination 
            foreach (var property in entityType.GetProperties())
            {
                if (dtoType.GetProperty(property.Name) == null)
                {
                    mappingExpression.ForMember(property.Name, opt => opt.Ignore());
                }
            }

            CustomMappings(mappingExpression.ReverseMap());
        }

        public virtual void CustomMappings(IMappingExpression<TEntity, TDto> mapping)
        {
        }
    }
}
