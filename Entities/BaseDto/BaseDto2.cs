using AutoMapper;

using Entities.CustomMapping;
using System.ComponentModel.DataAnnotations;

//https://github.com/mjebrahimi/auto-mapping
namespace Entities.Base
{
    public abstract class BaseDto2<TDto, TEntity, TKey> : IHaveCustomMapping
         where TDto : class, new()
         where TEntity : class, IEntity<TKey>, new()
    {
        [Display(Name = "ردیف")]
        public TKey Id { get; set; }

        public TEntity ToEntity(IMapper mapper)
        {
            return mapper.Map<TEntity>(CastToDerivedClass(mapper, this));
        }

        public TEntity ToEntity(IMapper mapper, TEntity entity)
        {
            return mapper.Map(CastToDerivedClass(mapper, this), entity);
        }

        public static TDto FromEntity(IMapper mapper, TEntity model)
        {
            return mapper.Map<TDto>(model);
        }

        protected TDto CastToDerivedClass(IMapper mapper, BaseDto2<TDto, TEntity, TKey> baseInstance)
        {
            return mapper.Map<TDto>(baseInstance);
        }

        public void CreateMappings(Profile profile)
        {

            var mappingExpression = profile.CreateMap<TDto, TEntity>();
            CustomMappings(mappingExpression.ReverseMap());
            var dtoType = typeof(TDto);
            var entityType = typeof(TEntity);
            //Ignore any property of source (like Post.Author) that dose not contains in destination 
            foreach (var property in entityType.GetProperties())
            {
                if (dtoType.GetProperty(property.Name) == null)
                    mappingExpression.ForMember(property.Name, opt => opt.Ignore());
            }

            
        }

        public virtual void CustomMappings(IMappingExpression<TEntity, TDto> mapping)
        {
        }
    }

    public abstract class BaseDto2<TDto, TEntity> : BaseDto2<TDto, TEntity, int>
        where TDto : class, new()
        where TEntity : class, IEntity<int>, new()
    {

    }

  
}
