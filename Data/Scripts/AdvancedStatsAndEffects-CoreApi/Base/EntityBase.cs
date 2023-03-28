using VRage.ModAPI;

namespace AdvancedStatsAndEffects
{

    public abstract class EntityBase<T> where T : IMyEntity
    {

        public T Entity;
        public IMyEntity ParentEntity;
        public bool Closed;

        public EntityBase(IMyEntity entity)
        {
            if (entity == null)
            {
                Closed = true;
                return;
            }
            Entity = (T)entity;
            ParentEntity = entity.GetTopMostParent();
            Closed = entity.Closed;
            Entity.OnClose += CloseEntity;
        }

        protected virtual void CloseEntity(IMyEntity entity)
        {
            Closed = true;
        }

    }

}